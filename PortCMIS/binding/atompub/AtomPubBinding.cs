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
using PortCMIS.Binding.Http;
using PortCMIS.Binding.Impl;
using PortCMIS.Binding.Services;
using PortCMIS.Client;
using PortCMIS.Client.Impl;
using PortCMIS.Data;
using PortCMIS.Data.Extensions;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Xml;

namespace PortCMIS.Binding.AtomPub
{

    /// <summary>
    /// Browser binding SPI.
    /// </summary>
    internal class CmisAtomPubSpi : ICmisSpi
    {
        public const string SessionLinkCache = "org.apache.chemistry.portcmis.binding.atompub.linkcache";

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

        public IRepositoryService GetRepositoryService()
        {
            return repositoryService;
        }

        public INavigationService GetNavigationService()
        {
            return navigationService;
        }

        public IObjectService GetObjectService()
        {
            return objectService;
        }

        public IVersioningService GetVersioningService()
        {
            return versioningService;
        }

        public IRelationshipService GetRelationshipService()
        {
            return relationshipService;
        }

        public IDiscoveryService GetDiscoveryService()
        {
            return discoveryService;
        }

        public IMultiFilingService GetMultiFilingService()
        {
            return multiFilingService;
        }

        public IAclService GetAclService()
        {
            return aclService;
        }

        public IPolicyService GetPolicyService()
        {
            return policyService;
        }

        public void ClearAllCaches()
        {
            session.RemoveValue(SessionLinkCache);
        }

        public void ClearRepositoryCache(string repositoryId)
        {
            LinkCache linkCache = session.GetValue(SessionLinkCache) as LinkCache;
            if (linkCache != null)
            {
                linkCache.ClearRepository(repositoryId);
            }
        }

        public void Dispose()
        {
            // nothing to do
        }
    }

    /// <summary>
    /// Common service data and operations.
    /// </summary>
    internal abstract class AbstractAtomPubService
    {
        protected enum IdentifierType
        {
            ID, PATH
        }

        protected const string NAME_COLLECTION = "collection";
        protected const string NAME_URI_TEMPLATE = "uritemplate";
        protected const string NAME_PATH_SEGMENT = "pathSegment";
        protected const string NAME_RELATIVE_PATH_SEGMENT = "relativePathSegment";
        protected const string NAME_NUM_ITEMS = "numItems";

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
            }
        }

        /// <summary>
        /// Returns the service document URL of this session.
        /// </summary>
        protected string GetServiceDocURL()
        {
            return Session.GetValue(SessionParameter.AtomPubUrl) as string;
        }

        /// <summary>
        /// Return the CMIS version of the given repository.
        /// </summary>
        protected CmisVersion GetCmisVersion(string repositoryId)
        {
            CmisVersion? forcedVersion = Session.GetValue(SessionParameter.ForceCmisVersion) as CmisVersion?;

            if (forcedVersion != null)
            {
                return (CmisVersion)forcedVersion;
            }

            RepositoryInfoCache cache = Session.GetRepositoryInfoCache();
            IRepositoryInfo info = cache.Get(repositoryId);

            if (info == null)
            {
                IList<IRepositoryInfo> infoList = GetRepositoriesInternal(repositoryId);
                if (infoList != null && infoList.Count > 0)
                {
                    info = infoList[0];
                    cache.Put(info);
                }
            }

            return info == null ? CmisVersion.Cmis_1_0 : info.CmisVersion;
        }

        // ---- link cache ----

        /// <summary>
        /// Returns the link cache or creates a new cache if it doesn't exist.
        /// </summary>
        protected LinkCache GetLinkCache()
        {
            LinkCache linkCache = (LinkCache)Session.GetValue(CmisAtomPubSpi.SessionLinkCache);
            if (linkCache == null)
            {
                linkCache = new LinkCache(Session);
                Session.PutValue(CmisAtomPubSpi.SessionLinkCache, linkCache);
            }

            return linkCache;
        }

        /// <summary>
        /// Gets a link from the cache.
        /// </summary>
        protected string GetLink(string repositoryId, string id, string rel, string type)
        {
            if (repositoryId == null)
            {
                throw new CmisInvalidArgumentException("Repository ID must be set!");
            }

            if (id == null)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            return GetLinkCache().GetLink(repositoryId, id, rel, type);
        }

        /// <summary>
        /// Gets a link from the cache.
        /// </summary>
        protected string GetLink(string repositoryId, string id, string rel)
        {
            return GetLink(repositoryId, id, rel, null);
        }

        /// <summary>
        /// Gets a link from the cache if it is there or loads it into the cache if
        /// it is not there.
        /// </summary>
        public string LoadLink(string repositoryId, string id, string rel, string type)
        {
            string link = GetLink(repositoryId, id, rel, type);
            if (link == null)
            {
                GetObjectInternal(repositoryId, IdentifierType.ID, id, ReturnVersion.This, "cmis:objectId", false,
                        IncludeRelationships.None, "cmis:none", false, false, null);
                link = GetLink(repositoryId, id, rel, type);
            }

            return link;
        }

        /// <summary>
        /// Gets the content link from the cache if it is there or loads it into the
        /// cache if it is not there.
        /// </summary>

        public string LoadContentLink(string repositoryId, string id)
        {
            return LoadLink(repositoryId, id, AtomPubParser.LinkRelContent, null);
        }

        /// <summary>
        /// Gets a rendition content link from the cache if it is there or loads it
        /// into the cache if it is not there.
        /// </summary>

        public string LoadRenditionContentLink(string repositoryId, string id, string streamId)
        {
            return LoadLink(repositoryId, id, BindingConstants.RelAlternate, streamId);
        }

        /// <summary>
        /// Adds a link to the cache.
        /// </summary>
        protected void AddLink(string repositoryId, string id, string rel, string type, string link)
        {
            GetLinkCache().AddLink(repositoryId, id, rel, type, link);
        }

        /// <summary>
        /// Adds a link to the cache.
        /// </summary>
        protected void AddLink(string repositoryId, string id, AtomLink link)
        {
            GetLinkCache().AddLink(repositoryId, id, link.Rel, link.Type, link.Href);
        }

        /// <summary>
        /// Removes all links of an object.
        /// </summary>
        protected void RemoveLinks(string repositoryId, string id)
        {
            GetLinkCache().RemoveLinks(repositoryId, id);
        }

        /// <summary>
        /// Locks the link cache.
        /// </summary>
        protected void LockLinks()
        {
            GetLinkCache().LockLinks();
        }

        /// <summary>
        /// Unlocks the link cache.
        /// </summary>
        protected void UnlockLinks()
        {
            GetLinkCache().UnlockLinks();
        }

        /// <summary>
        /// Checks a link throw an appropriate exception.
        /// </summary>
        protected void ThrowLinkException(string repositoryId, string id, string rel, string type)
        {
            int index = GetLinkCache().CheckLink(repositoryId, id, rel, type);

            switch (index)
            {
                case 0:
                    throw new CmisObjectNotFoundException("Unknown repository!");
                case 1:
                    throw new CmisObjectNotFoundException("Unknown object!");
                case 2:
                    throw new CmisNotSupportedException("Operation not supported by the repository for this object!");
                case 3:
                    throw new CmisNotSupportedException("No link with matching media type!");
                case 4:
                    throw new CmisRuntimeException("Nothing wrong! Either this is a bug or a threading issue.");
                default:
                    throw new CmisRuntimeException("Unknown error!");
            }
        }

        /// <summary>
        /// Gets a type link from the cache.
        /// </summary>
        protected string GetTypeLink(string repositoryId, string typeId, string rel, string type)
        {
            if (repositoryId == null)
            {
                throw new CmisInvalidArgumentException("Repository ID must be set!");
            }

            if (typeId == null)
            {
                throw new CmisInvalidArgumentException("Type ID must be set!");
            }

            return GetLinkCache().GetTypeLink(repositoryId, typeId, rel, type);
        }

        /// <summary>
        /// Gets a type link from the cache.
        /// </summary>
        protected string GetTypeLink(string repositoryId, string typeId, string rel)
        {
            return GetTypeLink(repositoryId, typeId, rel, null);
        }

        /// <summary>
        /// Gets a link from the cache if it is there or loads it into the cache if
        /// it is not there.
        /// </summary>
        protected string loadTypeLink(string repositoryId, string typeId, string rel, string type)
        {
            string link = GetTypeLink(repositoryId, typeId, rel, type);
            if (link == null)
            {
                GetTypeDefinitionInternal(repositoryId, typeId);
                link = GetTypeLink(repositoryId, typeId, rel, type);
            }

            return link;
        }

        /// <summary>
        /// Adds a type link to the cache.
        /// </summary>
        protected void AddTypeLink(string repositoryId, string typeId, string rel, string type, string link)
        {
            GetLinkCache().AddTypeLink(repositoryId, typeId, rel, type, link);
        }

        /// <summary>
        /// Adds a type link to the cache.
        /// </summary>
        protected void AddTypeLink(string repositoryId, string typeId, AtomLink link)
        {
            GetLinkCache().AddTypeLink(repositoryId, typeId, link.Rel, link.Type, link.Href);
        }

        /// <summary>
        /// Removes all links of a type.
        /// </summary>
        protected void RemoveTypeLinks(string repositoryId, string id)
        {
            GetLinkCache().RemoveTypeLinks(repositoryId, id);
        }

        /// <summary>
        /// Locks the type link cache.
        /// </summary>
        protected void LockTypeLinks()
        {
            GetLinkCache().LockTypeLinks();
        }

        /// <summary>
        /// Unlocks the type link cache.
        /// </summary>
        protected void UnlockTypeLinks()
        {
            GetLinkCache().UnlockTypeLinks();
        }

        /// <summary>
        /// Gets a collection from the cache.
        /// </summary>
        protected string GetCollection(string repositoryId, string collection)
        {
            return GetLinkCache().GetCollection(repositoryId, collection);
        }

        /// <summary>
        /// Gets a collection from the cache if it is there or loads it into the
        /// cache if it is not there.
        /// </summary>
        protected string loadCollection(string repositoryId, string collection)
        {
            string link = GetCollection(repositoryId, collection);
            if (link == null)
            {
                // cache repository info
                GetRepositoriesInternal(repositoryId);
                link = GetCollection(repositoryId, collection);
            }

            return link;
        }

        /// <summary>
        /// Adds a collection to the cache.
        /// </summary>
        protected void AddCollection(string repositoryId, string collection, string link)
        {
            GetLinkCache().AddCollection(repositoryId, collection, link);
        }

        /// <summary>
        /// Gets a repository link from the cache.
        /// </summary>
        protected string GetRepositoryLink(string repositoryId, string rel)
        {
            return GetLinkCache().GetRepositoryLink(repositoryId, rel);
        }

        /// <summary>
        /// Gets a repository link from the cache if it is there or loads it into the
        /// cache if it is not there.
        /// </summary>
        protected string LoadRepositoryLink(string repositoryId, string rel)
        {
            string link = GetRepositoryLink(repositoryId, rel);
            if (link == null)
            {
                // cache repository info
                GetRepositoriesInternal(repositoryId);
                link = GetRepositoryLink(repositoryId, rel);
            }

            return link;
        }

        /// <summary>
        /// Adds a repository link to the cache.
        /// </summary>
        protected void AddRepositoryLink(string repositoryId, string rel, string link)
        {
            GetLinkCache().AddRepositoryLink(repositoryId, rel, link);
        }

        /// <summary>
        /// Adds a repository link to the cache.
        /// </summary>
        protected void AddRepositoryLink(string repositoryId, AtomLink link)
        {
            AddRepositoryLink(repositoryId, link.Rel, link.Href);
        }

        /// <summary>
        /// Gets an URI template from the cache.
        /// </summary>
        protected string GetTemplateLink(string repositoryId, string type, Dictionary<string, object> parameters)
        {
            return GetLinkCache().GetTemplateLink(repositoryId, type, parameters);
        }

        /// <summary>
        /// Gets a template link from the cache if it is there or loads it into the
        /// cache if it is not there.
        /// </summary>
        protected string LoadTemplateLink(string repositoryId, string type, Dictionary<string, object> parameters)
        {
            string link = GetTemplateLink(repositoryId, type, parameters);
            if (link == null)
            {
                // cache repository info
                GetRepositoriesInternal(repositoryId);
                link = GetTemplateLink(repositoryId, type, parameters);
            }

            return link;
        }

        /// <summary>
        /// Adds an URI template to the cache.
        /// </summary>
        protected void AddTemplate(string repositoryId, string type, string link)
        {
            GetLinkCache().AddTemplate(repositoryId, type, link);
        }

        // ---- exceptions ----

        /// <summary>
        /// Converts a HTTP status code into an Exception.
        /// </summary>
        protected CmisBaseException ConvertStatusCode(int code, string message, string errorContent, Exception ex)
        {
            string exception = ExtractException(errorContent);
            message = ExtractErrorMessage(message, errorContent);

            switch (code)
            {
                case 301:
                case 302:
                case 303:
                case 307:
                    return new CmisConnectionException("Redirects are not supported (HTTP status code " + code + "): "
                            + message, errorContent, ex);
                case 400:
                    if (CmisFilterNotValidException.ExceptionName.Equals(exception, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisFilterNotValidException(message, errorContent, ex);
                    }
                    return new CmisInvalidArgumentException(message, errorContent, ex);
                case 401:
                    return new CmisUnauthorizedException(message, errorContent, ex);
                case 403:
                    if (CmisStreamNotSupportedException.ExceptionName.Equals(exception, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisStreamNotSupportedException(message, errorContent, ex);
                    }
                    return new CmisPermissionDeniedException(message, errorContent, ex);
                case 404:
                    return new CmisObjectNotFoundException(message, errorContent, ex);
                case 405:
                    return new CmisNotSupportedException(message, errorContent, ex);
                case 407:
                    return new CmisProxyAuthenticationException(message, errorContent, ex);
                case 409:
                    if (CmisContentAlreadyExistsException.ExceptionName.Equals(exception, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisContentAlreadyExistsException(message, errorContent, ex);
                    }
                    else if (CmisVersioningException.ExceptionName.Equals(exception, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisVersioningException(message, errorContent, ex);
                    }
                    else if (CmisUpdateConflictException.ExceptionName.Equals(exception, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisUpdateConflictException(message, errorContent, ex);
                    }
                    else if (CmisNameConstraintViolationException.ExceptionName.Equals(exception, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisNameConstraintViolationException(message, errorContent, ex);
                    }
                    return new CmisConstraintException(message, errorContent, ex);
                case 503:
                    return new CmisServiceUnavailableException(message, errorContent, ex);
                default:
                    if (CmisStorageException.ExceptionName.Equals(exception, StringComparison.OrdinalIgnoreCase))
                    {
                        return new CmisStorageException(message, errorContent, ex);
                    }
                    return new CmisRuntimeException(message, errorContent, ex);
            }
        }

        protected string ExtractException(string errorContent)
        {
            if (errorContent == null)
            {
                return null;
            }

            int begin = errorContent.IndexOf("<!--exception-->");
            int end = errorContent.IndexOf("<!--/exception-->");

            if (begin == -1 || end == -1 || begin > end)
            {
                return null;
            }

            return errorContent.Substring(begin + "<!--exception-->".Length, end);
        }

        protected string ExtractErrorMessage(string message, string errorContent)
        {
            if (errorContent == null)
            {
                return message;
            }

            int begin = errorContent.IndexOf("<!--message-->");
            int end = errorContent.IndexOf("<!--/message-->");

            if (begin == -1 || end == -1 || begin > end)
            {
                return message;
            }

            return errorContent.Substring(begin + "<!--message-->".Length, end);
        }

        // ---- helpers ----

        protected bool Matches(string name, AtomElement element)
        {
            return name == element.LocalName;
        }

        protected bool isStr(string name, AtomElement element)
        {
            return Matches(name, element) && (element.Object is String);
        }

        protected bool isInt(string name, AtomElement element)
        {
            return Matches(name, element) && (element.Object is BigInteger);
        }

        protected bool isNextLink(AtomElement element)
        {
            return BindingConstants.RelNext == ((AtomLink)element.Object).Rel;
        }

        /// <summary>
        /// Creates a CMIS object with properties and policy IDs.
        /// </summary>
        protected ObjectData CreateObject(IProperties properties, string changeToken, List<string> policies)
        {
            ObjectData obj = new ObjectData();

            bool omitChangeToken = Session.GetValue(SessionParameter.OmitChangeTokens, false);

            if (properties == null)
            {
                properties = new Properties();
                if (changeToken != null && !omitChangeToken)
                {

                    PropertyData changeTokenProp = new PropertyData(PropertyType.String);
                    changeTokenProp.Id = PropertyIds.ChangeToken;
                    changeTokenProp.AddValue(changeToken);

                    ((Properties)properties).AddProperty(changeTokenProp);
                }
            }
            else
            {
                if (omitChangeToken)
                {
                    if (properties[PropertyIds.ChangeToken] != null)
                    {
                        properties = new Properties(properties);
                        ((Properties)properties).RemoveProperty(PropertyIds.ChangeToken);
                    }
                }
                else
                {
                    if (changeToken != null && properties[PropertyIds.ChangeToken] == null)
                    {

                        PropertyData changeTokenProp = new PropertyData(PropertyType.String);
                        changeTokenProp.Id = PropertyIds.ChangeToken;
                        changeTokenProp.AddValue(changeToken);

                        properties = new Properties(properties);
                        ((Properties)properties).AddProperty(changeTokenProp);
                    }
                }
            }

            obj.Properties = properties;

            if (policies != null && policies.Count > 0)
            {
                PolicyIdList policyIdList = new PolicyIdList();
                policyIdList.PolicyIds = policies;
                obj.PolicyIds = policyIdList;
            }

            return obj;
        }

        /// <summary>
        /// Creates a CMIS object that only contains an ID in the property list.
        /// </summary>
        protected IObjectData createIdObject(string objectId)
        {
            ObjectData obj = new ObjectData();

            Properties properties = new Properties();
            obj.Properties = properties;

            PropertyData idProp = new PropertyData(PropertyType.Id);
            idProp.Id = PropertyIds.ObjectId;
            idProp.AddValue(objectId);

            properties.AddProperty(idProp);

            return obj;
        }

        /// <summary>
        /// Parses an input stream.
        /// </summary>
        protected T Parse<T>(Stream stream) where T : AtomBase
        {
            AtomPubParser parser = new AtomPubParser(stream);

            try
            {
                parser.Parse();
            }
            catch (Exception e)
            {
                throw new CmisConnectionException("Parsing exception!", e);
            }

            AtomBase parseResult = parser.GetResults();

            T result = parseResult as T;
            if (result == null)
            {
                throw new CmisConnectionException("Unexpected document! Received "
                        + (parseResult == null ? "something unknown" : parseResult.Type) + "!");
            }

            return (T)parseResult;
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
            if (resp.StatusCode != 201)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            return resp;
        }

        /// <summary>
        /// Performs a POST on an URL, checks the response code and consumes the
        /// response.
        /// </summary>
        protected void postAndConsume(UrlBuilder url, HttpContent content)
        {
            IResponse resp = Post(url, content);
            IOUtils.ConsumeAndClose(resp.Stream);
        }

        /// <summary>
        /// Performs a PUT on an URL, checks the response code and returns the
        /// result.
        /// </summary>
        protected IResponse Put(UrlBuilder url, HttpContent content)
        {
            return Put(url, null, content);
        }

        /// <summary>
        /// Performs a PUT on an URL, checks the response code and returns the
        /// result.
        /// </summary>
        protected IResponse Put(UrlBuilder url, Dictionary<string, string> headers, HttpContent content)
        {
            // make the call
            IResponse resp = Session.GetHttpInvoker().InvokePUT(url, headers, content, session);

            // check response code
            if (resp.StatusCode < 200 || resp.StatusCode > 299)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            return resp;
        }

        /// <summary>
        /// Performs a DELETE on an URL, checks the response code and returns the
        /// result.
        /// </summary>
        protected void Delete(UrlBuilder url)
        {
            // make the call
            IResponse resp = Session.GetHttpInvoker().InvokeDELETE(url, session);

            // check response code
            if (resp.StatusCode != 204)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }
        }

        // ---- common operations ----

        /// <summary>
        /// Checks if at least one ACE list is not empty.
        /// </summary>
        protected bool IsAclMergeRequired(IAcl addAces, IAcl removeAces)
        {
            return (addAces != null && addAces.Aces != null && addAces.Aces.Count > 0) ||
                (removeAces != null && removeAces.Aces != null && removeAces.Aces.Count > 0);
        }

        /// <summary>
        /// Merges the new ACL from original, add and remove ACEs lists.
        /// </summary>
        protected IAcl MergeAcls(IAcl originalAces, IAcl addAces, IAcl removeAces)
        {
            Dictionary<string, HashSet<string>> originals = ConvertAclToMap(originalAces);
            Dictionary<string, HashSet<string>> adds = ConvertAclToMap(addAces);
            Dictionary<string, HashSet<string>> removes = ConvertAclToMap(removeAces);
            IList<IAce> newAces = new List<IAce>();

            // iterate through the original ACEs
            foreach (KeyValuePair<string, HashSet<string>> ace in originals)
            {

                // add permissions
                HashSet<string> addPermissions;
                if (adds.TryGetValue(ace.Key, out addPermissions))
                {
                    foreach (string perm in addPermissions)
                    {
                        ace.Value.Add(perm);
                    }
                }

                // remove permissions
                HashSet<string> removePermissions;
                if (removes.TryGetValue(ace.Key, out removePermissions))
                {
                    foreach (string perm in removePermissions)
                    {
                        ace.Value.Remove(perm);
                    }
                }

                // create new ACE
                if (ace.Value.Count > 0)
                {
                    Ace newAce = new Ace();
                    newAce.Principal = new Principal() { Id = ace.Key };
                    newAce.Permissions = ace.Value.ToList();
                    newAces.Add(newAce);
                }
            }

            // find all ACEs that should be added but are not in the original ACE list
            foreach (KeyValuePair<string, HashSet<string>> ace in adds)
            {
                if (!originals.ContainsKey(ace.Key) && ace.Value.Count > 0)
                {
                    Ace newAce = new Ace();
                    newAce.Principal = new Principal() { Id = ace.Key };
                    newAce.Permissions = ace.Value.ToList();
                    newAces.Add(newAce);
                }
            }

            return new Acl() { Aces = newAces };
        }

        /// <summary>
        /// Converts a list of ACEs into Map for better handling.
        /// </summary>
        private static Dictionary<string, HashSet<string>> ConvertAclToMap(IAcl acl)
        {
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();

            if (acl == null || acl.Aces == null)
            {
                return result;
            }

            foreach (IAce ace in acl.Aces)
            {
                // don't consider indirect ACEs - we can't change them
                if (!ace.IsDirect)
                {
                    // ignore
                    continue;
                }

                // although a principal must not be null, check it
                if (ace.Principal == null || ace.Principal.Id == null)
                {
                    // ignore
                    continue;
                }

                HashSet<string> permissions;
                if (!result.TryGetValue(ace.Principal.Id, out permissions))
                {
                    permissions = new HashSet<string>();
                    result[ace.Principal.Id] = permissions;
                }

                if (ace.Permissions != null)
                {
                    foreach (string perm in ace.Permissions)
                    {
                        permissions.Add(perm);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the Service Document from the server and caches the repository
        /// info objects, collections, links, URI templates, etc.
        /// </summary>
        protected IList<IRepositoryInfo> GetRepositoriesInternal(string repositoryId)
        {
            List<IRepositoryInfo> repInfos = new List<IRepositoryInfo>();

            // retrieve service doc
            UrlBuilder url = new UrlBuilder(GetServiceDocURL());
            url.AddParameter(BindingConstants.ParamRepositoryId, repositoryId);

            // read and parse
            IResponse resp = Read(url);
            ServiceDoc serviceDoc = Parse<ServiceDoc>(resp.Stream);

            // walk through the workspaces
            foreach (RepositoryWorkspace ws in serviceDoc.Workspaces)
            {
                if (ws.Id == null)
                {
                    // found a non-CMIS workspace
                    continue;
                }

                foreach (AtomElement element in ws.Elements)
                {
                    if (Matches(NAME_COLLECTION, element))
                    {
                        Dictionary<string, string> colMap = (Dictionary<string, string>)element.Object;
                        string collectionType;
                        colMap.TryGetValue("collectionType", out collectionType);
                        string href;
                        colMap.TryGetValue("href", out href);

                        AddCollection(ws.Id, collectionType, href);
                    }
                    else if (element.Object is AtomLink)
                    {
                        AddRepositoryLink(ws.Id, (AtomLink)element.Object);
                    }
                    else if (Matches(NAME_URI_TEMPLATE, element))
                    {
                        Dictionary<string, string> tempMap = (Dictionary<string, string>)element.Object;
                        string type;
                        tempMap.TryGetValue("type", out type);
                        string template;
                        tempMap.TryGetValue("template", out template);

                        AddTemplate(ws.Id, type, template);
                    }
                    else if (element.Object is RepositoryInfo)
                    {
                        repInfos.Add((RepositoryInfo)element.Object);
                    }
                }
            }

            return repInfos;
        }

        /// <summary>
        /// Retrieves an object from the server and caches the links.
        /// </summary>
        protected IObjectData GetObjectInternal(string repositoryId, IdentifierType idOrPath, string objectIdOrPath,
                ReturnVersion? returnVersion, string filter, bool? includeAllowableActions,
                IncludeRelationships? includeRelationships, string renditionFilter, bool? includePolicyIds,
                bool? includeAcl, IExtensionsData extension)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters[BindingConstants.ParamId] = objectIdOrPath;
            parameters[BindingConstants.ParamPath] = objectIdOrPath;
            parameters[BindingConstants.ParamReturnVersion] = returnVersion;
            parameters[BindingConstants.ParamFilter] = filter;
            parameters[BindingConstants.ParamAllowableActions] = includeAllowableActions;
            parameters[BindingConstants.ParamAcl] = includeAcl;
            parameters[BindingConstants.ParamPolicyIds] = includePolicyIds;
            parameters[BindingConstants.ParamRelationships] = includeRelationships;
            parameters[BindingConstants.ParamRenditionfilter] = renditionFilter;

            string link = LoadTemplateLink(repositoryId, (idOrPath == IdentifierType.ID ? BindingConstants.TemplateObjectById
                    : BindingConstants.TemplateObjectByPath), parameters);
            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository!");
            }

            UrlBuilder url = new UrlBuilder(link);
            // workaround for missing template parameter in the CMIS spec
            if (returnVersion != null && returnVersion != ReturnVersion.This)
            {
                url.AddParameter(BindingConstants.ParamReturnVersion, returnVersion);
            }

            // read and parse
            IResponse resp = Read(url);
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // we expect a CMIS entry
            if (entry.Id == null)
            {
                throw new CmisConnectionException("Received Atom entry is not a CMIS entry!");
            }

            LockLinks();
            IObjectData result = null;
            try
            {
                // clean up cache
                RemoveLinks(repositoryId, entry.Id);

                // walk through the entry
                foreach (AtomElement element in entry.Elements)
                {
                    if (element.Object is AtomLink)
                    {
                        AddLink(repositoryId, entry.Id, (AtomLink)element.Object);
                    }
                    else if (element.Object is IObjectData)
                    {
                        result = (IObjectData)element.Object;
                    }
                }
            }
            finally
            {
                UnlockLinks();
            }

            return result;
        }

        /// <summary>
        /// Retrieves a type definition.
        /// </summary>
        protected ITypeDefinition GetTypeDefinitionInternal(string repositoryId, string typeId)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters[BindingConstants.ParamId] = typeId;

            string link = LoadTemplateLink(repositoryId, BindingConstants.TemplateTypeById, parameters);
            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository!");
            }

            // read and parse
            IResponse resp = Read(new UrlBuilder(link));
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // we expect a CMIS entry
            if (entry.Id == null)
            {
                throw new CmisConnectionException("Received Atom entry is not a CMIS entry!");
            }

            LockTypeLinks();
            ITypeDefinition result = null;
            try
            {
                // clean up cache
                RemoveTypeLinks(repositoryId, entry.Id);

                // walk through the entry
                foreach (AtomElement element in entry.Elements)
                {
                    if (element.Object is AtomLink)
                    {
                        AddTypeLink(repositoryId, entry.Id, (AtomLink)element.Object);
                    }
                    else if (element.Object is ITypeDefinition)
                    {
                        result = (ITypeDefinition)element.Object;
                    }
                }
            }
            finally
            {
                UnlockTypeLinks();
            }

            return result;
        }

        /// <summary>
        /// Retrieves the ACL of an object.
        /// </summary>
        public IAcl GetAclInternal(string repositoryId, string objectId, Boolean onlyBasicPermissions,
                ExtensionsData extension)
        {

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelAcl, BindingConstants.MediaTypeAcl);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelAcl, BindingConstants.MediaTypeAcl);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamOnlyBasicPermissions, onlyBasicPermissions);

            // read and parse
            IResponse resp = Read(url);
            AtomAcl acl = Parse<AtomAcl>(resp.Stream);

            return acl.Acl;
        }

        /// <summary>
        /// Updates the ACL of an object.
        /// </summary>
        protected AtomAcl UpdateAcl(string repositoryId, string objectId, IAcl acl, AclPropagation aclPropagation)
        {

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelAcl, BindingConstants.MediaTypeAcl);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelAcl, BindingConstants.MediaTypeAcl);
            }

            UrlBuilder aclUrl = new UrlBuilder(link);
            aclUrl.AddParameter(BindingConstants.ParamAclPropagation, aclPropagation);

            CmisVersion cmisVersion = GetCmisVersion(repositoryId);

            // update
            IResponse resp = Put(aclUrl, new AtomPubHttpContent(BindingConstants.MediaTypeAcl, (stream) =>
            {
                using (XmlWriter writer = XmlUtils.createWriter(stream))
                {
                    XmlUtils.StartXmlDocument(writer);
                    XmlConverter.writeAcl(writer, cmisVersion, true, acl);
                    XmlUtils.EndXmlDocument(writer);
                }
            }));

            // parse new entry
            return Parse<AtomAcl>(resp.Stream);
        }

    }

    internal class RepositoryService : AbstractAtomPubService, IRepositoryService
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

            throw new CmisObjectNotFoundException("Repository '" + repositoryId + "' not found!");
        }

        public ITypeDefinitionList GetTypeChildren(string repositoryId, string typeId, bool? includePropertyDefinitions,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            TypeDefinitionList result = new TypeDefinitionList();

            // find the link
            string link = null;
            if (typeId == null)
            {
                link = loadCollection(repositoryId, BindingConstants.CollectionTypes);
            }
            else
            {
                link = loadTypeLink(repositoryId, typeId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
            }

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or type!");
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamPropertyDefinitions, includePropertyDefinitions);
            url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
            url.AddParameter(BindingConstants.ParamSkipCount, skipCount);

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // handle top level
            foreach (AtomElement element in feed.Elements)
            {
                if (element.Object is AtomLink)
                {
                    if (isNextLink(element))
                    {
                        result.HasMoreItems = true;
                    }
                }
                else if (isInt(NAME_NUM_ITEMS, element))
                {
                    result.NumItems = (BigInteger)element.Object;
                }
            }

            result.List = new List<ITypeDefinition>(feed.Entries.Count);

            // get the children
            if (feed.Entries.Count > 0)
            {
                foreach (AtomEntry entry in feed.Entries)
                {
                    ITypeDefinition child = null;

                    LockTypeLinks();
                    try
                    {
                        // walk through the entry
                        foreach (AtomElement element in entry.Elements)
                        {
                            if (element.Object is AtomLink)
                            {
                                AddTypeLink(repositoryId, entry.Id, (AtomLink)element.Object);
                            }
                            else if (element.Object is ITypeDefinition)
                            {
                                child = (ITypeDefinition)element.Object;
                            }
                        }
                    }
                    finally
                    {
                        UnlockTypeLinks();
                    }

                    if (child != null)
                    {
                        result.List.Add(child);
                    }
                }
            }

            return result;
        }

        public IList<ITypeDefinitionContainer> GetTypeDescendants(string repositoryId, string typeId, BigInteger? depth,
            bool? includePropertyDefinitions, IExtensionsData extension)
        {
            IList<ITypeDefinitionContainer> result = new List<ITypeDefinitionContainer>();

            // find the link
            string link = null;
            if (typeId == null)
            {
                link = LoadRepositoryLink(repositoryId, BindingConstants.RepRelTypeDesc);
            }
            else
            {
                link = loadTypeLink(repositoryId, typeId, BindingConstants.RelDown, BindingConstants.MediaTypeDecendants);
            }

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or type!");
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamDepth, depth);
            url.AddParameter(BindingConstants.ParamPropertyDefinitions, includePropertyDefinitions);

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // process tree
            AddTypeDescendantsLevel(repositoryId, feed, result);

            return result;
        }

        /// <summary>
        /// Adds type descendants level recursively.
        /// </summary>
        private void AddTypeDescendantsLevel(string repositoryId, AtomFeed feed, IList<ITypeDefinitionContainer> containerList)
        {
            if (feed == null || feed.Entries.Count == 0)
            {
                return;
            }

            // walk through the feed
            foreach (AtomEntry entry in feed.Entries)
            {
                TypeDefinitionContainer childContainer = null;
                IList<ITypeDefinitionContainer> childContainerList = new List<ITypeDefinitionContainer>();

                // walk through the entry
                LockTypeLinks();
                try
                {
                    foreach (AtomElement element in entry.Elements)
                    {
                        if (element.Object is AtomLink)
                        {
                            AddTypeLink(repositoryId, entry.Id, (AtomLink)element.Object);
                        }
                        else if (element.Object is ITypeDefinition)
                        {
                            childContainer = new TypeDefinitionContainer() { TypeDefinition = (ITypeDefinition)element.Object };
                        }
                        else if (element.Object is AtomFeed)
                        {
                            AddTypeDescendantsLevel(repositoryId, (AtomFeed)element.Object, childContainerList);
                        }
                    }
                }
                finally
                {
                    UnlockTypeLinks();
                }

                if (childContainer != null)
                {
                    childContainer.Children = childContainerList;
                    containerList.Add(childContainer);
                }
            }
        }

        public ITypeDefinition GetTypeDefinition(string repositoryId, string typeId, IExtensionsData extension)
        {
            return GetTypeDefinitionInternal(repositoryId, typeId);
        }

        public ITypeDefinition CreateType(string repositoryId, ITypeDefinition type, IExtensionsData extension)
        {
            if (type == null)
            {
                throw new CmisInvalidArgumentException("Type definition must be set!");
            }

            string parentId = type.ParentTypeId;
            if (parentId == null)
            {
                throw new CmisInvalidArgumentException("Type definition has no parent type id!");
            }

            // find the link
            string link = loadTypeLink(repositoryId, parentId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or parent type!");
            }

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(type, GetCmisVersion(repositoryId));

            // post the new type definition
            IResponse resp = Post(new UrlBuilder(link), new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));

            // parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // we expect a CMIS entry
            if (entry.Id == null)
            {
                throw new CmisConnectionException("Received Atom entry is not a CMIS entry!");
            }

            LockTypeLinks();
            ITypeDefinition result = null;
            try
            {
                // clean up cache
                RemoveTypeLinks(repositoryId, entry.Id);

                // walk through the entry
                foreach (AtomElement element in entry.Elements)
                {
                    if (element.Object is AtomLink)
                    {
                        AddTypeLink(repositoryId, entry.Id, (AtomLink)element.Object);
                    }
                    else if (element.Object is ITypeDefinition)
                    {
                        result = (ITypeDefinition)element.Object;
                    }
                }
            }
            finally
            {
                UnlockTypeLinks();
            }

            return result;
        }

        public ITypeDefinition UpdateType(string repositoryId, ITypeDefinition type, IExtensionsData extension)
        {
            if (type == null)
            {
                throw new CmisInvalidArgumentException("Type definition must be set!");
            }

            string typeId = type.Id;
            if (typeId == null)
            {
                throw new CmisInvalidArgumentException("Type definition has no type ID!");
            }

            // find the link
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(BindingConstants.ParamId, typeId);

            string link = LoadTemplateLink(repositoryId, BindingConstants.TemplateTypeById, parameters);
            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or type!");
            }

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(type, GetCmisVersion(repositoryId));

            // post the new type definition
            IResponse resp = Put(new UrlBuilder(link), new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
                {
                    entryWriter.Write(stream);
                }));

            // parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // we expect a CMIS entry
            if (entry.Id == null)
            {
                throw new CmisConnectionException("Received Atom entry is not a CMIS entry!");
            }

            LockTypeLinks();
            ITypeDefinition result = null;
            try
            {
                // clean up cache
                RemoveTypeLinks(repositoryId, entry.Id);

                // walk through the entry
                foreach (AtomElement element in entry.Elements)
                {
                    if (element.Object is AtomLink)
                    {
                        AddTypeLink(repositoryId, entry.Id, (AtomLink)element.Object);
                    }
                    else if (element.Object is ITypeDefinition)
                    {
                        result = (ITypeDefinition)element.Object;
                    }
                }
            }
            finally
            {
                UnlockTypeLinks();
            }

            return result;
        }

        public void DeleteType(string repositoryId, string typeId, IExtensionsData extension)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(BindingConstants.ParamId, typeId);

            string link = LoadTemplateLink(repositoryId, BindingConstants.TemplateTypeById, parameters);
            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository!");
            }

            Delete(new UrlBuilder(link));
        }
    }

    internal class NavigationService : AbstractAtomPubService, INavigationService
    {
        public NavigationService(BindingSession session)
        {
            Session = session;
        }

        public IObjectInFolderList GetChildren(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            ObjectInFolderList result = new ObjectInFolderList();

            // find the link
            string link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

            if (link == null)
            {
                ThrowLinkException(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamOrderBy, orderBy);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPathSegment, includePathSegment);
            url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
            url.AddParameter(BindingConstants.ParamSkipCount, skipCount);

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // handle top level
            foreach (AtomElement element in feed.Elements)
            {
                if (element.Object is AtomLink)
                {
                    if (isNextLink(element))
                    {
                        result.HasMoreItems = true;
                    }
                }
                else if (isInt(NAME_NUM_ITEMS, element))
                {
                    result.NumItems = (BigInteger)element.Object;
                }
            }

            // get the children
            if (feed.Entries.Count > 0)
            {
                result.Objects = new List<IObjectInFolderData>(feed.Entries.Count);

                foreach (AtomEntry entry in feed.Entries)
                {
                    ObjectInFolderData child = null;
                    string pathSegment = null;

                    LockLinks();
                    try
                    {
                        // clean up cache
                        RemoveLinks(repositoryId, entry.Id);

                        // walk through the entry
                        foreach (AtomElement element in entry.Elements)
                        {
                            if (element.Object is AtomLink)
                            {
                                AddLink(repositoryId, entry.Id, (AtomLink)element.Object);
                            }
                            else if (isStr(NAME_PATH_SEGMENT, element))
                            {
                                pathSegment = (string)element.Object;
                            }
                            else if (element.Object is IObjectData)
                            {
                                child = new ObjectInFolderData();
                                child.Object = (IObjectData)element.Object;
                            }
                        }
                    }
                    finally
                    {
                        UnlockLinks();
                    }

                    if (child != null)
                    {
                        child.PathSegment = pathSegment;
                        result.Objects.Add(child);
                    }
                }
            }

            return result;
        }

        public IList<IObjectInFolderContainer> GetDescendants(string repositoryId, string folderId, BigInteger? depth, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension)
        {
            return null;
        }

        public IList<IObjectInFolderContainer> GetFolderTree(string repositoryId, string folderId, BigInteger? depth, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension)
        {
            return null;
        }

        public IList<IObjectParentData> GetObjectParents(string repositoryId, string objectId, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includeRelativePathSegment, IExtensionsData extension)
        {
            return null;
        }

        public IObjectData GetFolderParent(string repositoryId, string folderId, string filter, ExtensionsData extension)
        {
            return null;
        }

        public IObjectList GetCheckedOutDocs(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            return null;
        }
    }

    internal class ObjectService : AbstractAtomPubService, IObjectService
    {
        public ObjectService(BindingSession session)
        {
            Session = session;
        }

        public string CreateDocument(string repositoryId, IProperties properties, string folderId, IContentStream contentStream,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            return null;
        }

        public string CreateDocumentFromSource(string repositoryId, string sourceId, IProperties properties, string folderId,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("createDocumentFromSource is not supported by the AtomPub binding!");
        }

        public string CreateFolder(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            return null;
        }

        public string CreateRelationship(string repositoryId, IProperties properties, IList<string> policies, IAcl addAces,
            IAcl removeAces, IExtensionsData extension)
        {
            return null;
        }

        public string CreatePolicy(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            return null;
        }

        public string CreateItem(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            return null;
        }

        public IAllowableActions GetAllowableActions(string repositoryId, string objectId, IExtensionsData extension)
        {
            return null;
        }

        public IProperties GetProperties(string repositoryId, string objectId, string filter, IExtensionsData extension)
        {
            IObjectData objectData = GetObjectInternal(repositoryId, IdentifierType.ID, objectId, ReturnVersion.This, filter,
                    false, IncludeRelationships.None, "cmis:none", false, false, extension);

            return objectData.Properties;
        }

        public IList<IRenditionData> GetRenditions(string repositoryId, string objectId, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            return null;
        }

        public IObjectData GetObject(string repositoryId, string objectId, string filter, bool? includeAllowableActions,
            IncludeRelationships? includeRelationships, string renditionFilter, bool? includePolicyIds,
            bool? includeAcl, IExtensionsData extension)
        {
            return GetObjectInternal(repositoryId, IdentifierType.ID, objectId, ReturnVersion.This, filter,
                    includeAllowableActions, includeRelationships, renditionFilter, includePolicyIds, includeAcl, extension);
        }

        public IObjectData GetObjectByPath(string repositoryId, string path, string filter, bool? includeAllowableActions,
            IncludeRelationships? includeRelationships, string renditionFilter, bool? includePolicyIds, bool? includeAcl,
            IExtensionsData extension)
        {
            return GetObjectInternal(repositoryId, IdentifierType.PATH, path, ReturnVersion.This, filter,
                    includeAllowableActions, includeRelationships, renditionFilter, includePolicyIds, includeAcl, extension);
        }

        public IContentStream GetContentStream(string repositoryId, string objectId, string streamId, BigInteger? offset, BigInteger? length,
            IExtensionsData extension)
        {
            return null;
        }

        public void UpdateProperties(string repositoryId, ref string objectId, ref string changeToken, IProperties properties,
            IExtensionsData extension)
        {

        }

        public IList<IBulkUpdateObjectIdAndChangeToken> BulkUpdateProperties(string repositoryId,
                IList<IBulkUpdateObjectIdAndChangeToken> objectIdAndChangeToken, IProperties properties,
                IList<string> addSecondaryTypeIds, IList<string> removeSecondaryTypeIds, IExtensionsData extension)
        {
            return null;
        }

        public void MoveObject(string repositoryId, ref string objectId, string targetFolderId, string sourceFolderId,
            IExtensionsData extension)
        {

        }

        public void DeleteObject(string repositoryId, string objectId, bool? allVersions, IExtensionsData extension)
        {

        }

        public IFailedToDeleteData DeleteTree(string repositoryId, string folderId, bool? allVersions, UnfileObject? unfileObjects,
            bool? continueOnFailure, ExtensionsData extension)
        {
            return null;
        }

        public void SetContentStream(string repositoryId, ref string objectId, bool? overwriteFlag, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension)
        {

        }

        public void DeleteContentStream(string repositoryId, ref string objectId, ref string changeToken, IExtensionsData extension)
        {

        }

        public void AppendContentStream(string repositoryId, ref string objectId, bool? isLastChunk, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension)
        {

        }
    }

    internal class VersioningService : AbstractAtomPubService, IVersioningService
    {
        public VersioningService(BindingSession session)
        {
            Session = session;
        }

        public void CheckOut(string repositoryId, ref string objectId, IExtensionsData extension, out bool? contentCopied)
        {
            contentCopied = false;
        }

        public void CancelCheckOut(string repositoryId, string objectId, IExtensionsData extension)
        {

        }

        public void CheckIn(string repositoryId, ref string objectId, bool? major, IProperties properties,
            IContentStream contentStream, string checkinComment, IList<string> policies, IAcl addAces, IAcl removeAces,
            IExtensionsData extension)
        {

        }

        public IObjectData GetObjectOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool major,
            string filter, bool? includeAllowableActions, IncludeRelationships? includeRelationships,
            string renditionFilter, bool? includePolicyIds, bool? includeAcl, IExtensionsData extension)
        {
            return null;
        }

        public IProperties GetPropertiesOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool major,
            string filter, IExtensionsData extension)
        {
            return null;
        }

        public IList<IObjectData> GetAllVersions(string repositoryId, string objectId, string versionSeriesId, string filter,
            bool? includeAllowableActions, IExtensionsData extension)
        {
            return null;
        }
    }

    internal class DiscoveryService : AbstractAtomPubService, IDiscoveryService
    {
        public DiscoveryService(BindingSession session)
        {
            Session = session;
        }

        public IObjectList Query(string repositoryId, string statement, bool? searchAllVersions,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            return null;
        }

        public IObjectList GetContentChanges(string repositoryId, ref string changeLogToken, bool? includeProperties,
            string filter, bool? includePolicyIds, bool? includeAcl, BigInteger? maxItems, IExtensionsData extension)
        {
            return null;
        }
    }

    internal class MultiFilingService : AbstractAtomPubService, IMultiFilingService
    {
        public MultiFilingService(BindingSession session)
        {
            Session = session;
        }

        public void AddObjectToFolder(string repositoryId, string objectId, string folderId, bool? allVersions, IExtensionsData extension)
        {

        }

        public void RemoveObjectFromFolder(string repositoryId, string objectId, string folderId, IExtensionsData extension)
        {

        }
    }

    internal class RelationshipService : AbstractAtomPubService, IRelationshipService
    {
        public RelationshipService(BindingSession session)
        {
            Session = session;
        }

        public IObjectList GetObjectRelationships(string repositoryId, string objectId, bool? includeSubRelationshipTypes,
    RelationshipDirection? relationshipDirection, string typeId, string filter, bool? includeAllowableActions,
    BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            return null;
        }
    }

    internal class PolicyService : AbstractAtomPubService, IPolicyService
    {
        public PolicyService(BindingSession session)
        {
            Session = session;
        }

        public void ApplyPolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension)
        {

        }

        public void RemovePolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension)
        {

        }

        public IList<IObjectData> GetAppliedPolicies(string repositoryId, string objectId, string filter, IExtensionsData extension)
        {
            return null;
        }
    }

    internal class AclService : AbstractAtomPubService, IAclService
    {
        public AclService(BindingSession session)
        {
            Session = session;
        }

        public IAcl GetAcl(string repositoryId, string objectId, bool? onlyBasicPermissions, IExtensionsData extension)
        {
            return null;
        }

        public IAcl ApplyAcl(string repositoryId, string objectId, IAcl addAces, IAcl removeAces, AclPropagation? aclPropagation,
            IExtensionsData extension)
        {
            return null;
        }
    }
}
