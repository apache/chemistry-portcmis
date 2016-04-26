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
        /// <summary>
        /// Content can be updated at any time.
        /// </summary>
        [CmisValue("anytime")]
        Anyime,
        /// <summary>
        /// Only the content on PWCs can be updated.
        /// </summary>
        [CmisValue("pwconly")]
        PWCOnly,
        /// <summary>
        /// Content cannot be updated.
        /// </summary>
        [CmisValue("none")]
        None
    }

    /// <summary>
    /// Repository Capability: Content Changes.
    /// </summary>
    public enum CapabilityChanges
    {
        /// <summary>
        /// Change log is not supported.
        /// </summary>
        [CmisValue("none")]
        None,
        /// <summary>
        /// The change log only contains object IDs.
        /// </summary>
        [CmisValue("objectidsonly")]
        ObjectIdsOnly,
        /// <summary>
        /// The change log only contains properties.
        /// </summary>
        [CmisValue("properties")]
        Properties,
        /// <summary>
        /// The change log only contains everything.
        /// </summary>
        [CmisValue("all")]
        All
    }

    /// <summary>
    /// Repository Capability: Renditions.
    /// </summary>
    public enum CapabilityRenditions
    {
        /// <summary>
        /// Renditions are not supported.
        /// </summary>
        [CmisValue("none")]
        None,
        /// <summary>
        /// Renditions can be read.
        /// </summary>
        [CmisValue("read")]
        Read
    }

    /// <summary>
    /// Repository Capability: Query.
    /// </summary>
    public enum CapabilityQuery
    {
        /// <summary>
        /// Query is not supported.
        /// </summary>
        [CmisValue("none")]
        None,
        /// <summary>
        /// Only metadata queries are supported.
        /// </summary>
        [CmisValue("metadataonly")]
        MetadataOnly,
        /// <summary>
        /// Only fulltext queries are supported.
        /// </summary>
        [CmisValue("fulltextonly")]
        FulltextOnly,
        /// <summary>
        /// Metadata and fulltext queries are supported but only separately.
        /// </summary>
        [CmisValue("bothseparate")]
        BothSeparate,
        /// <summary>
        /// Metadata and fulltext queries are supported.
        /// </summary>
        [CmisValue("bothcombined")]
        BothCombined
    }

    /// <summary>
    /// Repository Capability: Query Joins.
    /// </summary>
    public enum CapabilityJoin
    {
        /// <summary>
        /// Joins are not supported.
        /// </summary>
        [CmisValue("none")]
        None,
        /// <summary>
        /// Only inner joins are supported.
        /// </summary>
        [CmisValue("inneronly")]
        InnerOnly,
        /// <summary>
        /// Inner and outer joins are supported.
        /// </summary>
        [CmisValue("innerandouter")]
        InnerAndOuter
    }

    /// <summary>
    /// Repository Capability: ACL.
    /// </summary>
    public enum CapabilityAcl
    {
        /// <summary>
        /// ACLs are not supported.
        /// </summary>
        [CmisValue("none")]
        None,
        /// <summary>
        /// ACLs can be discovered.
        /// </summary>
        [CmisValue("discover")]
        Discover,
        /// <summary>
        /// ACLs can be managed.
        /// </summary>
        [CmisValue("manage")]
        Manage
    }

    /// <summary>
    /// Repository Capability: Order by.
    /// </summary>
    public enum CapabilityOrderBy
    {
        /// <summary>
        /// Order by is not supported.
        /// </summary>
        [CmisValue("none")]
        None,
        /// <summary>
        /// Order by is supported for common properties.
        /// </summary>
        [CmisValue("common")]
        Common,
        /// <summary>
        /// Order by is supported for common and custom properties.
        /// </summary>
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
        /// <summary>
        /// Repository determines the objects to change.
        /// </summary>
        [CmisValue("repositorydetermined")]
        RepositoryDetermined,
        /// <summary>
        /// ACL is only changed for this object.
        /// </summary>
        [CmisValue("objectonly")]
        ObjectOnly,
        /// <summary>
        /// ACL is changed for this object and propagated to related objects.
        /// </summary>
        [CmisValue("propagate")]
        Propagate
    }

    /// <summary>
    /// Content Stream Allowed flag.
    /// </summary>
    public enum ContentStreamAllowed
    {
        /// <summary>
        /// Documents cannot have content.
        /// </summary>
        [CmisValue("notallowed")]
        NotAllowed,
        /// <summary>
        /// Documents can have content.
        /// </summary>
        [CmisValue("allowed")]
        Allowed,
        /// <summary>
        /// Documents must have content.
        /// </summary>
        [CmisValue("required")]
        Required
    }

    /// <summary>
    /// Property Data Type.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// Boolean.
        /// </summary>
        [CmisValue("boolean")]
        Boolean,
        /// <summary>
        /// ID.
        /// </summary>
        [CmisValue("id")]
        Id,
        /// <summary>
        /// Integer.
        /// </summary>
        [CmisValue("integer")]
        Integer,
        /// <summary>
        /// DateTime.
        /// </summary>
        [CmisValue("datetime")]
        DateTime,
        /// <summary>
        /// Decimal.
        /// </summary>
        [CmisValue("decimal")]
        Decimal,
        /// <summary>
        /// HTML.
        /// </summary>
        [CmisValue("html")]
        Html,
        /// <summary>
        /// String.
        /// </summary>
        [CmisValue("string")]
        String,
        /// <summary>
        /// URI.
        /// </summary>
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
        /// <summary>
        /// Years only.
        /// </summary>
        [CmisValue("year")]
        Year,
        /// <summary>
        /// Date.
        /// </summary>
        [CmisValue("date")]
        Date,
        /// <summary>
        /// Date and time.
        /// </summary>
        [CmisValue("time")]
        Time
    }

    /// <summary>
    /// Decimal Property Precision.
    /// </summary>
    public enum DecimalPrecision
    {
        /// <summary>
        /// 32 bit precision.
        /// </summary>
        [CmisValue("32")]
        Bits32,
        /// <summary>
        /// 64 bit precision.
        /// </summary>
        [CmisValue("64")]
        Bits64
    }

    /// <summary>
    /// Include Relationships flag.
    /// </summary>
    public enum IncludeRelationships
    {
        /// <summary>
        /// Include no relationships.
        /// </summary>
        [CmisValue("none")]
        None,
        /// <summary>
        /// Only relationships in which the objects returned are the source must be returned.
        /// </summary>
        [CmisValue("source")]
        Source,
        /// <summary>
        /// Only relationships in which the objects returned are the target must be returned.
        /// </summary>
        [CmisValue("target")]
        Target,
        /// <summary>
        /// Include all relationships.
        /// </summary>
        [CmisValue("both")]
        Both
    }

    /// <summary>
    /// Versioning State flag.
    /// </summary>
    public enum VersioningState
    {
        /// <summary>
        /// No version.
        /// </summary>
        [CmisValue("none")]
        None,
        /// <summary>
        /// Major version.
        /// </summary>
        [CmisValue("major")]
        Major,
        /// <summary>
        /// Minor version.
        /// </summary>
        [CmisValue("minor")]
        Minor,
        /// <summary>
        /// Checked out (PWC).
        /// </summary>
        [CmisValue("checkedout")]
        CheckedOut
    }

    /// <summary>
    /// Unfile Object flag.
    /// </summary>
    public enum UnfileObject
    {
        /// <summary>
        /// Unfile all objects.
        /// </summary>
        [CmisValue("unfile")]
        Unfile,
        /// <summary>
        /// Delete single filed object, unfile all others.
        /// </summary>
        [CmisValue("deletesinglefiled")]
        DeleteSinglefiled,
        /// <summary>
        /// Delete all objects.
        /// </summary>
        [CmisValue("delete")]
        Delete
    }

    /// <summary>
    /// Relationship Direction flag.
    /// </summary>
    public enum RelationshipDirection
    {
        /// <summary>
        /// Only relationship objects where the specified object is the source object.
        /// </summary>
        [CmisValue("source")]
        Source,
        /// <summary>
        /// Only relationship objects where the specified object is the target object
        /// </summary>
        [CmisValue("target")]
        Target,
        /// <summary>
        /// All relationships.
        /// </summary>
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
    internal class CmisValueAttribute : System.Attribute
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
    internal static class CmisValue
    {
        /// <summary>
        /// Gets the CMIS value from an enum.
        /// </summary>
        /// <param name="value">the enum</param>
        /// <returns>the CMIS value or <c>null</c> if no value can be determined</returns>
        public static string GetCmisValue(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetRuntimeField(value.ToString());
            CmisValueAttribute[] cmisValueAttr = fieldInfo.GetCustomAttributes(typeof(CmisValueAttribute), false) as CmisValueAttribute[];
            return cmisValueAttr.Length > 0 ? cmisValueAttr[0].Value : null;
        }

        /// <summary>
        /// Gets an enum from a CMIS value.
        /// </summary>
        /// <typeparam name="T">the enum type</typeparam>
        /// <param name="value">the CMIS value</param>
        /// <returns>the enum value</returns>
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
