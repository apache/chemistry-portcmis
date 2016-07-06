/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements. See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership. The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* Kind, either express or implied. See the License for the
* specific language governing permissions and limitations
* under the License.
*/

using PortCMIS.Client;
using PortCMIS.Exceptions;
using PortCMIS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PortCMIS.Binding.Http
{
    /// <summary>
    /// Default HTTP invoker implementation for portable environments. 
    /// </summary>
    public class DefaultHttpInvoker : IHttpInvoker
    {
        private const string InvokerHttpClient = "org.apache.chemistry.portcmis.invoker.httpclient";
        private object invokerLock = new object();

        /// <inheritdoc/>
        public IResponse InvokeGET(UrlBuilder url, IBindingSession session)
        {
            return Invoke(url, HttpMethod.Get, null, session, null, null, null);
        }

        /// <inheritdoc/>
        public IResponse InvokeGET(UrlBuilder url, IBindingSession session, long? offset, long? length)
        {
            return Invoke(url, HttpMethod.Get, null, session, offset, length, null);
        }

        /// <inheritdoc/>
        public IResponse InvokePOST(UrlBuilder url, HttpContent content, IBindingSession session)
        {
            return Invoke(url, HttpMethod.Post, content, session, null, null, null);
        }

        /// <inheritdoc/>
        public IResponse InvokePUT(UrlBuilder url, IDictionary<string, string> headers, HttpContent content, IBindingSession session)
        {
            return Invoke(url, HttpMethod.Put, content, session, null, null, headers);
        }

        /// <inheritdoc/>
        public IResponse InvokeDELETE(UrlBuilder url, IBindingSession session)
        {
            return Invoke(url, HttpMethod.Delete, null, session, null, null, null);
        }

        private IResponse Invoke(UrlBuilder url, HttpMethod method, HttpContent content, IBindingSession session,
                long? offset, long? length, IDictionary<string, string> headers)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("HTTP: " + method.ToString() + " " + url.ToString());
            }

            IPortableAuthenticationProvider authProvider = session.GetAuthenticationProvider() as IPortableAuthenticationProvider;

            HttpClient httpClient = session.GetValue(InvokerHttpClient) as HttpClient;

            if (httpClient == null)
            {
                lock (invokerLock)
                {
                    httpClient = session.GetValue(InvokerHttpClient) as HttpClient;
                    if (httpClient == null)
                    {

                        HttpClientHandler httpClientHandler = new HttpClientHandler();

                        // redirects
                        if (httpClientHandler.SupportsRedirectConfiguration)
                        {
                            httpClientHandler.AllowAutoRedirect = false;
                        }

                        // compression
                        if (httpClientHandler.SupportsAutomaticDecompression)
                        {
                            string compressionFlag = session.GetValue(SessionParameter.Compression) as string;
                            if (compressionFlag != null && compressionFlag.ToLowerInvariant().Equals("true"))
                            {
                                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                            }
                        }

                        // authentication
                        httpClientHandler.PreAuthenticate = true;
                        httpClientHandler.UseDefaultCredentials = false;

                        // authentication provider
                        if (authProvider != null)
                        {
                            authProvider.PrepareHttpClientHandler(httpClientHandler);
                        }

                        // create HttpClient
                        httpClient = new HttpClient(httpClientHandler, true);

                        // timeouts
                        int connectTimeout = session.GetValue(SessionParameter.ConnectTimeout, -2);
                        if (connectTimeout >= -1)
                        {
                            httpClient.Timeout = TimeSpan.FromMilliseconds(connectTimeout);
                        }

                        session.PutValue(InvokerHttpClient, httpClient);
                    }
                }
            }

            HttpRequestMessage request = new HttpRequestMessage(method, url.ToString());

            // set additional headers

            string userAgent = session.GetValue(SessionParameter.UserAgent) as string;
            request.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse(userAgent ?? ClientVersion.UserAgent));

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // range
            if (offset != null && length != null)
            {
                long longOffset = offset.Value < 0 ? 0 : offset.Value;
                if (length.Value > 0)
                {
                    request.Headers.Range = new RangeHeaderValue(longOffset, longOffset + length.Value - 1);
                }
                else
                {
                    request.Headers.Range = new RangeHeaderValue(longOffset, null);
                }
            }
            else if (offset != null && offset.Value > 0)
            {
                request.Headers.Range = new RangeHeaderValue(offset, null);
            }

            // content
            if (content != null)
            {
                request.Headers.TransferEncodingChunked = true;
                request.Content = content;
            }

            // authentication provider
            if (authProvider != null)
            {
                authProvider.PrepareHttpRequestMessage(request);
            }

            Response response;
            try
            {
                Task<HttpResponseMessage> task = Send(httpClient, request);
                if (task.IsFaulted)
                {
                    throw task.Exception;
                }
                else
                {
                    HttpResponseMessage httpResponseMessage = task.Result;

                    if (authProvider != null)
                    {
                        authProvider.HandleResponse(httpResponseMessage);
                    }
                    response = new Response(httpResponseMessage);
                }
            }
            catch (Exception e)
            {
                throw new CmisConnectionException("Cannot access " + url + ": " + e.Message, e);
            }


            return response;
        }

        private async Task<HttpResponseMessage> Send(HttpClient httpClient, HttpRequestMessage request)
        {
            return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        }
    }

    class Response : IResponse
    {
        private HttpResponseMessage response;

        public int StatusCode { get; private set; }
        public string Message { get; private set; }
        public Stream Stream { get; private set; }
        public string ErrorContent { get; private set; }
        public string ContentType { get; private set; }
        public string Charset { get; private set; }
        public long? ContentLength { get; private set; }
        public string Filename { get; private set; }

        public Response(HttpResponseMessage httpResponse)
        {
            this.response = httpResponse;
            StatusCode = (int)httpResponse.StatusCode;
            Message = httpResponse.ReasonPhrase;

            bool isBase64 = false;

            if (httpResponse.Content != null)
            {
                if (httpResponse.Content.Headers.ContentType != null)
                {
                    ContentType = httpResponse.Content.Headers.ContentType.MediaType;
                    Charset = httpResponse.Content.Headers.ContentType.CharSet;
                }
                ContentLength = httpResponse.Content.Headers.ContentLength;

                if (httpResponse.Content.Headers.ContentDisposition != null)
                {
                    Filename = httpResponse.Content.Headers.ContentDisposition.FileName;
                }

                if (httpResponse.Content.Headers.Contains("Content-Transfer-Encoding"))
                {
                    IEnumerable<String> contentTransferEncoding = httpResponse.Content.Headers.GetValues("Content-Transfer-Encoding");
                    isBase64 = contentTransferEncoding.Contains("base64", StringComparer.OrdinalIgnoreCase);
                }
            }

            if (httpResponse.StatusCode == HttpStatusCode.OK ||
                httpResponse.StatusCode == HttpStatusCode.Created ||
                httpResponse.StatusCode == HttpStatusCode.NonAuthoritativeInformation ||
                httpResponse.StatusCode == HttpStatusCode.PartialContent)
            {
                Stream = GetContentStream().Result;

                if (isBase64)
                {
                    // TODO: this is only required for the AtomPub binding of SharePoint 2010
                    // Stream = new CryptoStream(Stream, new FromBase64Transform(), CryptoStreamMode.Read);
                }
            }
            else
            {
                if (httpResponse.StatusCode != HttpStatusCode.NoContent)
                {
                    if (ContentType != null &&
                        (ContentType.ToLowerInvariant().StartsWith("text/") ||
                        ContentType.ToLowerInvariant().EndsWith("+xml") ||
                        ContentType.ToLowerInvariant().StartsWith("application/xml") ||
                        ContentType.ToLowerInvariant().StartsWith("application/json")))
                    {
                        ErrorContent = GetContentString().Result;
                    }
                }

                try
                {
                    response.Dispose();
                    response = null;
                }
                catch (Exception) { }
            }
        }

        public void CloseStream()
        {
            if (Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }

            if (response != null)
            {
                try
                {
                    response.Dispose();
                    response = null;
                }
                catch (Exception) { }
            }
        }

        async private Task<Stream> GetContentStream()
        {
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        async private Task<String> GetContentString()
        {
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
