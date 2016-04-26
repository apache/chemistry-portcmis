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
using PortCMIS.Data;
using PortCMIS.Data.Extensions;
using PortCMIS.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PortCMIS.Client
{
    /// <summary>
    /// Session factory interface.
    /// </summary>
    public interface ISessionFactory
    {
        /// <summary>
        /// Creates a new session with the given parameters and connects to the repository.
        /// </summary>
        /// <param name="parameters">the session parameters</param>
        /// <returns>the newly created session</returns>
        /// <example>
        /// Connect to an Browser CMIS endpoint:
        /// <code>
        /// Dictionary&lt;string, string&gt; parameters = new Dictionary&lt;string, string&gt;();
        /// 
        /// parameters[SessionParameter.BindingType] = BindingType.Browser;
        /// parameters[SessionParameter.BrowserUrl] = "http://localhost/cmis/browser";
        /// parameters[SessionParameter.Password] = "admin";
        /// parameters[SessionParameter.User] = "admin";
        /// parameters[SessionParameter.RepositoryId] = "1234-abcd-5678";
        ///
        /// SessionFactory factory = SessionFactory.NewInstance();
        /// ISession session = factory.CreateSession(parameters);
        /// </code>
        /// Connect to an AtomPub CMIS endpoint:
        /// <code>
        /// Dictionary&lt;string, string&gt; parameters = new Dictionary&lt;string, string&gt;();
        /// 
        /// parameters[SessionParameter.BindingType] = BindingType.AtomPub;
        /// parameters[SessionParameter.AtomPubUrl] = "http://localhost/cmis/atom";
        /// parameters[SessionParameter.Password] = "admin";
        /// parameters[SessionParameter.User] = "admin";
        /// parameters[SessionParameter.RepositoryId] = "1234-abcd-5678";
        ///
        /// SessionFactory factory = SessionFactory.NewInstance();
        /// ISession session = factory.CreateSession(parameters);
        /// </code>
        /// </example>
        /// <seealso cref="PortCMIS.Client.SessionParameter"/>
        ISession CreateSession(IDictionary<string, string> parameters);

        /// <summary>
        /// Creates the session.
        /// </summary>
        /// <returns>The session.</returns>
        /// <param name="parameters">Parameters.</param>
        /// <param name="objectFactory">Object factory.</param>
        /// <param name="authenticationProvider">Authentication provider.</param>
        /// <param name="cache">Client object cache.</param>
        /// <seealso cref="PortCMIS.Client.SessionParameter"/>
        ISession CreateSession(IDictionary<string, string> parameters, IObjectFactory objectFactory, IAuthenticationProvider authenticationProvider, ICache cache);

        /// <summary>
        /// Gets all repository available at the specified endpoint.
        /// </summary>
        /// <param name="parameters">the session parameters</param>
        /// <returns>a list of all available repositories</returns>
        /// <seealso cref="PortCMIS.Client.SessionParameter"/>
        IList<IRepository> GetRepositories(IDictionary<string, string> parameters);
    }

    /// <summary>
    /// Repository interface.
    /// </summary>
    public interface IRepository : IRepositoryInfo
    {
        /// <summary>
        /// Creates a session for this repository.
        /// </summary>
        ISession CreateSession();
    }

    /// <summary>
    /// A session is a connection to a CMIS repository with a specific user.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Not all operations might be supported by the connected repository. Either PortCMIS or the repository will 
    /// throw an exception if an unsupported operation is called. 
    /// The capabilities of the repository can be discovered by evaluating the repository info 
    /// (see <see cref="RepositoryInfo"/>).
    /// </para>
    /// <para>
    /// Almost all methods might throw exceptions derived from <see cref="PortCMIS.Exceptions.CmisBaseException"/>!
    /// </para>
    /// <para>
    /// (Please refer to the <a href="http://docs.oasis-open.org/cmis/CMIS/v1.0/os/">CMIS specification</a>
    /// for details about the domain model, terms, concepts, base types, properties, IDs and query names, 
    /// query language, etc.)
    /// </para>
    /// </remarks>
    public interface ISession
    {
        /// <summary>
        /// Clears all caches.
        /// </summary>
        void Clear();

        /// <value>
        /// Gets the CMIS binding object.
        /// </value>
        ICmisBinding Binding { get; }

        /// <value>
        /// Gets the default operation context.
        /// </value>
        IOperationContext DefaultContext { get; set; }

        /// <summary>
        /// Creates a new operation context object.
        /// </summary>
        IOperationContext CreateOperationContext();

        /// <summary>
        /// Creates a new operation context object with the given parameters.
        /// </summary>
        IOperationContext CreateOperationContext(HashSet<string> filter, bool includeAcls, bool includeAllowableActions, bool includePolicies,
            IncludeRelationships includeRelationships, HashSet<string> renditionFilter, bool includePathSegments, string orderBy,
            bool cacheEnabled, int maxItemsPerPage);

        /// <summary>
        /// Creates a new <see cref="PortCMIS.Client.IObjectId"/> with the given ID.
        /// </summary>
        IObjectId CreateObjectId(string id);

        /// <value>
        /// Gets the CMIS repository info.
        /// </value>
        /// <cmis>1.0</cmis>
        IRepositoryInfo RepositoryInfo { get; }

        /// <value>
        /// Gets the internal object factory. 
        /// </value>
        IObjectFactory ObjectFactory { get; }

        // types

        /// <summary>
        /// Gets the definition of a type.
        /// </summary>
        /// <param name="typeId">the ID of the type</param>
        /// <returns>the type definition</returns>
        /// <cmis>1.0</cmis>
        IObjectType GetTypeDefinition(string typeId);

        /// <summary>
        /// Gets the type children of a type.
        /// </summary>
        /// <param name="typeId">the type ID or <c>null</c> to request the base types</param>
        /// <param name="includePropertyDefinitions">indicates whether the property definitions should be included or not</param>
        /// <returns>the type iterator, not <c>null</c></returns>
        /// <cmis>1.0</cmis>
        IItemEnumerable<IObjectType> GetTypeChildren(string typeId, bool includePropertyDefinitions);

        /// <summary>
        /// Gets the type descendants of a type.
        /// </summary>
        /// <param name="typeId">the type ID or <c>null</c> to request the base types</param>
        /// <param name="depth">the tree depth, must be greater than 0 or -1 for infinite depth</param>
        /// <param name="includePropertyDefinitions">indicates whether the property definitions should be included or not</param>
        /// <returns>the tree of types</returns>
        IList<ITree<IObjectType>> GetTypeDescendants(string typeId, int depth, bool includePropertyDefinitions);

        // navigation

        /// <summary>
        /// Gets the root folder of the repository.
        /// </summary>
        /// <returns>the root folder object, not <c>null</c></returns>
        /// <cmis>1.0</cmis>
        IFolder GetRootFolder();

        /// <summary>
        /// Gets the root folder of the repository with the given <see cref="PortCMIS.Client.IOperationContext"/>.
        /// </summary>
        /// <param name="context">the operation context</param>
        /// <returns>the root folder object, not <c>null</c></returns>
        /// <cmis>1.0</cmis>
        IFolder GetRootFolder(IOperationContext context);

        /// <summary>
        /// Returns all checked out documents.
        /// </summary>
        /// <returns></returns>
        /// <cmis>1.0</cmis>
        IItemEnumerable<IDocument> GetCheckedOutDocs();

        /// <summary>
        /// Returns all checked out documents with the given operation context.
        /// </summary>
        /// <returns></returns>
        /// <cmis>1.0</cmis>
        IItemEnumerable<IDocument> GetCheckedOutDocs(IOperationContext context);

        /// <summary>
        /// Gets a CMIS object from the session cache. If the object is not in the cache or the cache is 
        /// turned off per default operation context, it will load the object from the repository and puts
        /// it into the cache.
        /// <para>
        /// This method might return a stale object if the object has been found in the cache and has
        /// been changed in or removed from the repository.
        /// Use <see cref="PortCMIS.Client.ICmisObject.Refresh()"/> and <see cref="PortCMIS.Client.ICmisObject.RefreshIfOld(long)"/>
        /// to update the object if necessary.
        /// </para>
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <cmis>1.0</cmis>
        ICmisObject GetObject(IObjectId objectId);

        /// <summary>
        /// Gets a CMIS object from the session cache. If the object is not in the cache or the cache is 
        /// turned off or the given operation context has caching turned off, it will load the object 
        /// from the repository and puts it into the cache.
        /// <para>
        /// This method might return a stale object if the object has been found in the cache and has
        /// been changed in or removed from the repository.
        /// Use <see cref="PortCMIS.Client.ICmisObject.Refresh()"/> and <see cref="PortCMIS.Client.ICmisObject.RefreshIfOld(long)"/>
        /// to update the object if necessary.
        /// </para>
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <param name="context">the operation context</param>
        /// <cmis>1.0</cmis>
        ICmisObject GetObject(IObjectId objectId, IOperationContext context);

        /// <summary>
        /// Gets a CMIS object from the session cache. If the object is not in the cache or the cache is 
        /// turned off per default operation context, it will load the object from the repository and puts
        /// it into the cache.
        /// <para>
        /// This method might return a stale object if the object has been found in the cache and has
        /// been changed in or removed from the repository.
        /// Use <see cref="PortCMIS.Client.ICmisObject.Refresh()"/> and <see cref="PortCMIS.Client.ICmisObject.RefreshIfOld(long)"/>
        /// to update the object if necessary.
        /// </para>
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <cmis>1.0</cmis>
        ICmisObject GetObject(string objectId);

        /// <summary>
        /// Gets a CMIS object from the session cache. If the object is not in the cache or the cache is 
        /// turned off or the given operation context has caching turned off, it will load the object 
        /// from the repository and puts it into the cache.
        /// <para>
        /// This method might return a stale object if the object has been found in the cache and has
        /// been changed in or removed from the repository.
        /// Use <see cref="PortCMIS.Client.ICmisObject.Refresh()"/> and <see cref="PortCMIS.Client.ICmisObject.RefreshIfOld(long)"/>
        /// to update the object if necessary.
        /// </para>
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <param name="context">the operation context</param>
        /// <cmis>1.0</cmis>
        ICmisObject GetObject(string objectId, IOperationContext context);

        /// <summary>
        /// Gets a CMIS object from the session cache. If the object is not in the cache or the cache is 
        /// turned off per default operation context, it will load the object
        /// from the repository and puts it into the cache.
        /// <para>
        /// This method might return a stale object if the object has been found in the cache and has
        /// been changed in or removed from the repository.
        /// Use <see cref="PortCMIS.Client.ICmisObject.Refresh()"/> and <see cref="PortCMIS.Client.ICmisObject.RefreshIfOld(long)"/>
        /// to update the object if necessary.
        /// </para>
        /// </summary>
        /// <param name="path">the path to the object</param>
        /// <cmis>1.0</cmis>
        ICmisObject GetObjectByPath(string path);

        /// <summary>
        /// Gets a CMIS object from the session cache. If the object is not in the cache or the cache is 
        /// turned off or the given operation context has caching turned off, it will load the object
        /// from the repository and puts it into the cache.
        /// <para>
        /// This method might return a stale object if the object has been found in the cache and has
        /// been changed in or removed from the repository.
        /// Use <see cref="PortCMIS.Client.ICmisObject.Refresh()"/> and <see cref="PortCMIS.Client.ICmisObject.RefreshIfOld(long)"/>
        /// to update the object if necessary.
        /// </para>
        /// </summary>
        /// <param name="path">the path to the object</param>
        /// <param name="context">the operation context</param>
        /// <cmis>1.0</cmis>
        ICmisObject GetObjectByPath(string path, IOperationContext context);

        /// <summary>
        /// Gets a CMIS object from the session cache. If the object is not in the cache or the cache is 
        /// turned off per default operation context, it will load the object
        /// from the repository and puts it into the cache.
        /// <para>
        /// This method might return a stale object if the object has been found in the cache and has
        /// been changed in or removed from the repository.
        /// Use <see cref="PortCMIS.Client.ICmisObject.Refresh()"/> and <see cref="PortCMIS.Client.ICmisObject.RefreshIfOld(long)"/>
        /// to update the object if necessary.
        /// </para>
        /// </summary>
        /// <param name="parentPath">the path of the parent folder</param>
        /// <param name="name">name of the object</param>
        /// <cmis>1.0</cmis>
        ICmisObject GetObjectByPath(string parentPath, string name);

        /// <summary>
        /// Gets a CMIS object from the session cache. If the object is not in the cache or the cache is 
        /// turned off or the given operation context has caching turned off, it will load the object
        /// from the repository and puts it into the cache.
        /// <para>
        /// This method might return a stale object if the object has been found in the cache and has
        /// been changed in or removed from the repository.
        /// Use <see cref="PortCMIS.Client.ICmisObject.Refresh()"/> and <see cref="PortCMIS.Client.ICmisObject.RefreshIfOld(long)"/>
        /// to update the object if necessary.
        /// </para>
        /// </summary>
        /// <param name="parentPath">the path of the parent folder</param>
        /// <param name="name">name of the object</param>
        /// <param name="context">the operation context</param>
        /// <cmis>1.0</cmis>
        ICmisObject GetObjectByPath(string parentPath, string name, IOperationContext context);

        /// <summary>
        /// Gets the latest version in a version series.
        /// </summary>
        /// <param name="objectId">the document ID of an arbitrary version in the version series</param>
        /// <cmis>1.0</cmis>
        IDocument GetLatestDocumentVersion(string objectId);

        /// <summary>
        /// Gets the latest version in a version series with the given operation context.
        /// </summary>
        /// <param name="objectId">the document ID of an arbitrary version in the version series</param>
        /// <param name="context">the operation context</param>
        /// <cmis>1.0</cmis>
        IDocument GetLatestDocumentVersion(string objectId, IOperationContext context);

        /// <summary>
        /// Gets the latest version in a version series.
        /// </summary>
        /// <param name="objectId">the document ID of an arbitrary version in the version series</param>
        /// <cmis>1.0</cmis>
        IDocument GetLatestDocumentVersion(IObjectId objectId);

        /// <summary>
        /// Gets the latest version in a version series with the given operation context.
        /// </summary>
        /// <param name="objectId">the document ID of an arbitrary version in the version series</param>
        /// <param name="context">the operation context</param>
        /// <cmis>1.0</cmis>
        IDocument GetLatestDocumentVersion(IObjectId objectId, IOperationContext context);

        /// <summary>
        /// Gets the latest version in a version series with the given operation context.
        /// </summary>
        /// <param name="objectId">the document ID of an arbitrary version in the version series</param>
        /// <param name="major">defines if the latest major or the latest minor version should be returned</param>
        /// <param name="context">the operation context</param>
        /// <cmis>1.0</cmis>
        IDocument GetLatestDocumentVersion(IObjectId objectId, bool major, IOperationContext context);

        /// <summary>
        ///  Removes the given object from the cache.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        void RemoveObjectFromCache(IObjectId objectId);

        /// <summary>
        ///  Removes the given object from the cache.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        void RemoveObjectFromCache(string objectId);

        // discovery

        /// <summary>
        /// Performs a query.
        /// </summary>
        /// <param name="statement">the CMIS QL statement</param>
        /// <param name="searchAllVersions">indicates if all versions or only latest version should be searched</param>
        /// <returns>query results</returns>
        /// <cmis>1.0</cmis>
        IItemEnumerable<IQueryResult> Query(string statement, bool searchAllVersions);
        IItemEnumerable<ICmisObject> QueryObjects(string typeId, string where, bool searchAllVersions, IOperationContext context);

        /// <summary>
        /// Creates a query statement.
        /// </summary>
        /// <param name="statement">the CMIS QL statement</param>
        /// <returns>the query statement object</returns>
        /// <seealso cref="PortCMIS.Client.IQueryStatement"/>
        /// <cmis>1.0</cmis>
        IQueryStatement CreateQueryStatement(string statement);

        /// <summary>
        /// Performs a query using the given <see cref="PortCMIS.Client.IOperationContext"/>.
        /// </summary>
        /// <param name="statement">the CMIS QL statement</param>
        /// <param name="searchAllVersions">indicates if all versions or only latest version should be searched</param>
        /// <param name="context">the <see cref="PortCMIS.Client.IOperationContext"/></param>
        /// <returns>query results</returns>
        /// <cmis>1.0</cmis>
        IItemEnumerable<IQueryResult> Query(string statement, bool searchAllVersions, IOperationContext context);

        /// <summary>
        /// Gets the latest change log token from the repository.
        /// </summary>
        /// <returns>the latest change log token</returns>
        string GetLatestChangeLogToken();
        IChangeEvents GetContentChanges(string changeLogToken, bool includeProperties, long maxNumItems);
        IChangeEvents GetContentChanges(string changeLogToken, bool includeProperties, long maxNumItems, IOperationContext context);

        // create

        IObjectId CreateDocument(IDictionary<string, object> properties, IObjectId folderId, IContentStream contentStream,
                VersioningState? versioningState, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces);
        IObjectId CreateDocument(IDictionary<string, object> properties, IObjectId folderId, IContentStream contentStream,
                VersioningState? versioningState);
        IObjectId CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, IObjectId folderId,
                VersioningState? versioningState, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces);
        IObjectId CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, IObjectId folderId,
                VersioningState? versioningState);
        IObjectId CreateFolder(IDictionary<string, object> properties, IObjectId folderId, IList<IPolicy> policies, IList<IAce> addAces,
                IList<IAce> removeAces);
        IObjectId CreateFolder(IDictionary<string, object> properties, IObjectId folderId);
        IObjectId CreatePolicy(IDictionary<string, object> properties, IObjectId folderId, IList<IPolicy> policies, IList<IAce> addAces,
                IList<IAce> removeAces);
        IObjectId CreatePolicy(IDictionary<string, object> properties, IObjectId folderId);
        IObjectId CreateRelationship(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces,
                IList<IAce> removeAces);
        IObjectId CreateRelationship(IDictionary<string, object> properties);
        IObjectId CreateItem(IDictionary<string, object> properties, IObjectId folderId, IList<IPolicy> policies, IList<IAce> addAces,
            IList<IAce> removeAces);
        IObjectId CreateItem(IDictionary<string, object> properties, IObjectId folderId);

        IItemEnumerable<IRelationship> GetRelationships(IObjectId objectId, bool includeSubRelationshipTypes,
                RelationshipDirection? relationshipDirection, IObjectType type, IOperationContext context);

        /// <summary>
        /// Deletes an object and, if it is a document, all versions in the version series.
        /// </summary>
        /// <param name="objectId">the ID of the object</param>
        /// <cmis>1.0</cmis>
        void Delete(IObjectId objectId);

        /// <summary>
        /// Deletes an object.
        /// </summary>
        /// <param name="objectId">the ID of the object</param>
        /// <param name="allVersions">if this object is a document this parameter defines if only this version or all versions should be deleted</param>
        /// <cmis>1.0</cmis>
        void Delete(IObjectId objectId, bool allVersions);

        /// <summary>
        /// Deletes a folder and all subfolders.
        /// </summary>
        /// <param name="folderId"> the ID of the folder</param>
        /// <param name="allVersions">if this object is a document this parameter defines
        /// if only this version or all versions should be deleted</param>
        /// <param name="unfile">defines how objects should be unfiled</param>
        /// <param name="continueOnFailure">if <c>true</c> the repository tries to delete as many objects as possible;
        /// if <c>false</c> the repository stops at the first object that could not be deleted</param>
        /// <returns>a list of object IDs which failed to be deleted</returns>
        /// <cmis>1.0</cmis>
        IList<string> DeleteTree(IObjectId folderId, bool allVersions, UnfileObject? unfile, bool continueOnFailure);

        /// <summary>
        /// Retrieves the main content stream of a document.
        /// </summary>
        /// <param name="docId">the ID of the document</param>
        /// <returns>the content stream or <c>null</c> if the document has no content stream</returns>
        /// <cmis>1.0</cmis>
        IContentStream GetContentStream(IObjectId docId);

        /// <summary>
        /// Retrieves the main content stream of a document.
        /// </summary>
        /// <param name="docId">the ID of the document</param>
        /// <param name="streamId">the stream ID</param>
        /// <param name="offset">the offset of the stream or <c>null</c> to read the stream from the beginning</param>
        /// <param name="length">the maximum length of the stream or <c>null</c> to read to the end of the stream</param>
        /// <returns>the content stream or <c>null</c> if the document has no content stream</returns>
        /// <cmis>1.0</cmis>
        IContentStream GetContentStream(IObjectId docId, string streamId, long? offset, long? length);

        // permissions

        /// <summary>
        /// Gets the ACL of an object.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <param name="onlyBasicPermissions">a flag indicating whether only basic permissions are requested</param>
        /// <returns>the ACL</returns>
        /// <cmis>1.0</cmis>
        IAcl GetAcl(IObjectId objectId, bool onlyBasicPermissions);

        /// <summary>
        /// Applies an ACL.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <param name="addAces">the ACEs to be added</param>
        /// <param name="removeAces">the ACSs to be removed</param>
        /// <param name="aclPropagation">the ACL propagation flag</param>
        /// <returns>the new ACL of the object</returns>
        /// <cmis>1.0</cmis>
        IAcl ApplyAcl(IObjectId objectId, IList<IAce> addAces, IList<IAce> removeAces, AclPropagation? aclPropagation);

        /// <summary>
        /// Applies policies.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <param name="policyIds">the policy IDs</param>
        /// <cmis>1.0</cmis>
        void ApplyPolicy(IObjectId objectId, params IObjectId[] policyIds);

        /// <summary>
        /// Removes policies.
        /// </summary>
        /// <param name="objectId">the object ID</param>
        /// <param name="policyIds">the policy IDs</param>
        /// <cmis>1.0</cmis>
        void RemovePolicy(IObjectId objectId, params IObjectId[] policyIds);
    }

    public interface IObjectFactory
    {
        void Initialize(ISession session, IDictionary<string, string> parameters);

        // Acl and ACE
        IAcl ConvertAces(IList<IAce> aces);
        IAcl CreateAcl(IList<IAce> aces);
        IAce CreateAce(string principal, IList<string> permissions);

        // policies
        IList<string> ConvertPolicies(IList<IPolicy> policies);

        // renditions
        IRendition ConvertRendition(string objectId, IRenditionData rendition);

        // content stream
        IContentStream CreateContentStream(string filename, long length, string mimetype, Stream stream);

        // types
        IObjectType ConvertTypeDefinition(ITypeDefinition typeDefinition);
        IObjectType GetTypeFromObjectData(IObjectData objectData);

        // properties
        IProperty CreateProperty<T>(IPropertyDefinition type, IList<T> values);
        IDictionary<string, IProperty> ConvertProperties(IObjectType objectType, IList<ISecondaryType> secondaryTypes, IProperties properties);
        IProperties ConvertProperties(IDictionary<string, object> properties, IObjectType type, IList<ISecondaryType> secondaryTypes, HashSet<Updatability> updatabilityFilter);
        IList<IPropertyData> ConvertQueryProperties(IProperties properties);

        // objects
        ICmisObject ConvertObject(IObjectData objectData, IOperationContext context);
        IQueryResult ConvertQueryResult(IObjectData objectData);
        IChangeEvent ConvertChangeEvent(IObjectData objectData);
        IChangeEvents ConvertChangeEvents(string changeLogToken, IObjectList objectList);
    }

    /// <summary>
    /// Operation context interface.
    /// </summary>
    public interface IOperationContext
    {
        /// <value>
        /// Gets and sets the property filter.
        /// </value>
        /// <remarks>
        /// This is a set of query names.
        /// </remarks>
        HashSet<string> Filter { get; set; }

        /// <value>
        /// Gets and sets the property filter.
        /// </value>
        /// <remarks>
        /// This is a comma-separated list of query names.
        /// </remarks>
        string FilterString { get; set; }

        /// <value>
        /// Gets and sets if allowable actions should be retrieved.
        /// </value>
        bool IncludeAllowableActions { get; set; }

        /// <value>
        /// Gets and sets if ACLs should be retrieved.
        /// </value>
        bool IncludeAcls { get; set; }

        /// <value>
        /// Gets and sets if relationships should be retrieved.
        /// </value>
        IncludeRelationships? IncludeRelationships { get; set; }

        /// <value>
        /// Gets and sets if policies should be retrieved.
        /// </value>
        bool IncludePolicies { get; set; }

        /// <value>
        /// Gets and sets the rendition filter.
        /// </value>
        /// <remarks>
        /// This is a set of rendition kinds or MIME types.
        /// </remarks>
        HashSet<string> RenditionFilter { get; set; }

        /// <value>
        /// Gets and sets the rendition filter.
        /// </value>
        /// <remarks>
        /// This is a comma-separated list of rendition kinds or MIME types.
        /// </remarks>
        string RenditionFilterString { get; set; }

        /// <value>
        /// Gets and sets if path segments should be retrieved.
        /// </value>
        bool IncludePathSegments { get; set; }

        /// <value>
        /// Gets and sets order by list. 
        /// </value>
        /// <remarks>
        /// This is a comma-separated list of query names.
        /// </remarks>
        string OrderBy { get; set; }

        /// <value>
        /// Gets and sets if object fetched with this <see cref="PortCMIS.Client.IOperationContext"/>
        /// should be cached or not.
        /// </value>
        bool CacheEnabled { get; set; }

        /// <value>
        /// Gets the cache key. (For internal use.)
        /// </value>
        string CacheKey { get; }

        /// <value>
        /// Gets and sets how many items should be fetched per page.
        /// </value>
        int MaxItemsPerPage { get; set; }
    }

    public interface ITree<T>
    {
        /// <value>
        /// The node item.
        /// </value>
        T Item { get; }

        /// <value>
        /// The children of this node.
        /// </value>
        IList<ITree<T>> Children { get; }
    }


    /// <summary>
    /// Query Statement.
    /// </summary>
    /// <remarks>
    /// Sample code:
    /// 
    /// <c>
    /// DateTime cal = ...
    /// Folder folder = ...
    /// 
    /// IQueryStatement qs = 
    ///   session.createQueryStatement("SELECT ?, ? FROM ? WHERE ? > TIMESTAMP ? AND IN_FOLDER(?) OR ? IN (?)");
    /// 
    /// qs.SetProperty(1, "cmis:document", "cmis:name");
    /// qs.SetProperty(2, "cmis:document", "cmis:objectId");
    /// qs.SetType(3, "cmis:document");
    /// 
    /// qs.SetProperty(4, "cmis:document", "cmis:creationDate");
    /// qs.SetDateTime(5, cal);
    /// 
    /// qs.SetId(6, folder);
    /// 
    /// qs.SetProperty(7, "cmis:document", "cmis:createdBy");
    /// qs.SetString(8, "bob", "tom", "lisa"); 
    /// 
    /// string statement = qs.ToQueryString();
    /// </c>
    /// </remarks>
    public interface IQueryStatement
    {
        /// <summary>
        /// Sets the designated parameter to the query name of the given type ID.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='typeId'>the type ID</param>

        void SetType(int parameterIndex, string typeId);

        /// <summary>
        /// Sets the designated parameter to the query name of the given type.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='type'>the object type</param>
        void SetType(int parameterIndex, IObjectType type);

        /// <summary>
        /// Sets the designated parameter to the query name of the given property.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='propertyId'>the property ID</param>
        void SetProperty(int parameterIndex, string typeId, string propertyId);

        /// <summary>
        /// Sets the designated parameter to the query name of the given property.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='propertyDefinition'>the property definition</param>
        void SetProperty(int parameterIndex, IPropertyDefinition propertyDefinition);

        /// <summary>
        /// Sets the designated parameter to the given decimal.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='num'>the number</param>
        void SetInteger(int parameterIndex, params BigInteger[] num);

        /// <summary>
        /// Sets the designated parameter to the given string.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='num'>the number</param>
        void SetDecimal(int parameterIndex, params decimal[] num);

        /// <summary>
        /// Sets the designated parameter to the given string.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='str'>the string</param>
        void SetString(int parameterIndex, params string[] str);

        /// <summary>
        /// Sets the designated parameter to the given string. It does not escape
        /// backslashes ('\') in front of '%' and '_'.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='str'>the LIKE string</param>
        void SetStringLike(int parameterIndex, string str);

        /// <summary>
        /// Sets the designated parameter to the given string in a CMIS contains
        /// statement.
        /// </summary>S
        /// <remarks>
        /// Note that the CMIS specification requires two levels of escaping. The
        /// first level escapes ', ", \ characters to \', \" and \\. The characters
        /// *, ? and - are interpreted as text search operators and are not escaped
        /// on first level. If *, ?, - shall be used as literals, they must be passed
        /// escaped with \*, \? and \- to this method.
        /// <p>
        /// For all statements in a CONTAINS() clause it is required to isolate those
        /// from a query statement. Therefore a second level escaping is performed.
        /// On the second level grammar ", ', - and \ are escaped with a \. See the
        /// spec for further details.
        /// <p>
        /// Summary (input --> first level escaping --> second level escaping and
        /// output):/// -->/// -->/// ? --> ? --> ? - --> - --> - \ --> \\ --> \\\\ (for
        /// any other character following other than/// ? -) \* --> \* --> \\* \? -->
        /// \? --> \\? \- --> \- --> \\- ' --> \' --> \\\' " --> \" --> \\\"
        /// </remarks>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='str'>the CONTAINS string</param>
        void SetStringContains(int parameterIndex, string str);

        /// <summary>
        /// Sets the designated parameter to the given object ID.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='id'>the object ID</param>
        void SetId(int parameterIndex, params IObjectId[] id);

        /// <summary>
        /// Sets the designated parameter to the given URI.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='uri'>the URI</param>
        void SetUri(int parameterIndex, params Uri[] uri);

        /// <summary>
        /// Sets the designated parameter to the given boolean.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='bool'>the boolean</param>
        void SetBoolean(int parameterIndex, params bool[] boolean);

        /// <summary>
        /// Sets the designated parameter to the given DateTime value.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='cal'>the DateTime value as Calendar object</param>
        void SetDateTime(int parameterIndex, params DateTime[] dt);

        /// <summary>
        /// Sets the designated parameter to the given DateTime value.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='cal'>the DateTime value in milliseconds from midnight, January 1, 1970 UTC.</param>
        void SetDateTime(int parameterIndex, params long[] ms);

        /// <summary>
        /// Sets the designated parameter to the given DateTime value with the prefix
        /// 'TIMESTAMP '.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='cal'>the DateTime value as Calendar object
        void SetDateTimeTimestamp(int parameterIndex, params DateTime[] dt);

        /// <summary>
        /// Sets the designated parameter to the given DateTime value with the prefix
        /// 'TIMESTAMP '.
        /// </summary>
        /// <param name='parameterIndex'>the parameter index (one-based)</param>
        /// <param name='cal'>the DateTime value in milliseconds from midnight, January 1, 1970 UTC.</param>
        void SetDateTimeTimestamp(int parameterIndex, params long[] ms);

        /// <summary>
        /// Returns the query statement.
        /// </summary>
        /// <return>the query statement, not null</return>
        string ToQueryString();

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name='searchAllVersions'>
        ///            true if all document versions should be included in
        ///            the search results, false if only the latest document
        ///            versions should be included in the search results</param>
        IItemEnumerable<IQueryResult> Query(bool searchAllVersions);

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name='searchAllVersions'>
        ///            true if all document versions should be included in
        ///            the search results, false if only the latest document
        ///            versions should be included in the search results</param>
        /// <param name='context'>the operation context to use</param>
        IItemEnumerable<IQueryResult> Query(bool searchAllVersions, IOperationContext context);
    }

    /// <summary>
    /// Base interface for all CMIS types.
    /// </summary>
    public interface IObjectType : ITypeDefinition
    {
        /// <value>
        /// The base type.
        /// </value>
        bool IsBaseType { get; }
        IObjectType GetBaseType();
        IObjectType GetParentType();
        IItemEnumerable<IObjectType> GetChildren();
        IList<ITree<IObjectType>> GetDescendants(int depth);
    }

    /// <summary>
    /// Document type interface.
    /// </summary>
    public interface IDocumentType : IObjectType
    {
        /// <value>
        /// The versionable flag.
        /// </value>
        bool? IsVersionable { get; }

        /// <value>
        /// Content stream allowed.
        /// </value>
        ContentStreamAllowed? ContentStreamAllowed { get; }
    }

    /// <summary>
    /// Folder type interface.
    /// </summary>
    public interface IFolderType : IObjectType
    {
    }

    public interface ISecondaryType : IObjectType
    {
    }

    /// <summary>
    /// Relationship type interface.
    /// </summary>
    public interface IRelationshipType : IObjectType
    {
        IList<IObjectType> GetAllowedSourceTypes { get; }
        IList<IObjectType> GetAllowedTargetTypes { get; }
    }

    /// <summary>
    /// Policy type interface.
    /// </summary>
    public interface IPolicyType : IObjectType
    {
    }

    /// <summary>
    /// Item type interface.
    /// </summary>
    public interface IItemType : IObjectType
    {
    }

    public interface IItemEnumerable<T> : IEnumerable<T>
    {
        IItemEnumerable<T> SkipTo(BigInteger position);
        IItemEnumerable<T> GetPage();
        IItemEnumerable<T> GetPage(int maxNumItems);
        BigInteger PageNumItems { get; }
        bool HasMoreItems { get; }
        BigInteger TotalNumItems { get; }
    }

    public interface IObjectId
    {
        /// <value>
        /// The object ID.
        /// </value>
        string Id { get; }
    }

    public interface IRendition : IRenditionData
    {
        IDocument GetRenditionDocument();
        IDocument GetRenditionDocument(IOperationContext context);
        IContentStream GetContentStream();
    }

    /// <summary>
    /// Property interface.
    /// </summary>
    public interface IProperty
    {
        /// <value>
        /// The property ID.
        /// </value>
        string Id { get; }

        /// <value>
        /// The property local name.
        /// </value>
        string LocalName { get; }

        /// <value>
        /// Property display name.
        /// </value>
        string DisplayName { get; }

        /// <value>
        /// Property query name.
        /// </value>
        string QueryName { get; }

        /// <value>
        /// Gets if the property is a multi-value property.
        /// </value>
        bool IsMultiValued { get; }

        /// <value>
        /// The property type.
        /// </value>
        PropertyType? PropertyType { get; }

        /// <value>
        /// The property definition.
        /// </value>
        IPropertyDefinition PropertyDefinition { get; }

        /// <value>
        /// The property value.
        /// </value>
        /// <remarks>
        /// If the property is a single-value property the single value is returned.
        /// If the property is a multi-value property a IList&lt;object&gt; is returned.
        /// </remarks>
        object Value { get; }

        /// <value>
        /// The value list of the property.
        /// </value>
        /// <remarks>
        /// If the property is a single-value property a list with one or no items is returned.
        /// </remarks>
        IList<object> Values { get; }

        /// <value>
        /// The first value of the value list or <c>null</c> if the list has no values.
        /// </value>
        object FirstValue { get; }

        /// <value>
        /// A string representation of the first value of the value list.
        /// </value>
        string ValueAsString { get; }

        /// <value>
        /// A string representation of the value list.
        /// </value>
        string ValuesAsString { get; }
    }

    /// <summary>
    /// Collection of common CMIS properties.
    /// </summary>
    public interface ICmisObjectProperties
    {
        /// <value>
        /// List of all available CMIS properties.
        /// </value>
        IList<IProperty> Properties { get; }

        /// <value>
        /// Gets a property by property ID.
        /// </value>
        /// <param name="propertyId">the property ID</param>
        /// <returns>the property or <c>null</c> if the property is not available</returns>
        IProperty this[string propertyId] { get; }

        /// <summary>
        /// Gets the value of the requested property.
        /// </summary>
        /// <param name="propertyId">the property ID</param>
        /// <returns>the property value or <c>null</c> if the property is not available or not set</returns>
        object GetPropertyValue(string propertyId);

        /// <summary>
        /// Gets the value of the requested property and converts it to string.
        /// </summary>
        /// <param name="propertyId">the property ID</param>
        /// <returns>the property value or <c>null</c> if the property is not available or not set</returns>
        string GetPropertyAsStringValue(string propertyId);

        /// <summary>
        /// Gets the value of the requested property and converts it to long value.
        /// </summary>
        /// <param name="propertyId">the property ID</param>
        /// <returns>the property value or <c>null</c> if the property is not available or not set</returns>
        /// <exception cref="FormatException">value is not in an appropriate format</exception>
        /// <exception cref="InvalidCastException">the conversion is not supported</exception>
        /// <exception cref="OverflowException">value represents a number that is less than Int64.MinValue or greater than Int64.MaxValue</exception>
        long? GetPropertyAsLongValue(string propertyId);

        /// <summary>
        /// Gets the value of the requested property and converts it to bool value.
        /// </summary>
        /// <param name="propertyId">the property ID</param>
        /// <returns>the property value or <c>null</c> if the property is not available or not set</returns>
        /// <exception cref="FormatException">value is not in an appropriate format</exception>
        /// <exception cref="InvalidCastException">the conversion is not supported</exception>
        bool? GetPropertyAsBoolValue(string propertyId);

        /// <summary>
        /// Gets the value of the requested property and converts it to DateTime value.
        /// </summary>
        /// <param name="propertyId">the property ID</param>
        /// <returns>the property value or <c>null</c> if the property is not available or not set</returns>
        /// <exception cref="FormatException">value is not in an appropriate format</exception>
        /// <exception cref="InvalidCastException">the conversion is not supported</exception>
        DateTime? GetPropertyAsDateTimeValue(string propertyId);

        /// <value>
        /// The name of this CMIS object (CMIS property <c>cmis:name</c>).
        /// </value>
        string Name { get; }

        /// <value>
        /// The user who created this CMIS object (CMIS property <c>cmis:createdBy</c>).
        /// </value>
        string CreatedBy { get; }

        /// <value>
        /// The timestamp when this CMIS object has been created (CMIS property <c>cmis:creationDate</c>).
        /// </value>
        DateTime? CreationDate { get; }

        /// <value>
        /// The user who modified this CMIS object (CMIS property <c>cmis:lastModifiedBy</c>).
        /// </value>
        string LastModifiedBy { get; }

        /// <value>
        /// The timestamp when this CMIS object has been modified (CMIS property <c>cmis:lastModificationDate</c>).
        /// </value>
        DateTime? LastModificationDate { get; }

        /// <value>
        /// The ID of the base type of this CMIS object (CMIS property <c>cmis:baseTypeId</c>).
        /// </value>
        BaseTypeId BaseTypeId { get; }

        /// <value>
        /// The base type of this CMIS object (object type identified by <c>cmis:baseTypeId</c>).
        /// </value>
        IObjectType BaseType { get; }

        /// <value>
        /// The type of this CMIS object (object type identified by <c>cmis:objectTypeId</c>).
        /// </value>
        IObjectType ObjectType { get; }

        /// <value>
        /// The change token (CMIS property <c>cmis:changeToken</c>).
        /// </value>
        string ChangeToken { get; }

        /// <value>
        /// The secondary types.
        /// </value>
        IList<ISecondaryType> SecondaryTypes { get; }
    }

    /// <summary>
    /// Extension level.
    /// </summary>
    public enum ExtensionLevel
    {
        /// <summary>
        /// Object extensions.
        /// </summary>
        Object,
        /// <summary>
        /// Properties extensions.
        /// </summary>
        Properties,
        /// <summary>
        /// Allowable Actions extensions.
        /// </summary>
        AllowableActions,
        /// <summary>
        /// ACL extensions.
        /// </summary>
        Acl,
        /// <summary>
        /// Policies extensions.
        /// </summary>
        Policies,
        /// <summary>
        /// Change Event extensions.
        /// </summary>
        ChangeEvent
    }

    /// <summary>
    /// Base interface for all CMIS objects.
    /// </summary>
    public interface ICmisObject : IObjectId, ICmisObjectProperties
    {
        /// <value>
        /// The allowable actions if they have been fetched for this object.
        /// </value>
        IAllowableActions AllowableActions { get; }

        /// <value>
        /// The relationships if they have been fetched for this object.
        /// </value>
        IList<IRelationship> Relationships { get; }

        /// <value>
        /// The ACL if it has been fetched for this object.
        /// </value>
        IAcl Acl { get; }

        /// <summary>
        /// Deletes this object.
        /// </summary>
        /// <param name="allVersions">if this object is a document this parameter defines if just this version or all versions should be deleted</param>
        void Delete(bool allVersions);

        /// <summary>
        /// Updates the properties that are provided.
        /// </summary>
        /// <param name="properties">the properties to update</param>
        /// <returns>the updated object (a repository might have created a new object)</returns>
        ICmisObject UpdateProperties(IDictionary<string, object> properties);

        /// <summary>
        /// Updates the properties that are provided.
        /// </summary>
        /// <param name="properties">the properties to update</param>
        /// <param name="refresh">indicates if the object should be refresh after the update</param>
        /// <returns>the object ID of the updated object (a repository might have created a new object)</returns>
        IObjectId UpdateProperties(IDictionary<string, object> properties, bool refresh);

        /// <summary>
        /// Renames the object.
        /// </summary>
        /// <param name="newName">the new name</param>
        /// <returns>the updated object (a repository might have created a new object)</returns>
        ICmisObject Rename(string newName);

        /// <summary>
        /// Renames the object.
        /// </summary>
        /// <param name="newName">the new name</param>
        /// <param name="refresh">indicates if the object should be refresh after the update</param>
        /// <returns>the object ID of the updated object (a repository might have created a new object)</returns>
        IObjectId Rename(string newName, bool refresh);

        /// <summary>
        /// Gets the renditions if they have been fetched for this object.
        /// </summary>
        IList<IRendition> Renditions { get; }

        /// <summary>
        /// Applies the given policies to the object.
        /// </summary>
        void ApplyPolicy(params IObjectId[] policyId);

        /// <summary>
        /// Removes the given policies from the object.
        /// </summary>
        void RemovePolicy(params IObjectId[] policyId);

        /// <summary>
        /// Gets a list of policies applied to this object.
        /// </summary>
        IList<IPolicy> Policies { get; }

        /// <summary>
        /// Adds and removes ACEs to this object.
        /// </summary>
        /// <returns>the new ACL of this object</returns>
        IAcl ApplyAcl(IList<IAce> addAces, IList<IAce> removeAces, AclPropagation? aclPropagation);

        /// <summary>
        /// Adds ACEs to this object.
        /// </summary>
        /// <returns>the new ACL of this object</returns>
        IAcl AddAcl(IList<IAce> addAces, AclPropagation? aclPropagation);

        /// <summary>
        /// Removes ACEs from this object.
        /// </summary>
        /// <returns>the new ACL of this object</returns>
        IAcl RemoveAcl(IList<IAce> removeAces, AclPropagation? aclPropagation);

        /// <summary>
        /// Gets the extensions of the given level.
        /// </summary>
        IList<ICmisExtensionElement> GetExtensions(ExtensionLevel level);

        /// <value>
        /// The timestamp of the last refresh.
        /// </value>
        DateTime RefreshTimestamp { get; }

        /// <summary>
        /// Reloads the data from the repository.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Reloads the data from the repository if the last refresh did not occur within <c>durationInMillis</c>.
        /// </summary>
        void RefreshIfOld(long durationInMillis);
    }

    /// <summary>
    /// Base interface for all fileable CMIS objects.
    /// </summary>
    public interface IFileableCmisObject : ICmisObject
    {
        /// <summary>
        /// Moves this object from a source folder to a target folder.
        /// </summary>
        /// <param name="sourceFolderId">the source folder ID</param>
        /// <param name="targetFolderId">the target folder ID</param>
        /// <returns>the object in the new location</returns>
        IFileableCmisObject Move(IObjectId sourceFolderId, IObjectId targetFolderId);

        /// <value>
        /// Gets a list of all parent folders. 
        /// </value>
        /// <remarks>
        /// Returns an empty list if it is an unfiled object or the root folder.
        /// </remarks>
        IList<IFolder> Parents { get; }

        /// <value>
        /// All paths for this object
        /// </value>
        /// <remarks>
        /// Returns an empty list for unfiled objects.
        /// </remarks>
        IList<string> Paths { get; }

        /// <summary>
        /// Adds this object to the given folder.
        /// </summary>
        /// <param name="folderId">the ID of the target folder</param>
        /// <param name="allVersions">indicates if only this object or all versions of the object should be added</param>
        void AddToFolder(IObjectId folderId, bool allVersions);

        /// <summary>
        /// Removes this object from the given folder.
        /// </summary>
        /// <param name="folderId">the ID of the folder</param>
        void RemoveFromFolder(IObjectId folderId);
    }

    /// <summary>
    /// Document properties.
    /// </summary>
    public interface IDocumentProperties
    {
        /// <value>
        /// Gets if this CMIS object is immutable (CMIS property <c>cmis:isImmutable</c>).
        /// </value>
        bool? IsImmutable { get; }

        /// <value>
        /// Gets if this CMIS object is the latest version (CMIS property <c>cmis:isLatestVersion</c>)
        /// </value>
        bool? IsLatestVersion { get; }

        /// <value>
        /// Gets if this CMIS object is the latest version (CMIS property <c>cmis:isMajorVersion</c>).
        /// </value>
        bool? IsMajorVersion { get; }

        /// <value>
        /// Gets if this CMIS object is the latest major version (CMIS property <c>cmis:isLatestMajorVersion</c>).
        /// </value>
        bool? IsLatestMajorVersion { get; }

        /// <value>
        /// Gets the version label (CMIS property <c>cmis:versionLabel</c>).
        /// </value>
        string VersionLabel { get; }

        /// <value>
        /// Gets the version series ID (CMIS property <c>cmis:versionSeriesId</c>).
        /// </value>
        string VersionSeriesId { get; }

        /// <value>
        /// Gets if this version series is checked out (CMIS property <c>cmis:isVersionSeriesCheckedOut</c>).
        /// </value>
        bool? IsVersionSeriesCheckedOut { get; }

        /// <value>
        /// Gets the user who checked out this version series (CMIS property <c>cmis:versionSeriesCheckedOutBy</c>).
        /// </value>
        string VersionSeriesCheckedOutBy { get; }

        /// <value>
        /// Gets the PWC ID of this version series (CMIS property <c>cmis:versionSeriesCheckedOutId</c>).
        /// </value>
        string VersionSeriesCheckedOutId { get; }

        /// <value>
        /// Gets the checkin comment (CMIS property <c>cmis:checkinComment</c>).
        /// </value>
        string CheckinComment { get; }

        /// <value>
        /// The content stream length or <c>null</c> if the document has no content (CMIS property <c>cmis:contentStreamLength</c>).
        /// </value>
        long? ContentStreamLength { get; }

        /// <value>
        /// The content stream MIME type or <c>null</c> if the document has no content (CMIS property <c>cmis:contentStreamMimeType</c>).
        /// </value>
        string ContentStreamMimeType { get; }

        /// <value>
        /// The content stream filename or <c>null</c> if the document has no content (CMIS property <c>cmis:contentStreamFileName</c>).
        /// </value>
        string ContentStreamFileName { get; }

        /// <value>
        /// The content stream ID or <c>null</c> if the document has no content (CMIS property <c>cmis:contentStreamId</c>).
        /// </value>
        string ContentStreamId { get; }

        /// <value>
        /// The content stream hashes or <c>null</c> if the document has no content or the repository hasn't provided content hashes (CMIS property <c>cmis:contentStreamHash</c>).
        /// </value>
        IList<IContentStreamHash> ContentStreamHashes { get; }
    }

    /// <summary>
    /// Document interface.
    /// </summary>
    public interface IDocument : IFileableCmisObject, IDocumentProperties
    {
        /// <summary>
        /// Deletes all versions of this document.
        /// </summary>
        void DeleteAllVersions();

        /// <summary>
        /// Gets the content stream of this document.
        /// </summary>
        /// <returns>the content stream or <c>null</c> if the document has no content</returns>
        IContentStream GetContentStream();

        /// <summary>
        /// Gets the content stream identified by the given stream ID.
        /// </summary>
        /// <returns>the content stream or <c>null</c> if the stream ID is not associated with content</returns>
        IContentStream GetContentStream(string streamId);

        /// <summary>
        /// Gets the content stream identified by the given stream ID with the given offset and length.
        /// </summary>
        /// <returns>the content stream or <c>null</c> if the stream ID is not associated with content</returns>
        IContentStream GetContentStream(string streamId, long? offset, long? length);

        /// <summary>
        /// Sets a new content stream for this document.
        /// </summary>
        /// <param name="contentStream">the content stream</param>
        /// <param name="overwrite">indicates if the current stream should be overwritten</param>
        /// <returns>the new document object</returns>
        /// <remarks>
        /// Repositories might create a new version if the content is updated.
        /// </remarks>
        IDocument SetContentStream(IContentStream contentStream, bool overwrite);

        /// <summary>
        /// Sets a new content stream for this document.
        /// </summary>
        /// <param name="contentStream">the content stream</param>
        /// <param name="overwrite">indicates if the current stream should be overwritten</param>
        /// <param name="refresh">indicates if this object should be refreshed after the new content is set</param>
        /// <returns>the new document object ID</returns>
        /// <remarks>
        /// Repositories might create a new version if the content is updated.
        /// </remarks>
        IObjectId SetContentStream(IContentStream contentStream, bool overwrite, bool refresh);

        /// <summary>
        /// Appends a content stream for this document.
        /// </summary>
        /// <param name="contentStream">the content stream</param>
        /// <param name="isLastChunk">indicates if the current stream should be the last trunk</param>
        /// <returns>the new document object</returns>
        /// <remarks>
        /// Repositories might create a new version if the content is appended.
        /// </remarks>
        IDocument AppendContentStream(IContentStream contentStream, bool isLastChunk);

        /// <summary>
        /// Appends a content stream for this document.
        /// </summary>
        /// <param name="contentStream">the content stream</param>
        /// <param name="isLastChunk">indicates if the current stream should be the last trunk</param>
        /// <param name="refresh">indicates if this object should be refreshed after the new content is set</param>
        /// <returns>the new document object ID</returns>
        /// <remarks>
        /// Repositories might create a new version if the content is appended.
        /// </remarks>
        IObjectId AppendContentStream(IContentStream contentStream, bool isLastChunk, bool refresh);

        /// <summary>
        /// Deletes the current content stream for this document.
        /// </summary>
        /// <returns>the new document object</returns>
        /// <remarks>
        /// Repositories might create a new version if the content is deleted.
        /// </remarks>
        IDocument DeleteContentStream();

        /// <summary>
        /// Deletes the current content stream for this document.
        /// </summary>
        /// <param name="refresh">indicates if this object should be refreshed after the content is deleted</param>
        /// <returns>the new document object ID</returns>
        /// <remarks>
        /// Repositories might create a new version if the content is deleted.
        /// </remarks>
        IObjectId DeleteContentStream(bool refresh);

        /// <summary>
        /// Checks out this document.
        /// </summary>
        /// <returns>the object ID of the newly created private working copy (PWC).</returns>
        IObjectId CheckOut();

        /// <summary>
        /// Cancels the check out.
        /// </summary>
        void CancelCheckOut();

        /// <summary>
        /// Checks in this private working copy (PWC).
        /// </summary>
        /// <returns>the object ID of the new created document</returns>
        IObjectId CheckIn(bool major, IDictionary<string, object> properties, IContentStream contentStream, string checkinComment,
            IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces);

        /// <summary>
        /// Checks in this private working copy (PWC).
        /// </summary>
        /// <returns>the object ID of the new created document</returns>
        IObjectId CheckIn(bool major, IDictionary<string, object> properties, IContentStream contentStream, string checkinComment);

        IDocument GetObjectOfLatestVersion(bool major);
        IDocument GetObjectOfLatestVersion(bool major, IOperationContext context);

        /// <summary>
        /// Gets a list of all versions in this version series.
        /// </summary>
        IList<IDocument> GetAllVersions();

        /// <summary>
        /// Gets a list of all versions in this version series using the given <see cref="PortCMIS.Client.IOperationContext"/>.
        /// </summary>
        IList<IDocument> GetAllVersions(IOperationContext context);

        IDocument Copy(IObjectId targetFolderId);
        IDocument Copy(IObjectId targetFolderId, IDictionary<string, object> properties, VersioningState? versioningState,
            IList<IPolicy> policies, IList<IAce> addACEs, IList<IAce> removeACEs, IOperationContext context);
    }

    /// <summary>
    /// Folder properties.
    /// </summary>
    public interface IFolderProperties
    {
        /// <value>
        /// The parent ID.
        /// </value>
        string ParentId { get; }

        /// <value>
        /// The list of allowable child object types.
        /// </value>
        IList<IObjectType> AllowedChildObjectTypes { get; }
    }

    /// <summary>
    /// Folder interface.
    /// </summary>
    public interface IFolder : IFileableCmisObject, IFolderProperties
    {
        IDocument CreateDocument(IDictionary<string, object> properties, IContentStream contentStream, VersioningState? versioningState,
                IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context);
        IDocument CreateDocument(IDictionary<string, object> properties, IContentStream contentStream, VersioningState? versioningState);
        IDocument CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, VersioningState? versioningState,
                IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context);
        IDocument CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, VersioningState? versioningState);
        IFolder CreateFolder(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces,
                IOperationContext context);
        IFolder CreateFolder(IDictionary<string, object> properties);
        IPolicy CreatePolicy(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces,
                IOperationContext context);
        IPolicy CreatePolicy(IDictionary<string, object> properties);
        IItem CreateItem(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces,
            IOperationContext context);
        IItem CreateItem(IDictionary<string, object> properties);
        IList<string> DeleteTree(bool allversions, UnfileObject? unfile, bool continueOnFailure);

        /// <summary>
        /// Gets the folder tress of this folder (only folder).
        /// </summary>
        /// <param name="depth">the depth</param>
        /// <returns>a list of folder trees</returns>
        /// <remarks>
        /// If depth == 1 only objects that are children of this folder are returned.
        /// If depth &gt; 1 only objects that are children of this folder and descendants up to "depth" levels deep are returned.
        /// If depth == -1 all descendant objects at all depth levels in the CMIS hierarchy are returned.
        /// </remarks>
        IList<ITree<IFileableCmisObject>> GetFolderTree(int depth);

        /// <summary>
        /// Gets the folder tress of this folder (only folder) using the given <see cref="PortCMIS.Client.IOperationContext"/>.
        /// </summary>
        /// <param name="depth">the depth</param>
        /// <param name="context">the <see cref="PortCMIS.Client.IOperationContext"/></param>
        /// <returns>a list of folder trees</returns>
        /// <remarks>
        /// If depth == 1 only objects that are children of this folder are returned.
        /// If depth &gt; 1 only objects that are children of this folder and descendants up to "depth" levels deep are returned.
        /// If depth == -1 all descendant objects at all depth levels in the CMIS hierarchy are returned.
        /// </remarks>
        IList<ITree<IFileableCmisObject>> GetFolderTree(int depth, IOperationContext context);

        /// <summary>
        /// Gets the descendants of this folder (all filable objects).
        /// </summary>
        /// <param name="depth">the depth</param>
        /// <returns>a list of descendant trees</returns>
        /// <remarks>
        /// If depth == 1 only objects that are children of this folder are returned.
        /// If depth &gt; 1 only objects that are children of this folder and descendants up to "depth" levels deep are returned.
        /// If depth == -1 all descendant objects at all depth levels in the CMIS hierarchy are returned.
        /// </remarks>
        IList<ITree<IFileableCmisObject>> GetDescendants(int depth);

        /// <summary>
        /// Gets the descendants of this folder (all filable objects) using the given <see cref="PortCMIS.Client.IOperationContext"/>.
        /// </summary>
        /// <param name="depth">the depth</param>
        /// <param name="context">the <see cref="PortCMIS.Client.IOperationContext"/></param>
        /// <returns>a list of descendant trees</returns>
        /// <remarks>
        /// If depth == 1 only objects that are children of this folder are returned.
        /// If depth &gt; 1 only objects that are children of this folder and descendants up to "depth" levels deep are returned.
        /// If depth == -1 all descendant objects at all depth levels in the CMIS hierarchy are returned.
        /// </remarks>
        IList<ITree<IFileableCmisObject>> GetDescendants(int depth, IOperationContext context);

        /// <summary>
        /// Gets the children of this folder.
        /// </summary>
        IItemEnumerable<ICmisObject> GetChildren();

        /// <summary>
        /// Gets the children of this folder using the given <see cref="PortCMIS.Client.IOperationContext"/>.
        /// </summary>
        IItemEnumerable<ICmisObject> GetChildren(IOperationContext context);

        /// <value>
        /// Gets if this folder is the root folder.
        /// </value>
        bool IsRootFolder { get; }

        /// <value>
        /// The parent of this folder or <c>null</c> if this folder is the root folder.
        /// </value>
        IFolder FolderParent { get; }

        /// <value>
        /// The path of this folder.
        /// </value>
        string Path { get; }

        IItemEnumerable<IDocument> GetCheckedOutDocs();
        IItemEnumerable<IDocument> GetCheckedOutDocs(IOperationContext context);
    }

    /// <summary>
    /// Policy properties.
    /// </summary>
    public interface IPolicyProperties
    {
        /// <value>
        /// The policy text of this CMIS policy (CMIS property <c>cmis:policyText</c>).
        /// </value>
        string PolicyText { get; }
    }

    /// <summary>
    /// Policy interface.
    /// </summary>
    public interface IPolicy : IFileableCmisObject, IPolicyProperties
    {
    }

    /// <summary>
    /// Relationship properties.
    /// </summary>
    public interface IRelationshipProperties
    {
        /// <value>
        /// The ID of the relationship source object.
        /// </value>
        IObjectId SourceId { get; }

        /// <value>
        /// The ID of the relationships target object.
        /// </value>
        IObjectId TargetId { get; }
    }

    /// <summary>
    /// Relationship interface.
    /// </summary>
    public interface IRelationship : ICmisObject, IRelationshipProperties
    {
        /// <summary>
        /// Gets the relationship source object.
        /// </summary>
        /// <remarks>
        /// If the source object ID is invalid, <c>null</c> will be returned.
        /// </remarks>
        ICmisObject GetSource();

        /// <summary>
        /// Gets the relationship source object using the given <see cref="PortCMIS.Client.IOperationContext"/>.
        /// </summary>
        /// <remarks>
        /// If the source object ID is invalid, <c>null</c> will be returned.
        /// </remarks>
        ICmisObject GetSource(IOperationContext context);

        /// <summary>
        /// Gets the relationship target object.
        /// </summary>
        /// <remarks>
        /// If the target object ID is invalid, <c>null</c> will be returned.
        /// </remarks>
        ICmisObject GetTarget();

        /// <summary>
        /// Gets the relationship target object using the given <see cref="PortCMIS.Client.IOperationContext"/>.
        /// </summary>
        /// <remarks>
        /// If the target object ID is invalid, <c>null</c> will be returned.
        /// </remarks>
        ICmisObject GetTarget(IOperationContext context);
    }

    /// <summary>
    /// Item properties.
    /// </summary>
    public interface ItemProperties
    {
    }

    /// <summary>
    /// Item interface.
    /// </summary>
    public interface IItem : IFileableCmisObject, ItemProperties
    {
    }

    /// <summary>
    /// Content hash.
    /// </summary>
    public interface IContentStreamHash
    {
        /// <value>
        /// The content stream hash property value.
        /// </value>
        string PropertyValue { get; }

        /// <value>
        /// The hash algorithm.
        /// </value>
        string Algorithm { get; }

        /// <value>
        /// The hash value.
        /// </value>
        string Hash { get; }
    }

    /// <summary>
    /// Query result.
    /// </summary>
    public interface IQueryResult
    {
        /// <value>
        /// Gets the property.
        /// </value>
        /// <param name="queryName">the propertys query name or alias</param>
        IPropertyData this[string queryName] { get; }

        /// <value>
        /// List of all properties in this query result.
        /// </value>
        IList<IPropertyData> Properties { get; }

        /// <summary>
        /// Returns a property by ID.
        /// </summary>
        /// <param name="propertyId">the property ID</param>
        /// <remarks>
        /// Since repositories are not obligated to add property IDs to their
        /// query result properties, this method might not always work as expected with
        /// some repositories. Use <see cref="P:this[string]"/> instead.
        /// </remarks>
        IPropertyData GetPropertyById(string propertyId);

        /// <summary>
        /// Gets the property (single) value by query name or alias.
        /// </summary>
        object GetPropertyValueByQueryName(string queryName);

        /// <summary>
        /// Gets the property (single) value by property ID.
        /// </summary>
        object GetPropertyValueById(string propertyId);

        /// <summary>
        /// Gets the property value by query name or alias.
        /// </summary>
        IList<object> GetPropertyMultivalueByQueryName(string queryName);

        /// <summary>
        /// Gets the property value by property ID.
        /// </summary>
        IList<object> GetPropertyMultivalueById(string propertyId);

        /// <value>
        /// The allowable actions if they were requested.
        /// </value>
        IAllowableActions AllowableActions { get; }

        /// <value>
        /// The relationships if they were requested.
        /// </value>
        IList<IRelationship> Relationships { get; }

        /// <value>
        /// The renditions if they were requested.
        /// </value>
        IList<IRendition> Renditions { get; }
    }

    /// <summary>
    /// A change event.
    /// </summary>
    public interface IChangeEvent : IChangeEventInfo
    {
        /// <value>
        /// The object ID.
        /// </value>
        string ObjectId { get; }

        /// <value>
        /// The object properties, if provided.
        /// </value>
        IDictionary<string, IList<object>> Properties { get; }

        /// <value>
        /// The policy IDs, if provided.
        /// </value>
        IList<string> PolicyIds { get; }

        /// <value>
        /// The ACL, if provided.
        /// </value>
        IAcl Acl { get; }
    }

    /// <summary>
    /// A collection of change events.
    /// </summary>
    public interface IChangeEvents
    {
        /// <value>
        /// The latest change log token.
        /// </value>
        string LatestChangeLogToken { get; }

        /// <value>
        /// The list of change events.
        /// </value>
        IList<IChangeEvent> ChangeEventList { get; }

        /// <value>
        /// The hasMoreItems flag, if provided.
        /// </value>
        bool? HasMoreItems { get; }

        /// <value>
        /// The total number of change events, if provided.
        /// </value>
        BigInteger? TotalNumItems { get; }
    }
}
