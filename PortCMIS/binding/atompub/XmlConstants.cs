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

namespace PortCMIS.Binding.AtomPub
{
    class XmlConstants
    {
        // namespaces
        public const string NAMESPACE_CMIS = "http://docs.oasis-open.org/ns/cmis/core/200908/";
        public const string NAMESPACE_ATOM = "http://www.w3.org/2005/Atom";
        public const string NAMESPACE_APP = "http://www.w3.org/2007/app";
        public const string NAMESPACE_RESTATOM = "http://docs.oasis-open.org/ns/cmis/restatom/200908/";
        public const string NAMESPACE_XSI = "http://www.w3.org/2001/XMLSchema-instance";
        public const string NAMESPACE_APACHE_CHEMISTRY = "http://chemistry.apache.org/";

        // prefixes
        public const string PREFIX_XSI = "xsi";
        public const string PREFIX_ATOM = "atom";
        public const string PREFIX_APP = "app";
        public const string PREFIX_CMIS = "cmis";
        public const string PREFIX_RESTATOM = "cmisra";
        public const string PREFIX_APACHE_CHEMISTY = "chemistry";

        // tags
        public const string TAG_REPOSITORY_INFO = "repositoryInfo";

        public const string TAG_REPINFO_ID = "repositoryId";
        public const string TAG_REPINFO_NAME = "repositoryName";
        public const string TAG_REPINFO_DESCRIPTION = "repositoryDescription";
        public const string TAG_REPINFO_VENDOR = "vendorName";
        public const string TAG_REPINFO_PRODUCT = "productName";
        public const string TAG_REPINFO_PRODUCT_VERSION = "productVersion";
        public const string TAG_REPINFO_ROOT_FOLDER_ID = "rootFolderId";
        public const string TAG_REPINFO_CAPABILITIES = "capabilities";
        public const string TAG_REPINFO_ACL_CAPABILITIES = "aclCapability";
        public const string TAG_REPINFO_CHANGE_LOG_TOKEN = "latestChangeLogToken";
        public const string TAG_REPINFO_CMIS_VERSION_SUPPORTED = "cmisVersionSupported";
        public const string TAG_REPINFO_THIN_CLIENT_URI = "thinClientURI";
        public const string TAG_REPINFO_CHANGES_INCOMPLETE = "changesIncomplete";
        public const string TAG_REPINFO_CHANGES_ON_TYPE = "changesOnType";
        public const string TAG_REPINFO_PRINCIPAL_ID_ANONYMOUS = "principalAnonymous";
        public const string TAG_REPINFO_PRINCIPAL_ID_ANYONE = "principalAnyone";
        public const string TAG_REPINFO_EXTENDED_FEATURES = "extendedFeatures";

        public const string TAG_CAP_CONTENT_STREAM_UPDATABILITY = "capabilityContentStreamUpdatability";
        public const string TAG_CAP_CHANGES = "capabilityChanges";
        public const string TAG_CAP_RENDITIONS = "capabilityRenditions";
        public const string TAG_CAP_GET_DESCENDANTS = "capabilityGetDescendants";
        public const string TAG_CAP_GET_FOLDER_TREE = "capabilityGetFolderTree";
        public const string TAG_CAP_MULTIFILING = "capabilityMultifiling";
        public const string TAG_CAP_UNFILING = "capabilityUnfiling";
        public const string TAG_CAP_VERSION_SPECIFIC_FILING = "capabilityVersionSpecificFiling";
        public const string TAG_CAP_PWC_SEARCHABLE = "capabilityPWCSearchable";
        public const string TAG_CAP_PWC_UPDATABLE = "capabilityPWCUpdatable";
        public const string TAG_CAP_ALL_VERSIONS_SEARCHABLE = "capabilityAllVersionsSearchable";
        public const string TAG_CAP_ORDER_BY = "capabilityOrderBy";
        public const string TAG_CAP_QUERY = "capabilityQuery";
        public const string TAG_CAP_JOIN = "capabilityJoin";
        public const string TAG_CAP_ACL = "capabilityACL";
        public const string TAG_CAP_CREATABLE_PROPERTY_TYPES = "capabilityCreatablePropertyTypes";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES = "capabilityNewTypeSettableAttributes";

        public const string TAG_CAP_CREATABLE_PROPERTY_TYPES_CANCREATE = "canCreate";

        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_ID = "id";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_LOCALNAME = "localName";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_LOCALNAMESPACE = "localNamespace";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_DISPLAYNAME = "displayName";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_QUERYNAME = "queryName";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_DESCRIPTION = "description";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CREATEABLE = "creatable";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_FILEABLE = "fileable";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_QUERYABLE = "queryable";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_FULLTEXTINDEXED = "fulltextIndexed";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_INCLUDEDINSUPERTYTPEQUERY = "includedInSupertypeQuery";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CONTROLABLEPOLICY = "controllablePolicy";
        public const string TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CONTROLABLEACL = "controllableACL";

        public const string TAG_ACLCAP_SUPPORTED_PERMISSIONS = "supportedPermissions";
        public const string TAG_ACLCAP_ACL_PROPAGATION = "propagation";
        public const string TAG_ACLCAP_PERMISSIONS = "permissions";
        public const string TAG_ACLCAP_PERMISSION_MAPPING = "mapping";

        public const string TAG_ACLCAP_PERMISSION_PERMISSION = "permission";
        public const string TAG_ACLCAP_PERMISSION_DESCRIPTION = "description";

        public const string TAG_ACLCAP_MAPPING_KEY = "key";
        public const string TAG_ACLCAP_MAPPING_PERMISSION = "permission";

        public const string TAG_FEATURE_ID = "id";
        public const string TAG_FEATURE_URL = "url";
        public const string TAG_FEATURE_COMMON_NAME = "commonName";
        public const string TAG_FEATURE_VERSION_LABEL = "versionLabel";
        public const string TAG_FEATURE_DESCRIPTION = "description";
        public const string TAG_FEATURE_DATA = "featureData";

        public const string TAG_FEATURE_DATA_KEY = "key";
        public const string TAG_FEATURE_DATA_VALUE = "value";

        public const string TAG_OBJECT = "object";

        public const string TAG_OBJECT_PROPERTIES = "properties";
        public const string TAG_OBJECT_ALLOWABLE_ACTIONS = "allowableActions";
        public const string TAG_OBJECT_RELATIONSHIP = "relationship";
        public const string TAG_OBJECT_CHANGE_EVENT_INFO = "changeEventInfo";
        public const string TAG_OBJECT_ACL = "acl";
        public const string TAG_OBJECT_EXACT_ACL = "exactACL";
        public const string TAG_OBJECT_POLICY_IDS = "policyIds";
        public const string TAG_OBJECT_RENDITION = "rendition";

        public const string TAG_PROP_BOOLEAN = "propertyBoolean";
        public const string TAG_PROP_ID = "propertyId";
        public const string TAG_PROP_INTEGER = "propertyInteger";
        public const string TAG_PROP_DATETIME = "propertyDateTime";
        public const string TAG_PROP_DECIMAL = "propertyDecimal";
        public const string TAG_PROP_HTML = "propertyHtml";
        public const string TAG_PROP_STRING = "propertyString";
        public const string TAG_PROP_URI = "propertyUri";

        public const string TAG_CHANGE_EVENT_TYPE = "changeType";
        public const string TAG_CHANGE_EVENT_TIME = "changeTime";

        public const string TAG_ACL_PERMISSISONS = "permission";
        public const string TAG_ACL_IS_EXACT = "permission";
        public const string TAG_ACE_PRINCIPAL = "principal";
        public const string TAG_ACE_PRINCIPAL_ID = "principalId";
        public const string TAG_ACE_PERMISSIONS = "permission";
        public const string TAG_ACE_IS_DIRECT = "direct";

        public const string TAG_POLICY_ID = "id";

        public const string TAG_RENDITION_STREAM_ID = "streamId";
        public const string TAG_RENDITION_MIMETYPE = "mimetype";
        public const string TAG_RENDITION_LENGTH = "length";
        public const string TAG_RENDITION_KIND = "kind";
        public const string TAG_RENDITION_TITLE = "title";
        public const string TAG_RENDITION_HEIGHT = "height";
        public const string TAG_RENDITION_WIDTH = "width";
        public const string TAG_RENDITION_DOCUMENT_ID = "renditionDocumentId";

        public const string ATTR_PROPERTY_ID = "propertyDefinitionId";
        public const string ATTR_PROPERTY_LOCALNAME = "localName";
        public const string ATTR_PROPERTY_DISPLAYNAME = "displayName";
        public const string ATTR_PROPERTY_QUERYNAME = "queryName";
        public const string TAG_PROPERTY_VALUE = "value";

        public const string TAG_TYPE = "type";

        public const string ATTR_DOCUMENT_TYPE = "cmisTypeDocumentDefinitionType";
        public const string ATTR_FOLDER_TYPE = "cmisTypeFolderDefinitionType";
        public const string ATTR_RELATIONSHIP_TYPE = "cmisTypeRelationshipDefinitionType";
        public const string ATTR_POLICY_TYPE = "cmisTypePolicyDefinitionType";
        public const string ATTR_ITEM_TYPE = "cmisTypeItemDefinitionType";
        public const string ATTR_SECONDARY_TYPE = "cmisTypeSecondaryDefinitionType";

        public const string TAG_TYPE_ID = "id";
        public const string TAG_TYPE_LOCALNAME = "localName";
        public const string TAG_TYPE_LOCALNAMESPACE = "localNamespace";
        public const string TAG_TYPE_DISPLAYNAME = "displayName";
        public const string TAG_TYPE_QUERYNAME = "queryName";
        public const string TAG_TYPE_DESCRIPTION = "description";
        public const string TAG_TYPE_BASE_ID = "baseId";
        public const string TAG_TYPE_PARENT_ID = "parentId";
        public const string TAG_TYPE_CREATABLE = "creatable";
        public const string TAG_TYPE_FILEABLE = "fileable";
        public const string TAG_TYPE_QUERYABLE = "queryable";
        public const string TAG_TYPE_FULLTEXT_INDEXED = "fulltextIndexed";
        public const string TAG_TYPE_INCLUDE_IN_SUPERTYPE_QUERY = "includedInSupertypeQuery";
        public const string TAG_TYPE_CONTROLABLE_POLICY = "controllablePolicy";
        public const string TAG_TYPE_CONTROLABLE_ACL = "controllableACL";
        public const string TAG_TYPE_TYPE_MUTABILITY = "typeMutability";
        public const string TAG_TYPE_VERSIONABLE = "versionable"; // document
        public const string TAG_TYPE_CONTENTSTREAM_ALLOWED = "contentStreamAllowed"; // document
        public const string TAG_TYPE_ALLOWED_SOURCE_TYPES = "allowedSourceTypes"; // relationship
        public const string TAG_TYPE_ALLOWED_TARGET_TYPES = "allowedTargetTypes"; // relationship

        public const string TAG_TYPE_PROP_DEF_BOOLEAN = "propertyBooleanDefinition";
        public const string TAG_TYPE_PROP_DEF_DATETIME = "propertyDateTimeDefinition";
        public const string TAG_TYPE_PROP_DEF_DECIMAL = "propertyDecimalDefinition";
        public const string TAG_TYPE_PROP_DEF_ID = "propertyIdDefinition";
        public const string TAG_TYPE_PROP_DEF_INTEGER = "propertyIntegerDefinition";
        public const string TAG_TYPE_PROP_DEF_HTML = "propertyHtmlDefinition";
        public const string TAG_TYPE_PROP_DEF_STRING = "propertyStringDefinition";
        public const string TAG_TYPE_PROP_DEF_URI = "propertyUriDefinition";

        public const string TAG_PROPERTY_TYPE_ID = "id";
        public const string TAG_PROPERTY_TYPE_LOCALNAME = "localName";
        public const string TAG_PROPERTY_TYPE_LOCALNAMESPACE = "localNamespace";
        public const string TAG_PROPERTY_TYPE_DISPLAYNAME = "displayName";
        public const string TAG_PROPERTY_TYPE_QUERYNAME = "queryName";
        public const string TAG_PROPERTY_TYPE_DESCRIPTION = "description";
        public const string TAG_PROPERTY_TYPE_PROPERTY_TYPE = "propertyType";
        public const string TAG_PROPERTY_TYPE_CARDINALITY = "cardinality";
        public const string TAG_PROPERTY_TYPE_UPDATABILITY = "updatability";
        public const string TAG_PROPERTY_TYPE_INHERITED = "inherited";
        public const string TAG_PROPERTY_TYPE_REQUIRED = "required";
        public const string TAG_PROPERTY_TYPE_QUERYABLE = "queryable";
        public const string TAG_PROPERTY_TYPE_ORDERABLE = "orderable";
        public const string TAG_PROPERTY_TYPE_OPENCHOICE = "openChoice";

        public const string TAG_PROPERTY_TYPE_DEAULT_VALUE = "defaultValue";

        public const string TAG_PROPERTY_TYPE_MAX_LENGTH = "maxLength";
        public const string TAG_PROPERTY_TYPE_MIN_VALUE = "minValue";
        public const string TAG_PROPERTY_TYPE_MAX_VALUE = "maxValue";
        public const string TAG_PROPERTY_TYPE_PRECISION = "precision";
        public const string TAG_PROPERTY_TYPE_RESOLUTION = "resolution";

        public const string TAG_PROPERTY_TYPE_CHOICE = "choice";
        public const string ATTR_PROPERTY_TYPE_CHOICE_DISPLAYNAME = "displayName";
        public const string TAG_PROPERTY_TYPE_CHOICE_VALUE = "value";
        public const string TAG_PROPERTY_TYPE_CHOICE_CHOICE = "choice";

        public const string TAG_TYPE_TYPE_MUTABILITY_CREATE = "create";
        public const string TAG_TYPE_TYPE_MUTABILITY_UPDATE = "update";
        public const string TAG_TYPE_TYPE_MUTABILITY_DELETE = "delete";

        public const string TAG_QUERY = "query";
        public const string TAG_QUERY_STATEMENT = "statement";
        public const string TAG_QUERY_SEARCHALLVERSIONS = "searchAllVersions";
        public const string TAG_QUERY_INCLUDEALLOWABLEACTIONS = "includeAllowableActions";
        public const string TAG_QUERY_INCLUDERELATIONSHIPS = "includeRelationships";
        public const string TAG_QUERY_RENDITIONFILTER = "renditionFilter";
        public const string TAG_QUERY_MAXITEMS = "maxItems";
        public const string TAG_QUERY_SKIPCOUNT = "skipCount";

        public const string TAG_BULK_UPDATE = "bulkUpdate";
        public const string TAG_BULK_UPDATE_ID_AND_TOKEN = "objectIdAndChangeToken";
        public const string TAG_BULK_UPDATE_PROPERTIES = "properties";
        public const string TAG_BULK_UPDATE_ADD_SECONDARY_TYPES = "addSecondaryTypeIds";
        public const string TAG_BULK_UPDATE_REMOVE_SECONDARY_TYPES = "removeSecondaryTypeIds";

        public const string TAG_IDANDTOKEN_ID = "id";
        public const string TAG_IDANDTOKEN_NEWID = "newId";
        public const string TAG_IDANDTOKEN_CHANGETOKEN = "changeToken";
    }
}
