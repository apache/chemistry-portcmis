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
using System.Text;

namespace PortCMIS.Data
{
    public class RepositoryInfo : ExtensionsData, IRepositoryInfo
    {
        public RepositoryInfo()
        {
        }
        public RepositoryInfo(IRepositoryInfo source)
        {
            Id = source.Id;
            Name = source.Name;
            Description = source.Description;
            VendorName = source.VendorName;
            ProductName = source.ProductName;
            ProductVersion = source.ProductVersion;
            RootFolderId = source.RootFolderId;
            Capabilities = source.Capabilities;
            AclCapabilities = source.AclCapabilities;
            LatestChangeLogToken = source.LatestChangeLogToken;
            CmisVersionSupported = source.CmisVersionSupported;
            ThinClientUri = source.ThinClientUri;
            ChangesIncomplete = source.ChangesIncomplete;
            ChangesOnType = source.ChangesOnType;
            PrincipalIdAnonymous = source.PrincipalIdAnonymous;
            PrincipalIdAnyone = source.PrincipalIdAnyone;
            ExtensionFeatures = source.ExtensionFeatures;
            Extensions = source.Extensions;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string VendorName { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public string RootFolderId { get; set; }
        public IRepositoryCapabilities Capabilities { get; set; }
        public IAclCapabilities AclCapabilities { get; set; }
        public string LatestChangeLogToken { get; set; }
        public string CmisVersionSupported { get; set; }
        public string ThinClientUri { get; set; }
        public bool? ChangesIncomplete { get; set; }
        public IList<BaseTypeId?> ChangesOnType { get; set; }
        public string PrincipalIdAnonymous { get; set; }
        public string PrincipalIdAnyone { get; set; }
        public IList<IExtensionFeature> ExtensionFeatures { get; set; }

        public override string ToString()
        {
            return "RepositoryInfo: " + Id;
        }
    }

    public class RepositoryCapabilities : ExtensionsData, IRepositoryCapabilities
    {
        public CapabilityContentStreamUpdates? ContentStreamUpdatesCapability { get; set; }
        public CapabilityChanges? ChangesCapability { get; set; }
        public CapabilityRenditions? RenditionsCapability { get; set; }
        public bool? IsGetDescendantsSupported { get; set; }
        public bool? IsGetFolderTreeSupported { get; set; }
        public CapabilityOrderBy? OrderByCapability { get; set; }
        public bool? IsMultifilingSupported { get; set; }
        public bool? IsUnfilingSupported { get; set; }
        public bool? IsVersionSpecificFilingSupported { get; set; }
        public bool? IsPwcSearchableSupported { get; set; }
        public bool? IsPwcUpdatableSupported { get; set; }
        public bool? IsAllVersionsSearchableSupported { get; set; }
        public CapabilityQuery? QueryCapability { get; set; }
        public CapabilityJoin? JoinCapability { get; set; }
        public CapabilityAcl? AclCapability { get; set; }
        public ICreatablePropertyTypes CreatablePropertyTypes { get; set; }
        public INewTypeSettableAttributes NewTypeSettableAttributes { get; set; }
    }

    public class CreatablePropertyTypes : ExtensionsData, ICreatablePropertyTypes
    {
        public ISet<PropertyType> CanCreate { get; set; }
    }

    public class NewTypeSettableAttributes : ExtensionsData, INewTypeSettableAttributes
    {
        public bool? CanSetId { get; set; }
        public bool? CanSetLocalName { get; set; }
        public bool? CanSetLocalNamespace { get; set; }
        public bool? CanSetDisplayName { get; set; }
        public bool? CanSetQueryName { get; set; }
        public bool? CanSetDescription { get; set; }
        public bool? CanSetCreatable { get; set; }
        public bool? CanSetFileable { get; set; }
        public bool? CanSetQueryable { get; set; }
        public bool? CanSetFulltextIndexed { get; set; }
        public bool? CanSetIncludedInSupertypeQuery { get; set; }
        public bool? CanSetControllablePolicy { get; set; }
        public bool? CanSetControllableAcl { get; set; }
    }

    public class AclCapabilities : ExtensionsData, IAclCapabilities
    {
        public SupportedPermissions? SupportedPermissions { get; set; }
        public AclPropagation? AclPropagation { get; set; }
        public IList<IPermissionDefinition> Permissions { get; set; }
        public IDictionary<string, IPermissionMapping> PermissionMapping { get; set; }
    }

    public class PermissionDefinition : ExtensionsData, IPermissionDefinition
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    public class PermissionMapping : ExtensionsData, IPermissionMapping
    {
        public string Key { get; set; }
        public IList<string> Permissions { get; set; }
    }

    public class ExtensionFeature : ExtensionsData, IExtensionFeature
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string CommonName { get; set; }
        public string VersionLabel { get; set; }
        public string Description { get; set; }
        public IDictionary<string, string> FeatureData { get; set; }
    }

    public abstract class AbstractTypeDefinition : ExtensionsData, ITypeDefinition
    {
        private List<IPropertyDefinition> propertyDefintionList = new List<IPropertyDefinition>();
        private Dictionary<string, IPropertyDefinition> propertyDefintionDict = new Dictionary<string, IPropertyDefinition>();
        private string parentTypeId;
        public string Id { get; set; }
        public string LocalName { get; set; }
        public string LocalNamespace { get; set; }
        public string DisplayName { get; set; }
        public string QueryName { get; set; }
        public string Description { get; set; }
        public BaseTypeId BaseTypeId { get; set; }
        public string ParentTypeId
        {
            get { return parentTypeId; }
            set { parentTypeId = (value == null || value.Length == 0 ? null : value); }
        }
        public bool? IsCreatable { get; set; }
        public bool? IsFileable { get; set; }
        public bool? IsQueryable { get; set; }
        public bool? IsFulltextIndexed { get; set; }
        public bool? IsIncludedInSupertypeQuery { get; set; }
        public bool? IsControllablePolicy { get; set; }
        public bool? IsControllableAcl { get; set; }
        public IPropertyDefinition this[string propertyId]
        {
            get
            {
                IPropertyDefinition propertyDefinition = null;
                propertyDefintionDict.TryGetValue(propertyId, out propertyDefinition);
                return propertyDefinition;
            }
        }
        public IList<IPropertyDefinition> PropertyDefinitions
        {
            get
            {
                return propertyDefintionList;
            }
        }
        public ITypeMutability TypeMutability { get; set; }

        public void Initialize(ITypeDefinition typeDefinition)
        {
            Id = typeDefinition.Id;
            LocalName = typeDefinition.LocalName;
            LocalNamespace = typeDefinition.LocalNamespace;
            DisplayName = typeDefinition.DisplayName;
            QueryName = typeDefinition.QueryName;
            Description = typeDefinition.Description;
            BaseTypeId = typeDefinition.BaseTypeId;
            ParentTypeId = typeDefinition.ParentTypeId;
            IsCreatable = typeDefinition.IsCreatable;
            IsFileable = typeDefinition.IsFileable;
            IsQueryable = typeDefinition.IsQueryable;
            IsFulltextIndexed = typeDefinition.IsFulltextIndexed;
            IsIncludedInSupertypeQuery = typeDefinition.IsIncludedInSupertypeQuery;
            IsControllablePolicy = typeDefinition.IsControllablePolicy;
            IsControllableAcl = typeDefinition.IsControllableAcl;
            if (typeDefinition.PropertyDefinitions != null)
            {
                foreach (IPropertyDefinition propDef in typeDefinition.PropertyDefinitions)
                {
                    AddPropertyDefinition(propDef);
                }
            }
        }
        public void AddPropertyDefinition(IPropertyDefinition propertyDefinition)
        {
            if (propertyDefinition == null || propertyDefinition.Id == null)
            {
                return;
            }
            propertyDefintionList.Add(propertyDefinition);
            propertyDefintionDict[propertyDefinition.Id] = propertyDefinition;
        }

        public override string ToString()
        {
            return "TypeDefinition: " + BaseTypeId + " (" + Id + ")";
        }
    }

    public class TypeMutability : ExtensionsData, ITypeMutability
    {
        public bool? CanCreate { get; set; }
        public bool? CanUpdate { get; set; }
        public bool? CanDelete { get; set; }
    }

    public class DocumentTypeDefinition : AbstractTypeDefinition, IDocumentTypeDefinition
    {
        public bool? IsVersionable { get; set; }
        public ContentStreamAllowed? ContentStreamAllowed { get; set; }
    }

    public class FolderTypeDefinition : AbstractTypeDefinition, IFolderTypeDefinition
    {
    }

    public class PolicyTypeDefinition : AbstractTypeDefinition, IPolicyTypeDefinition
    {
    }

    public class ItemTypeDefinition : AbstractTypeDefinition, IItemTypeDefinition
    {
    }

    public class SecondaryTypeDefinition : AbstractTypeDefinition, ISecondaryTypeDefinition
    {
    }

    public class RelationshipTypeDefinition : AbstractTypeDefinition, IRelationshipTypeDefinition
    {
        public IList<string> AllowedSourceTypeIds { get; set; }
        public IList<string> AllowedTargetTypeIds { get; set; }
    }

    public class TypeDefinitionList : ExtensionsData, ITypeDefinitionList
    {
        public IList<ITypeDefinition> List { get; set; }
        public bool? HasMoreItems { get; set; }
        public BigInteger? NumItems { get; set; }
    }

    public class TypeDefinitionContainer : ExtensionsData, ITypeDefinitionContainer
    {
        public ITypeDefinition TypeDefinition { get; set; }
        public IList<ITypeDefinitionContainer> Children { get; set; }
    }

    public abstract class PropertyDefinition : ExtensionsData, IPropertyDefinition
    {
        public string Id { get; set; }
        public string LocalName { get; set; }
        public string LocalNamespace { get; set; }
        public string DisplayName { get; set; }
        public string QueryName { get; set; }
        public string Description { get; set; }
        public PropertyType PropertyType { get; set; }
        public Cardinality? Cardinality { get; set; }
        public Updatability? Updatability { get; set; }
        public bool? IsInherited { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsQueryable { get; set; }
        public bool? IsOrderable { get; set; }
        public bool? IsOpenChoice { get; set; }
    }

    public class Choice<T> : IChoice<T>
    {
        public string DisplayName { get; set; }
        public IList<T> Value { get; set; }
        public IList<IChoice<T>> Choices { get; set; }
    }

    public class PropertyBooleanDefinition : PropertyDefinition, IPropertyBooleanDefinition
    {
        public IList<bool?> DefaultValue { get; set; }
        public IList<IChoice<bool?>> Choices { get; set; }
    }

    public class PropertyDateTimeDefinition : PropertyDefinition, IPropertyDateTimeDefinition
    {
        public IList<DateTime?> DefaultValue { get; set; }
        public IList<IChoice<DateTime?>> Choices { get; set; }
        public DateTimeResolution? DateTimeResolution { get; set; }
    }

    public class PropertyDecimalDefinition : PropertyDefinition, IPropertyDecimalDefinition
    {
        public IList<decimal?> DefaultValue { get; set; }
        public IList<IChoice<decimal?>> Choices { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public DecimalPrecision? Precision { get; set; }
    }

    public class PropertyHtmlDefinition : PropertyDefinition, IPropertyHtmlDefinition
    {
        public IList<string> DefaultValue { get; set; }
        public IList<IChoice<string>> Choices { get; set; }
    }

    public class PropertyIdDefinition : PropertyDefinition, IPropertyIdDefinition
    {
        public IList<string> DefaultValue { get; set; }
        public IList<IChoice<string>> Choices { get; set; }
    }

    public class PropertyIntegerDefinition : PropertyDefinition, IPropertyIntegerDefinition
    {
        public IList<BigInteger?> DefaultValue { get; set; }
        public IList<IChoice<BigInteger?>> Choices { get; set; }
        public BigInteger? MinValue { get; set; }
        public BigInteger? MaxValue { get; set; }
    }

    public class PropertyStringDefinition : PropertyDefinition, IPropertyStringDefinition
    {
        public IList<string> DefaultValue { get; set; }
        public IList<IChoice<string>> Choices { get; set; }
        public BigInteger? MaxLength { get; set; }
    }

    public class PropertyUriDefinition : PropertyDefinition, IPropertyUriDefinition
    {
        public IList<string> DefaultValue { get; set; }
        public IList<IChoice<string>> Choices { get; set; }
    }

    public class ObjectData : ExtensionsData, IObjectData
    {
        public string Id
        {
            get
            {
                return GetFirstValue(PropertyIds.ObjectId) as string;
            }
        }
        public BaseTypeId? BaseTypeId
        {
            get
            {
                string baseTypeId = GetFirstValue(PropertyIds.BaseTypeId) as string;
                if (baseTypeId == null)
                {
                    return null;
                }
                return baseTypeId.GetCmisEnum<BaseTypeId>();
            }
        }
        public IProperties Properties { get; set; }
        public IAllowableActions AllowableActions { get; set; }
        public IList<IObjectData> Relationships { get; set; }
        public IChangeEventInfo ChangeEventInfo { get; set; }
        public IAcl Acl { get; set; }
        public bool? IsExactAcl { get; set; }
        public IPolicyIdList PolicyIds { get; set; }
        public IList<IRenditionData> Renditions { get; set; }
        private object GetFirstValue(string id)
        {
            if (Properties == null) { return null; }
            IPropertyData property = Properties[id];
            if (property == null)
            {
                return null;
            }
            return property.FirstValue;
        }
    }

    public class ObjectList : ExtensionsData, IObjectList
    {
        public IList<IObjectData> Objects { get; set; }
        public bool? HasMoreItems { get; set; }
        public BigInteger? NumItems { get; set; }
    }

    public class ObjectInFolderData : ExtensionsData, IObjectInFolderData
    {
        public IObjectData Object { get; set; }
        public string PathSegment { get; set; }
    }

    public class ObjectInFolderList : ExtensionsData, IObjectInFolderList
    {
        public IList<IObjectInFolderData> Objects { get; set; }
        public bool? HasMoreItems { get; set; }
        public BigInteger? NumItems { get; set; }
    }

    public class ObjectInFolderContainer : ExtensionsData, IObjectInFolderContainer
    {
        public IObjectInFolderData Object { get; set; }
        public IList<IObjectInFolderContainer> Children { get; set; }
    }

    public class ObjectParentData : ExtensionsData, IObjectParentData
    {
        public IObjectData Object { get; set; }
        public string RelativePathSegment { get; set; }
    }

    public class Properties : ExtensionsData, IProperties
    {
        private List<IPropertyData> propertyList = new List<IPropertyData>();
        private Dictionary<string, IPropertyData> propertyDict = new Dictionary<string, IPropertyData>();
        public IPropertyData this[string propertyId]
        {
            get
            {
                IPropertyData property = null;
                propertyDict.TryGetValue(propertyId, out property);
                return property;
            }
        }
        public IList<IPropertyData> PropertyList
        {
            get
            {
                return propertyList;
            }
        }
        public void AddProperty(IPropertyData property)
        {
            if (property == null)
            {
                return;
            }
            propertyList.Add(property);
            if (property.Id != null)
            {
                propertyDict[property.Id] = property;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (IPropertyData prop in propertyList)
            {
                if (sb.Length == 0) { sb.Append("["); }
                else { sb.Append(", "); }
                sb.Append(prop.ToString());
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    public class PropertyData : ExtensionsData, IPropertyData
    {
        private IList<object> values;
        public PropertyData(PropertyType propertyType)
        {
            PropertyType = propertyType;
        }
        public string Id { get; set; }
        public string LocalName { get; set; }
        public string DisplayName { get; set; }
        public string QueryName { get; set; }
        public PropertyType PropertyType { get; protected set; }
        public IList<object> Values
        {
            get { return values; }
            set
            {
                if (value == null)
                {
                    values = null;
                }
                else
                {
                    foreach (object o in value)
                    {
                        CheckValue(o);
                    }
                    values = value;
                }
            }
        }
        public object FirstValue { get { return values == null || Values.Count < 1 ? null : values[0]; } }

        public void AddValue(object value)
        {
            object newValue = CheckValue(value);
            if (Values == null)
            {
                Values = new List<object>();
            }
            Values.Add(newValue);
        }

        public object CheckValue(object value)
        {
            switch (PropertyType)
            {
                case PropertyType.String:
                    if (!(value is string))
                    {
                        throw new ArgumentException("Property '" + Id + "' is a String property!");
                    }
                    return value;
                case PropertyType.Id:
                    if (!(value is string))
                    {
                        throw new ArgumentException("Property '" + Id + "' is an Id property!");
                    }
                    return value;
                case PropertyType.Integer:
                    if (!(value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is BigInteger))
                    {
                        throw new ArgumentException("Property '" + Id + "' is an Integer property!");
                    }
                    if (value is BigInteger)
                    {
                        return value;
                    }
                    else
                    {
                        return new BigInteger((long)value);
                    }
                case PropertyType.Boolean:
                    if (!(value is bool))
                    {
                        throw new ArgumentException("Property '" + Id + "' is a Boolean property!");
                    }
                    return value;
                case PropertyType.DateTime:
                    if (!(value is DateTime))
                    {
                        throw new ArgumentException("Property '" + Id + "' is a DateTime property!");
                    }
                    return value;
                case PropertyType.Decimal:
                    if (!(value is decimal || value is double || value is float))
                    {
                        throw new ArgumentException("Property '" + Id + "' is a Decimal property!");
                    }
                    return value;
                case PropertyType.Uri:
                    if (!(value is string))
                    {
                        throw new ArgumentException("Property '" + Id + "' is a URI property!");
                    }
                    return value;
                case PropertyType.Html:
                    if (!(value is string))
                    {
                        throw new ArgumentException("Property '" + Id + "' is a HTML property!");
                    }
                    return value;
                default:
                    throw new ArgumentException("Unknown property type!");
            }
        }

        public override string ToString()
        {
            return Id + ": " + values;
        }
    }

    public class Principal : ExtensionsData, IPrincipal
    {
        public string Id { get; set; }
    }

    public class Ace : ExtensionsData, IAce
    {
        public IPrincipal Principal { get; set; }
        public string PrincipalId { get { return Principal == null ? null : Principal.Id; } }
        public IList<string> Permissions { get; set; }
        public bool? IsDirect { get; set; }
    }

    public class Acl : ExtensionsData, IAcl
    {
        public IList<IAce> Aces { get; set; }
        public bool? IsExact { get; set; }
    }

    public class ContentStream : ExtensionsData, IContentStream
    {
        public BigInteger? Length { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
        public Stream Stream { get; set; }
    }

    public class PartialContentStream : ContentStream, IPartialContentStream
    {
    }

    public class AllowableActions : ExtensionsData, IAllowableActions
    {
        public ISet<PortCMIS.Enums.Action> Actions { get; set; }
    }

    public class RenditionData : ExtensionsData, IRenditionData
    {
        public string StreamId { get; set; }
        public string MimeType { get; set; }
        public BigInteger? Length { get; set; }
        public string Kind { get; set; }
        public string Title { get; set; }
        public BigInteger? Height { get; set; }
        public BigInteger? Width { get; set; }
        public string RenditionDocumentId { get; set; }
    }

    public class ChangeEventInfo : ExtensionsData, IChangeEventInfo
    {
        public ChangeType? ChangeType { get; set; }
        public DateTime? ChangeTime { get; set; }
    }

    public class PolicyIdList : ExtensionsData, IPolicyIdList
    {
        public IList<string> PolicyIds { get; set; }
    }

    public class FailedToDeleteData : ExtensionsData, IFailedToDeleteData
    {
        public IList<string> Ids { get; set; }
    }

    public class QueryType : ExtensionsData
    {
        public string Statement { get; set; }
        public bool SearchAllVersions { get; set; }
        public bool IncludeAllowableActions { get; set; }
        public IncludeRelationships IncludeRelationships { get; set; }
        public string RenditionFilter { get; set; }
        public BigInteger MaxItems { get; set; }
        public BigInteger SkipCount { get; set; }
    }

    public class BulkUpdateObjectIdAndChangeToken : ExtensionsData, IBulkUpdateObjectIdAndChangeToken
    {
        public string Id { get; set; }
        public string NewId { get; set; }
        public string ChangeToken { get; set; }
    }

    public class BulkUpdate : ExtensionsData
    {
        public IList<IBulkUpdateObjectIdAndChangeToken> ObjectIdAndChangeToken { get; set; }
        public Properties Properties { get; set; }
        public IList<string> AddSecondaryTypeIds { get; set; }
        public IList<string> RemoveSecondaryTypeIds { get; set; }
    }
}
