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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortCMIS.Binding.Browser.Json;
using PortCMIS.Data;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using PortCMIS.Data.Extensions;
using System.Collections;
using System.Numerics;
using PortCMIS.Const;

namespace PortCMIS.Binding.Browser
{
    internal class JsonConverter
    {
        public enum PropertyMode
        {
            Object,
            Query,
            Change
        }

        /// <summary>
        /// Converts a repository info object.
        /// </summary>
        internal static JsonObject Convert(IRepositoryInfo repositoryInfo, string repositoryUrl,
                string rootUrl, bool addExtendedDatetimeExtensionFeature)
        {
            if (repositoryInfo == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            result.Add(BrowserConstants.JsonRepInfoId, repositoryInfo.Id);
            result.Add(BrowserConstants.JsonRepInfoName, repositoryInfo.Name);
            result.Add(BrowserConstants.JsonRepInfoDescription, repositoryInfo.Description);
            result.Add(BrowserConstants.JsonRepInfoVendor, repositoryInfo.VendorName);
            result.Add(BrowserConstants.JsonRepInfoProduct, repositoryInfo.ProductName);
            result.Add(BrowserConstants.JsonRepInfoProductVersion, repositoryInfo.ProductVersion);
            result.Add(BrowserConstants.JsonRepInfoRootFolderId, repositoryInfo.RootFolderId);
            result.Add(BrowserConstants.JsonRepInfoCapabilities, Convert(repositoryInfo.Capabilities));
            SetIfNotNull(BrowserConstants.JsonRepInfoAclCapabilities, Convert(repositoryInfo.AclCapabilities), result);
            result.Add(BrowserConstants.JsonRepInfoChangeLogToken, repositoryInfo.LatestChangeLogToken);
            result.Add(BrowserConstants.JsonRepInfoCmisVersionSupported, repositoryInfo.CmisVersionSupported);
            SetIfNotNull(BrowserConstants.JsonRepInfoThinClientUri, repositoryInfo.ThinClientUri, result);
            SetIfNotNull(BrowserConstants.JsonRepInfoChangesIncomplete, repositoryInfo.ChangesIncomplete, result);

            JsonArray changesOnType = new JsonArray();
            if (repositoryInfo.ChangesOnType != null)
            {
                foreach (BaseTypeId? type in repositoryInfo.ChangesOnType)
                {
                    if (type != null)
                    {
                        changesOnType.Add(GetJsonStringValue(type.GetCmisValue()));
                    }
                }
            }
            result.Add(BrowserConstants.JsonRepInfoChangesOnType, changesOnType);

            SetIfNotNull(BrowserConstants.JsonRepInfoPrincipalIdAnonymous, repositoryInfo.PrincipalIdAnonymous, result);
            SetIfNotNull(BrowserConstants.JsonRepInfoPrincipalIdAnyone, repositoryInfo.PrincipalIdAnyone, result);

            if (IsNotEmpty(repositoryInfo.ExtensionFeatures))
            {
                JsonArray extendedFeatures = new JsonArray();

                foreach (ExtensionFeature feature in repositoryInfo.ExtensionFeatures)
                {
                    extendedFeatures.Add(Convert(feature));
                }

                result.Add(BrowserConstants.JsonRepInfoExtendedFeatures, extendedFeatures);
            }

            if (addExtendedDatetimeExtensionFeature)
            {
                object extendedFeatures;
                if (!result.TryGetValue(BrowserConstants.JsonRepInfoExtendedFeatures, out extendedFeatures))
                {
                    extendedFeatures = new JsonArray();
                    result.Add(BrowserConstants.JsonRepInfoExtendedFeatures, extendedFeatures);
                }

                if (extendedFeatures is JsonArray)
                {
                    ((JsonArray)extendedFeatures).Add(Convert(ExtensionFeatures.ExtendedDatetimeFormat));
                }
            }

            result.Add(BrowserConstants.JsonRepInfoRepositoryUrl, repositoryUrl);
            result.Add(BrowserConstants.JsonRepInfoRootFolderUrl, rootUrl);

            ConvertExtension(repositoryInfo, result);

            return result;
        }

        private static JsonObject Convert(IExtensionFeature feature)
        {
            if (feature == null)
            {
                return null;
            }

            JsonObject jsonFeature = new JsonObject();

            SetIfNotNull(BrowserConstants.JsonFeatureId, feature.Id, jsonFeature);
            SetIfNotNull(BrowserConstants.JsonFeatureUrl, feature.Url, jsonFeature);
            SetIfNotNull(BrowserConstants.JsonFeatureCommonName, feature.CommonName, jsonFeature);
            SetIfNotNull(BrowserConstants.JsonFeatureVersionLabel, feature.VersionLabel, jsonFeature);
            SetIfNotNull(BrowserConstants.JsonFeatureDescription, feature.Description, jsonFeature);

            if (IsNotEmpty(feature.FeatureData))
            {
                JsonObject data = new JsonObject();
                foreach (KeyValuePair<string, string> kv in feature.FeatureData)
                {
                    data.Add(kv.Key, kv.Value);
                }
                jsonFeature.Add(BrowserConstants.JsonFeatureData, data);
            }

            ConvertExtension(feature, jsonFeature);

            return jsonFeature;
        }

        /// <summary>
        /// Converts a capabilities object.
        /// </summary>
        internal static JsonObject Convert(IRepositoryCapabilities capabilities)
        {
            if (capabilities == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            result.Add(BrowserConstants.JsonCapContentStreamUpdatability, GetJsonEnumValue(capabilities.ContentStreamUpdatesCapability));
            result.Add(BrowserConstants.JsonCapChanges, GetJsonEnumValue(capabilities.ChangesCapability));
            result.Add(BrowserConstants.JsonCapRenditions, GetJsonEnumValue(capabilities.RenditionsCapability));
            result.Add(BrowserConstants.JsonCapGetDescendants, capabilities.IsGetDescendantsSupported);
            result.Add(BrowserConstants.JsonCapGetFolderTree, capabilities.IsGetFolderTreeSupported);
            result.Add(BrowserConstants.JsonCapMultifiling, capabilities.IsMultifilingSupported);
            result.Add(BrowserConstants.JsonCapUnfiling, capabilities.IsUnfilingSupported);
            result.Add(BrowserConstants.JsonCapVersionSpecificFiling, capabilities.IsVersionSpecificFilingSupported);
            result.Add(BrowserConstants.JsonCapPwcSearchable, capabilities.IsPwcSearchableSupported);
            result.Add(BrowserConstants.JsonCapPwcUpdatable, capabilities.IsPwcUpdatableSupported);
            result.Add(BrowserConstants.JsonCapAllVersionsSerachable, capabilities.IsAllVersionsSearchableSupported);
            result.Add(BrowserConstants.JsonCapOrderBy, GetJsonEnumValue(capabilities.OrderByCapability));
            result.Add(BrowserConstants.JsonCapQuery, GetJsonEnumValue(capabilities.QueryCapability));
            result.Add(BrowserConstants.JsonCapJoin, GetJsonEnumValue(capabilities.JoinCapability));
            result.Add(BrowserConstants.JsonCapAcl, GetJsonEnumValue(capabilities.AclCapability));

            if (capabilities.CreatablePropertyTypes != null)
            {
                ICreatablePropertyTypes creatablePropertyTypes = capabilities.CreatablePropertyTypes;

                JsonObject creatablePropertyTypesJson = new JsonObject();

                if (creatablePropertyTypes.CanCreate != null)
                {
                    JsonArray canCreate = new JsonArray();
                    foreach (PropertyType propType in creatablePropertyTypes.CanCreate)
                    {
                        canCreate.Add(propType.GetCmisValue());
                    }
                    creatablePropertyTypesJson.Add(BrowserConstants.JsonCapCreatablePropertyTypesCanCreate, canCreate);
                }

                ConvertExtension(creatablePropertyTypes, creatablePropertyTypesJson);

                result.Add(BrowserConstants.JsonCapCreatablePropertyTypes, creatablePropertyTypesJson);
            }

            if (capabilities.NewTypeSettableAttributes != null)
            {
                INewTypeSettableAttributes newTypeSettableAttributes = capabilities.NewTypeSettableAttributes;

                JsonObject newTypeSettableAttributesJson = new JsonObject();
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesId,
                        newTypeSettableAttributes.CanSetId);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesLocalName,
                        newTypeSettableAttributes.CanSetLocalName);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesLocalNameSpace,
                        newTypeSettableAttributes.CanSetLocalNamespace);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesDisplayName,
                        newTypeSettableAttributes.CanSetDisplayName);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesQueryName,
                        newTypeSettableAttributes.CanSetQueryName);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesDescription,
                        newTypeSettableAttributes.CanSetDescription);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesCreatable,
                        newTypeSettableAttributes.CanSetCreatable);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesFileable,
                        newTypeSettableAttributes.CanSetFileable);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesQueryable,
                        newTypeSettableAttributes.CanSetQueryable);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesFulltextIndexed,
                        newTypeSettableAttributes.CanSetFulltextIndexed);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesIncludeInSupertypeQuery,
                        newTypeSettableAttributes.CanSetIncludedInSupertypeQuery);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesControlablePolicy,
                        newTypeSettableAttributes.CanSetControllablePolicy);
                newTypeSettableAttributesJson.Add(BrowserConstants.JsonCapNewTypeSettableAttributesControlableAcl,
                        newTypeSettableAttributes.CanSetControllableAcl);

                ConvertExtension(newTypeSettableAttributes, newTypeSettableAttributesJson);

                result.Add(BrowserConstants.JsonCapNewTypeSettableAttributes, newTypeSettableAttributesJson);
            }

            ConvertExtension(capabilities, result);

            return result;
        }

        /// <summary>
        /// Converts an Acl capabilities object.
        /// </summary>
        internal static JsonObject Convert(IAclCapabilities capabilities)
        {
            if (capabilities == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            result.Add(BrowserConstants.JsonAclCapSupportedPermissions, GetJsonEnumValue(capabilities.SupportedPermissions));
            result.Add(BrowserConstants.JsonAclCapAclPropagation, GetJsonEnumValue(capabilities.AclPropagation));

            // permissions
            if (capabilities.Permissions != null)
            {
                JsonArray permissions = new JsonArray();

                foreach (PermissionDefinition permDef in capabilities.Permissions)
                {
                    JsonObject permission = new JsonObject();
                    permission.Add(BrowserConstants.JsonAclCapPermissionPermission, permDef.Id);
                    permission.Add(BrowserConstants.JsonAclCapPermissionDescription, permDef.Description);

                    permissions.Add(permission);
                }

                result.Add(BrowserConstants.JsonAclCapPermissions, permissions);
            }

            // permission mapping
            if (capabilities.PermissionMapping != null)
            {
                JsonArray permissionMapping = new JsonArray();

                foreach (PermissionMapping permMap in capabilities.PermissionMapping.Values)
                {
                    JsonArray mappingPermissions = new JsonArray();
                    if (permMap.Permissions != null)
                    {
                        foreach (string p in permMap.Permissions)
                        {
                            mappingPermissions.Add(p);
                        }
                    }

                    JsonObject mapping = new JsonObject();
                    mapping.Add(BrowserConstants.JsonAclCapMappingKey, permMap.Key);
                    mapping.Add(BrowserConstants.JsonAclCapMappingPermission, mappingPermissions);

                    permissionMapping.Add(mapping);
                }

                result.Add(BrowserConstants.JsonAclCapPermissionMapping, permissionMapping);
            }

            ConvertExtension(capabilities, result);

            return result;
        }

        internal static IRepositoryInfo ConvertRepositoryInfo(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            RepositoryInfoBrowserBinding result = new RepositoryInfoBrowserBinding();

            result.Id = GetString(json, BrowserConstants.JsonRepInfoId);
            result.Name = GetString(json, BrowserConstants.JsonRepInfoName);
            result.Description = GetString(json, BrowserConstants.JsonRepInfoDescription);
            result.VendorName = GetString(json, BrowserConstants.JsonRepInfoVendor);
            result.ProductName = GetString(json, BrowserConstants.JsonRepInfoProduct);
            result.ProductVersion = GetString(json, BrowserConstants.JsonRepInfoProductVersion);
            result.RootFolderId = GetString(json, BrowserConstants.JsonRepInfoRootFolderId);
            result.RepositoryUrl = GetString(json, BrowserConstants.JsonRepInfoRepositoryUrl);
            result.RootUrl = GetString(json, BrowserConstants.JsonRepInfoRootFolderUrl);
            result.Capabilities = ConvertRepositoryCapabilities(GetJsonObject(json, BrowserConstants.JsonRepInfoCapabilities));
            result.AclCapabilities = ConvertAclCapabilities(GetJsonObject(json, BrowserConstants.JsonRepInfoAclCapabilities));
            result.LatestChangeLogToken = GetString(json, BrowserConstants.JsonRepInfoChangeLogToken);
            result.CmisVersionSupported = GetString(json, BrowserConstants.JsonRepInfoCmisVersionSupported);
            result.ThinClientUri = GetString(json, BrowserConstants.JsonRepInfoThinClientUri);
            result.ChangesIncomplete = GetBoolean(json, BrowserConstants.JsonRepInfoChangesIncomplete);

            JsonArray changesOnType = GetJsonArray(json, BrowserConstants.JsonRepInfoChangesOnType);
            if (changesOnType != null)
            {
                IList<BaseTypeId?> types = new List<BaseTypeId?>();
                foreach (object type in changesOnType)
                {
                    if (type != null)
                    {
                        types.Add(type.ToString().GetCmisEnum<BaseTypeId>());
                    }
                }
                result.ChangesOnType = types;
            }

            result.PrincipalIdAnonymous = GetString(json, BrowserConstants.JsonRepInfoPrincipalIdAnonymous);
            result.PrincipalIdAnyone = GetString(json, BrowserConstants.JsonRepInfoPrincipalIdAnyone);

            JsonArray extendedFeatures = GetJsonArray(json, BrowserConstants.JsonRepInfoExtendedFeatures);
            if (extendedFeatures != null)
            {
                IList<IExtensionFeature> features = new List<IExtensionFeature>();

                foreach (object extendedFeature in extendedFeatures)
                {
                    JsonObject jsonFeature = GetJsonObject(extendedFeature);

                    ExtensionFeature feature = new ExtensionFeature();
                    feature.Id = GetString(jsonFeature, BrowserConstants.JsonFeatureId);
                    feature.Url = GetString(jsonFeature, BrowserConstants.JsonFeatureUrl);
                    feature.CommonName = GetString(jsonFeature, BrowserConstants.JsonFeatureCommonName);
                    feature.VersionLabel = GetString(jsonFeature, BrowserConstants.JsonFeatureVersionLabel);
                    feature.Description = GetString(jsonFeature, BrowserConstants.JsonFeatureDescription);

                    JsonObject data = GetJsonObject(jsonFeature, BrowserConstants.JsonFeatureData);
                    if (data != null)
                    {
                        IDictionary<string, string> dataMap = new Dictionary<string, string>();
                        foreach (KeyValuePair<string, object> kv in data)
                        {
                            dataMap.Add(kv.Key, (kv.Value == null ? null : kv.Value.ToString()));
                        }

                        if (dataMap.Count > 0)
                        {
                            feature.FeatureData = dataMap;
                        }
                    }

                    ConvertExtension(jsonFeature, feature, BrowserConstants.FeatureKeys);

                    features.Add(feature);
                }

                if (features.Count > 0)
                {
                    result.ExtensionFeatures = features;
                }
            }

            // handle extensions
            ConvertExtension(json, result, BrowserConstants.RepInfoKeys);

            return result;
        }

        internal static IRepositoryCapabilities ConvertRepositoryCapabilities(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            RepositoryCapabilities result = new RepositoryCapabilities();

            result.ContentStreamUpdatesCapability = GetEnum<CapabilityContentStreamUpdates?>(json, BrowserConstants.JsonCapContentStreamUpdatability);
            result.ChangesCapability = GetEnum<CapabilityChanges?>(json, BrowserConstants.JsonCapChanges);
            result.RenditionsCapability = GetEnum<CapabilityRenditions?>(json, BrowserConstants.JsonCapRenditions);
            result.IsGetDescendantsSupported = GetBoolean(json, BrowserConstants.JsonCapGetDescendants);
            result.IsGetFolderTreeSupported = GetBoolean(json, BrowserConstants.JsonCapGetFolderTree);
            result.IsMultifilingSupported = GetBoolean(json, BrowserConstants.JsonCapMultifiling);
            result.IsUnfilingSupported = GetBoolean(json, BrowserConstants.JsonCapUnfiling);
            result.IsVersionSpecificFilingSupported = GetBoolean(json, BrowserConstants.JsonCapVersionSpecificFiling);
            result.IsPwcSearchableSupported = GetBoolean(json, BrowserConstants.JsonCapPwcSearchable);
            result.IsPwcUpdatableSupported = GetBoolean(json, BrowserConstants.JsonCapPwcUpdatable);
            result.IsAllVersionsSearchableSupported = GetBoolean(json, BrowserConstants.JsonCapAllVersionsSerachable);
            result.OrderByCapability = GetEnum<CapabilityOrderBy?>(json, BrowserConstants.JsonCapOrderBy);
            result.QueryCapability = GetEnum<CapabilityQuery?>(json, BrowserConstants.JsonCapQuery);
            result.JoinCapability = GetEnum<CapabilityJoin?>(json, BrowserConstants.JsonCapJoin);
            result.AclCapability = GetEnum<CapabilityAcl?>(json, BrowserConstants.JsonCapAcl);

            JsonObject creatablePropertyTypesJson = GetJsonObject(json, BrowserConstants.JsonCapCreatablePropertyTypes);
            if (creatablePropertyTypesJson != null)
            {
                CreatablePropertyTypes creatablePropertyTypes = new CreatablePropertyTypes();

                JsonArray canCreateJson = GetJsonArray(creatablePropertyTypesJson, BrowserConstants.JsonCapCreatablePropertyTypesCanCreate);
                if (canCreateJson != null)
                {
                    ISet<PropertyType> canCreate = new HashSet<PropertyType>();

                    foreach (object o in canCreateJson)
                    {
                        try
                        {
                            if (o != null)
                            {
                                canCreate.Add(o.ToString().GetCmisEnum<PropertyType>());
                            }
                        }
                        catch (Exception)
                        {
                            // ignore
                        }
                    }

                    creatablePropertyTypes.CanCreate = canCreate;
                }

                ConvertExtension(creatablePropertyTypesJson, creatablePropertyTypes, BrowserConstants.CapCreatablePropertyTypesKeys);

                result.CreatablePropertyTypes = creatablePropertyTypes;
            }

            JsonObject newTypeSettableAttributesJson = GetJsonObject(json, BrowserConstants.JsonCapNewTypeSettableAttributes);
            if (newTypeSettableAttributesJson != null)
            {
                NewTypeSettableAttributes newTypeSettableAttributes = new NewTypeSettableAttributes();

                newTypeSettableAttributes.CanSetId = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesId);
                newTypeSettableAttributes.CanSetLocalName = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesLocalName);
                newTypeSettableAttributes.CanSetLocalNamespace = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesLocalNameSpace);
                newTypeSettableAttributes.CanSetDisplayName = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesDisplayName);
                newTypeSettableAttributes.CanSetQueryName = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesQueryName);
                newTypeSettableAttributes.CanSetDescription = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesDescription);
                newTypeSettableAttributes.CanSetCreatable = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesCreatable);
                newTypeSettableAttributes.CanSetFileable = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesFileable);
                newTypeSettableAttributes.CanSetQueryable = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesQueryable);
                newTypeSettableAttributes.CanSetFulltextIndexed = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesFulltextIndexed);
                newTypeSettableAttributes.CanSetIncludedInSupertypeQuery = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesIncludeInSupertypeQuery);
                newTypeSettableAttributes.CanSetControllablePolicy = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesControlablePolicy);
                newTypeSettableAttributes.CanSetControllableAcl = GetBoolean(newTypeSettableAttributesJson,
                        BrowserConstants.JsonCapNewTypeSettableAttributesControlableAcl);

                ConvertExtension(newTypeSettableAttributesJson, newTypeSettableAttributes, BrowserConstants.CapNewTypeSettableAttributesKeys);

                result.NewTypeSettableAttributes = newTypeSettableAttributes;
            }

            // handle extensions
            ConvertExtension(json, result, BrowserConstants.CapKeys);

            return result;
        }

        internal static AclCapabilities ConvertAclCapabilities(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            AclCapabilities result = new AclCapabilities();

            result.SupportedPermissions = GetEnum<SupportedPermissions?>(json, BrowserConstants.JsonAclCapSupportedPermissions);
            result.AclPropagation = GetEnum<AclPropagation?>(json, BrowserConstants.JsonAclCapAclPropagation);

            JsonArray permissions = GetJsonArray(json, BrowserConstants.JsonAclCapPermissions);
            if (permissions != null)
            {
                IList<IPermissionDefinition> permissionDefinitionList = new List<IPermissionDefinition>();

                foreach (object permission in permissions)
                {
                    JsonObject permissionMap = GetJsonObject(permission);
                    if (permissionMap != null)
                    {
                        PermissionDefinition permDef = new PermissionDefinition();

                        permDef.Id = GetString(permissionMap, BrowserConstants.JsonAclCapPermissionPermission);
                        permDef.Description = GetString(permissionMap, BrowserConstants.JsonAclCapPermissionDescription);

                        ConvertExtension(permissionMap, permDef, BrowserConstants.AclCapPermissionKeys);

                        permissionDefinitionList.Add(permDef);
                    }
                }

                result.Permissions = permissionDefinitionList;
            }

            JsonArray permissionMapping = GetJsonArray(json, BrowserConstants.JsonAclCapPermissionMapping);
            if (permissionMapping != null)
            {
                IDictionary<string, IPermissionMapping> permMap = new Dictionary<string, IPermissionMapping>();

                foreach (object permission in permissionMapping)
                {
                    JsonObject permissionMap = GetJsonObject(permission);
                    if (permissionMap != null)
                    {
                        PermissionMapping mapping = new PermissionMapping();

                        string key = GetString(permissionMap, BrowserConstants.JsonAclCapMappingKey);
                        mapping.Key = key;

                        JsonArray perms = GetJsonArray(permissionMap, BrowserConstants.JsonAclCapMappingPermission);
                        if (perms != null)
                        {
                            IList<string> permList = new List<string>();

                            foreach (object perm in perms)
                            {
                                if (perm != null)
                                {
                                    permList.Add(perm.ToString());
                                }
                            }

                            mapping.Permissions = permList;
                        }

                        ConvertExtension(permissionMap, mapping, BrowserConstants.AclCapMappingKeys);

                        permMap.Add(key, mapping);
                    }
                }

                result.PermissionMapping = permMap;
            }

            // handle extensions
            ConvertExtension(json, result, BrowserConstants.AclCapKeys);

            return result;
        }

        internal static ITypeDefinition ConvertTypeDefinition(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            AbstractTypeDefinition result = null;

            string id = GetString(json, BrowserConstants.JsonTypeId);

            // find base type
            BaseTypeId? baseType = GetEnum<BaseTypeId?>(json, BrowserConstants.JsonTypeBaseId);
            if (baseType == null)
            {
                throw new CmisInvalidArgumentException("Invalid base type: " + id);
            }

            switch (baseType)
            {
                case BaseTypeId.CmisFolder:
                    result = new FolderTypeDefinition();
                    break;
                case BaseTypeId.CmisDocument:
                    result = new DocumentTypeDefinition();

                    ((DocumentTypeDefinition)result).ContentStreamAllowed = GetEnum<ContentStreamAllowed?>(json, BrowserConstants.JsonTypeContentstreamAllowed);
                    ((DocumentTypeDefinition)result).IsVersionable = GetBoolean(json, BrowserConstants.JsonTypeVersionable);

                    break;
                case BaseTypeId.CmisRelationship:
                    result = new RelationshipTypeDefinition();

                    JsonArray allowedSourceTypes = GetJsonArray(json, BrowserConstants.JsonTypeAllowedSourceTypes);
                    if (allowedSourceTypes != null)
                    {
                        IList<string> types = new List<string>();
                        foreach (object type in allowedSourceTypes)
                        {
                            if (type != null)
                            {
                                types.Add(type.ToString());
                            }
                        }

                        ((RelationshipTypeDefinition)result).AllowedSourceTypeIds = types;
                    }

                    JsonArray allowedTargetTypes = GetJsonArray(json, BrowserConstants.JsonTypeAllowedTargetTypes);
                    if (allowedTargetTypes != null)
                    {
                        IList<string> types = new List<string>();
                        foreach (object type in allowedTargetTypes)
                        {
                            if (type != null)
                            {
                                types.Add(type.ToString());
                            }
                        }

                        ((RelationshipTypeDefinition)result).AllowedTargetTypeIds = types;
                    }

                    break;
                case BaseTypeId.CmisPolicy:
                    result = new PolicyTypeDefinition();
                    break;
                case BaseTypeId.CmisItem:
                    result = new ItemTypeDefinition();
                    break;
                case BaseTypeId.CmisSecondary:
                    result = new SecondaryTypeDefinition();
                    break;
                default:
                    throw new CmisRuntimeException("Type '" + id + "' does not match a base type!");
            }

            result.BaseTypeId = (BaseTypeId)baseType;
            result.Description = GetString(json, BrowserConstants.JsonTypeDescription);
            result.DisplayName = GetString(json, BrowserConstants.JsonTypeDisplayname);
            result.Id = id;
            result.IsControllableAcl = GetBoolean(json, BrowserConstants.JsonTypeControlableAcl);
            result.IsControllablePolicy = GetBoolean(json, BrowserConstants.JsonTypeControlablePolicy);
            result.IsCreatable = GetBoolean(json, BrowserConstants.JsonTypeCreatable);
            result.IsFileable = GetBoolean(json, BrowserConstants.JsonTypeFileable);
            result.IsFulltextIndexed = GetBoolean(json, BrowserConstants.JsonTypeFulltextIndexed);
            result.IsIncludedInSupertypeQuery = GetBoolean(json, BrowserConstants.JsonTypeIncludeInSuperTypeQuery);
            result.IsQueryable = GetBoolean(json, BrowserConstants.JsonTypeQueryable);
            result.LocalName = GetString(json, BrowserConstants.JsonTypeLocalName);
            result.LocalNamespace = GetString(json, BrowserConstants.JsonTypeLocalNameSpace);
            result.ParentTypeId = GetString(json, BrowserConstants.JsonTypeParentId);
            result.QueryName = GetString(json, BrowserConstants.JsonTypeQueryName);

            JsonObject typeMutabilityJson = GetJsonObject(json, BrowserConstants.JsonTypeTypeMutability);
            if (typeMutabilityJson != null)
            {
                TypeMutability typeMutability = new TypeMutability();

                typeMutability.CanCreate = GetBoolean(typeMutabilityJson, BrowserConstants.JsonTypeTypeMutablilityCreate);
                typeMutability.CanUpdate = GetBoolean(typeMutabilityJson, BrowserConstants.JsonTypeTypeMutablilityUpdate);
                typeMutability.CanDelete = GetBoolean(typeMutabilityJson, BrowserConstants.JsonTypeTypeMutablilityDelete);

                ConvertExtension(typeMutabilityJson, typeMutability, BrowserConstants.JsonTypeTypeMutablilityKeys);

                result.TypeMutability = typeMutability;
            }

            JsonObject propertyDefinitions = GetJsonObject(json, BrowserConstants.JsonTypePropertyDefinitions);
            if (propertyDefinitions != null)
            {
                foreach (KeyValuePair<string, object> propDef in propertyDefinitions)
                {
                    result.AddPropertyDefinition(ConvertPropertyDefinition(GetJsonObject(propDef.Value)));
                }
            }

            // handle extensions
            ConvertExtension(json, result, BrowserConstants.TypeKeys);

            return result;
        }

        internal static IPropertyDefinition ConvertPropertyDefinition(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            PropertyDefinition result = null;

            string id = GetString(json, BrowserConstants.JsonPropertyId);

            // find property type
            PropertyType? propertyType = GetEnum<PropertyType?>(json, BrowserConstants.JsonPropertyTypePropertyType);
            if (propertyType == null)
            {
                throw new CmisRuntimeException("Invalid property type '" + id + "'! Data type not set!");
            }

            // find cardinality
            Cardinality? cardinality = GetEnum<Cardinality?>(json, BrowserConstants.JsonPropertyTypeCardinality);
            if (cardinality == null)
            {
                throw new CmisRuntimeException("Invalid property type '" + id + "'! Cardinality not set!");
            }

            // set specific values
            switch (propertyType)
            {
                case PropertyType.String:
                    result = new PropertyStringDefinition();
                    ((PropertyStringDefinition)result).MaxLength = GetInteger(json, BrowserConstants.JsonPropertyTypeMaxLength);
                    ((PropertyStringDefinition)result).Choices = ConvertChoicesString(GetJsonArray(json, BrowserConstants.JsonPropertyTypeChoice));
                    ((PropertyStringDefinition)result).DefaultValue = CopyDefaultValue<string>(json);
                    break;
                case PropertyType.Id:
                    result = new PropertyIdDefinition();
                    ((PropertyIdDefinition)result).Choices = ConvertChoicesString(GetJsonArray(json, BrowserConstants.JsonPropertyTypeChoice));
                    ((PropertyIdDefinition)result).DefaultValue = CopyDefaultValue<string>(json);
                    break;
                case PropertyType.Boolean:
                    result = new PropertyBooleanDefinition();
                    ((PropertyBooleanDefinition)result).Choices = ConvertChoicesBoolean(GetJsonArray(json, BrowserConstants.JsonPropertyTypeChoice));
                    ((PropertyBooleanDefinition)result).DefaultValue = CopyDefaultValue<bool?>(json);
                    break;
                case PropertyType.Integer:
                    result = new PropertyIntegerDefinition();
                    ((PropertyIntegerDefinition)result).MinValue = GetInteger(json, BrowserConstants.JsonPropertyTypeMinValue);
                    ((PropertyIntegerDefinition)result).MaxValue = GetInteger(json, BrowserConstants.JsonPropertyTypeMaxValue);
                    ((PropertyIntegerDefinition)result).Choices = ConvertChoicesInteger(GetJsonArray(json, BrowserConstants.JsonPropertyTypeChoice));
                    ((PropertyIntegerDefinition)result).DefaultValue = CopyDefaultValue<BigInteger?>(json);
                    break;
                case PropertyType.DateTime:
                    result = new PropertyDateTimeDefinition();
                    ((PropertyDateTimeDefinition)result).DateTimeResolution = GetEnum<DateTimeResolution?>(json, BrowserConstants.JsonPropertyTypeResolution);
                    ((PropertyDateTimeDefinition)result).Choices = ConvertChoicesDateTime(GetJsonArray(json, BrowserConstants.JsonPropertyTypeChoice));
                    ((PropertyDateTimeDefinition)result).DefaultValue = CopyDefaultValue<DateTime?>(json);
                    break;
                case PropertyType.Decimal:
                    result = new PropertyDecimalDefinition();
                    ((PropertyDecimalDefinition)result).MinValue = GetDecimal(json, BrowserConstants.JsonPropertyTypeMinValue);
                    ((PropertyDecimalDefinition)result).MaxValue = GetDecimal(json, BrowserConstants.JsonPropertyTypeMaxValue);
                    ((PropertyDecimalDefinition)result).Precision = GetEnum<DecimalPrecision?>(json, BrowserConstants.JsonPropertyTypePrecision);
                    ((PropertyDecimalDefinition)result).Choices = ConvertChoicesDecimal(GetJsonArray(json, BrowserConstants.JsonPropertyTypeChoice));
                    ((PropertyDecimalDefinition)result).DefaultValue = CopyDefaultValue<decimal?>(json);
                    break;
                case PropertyType.Html:
                    result = new PropertyHtmlDefinition();
                    ((PropertyHtmlDefinition)result).Choices = ConvertChoicesString(GetJsonArray(json, BrowserConstants.JsonPropertyTypeChoice));
                    ((PropertyHtmlDefinition)result).DefaultValue = CopyDefaultValue<string>(json);
                    break;
                case PropertyType.Uri:
                    result = new PropertyUriDefinition();
                    ((PropertyUriDefinition)result).Choices = ConvertChoicesString(GetJsonArray(json, BrowserConstants.JsonPropertyTypeChoice));
                    ((PropertyUriDefinition)result).DefaultValue = CopyDefaultValue<string>(json);
                    break;
                default:
                    throw new CmisRuntimeException("Property type '" + id + "' does not match a data type!");
            }

            // generic
            result.Id = id;
            result.PropertyType = (PropertyType)propertyType;
            result.Cardinality = (Cardinality)cardinality;
            result.LocalName = GetString(json, BrowserConstants.JsonPropertyTypeLocalName);
            result.LocalNamespace = GetString(json, BrowserConstants.JsonPropertyTypeLocalNameSpace);
            result.QueryName = GetString(json, BrowserConstants.JsonPropertyTypeQueryName);
            result.Description = GetString(json, BrowserConstants.JsonPropertyTypeDescription);
            result.DisplayName = GetString(json, BrowserConstants.JsonPropertyTypeDisplayname);
            result.IsInherited = GetBoolean(json, BrowserConstants.JsonPropertyTypeInhertited);
            result.IsOpenChoice = GetBoolean(json, BrowserConstants.JsonPropertyTypeOpenChoice);
            result.IsOrderable = GetBoolean(json, BrowserConstants.JsonPropertyTypeOrderable);
            result.IsQueryable = GetBoolean(json, BrowserConstants.JsonPropertyTypeQueryable);
            result.IsRequired = GetBoolean(json, BrowserConstants.JsonPropertyTypeRequired);
            result.Updatability = GetEnum<Updatability?>(json, BrowserConstants.JsonPropertyTypeUpdatability);

            // handle extensions
            ConvertExtension(json, result, BrowserConstants.PropertyTypeKeys);

            return result;
        }

        private static IList<T> CopyDefaultValue<T>(JsonObject json)
        {
            IList<T> result = null;

            object defaultValue = null;
            if (json.TryGetValue(BrowserConstants.JsonPropertyTypeDefaultValue, out defaultValue))
            {
                if (defaultValue is JsonArray)
                {
                    result = new List<T>();
                    foreach (object value in (JsonArray)defaultValue)
                    {
                        if (value is T)
                        {
                            result.Add((T)value);
                        }
                    }
                }
                else if (defaultValue is T)
                {
                    result = new List<T>() { (T)defaultValue };
                }
            }

            return result;
        }

        /// <summary>
        /// Converts choices.
        /// </summary>
        private static IList<IChoice<string>> ConvertChoicesString(JsonArray choices)
        {
            if (choices == null)
            {
                return null;
            }

            IList<IChoice<string>> result = new List<IChoice<string>>();

            foreach (object obj in choices)
            {
                if (!(obj is JsonObject))
                {
                    continue;
                }

                JsonObject choiceJson = (JsonObject)obj;

                Choice<string> choice = new Choice<string>();
                choice.DisplayName = GetString(choiceJson, BrowserConstants.JsonPropertyTypeChoiceDisplayname);

                object choiceValue = choiceJson[BrowserConstants.JsonPropertyTypeChoiceValue];
                List<string> values = new List<string>();
                if (choiceValue is JsonArray)
                {
                    foreach (object value in (JsonArray)choiceValue)
                    {
                        values.Add((string)GetCmisValue(value, PropertyType.String));
                    }
                }
                else
                {
                    values.Add((string)GetCmisValue(choiceValue, PropertyType.String));
                }
                choice.Value = values;

                choice.Choices = ConvertChoicesString(GetJsonArray(choiceJson, BrowserConstants.JsonPropertyTypeChoiceChoice));

                result.Add(choice);
            }

            return result;
        }

        /// <summary>
        /// Converts choices.
        /// </summary>
        private static IList<IChoice<bool?>> ConvertChoicesBoolean(JsonArray choices)
        {
            if (choices == null)
            {
                return null;
            }

            IList<IChoice<bool?>> result = new List<IChoice<bool?>>();

            foreach (object obj in choices)
            {
                if (!(obj is JsonObject))
                {
                    continue;
                }


                JsonObject choiceJson = (JsonObject)obj;

                if (choiceJson != null)
                {
                    Choice<bool?> choice = new Choice<bool?>();
                    choice.DisplayName = GetString(choiceJson, BrowserConstants.JsonPropertyTypeChoiceDisplayname);

                    object choiceValue = choiceJson[BrowserConstants.JsonPropertyTypeChoiceValue];
                    List<bool?> values = new List<bool?>();
                    if (choiceValue is JsonArray)
                    {
                        foreach (object value in (JsonArray)choiceValue)
                        {
                            values.Add((bool?)GetCmisValue(value, PropertyType.Boolean));
                        }
                    }
                    else
                    {
                        values.Add((bool?)GetCmisValue(choiceValue, PropertyType.Boolean));
                    }
                    choice.Value = values;

                    choice.Choices = ConvertChoicesBoolean(GetJsonArray(choiceJson, BrowserConstants.JsonPropertyTypeChoiceChoice));

                    result.Add(choice);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts choices.
        /// </summary>
        private static IList<IChoice<BigInteger?>> ConvertChoicesInteger(JsonArray choices)
        {
            if (choices == null)
            {
                return null;
            }

            IList<IChoice<BigInteger?>> result = new List<IChoice<BigInteger?>>();

            foreach (object obj in choices)
            {
                if (!(obj is JsonObject))
                {
                    continue;
                }

                JsonObject choiceJson = (JsonObject)obj;

                if (choiceJson != null)
                {
                    Choice<BigInteger?> choice = new Choice<BigInteger?>();
                    choice.DisplayName = GetString(choiceJson, BrowserConstants.JsonPropertyTypeChoiceDisplayname);

                    object choiceValue = choiceJson[BrowserConstants.JsonPropertyTypeChoiceValue];
                    List<BigInteger?> values = new List<BigInteger?>();
                    if (choiceValue is JsonArray)
                    {
                        foreach (object value in (JsonArray)choiceValue)
                        {
                            values.Add((BigInteger?)GetCmisValue(value, PropertyType.Integer));
                        }
                    }
                    else
                    {
                        values.Add((BigInteger?)GetCmisValue(choiceValue, PropertyType.Integer));
                    }
                    choice.Value = values;

                    choice.Choices = ConvertChoicesInteger(GetJsonArray(choiceJson, BrowserConstants.JsonPropertyTypeChoiceChoice));

                    result.Add(choice);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts choices.
        /// </summary>
        private static IList<IChoice<decimal?>> ConvertChoicesDecimal(JsonArray choices)
        {
            if (choices == null)
            {
                return null;
            }

            IList<IChoice<decimal?>> result = new List<IChoice<decimal?>>();

            foreach (object obj in choices)
            {
                if (!(obj is JsonObject))
                {
                    continue;
                }

                JsonObject choiceJson = (JsonObject)obj;

                if (choiceJson != null)
                {
                    Choice<decimal?> choice = new Choice<decimal?>();
                    choice.DisplayName = GetString(choiceJson, BrowserConstants.JsonPropertyTypeChoiceDisplayname);

                    object choiceValue = choiceJson[BrowserConstants.JsonPropertyTypeChoiceValue];
                    List<decimal?> values = new List<decimal?>();
                    if (choiceValue is JsonArray)
                    {
                        foreach (object value in (JsonArray)choiceValue)
                        {
                            values.Add((decimal?)GetCmisValue(value, PropertyType.Decimal));
                        }
                    }
                    else
                    {
                        values.Add((decimal)GetCmisValue(choiceValue, PropertyType.Decimal));
                    }
                    choice.Value = values;

                    choice.Choices = ConvertChoicesDecimal(GetJsonArray(choiceJson, BrowserConstants.JsonPropertyTypeChoiceChoice));

                    result.Add(choice);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts choices.
        /// </summary>
        private static IList<IChoice<DateTime?>> ConvertChoicesDateTime(JsonArray choices)
        {
            if (choices == null)
            {
                return null;
            }

            IList<IChoice<DateTime?>> result = new List<IChoice<DateTime?>>();

            foreach (object obj in choices)
            {
                if (!(obj is JsonObject))
                {
                    continue;
                }

                JsonObject choiceJson = (JsonObject)obj;

                if (choiceJson != null)
                {
                    Choice<DateTime?> choice = new Choice<DateTime?>();
                    choice.DisplayName = GetString(choiceJson, BrowserConstants.JsonPropertyTypeChoiceDisplayname);

                    object choiceValue = choiceJson[BrowserConstants.JsonPropertyTypeChoiceValue];
                    List<DateTime?> values = new List<DateTime?>();
                    if (choiceValue is JsonArray)
                    {
                        foreach (object value in (JsonArray)choiceValue)
                        {
                            values.Add((DateTime?)GetCmisValue(value, PropertyType.DateTime));
                        }
                    }
                    else
                    {
                        values.Add((DateTime)GetCmisValue(choiceValue, PropertyType.DateTime));
                    }
                    choice.Value = values;

                    choice.Choices = ConvertChoicesDateTime(GetJsonArray(choiceJson, BrowserConstants.JsonPropertyTypeChoiceChoice));

                    result.Add(choice);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts an object.
        /// </summary>
        internal static JsonObject Convert(IObjectData objectData, ITypeCache typeCache, PropertyMode propertyMode, bool succinct, DateTimeFormat dateTimeFormat)
        {
            if (objectData == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            // properties
            if (objectData.Properties != null)
            {
                if (succinct)
                {
                    JsonObject properties = Convert(objectData.Properties, objectData.Id, typeCache, propertyMode, true,
                            dateTimeFormat);
                    if (properties != null)
                    {
                        result.Add(BrowserConstants.JsonObjectSuccinctProperties, properties);
                    }
                }
                else
                {
                    JsonObject properties = Convert(objectData.Properties, objectData.Id, typeCache, propertyMode, false,
                            dateTimeFormat);
                    if (properties != null)
                    {
                        result.Add(BrowserConstants.JsonObjectProperties, properties);
                    }
                }

                JsonObject propertiesExtension = new JsonObject();
                ConvertExtension(objectData.Properties, propertiesExtension);
                if (propertiesExtension.Count > 0)
                {
                    result.Add(BrowserConstants.JsonObjectPropertiesExtension, propertiesExtension);
                }
            }

            // allowable actions
            if (objectData.AllowableActions != null)
            {
                result.Add(BrowserConstants.JsonObjectAllowableActions, Convert(objectData.AllowableActions));
            }

            // relationships
            if (IsNotEmpty(objectData.Relationships))
            {
                JsonArray relationships = new JsonArray();

                foreach (ObjectData relationship in objectData.Relationships)
                {
                    relationships.Add(Convert(relationship, typeCache, propertyMode, succinct, dateTimeFormat));
                }

                result.Add(BrowserConstants.JsonObjectRelationships, relationships);
            }

            // change event info
            if (objectData.ChangeEventInfo != null && propertyMode == PropertyMode.Change)
            {
                JsonObject changeEventInfo = new JsonObject();

                IChangeEventInfo cei = objectData.ChangeEventInfo;
                changeEventInfo.Add(BrowserConstants.JsonChangeEventType, GetJsonEnumValue(cei.ChangeType));
                changeEventInfo.Add(BrowserConstants.JsonChangeEventTime, GetJsonValue(cei.ChangeTime, dateTimeFormat));

                ConvertExtension(objectData.ChangeEventInfo, changeEventInfo);

                result.Add(BrowserConstants.JsonObjectChangeEventInfo, changeEventInfo);
            }

            // Acl
            if ((objectData.Acl != null) && (objectData.Acl.Aces != null) && propertyMode != PropertyMode.Query)
            {
                result.Add(BrowserConstants.JsonObjectAcl, Convert(objectData.Acl));
            }
            SetIfNotNull(BrowserConstants.JsonObjectExactAcl, objectData.IsExactAcl, result);

            // policy ids
            if ((objectData.PolicyIds != null) && (objectData.PolicyIds.PolicyIds != null) && propertyMode != PropertyMode.Query)
            {
                JsonObject policyIds = new JsonObject();
                JsonArray ids = new JsonArray();
                policyIds.Add(BrowserConstants.JsonObjectPolicyIdsIds, ids);

                foreach (string pi in objectData.PolicyIds.PolicyIds)
                {
                    ids.Add(pi);
                }

                ConvertExtension(objectData.PolicyIds, policyIds);

                result.Add(BrowserConstants.JsonObjectPolicyIds, policyIds);
            }

            // renditions
            if (IsNotEmpty(objectData.Renditions))
            {
                JsonArray renditions = new JsonArray();

                foreach (RenditionData rendition in objectData.Renditions)
                {
                    renditions.Add(Convert(rendition));
                }

                result.Add(BrowserConstants.JsonObjectRenditions, renditions);
            }

            ConvertExtension(objectData, result);

            return result;
        }

        /// <summary>
        /// Converts a bag of properties.
        /// </summary>
        internal static JsonObject Convert(IProperties properties, string objectId, ITypeCache typeCache, PropertyMode propertyMode, bool succinct, DateTimeFormat dateTimeFormat)
        {
            if (properties == null)
            {
                return null;
            }

            // get the type
            ITypeDefinition type = null;
            if (typeCache != null)
            {
                IPropertyData typeProp = properties[PropertyIds.ObjectTypeId];
                if (typeProp != null && typeProp.PropertyType == PropertyType.Id)
                {
                    object typeId = typeProp.FirstValue;
                    if (typeId != null)
                    {
                        type = typeCache.GetTypeDefinition(typeId.ToString());
                    }
                }
            }

            JsonObject result = new JsonObject();

            foreach (IPropertyData property in properties.PropertyList)
            {
                IPropertyDefinition propDef = null;
                if (typeCache != null)
                {
                    propDef = typeCache.GetPropertyDefinition(property.Id);
                }
                if (propDef == null && type != null)
                {
                    propDef = type[property.Id];
                }
                if (propDef == null && typeCache != null && objectId != null && propertyMode != PropertyMode.Change)
                {
                    typeCache.GetTypeDefinitionForObject(objectId);
                    propDef = typeCache.GetPropertyDefinition(property.Id);
                }

                string propId = (propertyMode == PropertyMode.Query ? property.QueryName : property.Id);
                if (propId == null)
                {
                    throw new CmisRuntimeException("No query name or alias for property '" + property.Id + "'!");
                }
                result.Add(propId, Convert(property, propDef, succinct, dateTimeFormat));
            }

            return result;
        }

        /// <summary>
        /// Converts a property.
        /// </summary>
        internal static object Convert(IPropertyData property, IPropertyDefinition propDef, bool succinct, DateTimeFormat dateTimeFormat)
        {
            if (property == null)
            {
                return null;
            }

            if (succinct)
            {
                object result = null;

                if (propDef != null)
                {
                    if (IsNullOrEmpty(property.Values))
                    {
                        result = null;
                    }
                    else if (propDef.Cardinality == Cardinality.Single)
                    {
                        result = GetJsonValue(property.Values[0], dateTimeFormat);
                    }
                    else
                    {
                        JsonArray values = new JsonArray();

                        foreach (object value in property.Values)
                        {
                            values.Add(GetJsonValue(value, dateTimeFormat));
                        }

                        result = values;
                    }
                }
                else
                {
                    if (IsNullOrEmpty(property.Values))
                    {
                        result = null;
                    }
                    else
                    {
                        JsonArray values = new JsonArray();

                        foreach (object value in property.Values)
                        {
                            values.Add(GetJsonValue(value, dateTimeFormat));
                        }

                        result = values;
                    }
                }

                return result;
            }
            else
            {
                JsonObject result = new JsonObject();

                result.Add(BrowserConstants.JsonPropertyId, property.Id);
                SetIfNotNull(BrowserConstants.JsonPropertyLocalName, property.LocalName, result);
                SetIfNotNull(BrowserConstants.JsonPropertyDisplayname, property.DisplayName, result);
                SetIfNotNull(BrowserConstants.JsonPropertyQueryName, property.QueryName, result);

                if (propDef != null)
                {
                    result.Add(BrowserConstants.JsonPropertyDatatype, propDef.PropertyType.GetCmisValue());
                    result.Add(BrowserConstants.JsonPropertyCardinality, GetJsonEnumValue(propDef.Cardinality));

                    if (IsNullOrEmpty(property.Values))
                    {
                        result.Add(BrowserConstants.JsonPropertyValue, null);
                    }
                    else if (propDef.Cardinality == Cardinality.Single)
                    {
                        result.Add(BrowserConstants.JsonPropertyValue, GetJsonValue(property.Values[0], dateTimeFormat));
                    }
                    else
                    {
                        JsonArray values = new JsonArray();

                        foreach (object value in property.Values)
                        {
                            values.Add(GetJsonValue(value, dateTimeFormat));
                        }

                        result.Add(BrowserConstants.JsonPropertyValue, values);
                    }
                }
                else
                {
                    result.Add(BrowserConstants.JsonPropertyDatatype, property.PropertyType.GetCmisValue());

                    if (IsNullOrEmpty(property.Values))
                    {
                        result.Add(BrowserConstants.JsonPropertyValue, null);
                    }
                    else
                    {
                        JsonArray values = new JsonArray();

                        foreach (object value in property.Values)
                        {
                            values.Add(GetJsonValue(value, dateTimeFormat));
                        }

                        result.Add(BrowserConstants.JsonPropertyValue, values);
                    }
                }

                ConvertExtension(property, result);

                return result;
            }
        }

        /// <summary>
        /// Converts allowable actions.
        /// </summary>
        internal static JsonObject Convert(IAllowableActions allowableActions)
        {
            if (allowableActions == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            ISet<PortCMIS.Enums.Action> actionSet = allowableActions.Actions;

            var values = Enum.GetValues(typeof(PortCMIS.Enums.Action));
            foreach (var value in values)
            {
                PortCMIS.Enums.Action action = (PortCMIS.Enums.Action)Enum.ToObject(typeof(PortCMIS.Enums.Action), value);
                result.Add(action.GetCmisValue(), actionSet.Contains(action));
            }

            ConvertExtension(allowableActions, result);

            return result;
        }

        /// <summary>
        /// Converts an Acl.
        /// </summary>
        internal static JsonObject Convert(IAcl acl)
        {
            if (acl == null || acl.Aces == null)
            {
                return null;
            }

            JsonArray aceObjects = new JsonArray();

            foreach (IAce ace in acl.Aces)
            {
                JsonArray permissions = new JsonArray();
                if (ace.Permissions != null)
                {
                    foreach (string p in ace.Permissions)
                    {
                        permissions.Add(p);
                    }
                }

                JsonObject aceObject = new JsonObject();

                JsonObject principalObject = new JsonObject();
                principalObject.Add(BrowserConstants.JsonAcePrincipalId, ace.PrincipalId);
                ConvertExtension(ace.Principal, principalObject);
                aceObject.Add(BrowserConstants.JsonAcePrincipal, principalObject);

                aceObject.Add(BrowserConstants.JsonAcePermissions, permissions);
                aceObject.Add(BrowserConstants.JsonAceIsDirect, ace.IsDirect);

                ConvertExtension(ace, aceObject);

                aceObjects.Add(aceObject);
            }

            JsonObject result = new JsonObject();
            result.Add(BrowserConstants.JsonAclAces, aceObjects);
            SetIfNotNull(BrowserConstants.JsonAclIsExact, acl.IsExact, result);

            ConvertExtension(acl, result);

            return result;
        }

        /// <summary>
        /// Converts a rendition.
        /// </summary>
        internal static JsonObject Convert(IRenditionData rendition)
        {
            if (rendition == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            result.Add(BrowserConstants.JsonRenditionStreamId, rendition.StreamId);
            result.Add(BrowserConstants.JsonRenditionMimeType, rendition.MimeType);
            result.Add(BrowserConstants.JsonRenditionLength, rendition.Length);
            result.Add(BrowserConstants.JsonRenditionKind, rendition.Kind);
            SetIfNotNull(BrowserConstants.JsonRenditionTitle, rendition.Title, result);
            SetIfNotNull(BrowserConstants.JsonRenditionHeight, rendition.Height, result);
            SetIfNotNull(BrowserConstants.JsonRenditionWidth, rendition.Width, result);
            SetIfNotNull(BrowserConstants.JsonRenditionDocumentId, rendition.RenditionDocumentId, result);

            ConvertExtension(rendition, result);

            return result;
        }

        /// <summary>
        /// Converts a query object list.
        /// </summary>
        internal static JsonObject Convert(IObjectList list, ITypeCache typeCache, PropertyMode propertyMode,
                 bool succinct, DateTimeFormat dateTimeFormat)
        {
            if (list == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            JsonArray objects = new JsonArray();
            if (list.Objects != null)
            {
                foreach (IObjectData objectData in list.Objects)
                {
                    objects.Add(Convert(objectData, typeCache, propertyMode, succinct, dateTimeFormat));
                }
            }

            if (propertyMode == PropertyMode.Query)
            {
                result.Add(BrowserConstants.JsonQueryResultListResults, objects);

                SetIfNotNull(BrowserConstants.JsonQueryResultListHasMoreItems, list.HasMoreItems, result);
                SetIfNotNull(BrowserConstants.JsonQueryResultListNumItems, list.NumItems, result);
            }
            else
            {
                result.Add(BrowserConstants.JsonObjectListObjects, objects);

                SetIfNotNull(BrowserConstants.JsonObjectListHasMoreItems, list.HasMoreItems, result);
                SetIfNotNull(BrowserConstants.JsonObjectListNumItems, list.NumItems, result);
            }

            ConvertExtension(list, result);

            return result;
        }

        /// <summary>
        /// Converts an object in a folder list.
        /// </summary>
        internal static JsonObject Convert(IObjectInFolderData objectInFolder, ITypeCache typeCache,
                 bool succinct, DateTimeFormat dateTimeFormat)
        {
            if ((objectInFolder == null) || (objectInFolder.Object == null))
            {
                return null;
            }

            JsonObject result = new JsonObject();
            result.Add(BrowserConstants.JsonObjectInFolderObject, Convert(objectInFolder.Object, typeCache, PropertyMode.Object, succinct, dateTimeFormat));
            SetIfNotNull(BrowserConstants.JsonObjectInFolderPathSegment, objectInFolder.PathSegment, result);

            ConvertExtension(objectInFolder, result);

            return result;
        }

        /// <summary>
        /// Converts a folder list.
        /// </summary>
        internal static JsonObject Convert(IObjectInFolderList objectInFolderList, ITypeCache typeCache,
                 bool succinct, DateTimeFormat dateTimeFormat)
        {
            if (objectInFolderList == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            if (objectInFolderList.Objects != null)
            {
                JsonArray objects = new JsonArray();

                foreach (ObjectInFolderData objectData in objectInFolderList.Objects)
                {
                    objects.Add(Convert(objectData, typeCache, succinct, dateTimeFormat));
                }

                result.Add(BrowserConstants.JsonObjectInFolderListObjects, objects);
            }

            SetIfNotNull(BrowserConstants.JsonObjectInFolderListHasMoreItems, objectInFolderList.HasMoreItems, result);
            SetIfNotNull(BrowserConstants.JsonObjectInFolderListNumItems, objectInFolderList.NumItems, result);

            ConvertExtension(objectInFolderList, result);

            return result;
        }

        /// <summary>
        /// Converts a folder container.
        /// </summary>
        internal static JsonObject Convert(IObjectInFolderContainer container, ITypeCache typeCache, bool succinct, DateTimeFormat dateTimeFormat)
        {
            if (container == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();
            result.Add(BrowserConstants.JsonObjectInFolderContainerObject, Convert(container.Object, typeCache, succinct, dateTimeFormat));

            if (IsNotEmpty(container.Children))
            {
                JsonArray children = new JsonArray();
                foreach (ObjectInFolderContainer descendant in container.Children)
                {
                    children.Add(Convert(descendant, typeCache, succinct, dateTimeFormat));
                }

                result.Add(BrowserConstants.JsonObjectInFolderContainerChildren, children);
            }

            ConvertExtension(container, result);

            return result;
        }

        /// <summary>
        /// Converts an object parent.
        /// </summary>
        internal static JsonObject Convert(IObjectParentData parent, ITypeCache typeCache, bool succinct, DateTimeFormat dateTimeFormat)
        {
            if ((parent == null) || (parent.Object == null))
            {
                return null;
            }

            JsonObject result = new JsonObject();
            result.Add(BrowserConstants.JsonObjectParentsObject, Convert(parent.Object, typeCache, PropertyMode.Object, succinct, dateTimeFormat));
            if (parent.RelativePathSegment != null)
            {
                result.Add(BrowserConstants.JsonObjectParentsRelativePathSegment, parent.RelativePathSegment);
            }

            ConvertExtension(parent, result);

            return result;
        }

        /// <summary>
        /// Converts a type definition.
        /// </summary>
        internal static JsonObject Convert(ITypeDefinition type, DateTimeFormat dateTimeFormat)
        {
            if (type == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();
            result.Add(BrowserConstants.JsonTypeId, type.Id);
            result.Add(BrowserConstants.JsonTypeLocalName, type.LocalName);
            result.Add(BrowserConstants.JsonTypeLocalNameSpace, type.LocalNamespace);
            SetIfNotNull(BrowserConstants.JsonTypeDisplayname, type.DisplayName, result);
            SetIfNotNull(BrowserConstants.JsonTypeQueryName, type.QueryName, result);
            SetIfNotNull(BrowserConstants.JsonTypeDescription, type.Description, result);
            result.Add(BrowserConstants.JsonTypeBaseId, type.BaseTypeId.GetCmisValue());
            SetIfNotNull(BrowserConstants.JsonTypeParentId, type.ParentTypeId, result);
            result.Add(BrowserConstants.JsonTypeCreatable, type.IsCreatable);
            result.Add(BrowserConstants.JsonTypeFileable, type.IsFileable);
            result.Add(BrowserConstants.JsonTypeQueryable, type.IsQueryable);
            result.Add(BrowserConstants.JsonTypeFulltextIndexed, type.IsFulltextIndexed);
            result.Add(BrowserConstants.JsonTypeIncludeInSuperTypeQuery, type.IsIncludedInSupertypeQuery);
            result.Add(BrowserConstants.JsonTypeControlablePolicy, type.IsControllablePolicy);
            result.Add(BrowserConstants.JsonTypeControlableAcl, type.IsControllableAcl);

            if (type.TypeMutability != null)
            {
                ITypeMutability typeMutability = type.TypeMutability;
                JsonObject typeMutabilityJson = new JsonObject();

                typeMutabilityJson.Add(BrowserConstants.JsonTypeTypeMutablilityCreate, typeMutability.CanCreate);
                typeMutabilityJson.Add(BrowserConstants.JsonTypeTypeMutablilityUpdate, typeMutability.CanUpdate);
                typeMutabilityJson.Add(BrowserConstants.JsonTypeTypeMutablilityDelete, typeMutability.CanDelete);

                ConvertExtension(typeMutability, typeMutabilityJson);

                result.Add(BrowserConstants.JsonTypeTypeMutability, typeMutabilityJson);
            }

            if (type is DocumentTypeDefinition)
            {
                result.Add(BrowserConstants.JsonTypeVersionable, ((DocumentTypeDefinition)type).IsVersionable);
                result.Add(BrowserConstants.JsonTypeContentstreamAllowed, GetJsonEnumValue(((DocumentTypeDefinition)type).ContentStreamAllowed));
            }

            if (type is RelationshipTypeDefinition)
            {
                result.Add(BrowserConstants.JsonTypeAllowedSourceTypes, GetJsonArrayFromList(((RelationshipTypeDefinition)type).AllowedSourceTypeIds));
                result.Add(BrowserConstants.JsonTypeAllowedTargetTypes, GetJsonArrayFromList(((RelationshipTypeDefinition)type).AllowedTargetTypeIds));
            }

            if (IsNotEmpty(type.PropertyDefinitions))
            {
                JsonObject propertyDefs = new JsonObject();

                foreach (IPropertyDefinition pd in type.PropertyDefinitions)
                {
                    propertyDefs.Add(pd.Id, Convert(pd, dateTimeFormat));
                }

                result.Add(BrowserConstants.JsonTypePropertyDefinitions, propertyDefs);
            }

            ConvertExtension(type, result);

            return result;
        }

        /// <summary>
        /// Converts a property type definition.
        /// </summary>
        internal static JsonObject Convert(IPropertyDefinition propertyDefinition, DateTimeFormat dateTimeFormat)
        {
            if (propertyDefinition == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            // type specific
            if (propertyDefinition is PropertyStringDefinition)
            {
                SetIfNotNull(BrowserConstants.JsonPropertyTypeMaxLength, ((PropertyStringDefinition)propertyDefinition).MaxLength, result);
                AddCmisDefaultValue(((PropertyStringDefinition)propertyDefinition).DefaultValue, propertyDefinition.Cardinality, result, dateTimeFormat);
                AddChoices(((PropertyStringDefinition)propertyDefinition).Choices, propertyDefinition.Cardinality, result, dateTimeFormat);
            }
            else if (propertyDefinition is PropertyIdDefinition)
            {
                AddCmisDefaultValue(((PropertyIdDefinition)propertyDefinition).DefaultValue, propertyDefinition.Cardinality, result, dateTimeFormat);
                AddChoices(((PropertyIdDefinition)propertyDefinition).Choices, propertyDefinition.Cardinality, result, dateTimeFormat);
            }
            else if (propertyDefinition is PropertyIntegerDefinition)
            {
                SetIfNotNull(BrowserConstants.JsonPropertyTypeMinValue, ((PropertyIntegerDefinition)propertyDefinition).MinValue, result);
                SetIfNotNull(BrowserConstants.JsonPropertyTypeMaxValue, ((PropertyIntegerDefinition)propertyDefinition).MaxValue, result);
                AddCmisDefaultValue(((PropertyIntegerDefinition)propertyDefinition).DefaultValue, propertyDefinition.Cardinality, result, dateTimeFormat);
                AddChoices(((PropertyIntegerDefinition)propertyDefinition).Choices, propertyDefinition.Cardinality, result, dateTimeFormat);
            }
            else if (propertyDefinition is PropertyDecimalDefinition)
            {
                SetIfNotNull(BrowserConstants.JsonPropertyTypeMinValue, ((PropertyDecimalDefinition)propertyDefinition).MinValue, result);
                SetIfNotNull(BrowserConstants.JsonPropertyTypeMaxValue, ((PropertyDecimalDefinition)propertyDefinition).MaxValue, result);
                DecimalPrecision? precision = ((PropertyDecimalDefinition)propertyDefinition).Precision;
                if (precision != null)
                {
                    result.Add(BrowserConstants.JsonPropertyTypePrecision, precision.GetCmisValue());
                }
                AddCmisDefaultValue(((PropertyDecimalDefinition)propertyDefinition).DefaultValue, propertyDefinition.Cardinality, result, dateTimeFormat);
                AddChoices(((PropertyDecimalDefinition)propertyDefinition).Choices, propertyDefinition.Cardinality, result, dateTimeFormat);
            }
            else if (propertyDefinition is PropertyBooleanDefinition)
            {
                AddCmisDefaultValue(((PropertyBooleanDefinition)propertyDefinition).DefaultValue, propertyDefinition.Cardinality, result, dateTimeFormat);
                AddChoices(((PropertyBooleanDefinition)propertyDefinition).Choices, propertyDefinition.Cardinality, result, dateTimeFormat);
            }
            else if (propertyDefinition is PropertyDateTimeDefinition)
            {
                DateTimeResolution? resolution = ((PropertyDateTimeDefinition)propertyDefinition).DateTimeResolution;
                if (resolution != null)
                {
                    result.Add(BrowserConstants.JsonPropertyTypeResolution, resolution.GetCmisValue());
                }
                AddCmisDefaultValue(((PropertyDateTimeDefinition)propertyDefinition).DefaultValue, propertyDefinition.Cardinality, result, dateTimeFormat);
                AddChoices(((PropertyDateTimeDefinition)propertyDefinition).Choices, propertyDefinition.Cardinality, result, dateTimeFormat);
            }
            else if (propertyDefinition is PropertyHtmlDefinition)
            {
                AddCmisDefaultValue(((PropertyHtmlDefinition)propertyDefinition).DefaultValue, propertyDefinition.Cardinality, result, dateTimeFormat);
                AddChoices(((PropertyHtmlDefinition)propertyDefinition).Choices, propertyDefinition.Cardinality, result, dateTimeFormat);
            }
            else if (propertyDefinition is PropertyUriDefinition)
            {
                AddCmisDefaultValue(((PropertyUriDefinition)propertyDefinition).DefaultValue, propertyDefinition.Cardinality, result, dateTimeFormat);
                AddChoices(((PropertyUriDefinition)propertyDefinition).Choices, propertyDefinition.Cardinality, result, dateTimeFormat);
            }
            else
            {
                throw new CmisRuntimeException("Invalid property definition!");
            }

            // generic
            result.Add(BrowserConstants.JsonPropertyTypeId, propertyDefinition.Id);
            result.Add(BrowserConstants.JsonPropertyTypeLocalName, propertyDefinition.LocalName);
            SetIfNotNull(BrowserConstants.JsonPropertyTypeLocalNameSpace, propertyDefinition.LocalNamespace, result);
            SetIfNotNull(BrowserConstants.JsonPropertyTypeDisplayname, propertyDefinition.DisplayName, result);
            SetIfNotNull(BrowserConstants.JsonPropertyTypeQueryName, propertyDefinition.QueryName, result);
            SetIfNotNull(BrowserConstants.JsonPropertyTypeDescription, propertyDefinition.Description, result);
            result.Add(BrowserConstants.JsonPropertyTypePropertyType, propertyDefinition.PropertyType.GetCmisValue());
            result.Add(BrowserConstants.JsonPropertyTypeCardinality, GetJsonEnumValue(propertyDefinition.Cardinality));
            result.Add(BrowserConstants.JsonPropertyTypeUpdatability, GetJsonEnumValue(propertyDefinition.Updatability));
            SetIfNotNull(BrowserConstants.JsonPropertyTypeInhertited, propertyDefinition.IsInherited, result);
            result.Add(BrowserConstants.JsonPropertyTypeRequired, propertyDefinition.IsRequired);
            result.Add(BrowserConstants.JsonPropertyTypeQueryable, propertyDefinition.IsQueryable);
            result.Add(BrowserConstants.JsonPropertyTypeOrderable, propertyDefinition.IsOrderable);
            SetIfNotNull(BrowserConstants.JsonPropertyTypeOpenChoice, propertyDefinition.IsOpenChoice, result);

            ConvertExtension(propertyDefinition, result);

            return result;
        }

        private static void AddCmisDefaultValue<T>(IList<T> defaultValue, Cardinality? cardinality, JsonObject target, DateTimeFormat dateTimeFormat)
        {
            if (defaultValue == null)
            {
                return;
            }

            if (cardinality == Cardinality.Single)
            {
                if (defaultValue.Count > 0)
                {
                    target.Add(BrowserConstants.JsonPropertyTypeDefaultValue, GetJsonValue(defaultValue[0], dateTimeFormat));
                }
            }
            else
            {
                JsonArray values = new JsonArray();
                foreach (object value in defaultValue)
                {
                    values.Add(GetJsonValue(value, dateTimeFormat));
                }
                target.Add(BrowserConstants.JsonPropertyTypeDefaultValue, values);
            }
        }

        private static void AddChoices<T>(IList<IChoice<T>> choices, Cardinality? cardinality, JsonObject target, DateTimeFormat dateTimeFormat)
        {
            if (choices == null)
            {
                return;
            }

            target.Add(BrowserConstants.JsonPropertyTypeChoice, ConvertChoices(choices, cardinality, dateTimeFormat));
        }

        /// <summary>
        /// Converts choices.
        /// </summary>
        private static JsonArray ConvertChoices<T>(IList<IChoice<T>> choices, Cardinality? cardinality, DateTimeFormat dateTimeFormat)
        {
            if (choices == null)
            {
                return null;
            }

            JsonArray result = new JsonArray();

            foreach (IChoice<object> choice in choices)
            {
                JsonObject jsonChoice = new JsonObject();

                jsonChoice.Add(BrowserConstants.JsonPropertyTypeChoiceDisplayname, choice.DisplayName);

                if (cardinality == Cardinality.Single)
                {
                    if (choice.Value.Count > 0)
                    {
                        jsonChoice.Add(BrowserConstants.JsonPropertyTypeChoiceValue, GetJsonValue(choice.Value[0], dateTimeFormat));
                    }
                }
                else
                {
                    JsonArray values = new JsonArray();
                    foreach (object value in choice.Value)
                    {
                        values.Add(GetJsonValue(value, dateTimeFormat));
                    }
                    jsonChoice.Add(BrowserConstants.JsonPropertyTypeChoiceValue, values);
                }

                if (IsNotEmpty(choice.Choices))
                {
                    jsonChoice.Add(BrowserConstants.JsonPropertyTypeChoiceChoice, ConvertChoices(choice.Choices, cardinality, dateTimeFormat));
                }

                result.Add(jsonChoice);
            }

            return result;
        }

        /// <summary>
        /// Converts a type definition list.
        /// </summary>
        internal static JsonObject Convert(ITypeDefinitionList list, DateTimeFormat dateTimeFormat)
        {
            if (list == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            if (list.List != null)
            {
                JsonArray objects = new JsonArray();

                foreach (ITypeDefinition type in list.List)
                {
                    objects.Add(Convert(type, dateTimeFormat));
                }

                result.Add(BrowserConstants.JsonTypeListTypes, objects);
            }

            SetIfNotNull(BrowserConstants.JsonTypeListHasMoreItems, list.HasMoreItems, result);
            SetIfNotNull(BrowserConstants.JsonTypeListNumItems, list.NumItems, result);

            ConvertExtension(list, result);

            return result;
        }

        /// <summary>
        /// Converts a type definition list.
        /// </summary>
        internal static ITypeDefinitionList ConvertTypeChildren(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            TypeDefinitionList result = new TypeDefinitionList();

            JsonArray typesList = GetJsonArray(json, BrowserConstants.JsonTypeListTypes);
            IList<ITypeDefinition> types = new List<ITypeDefinition>();

            if (typesList != null)
            {
                foreach (object type in typesList)
                {
                    if (type is JsonObject)
                    {
                        types.Add(ConvertTypeDefinition((JsonObject)type));
                    }
                }
            }

            result.List = types;
            result.HasMoreItems = GetBoolean(json, BrowserConstants.JsonTypeListHasMoreItems);
            result.NumItems = GetInteger(json, BrowserConstants.JsonTypeListNumItems);

            ConvertExtension(json, result, BrowserConstants.TypeListKeys);

            return result;
        }

        /// <summary>
        /// Converts a type definition container.
        /// </summary>
        internal static JsonObject Convert(ITypeDefinitionContainer container, DateTimeFormat dateTimeFormat)
        {
            if (container == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();
            result.Add(BrowserConstants.JsonTypesContainerType, Convert(container.TypeDefinition, dateTimeFormat));

            if (IsNotEmpty(container.Children))
            {
                JsonArray children = new JsonArray();
                foreach (ITypeDefinitionContainer child in container.Children)
                {
                    children.Add(Convert(child, dateTimeFormat));
                }

                result.Add(BrowserConstants.JsonTypesContainerChildren, children);
            }

            ConvertExtension(container, result);

            return result;
        }

        /// <summary>
        /// Converts a type definition list.
        /// </summary>
        internal static IList<ITypeDefinitionContainer> ConvertTypeDescendants(JsonArray json)
        {
            if (json == null)
            {
                return null;
            }

            if (json.Count == 0)
            {
                return new List<ITypeDefinitionContainer>();
            }

            IList<ITypeDefinitionContainer> result = new List<ITypeDefinitionContainer>();

            foreach (object obj in json)
            {
                if (obj is JsonObject)
                {
                    JsonObject jsonContainer = (JsonObject)obj;
                    TypeDefinitionContainer container = new TypeDefinitionContainer();

                    container.TypeDefinition = ConvertTypeDefinition(GetJsonObject(jsonContainer, BrowserConstants.JsonTypesContainerType));

                    JsonArray children = GetJsonArray(jsonContainer, BrowserConstants.JsonTypesContainerChildren);
                    if (children != null)
                    {
                        container.Children = ConvertTypeDescendants(children);
                    }
                    else
                    {
                        container.Children = new List<ITypeDefinitionContainer>();
                    }

                    ConvertExtension(jsonContainer, container, BrowserConstants.TypesContainerKeys);

                    result.Add(container);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts an object.
        /// </summary>
        internal static IObjectData ConvertObject(JsonObject json, ITypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ObjectData result = new ObjectData();

            result.Acl = ConvertAcl(GetJsonObject(json, BrowserConstants.JsonObjectAcl));
            result.AllowableActions = ConvertAllowableActions(GetJsonObject(json, BrowserConstants.JsonObjectAllowableActions));
            JsonObject jsonChangeEventInfo = GetJsonObject(json, BrowserConstants.JsonObjectChangeEventInfo);
            if (jsonChangeEventInfo != null)
            {
                ChangeEventInfo changeEventInfo = new ChangeEventInfo();

                changeEventInfo.ChangeTime = GetDateTime(jsonChangeEventInfo, BrowserConstants.JsonChangeEventTime);
                changeEventInfo.ChangeType = GetEnum<ChangeType?>(jsonChangeEventInfo, BrowserConstants.JsonChangeEventType);

                ConvertExtension(json, result, BrowserConstants.ChangeEventKeys);

                result.ChangeEventInfo = changeEventInfo;
            }
            result.IsExactAcl = GetBoolean(json, BrowserConstants.JsonObjectExactAcl);
            result.PolicyIds = ConvertPolicyIds(GetJsonObject(json, BrowserConstants.JsonObjectPolicyIds));

            JsonObject propMap = GetJsonObject(json, BrowserConstants.JsonObjectSuccinctProperties);
            if (propMap != null)
            {
                result.Properties = ConvertSuccinctProperties(propMap, GetJsonObject(json, BrowserConstants.JsonObjectPropertiesExtension), typeCache);
            }
            propMap = GetJsonObject(json, BrowserConstants.JsonObjectProperties);
            if (propMap != null)
            {
                result.Properties = ConvertProperties(propMap, GetJsonObject(json, BrowserConstants.JsonObjectPropertiesExtension));
            }

            JsonArray jsonRelationships = GetJsonArray(json, BrowserConstants.JsonObjectRelationships);
            if (jsonRelationships != null)
            {
                result.Relationships = ConvertObjects(jsonRelationships, typeCache);
            }
            JsonArray jsonRenditions = GetJsonArray(json, BrowserConstants.JsonObjectRenditions);
            if (jsonRenditions != null)
            {
                result.Renditions = ConvertRenditions(jsonRenditions);
            }

            ConvertExtension(json, result, BrowserConstants.ObjectKeys);

            return result;
        }

        /// <summary>
        /// Converts an object.
        /// </summary>
        internal static IList<IObjectData> ConvertObjects(List<object> json, ITypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            IList<IObjectData> result = new List<IObjectData>();
            foreach (object obj in json)
            {
                IObjectData relationship = ConvertObject(GetJsonObject(obj), typeCache);
                if (relationship != null)
                {
                    result.Add(relationship);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts an Acl.
        /// </summary>
        internal static IAcl ConvertAcl(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            Acl result = new Acl();

            List<IAce> aces = new List<IAce>();

            JsonArray jsonAces = GetJsonArray(json, BrowserConstants.JsonAclAces);
            if (jsonAces != null)
            {
                foreach (Object obj in jsonAces)
                {
                    JsonObject entry = GetJsonObject(obj);
                    if (entry != null)
                    {
                        Ace ace = new Ace();

                        bool? isDirect = GetBoolean(entry, BrowserConstants.JsonAceIsDirect);
                        ace.IsDirect = (isDirect != null ? (bool)isDirect : true);

                        JsonArray jsonPermissions = GetJsonArray(entry, BrowserConstants.JsonAcePermissions);
                        if (jsonPermissions != null)
                        {
                            List<string> permissions = new List<string>();
                            foreach (object perm in jsonPermissions)
                            {
                                if (perm != null)
                                {
                                    permissions.Add(perm.ToString());
                                }
                            }
                            ace.Permissions = permissions;
                        }

                        JsonObject jsonPrincipal = GetJsonObject(entry, BrowserConstants.JsonAcePrincipal);
                        if (jsonPrincipal != null)
                        {
                            Principal principal = new Principal();

                            principal.Id = GetString(jsonPrincipal, BrowserConstants.JsonAcePrincipalId);

                            ConvertExtension(jsonPrincipal, principal, BrowserConstants.PrincipalKeys);

                            ace.Principal = principal;
                        }

                        ConvertExtension(entry, ace, BrowserConstants.AceKeys);

                        aces.Add(ace);
                    }
                }
            }

            result.Aces = aces;

            result.IsExact = GetBoolean(json, BrowserConstants.JsonAclIsExact);

            ConvertExtension(json, result, BrowserConstants.AclKeys);

            return result;
        }

        /// <summary>
        /// Converts allowable actions.
        /// </summary>
        internal static AllowableActions ConvertAllowableActions(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            AllowableActions result = new AllowableActions();
            ISet<PortCMIS.Enums.Action> allowableActions = new HashSet<PortCMIS.Enums.Action>();

            var values = Enum.GetValues(typeof(PortCMIS.Enums.Action));
            foreach (var value in values)
            {
                PortCMIS.Enums.Action action = (PortCMIS.Enums.Action)Enum.ToObject(typeof(PortCMIS.Enums.Action), value);
                bool? cmisValue = GetBoolean(json, action.GetCmisValue());
                if (cmisValue != null && cmisValue == true)
                {
                    allowableActions.Add(action);
                }
            }

            result.Actions = allowableActions;

            ConvertExtension(json, result, BrowserConstants.AllowableActionsKeys);

            return result;
        }

        /// <summary>
        /// Converts a list of policy ids.
        /// </summary>
        internal static PolicyIdList ConvertPolicyIds(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            PolicyIdList result = new PolicyIdList();
            List<string> policyIds = new List<string>();

            JsonArray ids = GetJsonArray(json, BrowserConstants.JsonObjectPolicyIdsIds);
            if (ids != null)
            {
                foreach (object obj in ids)
                {
                    if (obj is string)
                    {
                        policyIds.Add((string)obj);
                    }
                }
            }

            ConvertExtension(json, result, BrowserConstants.PolicyIdsKeys);

            result.PolicyIds = policyIds;

            return result;
        }

        /// <summary>
        /// Converts properties.
        /// </summary>
        internal static IProperties ConvertProperties(JsonObject json, JsonObject extJson)
        {
            if (json == null)
            {
                return null;
            }

            Properties result = new Properties();

            foreach (KeyValuePair<string, object> jsonProperty in json)
            {
                JsonObject jsonPropertyMap = GetJsonObject(jsonProperty.Value);
                if (jsonPropertyMap != null)
                {
                    string id = GetString(jsonPropertyMap, BrowserConstants.JsonPropertyId);
                    string queryName = GetString(jsonPropertyMap, BrowserConstants.JsonPropertyQueryName);
                    if (id == null && queryName == null)
                    {
                        throw new CmisRuntimeException("Invalid property! Neither a property ID nor a query name is provided!");
                    }

                    PropertyType? propertyType;
                    try
                    {
                        string propertyTypeStr = GetString(jsonPropertyMap, BrowserConstants.JsonPropertyDatatype);
                        propertyType = propertyTypeStr.GetCmisEnum<PropertyType>();
                    }
                    catch (Exception e)
                    {
                        throw new CmisRuntimeException("Invalid property datatype: " + id, e);
                    }

                    if (propertyType == null)
                    {
                        throw new CmisRuntimeException("Invalid property datatype: " + id);
                    }


                    object value = jsonPropertyMap[BrowserConstants.JsonPropertyValue];
                    IList<object> values = null;
                    if (value is JsonArray)
                    {
                        values = (JsonArray)value;
                    }
                    else if (value != null)
                    {
                        values = new List<object>() { value };
                    }

                    PropertyData property = new PropertyData((PropertyType)propertyType);
                    switch (propertyType)
                    {
                        case PropertyType.String:
                        case PropertyType.Id:
                        case PropertyType.Html:
                        case PropertyType.Uri:
                            property.Values = CopyStringValues(values);
                            break;
                        case PropertyType.Boolean:
                            property.Values = CopyBooleanValues(values);
                            break;
                        case PropertyType.Integer:
                            property.Values = CopyIntegerValues(values);
                            break;
                        case PropertyType.Decimal:
                            property.Values = CopyDecimalValues(values);
                            break;
                        case PropertyType.DateTime:
                            property.Values = CopyDateTimeValues(values);
                            break;
                        default:
                            throw new CmisRuntimeException("Unknown property type!");
                    }

                    property.Id = id;
                    property.DisplayName = GetString(jsonPropertyMap, BrowserConstants.JsonPropertyDisplayname);
                    property.QueryName = queryName;
                    property.LocalName = GetString(jsonPropertyMap, BrowserConstants.JsonPropertyLocalName);

                    ConvertExtension(jsonPropertyMap, property, BrowserConstants.PropertyKeys);

                    result.AddProperty(property);
                }
            }

            if (extJson != null)
            {
                ConvertExtension(extJson, result, new HashSet<string>());
            }

            return result;
        }

        /// <summary>
        /// Converts properties.
        /// </summary>
        internal static IProperties ConvertSuccinctProperties(JsonObject json, JsonObject extJson, ITypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ITypeDefinition typeDef = null;
            // TODO: try....

            object objectTypeId;
            if (json.TryGetValue(PropertyIds.ObjectTypeId, out objectTypeId))
            {
                if (objectTypeId is string)
                {
                    typeDef = typeCache.GetTypeDefinition((string)objectTypeId);
                }
            }

            JsonArray secTypeIds = GetJsonArray(json, PropertyIds.SecondaryObjectTypeIds);
            IList<ITypeDefinition> secTypeDefs = null;
            if (IsNotEmpty(secTypeIds))
            {
                secTypeDefs = new List<ITypeDefinition>(secTypeIds.Count);
                foreach (object secTypeId in secTypeIds)
                {
                    if (secTypeId is String)
                    {
                        secTypeDefs.Add(typeCache.GetTypeDefinition((string)secTypeId));
                    }
                }
            }

            Properties result = new Properties();

            foreach (KeyValuePair<string, object> kv in json)
            {
                string id = kv.Key;

                IPropertyDefinition propDef = null;
                if (typeDef != null)
                {
                    propDef = typeDef[id];
                }

                if (propDef == null && secTypeDefs != null)
                {
                    foreach (ITypeDefinition secTypeDef in secTypeDefs)
                    {
                        if (secTypeDef != null && secTypeDef.PropertyDefinitions != null)
                        {
                            propDef = secTypeDef[id];
                            if (propDef != null)
                            {
                                break;
                            }
                        }
                    }
                }

                if (propDef == null)
                {
                    propDef = typeCache.GetTypeDefinition(BaseTypeId.CmisDocument.GetCmisValue())[id];
                }

                if (propDef == null)
                {
                    propDef = typeCache.GetTypeDefinition(BaseTypeId.CmisFolder.GetCmisValue())[id];
                }

                if (propDef == null && typeDef != null)
                {
                    ITypeDefinition reloadedTypeDef = typeCache.ReloadTypeDefinition(typeDef.Id);
                    if (reloadedTypeDef != null)
                    {
                        propDef = reloadedTypeDef[id];
                    }
                }

                if (propDef == null && secTypeDefs != null)
                {
                    foreach (ITypeDefinition secTypeDef in secTypeDefs)
                    {
                        ITypeDefinition reloadedTypeDef = typeCache.ReloadTypeDefinition(secTypeDef.Id);
                        if (reloadedTypeDef != null && reloadedTypeDef.PropertyDefinitions != null)
                        {
                            propDef = reloadedTypeDef[id];
                            if (propDef != null)
                            {
                                break;
                            }
                        }
                    }
                }

                IList<object> values = null;
                if (kv.Value is JsonArray)
                {
                    values = (JsonArray)kv.Value;
                }
                else if (kv.Value != null)
                {
                    values = new List<object>() { kv.Value };
                }

                PropertyData property = null;

                if (propDef != null)
                {
                    property = new PropertyData(propDef.PropertyType);

                    switch (propDef.PropertyType)
                    {
                        case PropertyType.String:
                        case PropertyType.Id:
                        case PropertyType.Html:
                        case PropertyType.Uri:
                            property.Values = CopyStringValues(values);
                            break;
                        case PropertyType.Boolean:
                            property.Values = CopyBooleanValues(values);
                            break;
                        case PropertyType.Integer:
                            property.Values = CopyIntegerValues(values);
                            break;
                        case PropertyType.Decimal:
                            property.Values = CopyDecimalValues(values);
                            break;
                        case PropertyType.DateTime:
                            property.Values = CopyDateTimeValues(values);
                            break;
                        default:
                            throw new CmisRuntimeException("Unknown property type!");
                    }

                    property.Id = id;
                    property.DisplayName = propDef.DisplayName;
                    property.QueryName = propDef.QueryName;
                    property.LocalName = propDef.LocalName;
                }
                else
                {
                    // this else block should only be reached in rare circumstances
                    // it may return incorrect types

                    if (values == null)
                    {
                        property = new PropertyData(PropertyType.String);
                        property.Values = null;
                    }
                    else
                    {
                        object firstValue = values[0];
                        if (firstValue is bool)
                        {
                            property = new PropertyData(PropertyType.Boolean);
                            property.Values = CopyBooleanValues(values);
                        }
                        else if (firstValue is BigInteger)
                        {
                            property = new PropertyData(PropertyType.Integer);
                            property.Values = CopyIntegerValues(values);
                        }
                        else if (firstValue is decimal)
                        {
                            property = new PropertyData(PropertyType.Decimal);
                            property.Values = CopyDecimalValues(values);
                        }
                        else
                        {
                            property = new PropertyData(PropertyType.String);
                            property.Values = CopyStringValues(values);
                        }
                    }

                    property.Id = id;
                    property.DisplayName = id;
                    property.QueryName = null;
                    property.LocalName = null;
                }

                result.AddProperty(property);
            }

            if (extJson != null)
            {
                ConvertExtension(extJson, result, new HashSet<string>());
            }

            return result;
        }

        private static IList<object> CopyStringValues(IList<object> source)
        {
            List<object> result = null;
            if (source != null)
            {
                result = new List<object>(source.Count);
                foreach (object obj in source)
                {
                    if (obj is string)
                    {
                        result.Add((string)obj);
                    }
                    else
                    {
                        throw new CmisRuntimeException("Invalid property value: " + obj);
                    }
                }
            }

            return result;
        }

        private static IList<object> CopyBooleanValues(IList<object> source)
        {
            List<object> result = null;
            if (source != null)
            {
                result = new List<object>(source.Count);
                foreach (object obj in source)
                {
                    if (obj is bool)
                    {
                        result.Add((bool)obj);
                    }
                    else
                    {
                        throw new CmisRuntimeException("Invalid property value: " + obj);
                    }
                }
            }

            return result;
        }

        private static IList<object> CopyIntegerValues(IList<object> source)
        {
            List<object> result = null;
            if (source != null)
            {
                result = new List<object>(source.Count);
                foreach (object obj in source)
                {
                    if (obj is BigInteger)
                    {
                        result.Add((BigInteger)obj);
                    }
                    else
                    {
                        throw new CmisRuntimeException("Invalid property value: " + obj);
                    }
                }
            }

            return result;
        }

        private static IList<object> CopyDecimalValues(IList<object> source)
        {
            List<object> result = null;
            if (source != null)
            {
                result = new List<object>(source.Count);
                foreach (object obj in source)
                {
                    if (obj is decimal)
                    {
                        result.Add((decimal)obj);
                    }
                    else if (obj is BigInteger)
                    {
                        result.Add((decimal)((BigInteger)obj));
                    }
                    else
                    {
                        throw new CmisRuntimeException("Invalid property value: " + obj);
                    }
                }
            }

            return result;
        }

        private static IList<object> CopyDateTimeValues(IList<object> source)
        {
            List<object> result = null;
            if (source != null)
            {
                result = new List<object>(source.Count);
                foreach (object obj in source)
                {
                    if (obj is BigInteger)
                    {
                        result.Add(DateTimeHelper.ConvertMillisToDateTime((long)((BigInteger)obj)));
                    }
                    else if (obj is string)
                    {
                        DateTime dt;
                        try
                        {
                            dt = DateTimeHelper.ParseISO8601((string)obj);
                        }
                        catch (Exception e)
                        {
                            throw new CmisRuntimeException("Invalid property value: " + obj, e);
                        }
                        result.Add(dt);
                    }
                    else
                    {
                        throw new CmisRuntimeException("Invalid property value: " + obj);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a rendition.
        /// </summary>
        internal static IRenditionData ConvertRendition(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            RenditionData result = new RenditionData();

            result.Height = GetInteger(json, BrowserConstants.JsonRenditionHeight);
            result.Kind = GetString(json, BrowserConstants.JsonRenditionKind);
            result.Length = GetInteger(json, BrowserConstants.JsonRenditionLength);
            result.MimeType = GetString(json, BrowserConstants.JsonRenditionMimeType);
            result.RenditionDocumentId = GetString(json, BrowserConstants.JsonRenditionDocumentId);
            result.StreamId = GetString(json, BrowserConstants.JsonRenditionStreamId);
            result.Title = GetString(json, BrowserConstants.JsonRenditionTitle);
            result.Width = GetInteger(json, BrowserConstants.JsonRenditionWidth);

            ConvertExtension(json, result, BrowserConstants.RenditionKeys);

            return result;
        }

        /// <summary>
        /// Converts a list of renditions.
        /// </summary>
        internal static IList<IRenditionData> ConvertRenditions(List<object> json)
        {
            if (json == null)
            {
                return null;
            }

            IList<IRenditionData> result = new List<IRenditionData>();

            foreach (object obj in json)
            {
                IRenditionData rendition = ConvertRendition(GetJsonObject(obj));
                if (rendition != null)
                {
                    result.Add(rendition);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a object list.
        /// </summary>
        internal static ObjectInFolderList ConvertObjectInFolderList(JsonObject json, ITypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ObjectInFolderList result = new ObjectInFolderList();

            JsonArray jsonChildren = GetJsonArray(json, BrowserConstants.JsonObjectInFolderListObjects);
            IList<IObjectInFolderData> objects = new List<IObjectInFolderData>();

            if (jsonChildren != null)
            {
                foreach (object obj in jsonChildren)
                {
                    JsonObject JsonObject = GetJsonObject(obj);
                    if (JsonObject != null)
                    {
                        objects.Add(ConvertObjectInFolder(JsonObject, typeCache));
                    }
                }
            }

            result.Objects = objects;
            result.HasMoreItems = GetBoolean(json, BrowserConstants.JsonObjectInFolderListHasMoreItems);
            result.NumItems = GetInteger(json, BrowserConstants.JsonObjectInFolderListNumItems);

            ConvertExtension(json, result, BrowserConstants.ObjectInFolderListKeys);

            return result;
        }

        /// <summary>
        /// Converts an object in a folder.
        /// </summary>
        internal static ObjectInFolderData ConvertObjectInFolder(JsonObject json, ITypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ObjectInFolderData result = new ObjectInFolderData();

            result.Object = ConvertObject(GetJsonObject(json, BrowserConstants.JsonObjectInFolderObject), typeCache);
            result.PathSegment = GetString(json, BrowserConstants.JsonObjectInFolderPathSegment);

            ConvertExtension(json, result, BrowserConstants.ObjectInFolderKeys);

            return result;
        }

        /// <summary>
        /// Converts a descendants tree.
        /// </summary>
        internal static IList<IObjectInFolderContainer> ConvertDescendants(JsonArray json, ITypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            IList<IObjectInFolderContainer> result = new List<IObjectInFolderContainer>();

            foreach (object obj in json)
            {
                JsonObject desc = GetJsonObject(obj);
                if (desc != null)
                {
                    result.Add(ConvertDescendant(desc, typeCache));
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a descendant.
        /// </summary>
        internal static IObjectInFolderContainer ConvertDescendant(JsonObject json, ITypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            ObjectInFolderContainer result = new ObjectInFolderContainer();

            result.Object = ConvertObjectInFolder(GetJsonObject(json, BrowserConstants.JsonObjectInFolderContainerObject), typeCache);

            IList<IObjectInFolderContainer> containerList = new List<IObjectInFolderContainer>();
            JsonArray jsonContainerList = GetJsonArray(json, BrowserConstants.JsonObjectInFolderContainerChildren);
            if (jsonContainerList != null)
            {
                foreach (object obj in jsonContainerList)
                {
                    JsonObject containerChild = GetJsonObject(obj);
                    if (containerChild != null)
                    {
                        containerList.Add(ConvertDescendant(containerChild, typeCache));
                    }
                }
            }

            result.Children = containerList;

            ConvertExtension(json, result, BrowserConstants.ObjectInFolderContainerKeys);

            return result;
        }

        /// <summary>
        /// Converts an object parents list.
        /// </summary>
        internal static IList<IObjectParentData> ConvertObjectParents(JsonArray json, ITypeCache typeCache)
        {
            if (json == null)
            {
                return null;
            }

            IList<IObjectParentData> result = new List<IObjectParentData>();

            foreach (object obj in json)
            {
                JsonObject jsonParent = GetJsonObject(obj);
                if (jsonParent != null)
                {
                    ObjectParentData parent = new ObjectParentData();

                    parent.Object = ConvertObject(GetJsonObject(jsonParent, BrowserConstants.JsonObjectParentsObject), typeCache);
                    parent.RelativePathSegment = GetString(jsonParent, BrowserConstants.JsonObjectParentsRelativePathSegment);

                    ConvertExtension(jsonParent, parent, BrowserConstants.ObjectParentsKeys);

                    result.Add(parent);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a object list.
        /// </summary>
        internal static IObjectList ConvertObjectList(JsonObject json, ITypeCache typeCache, bool isQueryResult)
        {
            if (json == null)
            {
                return null;
            }

            ObjectList result = new ObjectList();

            JsonArray jsonChildren = GetJsonArray(json, isQueryResult ? BrowserConstants.JsonQueryResultListResults : BrowserConstants.JsonObjectListObjects);
            IList<IObjectData> objects = new List<IObjectData>();

            if (jsonChildren != null)
            {
                foreach (object obj in jsonChildren)
                {
                    JsonObject JsonObject = GetJsonObject(obj);
                    if (JsonObject != null)
                    {
                        objects.Add(ConvertObject(JsonObject, typeCache));
                    }
                }
            }

            result.Objects = objects;

            if (isQueryResult)
            {
                result.HasMoreItems = GetBoolean(json, BrowserConstants.JsonQueryResultListHasMoreItems);
                result.NumItems = GetInteger(json, BrowserConstants.JsonQueryResultListNumItems);
                ConvertExtension(json, result, BrowserConstants.QueryResultListKeys);
            }
            else
            {
                result.HasMoreItems = GetBoolean(json, BrowserConstants.JsonObjectListHasMoreItems);
                result.NumItems = GetInteger(json, BrowserConstants.JsonObjectListNumItems);
                ConvertExtension(json, result, BrowserConstants.ObjectListKeys);
            }

            return result;
        }

        // -----------------------------------------------------------------

        /// <summary>
        /// Converts FailedToDelete ids.
        /// </summary>
        internal static JsonObject Convert(IFailedToDeleteData ftd)
        {
            if (ftd == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            JsonArray ids = new JsonArray();
            if (ftd.Ids != null)
            {
                foreach (string id in ftd.Ids)
                {
                    ids.Add(id);
                }
            }

            result.Add(BrowserConstants.JsonFailedToDeleteId, ids);

            ConvertExtension(ftd, result);

            return result;
        }

        /// <summary>
        /// Converts FailedToDelete ids.
        /// </summary>
        internal static IFailedToDeleteData ConvertFailedToDelete(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            FailedToDeleteData result = new FailedToDeleteData();

            List<string> ids = new List<string>();
            JsonArray jsonIds = GetJsonArray(json, BrowserConstants.JsonFailedToDeleteId);

            if (jsonIds != null)
            {
                foreach (object obj in jsonIds)
                {
                    if (obj != null)
                    {
                        ids.Add(obj.ToString());
                    }
                }
            }

            result.Ids = ids;

            ConvertExtension(json, result, BrowserConstants.FailedToDeleteKeys);

            return result;
        }

        // -----------------------------------------------------------------

        /// <summary>
        /// Converts bulk update data.
        /// </summary>
        internal static JsonObject Convert(IBulkUpdateObjectIdAndChangeToken oc)
        {
            if (oc == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            SetIfNotNull(BrowserConstants.JsonBulkUpdateId, oc.Id, result);
            SetIfNotNull(BrowserConstants.JsonBulkUpdateNewId, oc.NewId, result);
            SetIfNotNull(BrowserConstants.JsonBulkUpdateChangeToken, oc.ChangeToken, result);

            ConvertExtension(oc, result);

            return result;
        }

        /// <summary>
        /// Converts bulk update data lists.
        /// </summary>
        internal static IList<IBulkUpdateObjectIdAndChangeToken> ConvertBulkUpdate(JsonArray json)
        {
            if (json == null)
            {
                return null;
            }

            IList<IBulkUpdateObjectIdAndChangeToken> result = new List<IBulkUpdateObjectIdAndChangeToken>();

            foreach (object ocJson in json)
            {
                IBulkUpdateObjectIdAndChangeToken oc = ConvertBulkUpdate(GetJsonObject(ocJson));
                if (oc != null)
                {
                    result.Add(oc);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts bulk update data.
        /// </summary>
        internal static IBulkUpdateObjectIdAndChangeToken ConvertBulkUpdate(JsonObject json)
        {
            if (json == null)
            {
                return null;
            }

            string id = GetString(json, BrowserConstants.JsonBulkUpdateId);
            string newId = GetString(json, BrowserConstants.JsonBulkUpdateNewId);
            string changeToken = GetString(json, BrowserConstants.JsonBulkUpdateChangeToken);

            BulkUpdateObjectIdAndChangeToken result = new BulkUpdateObjectIdAndChangeToken() { Id = id, NewId = newId, ChangeToken = changeToken };

            ConvertExtension(json, result, BrowserConstants.BulkUpdateKeys);

            return result;
        }

        // -----------------------------------------------------------------

        internal static void ConvertExtension(IExtensionsData source, JsonObject target)
        {
            if (source == null || source.Extensions == null)
            {
                return;
            }

            foreach (ICmisExtensionElement ext in source.Extensions)
            {
                AddExtensionToTarget(ext, target);
            }
        }

        private static JsonObject ConvertExtensionList(IList<ICmisExtensionElement> extensionList)
        {
            if (extensionList == null)
            {
                return null;
            }

            JsonObject result = new JsonObject();

            foreach (ICmisExtensionElement ext in extensionList)
            {
                AddExtensionToTarget(ext, result);
            }

            return result;
        }

        private static void AddExtensionToTarget(ICmisExtensionElement ext, JsonObject target)
        {
            if (ext == null)
            {
                return;
            }

            object value = null;

            if (IsNotEmpty(ext.Children))
            {
                value = ConvertExtensionList(ext.Children);
            }
            else
            {
                value = ext.Value;
            }

            if (!target.ContainsKey(ext.Name))
            {
                target.Add(ext.Name, value);
            }
            else
            {
                object extValue = target[ext.Name];

                JsonArray array;
                if (extValue is JsonArray)
                {
                    array = (JsonArray)extValue;
                }
                else
                {
                    array = new JsonArray();
                    array.Add(extValue);
                }

                array.Add(value);

                target.Add(ext.Name, array);
            }
        }

        internal static void ConvertExtension(JsonObject source, IExtensionsData target, ISet<string> cmisKeys)
        {
            if (source == null)
            {
                return;
            }

            IList<ICmisExtensionElement> extensions = null;

            foreach (KeyValuePair<string, object> element in source)
            {
                if (cmisKeys.Contains(element.Key))
                {
                    continue;
                }

                if (extensions == null)
                {
                    extensions = new List<ICmisExtensionElement>();
                }

                if (element.Value is JsonObject)
                {
                    extensions.Add(new CmisExtensionElement() { Name = element.Key, Children = ConvertExtension((JsonObject)element.Value) });
                }
                else if (element.Value is JsonArray)
                {
                    IList<ICmisExtensionElement> exts = ConvertExtension(element.Key, (JsonArray)element.Value);
                    foreach (ICmisExtensionElement ext in exts)
                    {
                        extensions.Add(ext);
                    }
                }
                else
                {
                    string value = (element.Value == null ? null : element.Value.ToString());
                    extensions.Add(new CmisExtensionElement() { Name = element.Key, Value = value });
                }
            }

            target.Extensions = extensions;
        }

        internal static IList<ICmisExtensionElement> ConvertExtension(JsonObject map)
        {
            if (map == null)
            {
                return null;
            }

            IList<ICmisExtensionElement> extensions = new List<ICmisExtensionElement>();

            foreach (KeyValuePair<string, object> element in map)
            {
                if (element.Value is JsonObject)
                {
                    extensions.Add(new CmisExtensionElement() { Name = element.Key, Children = ConvertExtension((JsonObject)element.Value) });
                }
                else if (element.Value is JsonArray)
                {
                    IList<ICmisExtensionElement> exts = ConvertExtension(element.Key, (JsonArray)element.Value);
                    foreach (ICmisExtensionElement ext in exts)
                    {
                        extensions.Add(ext);
                    }
                }
                else
                {
                    string value = (element.Value == null ? null : element.Value.ToString());
                    extensions.Add(new CmisExtensionElement() { Name = element.Key, Value = value });
                }
            }

            return extensions;
        }

        internal static IList<ICmisExtensionElement> ConvertExtension(string name, JsonArray list)
        {
            if (list == null)
            {
                return null;
            }

            IList<ICmisExtensionElement> extensions = new List<ICmisExtensionElement>();

            foreach (object element in list)
            {
                if (element is JsonObject)
                {
                    extensions.Add(new CmisExtensionElement() { Name = name, Children = ConvertExtension((JsonObject)element) });
                }
                else if (element is JsonArray)
                {
                    IList<ICmisExtensionElement> exts = ConvertExtension(name, (JsonArray)element);
                    foreach (ICmisExtensionElement ext in exts)
                    {
                        extensions.Add(ext);
                    }
                }
                else
                {
                    string value = (element == null ? null : element.ToString());
                    extensions.Add(new CmisExtensionElement() { Name = name, Value = value });
                }
            }

            return extensions;
        }

        // -----------------------------------------------------------------

        internal static string GetJsonStringValue(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return obj.ToString();
        }

        internal static object GetJsonValue(object value, DateTimeFormat dateTimeFormat)
        {
            if (value is DateTime)
            {
                if (dateTimeFormat == DateTimeFormat.Extended)
                {
                    return DateTimeHelper.FormatISO8601((DateTime)value);
                }
                else
                {
                    return new BigInteger(DateTimeHelper.ConvertDateTimeToMillis((DateTime)value));
                }
            }

            return value;
        }

        internal static string GetJsonEnumValue<T>(T? cmisEnum) where T : struct
        {
            if (!cmisEnum.HasValue)
            {
                return null;
            }

            if (!Enum.IsDefined(typeof(T), cmisEnum.Value))
            {
                throw new ArgumentException("Invalid enum value!");
            }

            return ((Enum)Enum.ToObject(typeof(T), cmisEnum.Value)).GetCmisValue();
        }

        internal static object GetCmisValue(object value, PropertyType propertyType)
        {
            if (value == null)
            {
                return null;
            }

            switch (propertyType)
            {
                case PropertyType.String:
                case PropertyType.Id:
                case PropertyType.Html:
                case PropertyType.Uri:
                    if (value is string)
                    {
                        return value;
                    }
                    throw new CmisRuntimeException("Invalid String value!");
                case PropertyType.Boolean:
                    if (value is bool)
                    {
                        return value;
                    }
                    throw new CmisRuntimeException("Invalid Boolean value!");
                case PropertyType.Integer:
                    if (value is BigInteger)
                    {
                        return value;
                    }
                    throw new CmisRuntimeException("Invalid Integer value!");
                case PropertyType.Decimal:
                    if (value is decimal)
                    {
                        return value;
                    }
                    throw new CmisRuntimeException("Invalid Decimal value!");
                case PropertyType.DateTime:
                    if (value is BigInteger)
                    {
                        return DateTimeHelper.ConvertMillisToDateTime((long)((BigInteger)value));
                    }
                    else if (value is string)
                    {
                        DateTime dt;
                        try
                        {
                            dt = DateTimeHelper.ParseISO8601((string)value);
                        }
                        catch (Exception e)
                        {
                            throw new CmisRuntimeException("Invalid DateTime value!", e);
                        }
                        return dt;
                    }
                    throw new CmisRuntimeException("Invalid DateTime value!");
                default:
                    break;
            }

            throw new CmisRuntimeException("Unknown property type!");
        }

        internal static JsonArray GetJsonArrayFromList<T>(IList<T> list)
        {
            if (list == null)
            {
                return null;
            }

            JsonArray result = new JsonArray();
            foreach (T value in list)
            {
                result.Add(value);
            }

            return result;
        }

        internal static void SetIfNotNull(string name, object obj, JsonObject json)
        {
            if (obj != null)
            {
                json.Add(name, obj);
            }
        }

        internal static JsonObject GetJsonObject(object o)
        {
            if (o == null)
            {
                return null;
            }

            if (o is JsonObject)
            {
                return (JsonObject)o;
            }

            throw new CmisRuntimeException("Expected a JSON object but found a "
                + (o is JsonArray ? "JSON array" : o.GetType().ToString()) + ": " + o.ToString());
        }


        internal static object GetObject(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                return value;
            }

            return null;
        }

        internal static JsonObject GetJsonObject(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                if (value is JsonObject)
                {
                    return (JsonObject)value;
                }
                else if (value == null)
                {
                    return null;
                }

                throw new CmisRuntimeException("Expected a JSON object but found a "
                    + (value is JsonArray ? "JSON array" : value.GetType().ToString()) + ": " + value.ToString());
            }

            return null;
        }

        internal static JsonArray GetJsonArray(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                if (value is JsonArray)
                {
                    return (JsonArray)value;
                }
                else if (value == null)
                {
                    return null;
                }

                throw new CmisRuntimeException("Expected a JSON array but found a "
                    + (value is JsonObject ? "JSON object" : value.GetType().ToString()) + ": " + value.ToString());
            }

            return null;
        }

        internal static string GetString(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                return value as string;
            }

            return null;
        }

        internal static bool? GetBoolean(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                return value as bool?;
            }

            return null;
        }

        internal static BigInteger? GetInteger(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                return value as BigInteger?;
            }

            return null;
        }

        internal static decimal? GetDecimal(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                if (value is decimal)
                {
                    return (decimal)value;
                }
                if (value is BigInteger)
                {
                    return (decimal)((BigInteger)value);
                }
            }

            return null;
        }

        internal static DateTime? GetDateTime(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                if (value is BigInteger)
                {
                    return DateTimeHelper.ConvertMillisToDateTime((long)((BigInteger)value));
                }
                else if (value is string)
                {
                    return DateTimeHelper.ParseISO8601((string)value);
                }
            }

            return null;
        }


        internal static T GetEnum<T>(JsonObject json, string key)
        {
            object value;
            if (json.TryGetValue(key, out value))
            {
                if (value is string)
                {
                    return ((string)value).GetCmisEnum<T>();
                }
            }

            return default(T);
        }

        internal static bool IsNotEmpty<T>(ICollection<T> col)
        {
            return col != null && col.Count > 0;
        }

        internal static bool IsNotEmpty(JsonObject json)
        {
            return json != null && json.Count > 0;
        }

        internal static bool IsNullOrEmpty<T>(ICollection<T> col)
        {
            return col == null || col.Count == 0;
        }
    }
}
