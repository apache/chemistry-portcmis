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
using System;
using System.Text;
using Windows.Security.Credentials;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace PortCMIS.Binding
{
    /// <summary>
    /// Authentication provider interface for the Windows HTTP client.
    /// </summary>
    public interface IWindowsAuthenticationProvider : IAuthenticationProvider
    {
        void PrepareHttpClientFilter(HttpBaseProtocolFilter httpClientFilter);
        void PrepareHttpRequestMessage(HttpRequestMessage httpRequestMessage);
        void HandleResponse(HttpResponseMessage httpResponseMessage);
    }

    /// <summary>
    /// Base implementation of a Windows authentication provider.
    /// </summary>
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

    /// <summary>
    /// Standard Authentication Provider for Windows.
    /// </summary>
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
                string preemptiveFlag = Session.GetValue(SessionParameter.PreemptivAuthentication) as string;
                if (preemptiveFlag != null && preemptiveFlag.ToLowerInvariant().Equals("true"))
                {
                    var userPassword = Encoding.UTF8.GetBytes(User + ":" + Password);
                    AuthenticationHeader = new HttpCredentialsHeaderValue("Basic", Convert.ToBase64String(userPassword));
                }
                else
                {
                    httpClientFilter.ServerCredential = new PasswordCredential("cmis", User, Password);

                }
            }
            else if (BearerToken != null)
            {
                AuthenticationHeader = new HttpCredentialsHeaderValue("Bearer", BearerToken);
            }

            if (ProxyUser != null)
            {
                var userPassword = Encoding.UTF8.GetBytes(ProxyUser + ":" + ProxyPassword);
                ProxyAuthenticationHeader = new HttpCredentialsHeaderValue("Basic", Convert.ToBase64String(userPassword));
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
