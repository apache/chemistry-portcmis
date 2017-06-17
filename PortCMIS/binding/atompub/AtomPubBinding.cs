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

        protected const string NameCollection = "collection";
        protected const string NameUriTemplate = "uritemplate";
        protected const string NamePathSegment = "pathSegment";
        protected const string NameRelativePathSegment = "relativePathSegment";
        protected const string NameNumItems = "numItems";

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
        /// Gets a link from the cache if it is there or Loads it into the cache if
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
        /// Gets the content link from the cache if it is there or Loads it into the
        /// cache if it is not there.
        /// </summary>

        public string LoadContentLink(string repositoryId, string id)
        {
            return LoadLink(repositoryId, id, AtomPubParser.LinkRelContent, null);
        }

        /// <summary>
        /// Gets a rendition content link from the cache if it is there or Loads it
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
        /// Gets a link from the cache if it is there or Loads it into the cache if
        /// it is not there.
        /// </summary>
        protected string LoadTypeLink(string repositoryId, string typeId, string rel, string type)
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
        /// Gets a collection from the cache if it is there or Loads it into the
        /// cache if it is not there.
        /// </summary>
        protected string LoadCollection(string repositoryId, string collection)
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
        /// Gets a repository link from the cache if it is there or Loads it into the
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
        /// Gets a template link from the cache if it is there or Loads it into the
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

            int textStart = begin + "<!--exception-->".Length;
            return errorContent.Substring(textStart, end - textStart);
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

            int textStart = begin + "<!--message-->".Length;
            return errorContent.Substring(textStart, end - textStart);
        }

        // ---- helpers ----

        protected bool Matches(string name, AtomElement element)
        {
            return name == element.LocalName;
        }

        protected bool IsStr(string name, AtomElement element)
        {
            return Matches(name, element) && (element.Object is string);
        }

        protected bool IsInt(string name, AtomElement element)
        {
            return Matches(name, element) && (element.Object is BigInteger);
        }

        protected bool IsNextLink(AtomElement element)
        {
            return BindingConstants.RelNext == ((AtomLink)element.Object).Rel;
        }

        /// <summary>
        /// Creates a CMIS object with properties and policy IDs.
        /// </summary>
        protected ObjectData CreateObject(IProperties properties, string changeToken, IList<string> policies)
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
        protected IObjectData CreateIdObject(string objectId)
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
        protected void PostAndConsume(UrlBuilder url, HttpContent content)
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
        protected IResponse Put(UrlBuilder url, IDictionary<string, string> headers, HttpContent content)
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
                    if (Matches(NameCollection, element))
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
                    else if (Matches(NameUriTemplate, element))
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
        public IAcl GetAclInternal(string repositoryId, string objectId, bool? onlyBasicPermissions,
                IExtensionsData extension)
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
        protected AtomAcl UpdateAcl(string repositoryId, string objectId, IAcl acl, AclPropagation? aclPropagation)
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
                using (XmlWriter writer = XmlUtils.CreateWriter(stream))
                {
                    XmlUtils.StartXmlDocument(writer);
                    XmlConverter.WriteAcl(writer, cmisVersion, true, acl);
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
                link = LoadCollection(repositoryId, BindingConstants.CollectionTypes);
            }
            else
            {
                link = LoadTypeLink(repositoryId, typeId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
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
                    if (IsNextLink(element))
                    {
                        result.HasMoreItems = true;
                    }
                }
                else if (IsInt(NameNumItems, element))
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
                link = LoadTypeLink(repositoryId, typeId, BindingConstants.RelDown, BindingConstants.MediaTypeDecendants);
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
            string link = LoadTypeLink(repositoryId, parentId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

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
                    if (IsNextLink(element))
                    {
                        result.HasMoreItems = true;
                    }
                }
                else if (IsInt(NameNumItems, element))
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
                            else if (IsStr(NamePathSegment, element))
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
            IList<IObjectInFolderContainer> result = new List<IObjectInFolderContainer>();

            // find the link
            string link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeDecendants);

            if (link == null)
            {
                ThrowLinkException(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeEntry);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamDepth, depth);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPathSegment, includePathSegment);

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // process tree
            AddDescendantsLevel(repositoryId, feed, result);

            return result;
        }

        public IList<IObjectInFolderContainer> GetFolderTree(string repositoryId, string folderId, BigInteger? depth, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension)
        {
            IList<IObjectInFolderContainer> result = new List<IObjectInFolderContainer>();

            // find the link
            string link = LoadLink(repositoryId, folderId, BindingConstants.RelFolderTree, BindingConstants.MediaTypeDecendants);

            if (link == null)
            {
                ThrowLinkException(repositoryId, folderId, BindingConstants.RelFolderTree, BindingConstants.MediaTypeEntry);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamDepth, depth);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamPathSegment, includePathSegment);

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // process tree
            AddDescendantsLevel(repositoryId, feed, result);

            return result;
        }

        /// <summary>
        /// Adds descendants level recursively.
        /// </summary>
        private void AddDescendantsLevel(string repositoryId, AtomFeed feed, IList<IObjectInFolderContainer> containerList)
        {
            if (feed == null || feed.Entries.Count == 0)
            {
                return;
            }

            // walk through the feed
            foreach (AtomEntry entry in feed.Entries)
            {
                ObjectInFolderData objectInFolder = null;
                string pathSegment = null;
                IList<IObjectInFolderContainer> childContainerList = new List<IObjectInFolderContainer>();

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
                        else if (element.Object is IObjectData)
                        {
                            objectInFolder = new ObjectInFolderData() { Object = (IObjectData)element.Object };
                        }
                        else if (IsStr(NamePathSegment, element))
                        {
                            pathSegment = (string)element.Object;
                        }
                        else if (element.Object is AtomFeed)
                        {
                            AddDescendantsLevel(repositoryId, (AtomFeed)element.Object, childContainerList);
                        }
                    }
                }
                finally
                {
                    UnlockLinks();
                }

                if (objectInFolder != null)
                {
                    objectInFolder.PathSegment = pathSegment;
                    ObjectInFolderContainer childContainer = new ObjectInFolderContainer() { Object = objectInFolder };
                    childContainer.Children = childContainerList;
                    containerList.Add(childContainer);
                }
            }
        }

        public IList<IObjectParentData> GetObjectParents(string repositoryId, string objectId, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includeRelativePathSegment, IExtensionsData extension)
        {
            IList<IObjectParentData> result = new List<IObjectParentData>();

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelUp, BindingConstants.MediaTypeFeed);

            if (link == null)
            {
                // root and unfiled objects have no UP link
                return result;
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
            url.AddParameter(BindingConstants.ParamRelativePathSegment, includeRelativePathSegment);

            // read and parse
            IResponse resp = Read(url);

            AtomBase atomBase = Parse<AtomBase>(resp.Stream);

            if (atomBase is AtomFeed)
            {
                // it's a feed
                AtomFeed feed = (AtomFeed)atomBase;

                // walk through the feed
                foreach (AtomEntry entry in feed.Entries)
                {
                    IObjectParentData objectParent = ProcessParentEntry(entry, repositoryId);

                    if (objectParent != null)
                    {
                        result.Add(objectParent);
                    }
                }
            }
            else if (atomBase is AtomEntry)
            {
                // it's an entry
                AtomEntry entry = (AtomEntry)atomBase;

                IObjectParentData objectParent = ProcessParentEntry(entry, repositoryId);

                if (objectParent != null)
                {
                    result.Add(objectParent);
                }
            }

            return result;
        }

        private ObjectParentData ProcessParentEntry(AtomEntry entry, string repositoryId)
        {
            ObjectParentData result = null;
            string relativePathSegment = null;

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
                    else if (element.Object is ObjectData)
                    {
                        result = new ObjectParentData() { Object = (ObjectData)element.Object };
                    }
                    else if (IsStr(NameRelativePathSegment, element))
                    {
                        relativePathSegment = (string)element.Object;
                    }
                }
            }
            finally
            {
                UnlockLinks();
            }

            if (result != null)
            {
                result.RelativePathSegment = relativePathSegment;
            }

            return result;
        }

        public IObjectData GetFolderParent(string repositoryId, string folderId, string filter, IExtensionsData extension)
        {
            IObjectData result = null;

            // find the link
            string link = LoadLink(repositoryId, folderId, BindingConstants.RelUp, BindingConstants.MediaTypeEntry);

            if (link == null)
            {
                ThrowLinkException(repositoryId, folderId, BindingConstants.RelUp, BindingConstants.MediaTypeEntry);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamFilter, filter);

            // read
            IResponse resp = Read(url);

            AtomBase atomBase = Parse<AtomBase>(resp.Stream);

            // get the entry
            AtomEntry entry = null;
            if (atomBase is AtomFeed)
            {
                AtomFeed feed = (AtomFeed)atomBase;
                if (feed.Entries.Count == 0)
                {
                    throw new CmisRuntimeException("Parent feed is empty!");
                }
                entry = feed.Entries[0];
            }
            else if (atomBase is AtomEntry)
            {
                entry = (AtomEntry)atomBase;
            }
            else
            {
                throw new CmisRuntimeException("Unexpected document!");
            }

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

        public IObjectList GetCheckedOutDocs(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension)
        {
            ObjectList result = new ObjectList();

            // find the link
            string link = LoadCollection(repositoryId, BindingConstants.CollectionCheckedout);

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or checkedout collection not supported!");
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamFolderId, folderId);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamOrderBy, orderBy);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
            url.AddParameter(BindingConstants.ParamRelationships, includeRelationships);
            url.AddParameter(BindingConstants.ParamRenditionfilter, renditionFilter);
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
                    if (IsNextLink(element))
                    {
                        result.HasMoreItems = true;
                    }
                }
                else if (IsInt(NameNumItems, element))
                {
                    result.NumItems = (BigInteger)element.Object;
                }
            }

            // get the documents
            if (feed.Entries.Count > 0)
            {
                result.Objects = new List<IObjectData>(feed.Entries.Count);

                foreach (AtomEntry entry in feed.Entries)
                {
                    IObjectData child = null;

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
                            else if (element.Object is IObjectData)
                            {
                                child = (IObjectData)element.Object;
                            }
                        }
                    }
                    finally
                    {
                        UnlockLinks();
                    }

                    if (child != null)
                    {
                        result.Objects.Add(child);
                    }
                }
            }

            return result;

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
            CheckCreateProperties(properties);

            // find the link
            string link = null;

            if (folderId == null)
            {
                // Creation of unfiled objects via AtomPub is not defined in the
                // CMIS 1.0 specification. This implementation follow the CMIS
                // 1.1 draft and POSTs the document to the Unfiled collection.
                link = LoadCollection(repositoryId, BindingConstants.CollectionUnfiled);

                if (link == null)
                {
                    throw new CmisObjectNotFoundException("Unknown repository or unfiling not supported!");
                }
            }
            else
            {
                link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

                if (link == null)
                {
                    ThrowLinkException(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
                }
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamVersioningState, versioningState);

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateObject(properties, null, policies), GetCmisVersion(repositoryId), contentStream);

            // post the new document object
            IResponse resp = Post(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));

            // parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // handle ACL modifications
            HandleAclModifications(repositoryId, entry, addAces, removeAces);

            return entry.Id;
        }

        public string CreateDocumentFromSource(string repositoryId, string sourceId, IProperties properties, string folderId,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            throw new CmisNotSupportedException("createDocumentFromSource is not supported by the AtomPub binding!");
        }

        public string CreateFolder(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            CheckCreateProperties(properties);

            // find the link
            string link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

            if (link == null)
            {
                ThrowLinkException(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
            }

            UrlBuilder url = new UrlBuilder(link);

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateObject(properties, null, policies), GetCmisVersion(repositoryId));

            // post the new folder object
            IResponse resp = Post(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));

            // Parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // handle ACL modifications
            HandleAclModifications(repositoryId, entry, addAces, removeAces);

            return entry.Id;
        }

        public string CreateRelationship(string repositoryId, IProperties properties, IList<string> policies, IAcl addAces,
            IAcl removeAces, IExtensionsData extension)
        {
            CheckCreateProperties(properties);

            // find source id
            IPropertyData sourceIdProperty = properties[PropertyIds.SourceId];
            if (sourceIdProperty == null)
            {
                throw new CmisInvalidArgumentException("Source Id is not set!");
            }

            string sourceId = sourceIdProperty.FirstValue as string;
            if (sourceId == null)
            {
                throw new CmisInvalidArgumentException("Source Id is not set!");
            }

            // find the link
            string link = LoadLink(repositoryId, sourceId, BindingConstants.RelRelationships, BindingConstants.MediaTypeFeed);

            if (link == null)
            {
                ThrowLinkException(repositoryId, sourceId, BindingConstants.RelRelationships, BindingConstants.MediaTypeFeed);
            }

            UrlBuilder url = new UrlBuilder(link);

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateObject(properties, null, policies), GetCmisVersion(repositoryId));

            // post the new relationship object
            IResponse resp = Post(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));

            // Parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // handle ACL modifications
            HandleAclModifications(repositoryId, entry, addAces, removeAces);

            return entry.Id;
        }

        public string CreatePolicy(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            CheckCreateProperties(properties);

            // find the link
            string link = null;

            if (folderId == null)
            {
                // Creation of unfiled objects via AtomPub is not defined in the
                // CMIS 1.0 specification. This implementation follow the CMIS
                // 1.1 draft and POSTs the policy to the Unfiled collection.
                link = LoadCollection(repositoryId, BindingConstants.CollectionUnfiled);

                if (link == null)
                {
                    throw new CmisObjectNotFoundException("Unknown repository or unfiling not supported!");
                }
            }
            else
            {
                link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

                if (link == null)
                {
                    ThrowLinkException(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
                }
            }

            UrlBuilder url = new UrlBuilder(link);

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateObject(properties, null, policies), GetCmisVersion(repositoryId));

            // post the new policy object
            IResponse resp = Post(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));

            // parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // handle ACL modifications
            HandleAclModifications(repositoryId, entry, addAces, removeAces);

            return entry.Id;
        }

        public string CreateItem(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension)
        {
            CheckCreateProperties(properties);

            // find the link
            string link = null;

            if (folderId == null)
            {
                link = LoadCollection(repositoryId, BindingConstants.CollectionUnfiled);

                if (link == null)
                {
                    throw new CmisObjectNotFoundException("Unknown repository or unfiling not supported!");
                }
            }
            else
            {
                link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

                if (link == null)
                {
                    ThrowLinkException(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
                }
            }

            UrlBuilder url = new UrlBuilder(link);

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateObject(properties, null, policies), GetCmisVersion(repositoryId));

            // post the new item object
            IResponse resp = Post(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));

            // parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // handle ACL modifications
            HandleAclModifications(repositoryId, entry, addAces, removeAces);

            return entry.Id;
        }

        private void CheckCreateProperties(IProperties properties)
        {
            if (properties == null)
            {
                throw new CmisInvalidArgumentException("Properties must be set!");
            }

            if (properties[PropertyIds.ObjectTypeId] == null)
            {
                throw new CmisInvalidArgumentException("Property " + PropertyIds.ObjectTypeId + " must be set!");
            }

            if (properties[PropertyIds.ObjectId] != null)
            {
                throw new CmisInvalidArgumentException("Property " + PropertyIds.ObjectId + " must not be set!");
            }
        }

        private void HandleAclModifications(string repositoryId, AtomEntry entry, IAcl addAces, IAcl removeAces)
        {
            if (!IsAclMergeRequired(addAces, removeAces))
            {
                return;
            }

            IAcl originalAces = GetAclInternal(repositoryId, entry.Id, false, null);

            if (originalAces != null)
            {
                // merge and update ACL
                IAcl newACL = MergeAcls(originalAces, addAces, removeAces);
                if (newACL != null)
                {
                    UpdateAcl(repositoryId, entry.Id, newACL, null);
                }
            }
        }

        public IAllowableActions GetAllowableActions(string repositoryId, string objectId, IExtensionsData extension)
        {
            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelAllowableActions, BindingConstants.MediaTypeAllowableAction);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelAllowableActions, BindingConstants.MediaTypeAllowableAction);
            }

            UrlBuilder url = new UrlBuilder(link);

            // read and parse
            IResponse resp = Read(url);
            AtomAllowableActions allowableActions = Parse<AtomAllowableActions>(resp.Stream);

            return allowableActions.AllowableActions;
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
            IObjectData objectData = GetObjectInternal(repositoryId, IdentifierType.ID, objectId, ReturnVersion.This,
                PropertyIds.ObjectId, false, IncludeRelationships.None, renditionFilter, false, false, extension);

            IList<IRenditionData> result = objectData.Renditions;
            if (result == null)
            {
                result = new List<IRenditionData>();
            }

            return result;
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
            // find the link
            string link = null;
            if (streamId != null)
            {
                // use the alternate link per spec
                link = LoadLink(repositoryId, objectId, BindingConstants.RelAlternate, streamId);
                if (link != null)
                {
                    streamId = null; // we have a full URL now
                }
            }
            if (link == null)
            {
                link = LoadLink(repositoryId, objectId, AtomPubParser.LinkRelContent, null);
            }

            if (link == null)
            {
                throw new CmisConstraintException("No content stream");
            }

            UrlBuilder url = new UrlBuilder(link);
            // using the content URL and adding a streamId param is not
            // spec-compliant
            url.AddParameter(BindingConstants.ParamStreamId, streamId);

            // get the content
            IResponse resp = Session.GetHttpInvoker().InvokeGET(url, Session, (long?)offset, (long?)length);

            // check response code
            if (resp.StatusCode != 200 && resp.StatusCode != 206)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            ContentStream result;
            if (resp.StatusCode == 206)
            {
                result = new PartialContentStream();
            }
            else
            {
                result = new ContentStream();
            }

            result.Length = resp.ContentLength;
            result.MimeType = resp.ContentType;
            result.Stream = resp.Stream;

            return result;
        }

        public void UpdateProperties(string repositoryId, ref string objectId, ref string changeToken, IProperties properties,
            IExtensionsData extension)
        {
            // we need an object id
            if (string.IsNullOrEmpty(objectId))
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelSelf, BindingConstants.MediaTypeEntry);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelSelf, BindingConstants.MediaTypeEntry);
            }

            UrlBuilder url = new UrlBuilder(link);
            if (changeToken != null)
            {
                if (Session.GetValue(SessionParameter.OmitChangeTokens, false))
                {
                    changeToken = null;
                }
                else
                {
                    // not required by the CMIS specification -> keep for backwards
                    // compatibility with older OpenCMIS servers
                    url.AddParameter(BindingConstants.ParamChangeToken, changeToken);
                }
            }

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateObject(properties, changeToken, null), GetCmisVersion(repositoryId));

            // update
            IResponse resp = Put(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
                {
                    entryWriter.Write(stream);
                }));

            // parse new entry
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // we expect a CMIS entry
            if (entry.Id == null)
            {
                throw new CmisConnectionException("Received Atom entry is not a CMIS entry!");
            }

            // set object id
            objectId = entry.Id;

            changeToken = null; // just in case

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
                    else if (element.Object is IObjectData)
                    {
                        // extract new change toke
                        IObjectData objectData = (IObjectData)element.Object;

                        if (objectData.Properties != null)
                        {
                            IPropertyData changeTokenStr = objectData.Properties[PropertyIds.ChangeToken];
                            if (changeTokenStr != null)
                            {
                                changeToken = changeTokenStr.FirstValue as string;
                            }
                        }
                    }
                }
            }
            finally
            {
                UnlockLinks();
            }
        }

        public IList<IBulkUpdateObjectIdAndChangeToken> BulkUpdateProperties(string repositoryId,
                IList<IBulkUpdateObjectIdAndChangeToken> objectIdAndChangeToken, IProperties properties,
                IList<string> addSecondaryTypeIds, IList<string> removeSecondaryTypeIds, IExtensionsData extension)
        {
            // find link
            string link = LoadCollection(repositoryId, BindingConstants.CollectionBulkUpdate);

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or bulk update properties is not supported!");
            }

            // set up writer
            BulkUpdate bulkUpdate = new BulkUpdate();
            bulkUpdate.ObjectIdAndChangeToken = objectIdAndChangeToken;
            bulkUpdate.Properties = properties;
            bulkUpdate.AddSecondaryTypeIds = addSecondaryTypeIds;
            bulkUpdate.RemoveSecondaryTypeIds = removeSecondaryTypeIds;

            AtomEntryWriter entryWriter = new AtomEntryWriter(bulkUpdate);

            // post update
            IResponse resp = Post(new UrlBuilder(link), new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
                {
                    entryWriter.Write(stream);
                }));

            AtomFeed feed = Parse<AtomFeed>(resp.Stream);
            List<IBulkUpdateObjectIdAndChangeToken> result = new List<IBulkUpdateObjectIdAndChangeToken>(feed.Entries.Count);

            // get the results
            if (feed.Entries.Count > 0)
            {

                foreach (AtomEntry entry in feed.Entries)
                {
                    // walk through the entry
                    // we are not interested in the links this time because they
                    // could belong to a new document version
                    foreach (AtomElement element in entry.Elements)
                    {
                        if (element.Object is IObjectData)
                        {
                            IObjectData objectData = (IObjectData)element.Object;
                            string id = objectData.Id;
                            if (id != null)
                            {
                                string changeToken = null;
                                IPropertyData changeTokenStr = objectData.Properties[PropertyIds.ChangeToken];
                                if (changeTokenStr != null)
                                {
                                    changeToken = changeTokenStr.FirstValue as string;
                                }

                                result.Add(new BulkUpdateObjectIdAndChangeToken() { Id = id, ChangeToken = changeToken });
                            }
                        }
                    }
                }
            }

            return result;
        }

        public void MoveObject(string repositoryId, ref string objectId, string targetFolderId, string sourceFolderId,
            IExtensionsData extension)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            if (string.IsNullOrEmpty(targetFolderId) || string.IsNullOrEmpty(sourceFolderId))
            {
                throw new CmisInvalidArgumentException("Source and target folder must be set!");
            }

            // find the link
            string link = LoadLink(repositoryId, targetFolderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

            if (link == null)
            {
                ThrowLinkException(repositoryId, targetFolderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamSourceFolderId, sourceFolderId);

            // workaround for SharePoint 2010 - see CMIS-839
            bool objectIdOnMove = Session.GetValue(SessionParameter.IncludeObjectIdUrlParamOnMove, false);
            if (objectIdOnMove)
            {
                url.AddParameter("objectId", objectId);
                url.AddParameter("targetFolderId", targetFolderId);
            }

            // set up object and writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateIdObject(objectId), GetCmisVersion(repositoryId));

            // post move request
            IResponse resp = Post(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));


            // workaround for SharePoint 2010 - see CMIS-839
            if (objectIdOnMove)
            {
                // SharePoint doesn't return a new object ID
                // we assume that the object ID hasn't changed
                return;
            }

            // parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            objectId = entry.Id;
        }

        public void DeleteObject(string repositoryId, string objectId, bool? allVersions, IExtensionsData extension)
        {
            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelSelf, BindingConstants.MediaTypeEntry);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelSelf, BindingConstants.MediaTypeEntry);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamAllVersions, allVersions);

            Delete(url);
        }

        public IFailedToDeleteData DeleteTree(string repositoryId, string folderId, bool? allVersions, UnfileObject? unfileObjects,
            bool? continueOnFailure, IExtensionsData extension)
        {
            // find the down links
            string link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, null);
            string childrenLink = null;

            if (link != null)
            {
                // found only a children link, but no descendants link
                // -> try folder tree link
                childrenLink = link;
                link = null;
            }
            else
            {
                // found no or two down links
                // -> get only the descendants link
                link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeDecendants);
            }

            if (link == null)
            {
                link = LoadLink(repositoryId, folderId, BindingConstants.RelFolderTree, BindingConstants.MediaTypeDecendants);
            }

            if (link == null)
            {
                link = LoadLink(repositoryId, folderId, BindingConstants.RelFolderTree, BindingConstants.MediaTypeFeed);
            }

            if (link == null)
            {
                link = childrenLink;
            }

            if (link == null)
            {
                ThrowLinkException(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeDecendants);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamAllVersions, allVersions);
            url.AddParameter(BindingConstants.ParamUnfileObjects, unfileObjects);
            url.AddParameter(BindingConstants.ParamContinueOnFailure, continueOnFailure);

            // make the call
            IResponse resp = Session.GetHttpInvoker().InvokeDELETE(url, Session);

            // check response code
            if (resp.StatusCode == 200 || resp.StatusCode == 202 || resp.StatusCode == 204)
            {
                return new FailedToDeleteData();
            }

            // If the server returned an internal server error, get the remaining
            // children of the folder. We only retrieve the first level, since
            // getDescendants() is not supported by all repositories.
            if (resp.StatusCode == 500)
            {
                link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

                if (link != null)
                {
                    url = new UrlBuilder(link);
                    // we only want the object ids
                    url.AddParameter(BindingConstants.ParamFilter, "cmis:objectId");
                    url.AddParameter(BindingConstants.ParamAllowableActions, false);
                    url.AddParameter(BindingConstants.ParamRelationships, IncludeRelationships.None);
                    url.AddParameter(BindingConstants.ParamRenditionfilter, "cmis:none");
                    url.AddParameter(BindingConstants.ParamPathSegment, false);
                    // 1000 children should be enough to indicate a problem
                    url.AddParameter(BindingConstants.ParamMaxItems, 1000);
                    url.AddParameter(BindingConstants.ParamSkipCount, 0);

                    // read and parse
                    resp = Read(url);
                    AtomFeed feed = Parse<AtomFeed>(resp.Stream);

                    // prepare result
                    FailedToDeleteData result = new FailedToDeleteData();
                    IList<string> ids = new List<string>();
                    result.Ids = ids;

                    // get the children ids
                    foreach (AtomEntry entry in feed.Entries)
                    {
                        ids.Add(entry.Id);
                    }

                    return result;
                }
            }

            throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
        }

        public void SetContentStream(string repositoryId, ref string objectId, bool? overwriteFlag, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension)
        {
            SetOrAppendContent(repositoryId, ref objectId, overwriteFlag, ref changeToken, contentStream, true, false, extension);
        }

        public void DeleteContentStream(string repositoryId, ref string objectId, ref string changeToken, IExtensionsData extension)
        {
            // we need an object id
            if (objectId == null)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelEditMedia, null);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelEditMedia, null);
            }

            UrlBuilder url = new UrlBuilder(link);
            if (changeToken != null && !Session.GetValue(SessionParameter.OmitChangeTokens, false))
            {
                url.AddParameter(BindingConstants.ParamChangeToken, changeToken);
            }

            Delete(url);

            objectId = null;
            changeToken = null;
        }

        public void AppendContentStream(string repositoryId, ref string objectId, bool? isLastChunk, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension)
        {
            SetOrAppendContent(repositoryId, ref objectId, null, ref changeToken, contentStream, isLastChunk, true, extension);
        }

        /// <summary>
        /// Sets or appends content.
        /// </summary>
        private void SetOrAppendContent(string repositoryId, ref string objectId, bool? overwriteFlag, ref string changeToken,
            IContentStream contentStream, bool? isLastChunk, bool append, IExtensionsData extension)
        {
            // we need an object id
            if (objectId == null)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // we need content
            if (contentStream == null || contentStream.Stream == null || contentStream.MimeType == null)
            {
                throw new CmisInvalidArgumentException("Content must be set!");
            }

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelEditMedia, null);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelEditMedia, null);
            }

            UrlBuilder url = new UrlBuilder(link);
            if (changeToken != null && !Session.GetValue(SessionParameter.OmitChangeTokens, false))
            {
                url.AddParameter(BindingConstants.ParamChangeToken, changeToken);
            }

            if (append)
            {
                url.AddParameter(BindingConstants.ParamAppend, true);
                url.AddParameter(BindingConstants.ParamIsLastChunk, isLastChunk);
            }
            else
            {
                url.AddParameter(BindingConstants.ParamOverwriteFlag, overwriteFlag);
            }

            Stream content = contentStream.Stream;

            // Content-Disposition header for the filename
            IDictionary<string, string> headers = null;
            if (contentStream.FileName != null)
            {
                headers = new Dictionary<string, string>();
                headers.Add(MimeHelper.ContentDisposition,
                    MimeHelper.EncodeContentDisposition(MimeHelper.DispositionAttachment, contentStream.FileName));
            }

            // send content
            IResponse resp = Put(url, headers, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                content.CopyTo(stream);
            }));

            // check response code further
            if (resp.StatusCode != 200 && resp.StatusCode != 201 && resp.StatusCode != 204)
            {
                throw ConvertStatusCode(resp.StatusCode, resp.Message, resp.ErrorContent, null);
            }

            if (resp.StatusCode == 201)
            {
                // unset the object ID if a new resource has been created
                // (if the resource has been updated (200 and 204), the object ID
                // hasn't changed)
                objectId = null;
            }

            changeToken = null;
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
            if (string.IsNullOrEmpty(objectId))
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // find the link
            string link = LoadCollection(repositoryId, BindingConstants.CollectionCheckedout);

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or checkedout collection not supported!");
            }

            UrlBuilder url = new UrlBuilder(link);

            // workaround for SharePoint 2010 - see CMIS-362
            if (Session.GetValue(SessionParameter.IncludeObjectIdUrlParamOnCheckout, false))
            {
                url.AddParameter("objectId", objectId);
            }

            // set up object and writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateIdObject(objectId), GetCmisVersion(repositoryId));

            // post check out request
            IResponse resp = Post(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));

            // parse the response
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            objectId = entry.Id;

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
                }
            }
            finally
            {
                UnlockLinks();
            }

            contentCopied = null;
        }

        public void CancelCheckOut(string repositoryId, string objectId, IExtensionsData extension)
        {
            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelSelf, BindingConstants.MediaTypeEntry);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelSelf, BindingConstants.MediaTypeEntry);
            }

            // prefer working copy link if available
            // (workaround for non-compliant repositories)
            string wcLink = GetLink(repositoryId, objectId, BindingConstants.RelWorkingCopy, BindingConstants.MediaTypeEntry);
            if (wcLink != null)
            {
                link = wcLink;
            }

            Delete(new UrlBuilder(link));
        }

        public void CheckIn(string repositoryId, ref string objectId, bool? major, IProperties properties,
            IContentStream contentStream, string checkinComment, IList<string> policies, IAcl addAces, IAcl removeAces,
            IExtensionsData extension)
        {
            // we need an object id
            if (string.IsNullOrEmpty(objectId))
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelSelf, BindingConstants.MediaTypeEntry);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelSelf, BindingConstants.MediaTypeEntry);
            }

            // prefer working copy link if available
            // (workaround for non-compliant repositories)
            string wcLink = GetLink(repositoryId, objectId, BindingConstants.RelWorkingCopy, BindingConstants.MediaTypeEntry);
            if (wcLink != null)
            {
                link = wcLink;
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamCheckinComment, checkinComment);
            url.AddParameter(BindingConstants.ParamMajor, major);
            url.AddParameter(BindingConstants.ParamCheckIn, "true");

            // workaround for SharePoint - check in without property change
            if (Session.GetValue(SessionParameter.AddNameOnCheckIn, false))
            {
                if (properties == null || properties.PropertyList.Count == 0)
                {
                    properties = new Properties();

                    try
                    {
                        string name = null;

                        // fetch the current name
                        IObjectData obj = GetObjectInternal(repositoryId, IdentifierType.ID, objectId, ReturnVersion.This,
                            "cmis:objectId,cmis:name", false, IncludeRelationships.None, "cmis:none", false, false, null);

                        if (obj != null && obj.Properties != null && obj.Properties[PropertyIds.Name] != null)
                        {
                            name = obj.Properties[PropertyIds.Name].FirstValue as string;
                        }

                        if (name == null)
                        {
                            throw new CmisRuntimeException("Could not determine the name of the PWC!");
                        }

                        // set the document name to the same value - silly, but
                        // SharePoint requires that at least one property value has
                        // to be changed and the name is the only reliable property
                        PropertyData newNameProp = new PropertyData(PropertyType.String);
                        newNameProp.Id = PropertyIds.Name;
                        newNameProp.AddValue(name);
                        ((Properties)properties).AddProperty(newNameProp);
                    }
                    catch (CmisBaseException e)
                    {
                        throw new CmisRuntimeException("Could not determine the name of the PWC: " + e.ToString(), e);
                    }
                }
            }

            // set up writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateObject(properties, null, policies), GetCmisVersion(repositoryId), contentStream);

            // update

            IResponse resp = Put(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));

            // parse new entry
            AtomEntry entry = Parse<AtomEntry>(resp.Stream);

            // we expect a CMIS entry
            if (entry.Id == null)
            {
                throw new CmisConnectionException("Received Atom entry is not a CMIS entry!");
            }

            // set object id
            objectId = entry.Id;

            Acl originalAces = null;

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
                    else if (element.Object is IObjectData)
                    {
                        // extract current ACL
                        IObjectData objectData = (IObjectData)element.Object;
                        if (objectData.Acl != null)
                        {
                            originalAces = new Acl() { Aces = objectData.Acl.Aces };
                            originalAces.IsExact = objectData.IsExactAcl;
                        }
                    }
                }
            }
            finally
            {
                UnlockLinks();
            }

            // handle ACL modifications
            if (originalAces != null && IsAclMergeRequired(addAces, removeAces))
            {
                // merge and update ACL
                IAcl newACL = MergeAcls(originalAces, addAces, removeAces);
                if (newACL != null)
                {
                    UpdateAcl(repositoryId, entry.Id, newACL, null);
                }
            }
        }

        public IObjectData GetObjectOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool? major,
            string filter, bool? includeAllowableActions, IncludeRelationships? includeRelationships,
            string renditionFilter, bool? includePolicyIds, bool? includeAcl, IExtensionsData extension)
        {
            ReturnVersion returnVersion = (major == true ? ReturnVersion.LatestMajor : ReturnVersion.Latest);

            // workaround for SharePoint - use the version series ID instead of the object ID
            if (Session.GetValue(SessionParameter.LatestVersionWithVersionSeriesId, false))
            {
                if (versionSeriesId != null)
                {
                    objectId = versionSeriesId;
                }
                else
                {
                    IObjectData obj = GetObjectInternal(repositoryId, IdentifierType.ID, objectId, null,
                            PropertyIds.ObjectId + "," + PropertyIds.VersionSeriesId, false,
                            IncludeRelationships.None, "cmis:none", false, false, extension);

                    if (obj.Properties != null)
                    {
                        IPropertyData versionSeriesProp = obj.Properties[PropertyIds.VersionSeriesId];
                        if (versionSeriesProp != null && versionSeriesProp.FirstValue is string)
                        {
                            objectId = (string)versionSeriesProp.FirstValue;
                        }
                    }
                }
            }

            return GetObjectInternal(repositoryId, IdentifierType.ID, objectId, returnVersion, filter,
                    includeAllowableActions, includeRelationships, renditionFilter, includePolicyIds, includeAcl, extension);
        }

        public IProperties GetPropertiesOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool? major,
            string filter, IExtensionsData extension)
        {
            ReturnVersion returnVersion = (major == true ? ReturnVersion.LatestMajor : ReturnVersion.Latest);

            IObjectData objectData = GetObjectInternal(repositoryId, IdentifierType.ID, objectId, returnVersion, filter,
                    false, IncludeRelationships.None, "cmis:none", false, false, extension);

            return objectData.Properties;
        }

        public IList<IObjectData> GetAllVersions(string repositoryId, string objectId, string versionSeriesId, string filter,
            bool? includeAllowableActions, IExtensionsData extension)
        {
            IList<IObjectData> result = new List<IObjectData>();

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelVersionHistory, BindingConstants.MediaTypeFeed);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelVersionHistory, BindingConstants.MediaTypeFeed);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // get the versions
            if (feed.Entries.Count > 0)
            {
                foreach (AtomEntry entry in feed.Entries)
                {
                    IObjectData version = null;

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
                            else if (element.Object is IObjectData)
                            {
                                version = (IObjectData)element.Object;
                            }
                        }
                    }
                    finally
                    {
                        UnlockLinks();
                    }

                    if (version != null)
                    {
                        result.Add(version);
                    }
                }
            }

            return result;
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
            ObjectList result = new ObjectList();

            // find the link
            string link = LoadCollection(repositoryId, BindingConstants.CollectionQuery);

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or query not supported!");
            }

            UrlBuilder url = new UrlBuilder(link);

            // compile query request
            QueryType query = new QueryType();
            query.Statement = statement;
            query.SearchAllVersions = searchAllVersions;
            query.IncludeAllowableActions = includeAllowableActions;
            query.IncludeRelationships = includeRelationships;
            query.RenditionFilter = renditionFilter;
            query.MaxItems = maxItems;
            query.SkipCount = skipCount;

            CmisVersion cmisVersion = GetCmisVersion(repositoryId);

            // post the query and parse results
            IResponse resp = Post(url, new AtomPubHttpContent(BindingConstants.MediaTypeQuery, (stream) =>
            {
                using (XmlWriter writer = XmlUtils.CreateWriter(stream))
                {
                    XmlUtils.StartXmlDocument(writer);
                    XmlConverter.WriteQuery(writer, cmisVersion, query);
                    XmlUtils.EndXmlDocument(writer);
                }
            }));

            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // handle top level
            foreach (AtomElement element in feed.Elements)
            {
                if (element.Object is AtomLink)
                {
                    if (IsNextLink(element))
                    {
                        result.HasMoreItems = true;
                    }
                }
                else if (IsInt(NameNumItems, element))
                {
                    result.NumItems = (BigInteger)element.Object;
                }
            }

            // get the result set
            if (feed.Entries.Count > 0)
            {
                result.Objects = new List<IObjectData>(feed.Entries.Count);

                foreach (AtomEntry entry in feed.Entries)
                {
                    IObjectData hit = null;

                    // walk through the entry
                    foreach (AtomElement element in entry.Elements)
                    {
                        if (element.Object is IObjectData)
                        {
                            hit = (IObjectData)element.Object;
                        }
                    }

                    if (hit != null)
                    {
                        result.Objects.Add(hit);
                    }
                }
            }

            return result;
        }

        public IObjectList GetContentChanges(string repositoryId, ref string changeLogToken, bool? includeProperties,
            string filter, bool? includePolicyIds, bool? includeAcl, BigInteger? maxItems, IExtensionsData extension)
        {
            ObjectList result = new ObjectList();

            // find the link
            string link = null;
            UrlBuilder url = null;

            // if the application didn't provide a link to next Atom feed
            if (link == null)
            {
                link = LoadRepositoryLink(repositoryId, BindingConstants.RepRelChanges);
                if (link != null)
                {
                    url = new UrlBuilder(link);
                    url.AddParameter(BindingConstants.ParamChangeLogToken, changeLogToken);
                    url.AddParameter(BindingConstants.ParamProperties, includeProperties);
                    url.AddParameter(BindingConstants.ParamFilter, filter);
                    url.AddParameter(BindingConstants.ParamPolicyIds, includePolicyIds);
                    url.AddParameter(BindingConstants.ParamAcl, includeAcl);
                    url.AddParameter(BindingConstants.ParamMaxItems, maxItems);
                }
            }

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or content changes not supported!");
            }

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);
            string lastChangeLogToken = null;

            // handle top level
            string nextLink = null;
            foreach (AtomElement element in feed.Elements)
            {
                if (element.Object is AtomLink)
                {
                    if (IsNextLink(element))
                    {
                        result.HasMoreItems = true;
                        nextLink = ((AtomLink)element.Object).Href;
                    }
                }
                else if (IsInt(NameNumItems, element))
                {
                    result.NumItems = (BigInteger)element.Object;
                }
                else if (IsStr("changeLogToken", element))
                {
                    lastChangeLogToken = (String)element.Object;
                }
            }

            // get the changes
            if (feed.Entries.Count > 0)
            {
                result.Objects = new List<IObjectData>(feed.Entries.Count);

                foreach (AtomEntry entry in feed.Entries)
                {
                    IObjectData hit = null;

                    // walk through the entry
                    foreach (AtomElement element in entry.Elements)
                    {
                        if (element.Object is IObjectData)
                        {
                            hit = (IObjectData)element.Object;
                        }
                    }

                    if (hit != null)
                    {
                        result.Objects.Add(hit);
                    }
                }
            }

            changeLogToken = lastChangeLogToken;

            return result;
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
            if (objectId == null)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // find the link
            string link = LoadLink(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);

            if (link == null)
            {
                ThrowLinkException(repositoryId, folderId, BindingConstants.RelDown, BindingConstants.MediaTypeChildren);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamAllVersions, allVersions);

            // set up object and writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateIdObject(objectId), GetCmisVersion(repositoryId));

            // post addObjectToFolder request
            PostAndConsume(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));
        }

        public void RemoveObjectFromFolder(string repositoryId, string objectId, string folderId, IExtensionsData extension)
        {
            if (objectId == null)
            {
                throw new CmisInvalidArgumentException("Object ID must be set!");
            }

            // find the link
            string link = LoadCollection(repositoryId, BindingConstants.CollectionUnfiled);

            if (link == null)
            {
                throw new CmisObjectNotFoundException("Unknown repository or unfiling not supported!");
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamRemoveFrom, folderId);

            // set up object and writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateIdObject(objectId), GetCmisVersion(repositoryId));

            // post removeObjectFromFolder request
            PostAndConsume(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));
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
            ObjectList result = new ObjectList();

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelRelationships, BindingConstants.MediaTypeFeed);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelRelationships, BindingConstants.MediaTypeFeed);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamSubRelationshipTypes, includeSubRelationshipTypes);
            url.AddParameter(BindingConstants.ParamRelationshipDirection, relationshipDirection);
            url.AddParameter(BindingConstants.ParamTypeId, typeId);
            url.AddParameter(BindingConstants.ParamFilter, filter);
            url.AddParameter(BindingConstants.ParamAllowableActions, includeAllowableActions);
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
                    if (IsNextLink(element))
                    {
                        result.HasMoreItems = true;
                    }
                }
                else if (IsInt(NameNumItems, element))
                {
                    result.NumItems = (BigInteger)element.Object;
                }
            }

            // get the children
            if (feed.Entries.Count > 0)
            {
                result.Objects = new List<IObjectData>(feed.Entries.Count);

                foreach (AtomEntry entry in feed.Entries)
                {
                    IObjectData relationship = null;

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
                            else if (element.Object is IObjectData)
                            {
                                relationship = (IObjectData)element.Object;
                            }
                        }
                    }
                    finally
                    {
                        UnlockLinks();
                    }

                    if (relationship != null)
                    {
                        result.Objects.Add(relationship);
                    }
                }
            }

            return result;
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
            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelPolicies, BindingConstants.MediaTypeFeed);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelPolicies, BindingConstants.MediaTypeFeed);
            }

            UrlBuilder url = new UrlBuilder(link);

            // set up object and writer
            AtomEntryWriter entryWriter = new AtomEntryWriter(CreateIdObject(policyId), GetCmisVersion(repositoryId));

            // post applyPolicy request
            PostAndConsume(url, new AtomPubHttpContent(BindingConstants.MediaTypeEntry, (stream) =>
            {
                entryWriter.Write(stream);
            }));
        }

        public void RemovePolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension)
        {
            // we need a policy id
            if (policyId == null)
            {
                throw new CmisInvalidArgumentException("Policy id must be set!");
            }

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelPolicies, BindingConstants.MediaTypeFeed);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelPolicies, BindingConstants.MediaTypeFeed);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamFilter, PropertyIds.ObjectId);

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // find the policy
            string policyLink = null;
            bool found = false;

            if (feed.Entries.Count > 0)
            {
                foreach (AtomEntry entry in feed.Entries)
                {
                    // walk through the entry
                    foreach (AtomElement element in entry.Elements)
                    {
                        if (element.Object is AtomLink)
                        {
                            AtomLink atomLink = (AtomLink)element.Object;
                            if (BindingConstants.RelSelf == atomLink.Rel)
                            {
                                policyLink = atomLink.Href;
                            }
                        }
                        else if (element.Object is IObjectData)
                        {
                            string id = ((IObjectData)element.Object).Id;
                            if (policyId == id)
                            {
                                found = true;
                            }
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }
            }

            // if found, delete it
            if (found && policyLink != null)
            {
                Delete(new UrlBuilder(policyLink));
            }
        }

        public IList<IObjectData> GetAppliedPolicies(string repositoryId, string objectId, string filter, IExtensionsData extension)
        {
            IList<IObjectData> result = new List<IObjectData>();

            // find the link
            string link = LoadLink(repositoryId, objectId, BindingConstants.RelPolicies, BindingConstants.MediaTypeFeed);

            if (link == null)
            {
                ThrowLinkException(repositoryId, objectId, BindingConstants.RelPolicies, BindingConstants.MediaTypeFeed);
            }

            UrlBuilder url = new UrlBuilder(link);
            url.AddParameter(BindingConstants.ParamFilter, filter);

            // read and parse
            IResponse resp = Read(url);
            AtomFeed feed = Parse<AtomFeed>(resp.Stream);

            // get the policies
            if (feed.Entries.Count > 0)
            {
                foreach (AtomEntry entry in feed.Entries)
                {
                    IObjectData policy = null;

                    // walk through the entry
                    foreach (AtomElement element in entry.Elements)
                    {
                        if (element.Object is IObjectData)
                        {
                            policy = (IObjectData)element.Object;
                        }
                    }

                    if (policy != null)
                    {
                        result.Add(policy);
                    }
                }
            }

            return result;
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
            return GetAclInternal(repositoryId, objectId, onlyBasicPermissions, extension);
        }

        public IAcl ApplyAcl(string repositoryId, string objectId, IAcl addAces, IAcl removeAces, AclPropagation? aclPropagation,
            IExtensionsData extension)
        {
            // fetch the current ACL
            IAcl originalAces = GetAcl(repositoryId, objectId, false, null);

            // if no changes required, just return the ACL
            if (!IsAclMergeRequired(addAces, removeAces))
            {
                return originalAces;
            }

            // merge ACLs
            IAcl newACL = MergeAcls(originalAces, addAces, removeAces);

            // update ACL
            AtomAcl acl = UpdateAcl(repositoryId, objectId, newACL, aclPropagation);
            IAcl result = acl.Acl;

            return result;
        }
    }
}
