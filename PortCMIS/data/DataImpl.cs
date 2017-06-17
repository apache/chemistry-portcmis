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
    /// <summary>
    /// Repository Info implementation.
    /// </summary>
    public class RepositoryInfo : ExtensionsData, IRepositoryInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RepositoryInfo()
        {
        }

        /// <summary>
        /// Copy Constructor.
        /// </summary>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return "RepositoryInfo: " + Id;
        }
    }

    /// <summary>
    /// Repository Capabilities implementation.
    /// </summary>
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

    /// <summary>
    /// Extension Feature implementation.
    /// </summary>
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

    /// <summary>
    /// Base type definition implementation.
    /// </summary>
    public abstract class AbstractTypeDefinition : ExtensionsData, ITypeDefinition
    {
        private List<IPropertyDefinition> propertyDefintionList = new List<IPropertyDefinition>();
        private Dictionary<string, IPropertyDefinition> propertyDefintionDict = new Dictionary<string, IPropertyDefinition>();
        private string parentTypeId;

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
        public BaseTypeId BaseTypeId { get; set; }

        /// <inheritdoc/>
        public string ParentTypeId
        {
            get { return parentTypeId; }
            set { parentTypeId = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <inheritdoc/>
        public bool? IsCreatable { get; set; }

        /// <inheritdoc/>
        public bool? IsFileable { get; set; }

        /// <inheritdoc/>
        public bool? IsQueryable { get; set; }

        /// <inheritdoc/>
        public bool? IsFulltextIndexed { get; set; }

        /// <inheritdoc/>
        public bool? IsIncludedInSupertypeQuery { get; set; }

        /// <inheritdoc/>
        public bool? IsControllablePolicy { get; set; }

        /// <inheritdoc/>
        public bool? IsControllableAcl { get; set; }

        /// <inheritdoc/>
        public IPropertyDefinition this[string propertyId]
        {
            get
            {
                IPropertyDefinition propertyDefinition = null;
                propertyDefintionDict.TryGetValue(propertyId, out propertyDefinition);
                return propertyDefinition;
            }
        }

        /// <inheritdoc/>
        public IList<IPropertyDefinition> PropertyDefinitions
        {
            get
            {
                return propertyDefintionList;
            }
        }

        /// <inheritdoc/>
        public ITypeMutability TypeMutability { get; set; }

        /// <summary>
        /// Initializes the type definition object.
        /// </summary>
        /// <param name="typeDefinition">the type definition</param>
        protected void Initialize(ITypeDefinition typeDefinition)
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

        /// <summary>
        /// Adds a property type definition.
        /// </summary>
        /// <param name="propertyDefinition">the property type definition</param>
        public void AddPropertyDefinition(IPropertyDefinition propertyDefinition)
        {
            if (propertyDefinition == null || propertyDefinition.Id == null)
            {
                return;
            }
            propertyDefintionList.Add(propertyDefinition);
            propertyDefintionDict[propertyDefinition.Id] = propertyDefinition;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "TypeDefinition: " + BaseTypeId + " (" + Id + ")";
        }
    }

    /// <summary>
    /// Type mutability implementation.
    /// </summary>
    public class TypeMutability : ExtensionsData, ITypeMutability
    {
        /// <inheritdoc/>
        public bool? CanCreate { get; set; }

        /// <inheritdoc/>
        public bool? CanUpdate { get; set; }

        /// <inheritdoc/>
        public bool? CanDelete { get; set; }
    }

    /// <summary>
    /// Document type definition implementation.
    /// </summary>
    public class DocumentTypeDefinition : AbstractTypeDefinition, IDocumentTypeDefinition
    {
        /// <inheritdoc/>
        public bool? IsVersionable { get; set; }

        /// <inheritdoc/>
        public ContentStreamAllowed? ContentStreamAllowed { get; set; }
    }

    /// <summary>
    /// Folder type definition implementation.
    /// </summary>
    public class FolderTypeDefinition : AbstractTypeDefinition, IFolderTypeDefinition
    {
    }

    /// <summary>
    /// Policy type definition implementation.
    /// </summary>
    public class PolicyTypeDefinition : AbstractTypeDefinition, IPolicyTypeDefinition
    {
    }

    /// <summary>
    /// Item type definition implementation.
    /// </summary>
    public class ItemTypeDefinition : AbstractTypeDefinition, IItemTypeDefinition
    {
    }

    /// <summary>
    /// Secondary type definition implementation.
    /// </summary>
    public class SecondaryTypeDefinition : AbstractTypeDefinition, ISecondaryTypeDefinition
    {
    }

    /// <summary>
    /// Relationship type definition implementation.
    /// </summary>
    public class RelationshipTypeDefinition : AbstractTypeDefinition, IRelationshipTypeDefinition
    {
        /// <inheritdoc/>
        public IList<string> AllowedSourceTypeIds { get; set; }

        /// <inheritdoc/>
        public IList<string> AllowedTargetTypeIds { get; set; }
    }

    /// <summary>
    /// Type Definition List implementation.
    /// </summary>
    public class TypeDefinitionList : ExtensionsData, ITypeDefinitionList
    {
        /// <inheritdoc/>
        public IList<ITypeDefinition> List { get; set; }

        /// <inheritdoc/>
        public bool? HasMoreItems { get; set; }

        /// <inheritdoc/>
        public BigInteger? NumItems { get; set; }
    }

    /// <summary>
    /// Type Definition Container implementation.
    /// </summary>
    public class TypeDefinitionContainer : ExtensionsData, ITypeDefinitionContainer
    {
        /// <inheritdoc/>
        public ITypeDefinition TypeDefinition { get; set; }

        /// <inheritdoc/>
        public IList<ITypeDefinitionContainer> Children { get; set; }
    }

    /// <summary>
    /// Property definition implementation.
    /// </summary>
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

    /// <summary>
    /// Choice implementation.
    /// </summary>
    public class Choice<T> : IChoice<T>
    {
        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public IList<T> Value { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<T>> Choices { get; set; }
    }

    /// <summary>
    /// Boolean property definition implementation.
    /// </summary>
    public class PropertyBooleanDefinition : PropertyDefinition, IPropertyBooleanDefinition
    {
        /// <inheritdoc/>
        public IList<bool?> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<bool?>> Choices { get; set; }
    }

    /// <summary>
    /// DataTime property definition implementation.
    /// </summary>
    public class PropertyDateTimeDefinition : PropertyDefinition, IPropertyDateTimeDefinition
    {
        /// <inheritdoc/>
        public IList<DateTime?> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<DateTime?>> Choices { get; set; }

        /// <inheritdoc/>
        public DateTimeResolution? DateTimeResolution { get; set; }
    }

    /// <summary>
    /// Decimal property definition implementation.
    /// </summary>
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

    /// <summary>
    /// HTML property definition implementation.
    /// </summary>
    public class PropertyHtmlDefinition : PropertyDefinition, IPropertyHtmlDefinition
    {
        /// <inheritdoc/>
        public IList<string> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<string>> Choices { get; set; }
    }

    /// <summary>
    /// ID property definition implementation.
    /// </summary>
    public class PropertyIdDefinition : PropertyDefinition, IPropertyIdDefinition
    {
        /// <inheritdoc/>
        public IList<string> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<string>> Choices { get; set; }
    }

    /// <summary>
    /// Integer property definition implementation.
    /// </summary>
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

    /// <summary>
    /// String property definition implementation.
    /// </summary>
    public class PropertyStringDefinition : PropertyDefinition, IPropertyStringDefinition
    {
        /// <inheritdoc/>
        public IList<string> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<string>> Choices { get; set; }

        /// <inheritdoc/>
        public BigInteger? MaxLength { get; set; }
    }

    /// <summary>
    /// URI property definition implementation.
    /// </summary>
    public class PropertyUriDefinition : PropertyDefinition, IPropertyUriDefinition
    {
        /// <inheritdoc/>
        public IList<string> DefaultValue { get; set; }

        /// <inheritdoc/>
        public IList<IChoice<string>> Choices { get; set; }
    }

    /// <summary>
    /// Object Data implementation.
    /// </summary>
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

    /// <summary>
    /// Object List implementation.
    /// </summary>
    public class ObjectList : ExtensionsData, IObjectList
    {
        /// <inheritdoc/>
        public IList<IObjectData> Objects { get; set; }

        /// <inheritdoc/>
        public bool? HasMoreItems { get; set; }

        /// <inheritdoc/>
        public BigInteger? NumItems { get; set; }
    }

    /// <summary>
    /// Object In Folder Data implementation.
    /// </summary>
    public class ObjectInFolderData : ExtensionsData, IObjectInFolderData
    {
        /// <inheritdoc/>
        public IObjectData Object { get; set; }

        /// <inheritdoc/>
        public string PathSegment { get; set; }
    }

    /// <summary>
    /// Object In Folder List implementation.
    /// </summary>
    public class ObjectInFolderList : ExtensionsData, IObjectInFolderList
    {
        /// <inheritdoc/>
        public IList<IObjectInFolderData> Objects { get; set; }

        /// <inheritdoc/>
        public bool? HasMoreItems { get; set; }

        /// <inheritdoc/>
        public BigInteger? NumItems { get; set; }
    }

    /// <summary>
    /// Object In Folder Container implementation.
    /// </summary>
    public class ObjectInFolderContainer : ExtensionsData, IObjectInFolderContainer
    {
        /// <inheritdoc/>
        public IObjectInFolderData Object { get; set; }

        /// <inheritdoc/>
        public IList<IObjectInFolderContainer> Children { get; set; }
    }

    /// <summary>
    /// Object Parent Data implementation.
    /// </summary>
    public class ObjectParentData : ExtensionsData, IObjectParentData
    {
        /// <inheritdoc/>
        public IObjectData Object { get; set; }

        /// <inheritdoc/>
        public string RelativePathSegment { get; set; }
    }

    /// <summary>
    /// Properties implementation.
    /// </summary>
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

        /// <summary>
        /// Constructor.
        /// </summary>
        public Properties() { }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="properties">source properties object</param>
        public Properties(IProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
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

        /// <summary>
        /// Adds a collection of properties.
        /// </summary>
        /// <param name="properties">the collection of properties to add</param>
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

        /// <summary>
        /// Adds a property.
        /// </summary>
        /// <param name="property">the property to add.</param>
        public void AddProperty(IPropertyData property)
        {
            if (property == null || property.Id == null)
            {
                return;
            }
            propertyList.Add(property);
            propertyDict[property.Id] = property;
        }

        /// <summary>
        /// Replaces a property.
        /// </summary>
        /// <param name="property">the property to replace</param>
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

        /// <summary>
        /// Removes a property.
        /// </summary>
        /// <param name="id">the ID of the property that should be removed</param>
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

        /// <inheritdoc/>
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

    /// <summary>
    /// Property Data implementation.
    /// </summary>
    public class PropertyData : ExtensionsData, IPropertyData
    {
        private IList<object> values;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyType">The property type of this property</param>
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

        /// <summary>
        /// Adds a value to this property.
        /// </summary>
        /// <param name="value">the value</param>
        public void AddValue(object value)
        {
            object newValue = CheckValue(value);
            if (Values == null)
            {
                Values = new List<object>();
            }
            Values.Add(newValue);
        }

        /// <summary>
        /// Checks if the given value matches the property type and converts the value if necessary.
        /// </summary>
        /// <param name="value">the value</param>
        /// <returns>the converted value</returns>
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
                        return new BigInteger(Convert.ToInt64(value));
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
                    if (!(value is decimal || value is double || value is float || value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long))
                    {
                        throw new ArgumentException("Property '" + Id + "' is a Decimal property!");
                    }
                    return Convert.ToDecimal(value);
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return Id + ": " + values;
        }
    }

    /// <summary>
    /// Principal implementation.
    /// </summary>
    public class Principal : ExtensionsData, IPrincipal
    {
        /// <inheritdoc/>
        public string Id { get; set; }
    }

    /// <summary>
    /// ACE implementation.
    /// </summary>
    public class Ace : ExtensionsData, IAce
    {
        /// <inheritdoc/>
        public IPrincipal Principal { get; set; }

        /// <inheritdoc/>
        public string PrincipalId { get { return Principal?.Id; } }

        /// <inheritdoc/>
        public IList<string> Permissions { get; set; }

        /// <inheritdoc/>
        public bool IsDirect { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return PrincipalId + ": " + (Permissions == null ? "?" : String.Join(", ", Permissions));
        }
    }

    /// <summary>
    /// ACL implementation.
    /// </summary>
    public class Acl : ExtensionsData, IAcl
    {
        /// <inheritdoc/>
        public IList<IAce> Aces { get; set; }

        /// <inheritdoc/>
        public bool? IsExact { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "ACL: " + (Aces == null ? "empty" : String.Join(" | ", Aces));
        }
    }

    /// <summary>
    /// Simple Content Stream implementation.
    /// </summary>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Content Stream : " + FileName + " (" + MimeType + ") " +
                (Length == null ? "" : Length + " bytes" +
                (Stream == null ? " no stream" : Stream.ToString()));
        }
    }
    /// <summary>
    /// Simple Content Stream implementation that indicates a partial stream.
    /// </summary>
    public class PartialContentStream : ContentStream, IPartialContentStream
    {
    }

    /// <summary>
    /// Allowable Actions implementation.
    /// </summary>
    public class AllowableActions : ExtensionsData, IAllowableActions
    {
        /// <inheritdoc/>
        public ISet<PortCMIS.Enums.Action> Actions { get; set; }
    }

    /// <summary>
    /// Rendition Data implementation.
    /// </summary>
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

    /// <summary>
    /// Chang Event Info implementation.
    /// </summary>
    public class ChangeEventInfo : ExtensionsData, IChangeEventInfo
    {
        /// <inheritdoc/>
        public ChangeType? ChangeType { get; set; }

        /// <inheritdoc/>
        public DateTime? ChangeTime { get; set; }
    }

    /// <summary>
    /// Policy ID List implementation.
    /// </summary>
    public class PolicyIdList : ExtensionsData, IPolicyIdList
    {
        /// <inheritdoc/>
        public IList<string> PolicyIds { get; set; }
    }

    /// <summary>
    /// Failed To Delete implementation.
    /// </summary>
    public class FailedToDeleteData : ExtensionsData, IFailedToDeleteData
    {
        /// <inheritdoc/>
        public IList<string> Ids { get; set; }
    }

    internal class QueryType : ExtensionsData
    {
        public string Statement { get; set; }
        public bool? SearchAllVersions { get; set; }
        public bool? IncludeAllowableActions { get; set; }
        public IncludeRelationships? IncludeRelationships { get; set; }
        public string RenditionFilter { get; set; }
        public BigInteger? MaxItems { get; set; }
        public BigInteger? SkipCount { get; set; }
    }

    /// <summary>
    /// Bulk Update data implementation.
    /// </summary>
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
        public IProperties Properties { get; set; }
        public IList<string> AddSecondaryTypeIds { get; set; }
        public IList<string> RemoveSecondaryTypeIds { get; set; }
    }
}
