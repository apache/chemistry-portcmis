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
using PortCMIS.Enums;

namespace PortCMIS.Binding.Browser
{
    class BrowserConstants
    {
        public const string ErrorException = "exception";
        public const string ErrorMessage = "message";
        public const string ErrorStacktrace = "stacktrace";

        public const string JsonRepInfoId = "repositoryId";
        public const string JsonRepInfoName = "repositoryName";
        public const string JsonRepInfoDescription = "repositoryDescription";
        public const string JsonRepInfoVendor = "vendorName";
        public const string JsonRepInfoProduct = "productName";
        public const string JsonRepInfoProductVersion = "productVersion";
        public const string JsonRepInfoRootFolderId = "rootFolderId";
        public const string JsonRepInfoRepositoryUrl = "repositoryUrl";
        public const string JsonRepInfoRootFolderUrl = "rootFolderUrl";
        public const string JsonRepInfoCapabilities = "capabilities";
        public const string JsonRepInfoAclCapabilities = "aclCapabilities";
        public const string JsonRepInfoChangeLogToken = "latestChangeLogToken";
        public const string JsonRepInfoCmisVersionSupported = "cmisVersionSupported";
        public const string JsonRepInfoThinClientUri = "thinClientURI";
        public const string JsonRepInfoChangesIncomplete = "changesIncomplete";
        public const string JsonRepInfoChangesOnType = "changesOnType";
        public const string JsonRepInfoPrincipalIdAnonymous = "principalIdAnonymous";
        public const string JsonRepInfoPrincipalIdAnyone = "principalIdAnyone";
        public const string JsonRepInfoExtendedFeatures = "extendedFeatures";

        public static readonly ISet<string> RepInfoKeys = new HashSet<string>()
        {
            JsonRepInfoId,
            JsonRepInfoName,
            JsonRepInfoDescription,
            JsonRepInfoVendor,
            JsonRepInfoProduct,
            JsonRepInfoProductVersion,
            JsonRepInfoRootFolderId,
            JsonRepInfoRepositoryUrl,
            JsonRepInfoRootFolderUrl,
            JsonRepInfoCapabilities,
            JsonRepInfoAclCapabilities,
            JsonRepInfoChangeLogToken,
            JsonRepInfoCmisVersionSupported,
            JsonRepInfoThinClientUri,
            JsonRepInfoChangesIncomplete,
            JsonRepInfoChangesOnType,
            JsonRepInfoPrincipalIdAnonymous,
            JsonRepInfoPrincipalIdAnyone,
            JsonRepInfoExtendedFeatures
        };

        public const string JsonCapContentStreamUpdatability = "capabilityContentStreamUpdatability";
        public const string JsonCapChanges = "capabilityChanges";
        public const string JsonCapRenditions = "capabilityRenditions";
        public const string JsonCapGetDescendants = "capabilityGetDescendants";
        public const string JsonCapGetFolderTree = "capabilityGetFolderTree";
        public const string JsonCapMultifiling = "capabilityMultifiling";
        public const string JsonCapUnfiling = "capabilityUnfiling";
        public const string JsonCapVersionSpecificFiling = "capabilityVersionSpecificFiling";
        public const string JsonCapPwcSearchable = "capabilityPWCSearchable";
        public const string JsonCapPwcUpdatable = "capabilityPWCUpdatable";
        public const string JsonCapAllVersionsSerachable = "capabilityAllVersionsSearchable";
        public const string JsonCapOrderBy = "capabilityOrderBy";
        public const string JsonCapQuery = "capabilityQuery";
        public const string JsonCapJoin = "capabilityJoin";
        public const string JsonCapAcl = "capabilityACL";
        public const string JsonCapCreatablePropertyTypes = "capabilityCreatablePropertyTypes";
        public const string JsonCapNewTypeSettableAttributes = "capabilityNewTypeSettableAttributes";

        public static readonly ISet<string> CapKeys = new HashSet<string>()
        {
            JsonCapContentStreamUpdatability,
            JsonCapChanges,
            JsonCapRenditions,
            JsonCapGetDescendants,
            JsonCapGetFolderTree,
            JsonCapMultifiling,
            JsonCapUnfiling,
            JsonCapVersionSpecificFiling,
            JsonCapPwcSearchable,
            JsonCapPwcUpdatable,
            JsonCapAllVersionsSerachable,
            JsonCapOrderBy,
            JsonCapQuery,
            JsonCapJoin,
            JsonCapAcl,
            JsonCapCreatablePropertyTypes,
            JsonCapNewTypeSettableAttributes
        };

        public const string JsonCapCreatablePropertyTypesCanCreate = "canCreate";

        public static readonly ISet<string> CapCreatablePropertyTypesKeys = new HashSet<string>()
        {
            JsonCapCreatablePropertyTypesCanCreate
        };

        public const string JsonCapNewTypeSettableAttributesId = "id";
        public const string JsonCapNewTypeSettableAttributesLocalName = "localName";
        public const string JsonCapNewTypeSettableAttributesLocalNameSpace = "localNamespace";
        public const string JsonCapNewTypeSettableAttributesDisplayName = "displayName";
        public const string JsonCapNewTypeSettableAttributesQueryName = "queryName";
        public const string JsonCapNewTypeSettableAttributesDescription = "description";
        public const string JsonCapNewTypeSettableAttributesCreatable = "creatable";
        public const string JsonCapNewTypeSettableAttributesFileable = "fileable";
        public const string JsonCapNewTypeSettableAttributesQueryable = "queryable";
        public const string JsonCapNewTypeSettableAttributesFulltextIndexed = "fulltextIndexed";
        public const string JsonCapNewTypeSettableAttributesIncludeInSupertypeQuery = "includedInSupertypeQuery";
        public const string JsonCapNewTypeSettableAttributesControlablePolicy = "controllablePolicy";
        public const string JsonCapNewTypeSettableAttributesControlableAcl = "controllableACL";

        public static readonly ISet<string> CapNewTypeSettableAttributesKeys = new HashSet<string>()
        {
            JsonCapNewTypeSettableAttributesId,
            JsonCapNewTypeSettableAttributesLocalName,
            JsonCapNewTypeSettableAttributesLocalNameSpace,
            JsonCapNewTypeSettableAttributesDisplayName,
            JsonCapNewTypeSettableAttributesQueryName,
            JsonCapNewTypeSettableAttributesDescription,
            JsonCapNewTypeSettableAttributesCreatable,
            JsonCapNewTypeSettableAttributesQueryable,
            JsonCapNewTypeSettableAttributesFileable,
            JsonCapNewTypeSettableAttributesFulltextIndexed,
            JsonCapNewTypeSettableAttributesIncludeInSupertypeQuery,
            JsonCapNewTypeSettableAttributesControlablePolicy,
            JsonCapNewTypeSettableAttributesControlableAcl
        };

        public const string JsonAclCapSupportedPermissions = "supportedPermissions";
        public const string JsonAclCapAclPropagation = "propagation";
        public const string JsonAclCapPermissions = "permissions";
        public const string JsonAclCapPermissionMapping = "permissionMapping";

        public static readonly ISet<string> AclCapKeys = new HashSet<string>()
        {
            JsonAclCapSupportedPermissions,
            JsonAclCapAclPropagation,
            JsonAclCapPermissions,
            JsonAclCapPermissionMapping
        };

        public const string JsonAclCapPermissionPermission = "permission";
        public const string JsonAclCapPermissionDescription = "description";

        public static readonly ISet<string> AclCapPermissionKeys = new HashSet<string>()
        { 
            JsonAclCapPermissionPermission,
            JsonAclCapPermissionDescription
        };

        public const string JsonAclCapMappingKey = "key";
        public const string JsonAclCapMappingPermission = "permission";

        public static readonly ISet<string> AclCapMappingKeys = new HashSet<string>()
        {
            JsonAclCapMappingKey,
            JsonAclCapMappingPermission
        };

        public const string JsonFeatureId = "id";
        public const string JsonFeatureUrl = "url";
        public const string JsonFeatureCommonName = "commonName";
        public const string JsonFeatureVersionLabel = "versionLabel";
        public const string JsonFeatureDescription = "description";
        public const string JsonFeatureData = "featureData";

        public static readonly ISet<string> FeatureKeys = new HashSet<string>()
        {
            JsonFeatureId,
            JsonFeatureUrl,
            JsonFeatureCommonName,
            JsonFeatureVersionLabel,
            JsonFeatureDescription,
            JsonFeatureData
        };

        public const string JsonObjectProperties = "properties";
        public const string JsonObjectSuccinctProperties = "succinctProperties";
        public const string JsonObjectPropertiesExtension = "propertiesExtension";
        public const string JsonObjectAllowableActions = "allowableActions";
        public const string JsonObjectRelationships = "relationships";
        public const string JsonObjectChangeEventInfo = "changeEventInfo";
        public const string JsonObjectAcl = "acl";
        public const string JsonObjectExactAcl = "exactACL";
        public const string JsonObjectPolicyIds = "policyIds";
        public const string JsonObjectPolicyIdsIds = "ids";
        public const string JsonObjectRenditions = "renditions";

        public static readonly ISet<string> ObjectKeys = new HashSet<string>()
        {
            JsonObjectProperties,
            JsonObjectSuccinctProperties,
            JsonObjectPropertiesExtension,
            JsonObjectAllowableActions,
            JsonObjectRelationships,
            JsonObjectChangeEventInfo,
            JsonObjectAcl,
            JsonObjectExactAcl,
            JsonObjectPolicyIds,
            JsonObjectRenditions
        };

        public static readonly ISet<string> AllowableActionsKeys = new HashSet<string>();
        static BrowserConstants()
        {
            var values = Enum.GetValues(typeof(PortCMIS.Enums.Action));
            foreach (var value in values)
            {
                PortCMIS.Enums.Action action = (PortCMIS.Enums.Action)Enum.ToObject(typeof(PortCMIS.Enums.Action), value);
                AllowableActionsKeys.Add(action.GetCmisValue());
            }
        }

        public static readonly ISet<string> PolicyIdsKeys = new HashSet<string>()
        {
            JsonObjectPolicyIdsIds
        };

        public const string JsonObjectInFolderObject = "object";
        public const string JsonObjectInFolderPathSegment = "pathSegment";

        public static readonly ISet<string> ObjectInFolderKeys = new HashSet<string>()
        {
            JsonObjectInFolderObject,
            JsonObjectInFolderPathSegment
        };

        public const string JsonObjectParentsObject = "object";
        public const string JsonObjectParentsRelativePathSegment = "relativePathSegment";

        public static readonly ISet<string> ObjectParentsKeys = new HashSet<string>()
        {
            JsonObjectParentsObject,
            JsonObjectParentsRelativePathSegment
        };

        public const string JsonPropertyId = "id";
        public const string JsonPropertyLocalName = "localName";
        public const string JsonPropertyDisplayname = "displayName";
        public const string JsonPropertyQueryName = "queryName";
        public const string JsonPropertyValue = "value";
        public const string JsonPropertyDatatype = "type";
        public const string JsonPropertyCardinality = "cardinality";

        public static readonly ISet<string> PropertyKeys = new HashSet<string>()
        {
            JsonPropertyId,
            JsonPropertyLocalName,
            JsonPropertyDisplayname,
            JsonPropertyQueryName,
            JsonPropertyValue,
            JsonPropertyDatatype,
            JsonPropertyCardinality
        };

        public const string JsonChangeEventType = "changeType";
        public const string JsonChangeEventTime = "changeTime";

        public static readonly ISet<string> ChangeEventKeys = new HashSet<string>()
        {
            JsonChangeEventType,
            JsonChangeEventTime
        };

        public const string JsonAclAces = "aces";
        public const string JsonAclIsExact = "isExact";

        public static readonly ISet<string> AclKeys = new HashSet<string>()
        {
            JsonAclAces,
            JsonAclIsExact
        };

        public const string JsonAcePrincipal = "principal";
        public const string JsonAcePrincipalId = "principalId";
        public const string JsonAcePermissions = "permissions";
        public const string JsonAceIsDirect = "isDirect";

        public static readonly ISet<string> AceKeys = new HashSet<string>()
        {
            JsonAcePrincipal,
            JsonAcePrincipalId,
            JsonAcePermissions,
            JsonAceIsDirect
        };

        public static readonly ISet<string> PrincipalKeys = new HashSet<string>()
        {
            JsonAcePrincipalId
        };

        public const string JsonRenditionStreamId = "streamId";
        public const string JsonRenditionMimeType = "mimeType";
        public const string JsonRenditionLength = "length";
        public const string JsonRenditionKind = "kind";
        public const string JsonRenditionTitle = "title";
        public const string JsonRenditionHeight = "height";
        public const string JsonRenditionWidth = "width";
        public const string JsonRenditionDocumentId = "renditionDocumentId";

        public static readonly ISet<string> RenditionKeys = new HashSet<string>()
        {
            JsonRenditionStreamId,
            JsonRenditionMimeType,
            JsonRenditionLength,
            JsonRenditionKind,
            JsonRenditionTitle,
            JsonRenditionHeight,
            JsonRenditionWidth,
            JsonRenditionDocumentId
        };

        public const string JsonObjectListObjects = "objects";
        public const string JsonObjectListHasMoreItems = "hasMoreItems";
        public const string JsonObjectListNumItems = "numItems";
        public const string JsonObjectListChangeLogToken = "changeLogToken";

        public static readonly ISet<string> ObjectListKeys = new HashSet<string>()
        {
            JsonObjectListObjects,
            JsonObjectListHasMoreItems,
            JsonObjectListNumItems,
            JsonObjectListChangeLogToken
        };

        public const string JsonObjectInFolderListObjects = "objects";
        public const string JsonObjectInFolderListHasMoreItems = "hasMoreItems";
        public const string JsonObjectInFolderListNumItems = "numItems";

        public static readonly ISet<string> ObjectInFolderListKeys = new HashSet<string>()
        {
            JsonObjectInFolderListObjects,
            JsonObjectInFolderListHasMoreItems,
            JsonObjectInFolderListNumItems
        };

        public const string JsonObjectInFolderContainerObject = "object";
        public const string JsonObjectInFolderContainerChildren = "children";

        public static readonly ISet<string> ObjectInFolderContainerKeys = new HashSet<string>()
        {
            JsonObjectInFolderContainerObject,
            JsonObjectInFolderContainerChildren
        };

        public const string JsonQueryResultListResults = "results";
        public const string JsonQueryResultListHasMoreItems = "hasMoreItems";
        public const string JsonQueryResultListNumItems = "numItems";

        public static readonly ISet<string> QueryResultListKeys = new HashSet<string>()
        { 
            JsonQueryResultListResults,
            JsonQueryResultListHasMoreItems,
            JsonQueryResultListNumItems
        };

        public const string JsonTypeId = "id";
        public const string JsonTypeLocalName = "localName";
        public const string JsonTypeLocalNameSpace = "localNamespace";
        public const string JsonTypeDisplayname = "displayName";
        public const string JsonTypeQueryName = "queryName";
        public const string JsonTypeDescription = "description";
        public const string JsonTypeBaseId = "baseId";
        public const string JsonTypeParentId = "parentId";
        public const string JsonTypeCreatable = "creatable";
        public const string JsonTypeFileable = "fileable";
        public const string JsonTypeQueryable = "queryable";
        public const string JsonTypeFulltextIndexed = "fulltextIndexed";
        public const string JsonTypeIncludeInSuperTypeQuery = "includedInSupertypeQuery";
        public const string JsonTypeControlablePolicy = "controllablePolicy";
        public const string JsonTypeControlableAcl = "controllableACL";
        public const string JsonTypePropertyDefinitions = "propertyDefinitions";
        public const string JsonTypeTypeMutability = "typeMutability";

        public const string JsonTypeVersionable = "versionable"; // document
        public const string JsonTypeContentstreamAllowed = "contentStreamAllowed"; // document

        public const string JsonTypeAllowedSourceTypes = "allowedSourceTypes"; // relationship
        public const string JsonTypeAllowedTargetTypes = "allowedTargetTypes"; // relationship

        public static readonly ISet<string> TypeKeys = new HashSet<string>()
        {
            JsonTypeId,
            JsonTypeLocalName,
            JsonTypeLocalNameSpace,
            JsonTypeDisplayname,
            JsonTypeQueryName,
            JsonTypeDescription,
            JsonTypeBaseId,
            JsonTypeParentId,
            JsonTypeCreatable,
            JsonTypeFileable,
            JsonTypeQueryable,
            JsonTypeFulltextIndexed,
            JsonTypeIncludeInSuperTypeQuery,
            JsonTypeControlablePolicy,
            JsonTypeControlableAcl,
            JsonTypePropertyDefinitions,
            JsonTypeVersionable,
            JsonTypeContentstreamAllowed,
            JsonTypeAllowedSourceTypes,
            JsonTypeAllowedTargetTypes,
            JsonTypeTypeMutability
        };

        public const string JsonPropertyTypeId = "id";
        public const string JsonPropertyTypeLocalName = "localName";
        public const string JsonPropertyTypeLocalNameSpace = "localNamespace";
        public const string JsonPropertyTypeDisplayname = "displayName";
        public const string JsonPropertyTypeQueryName = "queryName";
        public const string JsonPropertyTypeDescription = "description";
        public const string JsonPropertyTypePropertyType = "propertyType";
        public const string JsonPropertyTypeCardinality = "cardinality";
        public const string JsonPropertyTypeUpdatability = "updatability";
        public const string JsonPropertyTypeInhertited = "inherited";
        public const string JsonPropertyTypeRequired = "required";
        public const string JsonPropertyTypeQueryable = "queryable";
        public const string JsonPropertyTypeOrderable = "orderable";
        public const string JsonPropertyTypeOpenChoice = "openChoice";

        public const string JsonPropertyTypeDefaultValue = "defaultValue";

        public const string JsonPropertyTypeMaxLength = "maxLength";
        public const string JsonPropertyTypeMinValue = "minValue";
        public const string JsonPropertyTypeMaxValue = "maxValue";
        public const string JsonPropertyTypePrecision = "precision";
        public const string JsonPropertyTypeResolution = "resolution";

        public const string JsonPropertyTypeChoice = "choice";
        public const string JsonPropertyTypeChoiceDisplayname = "displayName";
        public const string JsonPropertyTypeChoiceValue = "value";
        public const string JsonPropertyTypeChoiceChoice = "choice";

        public static readonly ISet<string> PropertyTypeKeys = new HashSet<string>()
        {
            JsonPropertyTypeId,
            JsonPropertyTypeLocalName,
            JsonPropertyTypeLocalNameSpace,
            JsonPropertyTypeDisplayname,
            JsonPropertyTypeQueryName,
            JsonPropertyTypeDescription,
            JsonPropertyTypePropertyType,
            JsonPropertyTypeCardinality,
            JsonPropertyTypeUpdatability,
            JsonPropertyTypeInhertited,
            JsonPropertyTypeRequired,
            JsonPropertyTypeQueryable,
            JsonPropertyTypeOrderable,
            JsonPropertyTypeOpenChoice,
            JsonPropertyTypeDefaultValue,
            JsonPropertyTypeMaxLength,
            JsonPropertyTypeMinValue,
            JsonPropertyTypeMaxValue,
            JsonPropertyTypePrecision,
            JsonPropertyTypeResolution,
            JsonPropertyTypeChoice
        };

        public const string JsonTypeTypeMutablilityCreate = "create";
        public const string JsonTypeTypeMutablilityUpdate = "update";
        public const string JsonTypeTypeMutablilityDelete = "delete";

        public static readonly ISet<string> JsonTypeTypeMutablilityKeys = new HashSet<string>()
        {
            JsonTypeTypeMutablilityCreate,
            JsonTypeTypeMutablilityUpdate,
            JsonTypeTypeMutablilityDelete
        };

        public const string JsonTypeListTypes = "types";
        public const string JsonTypeListHasMoreItems = "hasMoreItems";
        public const string JsonTypeListNumItems = "numItems";

        public static readonly ISet<string> TypeListKeys = new HashSet<string>()
        {
            JsonTypeListTypes,
            JsonTypeListHasMoreItems,
            JsonTypeListNumItems
        };

        public const string JsonTypesContainerType = "type";
        public const string JsonTypesContainerChildren = "children";

        public static readonly ISet<string> TypesContainerKeys = new HashSet<string>()
        {
            JsonTypesContainerType,
            JsonTypesContainerChildren
        };

        public const string JsonFailedToDeleteId = "ids";

        public static readonly ISet<string> FailedToDeleteKeys = new HashSet<string>()
        {
            JsonFailedToDeleteId
        };

        public const string JsonBulkUpdateId = "id";
        public const string JsonBulkUpdateNewId = "newId";
        public const string JsonBulkUpdateChangeToken = "changeToken";

        public static readonly ISet<string> BulkUpdateKeys = new HashSet<string>()
        {
            JsonBulkUpdateId,
            JsonBulkUpdateNewId,
            JsonBulkUpdateChangeToken
        };
    }
}
