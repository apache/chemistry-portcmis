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

using PortCMIS.Binding;
using PortCMIS.Binding.Services;
using PortCMIS.Data;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace PortCMIS.Client.Impl
{
    /// <summary>
    /// Session factory implementation.
    /// </summary>
    public class SessionFactory : ISessionFactory
    {
        /// <summary>
        /// This is a factory.
        /// </summary>
        private SessionFactory()
        {
        }

        /// <summary>
        /// Creates a new factory object.
        /// </summary>
        /// <returns>a factory object</returns>
        public static SessionFactory NewInstance()
        {
            return new SessionFactory();
        }

        /// <inheritdoc/>
        public ISession CreateSession(IDictionary<string, string> parameters)
        {
            return CreateSession(parameters, null, null, null);
        }

        /// <inheritdoc/>
        public ISession CreateSession(IDictionary<string, string> parameters, IObjectFactory objectFactory, IAuthenticationProvider authenticationProvider, ICache cache)
        {
            Session session = new Session(parameters, objectFactory, authenticationProvider, cache);
            session.Connect();

            return session;
        }

        /// <inheritdoc/>
        public IList<IRepository> GetRepositories(IDictionary<string, string> parameters)
        {
            return GetRepositories(parameters, null, null, null);
        }

        /// <summary>
        /// Gets all repository available at the specified endpoint.
        /// </summary>
        /// <param name="parameters">the session parameters</param>
        /// <param name="objectFactory">Object factory.</param>
        /// <param name="authenticationProvider">Authentication provider.</param>
        /// <param name="cache">Client object cache.</param>
        /// <returns>a list of all available repositories</returns>
        /// <seealso cref="PortCMIS.Client.SessionParameter"/>
        public IList<IRepository> GetRepositories(IDictionary<string, string> parameters, IObjectFactory objectFactory, IAuthenticationProvider authenticationProvider, ICache cache)
        {
            ICmisBinding binding = CmisBindingHelper.CreateBinding(parameters);

            IList<IRepositoryInfo> repositoryInfos = binding.GetRepositoryService().GetRepositoryInfos(null);

            IList<IRepository> result = new List<IRepository>();
            foreach (IRepositoryInfo data in repositoryInfos)
            {
                result.Add(new Repository(data, parameters, this, objectFactory, binding.GetAuthenticationProvider(), cache));
            }

            return result;
        }
    }

    /// <summary>
    /// Binding helper class.
    /// </summary>
    internal class CmisBindingHelper
    {
        public static ICmisBinding CreateBinding(IDictionary<string, string> parameters)
        {
            return CreateBinding(parameters, null);
        }

        public static ICmisBinding CreateBinding(IDictionary<string, string> parameters, IAuthenticationProvider authenticationProvider)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (!parameters.ContainsKey(SessionParameter.BindingType))
            {
                parameters[SessionParameter.BindingType] = BindingType.Custom;
            }

            string bt = parameters[SessionParameter.BindingType];
            switch (bt)
            {
                case BindingType.AtomPub:
                    return CreateAtomPubBinding(parameters, authenticationProvider);
                case BindingType.WebServices:
                    return CreateWebServiceBinding(parameters, authenticationProvider);
                case BindingType.Browser:
                    return CreateBrowserBinding(parameters, authenticationProvider);
                case BindingType.Custom:
                    return CreateCustomBinding(parameters, authenticationProvider);
                default:
                    throw new CmisRuntimeException("Ambiguous session parameter: " + parameters);
            }
        }

        private static ICmisBinding CreateCustomBinding(IDictionary<string, string> parameters, IAuthenticationProvider authenticationProvider)
        {
            CmisBindingFactory factory = CmisBindingFactory.NewInstance();
            ICmisBinding binding = factory.CreateCmisBinding(parameters, authenticationProvider);

            return binding;
        }

        private static ICmisBinding CreateWebServiceBinding(IDictionary<string, string> parameters, IAuthenticationProvider authenticationProvider)
        {
            CmisBindingFactory factory = CmisBindingFactory.NewInstance();
            ICmisBinding binding = factory.CreateCmisWebServicesBinding(parameters, authenticationProvider);

            return binding;
        }

        private static ICmisBinding CreateAtomPubBinding(IDictionary<string, string> parameters, IAuthenticationProvider authenticationProvider)
        {
            CmisBindingFactory factory = CmisBindingFactory.NewInstance();
            ICmisBinding binding = factory.CreateCmisAtomPubBinding(parameters, authenticationProvider);

            return binding;
        }

        private static ICmisBinding CreateBrowserBinding(IDictionary<string, string> parameters, IAuthenticationProvider authenticationProvider)
        {
            CmisBindingFactory factory = CmisBindingFactory.NewInstance();
            ICmisBinding binding = factory.CreateCmisBrowserBinding(parameters, authenticationProvider);

            return binding;
        }
    }

    /// <summary>
    /// Repository implementation.
    /// </summary>
    public class Repository : RepositoryInfo, IRepository
    {
        private IDictionary<string, string> parameters;
        private SessionFactory sessionFactory;
        private IObjectFactory objectFactory;
        private IAuthenticationProvider authenticationProvider;
        private ICache cache;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">the low-level repository info object</param>
        /// <param name="parameters">the session parameters</param>
        /// <param name="sessionFactory">the session factory</param>
        /// <param name="objectFactory">the object factory (may be <c>null</c>)</param>
        /// <param name="authenticationProvider">the authentication provider (may be <c>null</c>)</param>
        /// <param name="cache">the cache (may be <c>null</c>)</param>
        public Repository(IRepositoryInfo info, IDictionary<string, string> parameters, SessionFactory sessionFactory, IObjectFactory objectFactory, IAuthenticationProvider authenticationProvider, ICache cache)
            : base(info)
        {
            this.parameters = new Dictionary<string, string>(parameters);
            this.parameters[SessionParameter.RepositoryId] = Id;

            this.sessionFactory = sessionFactory;
            this.objectFactory = objectFactory;
            this.authenticationProvider = authenticationProvider;
            this.cache = cache;
        }

        /// <inheritdoc/>
        public ISession CreateSession()
        {
            return sessionFactory.CreateSession(parameters, objectFactory, authenticationProvider, cache);
        }
    }

    /// <summary>
    /// Session implementation.
    /// </summary>
    public class Session : ISession
    {
        private static HashSet<Updatability> CreateUpdatability = new HashSet<Updatability>();
        private static HashSet<Updatability> CreateAndCheckoutUpdatability = new HashSet<Updatability>();
        static Session()
        {
            CreateUpdatability.Add(Updatability.OnCreate);
            CreateUpdatability.Add(Updatability.ReadWrite);
            CreateAndCheckoutUpdatability.Add(Updatability.OnCreate);
            CreateAndCheckoutUpdatability.Add(Updatability.ReadWrite);
            CreateAndCheckoutUpdatability.Add(Updatability.WhenCheckedOut);
        }

        /// <summary>
        /// Initial default operation context.
        /// </summary>
        protected static IOperationContext FallbackContext = new OperationContext(null, false, true, false, IncludeRelationships.None, null, true, null, true, 100);

        /// <summary>
        /// Session parameters
        /// </summary>
        protected IDictionary<string, string> parameters;
        private object sessionLock = new object();

        /// <value>
        /// Gets the low-level binding.
        /// </value>
        public ICmisBinding Binding { get; protected set; }

        /// <value>
        /// Gets the repository info.
        /// </value>
        public IRepositoryInfo RepositoryInfo { get; protected set; }

        /// <value>
        /// Gets the repository ID.
        /// </value>
        public string RepositoryId { get { return RepositoryInfo.Id; } }

        /// <inheritdoc/>
        public IObjectFactory ObjectFactory { get; protected set; }

        /// <summary>
        /// Authentication provider.
        /// </summary>
        protected IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Object and path cache.
        /// </summary>
        protected ICache Cache { get; set; }

        /// <summary>
        /// Indicating if the path cache should be used.
        /// </summary>
        protected bool cachePathOmit;

        private IOperationContext context = FallbackContext;

        /// <inheritdoc/>
        public IOperationContext DefaultContext
        {
            get
            {
                lock (sessionLock)
                {
                    return context;
                }
            }
            set
            {
                lock (sessionLock)
                {
                    context = (value == null ? FallbackContext : value);
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameters">the session parameters</param>
        /// <param name="objectFactory">an object factory, if available</param>
        /// <param name="authenticationProvider">an authentication provider, if available</param>
        /// <param name="cache">a cache, if available</param>
        public Session(IDictionary<string, string> parameters, IObjectFactory objectFactory, IAuthenticationProvider authenticationProvider, ICache cache)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            this.parameters = parameters;

            ObjectFactory = (objectFactory == null ? CreateObjectFactory() : objectFactory);
            AuthenticationProvider = authenticationProvider;
            Cache = (cache == null ? CreateCache() : cache);

            string cachePathOmitStr;
            if (parameters.TryGetValue(SessionParameter.CachePathOmit, out cachePathOmitStr))
            {
                cachePathOmit = cachePathOmitStr.ToLower() == "true";
            }
            else
            {
                cachePathOmit = false;
            }
        }

        /// <summary>
        /// Connects to the repository and fetches the repository info.
        /// </summary>
        public void Connect()
        {
            lock (sessionLock)
            {
                Binding = CmisBindingHelper.CreateBinding(parameters, AuthenticationProvider);

                string repositoryId;
                if (!parameters.TryGetValue(SessionParameter.RepositoryId, out repositoryId))
                {
                    throw new ArgumentException("Repository Id is not set!");
                }

                RepositoryInfo = Binding.GetRepositoryService().GetRepositoryInfo(repositoryId, null);
            }
        }

        /// <summary>
        /// Create the cache.
        /// </summary>
        /// <returns>the cache object</returns>
        protected ICache CreateCache()
        {
            try
            {
                string typeName;
                Type cacheType;

                if (parameters.TryGetValue(SessionParameter.CacheClass, out typeName))
                {
                    cacheType = Type.GetType(typeName);
                }
                else
                {
                    cacheType = typeof(CmisObjectCache);
                }

                ICache cacheObject = Activator.CreateInstance(cacheType) as ICache;
                if (cacheObject == null)
                {
                    throw new Exception("Class does not implement ICache!");
                }

                cacheObject.Initialize(this, parameters);

                return cacheObject;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Unable to create cache: " + e, e);
            }
        }

        /// <summary>
        /// Creates the object factory.
        /// </summary>
        /// <returns>the object factory</returns>
        protected IObjectFactory CreateObjectFactory()
        {
            try
            {
                string ofName;
                Type ofType;

                if (parameters.TryGetValue(SessionParameter.ObjectFactoryClass, out ofName))
                {
                    ofType = Type.GetType(ofName);
                }
                else
                {
                    ofType = typeof(ObjectFactory);
                }

                IObjectFactory ofObject = Activator.CreateInstance(ofType) as IObjectFactory;
                if (ofObject == null)
                {
                    throw new Exception("Class does not implement IObjectFactory!");
                }

                ofObject.Initialize(this, parameters);

                return ofObject;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Unable to create object factory: " + e, e);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (sessionLock)
            {
                Cache = CreateCache();
                Binding.ClearAllCaches();
            }
        }

        // session context

        /// <inheritdoc/>
        public IOperationContext CreateOperationContext()
        {
            return new OperationContext();
        }

        /// <inheritdoc/>
        public IOperationContext CreateOperationContext(HashSet<string> filter, bool includeAcls, bool includeAllowableActions, bool includePolicies,
            IncludeRelationships includeRelationships, HashSet<string> renditionFilter, bool includePathSegments, string orderBy,
            bool cacheEnabled, int maxItemsPerPage)
        {
            return new OperationContext(filter, includeAcls, includeAllowableActions, includePolicies, includeRelationships, renditionFilter,
                includePathSegments, orderBy, cacheEnabled, maxItemsPerPage);
        }

        /// <inheritdoc/>
        public IObjectId CreateObjectId(string id)
        {
            return new ObjectId(id);
        }

        // types

        /// <inheritdoc/>
        public IObjectType GetTypeDefinition(string typeId)
        {
            ITypeDefinition typeDefinition = Binding.GetRepositoryService().GetTypeDefinition(RepositoryId, typeId, null);
            return ObjectFactory.ConvertTypeDefinition(typeDefinition);
        }

        /// <inheritdoc/>
        public IItemEnumerable<IObjectType> GetTypeChildren(string typeId, bool includePropertyDefinitions)
        {
            IRepositoryService service = Binding.GetRepositoryService();

            PageFetcher<IObjectType>.FetchPage fetchPageDelegate = delegate(BigInteger maxNumItems, BigInteger skipCount)
            {
                // fetch the data
                ITypeDefinitionList tdl = service.GetTypeChildren(RepositoryId, typeId, includePropertyDefinitions, maxNumItems, skipCount, null);

                // convert type definitions
                int count = (tdl != null && tdl.List != null ? tdl.List.Count : 0);
                IList<IObjectType> page = new List<IObjectType>(count);
                if (count > 0)
                {
                    foreach (ITypeDefinition typeDefinition in tdl.List)
                    {
                        page.Add(ObjectFactory.ConvertTypeDefinition(typeDefinition));
                    }
                }

                return new PageFetcher<IObjectType>.Page<IObjectType>(page, tdl.NumItems, tdl.HasMoreItems);
            };

            return new CollectionEnumerable<IObjectType>(new PageFetcher<IObjectType>(DefaultContext.MaxItemsPerPage, fetchPageDelegate));
        }

        /// <inheritdoc/>
        public IList<ITree<IObjectType>> GetTypeDescendants(string typeId, int depth, bool includePropertyDefinitions)
        {
            IList<ITypeDefinitionContainer> descendants = Binding.GetRepositoryService().GetTypeDescendants(
            RepositoryId, typeId, depth, includePropertyDefinitions, null);

            return ConvertTypeDescendants(descendants);
        }

        private IList<ITree<IObjectType>> ConvertTypeDescendants(IList<ITypeDefinitionContainer> descendantsList)
        {
            if (descendantsList == null || descendantsList.Count == 0)
            {
                return null;
            }

            IList<ITree<IObjectType>> result = new List<ITree<IObjectType>>();

            foreach (ITypeDefinitionContainer container in descendantsList)
            {
                Tree<IObjectType> tree = new Tree<IObjectType>();
                tree.Item = ObjectFactory.ConvertTypeDefinition(container.TypeDefinition);
                tree.Children = ConvertTypeDescendants(container.Children);

                result.Add(tree);
            }

            return result;
        }

        /// <inheritdoc/>
        public IObjectType CreateType(ITypeDefinition type)
        {
            CheckCmisVersion();

            ITypeDefinition newType = Binding.GetRepositoryService().CreateType(RepositoryId, type, null);
            return ObjectFactory.ConvertTypeDefinition(newType);
        }

        /// <inheritdoc/>
        public IObjectType UpdateType(ITypeDefinition type)
        {
            CheckCmisVersion();

            ITypeDefinition updatedType = Binding.GetRepositoryService().UpdateType(RepositoryId, type, null);
            return ObjectFactory.ConvertTypeDefinition(updatedType);
        }

        /// <inheritdoc/>
        public void DeleteType(string typeId)
        {
            CheckCmisVersion();

            Binding.GetRepositoryService().DeleteType(RepositoryId, typeId, null);
        }


        // navigation

        /// <inheritdoc/>
        public IFolder GetRootFolder()
        {
            return GetRootFolder(DefaultContext);
        }

        /// <inheritdoc/>
        public IFolder GetRootFolder(IOperationContext context)
        {
            IFolder rootFolder = GetObject(RepositoryInfo.RootFolderId, context) as IFolder;
            if (rootFolder == null)
            {
                throw new CmisRuntimeException("Root folder object is not a folder!");
            }

            return rootFolder;
        }

        /// <inheritdoc/>
        public IItemEnumerable<IDocument> GetCheckedOutDocs()
        {
            return GetCheckedOutDocs(DefaultContext);
        }

        /// <inheritdoc/>
        public IItemEnumerable<IDocument> GetCheckedOutDocs(IOperationContext context)
        {
            INavigationService service = Binding.GetNavigationService();
            IOperationContext ctxt = new OperationContext(context);

            PageFetcher<IDocument>.FetchPage fetchPageDelegate = delegate(BigInteger maxNumItems, BigInteger skipCount)
            {
                // get all checked out documents
                IObjectList checkedOutDocs = service.GetCheckedOutDocs(RepositoryId, null, ctxt.FilterString, ctxt.OrderBy,
                    ctxt.IncludeAllowableActions, ctxt.IncludeRelationships, ctxt.RenditionFilterString, maxNumItems, skipCount, null);

                // convert objects
                IList<IDocument> page = new List<IDocument>();
                if (checkedOutDocs.Objects != null)
                {
                    foreach (IObjectData objectData in checkedOutDocs.Objects)
                    {
                        IDocument doc = ObjectFactory.ConvertObject(objectData, ctxt) as IDocument;
                        if (doc == null)
                        {
                            // should not happen...
                            continue;
                        }

                        page.Add(doc);
                    }
                }

                return new PageFetcher<IDocument>.Page<IDocument>(page, checkedOutDocs.NumItems, checkedOutDocs.HasMoreItems);
            };

            return new CollectionEnumerable<IDocument>(new PageFetcher<IDocument>(DefaultContext.MaxItemsPerPage, fetchPageDelegate));
        }

        /// <inheritdoc/>
        public ICmisObject GetObject(IObjectId objectId)
        {
            return GetObject(objectId, DefaultContext);
        }

        /// <inheritdoc/>
        public ICmisObject GetObject(IObjectId objectId, IOperationContext context)
        {
            if (objectId == null || objectId.Id == null)
            {
                throw new ArgumentException("Object Id must be set!", "objectId");
            }

            return GetObject(objectId.Id, context);
        }

        /// <inheritdoc/>
        public ICmisObject GetObject(string objectId)
        {
            return GetObject(objectId, DefaultContext);
        }

        /// <inheritdoc/>
        public ICmisObject GetObject(string objectId, IOperationContext context)
        {
            if (objectId == null)
            {
                throw new ArgumentException("Object Id must be set!", "objectId");
            }
            if (context == null)
            {
                throw new ArgumentException("Operation context must be set!", "context");
            }

            ICmisObject result = null;

            // ask the cache first
            if (context.CacheEnabled)
            {
                result = Cache.GetById(objectId, context.CacheKey);
                if (result != null)
                {
                    return result;
                }
            }

            // get the object
            IObjectData objectData = Binding.GetObjectService().GetObject(RepositoryId, objectId, context.FilterString,
                context.IncludeAllowableActions, context.IncludeRelationships, context.RenditionFilterString, context.IncludePolicies,
                context.IncludeAcls, null);

            result = ObjectFactory.ConvertObject(objectData, context);

            // put into cache
            if (context.CacheEnabled)
            {
                Cache.Put(result, context.CacheKey);
            }

            return result;
        }

        /// <inheritdoc/>
        public ICmisObject GetObjectByPath(string path)
        {
            return GetObjectByPath(path, DefaultContext);
        }

        /// <inheritdoc/>
        public ICmisObject GetObjectByPath(string path, IOperationContext context)
        {
            CheckPath(path);

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ICmisObject result = null;

            // ask the cache first
            if (context.CacheEnabled && !cachePathOmit)
            {
                result = Cache.GetByPath(path, context.CacheKey);
                if (result != null)
                {
                    return result;
                }
            }

            // get the object
            IObjectData objectData = Binding.GetObjectService().GetObjectByPath(RepositoryId, path, context.FilterString,
                context.IncludeAllowableActions, context.IncludeRelationships, context.RenditionFilterString, context.IncludePolicies,
                context.IncludeAcls, null);

            result = ObjectFactory.ConvertObject(objectData, context);

            // put into cache
            if (context.CacheEnabled)
            {
                Cache.PutPath(path, result, context.CacheKey);
            }

            return result;
        }

        /// <inheritdoc/>
        public ICmisObject GetObjectByPath(string parentPath, string name)
        {
            return GetObjectByPath(parentPath, name, DefaultContext);
        }

        /// <inheritdoc/>
        public ICmisObject GetObjectByPath(string parentPath, string name, IOperationContext context)
        {
            if (parentPath == null || parentPath.Length < 1)
            {
                throw new ArgumentException("Parent path must be set!", "parentPath");
            }
            if (parentPath[0] != '/')
            {
                throw new ArgumentException("Parent path must start with a '/'!", "parentPath");
            }
            if (name == null || name.Length < 1)
            {
                throw new ArgumentException("Name must be set!", "name");
            }

            StringBuilder path = new StringBuilder();
            path.Append(parentPath);
            if (!parentPath.EndsWith("/"))
            {
                path.Append('/');
            }
            path.Append(name);

            return GetObjectByPath(path.ToString(), context);
        }

        /// <inheritdoc/>
        public IDocument GetLatestDocumentVersion(string objectId)
        {
            return GetLatestDocumentVersion(objectId, DefaultContext);
        }

        /// <inheritdoc/>
        public IDocument GetLatestDocumentVersion(string objectId, IOperationContext context)
        {
            if (objectId == null)
            {
                throw new ArgumentNullException("objectId");
            }

            return GetLatestDocumentVersion(CreateObjectId(objectId), false, context);
        }

        /// <inheritdoc/>
        public IDocument GetLatestDocumentVersion(IObjectId objectId)
        {
            return GetLatestDocumentVersion(objectId, false, DefaultContext);
        }

        /// <inheritdoc/>
        public IDocument GetLatestDocumentVersion(IObjectId objectId, IOperationContext context)
        {
            return GetLatestDocumentVersion(objectId, false, context);
        }

        /// <inheritdoc/>
        public IDocument GetLatestDocumentVersion(IObjectId objectId, bool major, IOperationContext context)
        {
            if (objectId == null || objectId.Id == null)
            {
                throw new ArgumentNullException("objectId");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ICmisObject result = null;

            string versionSeriesId = null;

            // first attempt: if we got a Document object, try getting the version
            // series ID from it
            if (objectId is IDocument)
            {
                versionSeriesId = ((IDocument)objectId).VersionSeriesId;
            }

            // second attempt: if we have a Document object in the cache, retrieve
            // the version series ID form there
            if (versionSeriesId == null)
            {
                if (context.CacheEnabled)
                {
                    ICmisObject sourceDoc = Cache.GetById(objectId.Id, context.CacheKey);
                    if (sourceDoc is IDocument)
                    {
                        versionSeriesId = ((IDocument)sourceDoc).VersionSeriesId;
                    }
                }
            }

            // third attempt (Web Services only): get the version series ID from the
            // repository
            // (the AtomPub and Browser binding don't need the version series ID ->
            // avoid roundtrip)
            if (versionSeriesId == null)
            {
                string bindingType = Binding.BindingType;
                if (bindingType == BindingType.WebServices || bindingType == BindingType.Custom)
                {

                    // get the document to find the version series ID
                    IObjectData sourceObjectData = Binding.GetObjectService().GetObject(RepositoryId, objectId.Id,
                            PropertyIds.ObjectId + "," + PropertyIds.VersionSeriesId, false, IncludeRelationships.None,
                            "cmis:none", false, false, null);

                    if (sourceObjectData.Properties != null)
                    {
                        IPropertyData verionsSeriesIdProp = sourceObjectData.Properties[PropertyIds.VersionSeriesId];
                        if (verionsSeriesIdProp != null)
                        {
                            versionSeriesId = verionsSeriesIdProp.FirstValue as string;
                        }
                    }

                    // the Web Services binding needs the version series ID -> fail
                    if (versionSeriesId == null)
                    {
                        throw new ArgumentException("Object is not a document or not versionable!");
                    }
                }
            }

            // get the object
            IObjectData objectData = Binding.GetVersioningService().GetObjectOfLatestVersion(RepositoryId,
                    objectId.Id, versionSeriesId, major, context.FilterString,
                    context.IncludeAllowableActions, context.IncludeRelationships,
                    context.RenditionFilterString, context.IncludePolicies, context.IncludeAcls, null);

            result = ObjectFactory.ConvertObject(objectData, context);

            // put into cache
            if (context.CacheEnabled)
            {
                Cache.Put(result, context.CacheKey);
            }

            // check result
            if (!(result is IDocument))
            {
                throw new ArgumentException("Latest version is not a document!");
            }

            return result as IDocument;
        }

        /// <inheritdoc/>
        public bool Exists(IObjectId objectId)
        {
            if (objectId == null)
            {
                throw new ArgumentNullException("objectId");
            }
            return Exists(objectId.Id);
        }

        /// <inheritdoc/>
        public bool Exists(string objectId)
        {
            if (objectId == null)
            {
                throw new ArgumentNullException(objectId);
            }

            try
            {
                Binding.GetObjectService().GetObject(RepositoryId, objectId, "cmis:objectId", false,
                    IncludeRelationships.None, "cmis:none", false, false, null);
                return true;
            }
            catch (CmisObjectNotFoundException)
            {
                RemoveObjectFromCache(objectId);
                return false;
            }
        }

        /// <inheritdoc/>
        public bool ExistsPath(string path)
        {
            CheckPath(path);

            try
            {
                IObjectData obj = Binding.GetObjectService().GetObjectByPath(RepositoryId, path, "cmis:objectId",
                    false, IncludeRelationships.None, "cmis:none", false, false, null);

                string cacheObjectId = Cache.GetObjectIdByPath(path);
                if (cacheObjectId != obj.Id)
                {
                    Cache.RemovePath(path);
                }

                return true;
            }
            catch (CmisObjectNotFoundException)
            {
                Cache.RemovePath(path);
                return false;
            }
        }

        /// <inheritdoc/>
        public bool ExistsPath(string parentPath, string name)
        {
            if (parentPath == null || parentPath.Length < 1)
            {
                throw new ArgumentException("Parent path must be set!", "parentPath");
            }
            if (parentPath[0] != '/')
            {
                throw new ArgumentException("Parent path must start with a '/'!", "parentPath");
            }
            if (name == null || name.Length < 1)
            {
                throw new ArgumentException("Name must be set!", "name");
            }

            StringBuilder path = new StringBuilder(parentPath.Length + name.Length + 2);
            path.Append(parentPath);
            if (!parentPath.EndsWith("/"))
            {
                path.Append('/');
            }
            path.Append(name);

            return ExistsPath(path.ToString());
        }

        /// <inheritdoc/>
        public void RemoveObjectFromCache(IObjectId objectId)
        {
            if (objectId == null || objectId.Id == null)
            {
                return;
            }

            RemoveObjectFromCache(objectId.Id);
        }

        /// <inheritdoc/>
        public void RemoveObjectFromCache(string objectId)
        {
            Cache.Remove(objectId);
        }

        // discovery

        /// <inheritdoc/>
        public IItemEnumerable<IQueryResult> Query(string statement, bool searchAllVersions)
        {
            return Query(statement, searchAllVersions, DefaultContext);
        }

        /// <inheritdoc/>
        public IItemEnumerable<IQueryResult> Query(string statement, bool searchAllVersions, IOperationContext context)
        {
            IDiscoveryService service = Binding.GetDiscoveryService();
            IOperationContext ctxt = new OperationContext(context);

            PageFetcher<IQueryResult>.FetchPage fetchPageDelegate = delegate(BigInteger maxNumItems, BigInteger skipCount)
            {
                // fetch the data
                IObjectList resultList = service.Query(RepositoryId, statement, searchAllVersions, ctxt.IncludeAllowableActions,
                    ctxt.IncludeRelationships, ctxt.RenditionFilterString, maxNumItems, skipCount, null);

                // convert query results
                IList<IQueryResult> page = new List<IQueryResult>();
                if (resultList.Objects != null)
                {
                    foreach (IObjectData objectData in resultList.Objects)
                    {
                        if (objectData == null)
                        {
                            continue;
                        }

                        page.Add(ObjectFactory.ConvertQueryResult(objectData));
                    }
                }

                return new PageFetcher<IQueryResult>.Page<IQueryResult>(page, resultList.NumItems, resultList.HasMoreItems);
            };

            return new CollectionEnumerable<IQueryResult>(new PageFetcher<IQueryResult>(DefaultContext.MaxItemsPerPage, fetchPageDelegate));
        }

        /// <inheritdoc/>
        public IItemEnumerable<ICmisObject> QueryObjects(string typeId, string where, bool searchAllVersions, IOperationContext context)
        {
            if (typeId == null || typeId.Trim().Length == 0)
            {
                throw new ArgumentException("Type ID must be set!", "typeId");
            }

            if (context == null)
            {
                throw new ArgumentException("Operation context must be set!", "context");
            }

            IDiscoveryService discoveryService = Binding.GetDiscoveryService();
            IObjectFactory of = ObjectFactory;
            OperationContext ctxt = new OperationContext(context);
            StringBuilder statement = new StringBuilder("SELECT ");

            string select = ctxt.FilterString;
            if (select == null)
            {
                statement.Append('*');
            }
            else
            {
                statement.Append(select);
            }

            IObjectType type = GetTypeDefinition(typeId);
            statement.Append(" FROM ");
            statement.Append(type.QueryName);

            if (where != null && where.Trim().Length > 0)
            {
                statement.Append(" WHERE ");
                statement.Append(where);
            }

            string orderBy = ctxt.OrderBy;
            if (orderBy != null && orderBy.Trim().Length > 0)
            {
                statement.Append(" ORDER BY ");
                statement.Append(orderBy);
            }

            PageFetcher<ICmisObject>.FetchPage fetchPageDelegate = delegate(BigInteger maxNumItems, BigInteger skipCount)
            {
                // fetch the data
                IObjectList resultList = discoveryService.Query(RepositoryId, statement.ToString(),
                        searchAllVersions, ctxt.IncludeAllowableActions, ctxt.IncludeRelationships,
                        ctxt.RenditionFilterString, maxNumItems, skipCount, null);

                // convert query results
                IList<ICmisObject> page = new List<ICmisObject>();
                if (resultList.Objects != null)
                {
                    foreach (IObjectData objectData in resultList.Objects)
                    {
                        if (objectData == null)
                        {
                            continue;
                        }

                        page.Add(of.ConvertObject(objectData, ctxt));
                    }
                }

                return new PageFetcher<ICmisObject>.Page<ICmisObject>(page, resultList.NumItems, resultList.HasMoreItems);
            };

            return new CollectionEnumerable<ICmisObject>(new PageFetcher<ICmisObject>(DefaultContext.MaxItemsPerPage, fetchPageDelegate));
        }

        /// <inheritdoc/>
        public IQueryStatement CreateQueryStatement(string statement)
        {
            return new QueryStatement(this, statement);
        }

        /// <inheritdoc/>
        public string GetLatestChangeLogToken()
        {
            return Binding.GetRepositoryService().GetRepositoryInfo(RepositoryId, null).LatestChangeLogToken;
        }

        /// <inheritdoc/>
        public IChangeEvents GetContentChanges(string changeLogToken, bool includeProperties, long maxNumItems)
        {
            return GetContentChanges(changeLogToken, includeProperties, maxNumItems, DefaultContext);
        }

        /// <inheritdoc/>
        public IChangeEvents GetContentChanges(string changeLogToken, bool includeProperties, long maxNumItems,
                IOperationContext context)
        {
            lock (sessionLock)
            {
                IObjectList objectList = Binding.GetDiscoveryService().GetContentChanges(RepositoryId, ref changeLogToken, includeProperties,
                    context.FilterString, context.IncludePolicies, context.IncludeAcls, maxNumItems, null);

                return ObjectFactory.ConvertChangeEvents(changeLogToken, objectList);
            }
        }

        // create

        /// <inheritdoc/>
        public IObjectId CreateDocument(IDictionary<string, object> properties, IObjectId folderId, IContentStream contentStream,
            VersioningState? versioningState, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces)
        {
            CheckProperties(properties);

            string newId = Binding.GetObjectService().CreateDocument(RepositoryId, ObjectFactory.ConvertProperties(properties, null, null,
                (versioningState == VersioningState.CheckedOut ? CreateAndCheckoutUpdatability : CreateUpdatability)),
                (folderId == null ? null : folderId.Id), contentStream, versioningState, ObjectFactory.ConvertPolicies(policies),
                ObjectFactory.ConvertAces(addAces), ObjectFactory.ConvertAces(removeAces), null);

            return newId == null ? null : CreateObjectId(newId);
        }

        /// <inheritdoc/>
        public IObjectId CreateDocument(IDictionary<string, object> properties, IObjectId folderId, IContentStream contentStream,
            VersioningState? versioningState)
        {
            return CreateDocument(properties, folderId, contentStream, versioningState, null, null, null);
        }

        /// <inheritdoc/>
        public IObjectId CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, IObjectId folderId,
            VersioningState? versioningState, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces)
        {
            if (source == null || source.Id == null)
            {
                throw new ArgumentException("Source must be set!");
            }

            // get the type of the source document
            IObjectType type = null;
            IList<ISecondaryType> secondaryTypes = null;
            if (source is ICmisObject)
            {
                type = ((ICmisObject)source).ObjectType;
                secondaryTypes = ((ICmisObject)source).SecondaryTypes;
            }
            else
            {
                ICmisObject sourceObj = GetObject(source);
                type = sourceObj.ObjectType;
                secondaryTypes = sourceObj.SecondaryTypes;
            }

            if (type.BaseTypeId != BaseTypeId.CmisDocument)
            {
                throw new ArgumentException("Source object must be a document!");
            }

            string newId = Binding.GetObjectService().CreateDocumentFromSource(RepositoryId, source.Id,
                ObjectFactory.ConvertProperties(properties, type, secondaryTypes,
                (versioningState == VersioningState.CheckedOut ? CreateAndCheckoutUpdatability : CreateUpdatability)),
                (folderId == null ? null : folderId.Id),
                versioningState, ObjectFactory.ConvertPolicies(policies), ObjectFactory.ConvertAces(addAces),
                ObjectFactory.ConvertAces(removeAces), null);

            return newId == null ? null : CreateObjectId(newId);
        }

        /// <inheritdoc/>
        public IObjectId CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, IObjectId folderId,
                VersioningState? versioningState)
        {
            return CreateDocumentFromSource(source, properties, folderId, versioningState, null, null, null);
        }

        /// <inheritdoc/>
        public IObjectId CreateFolder(IDictionary<string, object> properties, IObjectId folderId, IList<IPolicy> policies,
            IList<IAce> addAces, IList<IAce> removeAces)
        {
            if (folderId == null || folderId.Id == null)
            {
                throw new ArgumentException("Folder ID must be set!");
            }
            CheckProperties(properties);

            string newId = Binding.GetObjectService().CreateFolder(RepositoryId, ObjectFactory.ConvertProperties(properties, null, null, CreateUpdatability),
                (folderId == null ? null : folderId.Id), ObjectFactory.ConvertPolicies(policies), ObjectFactory.ConvertAces(addAces),
                ObjectFactory.ConvertAces(removeAces), null);

            return newId == null ? null : CreateObjectId(newId);
        }

        /// <inheritdoc/>
        public IObjectId CreateFolder(IDictionary<string, object> properties, IObjectId folderId)
        {
            return CreateFolder(properties, folderId, null, null, null);
        }

        /// <inheritdoc/>
        public IObjectId CreatePolicy(IDictionary<string, object> properties, IObjectId folderId, IList<IPolicy> policies,
            IList<IAce> addAces, IList<IAce> removeAces)
        {
            CheckProperties(properties);

            string newId = Binding.GetObjectService().CreatePolicy(RepositoryId, ObjectFactory.ConvertProperties(properties, null, null, CreateUpdatability),
                (folderId == null ? null : folderId.Id), ObjectFactory.ConvertPolicies(policies), ObjectFactory.ConvertAces(addAces),
                ObjectFactory.ConvertAces(removeAces), null);

            return newId == null ? null : CreateObjectId(newId);
        }

        /// <inheritdoc/>
        public IObjectId CreatePolicy(IDictionary<string, object> properties, IObjectId folderId)
        {
            return CreatePolicy(properties, folderId, null, null, null);
        }

        /// <inheritdoc/>
        public IObjectId CreateItem(IDictionary<string, object> properties, IObjectId folderId, IList<IPolicy> policies, IList<IAce> addAces,
                IList<IAce> removeAces)
        {
            CheckProperties(properties);

            string newId = Binding.GetObjectService().CreateItem(RepositoryId, ObjectFactory.ConvertProperties(properties, null, null, CreateUpdatability),
                (folderId == null ? null : folderId.Id), ObjectFactory.ConvertPolicies(policies), ObjectFactory.ConvertAces(addAces),
                ObjectFactory.ConvertAces(removeAces), null);

            return newId == null ? null : CreateObjectId(newId);
        }

        /// <inheritdoc/>
        public IObjectId CreateItem(IDictionary<string, object> properties, IObjectId folderId)
        {
            return CreateItem(properties, folderId, null, null, null);
        }

        /// <inheritdoc/>
        public IObjectId CreateRelationship(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces,
                IList<IAce> removeAces)
        {
            CheckProperties(properties);

            string newId = Binding.GetObjectService().CreateRelationship(RepositoryId, ObjectFactory.ConvertProperties(properties, null, null, CreateUpdatability),
                ObjectFactory.ConvertPolicies(policies), ObjectFactory.ConvertAces(addAces), ObjectFactory.ConvertAces(removeAces), null);

            return newId == null ? null : CreateObjectId(newId);
        }

        /// <inheritdoc/>
        public IObjectId CreateRelationship(IDictionary<string, object> properties)
        {
            return CreateRelationship(properties, null, null, null);
        }

        /// <inheritdoc/>
        public IItemEnumerable<IRelationship> GetRelationships(IObjectId objectId, bool includeSubRelationshipTypes,
                RelationshipDirection? relationshipDirection, IObjectType type, IOperationContext context)
        {
            if (objectId == null || objectId.Id == null)
            {
                throw new ArgumentException("Invalid object ID!");
            }

            string id = objectId.Id;
            string typeId = (type == null ? null : type.Id);
            IRelationshipService service = Binding.GetRelationshipService();
            IOperationContext ctxt = new OperationContext(context);

            PageFetcher<IRelationship>.FetchPage fetchPageDelegate = delegate(BigInteger maxNumItems, BigInteger skipCount)
            {
                // fetch the relationships
                IObjectList relList = service.GetObjectRelationships(RepositoryId, id, includeSubRelationshipTypes, relationshipDirection,
                    typeId, ctxt.FilterString, ctxt.IncludeAllowableActions, maxNumItems, skipCount, null);

                // convert relationship objects
                IList<IRelationship> page = new List<IRelationship>();
                if (relList.Objects != null)
                {
                    foreach (IObjectData rod in relList.Objects)
                    {
                        IRelationship relationship = GetObject(CreateObjectId(rod.Id), ctxt) as IRelationship;
                        if (relationship == null)
                        {
                            throw new CmisRuntimeException("Repository returned an object that is not a relationship!");
                        }

                        page.Add(relationship);
                    }
                }

                return new PageFetcher<IRelationship>.Page<IRelationship>(page, relList.NumItems, relList.HasMoreItems);
            };

            return new CollectionEnumerable<IRelationship>(new PageFetcher<IRelationship>(DefaultContext.MaxItemsPerPage, fetchPageDelegate));
        }

        /// <inheritdoc/>
        public IList<IBulkUpdateObjectIdAndChangeToken> BulkUpdateProperties(IList<ICmisObject> objects,
            IDictionary<string, object> properties, IList<string> addSecondaryTypeIds, IList<string> removeSecondaryTypeIds)
        {
            CheckCmisVersion();
            CheckProperties(properties);

            IObjectType objectType = null;
            IDictionary<string, ISecondaryType> secondaryTypes = new Dictionary<string, ISecondaryType>();

            // gather secondary types
            if (addSecondaryTypeIds != null)
            {
                foreach (string stid in addSecondaryTypeIds)
                {
                    IObjectType secondaryType = GetTypeDefinition(stid);

                    if (!(secondaryType is ISecondaryType))
                    {
                        throw new ArgumentException("Secondary types contains a type that is not a secondary type: "
                                + secondaryType.Id, "addSecondaryTypeIds");
                    }

                    secondaryTypes[secondaryType.Id] = (ISecondaryType)secondaryType;
                }
            }

            // gather IDs and change tokens
            IList<IBulkUpdateObjectIdAndChangeToken> objectIdsAndChangeTokens = new List<IBulkUpdateObjectIdAndChangeToken>();
            foreach (ICmisObject obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }

                objectIdsAndChangeTokens.Add(new BulkUpdateObjectIdAndChangeToken() { Id = obj.Id, ChangeToken = obj.ChangeToken });

                if (objectType == null)
                {
                    objectType = obj.ObjectType;
                }

                if (obj.SecondaryTypes != null)
                {
                    foreach (ISecondaryType secondaryType in obj.SecondaryTypes)
                    {
                        secondaryTypes[secondaryType.Id] = secondaryType;
                    }
                }
            }

            ISet<Updatability> updatebility = new HashSet<Updatability>();
            updatebility.Add(Updatability.ReadWrite);

            return Binding.GetObjectService().BulkUpdateProperties(RepositoryId, objectIdsAndChangeTokens,
                    ObjectFactory.ConvertProperties(properties, objectType, secondaryTypes.Values, updatebility),
                    addSecondaryTypeIds, removeSecondaryTypeIds, null);
        }

        // delete

        /// <inheritdoc/>
        public void Delete(IObjectId objectId)
        {
            Delete(objectId, true);
        }

        /// <inheritdoc/>
        public void Delete(IObjectId objectId, bool allVersions)
        {
            if (objectId == null || objectId.Id == null)
            {
                throw new ArgumentException("Invalid object ID!", "objectId");
            }

            Binding.GetObjectService().DeleteObject(RepositoryId, objectId.Id, allVersions, null);
            RemoveObjectFromCache(objectId);
        }

        /// <inheritdoc/>
        public IList<string> DeleteTree(IObjectId folderId, bool allVersions, UnfileObject? unfile, bool continueOnFailure)
        {
            if (folderId == null || folderId.Id == null)
            {
                throw new ArgumentException("Invalid object ID!", "folderId");
            }

            IFailedToDeleteData failed = Binding.GetObjectService().DeleteTree(RepositoryId, folderId.Id, allVersions, unfile, continueOnFailure, null);

            if (failed == null || failed.Ids == null || failed.Ids.Count == 0)
            {
                RemoveObjectFromCache(folderId);
            }

            return failed != null ? failed.Ids : null;
        }

        // content stream

        /// <inheritdoc/>
        public IContentStream GetContentStream(IObjectId docId)
        {
            return GetContentStream(docId, null, null, null);
        }

        /// <inheritdoc/>
        public IContentStream GetContentStream(IObjectId docId, string streamId, long? offset, long? length)
        {
            if (docId == null || docId.Id == null)
            {
                throw new ArgumentException("Invalid document ID!", "objectId");
            }

            // get the content stream
            IContentStream contentStream = null;
            try
            {
                contentStream = Binding.GetObjectService().GetContentStream(RepositoryId, docId.Id, streamId, offset, length, null);
            }
            catch (CmisConstraintException)
            {
                // no content stream
                return null;
            }

            return contentStream;
        }

        // permissions

        /// <inheritdoc/>
        public IAcl GetAcl(IObjectId objectId, bool onlyBasicPermissions)
        {
            if (objectId == null || objectId.Id == null)
            {
                throw new ArgumentException("Invalid object ID!", "objectId");
            }

            return Binding.GetAclService().GetAcl(RepositoryId, objectId.Id, onlyBasicPermissions, null);
        }

        /// <inheritdoc/>
        public IAcl ApplyAcl(IObjectId objectId, IList<IAce> addAces, IList<IAce> removeAces, AclPropagation? aclPropagation)
        {
            if (objectId == null || objectId.Id == null)
            {
                throw new ArgumentException("Invalid object ID!", "objectId");
            }

            return Binding.GetAclService().ApplyAcl(RepositoryId, objectId.Id, ObjectFactory.ConvertAces(addAces),
                ObjectFactory.ConvertAces(removeAces), aclPropagation, null);
        }

        /// <inheritdoc/>
        public void ApplyPolicy(IObjectId objectId, params IObjectId[] policyIds)
        {
            if (objectId == null || objectId.Id == null)
            {
                throw new ArgumentException("Invalid object ID!", "objectId");
            }
            if (policyIds == null || (policyIds.Length == 0))
            {
                throw new ArgumentException("No Policies provided!");
            }

            string[] ids = new string[policyIds.Length];
            for (int i = 0; i < policyIds.Length; i++)
            {
                if (policyIds[i] == null || policyIds[i].Id == null)
                {
                    throw new ArgumentException("A Policy ID is not set!", "policyIds");
                }

                ids[i] = policyIds[i].Id;
            }

            foreach (string id in ids)
            {
                Binding.GetPolicyService().ApplyPolicy(RepositoryId, id, objectId.Id, null);
            }
        }

        /// <inheritdoc/>
        public void RemovePolicy(IObjectId objectId, params IObjectId[] policyIds)
        {
            if (objectId == null || objectId.Id == null)
            {
                throw new ArgumentException("Invalid object ID!", "objectId");
            }
            if (policyIds == null || policyIds.Length == 0)
            {
                throw new ArgumentException("No Policies provided!", "policyIds");
            }

            string[] ids = new string[policyIds.Length];
            for (int i = 0; i < policyIds.Length; i++)
            {
                if (policyIds[i] == null || policyIds[i].Id == null)
                {
                    throw new ArgumentException("A Policy Id is not set!");
                }

                ids[i] = policyIds[i].Id;
            }

            foreach (string id in ids)
            {
                Binding.GetPolicyService().RemovePolicy(RepositoryId, id, objectId.Id, null);
            }
        }

        /// <summary>
        /// Checks if the given string is a valid path.
        /// </summary>
        protected void CheckPath(string path)
        {
            if (path == null || path.Length < 1)
            {
                throw new ArgumentException("Invalid path!");
            }
            if (path[0] != '/')
            {
                throw new ArgumentException("Path must start with a '/'!");
            }
        }

        /// <summary>
        /// Checks if the CMIS version of this repository is 1.1.
        /// </summary>
        protected void CheckCmisVersion()
        {
            if (RepositoryInfo.CmisVersion == CmisVersion.Cmis_1_0)
            {
                throw new CmisNotSupportedException("This method is not supported for CMIS 1.0 repositories.");
            }
        }

        /// <summary>
        /// Checks if properties are set.
        /// </summary>
        protected void CheckProperties(IDictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                throw new ArgumentException("Properties must not be empty!");
            }
        }
    }

    internal class QueryStatement : IQueryStatement
    {
        private ISession session;
        private string statement;
        private IDictionary<int, string> parametersDict = new Dictionary<int, string>();

        public QueryStatement(Session session, string statement)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            this.session = session;
            this.statement = statement.Trim();
        }

        /// <inheritdoc/>
        public void SetType(int parameterIndex, string typeId)
        {
            SetType(parameterIndex, session.GetTypeDefinition(typeId));
        }

        /// <inheritdoc/>
        public void SetType(int parameterIndex, IObjectType type)
        {
            if (type == null)
            {
                throw new ArgumentException("Type must be set!");
            }

            if (type.QueryName == null)
            {
                throw new ArgumentException("Type has no query name!");
            }

            parametersDict[parameterIndex] = type.QueryName;
        }

        /// <inheritdoc/>
        public void SetProperty(int parameterIndex, string typeId, string propertyId)
        {
            IObjectType type = session.GetTypeDefinition(typeId);

            IPropertyDefinition propertyDefinition = type[propertyId];
            if (propertyDefinition == null)
            {
                throw new ArgumentException("Property does not exist!");
            }

            SetProperty(parameterIndex, propertyDefinition);
        }

        /// <inheritdoc/>
        public void SetProperty(int parameterIndex, IPropertyDefinition propertyDefinition)
        {
            if (propertyDefinition == null)
            {
                throw new ArgumentException("Property must be set!");
            }

            string queryName = propertyDefinition.QueryName;
            if (queryName == null)
            {
                throw new ArgumentException("Property has no query name!");
            }

            parametersDict[parameterIndex] = queryName;
        }

        /// <inheritdoc/>
        public void SetInteger(int parameterIndex, params BigInteger[] num)
        {
            if (num == null || num.Length == 0)
            {
                throw new ArgumentException("Number must be set!");
            }

            StringListBuilder slb = new StringListBuilder(",");
            foreach (BigInteger n in num)
            {
                if (n == null)
                {
                    throw new ArgumentException("Number is null!");
                }

                slb.Add(n.ToString("0", CultureInfo.InvariantCulture));
            }

            parametersDict[parameterIndex] = slb.ToString();
        }

        /// <inheritdoc/>
        public void SetDecimal(int parameterIndex, params decimal[] num)
        {
            if (num == null || num.Length == 0)
            {
                throw new ArgumentException("Number must be set!");
            }

            StringListBuilder slb = new StringListBuilder(",");
            foreach (decimal n in num)
            {
                slb.Add(n.ToString("g", CultureInfo.InvariantCulture));
            }

            parametersDict[parameterIndex] = slb.ToString();
        }

        /// <inheritdoc/>
        public void SetString(int parameterIndex, params string[] str)
        {
            if (str == null || str.Length == 0)
            {
                throw new ArgumentException("String must be set!");
            }

            StringListBuilder slb = new StringListBuilder(",");
            foreach (string s in str)
            {
                if (s == null)
                {
                    throw new ArgumentException("String is null!");
                }

                slb.Add(Escape(s));
            }

            parametersDict[parameterIndex] = slb.ToString();
        }

        /// <inheritdoc/>
        public void SetStringLike(int parameterIndex, string str)
        {
            if (str == null)
            {
                throw new ArgumentException("String must be set!");
            }

            parametersDict[parameterIndex] = EscapeLike(str);
        }

        /// <inheritdoc/>
        public void SetStringContains(int parameterIndex, string str)
        {
            if (str == null)
            {
                throw new ArgumentException("String must be set!");
            }

            parametersDict[parameterIndex] = EscapeContains(str);
        }

        /// <inheritdoc/>
        public void SetId(int parameterIndex, params  IObjectId[] id)
        {
            if (id == null || id.Length == 0)
            {
                throw new ArgumentException("Id must be set!");
            }

            StringListBuilder slb = new StringListBuilder(",");
            foreach (IObjectId oid in id)
            {
                if (oid == null || oid.Id == null)
                {
                    throw new ArgumentException("Id is null!");
                }

                slb.Add(Escape(oid.Id));
            }

            parametersDict[parameterIndex] = slb.ToString();
        }

        /// <inheritdoc/>
        public void SetUri(int parameterIndex, params Uri[] uri)
        {
            if (uri == null || uri.Length == 0)
            {
                throw new ArgumentException("URI must be set!");
            }

            StringListBuilder slb = new StringListBuilder(",");
            foreach (Uri u in uri)
            {
                if (u == null)
                {
                    throw new ArgumentException("URI is null!");
                }

                slb.Add(Escape(u.ToString()));
            }

            parametersDict[parameterIndex] = slb.ToString();
        }

        /// <inheritdoc/>
        public void SetBoolean(int parameterIndex, params bool[] boolean)
        {
            if (boolean == null || boolean.Length == 0)
            {
                throw new ArgumentException("Boolean must be set!");
            }

            StringListBuilder slb = new StringListBuilder(",");
            foreach (bool b in boolean)
            {
                slb.Add(b ? "TRUE" : "FALSE");
            }

            parametersDict[parameterIndex] = slb.ToString();
        }

        /// <inheritdoc/>
        public void SetDateTime(int parameterIndex, params DateTime[] dt)
        {
            SetDateTime(parameterIndex, false, dt);
        }

        /// <inheritdoc/>
        public void SetDateTime(int parameterIndex, params long[] ms)
        {
            SetDateTime(parameterIndex, false, ms);
        }

        /// <inheritdoc/>
        public void SetDateTimeTimestamp(int parameterIndex, params DateTime[] dt)
        {
            SetDateTime(parameterIndex, true, dt);
        }

        /// <inheritdoc/>
        public void SetDateTimeTimestamp(int parameterIndex, params long[] ms)
        {
            SetDateTime(parameterIndex, true, ms);
        }

        /// <inheritdoc/>
        protected void SetDateTime(int parameterIndex, bool prefix, params DateTime[] cal)
        {
            if (cal == null || cal.Length == 0)
            {
                throw new ArgumentException("DateTime must be set!");
            }

            StringBuilder sb = new StringBuilder();
            foreach (DateTime dt in cal)
            {
                if (dt == null)
                {
                    throw new ArgumentException("DateTime is null!");
                }

                if (sb.Length > 0)
                {
                    sb.Append(',');
                }

                if (prefix)
                {
                    sb.Append("TIMESTAMP ");
                }

                DateTime tmp = dt.ToUniversalTime();
                sb.Append("'");
                sb.Append(tmp.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
                sb.Append("'");
            }

            parametersDict[parameterIndex] = sb.ToString();
        }

        /// <inheritdoc/>
        protected void SetDateTime(int parameterIndex, bool prefix, params long[] ms)
        {
            if (ms == null || ms.Length == 0)
            {
                throw new ArgumentException("DateTime must be set!");
            }

            StringBuilder sb = new StringBuilder();
            foreach (long dt in ms)
            {
                if (sb.Length > 0)
                {
                    sb.Append(',');
                }

                if (prefix)
                {
                    sb.Append("TIMESTAMP ");
                }

                DateTime tmp = DateTimeHelper.ConvertMillisToDateTime(dt).ToUniversalTime();
                sb.Append("'");
                sb.Append(tmp.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
                sb.Append("'");
            }

            parametersDict[parameterIndex] = sb.ToString();
        }

        /// <inheritdoc/>
        public string ToQueryString()
        {
            bool inStr = false;
            int parameterIndex = 0;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < statement.Length; i++)
            {
                char c = statement[i];

                if (c == '\'')
                {
                    if (inStr && statement[i - 1] == '\\')
                    {
                        inStr = true;
                    }
                    else
                    {
                        inStr = !inStr;
                    }
                    sb.Append(c);
                }
                else if (c == '?' && !inStr)
                {
                    parameterIndex++;
                    string s;
                    if (parametersDict.TryGetValue(parameterIndex, out s))
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public IItemEnumerable<IQueryResult> Query(bool searchAllVersions)
        {
            return session.Query(ToQueryString(), searchAllVersions);
        }

        /// <inheritdoc/>
        public IItemEnumerable<IQueryResult> Query(bool searchAllVersions, IOperationContext context)
        {
            return session.Query(ToQueryString(), searchAllVersions, context);
        }

        // --- internal ---

        private static string Escape(string str)
        {
            StringBuilder sb = new StringBuilder("'");
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\'' || c == '\\')
                {
                    sb.Append('\\');
                }

                sb.Append(c);
            }

            sb.Append('\'');

            return sb.ToString();
        }

        private static string EscapeLike(string str)
        {
            StringBuilder sb = new StringBuilder("'");
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\'')
                {
                    sb.Append('\\');
                }
                else if (c == '\\')
                {
                    if (i + 1 < str.Length && (str[i + 1] == '%' || str[i + 1] == '_'))
                    {
                        // no additional back slash
                    }
                    else
                    {
                        sb.Append('\\');
                    }
                }

                sb.Append(c);
            }

            sb.Append('\'');

            return sb.ToString();
        }

        private static string EscapeContains(string str)
        {
            StringBuilder sb = new StringBuilder("'");
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\\')
                {
                    sb.Append('\\');
                }
                else if (c == '\'' || c == '\"')
                {
                    sb.Append("\\\\\\");
                }

                sb.Append(c);
            }

            sb.Append('\'');

            return sb.ToString();
        }
    }
}
