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

using PortCMIS.Data.Extensions;
using PortCMIS.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace PortCMIS.Data
{
    /// <summary>
    /// Represents a repository info.
    /// </summary>
    public interface IRepositoryInfo : IExtensionsData
    {
        /// <value>
        /// Repository ID.
        /// </value>
        string Id { get; }

        /// <value>
        /// Repository Name.
        /// </value>
        string Name { get; }

        /// <value>
        /// Repository description.
        /// </value>
        string Description { get; }

        /// <value>
        /// Repository vendor.
        /// </value>
        string VendorName { get; }

        /// <value>
        /// Repository product name.
        /// </value>
        string ProductName { get; }

        /// <value>
        /// Repository product version.
        /// </value>
        string ProductVersion { get; }

        /// <value>
        /// Root folder ID.
        /// </value>
        string RootFolderId { get; }

        /// <value>
        /// Repository capabilities.
        /// </value>
        IRepositoryCapabilities Capabilities { get; }

        /// <value>
        /// Repository ACL capabilities.
        /// </value>
        IAclCapabilities AclCapabilities { get; }

        /// <value>
        /// Latest change log token.
        /// </value>
        string LatestChangeLogToken { get; }

        /// <value>
        /// CMIS version (string).
        /// </value>
        string CmisVersionSupported { get; }

        /// <value>
        /// CMIS version (enum).
        /// </value>
        CmisVersion CmisVersion { get; }

        /// <value>
        /// Repository thin client URI.
        /// </value>
        string ThinClientUri { get; }

        /// <value>
        /// Changes incomplete flag.
        /// </value>
        bool? ChangesIncomplete { get; }

        /// <value>
        /// List of changable base types.
        /// </value>
        IList<BaseTypeId?> ChangesOnType { get; }

        /// <value>
        /// Principal ID of an anonymous user, if supported.
        /// </value>
        string PrincipalIdAnonymous { get; }

        /// <value>
        /// Principal ID of an unauthenticated user, if supported.
        /// </value>
        string PrincipalIdAnyone { get; }

        /// <value>
        /// List of extension features.
        /// </value>
        IList<IExtensionFeature> ExtensionFeatures { get; }
    }

    public interface IRepositoryCapabilities : IExtensionsData
    {
        CapabilityContentStreamUpdates? ContentStreamUpdatesCapability { get; }
        CapabilityChanges? ChangesCapability { get; }
        CapabilityRenditions? RenditionsCapability { get; }
        bool? IsGetDescendantsSupported { get; }
        bool? IsGetFolderTreeSupported { get; }
        CapabilityOrderBy? OrderByCapability { get; }
        bool? IsMultifilingSupported { get; }
        bool? IsUnfilingSupported { get; }
        bool? IsVersionSpecificFilingSupported { get; }
        bool? IsPwcSearchableSupported { get; }
        bool? IsPwcUpdatableSupported { get; }
        bool? IsAllVersionsSearchableSupported { get; }
        CapabilityQuery? QueryCapability { get; }
        CapabilityJoin? JoinCapability { get; }
        CapabilityAcl? AclCapability { get; }
        ICreatablePropertyTypes CreatablePropertyTypes { get; }
        INewTypeSettableAttributes NewTypeSettableAttributes { get; }
    }

    public interface ICreatablePropertyTypes : IExtensionsData
    {
        ISet<PropertyType> CanCreate { get; }
    }

    public interface INewTypeSettableAttributes : IExtensionsData
    {
        bool? CanSetId { get; }
        bool? CanSetLocalName { get; }
        bool? CanSetLocalNamespace { get; }
        bool? CanSetDisplayName { get; }
        bool? CanSetQueryName { get; }
        bool? CanSetDescription { get; }
        bool? CanSetCreatable { get; }
        bool? CanSetFileable { get; }
        bool? CanSetQueryable { get; }
        bool? CanSetFulltextIndexed { get; }
        bool? CanSetIncludedInSupertypeQuery { get; }
        bool? CanSetControllablePolicy { get; }
        bool? CanSetControllableAcl { get; }
    }

    public interface IAclCapabilities : IExtensionsData
    {
        SupportedPermissions? SupportedPermissions { get; }
        AclPropagation? AclPropagation { get; }
        IList<IPermissionDefinition> Permissions { get; }
        IDictionary<string, IPermissionMapping> PermissionMapping { get; }
    }

    public interface IPermissionDefinition : IExtensionsData
    {
        string Id { get; }
        string Description { get; }
    }

    public interface IPermissionMapping : IExtensionsData
    {
        string Key { get; }
        IList<string> Permissions { get; }
    }

    public interface IExtensionFeature : IExtensionsData
    {
        string Id { get; }
        string Url { get; }
        string CommonName { get; }
        string VersionLabel { get; }
        string Description { get; }
        IDictionary<string, string> FeatureData { get; }
    }

    /// <summary>
    /// Type definition.
    /// </summary>
    public interface ITypeDefinition : IExtensionsData
    {
        /// <value>
        /// Gets the type ID.
        /// </value>
        string Id { get; }

        /// <value>
        /// Gets the local name.
        /// </value>
        string LocalName { get; }

        /// <value>
        /// Gets the local name space.
        /// </value>
        string LocalNamespace { get; }

        /// <value>
        /// Gets the display name.
        /// </value>
        string DisplayName { get; }

        /// <value>
        /// Gets the query name.
        /// </value>
        string QueryName { get; }

        /// <value>
        /// Gets the description.
        /// </value>
        string Description { get; }

        /// <value>
        /// Gets the ID of the base type.
        /// </value>
        BaseTypeId BaseTypeId { get; }

        /// <value>
        /// Gets the ID of the parent type.
        /// </value>
        string ParentTypeId { get; }

        /// <value>
        /// Gets whether objects of this type can be created or not.
        /// </value>
        bool? IsCreatable { get; }

        /// <value>
        /// Gets whether objects of this type can be filed or not.
        /// </value>
        bool? IsFileable { get; }

        /// <value>
        /// Gets whether objects of this type can be queried or not.
        /// </value>
        bool? IsQueryable { get; }

        /// <value>
        /// Gets whether objects of this type are indexed or not.
        /// </value>
        bool? IsFulltextIndexed { get; }

        /// <value>
        /// Gets whether objects of this type can be found in super type queries or not.
        /// </value>
        bool? IsIncludedInSupertypeQuery { get; }

        /// <value>
        /// Gets whether polices can be applied to objects of this type or not.
        /// </value>
        bool? IsControllablePolicy { get; }

        /// <value>
        /// Gets whether ACLs can be applied to objects of this type or not.
        /// </value>
        bool? IsControllableAcl { get; }

        /// <value>
        /// Gets the property definition for the given property ID.
        /// </value>
        IPropertyDefinition this[string propertyId] { get; }

        /// <value>
        /// Gets a list of all property definitions.
        /// </value>
        IList<IPropertyDefinition> PropertyDefinitions { get; }

        /// <value>
        /// Gets the type mutability flags.
        /// </value>
        ITypeMutability TypeMutability { get; }
    }

    /// <summary>
    /// Type mutability flags.
    /// </summary>
    public interface ITypeMutability : IExtensionsData
    {
        /// <value>
        /// Defines whether subtypes can be created.
        /// </value>
        bool? CanCreate { get; }

        /// <value>
        /// Defines whether the type can be updated.
        /// </value>
        bool? CanUpdate { get; }

        /// <value>
        /// Defines whether the type can be deleted.
        /// </value>
        bool? CanDelete { get; }
    }

    /// <summary>
    /// Document type definition.
    /// </summary>
    public interface IDocumentTypeDefinition : ITypeDefinition
    {
        /// <value>
        /// Defines whether the type is versionabel or not.
        /// </value>
        bool? IsVersionable { get; }

        /// <value>
        /// Defines if content streams are supported for this type.
        /// </value>
        ContentStreamAllowed? ContentStreamAllowed { get; }
    }

    /// <summary>
    /// Folder type definition.
    /// </summary>
    public interface IFolderTypeDefinition : ITypeDefinition
    {
    }

    /// <summary>
    /// Secondary type definition.
    /// </summary>
    public interface ISecondaryTypeDefinition : ITypeDefinition
    {
    }

    /// <summary>
    /// Policy type definition.
    /// </summary>
    public interface IPolicyTypeDefinition : ITypeDefinition
    {
    }

    /// <summary>
    /// Item type definition.
    /// </summary>
    public interface IItemTypeDefinition : ITypeDefinition
    {
    }

    /// <summary>
    /// Relationship type definition.
    /// </summary>
    public interface IRelationshipTypeDefinition : ITypeDefinition
    {
        IList<string> AllowedSourceTypeIds { get; }
        IList<string> AllowedTargetTypeIds { get; }
    }

    public interface ITypeDefinitionList : IExtensionsData
    {
        IList<ITypeDefinition> List { get; }
        bool? HasMoreItems { get; }
        BigInteger? NumItems { get; }
    }

    public interface ITypeDefinitionContainer : IExtensionsData
    {
        ITypeDefinition TypeDefinition { get; }
        IList<ITypeDefinitionContainer> Children { get; }
    }

    /// <summary>
    /// Property definition.
    /// </summary>
    public interface IPropertyDefinition : IExtensionsData
    {
        string Id { get; }
        string LocalName { get; }
        string LocalNamespace { get; }
        string DisplayName { get; }
        string QueryName { get; }
        string Description { get; }
        PropertyType PropertyType { get; }
        Cardinality? Cardinality { get; }
        Updatability? Updatability { get; }
        bool? IsInherited { get; }
        bool? IsRequired { get; }
        bool? IsQueryable { get; }
        bool? IsOrderable { get; }
        bool? IsOpenChoice { get; }
    }

    public interface IChoice<T>
    {
        string DisplayName { get; }
        IList<T> Value { get; }
        IList<IChoice<T>> Choices { get; }
    }

    /// <summary>
    /// Property definition for a boolean property.
    /// </summary>
    public interface IPropertyBooleanDefinition : IPropertyDefinition
    {
        IList<bool?> DefaultValue { get; }
        IList<IChoice<bool?>> Choices { get; }
    }

    /// <summary>
    /// Property definition for a date time property.
    /// </summary>
    public interface IPropertyDateTimeDefinition : IPropertyDefinition
    {
        IList<DateTime?> DefaultValue { get; }
        IList<IChoice<DateTime?>> Choices { get; }
        DateTimeResolution? DateTimeResolution { get; }
    }

    /// <summary>
    /// Property definition for a decimal property.
    /// </summary>
    public interface IPropertyDecimalDefinition : IPropertyDefinition
    {
        IList<decimal?> DefaultValue { get; }
        IList<IChoice<decimal?>> Choices { get; }
        decimal? MinValue { get; }
        decimal? MaxValue { get; }
        DecimalPrecision? Precision { get; }
    }

    /// <summary>
    /// Property definition for a HTML property.
    /// </summary>
    public interface IPropertyHtmlDefinition : IPropertyDefinition
    {
        IList<string> DefaultValue { get; }
        IList<IChoice<string>> Choices { get; }
    }

    /// <summary>
    /// Property definition for an ID property.
    /// </summary>
    public interface IPropertyIdDefinition : IPropertyDefinition
    {
        IList<string> DefaultValue { get; }
        IList<IChoice<string>> Choices { get; }
    }

    /// <summary>
    /// Property definition for a integer property.
    /// </summary>
    public interface IPropertyIntegerDefinition : IPropertyDefinition
    {
        IList<BigInteger?> DefaultValue { get; }
        IList<IChoice<BigInteger?>> Choices { get; }
        BigInteger? MinValue { get; }
        BigInteger? MaxValue { get; }
    }

    /// <summary>
    /// Property definition for a string property.
    /// </summary>
    public interface IPropertyStringDefinition : IPropertyDefinition
    {
        IList<string> DefaultValue { get; }
        IList<IChoice<string>> Choices { get; }
        BigInteger? MaxLength { get; }
    }

    /// <summary>
    /// Property definition for a URI property.
    /// </summary>
    public interface IPropertyUriDefinition : IPropertyDefinition
    {
        IList<string> DefaultValue { get; }
        IList<IChoice<string>> Choices { get; }
    }

    public interface IObjectData : IExtensionsData
    {
        string Id { get; }
        BaseTypeId? BaseTypeId { get; }
        IProperties Properties { get; }
        IAllowableActions AllowableActions { get; }
        IList<IObjectData> Relationships { get; }
        IChangeEventInfo ChangeEventInfo { get; }
        IAcl Acl { get; }
        bool? IsExactAcl { get; }
        IPolicyIdList PolicyIds { get; }
        IList<IRenditionData> Renditions { get; }
    }

    public interface IObjectList : IExtensionsData
    {
        IList<IObjectData> Objects { get; }
        bool? HasMoreItems { get; }
        BigInteger? NumItems { get; }
    }

    public interface IObjectInFolderData : IExtensionsData
    {
        IObjectData Object { get; }
        string PathSegment { get; }
    }

    public interface IObjectInFolderList : IExtensionsData
    {
        IList<IObjectInFolderData> Objects { get; }
        bool? HasMoreItems { get; }
        BigInteger? NumItems { get; }
    }

    public interface IObjectInFolderContainer : IExtensionsData
    {
        IObjectInFolderData Object { get; }
        IList<IObjectInFolderContainer> Children { get; }
    }

    public interface IObjectParentData : IExtensionsData
    {
        IObjectData Object { get; }
        string RelativePathSegment { get; }
    }

    public interface IProperties : IExtensionsData
    {
        IPropertyData this[string propertyId] { get; }
        IList<IPropertyData> PropertyList { get; }
    }

    public interface IPropertyData : IExtensionsData
    {
        string Id { get; }
        string LocalName { get; }
        string DisplayName { get; }
        string QueryName { get; }
        PropertyType PropertyType { get; }
        IList<object> Values { get; }
        object FirstValue { get; }
    }

    /// <summary>
    /// Represents a principal.
    /// </summary>
    public interface IPrincipal : IExtensionsData
    {
        /// <value>The principal ID</value>
        string Id { get; }
    }

    /// <summary>
    /// Represents an ACE.
    /// </summary>
    public interface IAce : IExtensionsData
    {
        /// <value>The principal</value>
        IPrincipal Principal { get; }

        /// <value>The principal ID</value>
        string PrincipalId { get; }

        /// <value>The list of permissions</value>
        IList<string> Permissions { get; }

        /// <value>Indicates whether the ACE is a direct ACE or not</value>
        bool IsDirect { get; }
    }

    /// <summary>
    /// Represents an ACL.
    /// </summary>
    public interface IAcl : IExtensionsData
    {
        /// <value>The list of ACEs</value>
        IList<IAce> Aces { get; }

        /// <value>Indicates whether the ACL is exact or not</value>
        bool? IsExact { get; }
    }

    /// <summary>
    /// Represents a content stream.
    /// </summary>
    public interface IContentStream : IExtensionsData
    {
        /// <value>The stream length, if known</value>
        BigInteger? Length { get; }

        /// <value>The MIME type, if known</value>
        string MimeType { get; }

        /// <value>The file name, if known</value>
        string FileName { get; }

        /// <value>The stream</value>
        Stream Stream { get; }
    }

    /// <summary>
    /// Represents a partial content stream.
    /// </summary>
    public interface IPartialContentStream : IContentStream
    {
    }

    public interface IAllowableActions : IExtensionsData
    {
        ISet<PortCMIS.Enums.Action> Actions { get; }
    }

    public interface IRenditionData : IExtensionsData
    {
        string StreamId { get; }
        string MimeType { get; }
        BigInteger? Length { get; }
        string Kind { get; }
        string Title { get; }
        BigInteger? Height { get; }
        BigInteger? Width { get; }
        string RenditionDocumentId { get; }
    }

    public interface IChangeEventInfo : IExtensionsData
    {
        ChangeType? ChangeType { get; }
        DateTime? ChangeTime { get; }
    }

    public interface IPolicyIdList : IExtensionsData
    {
        IList<string> PolicyIds { get; }
    }

    public interface IFailedToDeleteData : IExtensionsData
    {
        IList<string> Ids { get; }
    }

    public interface IBulkUpdateObjectIdAndChangeToken : IExtensionsData
    {
        string Id { get; }
        string NewId { get; }
        string ChangeToken { get; }
    }
}
