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
using System.Reflection;

namespace PortCMIS.Enums
{
    /// <summary>
    /// Base Type ID.
    /// </summary>
    public enum BaseTypeId
    {
        /// <summary>
        /// CMIS Document Type.
        /// </summary>
        [CmisValue("cmis:document")]
        CmisDocument,
        /// <summary>
        /// CMIS Folder Type.
        /// </summary>
        [CmisValue("cmis:folder")]
        CmisFolder,
        /// <summary>
        /// CMIS Relationship Type.
        /// </summary>
        [CmisValue("cmis:relationship")]
        CmisRelationship,
        /// <summary>
        /// CMIS Policy Type.
        /// </summary>
        [CmisValue("cmis:policy")]
        CmisPolicy,
        /// <summary>
        /// CMIS Item Type.
        /// </summary>
        [CmisValue("cmis:item")]
        CmisItem,
        /// <summary>
        /// CMIS Secondary Type.
        /// </summary>
        [CmisValue("cmis:secondary")]
        CmisSecondary
    }

    /// <summary>
    /// Repository Capability: Content Stream Updates
    /// </summary>
    public enum CapabilityContentStreamUpdates
    {
        [CmisValue("anytime")]
        Anyime,
        [CmisValue("pwconly")]
        PWCOnly,
        [CmisValue("none")]
        None
    }

    /// <summary>
    /// Repository Capability: Content Changes.
    /// </summary>
    public enum CapabilityChanges
    {
        [CmisValue("none")]
        None,
        [CmisValue("objectidsonly")]
        ObjectIdsOnly,
        [CmisValue("properties")]
        Properties,
        [CmisValue("all")]
        All
    }

    /// <summary>
    /// Repository Capability: Renditions.
    /// </summary>
    public enum CapabilityRenditions
    {
        [CmisValue("none")]
        None,
        [CmisValue("read")]
        Read
    }

    /// <summary>
    /// Repository Capability: Query.
    /// </summary>
    public enum CapabilityQuery
    {
        [CmisValue("none")]
        None,
        [CmisValue("metadataonly")]
        MetadataOnly,
        [CmisValue("fulltextonly")]
        FulltextOnly,
        [CmisValue("bothseparate")]
        BothSeparate,
        [CmisValue("bothcombined")]
        BothCombined
    }

    /// <summary>
    /// Repository Capability: Query Joins.
    /// </summary>
    public enum CapabilityJoin
    {
        [CmisValue("none")]
        None,
        [CmisValue("inneronly")]
        InnerOnly,
        [CmisValue("innerandouter")]
        InnerAndOuter
    }

    /// <summary>
    /// Repository Capability: ACL.
    /// </summary>
    public enum CapabilityAcl
    {
        [CmisValue("none")]
        None,
        [CmisValue("discover")]
        Discover,
        [CmisValue("manage")]
        Manage
    }

    /// <summary>
    /// Repository Capability: Order by.
    /// </summary>
    public enum CapabilityOrderBy
    {
        [CmisValue("none")]
        None,
        [CmisValue("common")]
        Common,
        [CmisValue("custom")]
        Custom
    }

    /// <summary>
    /// Supported Permissions.
    /// </summary>
    public enum SupportedPermissions
    {
        /// <summary>
        /// Basic permissions.
        /// </summary>
        [CmisValue("basic")]
        Basic,
        /// <summary>
        /// Repository specific permissions.
        /// </summary>
        [CmisValue("repository")]
        Repository,
        /// <summary>
        /// Basic and repository specific permissions.
        /// </summary>
        [CmisValue("both")]
        Both
    }

    /// <summary>
    /// ACL Propagation flag.
    /// </summary>
    public enum AclPropagation
    {
        [CmisValue("repositorydetermined")]
        RepositoryDetermined,
        [CmisValue("objectonly")]
        ObjectOnly,
        [CmisValue("propagate")]
        Propagate
    }

    /// <summary>
    /// Content Stream Allowed flag.
    /// </summary>
    public enum ContentStreamAllowed
    {
        [CmisValue("notallowed")]
        NotAllowed,
        [CmisValue("allowed")]
        Allowed,
        [CmisValue("required")]
        Required
    }

    /// <summary>
    /// Property Data Type.
    /// </summary>
    public enum PropertyType
    {
        [CmisValue("boolean")]
        Boolean,
        [CmisValue("id")]
        Id,
        [CmisValue("integer")]
        Integer,
        [CmisValue("datetime")]
        DateTime,
        [CmisValue("decimal")]
        Decimal,
        [CmisValue("html")]
        Html,
        [CmisValue("string")]
        String,
        [CmisValue("uri")]
        Uri
    }

    /// <summary>
    /// Property Cardinality.
    /// </summary>
    public enum Cardinality
    {
        /// <summary>
        /// Single value property.
        /// </summary>
        [CmisValue("single")]
        Single,
        /// <summary>
        /// Multivalue property.
        /// </summary>
        [CmisValue("multi")]
        Multi
    }

    /// <summary>
    /// Property Updatability.
    /// </summary>
    public enum Updatability
    {
        /// <summary>
        /// Read-only property set by repository.
        /// </summary>
        [CmisValue("readonly")]
        ReadOnly,
        /// <summary>
        /// Read-write property.
        /// </summary>
        [CmisValue("readwrite")]
        ReadWrite,
        /// <summary>
        /// Property that can only be set if the document is checked out.
        /// </summary>
        [CmisValue("whencheckedout")]
        WhenCheckedOut,
        /// <summary>
        /// Property that can only be set at object creation.
        /// </summary>
        [CmisValue("oncreate")]
        OnCreate
    }

    /// <summary>
    /// DateTime Property Resolution.
    /// </summary>
    public enum DateTimeResolution
    {
        [CmisValue("year")]
        Year,
        [CmisValue("date")]
        Date,
        [CmisValue("time")]
        Time
    }

    /// <summary>
    /// Decimal Property Precision.
    /// </summary>
    public enum DecimalPrecision
    {
        [CmisValue("32")]
        Bits32,
        [CmisValue("64")]
        Bits64
    }

    /// <summary>
    /// Include Relationships flag.
    /// </summary>
    public enum IncludeRelationships
    {
        [CmisValue("none")]
        None,
        [CmisValue("source")]
        Source,
        [CmisValue("target")]
        Target,
        [CmisValue("both")]
        Both
    }

    /// <summary>
    /// Versioning State flag.
    /// </summary>
    public enum VersioningState
    {
        [CmisValue("none")]
        None,
        [CmisValue("major")]
        Major,
        [CmisValue("minor")]
        Minor,
        [CmisValue("checkedout")]
        CheckedOut
    }

    /// <summary>
    /// Unfile Object flag.
    /// </summary>
    public enum UnfileObject
    {
        [CmisValue("unfile")]
        Unfile,
        [CmisValue("deletesinglefiled")]
        DeleteSinglefiled,
        [CmisValue("delete")]
        Delete
    }

    /// <summary>
    /// Relationship Direction flag.
    /// </summary>
    public enum RelationshipDirection
    {
        [CmisValue("source")]
        Source,
        [CmisValue("target")]
        Target,
        [CmisValue("either")]
        Either
    }

    /// <summary>
    /// Return Version flag.
    /// </summary>
    public enum ReturnVersion
    {
        /// <summary>
        /// Return this version.
        /// </summary>
        [CmisValue("this")]
        This,
        /// <summary>
        /// Return latest version.
        /// </summary>
        [CmisValue("latest")]
        Latest,
        /// <summary>
        /// Return latest major version.
        /// </summary>
        [CmisValue("latestmajor")]
        LatestMajor
    }

    /// <summary>
    /// Change Type in change logs.
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// Object has been created.
        /// </summary>
        [CmisValue("created")]
        Created,
        /// <summary>
        /// Object has been updated.
        /// </summary>
        [CmisValue("updated")]
        Updated,
        /// <summary>
        /// Object has been deleted.
        /// </summary>
        [CmisValue("deleted")]
        Deleted,
        /// <summary>
        /// Permissions (ACL, policies) have been changed.
        /// </summary>
        [CmisValue("security")]
        Security
    }

    /// <summary>
    /// Browser Binding DateTime Format.
    /// </summary>
    public enum DateTimeFormat
    {
        /// <summary>
        /// Simple format (milliseconds).
        /// </summary>
        [CmisValue("simple")]
        Simple,
        /// <summary>
        /// Extended format (ISO format).
        /// </summary>
        [CmisValue("extended")]
        Extended
    }

    /// <summary>
    /// Allowable Actions.
    /// </summary>
    public enum Action
    {
        [CmisValue("canDeleteObject")]
        CanDeleteObject,
        [CmisValue("canUpdateProperties")]
        CanUpdateProperties,
        [CmisValue("canGetFolderTree")]
        CanGetFolderTree,
        [CmisValue("canGetProperties")]
        CanGetProperties,
        [CmisValue("canGetObjectRelationships")]
        CanGetObjectRelationships,
        [CmisValue("canGetObjectParents")]
        CanGetObjectParents,
        [CmisValue("canGetFolderParent")]
        CanGetFolderParent,
        [CmisValue("canGetDescendants")]
        CanGetDescendants,
        [CmisValue("canMoveObject")]
        CanMoveObject,
        [CmisValue("canDeleteContentStream")]
        CanDeleteContentStream,
        [CmisValue("canCheckOut")]
        CanCheckOut,
        [CmisValue("canCancelCheckOut")]
        CanCancelCheckOut,
        [CmisValue("canCheckIn")]
        CanCheckIn,
        [CmisValue("canSetContentStream")]
        CanSetContentStream,
        [CmisValue("canGetAllVersions")]
        CanGetAllVersions,
        [CmisValue("canAddObjectToFolder")]
        CanAddObjectToFolder,
        [CmisValue("canRemoveObjectFromFolder")]
        CanRemoveObjectFromFolder,
        [CmisValue("canGetContentStream")]
        CanGetContentStream,
        [CmisValue("canApplyPolicy")]
        CanApplyPolicy,
        [CmisValue("canGetAppliedPolicies")]
        CanGetAppliedPolicies,
        [CmisValue("canRemovePolicy")]
        CanRemovePolicy,
        [CmisValue("canGetChildren")]
        CanGetChildren,
        [CmisValue("canCreateDocument")]
        CanCreateDocument,
        [CmisValue("canCreateFolder")]
        CanCreateFolder,
        [CmisValue("canCreateRelationship")]
        CanCreateRelationship,
        [CmisValue("canCreateItem")]
        CanCreateItem,
        [CmisValue("canDeleteTree")]
        CanDeleteTree,
        [CmisValue("canGetRenditions")]
        CanGetRenditions,
        [CmisValue("canGetAcl")]
        CanGetAcl,
        [CmisValue("canApplyAcl")]
        CanApplyAcl
    }

    /// <summary>
    /// CMIS Version.
    /// </summary>
    public enum CmisVersion
    {
        /// <summary>
        /// CMIS 1.0
        /// </summary>
        [CmisValue("1.0")]
        Cmis_1_0,
        /// <summary>
        /// CMIS 1.1
        /// </summary>
        [CmisValue("1.1")]
        Cmis_1_1
    }

    // --- attribute class ---

    [AttributeUsage(AttributeTargets.Field)]
    public class CmisValueAttribute : System.Attribute
    {
        public CmisValueAttribute(string value)
        {
            Value = value;
        }
        public string Value
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Handles translations from Enums into CMIS values and vice versa.
    /// </summary>
    public static class CmisValue
    {
        public static string GetCmisValue(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetRuntimeField(value.ToString());
            CmisValueAttribute[] cmisValueAttr = fieldInfo.GetCustomAttributes(typeof(CmisValueAttribute), false) as CmisValueAttribute[];
            return cmisValueAttr.Length > 0 ? cmisValueAttr[0].Value : null;
        }

        public static T GetCmisEnum<T>(this string value)
        {
            Type type = typeof(T);
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            foreach (FieldInfo fieldInfo in type.GetRuntimeFields())
            {
                CmisValueAttribute[] cmisValueAttr = fieldInfo.GetCustomAttributes(typeof(CmisValueAttribute), false) as CmisValueAttribute[];
                if (cmisValueAttr != null && cmisValueAttr.Length > 0 && cmisValueAttr[0].Value == value)
                {
                    return (T)Enum.Parse(type, fieldInfo.Name);
                }
            }
            return default(T);
        }
    }
}
