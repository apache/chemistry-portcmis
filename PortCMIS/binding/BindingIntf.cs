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
    /// <summary>
    /// Low-level CMIS binding interface.
    /// </summary>
    public interface ICmisBinding : IDisposable
    {
        /// <value>
        /// Binding type.
        /// </value>
        string BindingType { get; }

        /// <summary>
        /// Returns the repository service instance.
        /// </summary>
        /// <returns>the repository service instance</returns>
        IRepositoryService GetRepositoryService();

        /// <summary>
        /// Returns the navigation service instance.
        /// </summary>
        /// <returns>the navigation service instance</returns>
        INavigationService GetNavigationService();

        /// <summary>
        /// Returns the object service instance.
        /// </summary>
        /// <returns>the object service instance</returns>
        IObjectService GetObjectService();

        /// <summary>
        /// Returns the versioning service instance.
        /// </summary>
        /// <returns>the versioning service instance</returns>
        IVersioningService GetVersioningService();

        /// <summary>
        /// Returns the relationship service instance.
        /// </summary>
        /// <returns>the relationship service instance</returns>
        IRelationshipService GetRelationshipService();

        /// <summary>
        /// Returns the discovery service instance.
        /// </summary>
        /// <returns>the discovery service instance</returns>
        IDiscoveryService GetDiscoveryService();

        /// <summary>
        /// Returns the multi-filing service instance.
        /// </summary>
        /// <returns>the multi-filing service instance</returns>
        IMultiFilingService GetMultiFilingService();

        /// <summary>
        /// Returns the ACL service instance.
        /// </summary>
        /// <returns>the ACL service instance</returns>
        IAclService GetAclService();

        /// <summary>
        /// Returns the policy service instance.
        /// </summary>
        /// <returns>the policy service instance</returns>
        IPolicyService GetPolicyService();

        /// <summary>
        /// Returns the authentication provider instance.
        /// </summary>
        /// <returns>the authentication provider instance</returns>
        IAuthenticationProvider GetAuthenticationProvider();

        /// <summary>
        /// Clears all low-level caches.
        /// </summary>
        void ClearAllCaches();

        /// <summary>
        /// Clears all low-level caches for the given repository.
        /// </summary>
        /// <param name="repositoryId">the repository ID</param>
        void ClearRepositoryCache(string repositoryId);
    }

    /// <summary>
    /// Binding Session interface.
    /// </summary>
    public interface IBindingSession
    {
        /// <summary>
        /// Gets a value from the session.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the value or <c>null</c> if the key is unknown</returns>
        object GetValue(string key);

        /// <summary>
        /// Gets a value from the session.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="defValue">the default value</param>
        /// <returns>the value or the default value if the key is unknown</returns>
        object GetValue(string key, object defValue);

        /// <summary>
        /// Gets a value as an integer from the session.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="defValue">the default value</param>
        /// <returns>the value or the default value if the key is unknown or the value cannot be returned as an integer</returns>
        int GetValue(string key, int defValue);

        /// <summary>
        /// Gets a value as a boolean from the session.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="defValue">the default value</param>
        /// <returns>the value or the default value if the key is unknown or the value cannot be returned as a boolean</returns>
        bool GetValue(string key, bool defValue);

        /// <summary>
        /// Adds a key-value pair to the session.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the value</param>
        void PutValue(string key, object value);

        /// <summary>
        /// Removes a key-value pair.
        /// </summary>
        /// <param name="key">the key</param>
        void RemoveValue(string key);

        /// <summary>
        /// Gets the authentication provider from the session.
        /// </summary>
        /// <returns>the authentication provider or <c>null</c> if no authentication provider has been set</returns>
        IAuthenticationProvider GetAuthenticationProvider();

        /// <summary>
        /// Gets the HTTP invoker from the session.
        /// </summary>
        /// <remarks>
        /// If no HTTP invoker has been set, a HTTP invoker is created, added to the session and returned.
        /// </remarks>
        /// <returns>the HTTP invoker</returns>
        IHttpInvoker GetHttpInvoker();
    }

    /// <summary>
    /// SPI interface.
    /// </summary>
    public interface ICmisSpi : IDisposable
    {
        /// <summary>
        /// Initializes the SPI with a binding session.
        /// </summary>
        /// <param name="session">the binding session</param>
        void Initialize(IBindingSession session);

        /// <summary>
        /// Returns the repository service instance.
        /// </summary>
        IRepositoryService GetRepositoryService();

        /// <summary>
        /// Returns the navigation service instance.
        /// </summary>
        INavigationService GetNavigationService();

        /// <summary>
        /// Returns the object service instance.
        /// </summary>
        IObjectService GetObjectService();

        /// <summary>
        /// Returns the versioning service instance.
        /// </summary>
        IVersioningService GetVersioningService();

        /// <summary>
        /// Returns the relationship service instance.
        /// </summary>
        IRelationshipService GetRelationshipService();

        /// <summary>
        /// Returns the discovery service instance.
        /// </summary>
        IDiscoveryService GetDiscoveryService();

        /// <summary>
        /// Returns the multi-filing service instance.
        /// </summary>
        IMultiFilingService GetMultiFilingService();

        /// <summary>
        /// Returns the ACL service instance.
        /// </summary>
        IAclService GetAclService();

        /// <summary>
        /// Returns the repository service instance.
        /// </summary>
        IPolicyService GetPolicyService();

        /// <summary>
        /// Clears all caches.
        /// </summary>
        void ClearAllCaches();

        /// <summary>
        /// Clears all caches of a repository.
        /// </summary>
        /// <param name="repositoryId">the repository ID</param>
        void ClearRepositoryCache(string repositoryId);
    }

    /// <summary>
    /// Authentication provider interface.
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <value>
        /// Gets the binding session instance
        /// </value>
        IBindingSession Session { get; set; }
    }

    /// <summary>
    /// Authentication provider interface for the portable HTTP client.
    /// </summary>
    public interface IPortableAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Prepares the HTTP client handler before it is used.
        /// </summary>
        /// <param name="httpClientHandler">the HTTP client handler</param>
        void PrepareHttpClientHandler(HttpClientHandler httpClientHandler);

        /// <summary>
        /// Prepares the HTTP request message before it is used.
        /// </summary>
        /// <param name="httpRequestMessage">the HTTP request message</param>
        void PrepareHttpRequestMessage(HttpRequestMessage httpRequestMessage);

        /// <summary>
        /// Handles the HTTP response if necessary.
        /// </summary>
        /// <param name="httpResponseMessage">the HTTP response message</param>
        void HandleResponse(HttpResponseMessage httpResponseMessage);
    }

    /// <summary>
    /// Base implementation of an authentication provider.
    /// </summary>
    public abstract class AbstractAuthenticationProvider : IPortableAuthenticationProvider
    {
        /// <inheritdoc/>
        public IBindingSession Session { get; set; }

        /// <value>
        /// Gets the HTTP cookie container.
        /// </value>
        public CookieContainer CookieContainer { get; private set; }

        /// <value>
        /// Gets the user name.
        /// </value>
        public string User { get { return Session.GetValue(SessionParameter.User) as string; } }

        /// <value>
        /// Gets the Password.
        /// </value>
        public string Password { get { return Session.GetValue(SessionParameter.Password) as string; } }

        /// <value>
        /// Gets the proxy user.
        /// </value>
        public string ProxyUser { get { return Session.GetValue(SessionParameter.ProxyUser) as string; } }

        /// <value>
        /// Gets the proxy password
        /// </value>
        public string ProxyPassword { get { return Session.GetValue(SessionParameter.ProxyPassword) as string; } }

        /// <inheritdoc/>
        public virtual void PrepareHttpClientHandler(HttpClientHandler httpClientHandler)
        {
            httpClientHandler.PreAuthenticate = true;
            httpClientHandler.UseCookies = true;
            httpClientHandler.UseProxy = true;
            CookieContainer = new CookieContainer();
            httpClientHandler.CookieContainer = CookieContainer;
        }

        /// <inheritdoc/>
        public virtual void PrepareHttpRequestMessage(HttpRequestMessage httpRequestMessage)
        {
        }

        /// <inheritdoc/>
        public virtual void HandleResponse(HttpResponseMessage httpResponseMessage)
        {
        }
    }

    /// <summary>
    /// Standard Authentication Provider.
    /// </summary>
    public class StandardAuthenticationProvider : AbstractAuthenticationProvider
    {
        /// <value>
        /// Gets the OAuth bearer token.
        /// </value>
        public string BearerToken { get { return Session.GetValue(SessionParameter.OAuthBearerToken) as string; } }

        /// <value>
        /// Gets the CSRF header.
        /// r</value>
        public string CsrfHeader { get { return Session.GetValue(SessionParameter.CsrfHeader) as string; } }

        /// <value>
        /// Gets the authentication header.
        /// </value>
        protected AuthenticationHeaderValue AuthenticationHeader { get; set; }

        /// <value>
        /// Gets the proxy authentication header.
        /// </value>
        protected AuthenticationHeaderValue ProxyAuthenticationHeader { get; set; }

        private object tokenLock = new object();
        private string token = "fetch";

        /// <value>
        /// Gets the CSRF header name.
        /// </value>
        protected string CsrfHeaderName { get; set; }

        /// <value>
        /// Gets the CSRF header token.
        /// </value>
        protected string CsrfToken
        {
            get { lock (tokenLock) { return token; } }
            set { lock (tokenLock) { token = value; } }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

    /// <summary>
    /// A factory that created low-level binding objects.
    /// </summary>
    public class CmisBindingFactory
    {
        /// <summary>
        /// Default CMIS AtomPub binding SPI implementation class name.
        /// </summary>
        public const string BindingSpiAtomPub = "PortCMIS.Binding.AtomPub.CmisAtomPubSpi";
        /// <summary>
        /// Default CMIS Browser binding SPI implementation class name.
        /// </summary>
        public const string BindingSpiBrowser = "PortCMIS.Binding.Browser.CmisBrowserSpi";

        /// <summary>
        /// Standard authentication provider class name.
        /// </summary>
        public const string StandardAuthenticationProviderClass = "PortCMIS.Binding.StandardAuthenticationProvider";

        /// <summary>
        /// Default HTTP invoker class name.
        /// </summary>
        public const string DefaultHttpInvokerClass = "PortCMIS.Binding.Http.DefaultHttpInvoker";

        private IDictionary<string, string> defaults;

        /// <summary>
        /// This is a factory.
        /// </summary>
        private CmisBindingFactory()
        {
            defaults = CreateNewDefaultParameters();
        }

        /// <summary>
        /// Creates a new instance of this factory.
        /// </summary>
        /// <returns>a factory object</returns>
        public static CmisBindingFactory NewInstance()
        {
            return new CmisBindingFactory();
        }

        /// <value>
        /// Gets and sets default session parameters.
        /// </value>
        public IDictionary<string, string> DefaultSessionParameters
        {
            get { return defaults; }
            set
            {
                if (value == null)
                {
                    defaults = CreateNewDefaultParameters();
                }
                else
                {
                    defaults = value;
                }
            }
        }

        /// <summary>
        /// Creates a binding object for custom binding implementations.
        /// </summary>
        /// <param name="sessionParameters">the session parameters</param>
        /// <param name="authenticationProvider">an authentication provider instance or <c>null</c> to use the default implementation</param>
        /// <returns>a low-level binding object</returns>
        public ICmisBinding CreateCmisBinding(IDictionary<string, string> sessionParameters, IAuthenticationProvider authenticationProvider)
        {
            CheckSessionParameters(sessionParameters, true);
            AddDefaultParameters(sessionParameters);

            return new CmisBinding(sessionParameters, authenticationProvider);
        }

        /// <summary>
        /// Creates an Browser binding object.
        /// </summary>
        /// <param name="sessionParameters">the session parameters</param>
        /// <param name="authenticationProvider">an authentication provider instance or <c>null</c> to use the default implementation</param>
        /// <returns>a low-level binding object</returns>
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

        /// <summary>
        /// Creates an AtomPub binding object.
        /// </summary>
        /// <param name="sessionParameters">the session parameters</param>
        /// <param name="authenticationProvider">an authentication provider instance or <c>null</c> to use the default implementation</param>
        /// <returns>a low-level binding object</returns>
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

        /// <summary>
        /// Creates a Web Services binding object.
        /// </summary>
        /// <remarks>
        /// PortCMIS doesn't support the Web Services binding. It may be implemented in the future.
        /// </remarks>
        /// <param name="sessionParameters">the session parameters</param>
        /// <param name="authenticationProvider">an authentication provider instance or <c>null</c> to use the default implementation</param>
        /// <returns>a low-level binding object</returns>
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
