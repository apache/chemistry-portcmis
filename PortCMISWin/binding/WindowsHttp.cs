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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;


namespace PortCMIS.Binding.Http
{
    public class WindowsHttpInvoker : IHttpInvoker
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
        public IResponse InvokePOST(UrlBuilder url, System.Net.Http.HttpContent content, IBindingSession session)
        {
            return Invoke(url, HttpMethod.Post, content, session, null, null, null);
        }

        /// <inheritdoc/>
        public IResponse InvokePUT(UrlBuilder url, IDictionary<string, string> headers, System.Net.Http.HttpContent content, IBindingSession session)
        {
            return Invoke(url, HttpMethod.Put, content, session, null, null, headers);
        }

        /// <inheritdoc/>
        public IResponse InvokeDELETE(UrlBuilder url, IBindingSession session)
        {
            return Invoke(url, HttpMethod.Delete, null, session, null, null, null);
        }

        private IResponse Invoke(UrlBuilder url, HttpMethod method, System.Net.Http.HttpContent content, IBindingSession session,
                long? offset, long? length, IDictionary<string, string> headers)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("HTTP: " + method.ToString() + " " + url.ToString());
            }

            IWindowsAuthenticationProvider authProvider = session.GetAuthenticationProvider() as IWindowsAuthenticationProvider;

            HttpClient httpClient = session.GetValue(InvokerHttpClient) as HttpClient;

            if (httpClient == null)
            {
                lock (invokerLock)
                {
                    httpClient = session.GetValue(InvokerHttpClient) as HttpClient;
                    if (httpClient == null)
                    {
                        HttpBaseProtocolFilter httpClientFilter = new HttpBaseProtocolFilter();

                        // redirects
                        httpClientFilter.AllowAutoRedirect = false;

                        // compression
                        string compressionFlag = session.GetValue(SessionParameter.Compression) as string;
                        if (compressionFlag != null && compressionFlag.ToLowerInvariant().Equals("true"))
                        {
                            httpClientFilter.AutomaticDecompression = true;
                        }

                        // authentication
                        httpClientFilter.AllowUI = false;

                        // authentication provider
                        if (authProvider != null)
                        {
                            authProvider.PrepareHttpClientFilter(httpClientFilter);
                        }

                        // create HttpClient
                        httpClient = new HttpClient(httpClientFilter);

                        session.PutValue(InvokerHttpClient, httpClient);
                    }
                }
            }

            HttpRequestMessage request = new HttpRequestMessage(method, new Uri(url.ToString()));

            // set additional headers
            request.Headers.UserAgent.Add(new HttpProductInfoHeaderValue("ApacheChemistryPortCMIS", "0.1"));
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // range
            if (offset != null && length != null)
            {
                long longOffset = offset.Value < 0 ? 0 : offset.Value;
                if (length.Value > 0)
                {
                    request.Headers.Add(new KeyValuePair<string, string>("Range", "bytes=" + longOffset + "-" + (longOffset + length.Value - 1)));
                }
                else
                {
                    request.Headers.Add(new KeyValuePair<string, string>("Range", "bytes=" + longOffset + "-"));
                }
            }
            else if (offset != null && offset.Value > 0)
            {
                request.Headers.Add(new KeyValuePair<string, string>("Range", "bytes=" + offset.Value + "-"));
            }

            // content
            if (content != null)
            {
                request.Content = new ConvertedHttpContent(content);

                if (request.Content.Headers.ContentLength == null)
                {
                    request.Headers.TransferEncoding.TryParseAdd("chunked");
                }
            }

            // authentication provider
            if (authProvider != null)
            {
                authProvider.PrepareHttpRequestMessage(request);
            }

            // timeouts
            int timeout = session.GetValue(SessionParameter.ConnectTimeout, -2);

            WindowsResponse response;
            try
            {
                Task<HttpResponseMessage> task = Send(httpClient, request, timeout);
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
                    response = new WindowsResponse(httpResponseMessage);
                }
            }
            catch (Exception e)
            {
                throw new CmisConnectionException("Cannot access " + url + ": " + e.Message, e);
            }

            return response;
        }

        private async Task<HttpResponseMessage> Send(HttpClient httpClient, HttpRequestMessage request, int timeout)
        {
            if (timeout > 0)
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
                return await httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead).AsTask(cts.Token).ConfigureAwait(false);
            }
            else
            {
                return await httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead).AsTask().ConfigureAwait(false);
            }
        }
    }

    class ConvertedHttpContent : IHttpContent
    {
        private System.Net.Http.HttpContent content;

        public ConvertedHttpContent(System.Net.Http.HttpContent httpContent)
        {
            content = httpContent;
            Headers = new HttpContentHeaderCollection();

            foreach (KeyValuePair<string, IEnumerable<string>> header in httpContent.Headers)
            {
                Headers.Add(header.Key, header.Value.First());
            }
        }

        public HttpContentHeaderCollection Headers { get; set; }

        public IAsyncOperationWithProgress<ulong, ulong> BufferAllAsync()
        {
            return AsyncInfo.Run<ulong, ulong>(async (token, progress) =>
            {
                await content.LoadIntoBufferAsync();
                return 0;
            });
        }

        public IAsyncOperationWithProgress<IBuffer, ulong> ReadAsBufferAsync()
        {
            return AsyncInfo.Run<IBuffer, ulong>(async (token, progress) =>
            {
                return (await content.ReadAsByteArrayAsync()).AsBuffer();
            });
        }

        public IAsyncOperationWithProgress<IInputStream, ulong> ReadAsInputStreamAsync()
        {
            return AsyncInfo.Run<IInputStream, ulong>(async (token, progress) =>
            {
                return (await content.ReadAsStreamAsync()).AsInputStream();
            });
        }

        public IAsyncOperationWithProgress<string, ulong> ReadAsStringAsync()
        {
            return AsyncInfo.Run<string, ulong>((token, progress) =>
            {
                return content.ReadAsStringAsync();
            });
        }

        public bool TryComputeLength(out ulong length)
        {
            length = 0;
            return false;
        }

        public IAsyncOperationWithProgress<ulong, ulong> WriteToStreamAsync(IOutputStream outputStream)
        {
            return AsyncInfo.Run<ulong, ulong>(async (token, progress) =>
            {
                Stream stream = outputStream.AsStreamForWrite();
                await content.CopyToAsync(stream);
                stream.Flush();

                return 0;
            });
        }

        public void Dispose()
        {
        }
    }


    class WindowsResponse : IResponse
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

        public WindowsResponse(HttpResponseMessage httpResponse)
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
                ContentLength = (long?)httpResponse.Content.Headers.ContentLength;

                if (httpResponse.Content.Headers.ContentDisposition != null)
                {
                    Filename = httpResponse.Content.Headers.ContentDisposition.FileName;
                }

                string contentTransferEncoding;
                if (httpResponse.Content.Headers.TryGetValue("Content-Transfer-Encoding", out contentTransferEncoding))
                {
                    isBase64 = contentTransferEncoding == "base64";
                }
            }

            if (httpResponse.StatusCode == HttpStatusCode.Ok ||
                httpResponse.StatusCode == HttpStatusCode.Created ||
                httpResponse.StatusCode == HttpStatusCode.NonAuthoritativeInformation ||
                httpResponse.StatusCode == HttpStatusCode.PartialContent)
            {
                Stream = GetContentStream().Result.AsStreamForRead();

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

        async private Task<IInputStream> GetContentStream()
        {
            return await response.Content.ReadAsInputStreamAsync().AsTask().ConfigureAwait(false);
        }

        async private Task<String> GetContentString()
        {
            return await response.Content.ReadAsStringAsync().AsTask().ConfigureAwait(false);
        }
    }
}
