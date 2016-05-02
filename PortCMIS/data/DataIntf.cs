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
        /// <value>
        /// Gets the list of allowed source types.
        /// If the list is empty all types are allowed.
        /// </value>
        IList<string> AllowedSourceTypeIds { get; }

        /// <value>
        /// Gets the list of allowed target types.
        /// If the list is empty all types are allowed.
        /// </value>
        IList<string> AllowedTargetTypeIds { get; }
    }

    /// <summary>
    /// List of type definitions.
    /// </summary>
    /// <remarks>
    /// The list may be incomplete if paging was used to fetch the list.
    /// </remarks>
    public interface ITypeDefinitionList : IExtensionsData
    {
        /// <value>
        /// Gets the (partial) list of type definitions.
        /// </value>
        IList<ITypeDefinition> List { get; }

        /// <value>
        /// Gets whether there are more type definitions; <c>null</c> if unknown.
        /// </value>
        bool? HasMoreItems { get; }

        /// <value>
        /// Gets the total number of type definitions; <c>null</c> if unknown.
        /// </value>
        BigInteger? NumItems { get; }
    }

    /// <summary>
    /// Type Definition Container for trees of type definitions.
    /// </summary>
    public interface ITypeDefinitionContainer : IExtensionsData
    {
        /// <summary>
        /// Gets the type definition.
        /// </summary>
        ITypeDefinition TypeDefinition { get; }

        /// <summary>
        /// Gets the children of this type definition, if requested.
        /// </summary>
        IList<ITypeDefinitionContainer> Children { get; }
    }

    /// <summary>
    /// Property definition.
    /// </summary>
    public interface IPropertyDefinition : IExtensionsData
    {
        /// <value>
        /// Gets the property ID.
        /// </value>
        string Id { get; }

        /// <value>
        /// Gets the local name.
        /// </value>
        string LocalName { get; }

        /// <value>
        /// Gets the local namespace.
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
        /// Get the property type.
        /// </value>
        PropertyType PropertyType { get; }

        /// <value>
        /// Gets the cardinality.
        /// </value>
        Cardinality? Cardinality { get; }

        /// <value>
        /// Gets the updatability.
        /// </value>
        Updatability? Updatability { get; }

        /// <value>
        /// Gets whether the property has been inherited from a super type or not.
        /// </value>
        bool? IsInherited { get; }

        /// <value>
        /// Gets whether a value for this property required or not.
        /// </value>
        bool? IsRequired { get; }

        /// <value>
        /// Gets whether this property is queryable or not.
        /// </value>
        bool? IsQueryable { get; }

        /// <value>
        /// Gets whether this property is orderable or not.
        /// </value>
        bool? IsOrderable { get; }

        /// <value>
        /// Gets whether this property is an open choice property or not.
        /// </value>
        bool? IsOpenChoice { get; }
    }

    /// <summary>
    /// A choice value.
    /// </summary>
    /// <typeparam name="T">type of the choice</typeparam>
    public interface IChoice<T>
    {
        /// <value>
        /// Gets the display name.
        /// </value>
        string DisplayName { get; }

        /// <value>
        /// Gets the value.
        /// </value>
        IList<T> Value { get; }

        /// <value>
        /// Gets the children of this choice.
        /// </value>
        IList<IChoice<T>> Choices { get; }
    }

    /// <summary>
    /// Property definition for a boolean property.
    /// </summary>
    public interface IPropertyBooleanDefinition : IPropertyDefinition
    {
        /// <value>
        /// Gets the default value.
        /// </value>
        IList<bool?> DefaultValue { get; }

        /// <value>
        /// Gets the choices of this property.
        /// </value>
        IList<IChoice<bool?>> Choices { get; }
    }

    /// <summary>
    /// Property definition for a date time property.
    /// </summary>
    public interface IPropertyDateTimeDefinition : IPropertyDefinition
    {
        /// <value>
        /// Gets the default value.
        /// </value>
        IList<DateTime?> DefaultValue { get; }

        /// <value>
        /// Gets the choices of this property.
        /// </value>
        IList<IChoice<DateTime?>> Choices { get; }

        /// <value>
        /// Gets the supported resolution.
        /// </value>
        DateTimeResolution? DateTimeResolution { get; }
    }

    /// <summary>
    /// Property definition for a decimal property.
    /// </summary>
    public interface IPropertyDecimalDefinition : IPropertyDefinition
    {
        /// <value>
        /// Gets the default value.
        /// </value>
        IList<decimal?> DefaultValue { get; }

        /// <value>
        /// Gets the choices of this property.
        /// </value>
        IList<IChoice<decimal?>> Choices { get; }

        /// <summary>
        /// Gets the min value for this property.
        /// </summary>
        decimal? MinValue { get; }

        /// <summary>
        /// Gets the max value for this property.
        /// </summary>
        decimal? MaxValue { get; }

        /// <summary>
        /// Gets the supported precision.
        /// </summary>
        DecimalPrecision? Precision { get; }
    }

    /// <summary>
    /// Property definition for a HTML property.
    /// </summary>
    public interface IPropertyHtmlDefinition : IPropertyDefinition
    {
        /// <value>
        /// Gets the default value.
        /// </value>
        IList<string> DefaultValue { get; }

        /// <value>
        /// Gets the choices of this property.
        /// </value>
        IList<IChoice<string>> Choices { get; }
    }

    /// <summary>
    /// Property definition for an ID property.
    /// </summary>
    public interface IPropertyIdDefinition : IPropertyDefinition
    {
        /// <value>
        /// Gets the default value.
        /// </value>
        IList<string> DefaultValue { get; }

        /// <value>
        /// Gets the choices of this property.
        /// </value>
        IList<IChoice<string>> Choices { get; }
    }

    /// <summary>
    /// Property definition for a integer property.
    /// </summary>
    public interface IPropertyIntegerDefinition : IPropertyDefinition
    {
        /// <value>
        /// Gets the default value.
        /// </value>
        IList<BigInteger?> DefaultValue { get; }

        /// <value>
        /// Gets the choices of this property.
        /// </value>
        IList<IChoice<BigInteger?>> Choices { get; }

        /// <summary>
        /// Gets the min value for this property.
        /// </summary>
        BigInteger? MinValue { get; }

        /// <summary>
        /// Gets the max value for this property.
        /// </summary>
        BigInteger? MaxValue { get; }
    }

    /// <summary>
    /// Property definition for a string property.
    /// </summary>
    public interface IPropertyStringDefinition : IPropertyDefinition
    {
        /// <value>
        /// Gets the default value.
        /// </value>
        IList<string> DefaultValue { get; }

        /// <value>
        /// Gets the choices of this property.
        /// </value>
        IList<IChoice<string>> Choices { get; }

        /// <value>
        /// Gets the max length of the string.
        /// </value>
        BigInteger? MaxLength { get; }
    }

    /// <summary>
    /// Property definition for a URI property.
    /// </summary>
    public interface IPropertyUriDefinition : IPropertyDefinition
    {
        /// <value>
        /// Gets the default value.
        /// </value>
        IList<string> DefaultValue { get; }

        /// <value>
        /// Gets the choices of this property.
        /// </value>
        IList<IChoice<string>> Choices { get; }
    }

    /// <summary>
    /// Object data.
    /// </summary>
    public interface IObjectData : IExtensionsData
    {
        /// <value>
        /// Gets the ID of this object.
        /// </value>
        string Id { get; }

        /// <value>
        /// Gets the base type ID of this object.
        /// </value>
        BaseTypeId? BaseTypeId { get; }

        /// <value>
        /// Gets the properties of this object.
        /// </value>
        IProperties Properties { get; }

        /// <value>
        /// Gets the allowable actions of this object.
        /// </value>
        IAllowableActions AllowableActions { get; }

        /// <value>
        /// Gets the relationships of this object.
        /// </value>
        IList<IObjectData> Relationships { get; }

        /// <value>
        /// Gets the change events of this object.
        /// </value>
        IChangeEventInfo ChangeEventInfo { get; }

        /// <value>
        /// Gets the ACL of this object.
        /// </value>
        IAcl Acl { get; }

        /// <value>
        /// Gets whether the ACL is exact or not.
        /// </value>
        bool? IsExactAcl { get; }

        /// <value>
        /// Gets the policy IDs of this object.
        /// </value>
        IPolicyIdList PolicyIds { get; }

        /// <value>
        /// Gets the renditions of this object.
        /// </value>
        IList<IRenditionData> Renditions { get; }
    }

    /// <summary>
    /// Object List.
    /// </summary>
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

    /// <summary>
    /// Collection of property data.
    /// </summary>
    public interface IProperties : IExtensionsData
    {
        /// <value>
        /// Gets a property by property ID.
        /// </value>
        /// <param name="propertyId">the property ID</param>
        /// <returns>the property or <c>null</c> if the property doesn't exist</returns>
        IPropertyData this[string propertyId] { get; }

        /// <value>
        /// Gets the list of properties.
        /// </value>
        IList<IPropertyData> PropertyList { get; }
    }

    /// <summary>
    /// Property Data.
    /// </summary>
    public interface IPropertyData : IExtensionsData
    {
        /// <value>
        /// Gets the property ID.
        /// </value>
        string Id { get; }

        /// <value>
        /// Gets the local name.
        /// </value>
        string LocalName { get; }

        /// <value>
        /// Gets the display name.
        /// </value>
        string DisplayName { get; }

        /// <value>
        /// Gets the query name.
        /// </value>
        string QueryName { get; }

        /// <value>
        /// Gets the property type.
        /// </value>
        PropertyType PropertyType { get; }

        /// <value>
        /// Gets the property values.
        /// </value>
        IList<object> Values { get; }

        /// <value>
        /// Gets the first property value.
        /// </value>
        object FirstValue { get; }
    }

    /// <summary>
    /// Represents a principal.
    /// </summary>
    public interface IPrincipal : IExtensionsData
    {
        /// <value>
        /// Gets the principal ID.
        /// </value>
        string Id { get; }
    }

    /// <summary>
    /// Represents an ACE.
    /// </summary>
    public interface IAce : IExtensionsData
    {
        /// <value>
        /// Gets the principal
        /// </value>
        IPrincipal Principal { get; }

        /// <value>
        /// Gets the principal ID
        /// </value>
        string PrincipalId { get; }

        /// <value>
        /// Gets the list of permissions
        /// </value>
        IList<string> Permissions { get; }

        /// <value>
        /// Gets whether the ACE is a direct ACE or not
        /// </value>
        bool IsDirect { get; }
    }

    /// <summary>
    /// Represents an ACL.
    /// </summary>
    public interface IAcl : IExtensionsData
    {
        /// <value>
        /// Gets the list of ACEs.
        /// </value>
        IList<IAce> Aces { get; }

        /// <value>
        /// Gets whether the ACL is exact or not.
        /// </value>
        bool? IsExact { get; }
    }

    /// <summary>
    /// Represents a content stream.
    /// </summary>
    public interface IContentStream : IExtensionsData
    {
        /// <value>
        /// Gets the stream length, if known.
        /// </value>
        BigInteger? Length { get; }

        /// <value>
        /// Gets the MIME type, if known.
        /// </value>
        string MimeType { get; }

        /// <value>
        /// Gets the file name, if known.
        /// </value>
        string FileName { get; }

        /// <value>
        /// Gets the stream.
        /// </value>
        Stream Stream { get; }
    }

    /// <summary>
    /// Represents a partial content stream.
    /// </summary>
    public interface IPartialContentStream : IContentStream
    {
    }

    /// <summary>
    /// Allowable Actions.
    /// </summary>
    public interface IAllowableActions : IExtensionsData
    {
        /// <value>
        /// Gets the set of Allowable Actions.
        /// </value>
        ISet<PortCMIS.Enums.Action> Actions { get; }
    }

    /// <summary>
    /// Rendition Data.
    /// </summary>
    public interface IRenditionData : IExtensionsData
    {
        /// <value>
        /// Gets the stream ID.
        /// </value>
        string StreamId { get; }

        /// <value>
        /// Gets the MIME type.
        /// </value>
        string MimeType { get; }

        /// <value>
        /// Gets the stream length, if known.
        /// </value>
        BigInteger? Length { get; }

        /// <value>
        /// Gets the kind.
        /// </value>
        string Kind { get; }

        /// <value>
        /// Gets the title.
        /// </value>
        string Title { get; }

        /// <value>
        /// Gets the height, if the rendition is an image.
        /// </value>
        BigInteger? Height { get; }

        /// <value>
        /// Gets the width, if the rendition is an image.
        /// </value>
        BigInteger? Width { get; }

        /// <value>
        /// Gets the rendition document ID, if the rendition is a standalone document.
        /// </value>
        string RenditionDocumentId { get; }
    }

    /// <summary>
    /// Change event.
    /// </summary>
    public interface IChangeEventInfo : IExtensionsData
    {
        /// <value>
        /// Gets the type of the change.
        /// </value>
        ChangeType? ChangeType { get; }

        /// <value>
        /// Gets the date and time of the change.
        /// </value>
        DateTime? ChangeTime { get; }
    }

    /// <summary>
    /// Policy ID List.
    /// </summary>
    public interface IPolicyIdList : IExtensionsData
    {
        /// <value>
        /// Gets the IDs of policies.
        /// </value>
        IList<string> PolicyIds { get; }
    }

    /// <summary>
    /// Failed to Delete data.
    /// </summary>
    public interface IFailedToDeleteData : IExtensionsData
    {
        /// <value>
        /// Gets the IDs of objects that couldn't be deleted.
        /// </value>
        IList<string> Ids { get; }
    }

    /// <summary>
    /// Bulk Update data.
    /// </summary>
    public interface IBulkUpdateObjectIdAndChangeToken : IExtensionsData
    {
        /// <value>
        /// Gets the original object ID.
        /// </value>
        string Id { get; }

        /// <value>
        /// Gets the new object ID, if available.
        /// </value>
        string NewId { get; }

        /// <value>
        /// Gets the change token.
        /// </value>
        string ChangeToken { get; }
    }
}
