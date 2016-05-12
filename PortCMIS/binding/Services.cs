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

using PortCMIS.Data;
using PortCMIS.Data.Extensions;
using PortCMIS.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace PortCMIS.Binding.Services
{
    /// <summary>
    /// Repository Service interface.
    /// </summary>
    public interface IRepositoryService
    {
        /// <summary>
        /// Returns a list of CMIS repository information available from this CMIS service endpoint.
        /// </summary>
        /// <remarks>
        /// In contrast to the CMIS specification this method returns repository infos not only repository IDs.
        /// </remarks>
        IList<IRepositoryInfo> GetRepositoryInfos(IExtensionsData extension);

        /// <summary>
        /// Returns information about the CMIS repository, the optional capabilities it supports and its
        /// access control information if applicable.
        /// </summary>
        IRepositoryInfo GetRepositoryInfo(string repositoryId, IExtensionsData extension);

        /// <summary>
        /// Returns the list of object types defined for the repository that are children of the specified type.
        /// </summary>
        ITypeDefinitionList GetTypeChildren(string repositoryId, string typeId, bool? includePropertyDefinitions,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension);

        /// <summary>
        /// Returns the set of descendant object type defined for the repository under the specified type.
        /// </summary>
        IList<ITypeDefinitionContainer> GetTypeDescendants(string repositoryId, string typeId, BigInteger? depth,
            bool? includePropertyDefinitions, IExtensionsData extension);

        /// <summary>
        /// Gets the definition of the specified object type.
        /// </summary>
        ITypeDefinition GetTypeDefinition(string repositoryId, string typeId, IExtensionsData extension);

        /// <summary>
        /// Creates a new type.
        /// </summary>
        ITypeDefinition CreateType(string repositoryId, ITypeDefinition type, IExtensionsData extension);

        /// <summary>
        /// Updates a type.
        /// </summary>
        ITypeDefinition UpdateType(string repositoryId, ITypeDefinition type, IExtensionsData extension);

        /// <summary>
        /// Deletes a type.
        /// </summary>
        void DeleteType(string repositoryId, string typeId, IExtensionsData extension);
    }

    /// <summary>
    /// Navigation Service interface.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Gets the list of child objects contained in the specified folder.
        /// </summary>
        IObjectInFolderList GetChildren(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension);

        /// <summary>
        /// Gets the set of descendant objects contained in the specified folder or any of its child folders.
        /// </summary>
        IList<IObjectInFolderContainer> GetDescendants(string repositoryId, string folderId, BigInteger? depth, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension);

        /// <summary>
        ///  Gets the set of descendant folder objects contained in the specified folder.
        /// </summary>
        IList<IObjectInFolderContainer> GetFolderTree(string repositoryId, string folderId, BigInteger? depth, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includePathSegment, IExtensionsData extension);

        /// <summary>
        /// Gets the parent folder(s) for the specified non-folder, fileable object.
        /// </summary>
        IList<IObjectParentData> GetObjectParents(string repositoryId, string objectId, string filter,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            bool? includeRelativePathSegment, IExtensionsData extension);

        /// <summary>
        /// Gets the parent folder object for the specified folder object.
        /// </summary>
        IObjectData GetFolderParent(string repositoryId, string folderId, string filter, IExtensionsData extension);

        /// <summary>
        /// Gets the list of documents that are checked out that the user has access to.
        /// </summary>
        IObjectList GetCheckedOutDocs(string repositoryId, string folderId, string filter, string orderBy,
            bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension);
    }

    /// <summary>
    /// Object Service interface.
    /// </summary>
    public interface IObjectService
    {
        /// <summary>
        /// Creates a document object of the specified type (given by the  cmis:objectTypeId property) in the (optionally) specified location.
        /// </summary>
        string CreateDocument(string repositoryId, IProperties properties, string folderId, IContentStream contentStream,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension);

        /// <summary>
        /// Creates a document object as a copy of the given source document in the (optionally) specified location.
        /// </summary>
        string CreateDocumentFromSource(string repositoryId, string sourceId, IProperties properties, string folderId,
            VersioningState? versioningState, IList<string> policies, IAcl addAces, IAcl removeAces, IExtensionsData extension);

        /// <summary>
        /// Creates a folder object of the specified type (given by the cmis:objectTypeId property) in the specified location.
        /// </summary>
        string CreateFolder(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension);

        /// <summary>
        /// Creates a relationship object of the specified type (given by the cmis:objectTypeId property).
        /// </summary>
        string CreateRelationship(string repositoryId, IProperties properties, IList<string> policies, IAcl addAces,
            IAcl removeAces, IExtensionsData extension);

        /// <summary>
        /// Creates a policy object of the specified type (given by the cmis:objectTypeId property).
        /// </summary>
        string CreatePolicy(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension);

        /// <summary>
        /// Creates an item object of the specified type (given by the cmis:objectTypeId property).
        /// </summary>
        string CreateItem(string repositoryId, IProperties properties, string folderId, IList<string> policies,
            IAcl addAces, IAcl removeAces, IExtensionsData extension);

        /// <summary>
        /// Gets the list of allowable actions for an object.
        /// </summary>
        IAllowableActions GetAllowableActions(string repositoryId, string objectId, IExtensionsData extension);

        /// <summary>
        /// Gets the list of properties for an object.
        /// </summary>
        IProperties GetProperties(string repositoryId, string objectId, string filter, IExtensionsData extension);

        /// <summary>
        /// Gets the list of associated renditions for the specified object.
        /// </summary>
        /// <remarks>
        /// Only rendition attributes are returned, not rendition stream.
        /// </remarks>
        IList<IRenditionData> GetRenditions(string repositoryId, string objectId, string renditionFilter,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension);

        /// <summary>
        /// Gets the specified information for the object specified by ID.
        /// </summary>
        IObjectData GetObject(string repositoryId, string objectId, string filter, bool? includeAllowableActions,
            IncludeRelationships? includeRelationships, string renditionFilter, bool? includePolicyIds,
            bool? includeAcl, IExtensionsData extension);

        /// <summary>
        /// Gets the specified information for the object specified by path.
        /// </summary>
        IObjectData GetObjectByPath(string repositoryId, string path, string filter, bool? includeAllowableActions,
            IncludeRelationships? includeRelationships, string renditionFilter, bool? includePolicyIds, bool? includeAcl,
            IExtensionsData extension);

        /// <summary>
        /// Gets the content stream for the specified document object, or gets a rendition stream
        /// for a specified rendition of a document or folder object.
        /// </summary>
        IContentStream GetContentStream(string repositoryId, string objectId, string streamId, BigInteger? offset, BigInteger? length,
            IExtensionsData extension);

        /// <summary>
        /// Updates properties of the specified object.
        /// </summary>
        void UpdateProperties(string repositoryId, ref string objectId, ref string changeToken, IProperties properties,
            IExtensionsData extension);

        /// <summary>
        /// Updates properties and secondary types of one or more objects.
        /// </summary>
        IList<IBulkUpdateObjectIdAndChangeToken> BulkUpdateProperties(string repositoryId,
                IList<IBulkUpdateObjectIdAndChangeToken> objectIdAndChangeToken, IProperties properties,
                IList<string> addSecondaryTypeIds, IList<string> removeSecondaryTypeIds, IExtensionsData extension);

        /// <summary>
        /// Moves the specified file-able object from one folder to another.
        /// </summary>
        void MoveObject(string repositoryId, ref string objectId, string targetFolderId, string sourceFolderId,
            IExtensionsData extension);

        /// <summary>
        /// Deletes the specified object.
        /// </summary>
        void DeleteObject(string repositoryId, string objectId, bool? allVersions, IExtensionsData extension);

        /// <summary>
        /// Deletes the specified folder object and all of its child- and descendant-objects.
        /// </summary>
        IFailedToDeleteData DeleteTree(string repositoryId, string folderId, bool? allVersions, UnfileObject? unfileObjects,
            bool? continueOnFailure, IExtensionsData extension);

        /// <summary>
        /// Sets the content stream for the specified document object.
        /// </summary>
        void SetContentStream(string repositoryId, ref string objectId, bool? overwriteFlag, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension);

        /// <summary>
        /// Deletes the content stream for the specified document object.
        /// </summary>
        void DeleteContentStream(string repositoryId, ref string objectId, ref string changeToken, IExtensionsData extension);

        /// <summary>
        /// Appends the content stream to the content of the document.
        /// </summary>
        void AppendContentStream(string repositoryId, ref string objectId, bool? isLastChunk, ref string changeToken,
            IContentStream contentStream, IExtensionsData extension);
    }

    /// <summary>
    /// Versioning Service interface.
    /// </summary>
    public interface IVersioningService
    {
        /// <summary>
        /// Create a private working copy of the document.
        /// </summary>
        void CheckOut(string repositoryId, ref string objectId, IExtensionsData extension, out bool? contentCopied);

        /// <summary>
        /// Reverses the effect of a check-out.
        /// </summary>
        void CancelCheckOut(string repositoryId, string objectId, IExtensionsData extension);

        /// <summary>
        /// Checks-in the private working copy (PWC) document.
        /// </summary>
        void CheckIn(string repositoryId, ref string objectId, bool? major, IProperties properties,
            IContentStream contentStream, string checkinComment, IList<string> policies, IAcl addAces, IAcl removeAces,
            IExtensionsData extension);

        /// <summary>
        /// Get the latest document object in the version series.
        /// </summary>
        IObjectData GetObjectOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool? major,
            string filter, bool? includeAllowableActions, IncludeRelationships? includeRelationships,
            string renditionFilter, bool? includePolicyIds, bool? includeAcl, IExtensionsData extension);

        /// <summary>
        /// Get a subset of the properties for the latest document object in the version series.
        /// </summary>
        IProperties GetPropertiesOfLatestVersion(string repositoryId, string objectId, string versionSeriesId, bool? major,
            string filter, IExtensionsData extension);

        /// <summary>
        /// Returns the list of all document objects in the specified version series, sorted by the property "cmis:creationDate" descending.
        /// </summary>
        IList<IObjectData> GetAllVersions(string repositoryId, string objectId, string versionSeriesId, string filter,
            bool? includeAllowableActions, IExtensionsData extension);
    }

    /// <summary>
    /// Relationship Service interface.
    /// </summary>
    public interface IRelationshipService
    {
        /// <summary>
        /// Gets all or a subset of relationships associated with an independent object.
        /// </summary>
        IObjectList GetObjectRelationships(string repositoryId, string objectId, bool? includeSubRelationshipTypes,
            RelationshipDirection? relationshipDirection, string typeId, string filter, bool? includeAllowableActions,
            BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension);
    }

    /// <summary>
    /// Discovery Service interface.
    /// </summary>
    public interface IDiscoveryService
    {
        /// <summary>
        /// Executes a CMIS query statement against the contents of the repository.
        /// </summary>
        IObjectList Query(string repositoryId, string statement, bool? searchAllVersions,
           bool? includeAllowableActions, IncludeRelationships? includeRelationships, string renditionFilter,
           BigInteger? maxItems, BigInteger? skipCount, IExtensionsData extension);

        /// <summary>
        /// Gets a list of content changes.
        /// </summary>
        IObjectList GetContentChanges(string repositoryId, ref string changeLogToken, bool? includeProperties,
           string filter, bool? includePolicyIds, bool? includeAcl, BigInteger? maxItems, IExtensionsData extension);
    }

    /// <summary>
    /// MultiFiling Service interface.
    /// </summary>
    public interface IMultiFilingService
    {
        /// <summary>
        /// Adds an existing fileable non-folder object to a folder.
        /// </summary>
        void AddObjectToFolder(string repositoryId, string objectId, string folderId, bool? allVersions, IExtensionsData extension);

        /// <summary>
        /// Removes an existing fileable non-folder object from a folder.
        /// </summary>
        void RemoveObjectFromFolder(string repositoryId, string objectId, string folderId, IExtensionsData extension);
    }

    /// <summary>
    /// ACL Service interface.
    /// </summary>
    public interface IAclService
    {
        /// <summary>
        /// Get the ACL currently applied to the specified object.
        /// </summary>
        IAcl GetAcl(string repositoryId, string objectId, bool? onlyBasicPermissions, IExtensionsData extension);

        /// <summary>
        /// Adds or removes the given ACEs to or from the ACL of the object.
        /// </summary>
        IAcl ApplyAcl(string repositoryId, string objectId, IAcl addAces, IAcl removeAces, AclPropagation? aclPropagation,
            IExtensionsData extension);
    }

    /// <summary>
    /// Policy Service interface.
    /// </summary>
    public interface IPolicyService
    {
        /// <summary>
        ///  Applies a specified policy to an object.
        /// </summary>
        void ApplyPolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension);

        /// <summary>
        /// Removes a specified policy from an object.
        /// </summary>
        void RemovePolicy(string repositoryId, string policyId, string objectId, IExtensionsData extension);

        /// <summary>
        /// Gets the list of policies currently applied to the specified object.
        /// </summary>
        IList<IObjectData> GetAppliedPolicies(string repositoryId, string objectId, string filter, IExtensionsData extension);
    }
}
