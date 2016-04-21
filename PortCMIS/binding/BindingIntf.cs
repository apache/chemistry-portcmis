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

using PortCMIS.Binding.Http;
using PortCMIS.Binding.Impl;
using PortCMIS.Binding.Services;
using PortCMIS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PortCMIS.Binding
{
    public interface ICmisBinding : IDisposable
    {
        string BindingType { get; }
        IRepositoryService GetRepositoryService();
        INavigationService GetNavigationService();
        IObjectService GetObjectService();
        IVersioningService GetVersioningService();
        IRelationshipService GetRelationshipService();
        IDiscoveryService GetDiscoveryService();
        IMultiFilingService GetMultiFilingService();
        IAclService GetAclService();
        IPolicyService GetPolicyService();
        IAuthenticationProvider GetAuthenticationProvider();
        void ClearAllCaches();
        void ClearRepositoryCache(string repositoryId);
    }

    public interface IBindingSession
    {
        object GetValue(string key);
        object GetValue(string key, object defValue);
        int GetValue(string key, int defValue);
        bool GetValue(string key, bool defValue);

        void PutValue(string key, object value);
        void RemoveValue(string key);

        IAuthenticationProvider GetAuthenticationProvider();
        IHttpInvoker GetHttpInvoker();
    }

    /// <summary>
    /// SPI interface.
    /// </summary>
    public interface ICmisSpi : IDisposable
    {
        void Initialize(IBindingSession session);
        IRepositoryService GetRepositoryService();
        INavigationService GetNavigationService();
        IObjectService GetObjectService();
        IVersioningService GetVersioningService();
        IRelationshipService GetRelationshipService();
        IDiscoveryService GetDiscoveryService();
        IMultiFilingService GetMultiFilingService();
        IAclService GetAclService();
        IPolicyService GetPolicyService();
        void ClearAllCaches();
        void ClearRepositoryCache(string repositoryId);
    }

    /// <summary>
    /// Basic authentication provider 
    /// </summary>
    public interface IAuthenticationProvider
    {
        IBindingSession Session { get; set; }
    }

    public interface IPortableAuthenticationProvider : IAuthenticationProvider
    {
        void PrepareHttpClientHandler(HttpClientHandler httpClientHandler);
        void PrepareHttpRequestMessage(HttpRequestMessage httpRequestMessage);
        void HandleResponse(HttpResponseMessage httpResponseMessage);
    }

    public abstract class AbstractAuthenticationProvider : IPortableAuthenticationProvider
    {
        public IBindingSession Session { get; set; }
        public CookieContainer CookieContainer { get; private set; }
        public string User { get { return Session.GetValue(SessionParameter.User) as string; } }
        public string Password { get { return Session.GetValue(SessionParameter.Password) as string; } }
        public string ProxyUser { get { return Session.GetValue(SessionParameter.ProxyUser) as string; } }
        public string ProxyPassword { get { return Session.GetValue(SessionParameter.ProxyPassword) as string; } }

        public virtual void PrepareHttpClientHandler(HttpClientHandler httpClientHandler)
        {
            httpClientHandler.PreAuthenticate = true;
            httpClientHandler.UseCookies = true;
            httpClientHandler.UseProxy = true;
            CookieContainer = new CookieContainer();
            httpClientHandler.CookieContainer = CookieContainer;
        }

        public virtual void PrepareHttpRequestMessage(HttpRequestMessage httpRequestMessage)
        {
        }

        public virtual void HandleResponse(HttpResponseMessage httpResponseMessage)
        {
        }
    }

    public class StandardAuthenticationProvider : AbstractAuthenticationProvider
    {
        public string BearerToken { get { return Session.GetValue(SessionParameter.OAuthBearerToken) as string; } }
        public string CsrfHeader { get { return Session.GetValue(SessionParameter.CsrfHeader) as string; } }

        protected AuthenticationHeaderValue AuthenticationHeader { get; set; }
        protected AuthenticationHeaderValue ProxyAuthenticationHeader { get; set; }

        private object tokenLock = new object();
        private string token = "fetch";
        protected string CsrfHeaderName { get; set; }
        protected string CsrfToken
        {
            get { lock (tokenLock) { return token; } }
            set { lock (tokenLock) { token = value; } }
        }

        public override void PrepareHttpClientHandler(HttpClientHandler httpClientHandler)
        {
            base.PrepareHttpClientHandler(httpClientHandler);

            if (User != null)
            {
                httpClientHandler.Credentials = new NetworkCredential(User, Password);
            }
            else
            {
                if (BearerToken != null)
                {
                    httpClientHandler.PreAuthenticate = true;
                    httpClientHandler.UseDefaultCredentials = false;
                    AuthenticationHeader = new AuthenticationHeaderValue("Bearer", BearerToken);
                }
                else
                {
                    httpClientHandler.UseDefaultCredentials = true;
                }
            }

            if (ProxyUser != null)
            {
                var userPassword = Encoding.UTF8.GetBytes(ProxyUser + ":" + ProxyPassword);
                ProxyAuthenticationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(userPassword));
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
                IEnumerable<string> values;
                if (httpResponseMessage.Headers.TryGetValues(CsrfHeaderName, out values))
                {
                    CsrfToken = values.First();
                }
            }
        }
    }

    public class CmisBindingFactory
    {
        // Default CMIS AtomPub binding SPI implementation
        public const string BindingSpiAtomPub = "PortCMIS.Binding.AtomPub.CmisAtomPubSpi";
        // Default CMIS Browser binding SPI implementation
        public const string BindingSpiBrowser = "PortCMIS.Binding.Browser.CmisBrowserSpi";

        public const string StandardAuthenticationProviderClass = "PortCMIS.Binding.StandardAuthenticationProvider";
        public const string DefaultHttpInvokerClass = "PortCMIS.Binding.Http.DefaultHttpInvoker";

        private IDictionary<string, string> defaults;

        private CmisBindingFactory()
        {
            defaults = CreateNewDefaultParameters();
        }

        public static CmisBindingFactory NewInstance()
        {
            return new CmisBindingFactory();
        }

        public IDictionary<string, string> GetDefaultSessionParameters()
        {
            return defaults;
        }

        public void SetDefaultSessionParameters(IDictionary<string, string> sessionParameters)
        {
            if (sessionParameters == null)
            {
                defaults = CreateNewDefaultParameters();
            }
            else
            {
                defaults = sessionParameters;
            }
        }

        public ICmisBinding CreateCmisBinding(IDictionary<string, string> sessionParameters, IAuthenticationProvider authenticationProvider)
        {
            CheckSessionParameters(sessionParameters, true);
            AddDefaultParameters(sessionParameters);

            return new CmisBinding(sessionParameters, authenticationProvider);
        }

        public ICmisBinding CreateCmisBrowserBinding(IDictionary<string, string> sessionParameters, IAuthenticationProvider authenticationProvider)
        {
            CheckSessionParameters(sessionParameters, false);
            sessionParameters[SessionParameter.BindingSpiClass] = BindingSpiBrowser;
            if (authenticationProvider == null)
            {
                if (!sessionParameters.ContainsKey(SessionParameter.AuthenticationProviderClass))
                {
                    sessionParameters[SessionParameter.AuthenticationProviderClass] = StandardAuthenticationProviderClass;
                }
            }
            if (!sessionParameters.ContainsKey(SessionParameter.HttpInvokerClass))
            {
                sessionParameters[SessionParameter.HttpInvokerClass] = DefaultHttpInvokerClass;
            }

            AddDefaultParameters(sessionParameters);
            if (!sessionParameters.ContainsKey(SessionParameter.BrowserSuccinct))
            {
                sessionParameters.Add(SessionParameter.BrowserSuccinct, "true");
            }

            Check(sessionParameters, SessionParameter.BrowserUrl);

            return new CmisBinding(sessionParameters, authenticationProvider);
        }

        public ICmisBinding CreateCmisAtomPubBinding(IDictionary<string, string> sessionParameters, IAuthenticationProvider authenticationProvider)
        {
            CheckSessionParameters(sessionParameters, false);
            sessionParameters[SessionParameter.BindingSpiClass] = BindingSpiAtomPub;
            if (authenticationProvider == null)
            {
                if (!sessionParameters.ContainsKey(SessionParameter.AuthenticationProviderClass))
                {
                    sessionParameters[SessionParameter.AuthenticationProviderClass] = StandardAuthenticationProviderClass;
                }
            }
            if (!sessionParameters.ContainsKey(SessionParameter.HttpInvokerClass))
            {
                sessionParameters[SessionParameter.HttpInvokerClass] = DefaultHttpInvokerClass;
            }

            AddDefaultParameters(sessionParameters);

            Check(sessionParameters, SessionParameter.AtomPubUrl);

            return new CmisBinding(sessionParameters, authenticationProvider);
        }

        public ICmisBinding CreateCmisWebServicesBinding(IDictionary<string, string> sessionParameters, IAuthenticationProvider authenticationProvider)
        {
            throw new ArgumentException("The Web Services binding is not supported!");
        }

        // ---- internals ----

        private void CheckSessionParameters(IDictionary<string, string> sessionParameters, bool mustContainSpi)
        {
            // don't accept null
            if (sessionParameters == null)
            {
                throw new ArgumentNullException("sessionParameters");
            }

            // check binding entry
            if (mustContainSpi)
            {
                string spiClass;

                if (sessionParameters.TryGetValue(SessionParameter.BindingSpiClass, out spiClass))
                {
                    throw new ArgumentException("SPI class entry (" + SessionParameter.BindingSpiClass + ") is missing!");
                }

                if ((spiClass == null) || (spiClass.Trim().Length == 0))
                {
                    throw new ArgumentException("SPI class entry (" + SessionParameter.BindingSpiClass + ") is invalid!");
                }
            }
        }

        private void Check(IDictionary<string, string> sessionParameters, String parameter)
        {
            if (!sessionParameters.ContainsKey(parameter))
            {
                throw new ArgumentException("Parameter '" + parameter + "' is missing!");
            }
        }

        private void AddDefaultParameters(IDictionary<string, string> sessionParameters)
        {
            foreach (string key in defaults.Keys)
            {
                if (!sessionParameters.ContainsKey(key))
                {
                    sessionParameters[key] = defaults[key];
                }
            }
        }

        private IDictionary<string, string> CreateNewDefaultParameters()
        {
            IDictionary<string, string> result = new Dictionary<string, string>();

            result[SessionParameter.CacheSizeRepositories] = SessionParameterDefaults.CacheSizeRepositories.ToString();
            result[SessionParameter.CacheSizeTypes] = SessionParameterDefaults.CacheSizeTypes.ToString();
            result[SessionParameter.CacheSizeLinks] = SessionParameterDefaults.CacheSizeLinks.ToString();

            return result;
        }
    }
}
