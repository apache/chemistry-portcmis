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
        /// Gets the repository ID.
        /// </value>
        string Id { get; }

        /// <value>
        /// Gets the repository Name.
        /// </value>
        string Name { get; }

        /// <value>
        /// Gets the repository description.
        /// </value>
        string Description { get; }

        /// <value>
        /// Gets the repository vendor.
        /// </value>
        string VendorName { get; }

        /// <value>
        /// Gets the repository product name.
        /// </value>
        string ProductName { get; }

        /// <value>
        /// Gets the repository product version.
        /// </value>
        string ProductVersion { get; }

        /// <value>
        /// Gets the root folder ID.
        /// </value>
        string RootFolderId { get; }

        /// <value>
        /// Gets the repository capabilities.
        /// </value>
        IRepositoryCapabilities Capabilities { get; }

        /// <value>
        /// Gets the repository ACL capabilities.
        /// </value>
        IAclCapabilities AclCapabilities { get; }

        /// <value>
        /// Gets the latest change log token.
        /// </value>
        string LatestChangeLogToken { get; }

        /// <value>
        /// Gets the CMIS version as string.
        /// </value>
        string CmisVersionSupported { get; }

        /// <value>
        /// Gets the CMIS version as enum.
        /// </value>
        CmisVersion CmisVersion { get; }

        /// <value>
        /// Gets the repository thin client URI.
        /// </value>
        string ThinClientUri { get; }

        /// <value>
        /// Gets the changes incomplete flag.
        /// </value>
        bool? ChangesIncomplete { get; }

        /// <value>
        /// Gets the list of changable base types.
        /// </value>
        IList<BaseTypeId?> ChangesOnType { get; }

        /// <value>
        /// Gets the principal ID of an anonymous user, if supported.
        /// </value>
        string PrincipalIdAnonymous { get; }

        /// <value>
        /// Gets the principal ID of an unauthenticated user, if supported.
        /// </value>
        string PrincipalIdAnyone { get; }

        /// <value>
        /// Gets the list of extension features.
        /// </value>
        IList<IExtensionFeature> ExtensionFeatures { get; }
    }

    /// <summary>
    /// Repository Capabilities.
    /// </summary>
    public interface IRepositoryCapabilities : IExtensionsData
    {
        /// <value>
        /// Gets the content Stream Updates capability.
        /// </value>
        CapabilityContentStreamUpdates? ContentStreamUpdatesCapability { get; }

        /// <value>
        /// Gets the change log capability.
        /// </value>
        CapabilityChanges? ChangesCapability { get; }

        /// <value>
        /// Gets the rendition capability.
        /// </value>
        CapabilityRenditions? RenditionsCapability { get; }

        /// <value>
        /// Gets whether getDescendants is supported or not.
        /// </value>
        bool? IsGetDescendantsSupported { get; }

        /// <value>
        /// Gets whether getFolderTree is supported or not.
        /// </value>
        bool? IsGetFolderTreeSupported { get; }

        /// <value>
        /// Gets the OREDER BY capability.
        /// </value>
        CapabilityOrderBy? OrderByCapability { get; }

        /// <value>
        /// Gets whether multi-filing is supported or not.
        /// </value>
        bool? IsMultifilingSupported { get; }

        /// <value>
        /// Gets whether unfiling is supported or not.
        /// </value>
        bool? IsUnfilingSupported { get; }

        /// <value>
        /// Gets whether version specific filing is supported or not.
        /// </value>
        bool? IsVersionSpecificFilingSupported { get; }

        /// <value>
        /// Gets whether the PWC is searchable or not.
        /// </value>
        bool? IsPwcSearchableSupported { get; }

        /// <value>
        /// Gets whether PWC is updapatable or not.
        /// </value>
        bool? IsPwcUpdatableSupported { get; }

        /// <value>
        /// Gets whether query for all versions is supported or not.
        /// </value>
        bool? IsAllVersionsSearchableSupported { get; }

        /// <value>
        /// Gets the query capability.
        /// </value>
        CapabilityQuery? QueryCapability { get; }

        /// <value>
        /// Gets the Join capability.
        /// </value>
        CapabilityJoin? JoinCapability { get; }

        /// <value>
        /// Gets the ACL capability.
        /// </value>
        CapabilityAcl? AclCapability { get; }

        /// <value>
        /// Gets which property types are supported for new types.
        /// </value>
        ICreatablePropertyTypes CreatablePropertyTypes { get; }

        /// <value>
        /// Gets which attributes can be set on a new type.
        /// </value>
        INewTypeSettableAttributes NewTypeSettableAttributes { get; }
    }

    /// <summary>
    /// Property Type that are supported for new types.
    /// </summary>
    public interface ICreatablePropertyTypes : IExtensionsData
    {
        /// <value>
        /// Gets the set of property types that are supported for new types. 
        /// </value>
        ISet<PropertyType> CanCreate { get; }
    }

    /// <summary>
    /// Attributes that can be set on new types.
    /// </summary>
    public interface INewTypeSettableAttributes : IExtensionsData
    {
        /// <value>
        /// Gets whether the type ID can be set or not.
        /// </value>
        bool? CanSetId { get; }

        /// <value>
        /// Gets whether the local name can be set or not.
        /// </value>
        bool? CanSetLocalName { get; }

        /// <value>
        /// Gets whether the local namespace can be set or not.
        /// </value>
        bool? CanSetLocalNamespace { get; }

        /// <value>
        /// Gets whether the display name can be set or not.
        /// </value>
        bool? CanSetDisplayName { get; }

        /// <value>
        /// Gets whether the query name can be set or not.
        /// </value>
        bool? CanSetQueryName { get; }

        /// <value>
        /// Gets whether the description can be set or not.
        /// </value>
        bool? CanSetDescription { get; }

        /// <value>
        /// Gets whether the creatable flag can be set or not.
        /// </value>
        bool? CanSetCreatable { get; }

        /// <value>
        /// Gets whether the filable flag can be set or not.
        /// </value>
        bool? CanSetFileable { get; }

        /// <value>
        /// Gets whether the queryable flag can be set or not.
        /// </value>
        bool? CanSetQueryable { get; }

        /// <value>
        /// Gets whether the fulltext flag can be set or not.
        /// </value>
        bool? CanSetFulltextIndexed { get; }

        /// <value>
        /// Gets whether the IncludedInSupertype flag can be set or not.
        /// </value>
        bool? CanSetIncludedInSupertypeQuery { get; }

        /// <value>
        /// Gets whether the policy control can be set or not.
        /// </value>
        bool? CanSetControllablePolicy { get; }

        /// <value>
        /// Gets whether the ACL control can be set or not.
        /// </value>
        bool? CanSetControllableAcl { get; }
    }

    /// <summary>
    /// ACL capabilities.
    /// </summary>
    public interface IAclCapabilities : IExtensionsData
    {
        /// <value>
        /// Gets which permission set is supported.
        /// </value>
        SupportedPermissions? SupportedPermissions { get; }

        /// <value>
        /// Gets which ACL propagation is supported.
        /// </value>
        AclPropagation? AclPropagation { get; }

        /// <value>
        /// Gets permission definitions.
        /// </value>
        IList<IPermissionDefinition> Permissions { get; }

        /// <value>
        /// Gets permission mapping.
        /// </value>
        IDictionary<string, IPermissionMapping> PermissionMapping { get; }
    }

    /// <summary>
    /// Permission definition.
    /// </summary>
    public interface IPermissionDefinition : IExtensionsData
    {
        /// <value>
        /// Gets the permission ID.
        /// </value>
        string Id { get; }

        /// <value>
        /// Gets the description of the permission.
        /// </value>
        string Description { get; }
    }

    /// <summary>
    /// Permission mapping.
    /// </summary>
    public interface IPermissionMapping : IExtensionsData
    {
        /// <value>
        /// Gets the permission key.
        /// </value>
        /// <seealso cref="PortCMIS.PermissionMappingKeys"/>
        string Key { get; }

        /// <value>
        /// Gets the required permissions.
        /// </value>
        IList<string> Permissions { get; }
    }

    /// <summary>
    /// Extension feature.
    /// </summary>
    public interface IExtensionFeature : IExtensionsData
    {
        /// <value>
        /// Gets the ID of the feature.
        /// </value>
        string Id { get; }

        /// <value>
        /// Gets the URL of the feature.
        /// </value>
        string Url { get; }

        /// <value>
        /// Gets the name of the feature.
        /// </value>
        string CommonName { get; }

        /// <value>
        /// Gets the version label of the feature.
        /// </value>
        string VersionLabel { get; }

        /// <value>
        /// Gets the description of the feature.
        /// </value>
        string Description { get; }

        /// <value>
        /// Gets a feature specific set of data.
        /// </value>
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
        /// <value>
        /// Gets the list of objects.
        /// </value>
        IList<IObjectData> Objects { get; }

        /// <value>
        /// Gets whether there are more objects, if known.
        /// </value>
        bool? HasMoreItems { get; }

        /// <value>
        /// Gets total number of objects in the list, if known.
        /// </value>
        BigInteger? NumItems { get; }
    }

    /// <summary>
    /// Object in a folder.
    /// </summary>
    public interface IObjectInFolderData : IExtensionsData
    {
        /// <value>
        /// Gets the object.
        /// </value>
        IObjectData Object { get; }

        /// <value>
        /// Get the path segment of the object in the folder.
        /// </value>
        string PathSegment { get; }
    }

    /// <summary>
    /// List of objects in a folder.
    /// </summary>
    public interface IObjectInFolderList : IExtensionsData
    {
        /// <value>
        /// Gets the list of objects.
        /// </value>
        IList<IObjectInFolderData> Objects { get; }

        /// <value>
        /// Gets whether there are more objects, if known.
        /// </value>
        bool? HasMoreItems { get; }

        /// <value>
        /// Gets total number of objects in the list, if known.
        /// </value>
        BigInteger? NumItems { get; }
    }

    /// <summary>
    /// Tree node of objects in a folder.
    /// </summary>
    public interface IObjectInFolderContainer : IExtensionsData
    {
        /// <value>
        /// Gets the object.
        /// </value>
        IObjectInFolderData Object { get; }

        /// <value>
        /// Gets the children of the object, if any.
        /// </value>
        IList<IObjectInFolderContainer> Children { get; }
    }

    /// <summary>
    /// Object parent.
    /// </summary>
    public interface IObjectParentData : IExtensionsData
    {
        /// <value>
        /// Gets the parent object.
        /// </value>
        IObjectData Object { get; }

        /// <value>
        /// Gets the relative path segment of the object in the parent folder.
        /// </value>
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
