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
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace PortCMIS.Binding
{
    public interface IWindowsAuthenticationProvider : IAuthenticationProvider
    {
        void PrepareHttpClientFilter(HttpBaseProtocolFilter httpClientFilter);
        void PrepareHttpRequestMessage(HttpRequestMessage httpRequestMessage);
        void HandleResponse(HttpResponseMessage httpResponseMessage);
    }

    public abstract class AbstractWindowsAuthenticationProvider : IWindowsAuthenticationProvider
    {
        public IBindingSession Session { get; set; }
        public HttpCookieManager CookieManager { get; private set; }
        public string User { get { return Session.GetValue(SessionParameter.User) as string; } }
        public string Password { get { return Session.GetValue(SessionParameter.Password) as string; } }
        public string ProxyUser { get { return Session.GetValue(SessionParameter.ProxyUser) as string; } }
        public string ProxyPassword { get { return Session.GetValue(SessionParameter.ProxyPassword) as string; } }

        public virtual void PrepareHttpClientFilter(HttpBaseProtocolFilter httpClientFilter)
        {
            httpClientFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            httpClientFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

            httpClientFilter.MaxConnectionsPerServer = 20;

            httpClientFilter.UseProxy = true;

            CookieManager = httpClientFilter.CookieManager;
        }

        public virtual void PrepareHttpRequestMessage(HttpRequestMessage httpRequestMessage)
        {
        }

        public virtual void HandleResponse(HttpResponseMessage httpResponseMessage)
        {
        }
    }

    public class StandardWindowsAuthenticationProvider : AbstractWindowsAuthenticationProvider
    {
        public string BearerToken { get { return Session.GetValue(SessionParameter.OAuthBearerToken) as string; } }
        public string CsrfHeader { get { return Session.GetValue(SessionParameter.CsrfHeader) as string; } }

        protected HttpCredentialsHeaderValue AuthenticationHeader { get; set; }
        protected HttpCredentialsHeaderValue ProxyAuthenticationHeader { get; set; }

        private object tokenLock = new object();
        private string token = "fetch";
        protected string CsrfHeaderName { get; set; }
        protected string CsrfToken
        {
            get { lock (tokenLock) { return token; } }
            set { lock (tokenLock) { token = value; } }
        }

        public override void PrepareHttpClientFilter(HttpBaseProtocolFilter httpClientFilter)
        {
            base.PrepareHttpClientFilter(httpClientFilter);

            if (User != null)
            {
                httpClientFilter.ServerCredential = new PasswordCredential("cmis", User, Password);
            }
            else if (BearerToken != null)
            {
                AuthenticationHeader = new HttpCredentialsHeaderValue("Bearer", BearerToken);
            }

            if (ProxyUser != null)
            {
                var userPassword = CryptographicBuffer.ConvertStringToBinary(ProxyUser + ":" + ProxyPassword, BinaryStringEncoding.Utf16LE);
                ProxyAuthenticationHeader = new HttpCredentialsHeaderValue("Basic", CryptographicBuffer.EncodeToBase64String(userPassword));
            }

            if (CsrfHeader != null)
            {
                CsrfHeaderName = CsrfHeader;
            }
        }

        public override void PrepareHttpRequestMessage(HttpRequestMessage httpRequestMessage)
        {
            base.PrepareHttpRequestMessage(httpRequestMessage);

            if (AuthenticationHeader != null)
            {
                httpRequestMessage.Headers.Authorization = AuthenticationHeader;
            }

            if (ProxyAuthenticationHeader != null)
            {
                httpRequestMessage.Headers.ProxyAuthorization = ProxyAuthenticationHeader;
            }

            if (CsrfHeaderName != null && CsrfToken != null)
            {
                httpRequestMessage.Headers.Add(CsrfHeaderName, CsrfToken);
            }
        }

        public override void HandleResponse(HttpResponseMessage httpResponseMessage)
        {
            base.HandleResponse(httpResponseMessage);

            if (CsrfHeaderName != null)
            {
                string value;
                if (httpResponseMessage.Headers.TryGetValue(CsrfHeaderName, out value))
                {
                    CsrfToken = value;
                }
            }
        }
    }
}
