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

namespace PortCMIS
{
    public static class PropertyIds
    {
        // ---- base ----
        public const string Name = "cmis:name";
        public const string ObjectId = "cmis:objectId";
        public const string ObjectTypeId = "cmis:objectTypeId";
        public const string BaseTypeId = "cmis:baseTypeId";
        public const string CreatedBy = "cmis:createdBy";
        public const string CreationDate = "cmis:creationDate";
        public const string LastModifiedBy = "cmis:lastModifiedBy";
        public const string LastModificationDate = "cmis:lastModificationDate";
        public const string ChangeToken = "cmis:changeToken";
        public const string Description = "cmis:description";
        public const string SecondaryObjectTypeIds = "cmis:secondaryObjectTypeIds";

        // ---- document ----
        public const string IsImmutable = "cmis:isImmutable";
        public const string IsLatestVersion = "cmis:isLatestVersion";
        public const string IsMajorVersion = "cmis:isMajorVersion";
        public const string IsLatestMajorVersion = "cmis:isLatestMajorVersion";
        public const string VersionLabel = "cmis:versionLabel";
        public const string VersionSeriesId = "cmis:versionSeriesId";
        public const string IsVersionSeriesCheckedOut = "cmis:isVersionSeriesCheckedOut";
        public const string VersionSeriesCheckedOutBy = "cmis:versionSeriesCheckedOutBy";
        public const string VersionSeriesCheckedOutId = "cmis:versionSeriesCheckedOutId";
        public const string CheckinComment = "cmis:checkinComment";
        public const string IsPrivateWorkingCopy = "cmis:isPrivateWorkingCopy";
        public const string ContentStreamLength = "cmis:contentStreamLength";
        public const string ContentStreamMimeType = "cmis:contentStreamMimeType";
        public const string ContentStreamFileName = "cmis:contentStreamFileName";
        public const string ContentStreamId = "cmis:contentStreamId";

        // ---- folder ----
        public const string ParentId = "cmis:parentId";
        public const string AllowedChildObjectTypeIds = "cmis:allowedChildObjectTypeIds";
        public const string Path = "cmis:path";

        // ---- relationship ----
        public const string SourceId = "cmis:sourceId";
        public const string TargetId = "cmis:targetId";

        // ---- policy ----
        public const string PolicyText = "cmis:policyText";

        // ---- retention ----
        public const string ExpirationDate = "cmis:rm_expirationDate";
        public const string StartOfRetention = "cmis:rm_startOfRetention";
        public const string DestructionDate = "cmis:rm_destructionDate";
        public const string HoldIds = "cmis:rm_holdIds";

        // ---- extensions ----
        public const string ContentStreamHash = "cmis:contentStreamHash";
    }
}
