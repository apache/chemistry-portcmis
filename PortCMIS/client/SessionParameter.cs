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
    /// <summary>
    /// Session parameter constants.
    /// </summary>
    public static class SessionParameter
    {
        // ---- general parameter ----

        /// <summary>Repository ID</summary>
        public const string RepositoryId = "org.apache.chemistry.portcmis.session.repository.id";

        /// <summary>User</summary>
        public const string User = "org.apache.chemistry.portcmis.user";

        /// <summary>Password</summary>
        public const string Password = "org.apache.chemistry.portcmis.password";

        /// <summary>Preemptive authentication ("true" or "false") (basic auth only)</summary>
        public const string PreemptivAuthentication = "org.apache.chemistry.portcmis.binding.auth.http.preemptive";

        // ---- provider parameter ----

        /// <summary>Binding Type ("atompub" or "browser")</summary>
        /// <seealso cref="PortCMIS.BindingType"/>
        public const string BindingType = "org.apache.chemistry.portcmis.binding.spi.type";

        /// <summary>Force CMIS version ("1.0" or "1.1")</summary>
        public const string ForceCmisVersion = "org.apache.chemistry.portcmis.cmisversion";

        /// <summary>Class name of the binding class</summary>
        public const string BindingSpiClass = "org.apache.chemistry.portcmis.binding.spi.classname";

        /// <summary>URL of the AtomPub service document</summary>
        public const string AtomPubUrl = "org.apache.chemistry.portcmis.binding.atompub.url";

        /// <summary>URL of the Browser service</summary>
        public const string BrowserUrl = "org.apache.chemistry.portcmis.binding.browser.url";

        /// <summary>Succinct flag (browser binding only)</summary>
        public const string BrowserSuccinct = "org.apache.chemistry.portcmis.binding.browser.succinct";

        /// <summary>DateTime format ("simple" or "extended") (browser binding only)</summary>
        /// <seealso cref="PortCMIS.Enums.DateTimeFormat"/>
        public const string BrowserDateTimeFormat = "org.apache.chemistry.portcmis.binding.browser.datetimeformat";

        /// <summary>Class name of the authentication provider class</summary>
        public const string AuthenticationProviderClass = "org.apache.chemistry.portcmis.binding.auth.classname";

        /// <summary>Class name of the HTTP invoker class</summary>
        public const string HttpInvokerClass = "org.apache.chemistry.portcmis.binding.httpinvoker.classname";

        /// <summary>Compression flag ("true" or "false")</summary>
        public const string Compression = "org.apache.chemistry.portcmis.binding.compression";

        /// <summary>Connect timeout in milliseconds</summary>
        public const string ConnectTimeout = "org.apache.chemistry.portcmis.binding.connecttimeout";

        /// <summary>Read timeout in milliseconds</summary>
        public const string ReadTimeout = "org.apache.chemistry.portcmis.binding.readtimeout";

        /// <summary>OAuth 2 bearer token</summary>
        public const string OAuthBearerToken = "org.apache.chemistry.portcmis.binding.auth.http.oauth.bearer";

        /// <summary>Proxy user</summary>
        public const string ProxyUser = "org.apache.chemistry.portcmis.binding.proxyuser";

        /// <summary>Proxy password</summary>
        public const string ProxyPassword = "org.apache.chemistry.portcmis.binding.proxypassword";

        /// <summary>CSRF HTTP header</summary>
        public const string CsrfHeader = "org.apache.chemistry.portcmis.binding.csrfheader";

        /// <summary>User agent</summary>
        public const string UserAgent = "org.apache.chemistry.portcmis.binding.useragent";

        // ---- binding caches ----

        /// <summary>Size of the repositories cache</summary>
        public const string CacheSizeRepositories = "org.apache.chemistry.portcmis.binding.cache.repositories.size";

        /// <summary>Size of the types cache</summary>
        public const string CacheSizeTypes = "org.apache.chemistry.portcmis.binding.cache.types.size";

        /// <summary>Size of the link cache</summary>
        public const string CacheSizeLinks = "org.apache.chemistry.portcmis.binding.cache.links.size";

        // ---- session parameter ----

        /// <summary>Class name of the object factory class</summary>
        public const string ObjectFactoryClass = "org.apache.chemistry.portcmis.objectfactory.classname";

        /// <summary>Class name of the object cache class</summary>
        public const string CacheClass = "org.apache.chemistry.portcmis.cache.classname";

        /// <summary>Size of the object cache</summary>
        public const string CacheSizeObjects = "org.apache.chemistry.portcmis.cache.objects.size";

        /// <summary>Time-to-live of objects in the object cache</summary>
        public const string CacheTTLObjects = "org.apache.chemistry.portcmis.cache.objects.ttl";

        /// <summary>Size of the path cache</summary>
        public const string CacheSizePathToId = "org.apache.chemistry.portcmis.cache.pathtoid.size";

        /// <summary>Time-to-live of objects in the path cache</summary>
        public const string CacheTTLPathToId = "org.apache.chemistry.portcmis.cache.pathtoid.ttl";

        /// <summary>Path cache ("true" or "false")</summary>
        public const string CachePathOmit = "org.apache.chemistry.portcmis.cache.path.omit";

        // ---- workarounds ----

        /// <summary>Defines if the object ID should be added to the move URL ("true" or "false")
        /// (Workaround for SharePoint 2010)</summary>
        public const string IncludeObjectIdUrlParamOnCheckout = "org.apache.chemistry.portcmis.workaround.includeObjectIdOnCheckout";

        /// <summary>Defines if the change token should be omitted for updating calls ("true" or "false")
        /// (Workaround for SharePoint 2010 and SharePoint 2013)</summary>
        public const string IncludeObjectIdUrlParamOnMove = "org.apache.chemistry.portcmis.workaround.includeObjectIdOnMove";

        /// <summary>Defines if the document name should be added to the properties on check in if no properties are updated ("true" or "false")
        /// (Workaround for SharePoint 2010 and SharePoint 2013)</summary>
        public const string OmitChangeTokens = "org.apache.chemistry.portcmis.workaround.omitChangeTokens";

        /// <summary>Defines if the document name should be added to the properties on check in if no properties are updated
        /// (Workaround for SharePoint 2010 and SharePoint 2013)</summary>
        public const string AddNameOnCheckIn = "org.apache.chemistry.portcmis.workaround.addNameOnCheckIn";

        /// <summary>Defines if getObjectOfLatestVersion should use the version series ID instead of the object ID
        /// (Workaround for SharePoint 2010 and SharePoint 2013)</summary>
        public const string LatestVersionWithVersionSeriesId = "org.apache.chemistry.portcmis.workaround.getLatestVersionWithVersionSeriesId";
    }
}