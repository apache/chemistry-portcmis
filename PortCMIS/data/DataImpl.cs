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

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public string VendorName { get; set; }

        /// <inheritdoc/>
        public string ProductName { get; set; }

        /// <inheritdoc/>
        public string ProductVersion { get; set; }

        /// <inheritdoc/>
        public string RootFolderId { get; set; }

        /// <inheritdoc/>
        public IRepositoryCapabilities Capabilities { get; set; }

        /// <inheritdoc/>
        public IAclCapabilities AclCapabilities { get; set; }

        /// <inheritdoc/>
        public string LatestChangeLogToken { get; set; }

        /// <inheritdoc/>
        public string CmisVersionSupported { get; set; }

        /// <inheritdoc/>
        public CmisVersion CmisVersion
        {
            get
            {
                if (CmisVersionSupported == null)
                {
                    return CmisVersion.Cmis_1_0;
                }
                else
                {
                    CmisVersion? cmisVersion = CmisVersionSupported.GetCmisEnum<CmisVersion?>();
                    if (cmisVersion == null)
                    {
                        return CmisVersion.Cmis_1_0;
                    }
                    else
                    {
                        return (CmisVersion)cmisVersion;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public string ThinClientUri { get; set; }

        /// <inheritdoc/>
        public bool? ChangesIncomplete { get; set; }

        /// <inheritdoc/>
        public IList<BaseTypeId?> ChangesOnType { get; set; }

        /// <inheritdoc/>
        public string PrincipalIdAnonymous { get; set; }

        /// <inheritdoc/>
        public string PrincipalIdAnyone { get; set; }

        /// <inheritdoc/>
        public IList<IExtensionFeature> ExtensionFeatures { get; set; }

        public override string ToString()
        {
            return "RepositoryInfo: " + Id;
        }
    }

    internal class RepositoryCapabilities : ExtensionsData, IRepositoryCapabilities
    {
        /// <inheritdoc/>
        public CapabilityContentStreamUpdates? ContentStreamUpdatesCapability { get; set; }

        /// <inheritdoc/>
        public CapabilityChanges? ChangesCapability { get; set; }

        /// <inheritdoc/>
        public CapabilityRenditions? RenditionsCapability { get; set; }

        /// <inheritdoc/>
        public bool? IsGetDescendantsSupported { get; set; }

        /// <inheritdoc/>
        public bool? IsGetFolderTreeSupported { get; set; }

        /// <inheritdoc/>
        public CapabilityOrderBy? OrderByCapability { get; set; }

        /// <inheritdoc/>
        public bool? IsMultifilingSupported { get; set; }

        /// <inheritdoc/>
        public bool? IsUnfilingSupported { get; set; }

        /// <inheritdoc/>
        public bool? IsVersionSpecificFilingSupported { get; set; }

        /// <inheritdoc/>
        public bool? IsPwcSearchableSupported { get; set; }

        /// <inheritdoc/>
        public bool? IsPwcUpdatableSupported { get; set; }

        /// <inheritdoc/>
        public bool? IsAllVersionsSearchableSupported { get; set; }

        /// <inheritdoc/>
        public CapabilityQuery? QueryCapability { get; set; }

        /// <inheritdoc/>
        public CapabilityJoin? JoinCapability { get; set; }

        /// <inheritdoc/>
        public CapabilityAcl? AclCapability { get; set; }

        /// <inheritdoc/>
        public ICreatablePropertyTypes CreatablePropertyTypes { get; set; }

        /// <inheritdoc/>
        public INewTypeSettableAttributes NewTypeSettableAttributes { get; set; }
    }

    internal class CreatablePropertyTypes : ExtensionsData, ICreatablePropertyTypes
    {
        /// <inheritdoc/>
        public ISet<PropertyType> CanCreate { get; set; }
    }

    internal class NewTypeSettableAttributes : ExtensionsData, INewTypeSettableAttributes
    {
        /// <inheritdoc/>
        public bool? CanSetId { get; set; }

        /// <inheritdoc/>
        public bool? CanSetLocalName { get; set; }

        /// <inheritdoc/>
        public bool? CanSetLocalNamespace { get; set; }

        /// <inheritdoc/>
        public bool? CanSetDisplayName { get; set; }

        /// <inheritdoc/>
        public bool? CanSetQueryName { get; set; }

        /// <inheritdoc/>
        public bool? CanSetDescription { get; set; }

        /// <inheritdoc/>
        public bool? CanSetCreatable { get; set; }

        /// <inheritdoc/>
        public bool? CanSetFileable { get; set; }

        /// <inheritdoc/>
        public bool? CanSetQueryable { get; set; }

        /// <inheritdoc/>
        public bool? CanSetFulltextIndexed { get; set; }

        /// <inheritdoc/>
        public bool? CanSetIncludedInSupertypeQuery { get; set; }

        /// <inheritdoc/>
        public bool? CanSetControllablePolicy { get; set; }

        /// <inheritdoc/>
        public bool? CanSetControllableAcl { get; set; }
    }

    internal class AclCapabilities : ExtensionsData, IAclCapabilities
    {
        /// <inheritdoc/>
        public SupportedPermissions? SupportedPermissions { get; set; }

        /// <inheritdoc/>
        public AclPropagation? AclPropagation { get; set; }

        /// <inheritdoc/>
        public IList<IPermissionDefinition> Permissions { get; set; }

        /// <inheritdoc/>
        public IDictionary<string, IPermissionMapping> PermissionMapping { get; set; }
    }

    internal class PermissionDefinition : ExtensionsData, IPermissionDefinition
    {
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }
    }

    internal class PermissionMapping : ExtensionsData, IPermissionMapping
    {
        /// <inheritdoc/>
        public string Key { get; set; }

        /// <inheritdoc/>
        public IList<string> Permissions { get; set; }
    }

    public class ExtensionFeature : ExtensionsData, IExtensionFeature
    {
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string Url { get; set; }

        /// <inheritdoc/>
        public string CommonName { get; set; }

        /// <inheritdoc/>
        public string VersionLabel { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string LocalName { get; set; }

        /// <inheritdoc/>
        public string LocalNamespace { get; set; }

        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public string QueryName { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public PropertyType PropertyType { get; set; }

        /// <inheritdoc/>
        public Cardinality? Cardinality { get; set; }

        /// <inheritdoc/>
        public Updatability? Updatability { get; set; }

        /// <inheritdoc/>
        public bool? IsInherited { get; set; }

        /// <inheritdoc/>
        public bool? IsRequired { get; set; }

        /// <inheritdoc/>
        public bool? IsQueryable { get; set; }

        /// <inheritdoc/>
        public bool? IsOrderable { get; set; }

        /// <inheritdoc/>
        public bool? IsOpenChoice { get; set; }
    }

    public class Choice<T> : IChoice<T>
    {
        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public IList<T> Value { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<T>> Choices { get; set; }
    }

    public class PropertyBooleanDefinition : PropertyDefinition, IPropertyBooleanDefinition
    {
        /// <inheritdoc/>
        public IList<bool?> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<bool?>> Choices { get; set; }
    }

    public class PropertyDateTimeDefinition : PropertyDefinition, IPropertyDateTimeDefinition
    {
        /// <inheritdoc/>
        public IList<DateTime?> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<DateTime?>> Choices { get; set; }

        /// <inheritdoc/>
        public DateTimeResolution? DateTimeResolution { get; set; }
    }

    public class PropertyDecimalDefinition : PropertyDefinition, IPropertyDecimalDefinition
    {
        /// <inheritdoc/>
        public IList<decimal?> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<decimal?>> Choices { get; set; }

        /// <inheritdoc/>
        public decimal? MinValue { get; set; }

        /// <inheritdoc/>
        public decimal? MaxValue { get; set; }

        /// <inheritdoc/>
        public DecimalPrecision? Precision { get; set; }
    }

    public class PropertyHtmlDefinition : PropertyDefinition, IPropertyHtmlDefinition
    {
        /// <inheritdoc/>
        public IList<string> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<string>> Choices { get; set; }
    }

    public class PropertyIdDefinition : PropertyDefinition, IPropertyIdDefinition
    {
        /// <inheritdoc/>
        public IList<string> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<string>> Choices { get; set; }
    }

    public class PropertyIntegerDefinition : PropertyDefinition, IPropertyIntegerDefinition
    {
        /// <inheritdoc/>
        public IList<BigInteger?> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<BigInteger?>> Choices { get; set; }

        /// <inheritdoc/>
        public BigInteger? MinValue { get; set; }

        /// <inheritdoc/>
        public BigInteger? MaxValue { get; set; }
    }

    public class PropertyStringDefinition : PropertyDefinition, IPropertyStringDefinition
    {
        /// <inheritdoc/>
        public IList<string> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<string>> Choices { get; set; }

        /// <inheritdoc/>
        public BigInteger? MaxLength { get; set; }
    }

    public class PropertyUriDefinition : PropertyDefinition, IPropertyUriDefinition
    {
        /// <inheritdoc/>
        public IList<string> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<string>> Choices { get; set; }
    }

    public class ObjectData : ExtensionsData, IObjectData
    {
        /// <inheritdoc/>
        public string Id
        {
            get
            {
                return GetFirstValue(PropertyIds.ObjectId) as string;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IProperties Properties { get; set; }

        /// <inheritdoc/>
        public IAllowableActions AllowableActions { get; set; }

        /// <inheritdoc/>
        public IList<IObjectData> Relationships { get; set; }

        /// <inheritdoc/>
        public IChangeEventInfo ChangeEventInfo { get; set; }

        /// <inheritdoc/>
        public IAcl Acl { get; set; }

        /// <inheritdoc/>
        public bool? IsExactAcl { get; set; }

        /// <inheritdoc/>
        public IPolicyIdList PolicyIds { get; set; }

        /// <inheritdoc/>
        public IList<IRenditionData> Renditions { get; set; }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public IList<IObjectData> Objects { get; set; }

        /// <inheritdoc/>
        public bool? HasMoreItems { get; set; }

        /// <inheritdoc/>
        public BigInteger? NumItems { get; set; }
    }

    public class ObjectInFolderData : ExtensionsData, IObjectInFolderData
    {
        /// <inheritdoc/>
        public IObjectData Object { get; set; }

        /// <inheritdoc/>
        public string PathSegment { get; set; }
    }

    public class ObjectInFolderList : ExtensionsData, IObjectInFolderList
    {
        /// <inheritdoc/>
        public IList<IObjectInFolderData> Objects { get; set; }

        /// <inheritdoc/>
        public bool? HasMoreItems { get; set; }

        /// <inheritdoc/>
        public BigInteger? NumItems { get; set; }
    }

    public class ObjectInFolderContainer : ExtensionsData, IObjectInFolderContainer
    {
        /// <inheritdoc/>
        public IObjectInFolderData Object { get; set; }

        /// <inheritdoc/>
        public IList<IObjectInFolderContainer> Children { get; set; }
    }

    public class ObjectParentData : ExtensionsData, IObjectParentData
    {
        /// <inheritdoc/>
        public IObjectData Object { get; set; }

        /// <inheritdoc/>
        public string RelativePathSegment { get; set; }
    }

    public class Properties : ExtensionsData, IProperties
    {
        private List<IPropertyData> propertyList = new List<IPropertyData>();
        private Dictionary<string, IPropertyData> propertyDict = new Dictionary<string, IPropertyData>();

        /// <inheritdoc/>
        public IPropertyData this[string propertyId]
        {
            get
            {
                IPropertyData property = null;
                propertyDict.TryGetValue(propertyId, out property);
                return property;
            }
        }

        public Properties() { }

        public Properties(IProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            AddProperties(properties.PropertyList);
            Extensions = properties.Extensions;
        }

        /// <inheritdoc/>
        public IList<IPropertyData> PropertyList
        {
            get
            {
                return propertyList;
            }
        }

        protected void AddProperties(ICollection<IPropertyData> properties)
        {
            if (properties != null)
            {
                foreach (IPropertyData property in properties)
                {
                    AddProperty(property);
                }
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

        public void ReplaceProperty(IPropertyData property)
        {
            if (property == null || property.Id == null)
            {
                return;
            }

            RemoveProperty(property.Id);

            propertyList.Add(property);
            propertyDict[property.Id] = property;
        }

        public void RemoveProperty(string id)
        {
            if (id == null)
            {
                return;
            }

            int idx = propertyList.FindIndex(pd => pd.Id == id);
            if (idx > -1)
            {
                propertyList.RemoveAt(idx);
            }

            propertyDict.Remove(id);
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
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string LocalName { get; set; }

        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public string QueryName { get; set; }

        /// <inheritdoc/>
        public PropertyType PropertyType { get; protected set; }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public string Id { get; set; }
    }

    public class Ace : ExtensionsData, IAce
    {
        /// <inheritdoc/>
        public IPrincipal Principal { get; set; }

        /// <inheritdoc/>
        public string PrincipalId { get { return Principal == null ? null : Principal.Id; } }

        /// <inheritdoc/>
        public IList<string> Permissions { get; set; }

        /// <inheritdoc/>
        public bool IsDirect { get; set; }
    }

    public class Acl : ExtensionsData, IAcl
    {
        /// <inheritdoc/>
        public IList<IAce> Aces { get; set; }

        /// <inheritdoc/>
        public bool? IsExact { get; set; }
    }

    public class ContentStream : ExtensionsData, IContentStream
    {
        /// <inheritdoc/>
        public BigInteger? Length { get; set; }

        /// <inheritdoc/>
        public string MimeType { get; set; }

        /// <inheritdoc/>
        public string FileName { get; set; }

        /// <inheritdoc/>
        public Stream Stream { get; set; }
    }

    public class PartialContentStream : ContentStream, IPartialContentStream
    {
    }

    public class AllowableActions : ExtensionsData, IAllowableActions
    {
        /// <inheritdoc/>
        public ISet<PortCMIS.Enums.Action> Actions { get; set; }
    }

    public class RenditionData : ExtensionsData, IRenditionData
    {
        /// <inheritdoc/>
        public string StreamId { get; set; }

        /// <inheritdoc/>
        public string MimeType { get; set; }

        /// <inheritdoc/>
        public BigInteger? Length { get; set; }

        /// <inheritdoc/>
        public string Kind { get; set; }

        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public BigInteger? Height { get; set; }

        /// <inheritdoc/>
        public BigInteger? Width { get; set; }

        /// <inheritdoc/>
        public string RenditionDocumentId { get; set; }
    }

    public class ChangeEventInfo : ExtensionsData, IChangeEventInfo
    {
        /// <inheritdoc/>
        public ChangeType? ChangeType { get; set; }

        /// <inheritdoc/>
        public DateTime? ChangeTime { get; set; }
    }

    public class PolicyIdList : ExtensionsData, IPolicyIdList
    {
        /// <inheritdoc/>
        public IList<string> PolicyIds { get; set; }
    }

    internal class FailedToDeleteData : ExtensionsData, IFailedToDeleteData
    {
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string NewId { get; set; }

        /// <inheritdoc/>
        public string ChangeToken { get; set; }
    }

    internal class BulkUpdate : ExtensionsData
    {
        public IList<IBulkUpdateObjectIdAndChangeToken> ObjectIdAndChangeToken { get; set; }
        public Properties Properties { get; set; }
        public IList<string> AddSecondaryTypeIds { get; set; }
        public IList<string> RemoveSecondaryTypeIds { get; set; }
    }
}
