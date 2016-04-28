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

using PortCMIS.binding;
using PortCMIS.Binding.Browser.Json;
using PortCMIS.Binding.Http;
using PortCMIS.Binding.Impl;
using PortCMIS.Binding.Services;
using PortCMIS.Client;
using PortCMIS.Data;
using PortCMIS.Data.Extensions;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PortCMIS.Binding.Browser
{
    /// <summary>
    /// Extended Repository Info for the Browser Binding.
    /// </summary>
    public class RepositoryInfoBrowserBinding : RepositoryInfo
    {
        public string RepositoryUrl { get; set; }
        public string RootUrl { get; set; }
    }

    /// <summary>
    /// Browser binding SPI.
    /// </summary>
    internal class CmisBrowserSpi : ICmisSpi
    {
        public const string RepositoryUrlCache = "org.apache.chemistry.portcmis.binding.browser.repositoryurls";

        private BindingSession session;
        private RepositoryService repositoryService;
        private NavigationService navigationService;
        private ObjectService objectService;
        private VersioningService versioningService;
        private DiscoveryService discoveryService;
        private MultiFilingService multiFilingService;
        private RelationshipService relationshipService;
        private PolicyService policyService;
        private AclService aclService;

        /// <inheritdoc/>
        public void Initialize(IBindingSession session)
        {
            this.session = session as BindingSession;
            if (this.session == null)
            {
                throw new ArgumentException("Invalid binding session!");
            }

            repositoryService = new RepositoryService(this.session);
            navigationService = new NavigationService(this.session);
            objectService = new ObjectService(this.session);
            versioningService = new VersioningService(this.session);
            discoveryService = new DiscoveryService(this.session);
            multiFilingService = new MultiFilingService(this.session);
            relationshipService = new RelationshipService(this.session);
            policyService = new PolicyService(this.session);
            aclService = new AclService(this.session);
        }

        /// <inheritdoc/>
        public IRepositoryService GetRepositoryService()
        {
            return repositoryService;
        }

        /// <inheritdoc/>
        public INavigationService GetNavigationService()
        {
            return navigationService;
        }

        /// <inheritdoc/>
        public IObjectService GetObjectService()
        {
            return objectService;
        }

        /// <inheritdoc/>
        public IVersioningService GetVersioningService()
        {
            return versioningService;
        }

        /// <inheritdoc/>
        public IRelationshipService GetRelationshipService()
        {
            return relationshipService;
        }

        /// <inheritdoc/>
        public IDiscoveryService GetDiscoveryService()
        {
            return discoveryService;
        }

        /// <inheritdoc/>
        public IMultiFilingService GetMultiFilingService()
        {
            return multiFilingService;
        }

        /// <inheritdoc/>
        public IAclService GetAclService()
        {
            return aclService;
        }

        /// <inheritdoc/>
        public IPolicyService GetPolicyService()
        {
            return policyService;
        }

        /// <inheritdoc/>
        public void ClearAllCaches()
        {
            session.RemoveValue(RepositoryUrlCache);
        }

        /// <inheritdoc/>
        public void ClearRepositoryCache(string repositoryId)
        {
            RepositoryUrlCache repUrlCache = session.GetValue(RepositoryUrlCache) as RepositoryUrlCache;
            if (repUrlCache != null)
            {
                repUrlCache.RemoveRepository(repositoryId);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // nothing to do
        }
    }

    internal class ClientTypeCache : ITypeCache
    {
        private string RepositoryId { get; set; }
        private AbstractBrowserBindingService Service { get; set; }

        public ClientTypeCache(string repositoryId, AbstractBrowserBindingService service)
        {
            this.RepositoryId = repositoryId;
            this.Service = service;
        }

        public ITypeDefinition GetTypeDefinition(string typeId)
        {
            TypeDefinitionCache cache = Service.Session.GetTypeDefinitionCache();

            ITypeDefinition type = cache.Get(RepositoryId, typeId);
            if (type == null)
            {
                type = Service.GetTypeDefinitionInternal(RepositoryId, typeId);
                if (type != null)
                {
                    cache.Put(RepositoryId, type);
                }
            }

            return type;
        }

        public ITypeDefinition ReloadTypeDefinition(string typeId)
        {
            TypeDefinitionCache cache = Service.Session.GetTypeDefinitionCache();

            ITypeDefinition type = Service.GetTypeDefinitionInternal(RepositoryId, typeId);
            if (type != null)
            {
                cache.Put(RepositoryId, type);
            }

            return type;
        }

        public ITypeDefinition GetTypeDefinitionForObject(string objectId)
        {
            return null;
        }

        public IPropertyDefinition GetPropertyDefinition(string propId)
        {
            return null;
        }
    }

    internal class FormDataComposer
    {
        public string CmisAction { get; private set; }

        public IContentStream Stream { get; set; }

        public IProperties Properties { get; set; }
        public bool Succinct { get; set; }
        public DateTimeFormat DateTimeFormat { get; set; }

        public IAcl AddAces { get; set; }
        public IAcl RemoveAces { get; set; }

        public IList<string> Policies { get; set; }
        public string PolicyId { get; set; }

        public IList<string> AddSecondaryTypeIds { get; set; }
        public IList<string> RemoveSecondaryTypeIds { get; set; }

        public IList<IBulkUpdateObjectIdAndChangeToken> ObjectIdsAndChangeTokens { get; set; }

        private IDictionary<string, object> parameters;

        public IDictionary<string, object> Parameters
        {
            get
            {
                if (parameters == null)
                {
                    parameters = new Dictionary<string, object>();
                }
                return parameters;
            }
            set { parameters = value; }
        }

        public FormDataComposer(string cmisAction)
        {
            CmisAction = cmisAction;
        }

        public HttpContent CreateHttpContent()
        {
            if (Stream == null)
            {
                return CreateFormUrlEncodedContent();
            }
            else
            {
                return CreateMultipartFormDataContent();
            }
        }

        protected FormUrlEncodedContent CreateFormUrlEncodedContent()
        {
            return new FormUrlEncodedContent(CreateContent());
        }

        protected MultipartFormDataContent CreateMultipartFormDataContent()
        {
            MultipartFormDataContent content = new MultipartFormDataContent();

            IList<KeyValuePair<string, string>> parameters = CreateContent();
            foreach (KeyValuePair<string, string> p in parameters)
            {
                content.Add(new StringContent(p.Value, Encoding.UTF8), p.Key);
            }

            if (Stream.Stream != null)
            {
                StreamContent streamContent = new StreamContent(Stream.Stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(Stream.MimeType ?? "application/octet-stream");

                content.Add(streamContent, "content", Stream.FileName ?? "content");
            }

            return content;
        }

        protected IList<KeyValuePair<string, string>> CreateContent()
        {
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            // set cmisaction
            result.Add(new KeyValuePair<string, string>(BindingConstants.ControlCmisAction, CmisAction));

            // set parameters
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> p in parameters)
                {
                    if (p.Value != null)
                    {
                        result.Add(new KeyValuePair<string, string>(p.Key, UrlBuilder.NormalizeParameter(p.Value)));
                    }
                }
            }

            // set succinct
            if (Succinct)
            {
                result.Add(new KeyValuePair<string, string>(BindingConstants.ControlSuccinct, "true"));
            }

            // set properties
            AddPropertiesParameters(Properties, result);

            // set Aces
            AddAcesParameters(AddAces, BindingConstants.ControlAddAcePrincipal, BindingConstants.ControlRemoveAcePermission, result);
            AddAcesParameters(RemoveAces, BindingConstants.ControlRemoveAcePrincipal, BindingConstants.ControlRemoveAcePermission, result);

            // set policies
            AddPoliciesParameters(Policies, result);
            AddPolicyIdParameter(PolicyId, result);

            // set secondary type IDs
            AddSecondaryTypeIdParameters(AddSecondaryTypeIds, BindingConstants.ControlAddSecondaryType, result);
            AddSecondaryTypeIdParameters(RemoveSecondaryTypeIds, BindingConstants.ControlRemoveSecondaryType, result);

            // set bulk update values
            AddObjectIdsAndChangeTokens(ObjectIdsAndChangeTokens, result);

            return result;
        }

        private void AddPropertiesParameters(IProperties properties, IList<KeyValuePair<string, string>> result)
        {
            if (properties == null || properties.PropertyList == null)
            {
                return;
            }

            int idx = 0;
            foreach (PropertyData prop in properties.PropertyList)
            {
                if (prop == null)
                {
                    continue;
                }

                string idxStr = "[" + idx + "]";
                result.Add(new KeyValuePair<string, string>(BindingConstants.ControlPropId + idxStr, prop.Id));

                if (prop.Values != null && prop.Values.Count > 0)
                {
                    if (prop.Values.Count == 1)
                    {
                        result.Add(new KeyValuePair<string, string>(
                            BindingConstants.ControlPropValue + idxStr, ConvertPropertyValue(prop.FirstValue, DateTimeFormat)));
                    }
                    else
                    {
                        int vidx = 0;
                        foreach (object obj in prop.Values)
                        {
                            string vidxStr = "[" + vidx + "]";
                            result.Add(new KeyValuePair<string, string>(
                                BindingConstants.ControlPropValue + idxStr + vidxStr, ConvertPropertyValue(obj, DateTimeFormat)));
                            vidx++;
                        }
                    }
                }
                idx++;
            }
        }

        private void AddAcesParameters(IAcl acl, string principalControl, string permissionControl, IList<KeyValuePair<string, string>> result)
        {
            if (acl == null || acl.Aces == null)
            {
                return;
            }

            int idx = 0;
            foreach (IAce ace in acl.Aces)
            {
                if (ace.PrincipalId != null && ace.Permissions != null && ace.Permissions.Count > 0)
                {
                    string idxStr = "[" + idx + "]";
                    result.Add(new KeyValuePair<string, string>(principalControl + idxStr, ace.PrincipalId));

                    int permIdx = 0;
                    foreach (string perm in ace.Permissions)
                    {
                        if (perm != null)
                        {
                            string permIdxStr = "[" + permIdx + "]";
                            result.Add(new KeyValuePair<string, string>(permissionControl + idxStr + permIdxStr, perm));
                            permIdx++;
                        }
                    }
                    idx++;
                }
            }
        }

        private void AddPoliciesParameters(IList<string> policies, IList<KeyValuePair<string, string>> result)
        {
            if (policies == null)
            {
                return;
            }

            int idx = 0;
            foreach (string policy in policies)
            {
                if (policy != null)
                {
                    string idxStr = "[" + idx + "]";
                    result.Add(new KeyValuePair<string, string>(BindingConstants.ControlPolicy + idxStr, policy));
                    idx++;
                }
            }
        }

        private void AddPolicyIdParameter(string policyId, IList<KeyValuePair<string, string>> result)
        {
            if (policyId == null)
            {
                return;
            }

            result.Add(new KeyValuePair<string, string>(BindingConstants.ControlPolicyId, policyId));
        }

        private void AddSecondaryTypeIdParameters(IList<string> secondaryTypeIds, string secondaryTypeIdControl, IList<KeyValuePair<string, string>> result)
        {
            if (secondaryTypeIds == null || secondaryTypeIds.Count == 0)
            {
                return;
            }

            int idx = 0;
            foreach (string typeId in secondaryTypeIds)
            {
                if (typeId == null || typeId.Length == 0)
                {
                    continue;
                }

                string idxStr = "[" + idx + "]";
                result.Add(new KeyValuePair<string, string>(secondaryTypeIdControl + idxStr, typeId));

                idx++;
            }
        }

        public void AddObjectIdsAndChangeTokens(IList<IBulkUpdateObjectIdAndChangeToken> objectIdsAndChangeTokens, IList<KeyValuePair<string, string>> result)
        {
            if (objectIdsAndChangeTokens == null || objectIdsAndChangeTokens.Count == 0)
            {
                return;
            }

            int idx = 0;
            foreach (IBulkUpdateObjectIdAndChangeToken oc in objectIdsAndChangeTokens)
            {
                if (oc == null || oc.Id == null || oc.Id.Length == 0)
                {
                    continue;
                }

                string idxStr = "[" + idx + "]";
                result.Add(new KeyValuePair<string, string>(BindingConstants.ControlObjectId + idxStr, oc.Id));
                result.Add(new KeyValuePair<string, string>(BindingConstants.ControlChangeToken + idxStr, oc.ChangeToken ?? ""));

                idx++;
            }
        }

        private string ConvertPropertyValue(object value, DateTimeFormat dateTimeFormat)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is DateTime)
            {
                if (DateTimeFormat == DateTimeFormat.Extended)
                {
                    return DateTimeHelper.FormatISO8601((DateTime)value);
                }
                else
                {
                    return DateTimeHelper.ConvertDateTimeToMillis((DateTime)value).ToString();
                }
            }
            else if (value is decimal)
            {
                return ((decimal)value).ToString("#", CultureInfo.InvariantCulture);
            }
            else if (value is BigInteger)
            {
                return ((BigInteger)value).ToString("#", CultureInfo.InvariantCulture);
            }
            else
            {
                return value.ToString();
            }
        }
    }


    /// <summary>
    /// Common service data and operations.
    /// </summary>
    internal abstract class AbstractBrowserBindingService
    {
        private BindingSession session;

        public BindingSession Session
        {
            get
            {
                return session;
            }
            protected set
            {
                session = value;

                Succinct = true;
                object succintObj = session.GetValue(SessionParameter.BrowserSuccinct);
                if (succintObj is string)
                {
                    Succinct = Convert.ToBoolean((string)succintObj);
                }

                DateTimeFormat = DateTimeFormat.Simple;
                object dateTimeFormatObj = session.GetValue(SessionParameter.BrowserDateTimeFormat);
                if (dateTimeFormatObj is string)
                {
                    DateTimeFormat? dtf = ((string)dateTimeFormatObj).GetCmisEnum<DateTimeFormat>();
                    DateTimeFormat = dtf ?? DateTimeFormat.Simple;
                }
            }
        }
        protected bool Succinct { get; private set; }
        protected string SuccinctParameter { get { return Succinct ? "true" : null; } }

        protected DateTimeFormat DateTimeFormat { get; private set; }
        protected string DateTimeFormatParameter { get { return DateTimeFormat == DateTimeFormat.Simple ? null : DateTimeFormat.GetCmisValue(); } }

        protected string GetServiceUrl()
        {
            return Session.GetValue(SessionParameter.BrowserUrl) as string;
        }

        protected UrlBuilder GetRepositoryUrl(string repositoryId, string selector)
        {
            UrlBuilder result = GetRepositoryUrlCache().GetRepositoryUrl(repositoryId, selector);

            if (result == null)
            {
                GetRepositoriesInternal(repositoryId);
                result = GetRepositoryUrlCache().GetRepositoryUrl(repositoryId, selector);
            }

            if (result == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository!");
            }

            return result;
        }

        protected UrlBuilder GetRepositoryUrl(string repositoryId)
        {
            UrlBuilder result = GetRepositoryUrlCache().GetRepositoryUrl(repositoryId);

            if (result == null)
            {
                GetRepositoriesInternal(repositoryId);
                result = GetRepositoryUrlCache().GetRepositoryUrl(repositoryId);
            }

            if (result == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository!");
            }

            return result;
        }

        protected UrlBuilder GetObjectUrl(string repositoryId, string objectId, string selector)
        {
            UrlBuilder result = GetRepositoryUrlCache().GetObjectUrl(repositoryId, objectId, selector);

            if (result == null)
            {
                GetRepositoriesInternal(repositoryId);
                result = GetRepositoryUrlCache().GetObjectUrl(repositoryId, objectId, selector);
            }

            if (result == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository!");
            }

            return result;
        }

        protected UrlBuilder GetObjectUrl(string repositoryId, string objectId)
        {
            UrlBuilder result = GetRepositoryUrlCache().GetObjectUrl(repositoryId, objectId);

            if (result == null)
            {
                GetRepositoriesInternal(repositoryId);
                result = GetRepositoryUrlCache().GetObjectUrl(repositoryId, objectId);
            }

            if (result == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository!");
            }

            return result;
        }

        protected UrlBuilder GetPathUrl(string repositoryId, string path, string selector)
        {
            UrlBuilder result = GetRepositoryUrlCache().GetPathUrl(repositoryId, path, selector);

            if (result == null)
            {
                GetRepositoriesInternal(repositoryId);
                result = GetRepositoryUrlCache().GetPathUrl(repositoryId, path, selector);
            }

            if (result == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository!");
            }

            return result;
        }


        protected RepositoryUrlCache GetRepositoryUrlCache()
        {
            RepositoryUrlCache repositoryUrlCache = Session.GetValue(CmisBrowserSpi.RepositoryUrlCache) as RepositoryUrlCache;
            if (repositoryUrlCache == null)
            {
                repositoryUrlCache = new RepositoryUrlCache();
                Session.PutValue(CmisBrowserSpi.RepositoryUrlCache, repositoryUrlCache);
            }

            return repositoryUrlCache;
        }

        protected void SetChangeToken(ref string changeToken, IObjectData obj)
        {
            if (changeToken == null)
            {
                return;
            }

            changeToken = null;

            if (obj == null || obj.Properties == null)
            {
                return;
            }

            IPropertyData ct = obj.Properties[PropertyIds.ChangeToken];
            if (ct != null && ct.PropertyType == PropertyType.String)
            {
                changeToken = (string)ct.FirstValue;
            }
        }

        // ---- exceptions ----

        /// <summary>
        /// Converts an error message or a HTTP status code into an Exception.
        /// </summary>
        protected CmisBaseException ConvertStatusCode(int code, string message, string errorContent, Exception e)
        {
            object obj = null;
            try
            {
                if (errorContent != null)
                {
                    JsonParser parser = new JsonParser();
                    obj = parser.Parse(new StringReader(errorContent));
                }
            }
            catch (JsonParseException)
            {
                // error content is not valid JSON -> ignore
            }

            if (obj is JsonObject)
            {
                JsonObject json = (JsonObject)obj;
                object jsonError = json[BrowserConstants.ErrorException];
                if (jsonError is string)
                {
                    object jsonMessage = json[BrowserConstants.ErrorMessage];
                    if (jsonMessage != null)
                    {
                        message = jsonMessage.ToString();
                    }

                    if (CmisConstraintException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisConstraintException(message, errorContent, e);
                    }
                    else if (CmisContentAlreadyExistsException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisContentAlreadyExistsException(message, errorContent, e);
                    }
                    else if (CmisFilterNotValidException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisFilterNotValidException(message, errorContent, e);
                    }
                    else if (CmisInvalidArgumentException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisInvalidArgumentException(message, errorContent, e);
                    }
                    else if (CmisNameConstraintViolationException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisNameConstraintViolationException(message, errorContent, e);
                    }
                    else if (CmisNotSupportedException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisNotSupportedException(message, errorContent, e);
                    }
                    else if (CmisObjectNotFoundException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisObjectNotFoundException(message, errorContent, e);
                    }
                    else if (CmisPermissionDeniedException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisPermissionDeniedException(message, errorContent, e);
                    }
                    else if (CmisStorageException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisStorageException(message, errorContent, e);
                    }
                    else if (CmisStreamNotSupportedException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisStreamNotSupportedException(message, errorContent, e);
                    }
                    else if (CmisUpdateConflictException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisUpdateConflictException(message, errorContent, e);
                    }
                    else if (CmisVersioningException.ExceptionName.Equals((string)jsonError, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisVersioningException(message, errorContent, e);
                    }
                    else if (code == 503)
                    {
                        return new CmisServiceUnavailableException(message, errorContent, e);
                    }
                }
            }

            // fall back to status code
            switch (code)
            {
                case 301:
                case 302:
                case 303:
                case 307:
                    return new CmisConnectionException("Redirects are not supported (HTTP status code " + code + "): " + message, errorContent, e);
                case 400:
                    return new CmisInvalidArgumentException(message, errorContent, e);
                case 401:
                    return new CmisUnauthorizedException(message, errorContent, e);
                case 403:
                    return new CmisPermissionDeniedException(message, errorContent, e);
                case 404:
                    return new CmisObjectNotFoundException(message, errorContent, e);
                case 405:
                    return new CmisNotSupportedException(message, errorContent, e);
                case 407:
                    return new CmisProxyAuthenticationException(message, errorContent, e);
                case 409:
                    return new CmisConstraintException(message, errorContent, e);
                case 503:
                    return new CmisServiceUnavailableException(message, errorContent, e);
                default:
                    return new CmisRuntimeException(message, errorContent, e);
            }
        }


        // ---- helpers ----

        /// <summary>
        /// Parses an object from an input stream.
        /// </summary>
        protected JsonObject ParseObject(Stream stream, string charset)
        {
            object obj = Parse(stream, charset);

            if (obj is JsonObject)
            {
                return (JsonObject)obj;
            }

            throw new CmisConnectionException("Unexpected object!");
        }

        /// <summary>
        /// Parses an array from an input stream.
        /// </summary>
        protected JsonArray ParseArray(Stream stream, string charset)
        {
            object obj = Parse(stream, charset);

            if (obj is JsonArray)
            {
                return (JsonArray)obj;
            }

            throw new CmisConnectionException("Unexpected object!");
        }

        /// <summary>
        /// Parses an input stream.
        /// </summary>
        protected object Parse(Stream stream, string charset)
        {
            Encoding enc;
            if (charset == null)
            {
                enc = Encoding.UTF8;
            }
            else
            {
                try
                {
                    enc = Encoding.GetEncoding(charset);
                }
                catch (Exception e)
                {
                    throw new CmisConnectionException("Unsupported charset: " + charset, e);
                }
            }

            StreamReader reader = null;

            object obj = null;
            try
            {
                reader = new StreamReader(stream, enc);
                JsonParser parser = new JsonParser();
                obj = parser.Parse(reader);
            }
            catch (Exception e)
            {
                throw new CmisConnectionException("Parsing exception!", e);
            }
            finally
            {
                IOUtils.ConsumeAndClose(reader);
                if (reader == null)
                {
                    stream.Dispose();
                }
            }

            return obj;
        }

        /// <summary>
        /// Performs a GET on an URL, checks the response code and returns the
        /// result.
        /// </summary>
        protected IResponse Read(UrlBuilder url)
        {
            // make the call
            IResponse resp = Session.GetHttpInvoker().InvokeGET(url, session);

            // check response code
            if (resp.StatusCode != 200)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            return resp;
        }


        /// <summary>
        /// Performs a POST on an URL, checks the response code and returns the
        /// result.
        /// </summary>
        protected IResponse Post(UrlBuilder url, HttpContent content)
        {
            // make the call
            IResponse resp = Session.GetHttpInvoker().InvokePOST(url, content, session);

            // check response code
            if (resp.StatusCode != 200 && resp.StatusCode != 201)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            return resp;
        }

        /// <summary>
        /// Performs a POST on an URL, checks the response code and returns the
        /// result.
        /// </summary>
        protected void PostAndConsume(UrlBuilder url, HttpContent content)
        {
            IResponse resp = Post(url, content);
            IOUtils.ConsumeAndClose(resp.Stream);
        }

        // -----

        protected IList<IRepositoryInfo> GetRepositoriesInternal(string repositoryId)
        {
            UrlBuilder url = null;

            if (repositoryId == null)
            {
                // no repository id provided -> get all
                url = new UrlBuilder(GetServiceUrl());
            }
            else
            {
                // use URL of the specified repository
                url = GetRepositoryUrlCache().GetRepositoryUrl(repositoryId, BindingConstants.SelectorRepositoryInfo);
                if (url == null)
                {
                    // repository infos haven't been fetched yet -> get them all
                    url = new UrlBuilder(GetServiceUrl());
                }
            }

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            IList<IRepositoryInfo> repInfos = new List<IRepositoryInfo>();

            foreach (KeyValuePair<string, object> jri in json)
            {
                if (jri.Value is JsonObject)
                {
                    IRepositoryInfo ri = JsonConverter.ConvertRepositoryInfo((JsonObject)jri.Value);
                    string id = ri.Id;

                    if (ri is RepositoryInfoBrowserBinding)
                    {
                        string repositoryUrl = ((RepositoryInfoBrowserBinding)ri).RepositoryUrl;
                        string rootUrl = ((RepositoryInfoBrowserBinding)ri).RootUrl;

                        if (id == null || repositoryUrl == null || rootUrl == null)
                        {
                            throw new CmisConnectionException("Found invalid Repository Info! (id: " + id + ")");
                        }

                        GetRepositoryUrlCache().AddRepository(id, repositoryUrl, rootUrl);
                    }

                    repInfos.Add(ri);
                }
                else
                {
                    throw new CmisConnectionException("Found invalid Repository Info!");
                }
            }

            return repInfos;
        }

        /// <summary>
        /// Retrieves a type definition.
        /// </summary>
        public ITypeDefinition GetTypeDefinitionInternal(string repositoryId, string typeId)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId, BindingConstants.SelectorTypeDefinition);
            url.AddParameter(BindingConstants.ParamTypeId, typeId);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            return JsonConverter.ConvertTypeDefinition(json);
        }
    }

    internal class RepositoryService : AbstractBrowserBindingService, IRepositoryService
    {
        public RepositoryService(BindingSession session)
        {
            Session = session;
        }

        public IList<IRepositoryInfo> GetRepositoryInfos(IExtensionsData extension)
        {
            return GetRepositoriesInternal(null);
        }

        public IRepositoryInfo GetRepositoryInfo(string repositoryId, IExtensionsData extension)
        {
            IList<IRepositoryInfo> repositoryInfos = GetRepositoriesInternal(repositoryId);

            if (repositoryInfos.Count == 0)
            {
                throw new CmisObjectNotFoundException("Repository '" + repositoryId + "'not found!");
            }

            if (repositoryInfos.Count == 1)
            {
                return repositoryInfos[0];
            }

            // find the repository
            foreach (IRepositoryInfo info in repositoryInfos)
            {
                if (info.Id == null)
                {
                    continue;
                }

                if (info.Id == repositoryId)
                {
                    return info;
                }
            }

            throw new CmisObjectNotFoundException("Repository '" + repositoryId + "'not found!");
        }

        public ITypeDefinitionList GetTypeChildren(string repositoryId, string typeId, bool? includePropertyDefinitions,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId, BindingConstants.SelectorTypeChildren);
            url.AddParameter(BindingConstants.ParamTypeId, typeId);
            url.AddParameter(BindingConstants.ParamPropertyDefinitions, includePropertyDefinitions);
            url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
            url.AddParameter(BindingConstants.ParamSkipCount, skipCount);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            return JsonConverter.ConvertTypeChildren(json);
        }

        public IList<ITypeDefinitionContainer> GetTypeDescendants(string repositoryId, string typeId, BigInteger? depth,
            bool? includePropertyDefinitions, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId, BindingConstants.SelectorTypeDecendants);
            url.AddParameter(BindingConstants.ParamTypeId, typeId);
            url.AddParameter(BindingConstants.ParamDepth, depth);
            url.AddParameter(BindingConstants.ParamPropertyDefinitions, includePropertyDefinitions);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonArray json = ParseArray(resp.Stream, resp.Charset);

            return JsonConverter.ConvertTypeDescendants(json);
        }

        public ITypeDefinition GetTypeDefinition(string repositoryId, string typeId, IExtensionsData extension)
        {
            return GetTypeDefinitionInternal(repositoryId, typeId);
        }


        public ITypeDefinition CreateType(string repositoryId, ITypeDefinition type, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCreateType);
            if (type != null)
            {
                StringWriter sw = new StringWriter();
                JsonConverter.Convert(type, DateTimeFormat).WriteJsonString(sw);

                composer.Parameters[BindingConstants.ControlType] = sw.ToString();
            }

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            return JsonConverter.ConvertTypeDefinition(json);
        }

        public ITypeDefinition UpdateType(string repositoryId, ITypeDefinition type, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionUpdateType);
            if (type != null)
            {
                StringWriter sw = new StringWriter();
                JsonConverter.Convert(type, DateTimeFormat).WriteJsonString(sw);

                composer.Parameters[BindingConstants.ControlType] = sw.ToString();
            }

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            return JsonConverter.ConvertTypeDefinition(json);
        }

        public void DeleteType(string repositoryId, string typeId, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionDeleteType);
            composer.Parameters[BindingConstants.ControlTypeId] = typeId;

            // send
            PostAndConsume(url, composer.CreateHttpContent());
        }
    }

    internal class NavigationService : AbstractBrowserBindingService, INavigationService
    {
        public NavigationService(BindingSession session)
        {
            Session = session;
        }

        public IObjectInFolderList GetChildren(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, folderId, BindingConstants.SelectorChildren);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamOrderBy, orderBy);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPathSegment, includePathSegment);
            url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
            url.AddParameter(BindingConstants.ParamSkipCount, skipCount);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObjectInFolderList(json, typeCache);
        }

        public IList<IObjectInFolderContainer> GetDescendants(string repositoryId, string folderId, BigInteger? depth, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, folderId, BindingConstants.SelectorDecendants);
            url.AddParameter(BindingConstants.ParamDepth, depth);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPathSegment, includePathSegment);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonArray json = ParseArray(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertDescendants(json, typeCache);
        }

        public IList<IObjectInFolderContainer> GetFolderTree(string repositoryId, string folderId, BigInteger? depth, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, folderId, BindingConstants.SelectorFolderTree);
            url.AddParameter(BindingConstants.ParamDepth, depth);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPathSegment, includePathSegment);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonArray json = ParseArray(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertDescendants(json, typeCache);
        }

        public IList<IObjectParentData> GetObjectParents(string repositoryId, string objectId, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includeRelativePathSegment, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorParents);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamRelativePathSegment, includeRelativePathSegment);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonArray json = ParseArray(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObjectParents(json, typeCache);
        }

        public IObjectData GetFolderParent(string repositoryId, string folderId, string filter, ExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, folderId, BindingConstants.SelectorParent);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObject(json, typeCache);
        }

        public IObjectList GetCheckedOutDocs(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = (folderId != null ? GetObjectUrl(repositoryId, folderId, BindingConstants.SelectorCheckedout)
                    : GetRepositoryUrl(repositoryId, BindingConstants.SelectorCheckedout));
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamOrderBy, orderBy);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
            url.AddParameter(BindingConstants.ParamSkipCount, skipCount);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObjectList(json, typeCache, false);
        }
    }

    internal class ObjectService : AbstractBrowserBindingService, IObjectService
    {
        public ObjectService(BindingSession session)
        {
            Session = session;
        }

        public string CreateDocument(string repositoryId, IProperties properties, string folderId, IContentStream contentStream,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = (folderId != null ? GetObjectUrl(repositoryId, folderId) : GetRepositoryUrl(repositoryId));

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCreateDocument);
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.Parameters[BindingConstants.ParamVersioningState] = versioningState;
            composer.Policies = policies;
            composer.AddAces = addAces;
            composer.RemoveAces = removeAces;
            composer.Stream = contentStream;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);
            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            return (newObj == null ? null : newObj.Id);
        }

        public string CreateDocumentFromSource(string repositoryId, string sourceId, IProperties properties, string folderId,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = (folderId != null ? GetObjectUrl(repositoryId, folderId) : GetRepositoryUrl(repositoryId));

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCreateDocumentFromSource);
            composer.Parameters[BindingConstants.ParamSourceId] = sourceId;
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.Parameters[BindingConstants.ParamVersioningState] = versioningState;
            composer.Policies = policies;
            composer.AddAces = addAces;
            composer.RemoveAces = removeAces;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            return (newObj == null ? null : newObj.Id);
        }

        public string CreateFolder(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, folderId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCreateFolder);
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.Policies = policies;
            composer.AddAces = addAces;
            composer.RemoveAces = removeAces;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            return (newObj == null ? null : newObj.Id);
        }

        public string CreateRelationship(string repositoryId, IProperties properties, IList<string> policies, IAcl addAces,
            IAcl removeAces, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCreateRelationship);
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.Policies = policies;
            composer.AddAces = addAces;
            composer.RemoveAces = removeAces;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            return (newObj == null ? null : newObj.Id);
        }

        public string CreatePolicy(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = (folderId != null ? GetObjectUrl(repositoryId, folderId) : GetRepositoryUrl(repositoryId));

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCreatePolicy);
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.Policies = policies;
            composer.AddAces = addAces;
            composer.RemoveAces = removeAces;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            return (newObj == null ? null : newObj.Id);
        }

        public string CreateItem(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = (folderId != null ? GetObjectUrl(repositoryId, folderId) : GetRepositoryUrl(repositoryId));

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCreateItem);
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.Policies = policies;
            composer.AddAces = addAces;
            composer.RemoveAces = removeAces;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            return (newObj == null ? null : newObj.Id);
        }

        public IAllowableActions GetAllowableActions(string repositoryId, string objectId, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorAllowableActionS);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            return JsonConverter.ConvertAllowableActions(json);
        }

        public IProperties GetProperties(string repositoryId, string objectId, string filter, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorProperties);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            if (Succinct)
            {
                ITypeCache typeCache = new ClientTypeCache(repositoryId, this);
                return JsonConverter.ConvertSuccinctProperties(json, null, typeCache);
            }
            else
            {
                return JsonConverter.ConvertProperties(json, null);
            }
        }

        public IList<IRenditionData> GetRenditions(string repositoryId, string objectId, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorRenditions);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
            url.AddParameter(BindingConstants.ParamSkipCount, skipCount);

            // read and parse
            IResponse resp = Read(url);
            JsonArray json = ParseArray(resp.Stream, resp.Charset);

            return JsonConverter.ConvertRenditions(json);
        }

        public IObjectData GetObject(string repositoryId, string objectId, string filter, bool? includeAllowableActions,
            IncludeRelationships? includeRelationships, string renditionFilter, bool? includePolicyIds,
            bool? includeAcl, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorObject);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPolicyIds, includePolicyIds);
            url.AddParameter(BindingConstants.ParamAcl, includeAcl);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObject(json, typeCache);
        }

        public IObjectData GetObjectByPath(string repositoryId, string path, string filter, bool? includeAllowableActions,
            IncludeRelationships? includeRelationships, string renditionFilter, bool? includePolicyIds, bool? includeAcl,
            IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetPathUrl(repositoryId, path, BindingConstants.SelectorObject);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPolicyIds, includePolicyIds);
            url.AddParameter(BindingConstants.ParamAcl, includeAcl);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObject(json, typeCache);
        }

        public IContentStream GetContentStream(string repositoryId, string objectId, string streamId, BigInteger? offset, BigInteger? length,
            IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorContent);
            url.AddParameter(BindingConstants.ParamStreamId, streamId);

            // get the content
            IResponse resp = Session.GetHttpInvoker().InvokeGET(url, Session, (long?)offset, (long?)length);

            // check response code
            if ((resp.StatusCode != 200) && (resp.StatusCode != 206))
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            // build result object
            ContentStream result;
            if (resp.StatusCode == 206)
            {
                result = new PartialContentStream();
            }
            else
            {
                result = new ContentStream();
            }

            result.FileName = resp.Filename;
            result.Length = resp.ContentLength;
            result.MimeType = resp.ContentType;
            result.Stream = resp.Stream;

            return result;
        }

        public void UpdateProperties(string repositoryId, ref string objectId, ref string changeToken, IProperties properties,
            IExtensionsData extension)
        {
            // we need an object ID
            if (objectId == null || objectId.Length == 0)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionUpdateProperties);
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.Parameters[BindingConstants.ParamChangeToken] =
                (changeToken == null || Session.GetValue(SessionParameter.OmitChangeTokens, false) ? null : changeToken);

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            objectId = (newObj == null ? null : newObj.Id);

            SetChangeToken(ref changeToken, newObj);
        }

        public IList<IBulkUpdateObjectIdAndChangeToken> BulkUpdateProperties(string repositoryId,
                IList<IBulkUpdateObjectIdAndChangeToken> objectIdAndChangeToken, IProperties properties,
                IList<string> addSecondaryTypeIds, IList<string> removeSecondaryTypeIds, IExtensionsData extension)
        {
            // we need object ids
            if (objectIdAndChangeToken == null || objectIdAndChangeToken.Count == 0)
            {
                throw new CmisInvalidArgumentException("Object IDs must be set!");
            }

            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionBulkUpdate);
            composer.ObjectIdsAndChangeTokens = objectIdAndChangeToken;
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.AddSecondaryTypeIds = addSecondaryTypeIds;
            composer.RemoveSecondaryTypeIds = removeSecondaryTypeIds;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonArray json = ParseArray(resp.Stream, resp.Charset);

            return JsonConverter.ConvertBulkUpdate(json);
        }

        public void MoveObject(string repositoryId, ref string objectId, string targetFolderId, string sourceFolderId,
            IExtensionsData extension)
        {
            // we need an object id
            if (objectId == null || objectId.Length == 0)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionMove);
            composer.Parameters[BindingConstants.ParamTargetFolderId] = targetFolderId;
            composer.Parameters[BindingConstants.ParamSourceFolderId] = sourceFolderId;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            objectId = (newObj == null ? null : newObj.Id);
        }

        public void DeleteObject(string repositoryId, string objectId, bool? allVersions, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionDelete);
            composer.Parameters[BindingConstants.ParamAllVersions] = allVersions;

            // send
            PostAndConsume(url, composer.CreateHttpContent());
        }

        public IFailedToDeleteData DeleteTree(string repositoryId, string folderId, bool? allVersions, UnfileObject? unfileObjects,
            bool? continueOnFailure, ExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, folderId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionDeleteTree);
            composer.Parameters[BindingConstants.ParamAllVersions] = allVersions;
            composer.Parameters[BindingConstants.ParamUnfileObjects] = unfileObjects;
            composer.Parameters[BindingConstants.ParamContinueOnFailure] = continueOnFailure;

            // send
            IResponse resp = Post(url, composer.CreateHttpContent());

            if (resp.StatusCode == 200 && resp.ContentLength > 0 && (!resp.Stream.CanSeek || resp.Stream.Length > 0))
            {
                try
                {
                    JsonObject json = ParseObject(resp.Stream, resp.Charset);
                    return JsonConverter.ConvertFailedToDelete(json);
                }
                catch (IOException e)
                {
                    throw new CmisConnectionException("Cannot read response!", e);
                }
            }

            return new FailedToDeleteData();
        }

        public void SetContentStream(string repositoryId, ref string objectId, bool? overwriteFlag, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension)
        {
            // we need an object id
            if (objectId == null || objectId.Length == 0)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionSetContent);
            composer.Stream = contentStream;
            composer.Parameters[BindingConstants.ParamOverwriteFlag] = overwriteFlag;
            composer.Succinct = Succinct;
            composer.Parameters[BindingConstants.ParamChangeToken] =
                (changeToken == null || Session.GetValue(SessionParameter.OmitChangeTokens, false) ? null : changeToken);


            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            objectId = (newObj == null ? null : newObj.Id);

            SetChangeToken(ref changeToken, newObj);
        }

        public void AppendContentStream(string repositoryId, ref string objectId, bool? isLastChunk, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension)
        {
            // we need an object id
            if (objectId == null || objectId.Length == 0)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionAppendContent);
            composer.Stream = contentStream;
            composer.Parameters[BindingConstants.ControlIsLastChunk] = isLastChunk;
            composer.Succinct = Succinct;
            composer.Parameters[BindingConstants.ParamChangeToken] =
                (changeToken == null || Session.GetValue(SessionParameter.OmitChangeTokens, false) ? null : changeToken);

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            objectId = (newObj == null ? null : newObj.Id);

            SetChangeToken(ref changeToken, newObj);
        }

        public void DeleteContentStream(string repositoryId, ref string objectId, ref string changeToken, IExtensionsData extension)
        {
            // we need an object id
            if (objectId == null || objectId.Length == 0)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionDeleteContent);
            composer.Succinct = Succinct;
            composer.Parameters[BindingConstants.ParamChangeToken] =
                (changeToken == null || Session.GetValue(SessionParameter.OmitChangeTokens, false) ? null : changeToken);

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            objectId = (newObj == null ? null : newObj.Id);

            SetChangeToken(ref changeToken, newObj);
        }
    }

    internal class VersioningService : AbstractBrowserBindingService, IVersioningService
    {
        public VersioningService(BindingSession session)
        {
            Session = session;
        }

        public void CheckOut(string repositoryId, ref string objectId, IExtensionsData extension, out bool? contentCopied)
        {
            // we need an object id
            if (objectId == null || objectId.Length == 0)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCheckOut);
            composer.Succinct = Succinct;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            objectId = (newObj == null ? null : newObj.Id);

            contentCopied = null;
        }

        public void CancelCheckOut(string repositoryId, string objectId, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCancelCheckOut);

            // send
            PostAndConsume(url, composer.CreateHttpContent());
        }

        public void CheckIn(string repositoryId, ref string objectId, bool? major, IProperties properties,
            IContentStream contentStream, string checkinComment, IList<string> policies, IAcl addAces, IAcl removeAces,
            IExtensionsData extension)
        {
            // we need an object id
            if (objectId == null || objectId.Length == 0)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionCheckIn);
            composer.Stream = contentStream;
            composer.Parameters[BindingConstants.ParamMajor] = major;
            composer.Properties = properties;
            composer.Succinct = Succinct;
            composer.DateTimeFormat = DateTimeFormat;
            composer.Parameters[BindingConstants.ParamCheckinComment] = checkinComment;
            composer.Policies = policies;
            composer.AddAces = addAces;
            composer.RemoveAces = removeAces;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            IObjectData newObj = JsonConverter.ConvertObject(json, typeCache);

            objectId = (newObj == null ? null : newObj.Id);
        }

        public IObjectData GetObjectOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool? major,
            string filter, bool? includeAllowableActions, IncludeRelationships? includeRelationships,
            string renditionFilter, bool? includePolicyIds, bool? includeAcl, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorObject);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPolicyIds, includePolicyIds);
            url.AddParameter(BindingConstants.ParamAcl, includeAcl);
            url.AddParameter(BindingConstants.ParamReturnVersion, (major == true ? ReturnVersion.LatestMajor : ReturnVersion.Latest));
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);

            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObject(json, typeCache);
        }

        public IProperties GetPropertiesOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool? major,
            string filter, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorProperties);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamReturnVersion, (major == true ? ReturnVersion.LatestMajor : ReturnVersion.Latest));
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            if (Succinct)
            {
                ITypeCache typeCache = new ClientTypeCache(repositoryId, this);
                return JsonConverter.ConvertSuccinctProperties(json, null, typeCache);
            }
            else
            {
                return JsonConverter.ConvertProperties(json, null);
            }
        }

        public IList<IObjectData> GetAllVersions(string repositoryId, string objectId, string versionSeriesId, string filter,
            bool? includeAllowableActions, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorVersions);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);

            JsonArray json = ParseArray(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObjects(json, typeCache);
        }
    }

    internal class DiscoveryService : AbstractBrowserBindingService, IDiscoveryService
    {
        public DiscoveryService(BindingSession session)
        {
            Session = session;
        }

        public IObjectList Query(string repositoryId, string statement, bool? searchAllVersions,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionQuery);
            composer.Parameters[BindingConstants.ParamStatement] = statement;
            composer.Parameters[BindingConstants.ParamSearchAllVersions] = searchAllVersions;
            composer.Parameters[BindingConstants.ParamAllowableActions] = includeAllowableActions;
            composer.Parameters[BindingConstants.ParamRelationships] = includeRelationships;
            composer.Parameters[BindingConstants.ParamRenditionfilter] = renditionFilter;
            composer.Parameters[BindingConstants.ParamMaxItems] = maxItems;
            composer.Parameters[BindingConstants.ParamSkipCount] = skipCount;
            composer.DateTimeFormat = DateTimeFormat;
            // Important: No succinct flag here!!!

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            JsonObject json = ParseObject(resp.Stream, resp.Charset);
            return JsonConverter.ConvertObjectList(json, typeCache, true);
        }

        public IObjectList GetContentChanges(string repositoryId, ref string changeLogToken, bool? includeProperties,
           string filter, bool? includePolicyIds, bool? includeAcl, BigInteger? maxItems, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetRepositoryUrl(repositoryId, BindingConstants.SelectorContentChanges);
            url.AddParameter(BindingConstants.ParamChangeLogToken, changeLogToken);
            url.AddParameter(BindingConstants.ParamProperties, includeProperties);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamPolicyIds, includePolicyIds);
            url.AddParameter(BindingConstants.ParamAcl, includeAcl);
            url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            if (changeLogToken != null && json != null)
            {
                object token = json[BrowserConstants.JsonObjectListChangeLogToken];
                if (token is string)
                {
                    changeLogToken = (string)token;
                }
            }

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObjectList(json, typeCache, false);
        }
    }

    internal class RelationshipService : AbstractBrowserBindingService, IRelationshipService
    {
        public RelationshipService(BindingSession session)
        {
            Session = session;
        }

        public IObjectList GetObjectRelationships(string repositoryId, string objectId, bool? includeSubRelationshipTypes,
            RelationshipDirection? relationshipDirection, string typeId, string filter, bool? includeAllowableActions,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorRelationships);
            url.AddParameter(BindingConstants.ParamSubRelationshipTypes, includeSubRelationshipTypes);
            url.AddParameter(BindingConstants.ParamRelationshipDirection, relationshipDirection);
            url.AddParameter(BindingConstants.ParamTypeId, typeId);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
            url.AddParameter(BindingConstants.ParamSkipCount, skipCount);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObjectList(json, typeCache, false);
        }
    }

    internal class MultiFilingService : AbstractBrowserBindingService, IMultiFilingService
    {
        public MultiFilingService(BindingSession session)
        {
            Session = session;
        }

        public void AddObjectToFolder(string repositoryId, string objectId, string folderId, bool? allVersions, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionRemoveObjectToFolder);
            composer.Parameters[BindingConstants.ParamFolderId] = folderId;
            composer.Parameters[BindingConstants.ParamAllVersions] = allVersions;

            // send
            PostAndConsume(url, composer.CreateHttpContent());
        }

        public void RemoveObjectFromFolder(string repositoryId, string objectId, string folderId, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionRemoveObjectFromFolder);
            composer.Parameters[BindingConstants.ParamFolderId] = folderId;

            // send
            PostAndConsume(url, composer.CreateHttpContent());
        }
    }

    internal class PolicyService : AbstractBrowserBindingService, IPolicyService
    {
        public PolicyService(BindingSession session)
        {
            Session = session;
        }

        public void ApplyPolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionApplyPolicy);
            composer.PolicyId = policyId;

            // send
            PostAndConsume(url, composer.CreateHttpContent());
        }

        public void RemovePolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionRemovePolicy);
            composer.PolicyId = policyId;

            // send
            PostAndConsume(url, composer.CreateHttpContent());
        }

        public IList<IObjectData> GetAppliedPolicies(string repositoryId, string objectId, string filter, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorPolicies);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamSuccinct, SuccinctParameter);
            url.AddParameter(BindingConstants.ParamDateTimeFormat, DateTimeFormatParameter);

            // read and parse
            IResponse resp = Read(url);
            JsonArray json = ParseArray(resp.Stream, resp.Charset);

            ITypeCache typeCache = new ClientTypeCache(repositoryId, this);

            return JsonConverter.ConvertObjects(json, typeCache);
        }
    }

    internal class AclService : AbstractBrowserBindingService, IAclService
    {
        public AclService(BindingSession session)
        {
            Session = session;
        }

        public IAcl GetAcl(string repositoryId, string objectId, bool? onlyBasicPermissions, IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId, BindingConstants.SelectorAcl);
            url.AddParameter(BindingConstants.ParamOnlyBasicPermissions, onlyBasicPermissions);

            // read and parse
            IResponse resp = Read(url);
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            return JsonConverter.ConvertAcl(json);
        }

        public IAcl ApplyAcl(string repositoryId, string objectId, IAcl addAces, IAcl removeAces, AclPropagation? aclPropagation,
            IExtensionsData extension)
        {
            // build URL
            UrlBuilder url = GetObjectUrl(repositoryId, objectId);

            // prepare form data
            FormDataComposer composer = new FormDataComposer(BindingConstants.CmisActionApplyAcl);
            composer.AddAces = addAces;
            composer.RemoveAces = removeAces;
            composer.Parameters[BindingConstants.ParamAclPropagation] = aclPropagation;

            // send and parse
            IResponse resp = Post(url, composer.CreateHttpContent());
            JsonObject json = ParseObject(resp.Stream, resp.Charset);

            return JsonConverter.ConvertAcl(json);
        }
    }
}