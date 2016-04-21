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
    /// <summary>
    /// CMIS property ID constants.
    /// </summary>
    public static class PropertyIds
    {
        // ---- base ----

        /// <summary>
        /// CMIS property <c>cmis:name</c>: name of the object.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string Name = "cmis:name";

        /// <summary>
        /// CMIS property <c>cmis:objectId</c>: ID of the object.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string ObjectId = "cmis:objectId";

        /// <summary>
        /// CMIS property <c>cmis:objectTypeId</c>: ID of primary type of the
        /// object.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string ObjectTypeId = "cmis:objectTypeId";

        /// <summary>
        /// CMIS property <c>cmis:baseTypeId</c>: ID of the base type of the object.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string BaseTypeId = "cmis:baseTypeId";

        /// <summary>
        /// CMIS property <c>cmis:createdBy</c>: creator of the object.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string CreatedBy = "cmis:createdBy";

        /// <summary>
        /// CMIS property <c>cmis:creationDate</c>: creation date.
        /// <para>
        /// CMIS data type: datetime<br/>
        /// C# type: DateTime
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string CreationDate = "cmis:creationDate";

        /// <summary>
        /// CMIS property <c>cmis:lastModifiedBy</c>: last modifier of the object.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string LastModifiedBy = "cmis:lastModifiedBy";

        /// <summary>
        /// CMIS property <c>cmis:lastModificationDate</c>: last modification date.
        /// <para>
        /// CMIS data type: datetime<br/>
        /// C# type: DateTime
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string LastModificationDate = "cmis:lastModificationDate";

        /// <summary>
        /// CMIS property <c>cmis:changeToken</c>: change token of the object.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string ChangeToken = "cmis:changeToken";

        /// <summary>
        /// CMIS property <c>cmis:description</c>: description of the object.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.1</cmis>
        public const string Description = "cmis:description";

        /// <summary>
        /// CMIS property <c>cmis:secondaryObjectTypeIds</c> (multivalue): list of
        /// IDs of the secondary types of the object.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.1</cmis>
        public const string SecondaryObjectTypeIds = "cmis:secondaryObjectTypeIds";

        // ---- document ----

        /// <summary>
        /// CMIS document property <c>cmis:isImmutable</c>: flag the indicates if
        /// the document is immutable.
        /// <para>
        /// CMIS data type: boolean<br/>
        /// C# type: bool
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string IsImmutable = "cmis:isImmutable";

        /// <summary>
        /// CMIS document property <c>cmis:isLatestVersion</c>: flag the indicates
        /// if the document is the latest version.
        /// <para>
        /// CMIS data type: boolean<br/>
        /// C# type: bool
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string IsLatestVersion = "cmis:isLatestVersion";

        /// <summary>
        /// CMIS document property <c>cmis:isMajorVersion</c>: flag the indicates if
        /// the document is a major version.
        /// <para>
        /// CMIS data type: boolean<br/>
        /// C# type: bool
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string IsMajorVersion = "cmis:isMajorVersion";

        /// <summary>
        /// CMIS document property <c>cmis:isLatestMajorVersion</c>: flag the
        /// indicates if the document is the latest major version.
        /// <para>
        /// CMIS data type: boolean<br/>
        /// C# type: bool
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string IsLatestMajorVersion = "cmis:isLatestMajorVersion";

        /// <summary>
        /// CMIS document property <c>cmis:versionLabel</c>: version label of the
        /// document.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string VersionLabel = "cmis:versionLabel";

        /// <summary>
        /// CMIS document property <c>cmis:versionSeriesId</c>: ID of the version
        /// series.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string VersionSeriesId = "cmis:versionSeriesId";

        /// <summary>
        /// CMIS document property <c>cmis:isVersionSeriesCheckedOut</c>: flag the
        /// indicates if the document is checked out.
        /// <para>
        /// CMIS data type: boolean<br/>
        /// C# type: bool
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string IsVersionSeriesCheckedOut = "cmis:isVersionSeriesCheckedOut";

        /// <summary>
        /// CMIS document property <c>cmis:versionSeriesCheckedOutBy</c>: user who
        /// checked out the document, if the document is checked out.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string VersionSeriesCheckedOutBy = "cmis:versionSeriesCheckedOutBy";

        /// <summary>
        /// CMIS document property <c>cmis:versionSeriesCheckedOutId</c>: ID of the
        /// PWC, if the document is checked out.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string VersionSeriesCheckedOutId = "cmis:versionSeriesCheckedOutId";

        /// <summary>
        /// CMIS document property <c>cmis:checkinComment</c>: check-in comment for
        /// the document version.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string CheckinComment = "cmis:checkinComment";

        /// <summary>
        /// CMIS document property <c>cmis:isPrivateWorkingCopy</c>: flag the
        /// indicates if the document is a PWC.
        /// <para>
        /// CMIS data type: boolean<br/>
        /// C# type: bool
        /// </para>
        /// </summary>
        /// <cmis>1.1</cmis>
        public const string IsPrivateWorkingCopy = "cmis:isPrivateWorkingCopy";

        /// <summary>
        /// CMIS document property <c>cmis:contentStreamLength</c>: length of the
        /// content stream, if the document has content.
        /// <para>
        /// CMIS data type: integer<br/>
        /// C# type: BigInteger
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string ContentStreamLength = "cmis:contentStreamLength";

        /// <summary>
        /// CMIS document property <c>cmis:contentStreamMimeType</c>: MIME type of
        /// the content stream, if the document has content.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string ContentStreamMimeType = "cmis:contentStreamMimeType";

        /// <summary>
        /// CMIS document property <c>cmis:contentStreamFileName</c>: file name, if
        /// the document has content.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string ContentStreamFileName = "cmis:contentStreamFileName";

        /// <summary>
        /// CMIS document property <c>cmis:contentStreamId</c>: content stream ID.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string ContentStreamId = "cmis:contentStreamId";

        // ---- folder ----

        /// <summary>
        /// CMIS folder property <c>cmis:parentId</c>: ID of the parent folder.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string ParentId = "cmis:parentId";

        /// <summary>
        /// CMIS folder property <c>cmis:allowedChildObjectTypeIds</c> (multivalue):
        /// IDs of the types that can be filed in the folder.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string AllowedChildObjectTypeIds = "cmis:allowedChildObjectTypeIds";

        /// <summary>
        /// CMIS folder property <c>cmis:path</c>: folder path.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string Path = "cmis:path";

        // ---- relationship ----

        /// <summary>
        /// CMIS relationship property <c>cmis:sourceId</c>: ID of the source
        /// object.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string SourceId = "cmis:sourceId";

        /// <summary>
        /// CMIS relationship property <c>cmis:targetId</c>: ID of the target
        /// object.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string TargetId = "cmis:targetId";

        // ---- policy ----

        /// <summary>
        /// CMIS policy property <c>cmis:policyText</c>: policy text.
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>1.0</cmis>
        public const string PolicyText = "cmis:policyText";

        // ---- retention ----

        /// <summary>
        /// CMIS retention property <c>cmis:rm_expirationDate</c>: expiration date.
        /// <para>
        /// CMIS data type: datetime<br/>
        /// C# type: DateTime
        /// </para>
        /// </summary> 
        /// <cmis>1.1</cmis>
        public const string ExpirationDate = "cmis:rm_expirationDate";

        /// <summary>
        /// CMIS retention property <c>cmis:rm_startOfRetention</c>: start date.
        /// <para>
        /// CMIS data type: datetime<br/>
        /// C# type: DateTime
        /// </para>
        /// </summary>
        /// <cmis>1.1</cmis>
        public const string StartOfRetention = "cmis:rm_startOfRetention";

        /// <summary>
        /// CMIS retention property <c>cmis:rm_destructionDate</c>: destruction
        /// date.
        /// <para>
        /// CMIS data type: datetime<br/>
        /// C# type: DateTime
        /// </para>
        /// </summary> 
        /// <cmis>1.1</cmis>
        public const string DestructionDate = "cmis:rm_destructionDate";

        /// <summary>
        /// CMIS retention property <c>cmis:rm_holdIds</c> (multivalue): IDs of the
        /// holds that are applied.
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary> 
        /// <cmis>1.1</cmis>
        public const string HoldIds = "cmis:rm_holdIds";

        // ---- extensions ----

        /// <summary>
        /// Content Hash property <c>cmis:contentStreamHash</c> (multivalue): hashes
        /// of the content stream
        /// <para>
        /// CMIS data type: string<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>Extension</cmis>
        public const string ContentStreamHash = "cmis:contentStreamHash";

        /// <summary>
        /// Latest accessible state property <c>cmis:latestAccessibleStateId</c>: ID
        /// of the latest accessible version of a document
        /// <para>
        /// CMIS data type: id<br/>
        /// C# type: string
        /// </para>
        /// </summary>
        /// <cmis>Extension</cmis>
        public const string LatestAccessibleStateId = "cmis:latestAccessibleStateId";
    }
}
