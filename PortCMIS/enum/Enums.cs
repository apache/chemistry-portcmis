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
    public enum BaseTypeId
    {
        [CmisValue("cmis:document")]
        CmisDocument,
        [CmisValue("cmis:folder")]
        CmisFolder,
        [CmisValue("cmis:relationship")]
        CmisRelationship,
        [CmisValue("cmis:policy")]
        CmisPolicy,
        [CmisValue("cmis:item")]
        CmisItem,
        [CmisValue("cmis:secondary")]
        CmisSecondary
    }

    public enum CapabilityContentStreamUpdates
    {
        [CmisValue("anytime")]
        Anyime,
        [CmisValue("pwconly")]
        PWCOnly,
        [CmisValue("none")]
        None
    }

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

    public enum CapabilityRenditions
    {
        [CmisValue("none")]
        None,
        [CmisValue("read")]
        Read
    }

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

    public enum CapabilityJoin
    {
        [CmisValue("none")]
        None,
        [CmisValue("inneronly")]
        InnerOnly,
        [CmisValue("innerandouter")]
        InnerAndOuter
    }

    public enum CapabilityAcl
    {
        [CmisValue("none")]
        None,
        [CmisValue("discover")]
        Discover,
        [CmisValue("manage")]
        Manage
    }

    public enum CapabilityOrderBy
    {
        [CmisValue("none")]
        None,
        [CmisValue("common")]
        Common,
        [CmisValue("custom")]
        Custom
    }

    public enum SupportedPermissions
    {
        [CmisValue("basic")]
        Basic,
        [CmisValue("repository")]
        Repository,
        [CmisValue("both")]
        Both
    }

    public enum AclPropagation
    {
        [CmisValue("repositorydetermined")]
        RepositoryDetermined,
        [CmisValue("objectonly")]
        ObjectOnly,
        [CmisValue("propagate")]
        Propagate
    }

    public enum ContentStreamAllowed
    {
        [CmisValue("notallowed")]
        NotAllowed,
        [CmisValue("allowed")]
        Allowed,
        [CmisValue("required")]
        Required
    }

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

    public enum Cardinality
    {
        [CmisValue("single")]
        Single,
        [CmisValue("multi")]
        Multi
    }

    public enum Updatability
    {
        [CmisValue("readonly")]
        ReadOnly,
        [CmisValue("readwrite")]
        ReadWrite,
        [CmisValue("whencheckedout")]
        WhenCheckedOut,
        [CmisValue("oncreate")]
        OnCreate
    }

    public enum DateTimeResolution
    {
        [CmisValue("year")]
        Year,
        [CmisValue("date")]
        Date,
        [CmisValue("time")]
        Time
    }

    public enum DecimalPrecision
    {
        [CmisValue("32")]
        Bits32,
        [CmisValue("64")]
        Bits64
    }

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

    public enum UnfileObject
    {
        [CmisValue("unfile")]
        Unfile,
        [CmisValue("deletesinglefiled")]
        DeleteSinglefiled,
        [CmisValue("delete")]
        Delete
    }

    public enum RelationshipDirection
    {
        [CmisValue("source")]
        Source,
        [CmisValue("target")]
        Target,
        [CmisValue("either")]
        Either
    }

    public enum ReturnVersion
    {
        [CmisValue("this")]
        This,
        [CmisValue("latest")]
        Latest,
        [CmisValue("latestmajor")]
        LatestMajor
    }

    public enum ChangeType
    {
        [CmisValue("created")]
        Created,
        [CmisValue("updated")]
        Updated,
        [CmisValue("deleted")]
        Deleted,
        [CmisValue("security")]
        Security
    }

    public enum DateTimeFormat
    {
        [CmisValue("simple")]
        Simple,
        [CmisValue("extended")]
        Extended
    }

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

    public enum CmisVersion
    {
        [CmisValue("1.0")]
        Cmis_1_0,
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
