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

namespace PortCMIS.Client
{
    public static class SessionParameter
    {
        // ---- general parameter ----
        public const string User = "org.apache.chemistry.portcmis.user";
        public const string Password = "org.apache.chemistry.portcmis.password";
        // ---- provider parameter ----
        // Predefined binding types
        public const string BindingType = "org.apache.chemistry.portcmis.binding.spi.type";
        public const string ForceCmisVersion = "org.apache.chemistry.portcmis.cmisversion";
        // Class name of the binding class.
        public const string BindingSpiClass = "org.apache.chemistry.portcmis.binding.spi.classname";
        // URL of the AtomPub service document.
        public const string AtomPubUrl = "org.apache.chemistry.portcmis.binding.atompub.url";
        // URL of the Browser service
        public const string BrowserUrl = "org.apache.chemistry.portcmis.binding.browser.url";
        // succinct flag
        public const string BrowserSuccinct = "org.apache.chemistry.portcmis.binding.browser.succinct";
        // date time format
        public const string BrowserDateTimeFormat = "org.apache.chemistry.portcmis.binding.browser.datetimeformat";
        // authentication provider
        public const string AuthenticationProviderClass = "org.apache.chemistry.portcmis.binding.auth.classname";
        // HTTP invoker
        public const string HttpInvokerClass = "org.apache.chemistry.portcmis.binding.httpinvoker.classname";
        // compression flag
        public const string Compression = "org.apache.chemistry.portcmis.binding.compression";
        // timeouts
        public const string ConnectTimeout = "org.apache.chemistry.portcmis.binding.connecttimeout";
        public const string ReadTimeout = "org.apache.chemistry.portcmis.binding.readtimeout";
        // binding caches
        public const string CacheSizeRepositories = "org.apache.chemistry.portcmis.binding.cache.repositories.size";
        public const string CacheSizeTypes = "org.apache.chemistry.portcmis.binding.cache.types.size";
        public const string CacheSizeLinks = "org.apache.chemistry.portcmis.binding.cache.links.size";
        // session parameter
        public const string ObjectFactoryClass = "org.apache.chemistry.portcmis.objectfactory.classname";
        public const string CacheClass = "org.apache.chemistry.portcmis.cache.classname";
        public const string RepositoryId = "org.apache.chemistry.portcmis.session.repository.id";
        public const string CacheSizeObjects = "org.apache.chemistry.portcmis.cache.objects.size";
        public const string CacheTTLObjects = "org.apache.chemistry.portcmis.cache.objects.ttl";
        public const string CacheSizePathToId = "org.apache.chemistry.portcmis.cache.pathtoid.size";
        public const string CacheTTLPathToId = "org.apache.chemistry.portcmis.cache.pathtoid.ttl";
        public const string CachePathOmit = "org.apache.chemistry.portcmis.cache.path.omit";

        // OAuth 2
        public const string OAuthBearerToken = "org.apache.chemistry.portcmis.binding.auth.http.oauth.bearer";
        // proxy
        public const string ProxyUser = "org.apache.chemistry.portcmis.binding.proxyuser";
        public const string ProxyPassword = "org.apache.chemistry.portcmis.binding.proxypassword";
        // CSRF 
        public const string CsrfHeader = "org.apache.chemistry.portcmis.binding.csrfheader";

        //  workarounds 
        public const string IncludeObjectIdUrlParamOnCheckout = "org.apache.chemistry.portcmis.workaround.includeObjectIdOnCheckout";
        public const string IncludeObjectIdUrlParamOnMove = "org.apache.chemistry.portcmis.workaround.includeObjectIdOnMove";
        public const string OmitChangeTokens = "org.apache.chemistry.opencmis.portcmis.omitChangeTokens";
    }
}
