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

namespace PortCMIS.Binding
{
    internal class BindingConstants
    {
        // media types
        public const string MediaTypeService = "application/atomsvc+xml";
        public const string MediaTypeFeed = "application/atom+xml;type=feed";
        public const string MediaTypeEntry = "application/atom+xml;type=entry";
        public const string MediaTypeChildren = MediaTypeFeed;
        public const string MediaTypeDecendants = "application/cmistree+xml";
        public const string MediaTypeQuery = "application/cmisquery+xml";
        public const string MediaTypeAllowableAction = "application/cmisallowableactions+xml";
        public const string MediaTypeAcl = "application/cmisacl+xml";
        public const string MediaTypeCmisAtom = "application/cmisatom+xml";
        public const string MediaTypeOctetStream = "application/octet-stream";

        // collections
        public const string CollectionRoot = "root";
        public const string CollectionTypes = "types";
        public const string CollectionQuery = "query";
        public const string CollectionCheckedout = "checkedout";
        public const string CollectionUnfiled = "unfiled";
        public const string CollectionBulkUpdate = "update";

        // URI templates
        public const string TemplateObjectById = "objectbyid";
        public const string TemplateObjectByPath = "objectbypath";
        public const string TemplateTypeById = "typebyid";
        public const string TemplateQuery = "query";

        // Link rel
        public const string RelSelf = "self";
        public const string RelEnclosure = "enclosure";
        public const string RelService = "service";
        public const string RelDescribedby = "describedby";
        public const string RelAlternate = "alternate";
        public const string RelDown = "down";
        public const string RelUp = "up";
        public const string RelFirst = "first";
        public const string RelLast = "last";
        public const string RelPrev = "previous";
        public const string RelNext = "next";
        public const string RelVia = "via";
        public const string RelEdit = "edit";
        public const string RelEditMedia = "edit-media";
        public const string RelVersionHistory = "version-history";
        public const string RelCurrentVersion = "current-version";
        public const string RelWorkingCopy = "working-copy";
        public const string RelFolderTree = "http://docs.oasis-open.org/ns/cmis/link/200908/foldertree";
        public const string RelAllowableActions = "http://docs.oasis-open.org/ns/cmis/link/200908/allowableactions";
        public const string RelAcl = "http://docs.oasis-open.org/ns/cmis/link/200908/acl";
        public const string RelSource = "http://docs.oasis-open.org/ns/cmis/link/200908/source";
        public const string RelTarget = "http://docs.oasis-open.org/ns/cmis/link/200908/target";

        public const string RelRelationships = "http://docs.oasis-open.org/ns/cmis/link/200908/relationships";
        public const string RelPolicies = "http://docs.oasis-open.org/ns/cmis/link/200908/policies";

        public const string RepRelTypeDesc = "http://docs.oasis-open.org/ns/cmis/link/200908/typedescendants";
        public const string RepRelFolderTree = "http://docs.oasis-open.org/ns/cmis/link/200908/foldertree";
        public const string RepRelRootDesc = "http://docs.oasis-open.org/ns/cmis/link/200908/rootdescendants";
        public const string RepRelChanges = "http://docs.oasis-open.org/ns/cmis/link/200908/changes";

        // browser binding selectors
        public const string SelectorLastResult = "lastResult";
        public const string SelectorRepositoryInfo = "repositoryInfo";
        public const string SelectorTypeChildren = "typeChildren";
        public const string SelectorTypeDecendants = "typeDescendants";
        public const string SelectorTypeDefinition = "typeDefinition";
        public const string SelectorContent = "content";
        public const string SelectorObject = "object";
        public const string SelectorProperties = "properties";
        public const string SelectorAllowableActionS = "allowableActions";
        public const string SelectorRenditions = "renditions";
        public const string SelectorChildren = "children";
        public const string SelectorDecendants = "descendants";
        public const string SelectorParents = "parents";
        public const string SelectorParent = "parent";
        public const string SelectorFolderTree = "folderTree";
        public const string SelectorQuery = "query";
        public const string SelectorVersions = "versions";
        public const string SelectorRelationships = "relationships";
        public const string SelectorCheckedout = "checkedout";
        public const string SelectorPolicies = "policies";
        public const string SelectorAcl = "acl";
        public const string SelectorContentChanges = "contentChanges";

        // browser binding actions
        public const string CmisActionCreateType = "createType";
        public const string CmisActionUpdateType = "updateType";
        public const string CmisActionDeleteType = "deleteType";
        public const string CmisActionCreateDocument = "createDocument";
        public const string CmisActionCreateDocumentFromSource = "createDocumentFromSource";
        public const string CmisActionCreateFolder = "createFolder";
        public const string CmisActionCreateRelationship = "createRelationship";
        public const string CmisActionCreatePolicy = "createPolicy";
        public const string CmisActionCreateItem = "createItem";
        public const string CmisActionUpdateProperties = "update";
        public const string CmisActionBulkUpdate = "bulkUpdate";
        public const string CmisActionDeleteContent = "deleteContent";
        public const string CmisActionSetContent = "setContent";
        public const string CmisActionAppendContent = "appendContent";
        public const string CmisActionDelete = "delete";
        public const string CmisActionDeleteTree = "deleteTree";
        public const string CmisActionMove = "move";
        public const string CmisActionRemoveObjectToFolder = "addObjectToFolder";
        public const string CmisActionRemoveObjectFromFolder = "removeObjectFromFolder";
        public const string CmisActionQuery = "query";
        public const string CmisActionCheckOut = "checkOut";
        public const string CmisActionCancelCheckOut = "cancelCheckOut";
        public const string CmisActionCheckIn = "checkIn";
        public const string CmisActionApplyPolicy = "applyPolicy";
        public const string CmisActionRemovePolicy = "removePolicy";
        public const string CmisActionApplyAcl = "applyACL";

        // browser binding control
        public const string ControlCmisAction = "cmisaction";
        public const string ControlSuccinct = "succinct";
        public const string ControlToken = "token";
        public const string ControlObjectId = "objectId";
        public const string ControlPropId = "propertyId";
        public const string ControlPropValue = "propertyValue";
        public const string ControlPolicy = "policy";
        public const string ControlPolicyId = "policyId";
        public const string ControlAddAcePrincipal = "addACEPrincipal";
        public const string ControlAddAcePermission = "addACEPermission";
        public const string ControlRemoveAcePrincipal = "removeACEPrincipal";
        public const string ControlRemoveAcePermission = "removeACEPermission";
        public const string ControlContentType = "contenttype";
        public const string ControlFilename = "filename";
        public const string ControlIsLastChunk = "isLastChunk";
        public const string ControlType = "type";
        public const string ControlTypeId = "typeId";
        public const string ControlChangeToken = "changeToken";
        public const string ControlAddSecondaryType = "addSecondaryTypeId";
        public const string ControlRemoveSecondaryType = "removeSecondaryTypeId";

        // parameter
        public const string ParamAcl = "includeACL";
        public const string ParamAllowableActions = "includeAllowableActions";
        public const string ParamAllVersions = "allVersions";
        public const string ParamAppend = "append";
        public const string ParamChangeLogToken = "changeLogToken";
        public const string ParamChangeToken = "changeToken";
        public const string ParamCheckinComment = "checkinComment";
        public const string ParamCheckIn = "checkin";
        public const string ParamchildTypes = "childTypes";
        public const string ParamContinueOnFailure = "continueOnFailure";
        public const string ParamDepth = "depth";
        public const string Paramdownload = "download";
        public const string Paramfilter = "filter";
        public const string ParamSuccinct = "succinct";
        public const string ParamDateTimeFormat = "dateTimeFormat";
        public const string ParamFolderId = "folderId";
        public const string ParamId = "id";
        public const string ParamIsLastChunk = "isLastChunk";
        public const string ParamMajor = "major";
        public const string ParamMaxItems = "maxItems";
        public const string ParamObjectId = "objectId";
        public const string ParamOnlyBasicPermissions = "onlyBasicPermissions";
        public const string ParamOrderBy = "orderBy";
        public const string ParamOverwriteFlag = "overwriteFlag";
        public const string ParamPath = "path";
        public const string ParamPathSegment = "includePathSegment";
        public const string ParamPolicyId = "policyId";
        public const string ParamPolicyIds = "includePolicyIds";
        public const string ParamProperties = "includeProperties";
        public const string ParamPropertyDefinitions = "includePropertyDefinitions";
        public const string ParamRelationships = "includeRelationships";
        public const string ParamRelationshipDirection = "relationshipDirection";
        public const string ParamRelativePathSegment = "includeRelativePathSegment";
        public const string ParamRemoveFrom = "removeFrom";
        public const string ParamRenditionfilter = "renditionFilter";
        public const string ParamRepositoryId = "repositoryId";
        public const string ParamReturnVersion = "returnVersion";
        public const string ParamSkipCount = "skipCount";
        public const string ParamSourceFolderId = "sourceFolderId";
        public const string ParamTargetFolderId = "targetFolderId";
        public const string ParamStreamId = "streamId";
        public const string ParamSubRelationshipTypes = "includeSubRelationshipTypes";
        public const string ParamTypeId = "typeId";
        public const string ParamUnfileObjects = "unfileObjects";
        public const string ParamVersionSeriesId = "versionSeries";
        public const string ParamVersioningState = "versioningState";
        public const string ParamQ = "q";
        public const string ParamStatement = "statement";
        public const string ParamSearchAllVersions = "searchAllVersions";
        public const string ParamAclPropagation = "ACLPropagation";
        public const string ParamSourceId = "sourceId";

        public const string ParamSelector = "cmisselector";
        public const string ParamCallback = "callback";
        public const string ParamSuppressResponseCodes = "suppressResponseCodes";
        public const string ParamToken = "token";

        // rendition filter
        public const string RenditionNone = "cmis:none";
    }
}
