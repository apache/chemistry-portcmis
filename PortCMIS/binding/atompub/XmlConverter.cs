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

using PortCMIS.Data;
using PortCMIS.Data.Extensions;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using PortCMIS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml;

namespace PortCMIS.Binding.AtomPub
{
    class XmlConverter
    {
        // ---------------
        // --- writers ---
        // ---------------

        public static void WriteRepositoryInfo(XmlWriter writer, CmisVersion cmisVersion, string ns, IRepositoryInfo source)
        {
            if (source == null)
            {
                return;
            }

            writer.WriteStartElement(XmlConstants.TAG_REPOSITORY_INFO, ns);

            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_ID, source.Id);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_NAME, source.Name);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_DESCRIPTION, source.Description);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_VENDOR, source.VendorName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_PRODUCT, source.ProductName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_PRODUCT_VERSION, source.ProductVersion);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_ROOT_FOLDER_ID, source.RootFolderId);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_CHANGE_LOG_TOKEN, source.LatestChangeLogToken);
            WriteRepositoryCapabilities(writer, cmisVersion, source.Capabilities);
            WriteAclCapabilities(writer, cmisVersion, source.AclCapabilities);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_CMIS_VERSION_SUPPORTED, source.CmisVersionSupported);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_THIN_CLIENT_URI, source.ThinClientUri);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_CHANGES_INCOMPLETE, source.ChangesIncomplete);
            if (source.ChangesOnType != null)
            {
                foreach (BaseTypeId baseType in source.ChangesOnType)
                {
                    if (cmisVersion == CmisVersion.Cmis_1_0 && baseType == BaseTypeId.CmisItem)
                    {
                        Logger.Warn("Receiver only understands CMIS 1.0 but the Changes On Type list in the Repository info contains the base type Item. "
                                + "The Item base type has been removed from the list.");
                        continue;
                    }
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_CHANGES_ON_TYPE, baseType);
                }
            }
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_PRINCIPAL_ID_ANONYMOUS, source.PrincipalIdAnonymous);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_REPINFO_PRINCIPAL_ID_ANYONE, source.PrincipalIdAnyone);
            if (cmisVersion != CmisVersion.Cmis_1_0 && source.ExtensionFeatures != null)
            {
                foreach (ExtensionFeature feature in source.ExtensionFeatures)
                {
                    WriteExtendedFeatures(writer, cmisVersion, feature);
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        public static void WriteRepositoryCapabilities(XmlWriter writer, CmisVersion cmisVersion, IRepositoryCapabilities source)
        {
            if (source == null)
            {
                return;
            }

            writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_REPINFO_CAPABILITIES, XmlConstants.NamespaceCmis);

            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_ACL, source.AclCapability);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_ALL_VERSIONS_SEARCHABLE, source.IsAllVersionsSearchableSupported);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_CHANGES, source.ChangesCapability);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_CONTENT_STREAM_UPDATABILITY, source.ContentStreamUpdatesCapability);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_GET_DESCENDANTS, source.IsGetDescendantsSupported);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_GET_FOLDER_TREE, source.IsGetFolderTreeSupported);
            if (cmisVersion != CmisVersion.Cmis_1_0)
            {
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_ORDER_BY, source.OrderByCapability);
            }
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_MULTIFILING, source.IsMultifilingSupported);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_PWC_SEARCHABLE, source.IsPwcSearchableSupported);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_PWC_UPDATABLE, source.IsPwcUpdatableSupported);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_QUERY, source.QueryCapability);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_RENDITIONS, source.RenditionsCapability);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_UNFILING, source.IsUnfilingSupported);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_VERSION_SPECIFIC_FILING, source.IsVersionSpecificFilingSupported);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_JOIN, source.JoinCapability);
            if (cmisVersion != CmisVersion.Cmis_1_0)
            {
                if (source.CreatablePropertyTypes != null)
                {
                    ICreatablePropertyTypes creatablePropertyTypes = source.CreatablePropertyTypes;

                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_CAP_CREATABLE_PROPERTY_TYPES, XmlConstants.NamespaceCmis);

                    if (creatablePropertyTypes.CanCreate != null)
                    {
                        foreach (PropertyType pt in creatablePropertyTypes.CanCreate)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_CREATABLE_PROPERTY_TYPES_CANCREATE,
                                    pt);
                        }
                    }

                    WriteExtensions(writer, creatablePropertyTypes);
                    writer.WriteEndElement();
                }
                if (source.NewTypeSettableAttributes != null)
                {
                    INewTypeSettableAttributes newTypeSettableAttributes = source.NewTypeSettableAttributes;

                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES, XmlConstants.NamespaceCmis);

                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_ID,
                            newTypeSettableAttributes.CanSetId);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_LOCALNAME,
                            newTypeSettableAttributes.CanSetLocalName);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis,
                            XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_LOCALNAMESPACE,
                            newTypeSettableAttributes.CanSetLocalNamespace);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_DISPLAYNAME,
                            newTypeSettableAttributes.CanSetDisplayName);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_QUERYNAME,
                            newTypeSettableAttributes.CanSetQueryName);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_DESCRIPTION,
                            newTypeSettableAttributes.CanSetDescription);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CREATEABLE,
                            newTypeSettableAttributes.CanSetCreatable);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_FILEABLE,
                            newTypeSettableAttributes.CanSetFileable);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_QUERYABLE,
                            newTypeSettableAttributes.CanSetQueryable);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis,
                            XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_FULLTEXTINDEXED,
                            newTypeSettableAttributes.CanSetFulltextIndexed);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis,
                            XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_INCLUDEDINSUPERTYTPEQUERY,
                            newTypeSettableAttributes.CanSetIncludedInSupertypeQuery);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis,
                            XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CONTROLABLEPOLICY,
                            newTypeSettableAttributes.CanSetControllablePolicy);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis,
                            XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CONTROLABLEACL,
                            newTypeSettableAttributes.CanSetControllableAcl);

                    WriteExtensions(writer, newTypeSettableAttributes);
                    writer.WriteEndElement();
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        public static void WriteAclCapabilities(XmlWriter writer, CmisVersion cmisVersion, IAclCapabilities source)
        {
            if (source == null)
            {
                return;
            }

            writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_REPINFO_ACL_CAPABILITIES, XmlConstants.NamespaceCmis);

            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACLCAP_SUPPORTED_PERMISSIONS, source.SupportedPermissions);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACLCAP_ACL_PROPAGATION, source.AclPropagation);
            if (source.Permissions != null)
            {
                foreach (IPermissionDefinition pd in source.Permissions)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_ACLCAP_PERMISSIONS, XmlConstants.NamespaceCmis);

                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACLCAP_PERMISSION_PERMISSION, pd.Id);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACLCAP_PERMISSION_DESCRIPTION, pd.Description);

                    WriteExtensions(writer, pd);
                    writer.WriteEndElement();
                }
            }
            if (source.PermissionMapping != null)
            {
                foreach (IPermissionMapping pm in source.PermissionMapping.Values)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_ACLCAP_PERMISSION_MAPPING, XmlConstants.NamespaceCmis);

                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACLCAP_MAPPING_KEY, pm.Key);
                    if (pm.Permissions != null)
                    {
                        foreach (String perm in pm.Permissions)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACLCAP_MAPPING_PERMISSION, perm);
                        }
                    }

                    WriteExtensions(writer, pm);
                    writer.WriteEndElement();
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        public static void WriteExtendedFeatures(XmlWriter writer, CmisVersion cmisVersion, ExtensionFeature source)
        {
            if (source == null)
            {
                return;
            }

            writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_REPINFO_EXTENDED_FEATURES, XmlConstants.NamespaceCmis);

            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_FEATURE_ID, source.Id);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_FEATURE_URL, source.Url);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_FEATURE_COMMON_NAME, source.CommonName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_FEATURE_VERSION_LABEL, source.VersionLabel);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_FEATURE_DESCRIPTION, source.Description);
            if (source.FeatureData != null)
            {
                foreach (KeyValuePair<string, string> data in source.FeatureData)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_FEATURE_DATA, XmlConstants.NamespaceCmis);

                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_FEATURE_DATA_KEY, data.Key);
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_FEATURE_DATA_VALUE, data.Value);

                    writer.WriteEndElement();
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        // --------------------------
        // --- definition writers ---
        // --------------------------

        public static void WriteTypeDefinition(XmlWriter writer, CmisVersion cmisVersion, string ns, ITypeDefinition source)
        {
            if (source == null)
            {
                return;
            }

            if (cmisVersion == CmisVersion.Cmis_1_0)
            {
                if (source.BaseTypeId == BaseTypeId.CmisItem)
                {
                    Logger.Warn("Receiver only understands CMIS 1.0. It may not able to handle an Item type definition.");
                }
                else if (source.BaseTypeId == BaseTypeId.CmisSecondary)
                {
                    Logger.Warn("Receiver only understands CMIS 1.0. It may not able to handle a Secondary type definition.");
                }
            }

            writer.WriteStartElement(XmlConstants.TAG_TYPE, ns);
            writer.WriteAttributeString("xmlns", XmlConstants.PrefixXsi, null, XmlConstants.NamespaceXsi);
            string prefix = writer.LookupPrefix(ns);
            if (prefix != null)
            {
                writer.WriteAttributeString("xmlns", prefix, null, ns);
            }

            if (source.BaseTypeId == BaseTypeId.CmisDocument)
            {
                writer.WriteAttributeString(XmlConstants.PrefixXsi, "type", XmlConstants.NamespaceXsi, XmlConstants.PrefixCmis + ":" + XmlConstants.ATTR_DOCUMENT_TYPE);
            }
            else if (source.BaseTypeId == BaseTypeId.CmisFolder)
            {
                writer.WriteAttributeString(XmlConstants.PrefixXsi, "type", XmlConstants.NamespaceXsi, XmlConstants.PrefixCmis + ":" + XmlConstants.ATTR_FOLDER_TYPE);
            }
            else if (source.BaseTypeId == BaseTypeId.CmisRelationship)
            {
                writer.WriteAttributeString(XmlConstants.PrefixXsi, "type", XmlConstants.NamespaceXsi, XmlConstants.PrefixCmis + ":" + XmlConstants.ATTR_RELATIONSHIP_TYPE);
            }
            else if (source.BaseTypeId == BaseTypeId.CmisPolicy)
            {
                writer.WriteAttributeString(XmlConstants.PrefixXsi, "type", XmlConstants.NamespaceXsi, XmlConstants.PrefixCmis + ":" + XmlConstants.ATTR_POLICY_TYPE);
            }
            else if (source.BaseTypeId == BaseTypeId.CmisItem)
            {
                writer.WriteAttributeString(XmlConstants.PrefixXsi, "type", XmlConstants.NamespaceXsi, XmlConstants.PrefixCmis + ":" + XmlConstants.ATTR_ITEM_TYPE);
            }
            else if (source.BaseTypeId == BaseTypeId.CmisSecondary)
            {
                writer.WriteAttributeString(XmlConstants.PrefixXsi, "type", XmlConstants.NamespaceXsi, XmlConstants.PrefixCmis + ":" + XmlConstants.ATTR_SECONDARY_TYPE);
            }
            else
            {
                throw new CmisRuntimeException("Type definition has no base type id!");
            }

            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_ID, source.Id);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_LOCALNAME, source.LocalName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_LOCALNAMESPACE, source.LocalNamespace);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_DISPLAYNAME, source.DisplayName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_QUERYNAME, source.QueryName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_DESCRIPTION, source.Description);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_BASE_ID, source.BaseTypeId);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_PARENT_ID, source.ParentTypeId);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_CREATABLE, source.IsCreatable);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_FILEABLE, source.IsFileable);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_QUERYABLE, source.IsQueryable);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_FULLTEXT_INDEXED, source.IsFulltextIndexed);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_INCLUDE_IN_SUPERTYPE_QUERY, source.IsIncludedInSupertypeQuery);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_CONTROLABLE_POLICY, source.IsControllablePolicy);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_CONTROLABLE_ACL, source.IsControllableAcl);
            if (cmisVersion != CmisVersion.Cmis_1_0 && source.TypeMutability != null)
            {
                ITypeMutability tm = source.TypeMutability;

                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_TYPE_MUTABILITY, XmlConstants.NamespaceCmis);

                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_TYPE_MUTABILITY_CREATE, tm.CanCreate);
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_TYPE_MUTABILITY_UPDATE, tm.CanUpdate);
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_TYPE_MUTABILITY_DELETE, tm.CanDelete);

                WriteExtensions(writer, tm);
                writer.WriteEndElement();
            }
            if (source.PropertyDefinitions != null)
            {
                foreach (IPropertyDefinition pd in source.PropertyDefinitions)
                {
                    WritePropertyDefinition(writer, cmisVersion, pd);
                }
            }

            if (source is IDocumentTypeDefinition)
            {
                IDocumentTypeDefinition docDef = (IDocumentTypeDefinition)source;
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_VERSIONABLE, docDef.IsVersionable);
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_CONTENTSTREAM_ALLOWED, docDef.ContentStreamAllowed);
            }

            if (source is IRelationshipTypeDefinition)
            {
                IRelationshipTypeDefinition relDef = (IRelationshipTypeDefinition)source;
                if (relDef.AllowedSourceTypeIds != null)
                {
                    foreach (string id in relDef.AllowedSourceTypeIds)
                    {
                        if (id != null)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_ALLOWED_SOURCE_TYPES, id);
                        }
                    }
                }
                if (relDef.AllowedTargetTypeIds != null)
                {
                    foreach (string id in relDef.AllowedTargetTypeIds)
                    {
                        if (id != null)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_TYPE_ALLOWED_TARGET_TYPES, id);
                        }
                    }
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        public static void WritePropertyDefinition(XmlWriter writer, CmisVersion cmisVersion, IPropertyDefinition source)
        {
            if (source == null)
            {
                return;
            }

            //if (source.PropertyType == null)
            //{
            //    throw new CmisRuntimeException("Property type for property definition '" + source.Id + "' is not set!");
            //}

            switch (source.PropertyType)
            {
                case PropertyType.String:
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_PROP_DEF_STRING, XmlConstants.NamespaceCmis);
                    break;
                case PropertyType.Id:
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_PROP_DEF_ID, XmlConstants.NamespaceCmis);
                    break;
                case PropertyType.Integer:
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_PROP_DEF_INTEGER, XmlConstants.NamespaceCmis);
                    break;
                case PropertyType.Boolean:
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_PROP_DEF_BOOLEAN, XmlConstants.NamespaceCmis);
                    break;
                case PropertyType.DateTime:
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_PROP_DEF_DATETIME, XmlConstants.NamespaceCmis);
                    break;
                case PropertyType.Decimal:
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_PROP_DEF_DECIMAL, XmlConstants.NamespaceCmis);
                    break;
                case PropertyType.Html:
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_PROP_DEF_HTML, XmlConstants.NamespaceCmis);
                    break;
                case PropertyType.Uri:
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_TYPE_PROP_DEF_URI, XmlConstants.NamespaceCmis);
                    break;
                default:
                    throw new CmisRuntimeException("Property definition has no property type!");
            }

            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_ID, source.Id);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_LOCALNAME, source.LocalName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_LOCALNAMESPACE, source.LocalNamespace);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_DISPLAYNAME, source.DisplayName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_QUERYNAME, source.QueryName);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_DESCRIPTION, source.Description);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_PROPERTY_TYPE, source.PropertyType);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_CARDINALITY, source.Cardinality);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_UPDATABILITY, source.Updatability);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_INHERITED, source.IsInherited);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_REQUIRED, source.IsRequired);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_QUERYABLE, source.IsQueryable);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_ORDERABLE, source.IsOrderable);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_OPENCHOICE, source.IsOpenChoice);

            if (source is IPropertyStringDefinition)
            {
                IPropertyStringDefinition def = (IPropertyStringDefinition)source;

                if (def.DefaultValue != null)
                {
                    PropertyData prop = new PropertyData(PropertyType.String);
                    prop.AddValue(def.DefaultValue);
                    WriteProperty(writer, prop, true);
                }

                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_MAX_LENGTH, def.MaxLength);

                if (def.Choices != null)
                {
                    foreach (IChoice<string> c in def.Choices)
                    {
                        if (c != null)
                        {
                            WriteChoice<string>(writer, source.PropertyType, c);
                        }
                    }
                }
            }
            else if (source is IPropertyIdDefinition)
            {
                IPropertyIdDefinition def = (IPropertyIdDefinition)source;

                if (def.DefaultValue != null)
                {
                    PropertyData prop = new PropertyData(PropertyType.Id);
                    prop.AddValue(def.DefaultValue);
                    WriteProperty(writer, prop, true);
                }

                if (def.Choices != null)
                {
                    foreach (IChoice<string> c in def.Choices)
                    {
                        if (c != null)
                        {
                            WriteChoice<string>(writer, source.PropertyType, c);
                        }
                    }
                }
            }
            else if (source is IPropertyIntegerDefinition)
            {
                IPropertyIntegerDefinition def = (IPropertyIntegerDefinition)source;

                if (def.DefaultValue != null)
                {
                    PropertyData prop = new PropertyData(PropertyType.Integer);
                    prop.AddValue(def.DefaultValue);
                    WriteProperty(writer, prop, true);
                }

                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_MAX_VALUE, def.MaxValue);
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_MIN_VALUE, def.MinValue);

                if (def.Choices != null)
                {
                    foreach (IChoice<BigInteger> c in def.Choices)
                    {
                        if (c != null)
                        {
                            WriteChoice<BigInteger>(writer, source.PropertyType, c);
                        }
                    }
                }
            }
            else if (source is IPropertyBooleanDefinition)
            {
                IPropertyBooleanDefinition def = (IPropertyBooleanDefinition)source;

                if (def.DefaultValue != null)
                {
                    PropertyData prop = new PropertyData(PropertyType.Boolean);
                    prop.AddValue(def.DefaultValue);
                    WriteProperty(writer, prop, true);
                }

                if (def.Choices != null)
                {
                    foreach (IChoice<bool> c in def.Choices)
                    {
                        if (c != null)
                        {
                            WriteChoice<bool>(writer, source.PropertyType, c);
                        }
                    }
                }
            }
            else if (source is IPropertyDateTimeDefinition)
            {
                IPropertyDateTimeDefinition def = (IPropertyDateTimeDefinition)source;

                if (def.DefaultValue != null)
                {
                    PropertyData prop = new PropertyData(PropertyType.DateTime);
                    prop.AddValue(def.DefaultValue);
                    WriteProperty(writer, prop, true);
                }

                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_RESOLUTION, def.DateTimeResolution);

                if (def.Choices != null)
                {
                    foreach (IChoice<DateTime> c in def.Choices)
                    {
                        if (c != null)
                        {
                            WriteChoice<DateTime>(writer, source.PropertyType, c);
                        }
                    }
                }
            }
            else if (source is IPropertyDecimalDefinition)
            {
                IPropertyDecimalDefinition def = (IPropertyDecimalDefinition)source;

                if (def.DefaultValue != null)
                {
                    PropertyData prop = new PropertyData(PropertyType.Decimal);
                    prop.AddValue(def.DefaultValue);
                    WriteProperty(writer, prop, true);
                }

                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_MAX_VALUE, def.MaxValue);
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_MIN_VALUE, def.MinValue);
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_PRECISION, def.Precision);

                if (def.Choices != null)
                {
                    foreach (IChoice<decimal?> c in def.Choices)
                    {
                        if (c != null)
                        {
                            WriteChoice<decimal?>(writer, source.PropertyType, c);
                        }
                    }
                }
            }
            else if (source is IPropertyHtmlDefinition)
            {
                IPropertyHtmlDefinition def = (IPropertyHtmlDefinition)source;

                if (def.DefaultValue != null)
                {
                    PropertyData prop = new PropertyData(PropertyType.Html);
                    prop.AddValue(def.DefaultValue);
                    WriteProperty(writer, prop, true);
                }

                if (def.Choices != null)
                {
                    foreach (IChoice<string> c in def.Choices)
                    {
                        if (c != null)
                        {
                            WriteChoice<string>(writer, source.PropertyType, c);
                        }
                    }
                }
            }
            else if (source is IPropertyUriDefinition)
            {
                IPropertyUriDefinition def = (IPropertyUriDefinition)source;

                if (def.DefaultValue != null)
                {
                    PropertyData prop = new PropertyData(PropertyType.Uri);
                    prop.AddValue(def.DefaultValue);
                    WriteProperty(writer, prop, true);
                }

                if (def.Choices != null)
                {
                    foreach (IChoice<string> c in def.Choices)
                    {
                        if (c != null)
                        {
                            WriteChoice<string>(writer, source.PropertyType, c);
                        }
                    }
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        public static void WriteChoice<T>(XmlWriter writer, PropertyType propType, IChoice<T> source)
        {
            if (source == null)
            {
                return;
            }

            writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROPERTY_TYPE_CHOICE, XmlConstants.NamespaceCmis);

            if (source.DisplayName != null)
            {
                writer.WriteAttributeString(XmlConstants.ATTR_PROPERTY_TYPE_CHOICE_DISPLAYNAME, source.DisplayName);
            }

            if (source.Value != null)
            {
                switch (propType)
                {
                    case PropertyType.String:
                    case PropertyType.Id:
                    case PropertyType.Html:
                    case PropertyType.Uri:
                        foreach (string value in (IList<string>)source.Value)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_CHOICE_VALUE, value);
                        }
                        break;
                    case PropertyType.Integer:
                        foreach (BigInteger value in (IList<BigInteger>)source.Value)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_CHOICE_VALUE, value);
                        }
                        break;
                    case PropertyType.Boolean:
                        foreach (bool? value in (IList<bool?>)source.Value)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_CHOICE_VALUE, value);
                        }
                        break;
                    case PropertyType.DateTime:
                        foreach (DateTime value in (IList<DateTime>)source.Value)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_CHOICE_VALUE, value);
                        }
                        break;
                    case PropertyType.Decimal:
                        foreach (decimal? value in (IList<decimal?>)source.Value)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_TYPE_CHOICE_VALUE, value);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (source.Choices != null)
            {
                foreach (Choice<T> c in source.Choices)
                {
                    if (c != null)
                    {
                        WriteChoice<T>(writer, propType, c);
                    }
                }
            }

            writer.WriteEndElement();
        }

        // -----------------------
        // --- object writers ---
        // -----------------------

        public static void WriteObject(XmlWriter writer, CmisVersion cmisVersion, string ns, IObjectData source)
        {
            WriteObject(writer, cmisVersion, false, XmlConstants.TAG_OBJECT, ns, source);
        }

        public static void WriteObject(XmlWriter writer, CmisVersion cmisVersion, bool root, string name, string ns, IObjectData source)
        {
            if (source == null)
            {
                return;
            }

            if (cmisVersion == CmisVersion.Cmis_1_0)
            {
                if (source.BaseTypeId == BaseTypeId.CmisItem)
                {
                    Logger.Warn("Receiver only understands CMIS 1.0. It may not be able to handle an Item object.");
                }
            }

            if (root)
            {
                writer.WriteStartElement(XmlConstants.PrefixCmis, name, XmlConstants.NamespaceCmis);
                writer.WriteAttributeString("xmlns", XmlConstants.PrefixCmis, null, XmlConstants.NamespaceCmis);
            }
            else
            {
                writer.WriteStartElement(name, ns);
            }

            if (source.Properties != null)
            {
                IProperties properties = source.Properties;

                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_OBJECT_PROPERTIES, XmlConstants.NamespaceCmis);

                if (properties.PropertyList != null)
                {
                    foreach (PropertyData property in properties.PropertyList)
                    {
                        WriteProperty(writer, property, false);
                    }
                }

                WriteExtensions(writer, properties);
                writer.WriteEndElement();
            }
            if (source.AllowableActions != null)
            {
                WriteAllowableActions(writer, cmisVersion, false, source.AllowableActions);
            }
            if (source.Relationships != null)
            {
                foreach (IObjectData rel in source.Relationships)
                {
                    if (rel != null)
                    {
                        WriteObject(writer, cmisVersion, false, XmlConstants.TAG_OBJECT_RELATIONSHIP, XmlConstants.NamespaceCmis, rel);
                    }
                }
            }
            if (source.ChangeEventInfo != null)
            {
                IChangeEventInfo info = source.ChangeEventInfo;

                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_OBJECT_CHANGE_EVENT_INFO, XmlConstants.NamespaceCmis);

                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CHANGE_EVENT_TYPE, info.ChangeType);
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_CHANGE_EVENT_TIME, info.ChangeTime);

                WriteExtensions(writer, info);
                writer.WriteEndElement();
            }
            if (source.Acl != null)
            {
                WriteAcl(writer, cmisVersion, false, source.Acl);
            }
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_OBJECT_EXACT_ACL, source.IsExactAcl);
            if (source.PolicyIds != null)
            {
                IPolicyIdList pids = source.PolicyIds;

                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_OBJECT_POLICY_IDS, XmlConstants.NamespaceCmis);

                if (pids.PolicyIds != null)
                {
                    foreach (string id in pids.PolicyIds)
                    {
                        if (id != null)
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_POLICY_ID, id);
                        }
                    }
                }

                WriteExtensions(writer, pids);
                writer.WriteEndElement();
            }
            if (source.Renditions != null)
            {
                foreach (IRenditionData rend in source.Renditions)
                {
                    if (rend != null)
                    {
                        writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_OBJECT_RENDITION, XmlConstants.NamespaceCmis);

                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_RENDITION_STREAM_ID, rend.StreamId);
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_RENDITION_MIMETYPE, rend.MimeType);
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_RENDITION_LENGTH, rend.Length);
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_RENDITION_KIND, rend.Kind);
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_RENDITION_TITLE, rend.Title);
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_RENDITION_HEIGHT, rend.Height);
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_RENDITION_WIDTH, rend.Width);
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_RENDITION_DOCUMENT_ID, rend.RenditionDocumentId);

                        WriteExtensions(writer, rend);
                        writer.WriteEndElement();
                    }
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        public static void WriteProperty(XmlWriter writer, IPropertyData source, bool isDefaultValue)
        {
            if (source == null)
            {
                return;
            }

            if (isDefaultValue)
            {
                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE, XmlConstants.NamespaceCmis);
            }
            else
            {
                if (source.PropertyType == PropertyType.String)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROP_STRING, XmlConstants.NamespaceCmis);
                }
                else if (source.PropertyType == PropertyType.Id)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROP_ID, XmlConstants.NamespaceCmis);
                }
                else if (source.PropertyType == PropertyType.Integer)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROP_INTEGER, XmlConstants.NamespaceCmis);
                }
                else if (source.PropertyType == PropertyType.Boolean)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROP_BOOLEAN, XmlConstants.NamespaceCmis);
                }
                else if (source.PropertyType == PropertyType.DateTime)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROP_DATETIME, XmlConstants.NamespaceCmis);
                }
                else if (source.PropertyType == PropertyType.Decimal)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROP_DECIMAL, XmlConstants.NamespaceCmis);
                }
                else if (source.PropertyType == PropertyType.Html)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROP_HTML, XmlConstants.NamespaceCmis);
                }
                else if (source.PropertyType == PropertyType.Uri)
                {
                    writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_PROP_URI, XmlConstants.NamespaceCmis);
                }
                else
                {
                    throw new CmisRuntimeException("Invalid property datatype!");
                }
            }

            if (source.Id != null)
            {
                writer.WriteAttributeString(XmlConstants.ATTR_PROPERTY_ID, source.Id);
            }
            if (source.DisplayName != null)
            {
                writer.WriteAttributeString(XmlConstants.ATTR_PROPERTY_DISPLAYNAME, source.DisplayName);
            }
            if (source.LocalName != null)
            {
                writer.WriteAttributeString(XmlConstants.ATTR_PROPERTY_LOCALNAME, source.LocalName);
            }
            if (source.QueryName != null)
            {
                writer.WriteAttributeString(XmlConstants.ATTR_PROPERTY_QUERYNAME, source.QueryName);
            }

            if (source.Values != null)
            {

                switch (source.PropertyType)
                {
                    case PropertyType.String:
                    case PropertyType.Id:
                    case PropertyType.Html:
                    case PropertyType.Uri:
                        foreach (string value in source.Values.Cast<string>())
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_VALUE, value);
                        }
                        break;
                    case PropertyType.Integer:
                        foreach (BigInteger value in source.Values.Cast<BigInteger>())
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_VALUE, value);
                        }
                        break;
                    case PropertyType.Boolean:
                        foreach (bool value in source.Values.Cast<bool>())
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_VALUE, value);
                        }
                        break;
                    case PropertyType.DateTime:
                        foreach (DateTime value in source.Values.Cast<DateTime>())
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_VALUE, value);
                        }
                        break;
                    case PropertyType.Decimal:
                        foreach (decimal value in source.Values.Cast<decimal>())
                        {
                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_PROPERTY_VALUE, value);
                        }
                        break;
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        public static void WriteAllowableActions(XmlWriter writer, CmisVersion cmisVersion, bool root, IAllowableActions source)
        {
            if (source == null)
            {
                return;
            }

            if (root)
            {
                writer.WriteStartElement(XmlConstants.PrefixCmis, "allowableActions", XmlConstants.NamespaceCmis);
                writer.WriteAttributeString("xmlns", XmlConstants.PrefixCmis, null, XmlConstants.NamespaceCmis);
            }
            else
            {
                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_OBJECT_ALLOWABLE_ACTIONS, XmlConstants.NamespaceCmis);
            }

            if (source.Actions != null)
            {
                var values = Enum.GetValues(typeof(PortCMIS.Enums.Action));
                foreach (var value in values)
                {
                    PortCMIS.Enums.Action action = (PortCMIS.Enums.Action)Enum.ToObject(typeof(PortCMIS.Enums.Action), value);
                    if (source.Actions.Contains(action))
                    {
                        if (action == PortCMIS.Enums.Action.CanCreateItem && cmisVersion == CmisVersion.Cmis_1_0)
                        {
                            Logger.Warn("Receiver only understands CMIS 1.0 but the Allowable Actions contain the canCreateItem action. "
                                    + "The canCreateItem action has been removed from the Allowable Actions.");
                            continue;
                        }
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, action.GetCmisValue(), true);
                    }
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        public static void WriteAcl(XmlWriter writer, CmisVersion cmisVersion, bool root, IAcl source)
        {
            if (source == null)
            {
                return;
            }

            if (root)
            {
                writer.WriteStartElement(XmlConstants.PrefixCmis, "acl", XmlConstants.NamespaceCmis);
                writer.WriteAttributeString("xmlns", XmlConstants.PrefixCmis, null, XmlConstants.NamespaceCmis);
            }
            else
            {
                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_OBJECT_ACL, XmlConstants.NamespaceCmis);
            }

            if (source.Aces != null)
            {
                foreach (IAce ace in source.Aces)
                {
                    if (ace != null)
                    {
                        writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_ACL_PERMISSISONS, XmlConstants.NamespaceCmis);

                        if (ace.Principal != null)
                        {
                            IPrincipal principal = ace.Principal;

                            writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_ACE_PRINCIPAL, XmlConstants.NamespaceCmis);

                            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACE_PRINCIPAL_ID, principal.Id);

                            WriteExtensions(writer, principal);
                            writer.WriteEndElement();
                        }
                        if (ace.Permissions != null)
                        {
                            foreach (String perm in ace.Permissions)
                            {
                                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACE_PERMISSIONS, perm);
                            }
                        }
                        XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_ACE_IS_DIRECT, ace.IsDirect);

                        WriteExtensions(writer, ace);
                        writer.WriteEndElement();
                    }
                }
            }

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        // -------------
        // --- query ---
        // -------------

        public static void WriteQuery(XmlWriter writer, CmisVersion cmisVersion, QueryType source)
        {
            if (source == null)
            {
                return;
            }

            writer.WriteStartElement(XmlConstants.TAG_QUERY, XmlConstants.NamespaceCmis);
            writer.WriteAttributeString("xmlns", XmlConstants.PrefixCmis, null, XmlConstants.NamespaceCmis);

            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_QUERY_STATEMENT, source.Statement);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_QUERY_SEARCHALLVERSIONS, source.SearchAllVersions);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_QUERY_INCLUDEALLOWABLEACTIONS, source.IncludeAllowableActions);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_QUERY_INCLUDERELATIONSHIPS, source.IncludeRelationships);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_QUERY_RENDITIONFILTER, source.RenditionFilter);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_QUERY_MAXITEMS, source.MaxItems);
            XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_QUERY_SKIPCOUNT, source.SkipCount);

            WriteExtensions(writer, source);
            writer.WriteEndElement();
        }

        // -------------------
        // --- bulk update ---
        // -------------------

        public static void WriteBulkUpdate(XmlWriter writer, string ns, BulkUpdate bulkUpdate)
        {
            if (bulkUpdate == null || bulkUpdate.ObjectIdAndChangeToken == null)
            {
                return;
            }

            writer.WriteStartElement(XmlConstants.TAG_BULK_UPDATE, ns);

            foreach (IBulkUpdateObjectIdAndChangeToken idAndToken in bulkUpdate.ObjectIdAndChangeToken)
            {
                if (idAndToken == null)
                {
                    continue;
                }

                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_BULK_UPDATE_ID_AND_TOKEN, XmlConstants.NamespaceCmis);

                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_IDANDTOKEN_ID, idAndToken.Id);
                XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_IDANDTOKEN_CHANGETOKEN, idAndToken.ChangeToken);

                WriteExtensions(writer, idAndToken);
                writer.WriteEndElement();
            }

            if (bulkUpdate.Properties != null)
            {
                IProperties properties = bulkUpdate.Properties;
                writer.WriteStartElement(XmlConstants.PrefixCmis, XmlConstants.TAG_BULK_UPDATE_PROPERTIES, XmlConstants.NamespaceCmis);

                if (properties.PropertyList != null)
                {
                    foreach (PropertyData property in properties.PropertyList)
                    {
                        WriteProperty(writer, property, false);
                    }
                }

                WriteExtensions(writer, properties);
                writer.WriteEndElement();
            }

            if (bulkUpdate.AddSecondaryTypeIds != null)
            {
                foreach (string id in bulkUpdate.AddSecondaryTypeIds)
                {
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_BULK_UPDATE_ADD_SECONDARY_TYPES, id);
                }
            }

            if (bulkUpdate.RemoveSecondaryTypeIds != null)
            {
                foreach (string id in bulkUpdate.RemoveSecondaryTypeIds)
                {
                    XmlUtils.Write(writer, XmlConstants.PrefixCmis, XmlConstants.NamespaceCmis, XmlConstants.TAG_BULK_UPDATE_REMOVE_SECONDARY_TYPES, id);
                }
            }

            writer.WriteEndElement();
        }

        // -------------------------
        // --- extension writers ---
        // -------------------------

        public static void WriteExtensions(XmlWriter writer, IExtensionsData source)
        {
            if (source == null)
            {
                return;
            }

            IList<string> ns = new List<string>();

            if (source.Extensions != null)
            {
                foreach (ICmisExtensionElement element in source.Extensions)
                {
                    if (element == null)
                    {
                        continue;
                    }

                    WriteExtensionElement(writer, element, ns);
                }
            }
        }

        private static void WriteExtensionElement(XmlWriter writer, ICmisExtensionElement source, IList<string> ns)
        {
            if (source == null || source.Name == null)
            {
                return;
            }

            bool addedNamespace = false;

            if (source.Namespace != null)
            {
                string prefix = writer.LookupPrefix(source.Namespace);
                if (prefix == null)
                {
                    int p = ns.IndexOf(source.Namespace);

                    if (p == -1)
                    {
                        prefix = "e" + (ns.Count + 1);
                        ns.Add(source.Namespace);
                        addedNamespace = true;
                    }
                    else
                    {
                        prefix = "e" + (p + 1);
                    }
                }

                writer.WriteStartElement(prefix, source.Name, source.Namespace);

                if (addedNamespace)
                {
                    writer.WriteAttributeString("xmlns", prefix, null, source.Namespace);
                }
            }
            else
            {
                writer.WriteStartElement(source.Name);
            }

            if (source.Attributes != null)
            {
                foreach (KeyValuePair<string, string> attr in source.Attributes)
                {
                    writer.WriteAttributeString(attr.Key, attr.Value);
                }
            }

            if (source.Value != null)
            {
                writer.WriteString(source.Value);
            }
            else
            {
                if (source.Children != null)
                {
                    foreach (ICmisExtensionElement child in source.Children)
                    {
                        WriteExtensionElement(writer, child, ns);
                    }
                }
            }

            writer.WriteEndElement();

            if (addedNamespace)
            {
                ns.RemoveAt(ns.Count - 1);
            }
        }

        // ---------------
        // --- parsers ---
        // ---------------

        public static RepositoryInfo ConvertRepositoryInfo(XmlReader parser)
        {
            return REPOSITORY_INFO_PARSER.Walk(parser);
        }

        public static ITypeDefinition ConvertTypeDefinition(XmlReader parser)
        {
            return TYPE_DEF_PARSER.Walk(parser);
        }

        public static IObjectData ConvertObject(XmlReader parser)
        {
            return OBJECT_PARSER.Walk(parser);
        }

        public static QueryType ConvertQuery(XmlReader parser)
        {
            return QUERY_PARSER.Walk(parser);
        }

        public static IAllowableActions ConvertAllowableActions(XmlReader parser)
        {
            return ALLOWABLE_ACTIONS_PARSER.Walk(parser);
        }

        public static IAcl ConvertAcl(XmlReader parser)
        {
            return ACL_PARSER.Walk(parser);
        }

        public static BulkUpdate ConvertBulkUpdate(XmlReader parser)
        {
            return BULK_UPDATE_PARSER.Walk(parser);
        }

        // ------------------------------
        // --- repository info parser ---
        // ------------------------------

        private static readonly RepositoryInfoParser REPOSITORY_INFO_PARSER = new RepositoryInfoParser();
        private class RepositoryInfoParser : XmlWalker<RepositoryInfo>
        {
            protected override RepositoryInfo PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new RepositoryInfo();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, RepositoryInfo target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_REPINFO_ID))
                    {
                        target.Id = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_NAME))
                    {
                        target.Name = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_DESCRIPTION))
                    {
                        target.Description = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_VENDOR))
                    {
                        target.VendorName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_PRODUCT))
                    {
                        target.ProductName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_PRODUCT_VERSION))
                    {
                        target.ProductVersion = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_ROOT_FOLDER_ID))
                    {
                        target.RootFolderId = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_CHANGE_LOG_TOKEN))
                    {
                        target.LatestChangeLogToken = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_CAPABILITIES))
                    {
                        target.Capabilities = CAPABILITIES_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_ACL_CAPABILITIES))
                    {
                        target.AclCapabilities = ACL_CAPABILITIES_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_CMIS_VERSION_SUPPORTED))
                    {
                        target.CmisVersionSupported = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_THIN_CLIENT_URI))
                    {
                        target.ThinClientUri = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_CHANGES_INCOMPLETE))
                    {
                        target.ChangesIncomplete = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_CHANGES_ON_TYPE))
                    {
                        target.ChangesOnType = AddToList(target.ChangesOnType, ReadEnum<BaseTypeId>(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_PRINCIPAL_ID_ANONYMOUS))
                    {
                        target.PrincipalIdAnonymous = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_PRINCIPAL_ID_ANYONE))
                    {
                        target.PrincipalIdAnyone = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_REPINFO_EXTENDED_FEATURES))
                    {
                        target.ExtensionFeatures = AddToList(target.ExtensionFeatures, EXTENDED_FEATURES_PARSER.Walk(parser));
                        return true;
                    }
                }

                return false;
            }
        };

        private static readonly CapabilitiesParser CAPABILITIES_PARSER = new CapabilitiesParser();
        private class CapabilitiesParser : XmlWalker<RepositoryCapabilities>
        {
            protected override RepositoryCapabilities PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new RepositoryCapabilities();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, RepositoryCapabilities target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_CAP_ACL))
                    {
                        target.AclCapability = ReadEnum<CapabilityAcl>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_ALL_VERSIONS_SEARCHABLE))
                    {
                        target.IsAllVersionsSearchableSupported = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_CHANGES))
                    {
                        target.ChangesCapability = ReadEnum<CapabilityChanges>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_CONTENT_STREAM_UPDATABILITY))
                    {
                        target.ContentStreamUpdatesCapability = ReadEnum<CapabilityContentStreamUpdates>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_GET_DESCENDANTS))
                    {
                        target.IsGetDescendantsSupported = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_GET_FOLDER_TREE))
                    {
                        target.IsGetFolderTreeSupported = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_ORDER_BY))
                    {
                        target.OrderByCapability = ReadEnum<CapabilityOrderBy>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_MULTIFILING))
                    {
                        target.IsMultifilingSupported = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_PWC_SEARCHABLE))
                    {
                        target.IsPwcSearchableSupported = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_PWC_UPDATABLE))
                    {
                        target.IsPwcUpdatableSupported = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_QUERY))
                    {
                        target.QueryCapability = ReadEnum<CapabilityQuery>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_RENDITIONS))
                    {
                        target.RenditionsCapability = ReadEnum<CapabilityRenditions>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_UNFILING))
                    {
                        target.IsUnfilingSupported = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_VERSION_SPECIFIC_FILING))
                    {
                        target.IsVersionSpecificFilingSupported = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_JOIN))
                    {
                        target.JoinCapability = ReadEnum<CapabilityJoin>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_CREATABLE_PROPERTY_TYPES))
                    {
                        target.CreatablePropertyTypes = CREATABLE_PROPERTY_TYPES_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES))
                    {
                        target.NewTypeSettableAttributes = NEW_TYPES_SETTABLE_ATTRIBUTES_PARSER.Walk(parser);
                        return true;
                    }
                }

                return false;
            }
        };

        private static readonly CreatablePropertyTypesParser CREATABLE_PROPERTY_TYPES_PARSER = new CreatablePropertyTypesParser();
        private class CreatablePropertyTypesParser : XmlWalker<CreatablePropertyTypes>
        {
            protected override CreatablePropertyTypes PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new CreatablePropertyTypes();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, CreatablePropertyTypes target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_CAP_CREATABLE_PROPERTY_TYPES_CANCREATE))
                    {
                        if (target.CanCreate == null)
                        {
                            target.CanCreate = new HashSet<PropertyType>();
                        }
                        target.CanCreate.Add(ReadEnum<PropertyType>(parser));
                        return true;
                    }
                }
                return false;
            }
        };

        private static readonly NewTypesSettableAttributesParser NEW_TYPES_SETTABLE_ATTRIBUTES_PARSER = new NewTypesSettableAttributesParser();
        private class NewTypesSettableAttributesParser : XmlWalker<NewTypeSettableAttributes>
        {
            protected override NewTypeSettableAttributes PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new NewTypeSettableAttributes();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, NewTypeSettableAttributes target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_ID))
                    {
                        target.CanSetId = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_LOCALNAME))
                    {
                        target.CanSetLocalName = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_LOCALNAMESPACE))
                    {
                        target.CanSetLocalNamespace = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_DISPLAYNAME))
                    {
                        target.CanSetDisplayName = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_QUERYNAME))
                    {
                        target.CanSetQueryName = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_DESCRIPTION))
                    {
                        target.CanSetDescription = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CREATEABLE))
                    {
                        target.CanSetCreatable = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_FILEABLE))
                    {
                        target.CanSetFileable = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_QUERYABLE))
                    {
                        target.CanSetQueryable = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_FULLTEXTINDEXED))
                    {
                        target.CanSetFulltextIndexed = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_INCLUDEDINSUPERTYTPEQUERY))
                    {
                        target.CanSetIncludedInSupertypeQuery = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CONTROLABLEPOLICY))
                    {
                        target.CanSetControllablePolicy = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CAP_NEW_TYPE_SETTABLE_ATTRIBUTES_CONTROLABLEACL))
                    {
                        target.CanSetControllableAcl = ReadBoolean(parser);
                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly AclCapabilitiesParser ACL_CAPABILITIES_PARSER = new AclCapabilitiesParser();
        private class AclCapabilitiesParser : XmlWalker<AclCapabilities>
        {
            protected override AclCapabilities PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new AclCapabilities();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, AclCapabilities target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_ACLCAP_SUPPORTED_PERMISSIONS))
                    {
                        target.SupportedPermissions = ReadEnum<SupportedPermissions>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_ACLCAP_ACL_PROPAGATION))
                    {
                        target.AclPropagation = ReadEnum<AclPropagation>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_ACLCAP_PERMISSIONS))
                    {
                        target.Permissions = AddToList(target.Permissions, PERMISSION_DEFINITION_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_ACLCAP_PERMISSION_MAPPING))
                    {
                        PermissionMapping pm = PERMISSION_MAPPING_PARSER.Walk(parser);

                        IDictionary<string, IPermissionMapping> mapping = target.PermissionMapping;
                        if (mapping == null)
                        {
                            target.PermissionMapping = new Dictionary<string, IPermissionMapping>();
                            mapping = target.PermissionMapping;
                        }
                        mapping.Add(pm.Key, pm);

                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly PermissionDefinitionParser PERMISSION_DEFINITION_PARSER = new PermissionDefinitionParser();
        private class PermissionDefinitionParser : XmlWalker<PermissionDefinition>
        {
            protected override PermissionDefinition PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new PermissionDefinition();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, PermissionDefinition target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_ACLCAP_PERMISSION_PERMISSION))
                    {
                        target.Id = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_ACLCAP_PERMISSION_DESCRIPTION))
                    {
                        target.Description = ReadText(parser);
                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly PermissionMappingParser PERMISSION_MAPPING_PARSER = new PermissionMappingParser();
        private class PermissionMappingParser : XmlWalker<PermissionMapping>
        {
            protected override PermissionMapping PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new PermissionMapping();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, PermissionMapping target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_ACLCAP_MAPPING_KEY))
                    {
                        target.Key = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_ACLCAP_MAPPING_PERMISSION))
                    {
                        target.Permissions = AddToList(target.Permissions, ReadText(parser));
                        return true;
                    }
                }

                return false;
            }
        };

        private static readonly ExtensionFeatureParser EXTENDED_FEATURES_PARSER = new ExtensionFeatureParser();
        private class ExtensionFeatureParser : XmlWalker<ExtensionFeature>
        {
            protected override ExtensionFeature PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new ExtensionFeature();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, ExtensionFeature target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_FEATURE_ID))
                    {
                        target.Id = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_FEATURE_URL))
                    {
                        target.Url = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_FEATURE_COMMON_NAME))
                    {
                        target.CommonName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_FEATURE_VERSION_LABEL))
                    {
                        target.VersionLabel = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_FEATURE_DESCRIPTION))
                    {
                        target.Description = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_FEATURE_DATA))
                    {
                        String[] data = FEATURE_DATA_PARSER.Walk(parser);

                        IDictionary<string, string> featureData = target.FeatureData;
                        featureData.Add(data[0], data[1]);

                        return true;
                    }
                }

                return false;
            }
        };

        private static readonly FetaureDataParser FEATURE_DATA_PARSER = new FetaureDataParser();
        private class FetaureDataParser : XmlWalker<string[]>
        {
            protected override string[] PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new string[2];
            }

            protected override bool Read(XmlReader parser, string localname, string ns, string[] target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_FEATURE_DATA_KEY))
                    {
                        target[0] = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_FEATURE_DATA_VALUE))
                    {
                        target[1] = ReadText(parser);
                        return true;
                    }
                }

                return false;
            }
        };

        // --------------------------
        // --- definition parsers ---
        // --------------------------

        private static readonly TypeDefParser TYPE_DEF_PARSER = new TypeDefParser();
        private class TypeDefParser : XmlWalker<AbstractTypeDefinition>
        {
            protected override AbstractTypeDefinition PrepareTarget(XmlReader parser, string localname, string ns)
            {
                AbstractTypeDefinition result = null;

                string typeAttr = parser.GetAttribute("type", XmlConstants.NamespaceXsi);
                if (typeAttr != null)
                {
                    if (typeAttr.EndsWith(XmlConstants.ATTR_DOCUMENT_TYPE, StringComparison.Ordinal))
                    {
                        result = new DocumentTypeDefinition();
                    }
                    else if (typeAttr.EndsWith(XmlConstants.ATTR_FOLDER_TYPE, StringComparison.Ordinal))
                    {
                        result = new FolderTypeDefinition();
                    }
                    else if (typeAttr.EndsWith(XmlConstants.ATTR_RELATIONSHIP_TYPE, StringComparison.Ordinal))
                    {
                        result = new RelationshipTypeDefinition();
                        ((RelationshipTypeDefinition)result).AllowedSourceTypeIds = new List<string>();
                        ((RelationshipTypeDefinition)result).AllowedTargetTypeIds = new List<string>();
                    }
                    else if (typeAttr.EndsWith(XmlConstants.ATTR_POLICY_TYPE, StringComparison.Ordinal))
                    {
                        result = new PolicyTypeDefinition();
                    }
                    else if (typeAttr.EndsWith(XmlConstants.ATTR_ITEM_TYPE, StringComparison.Ordinal))
                    {
                        result = new ItemTypeDefinition();
                    }
                    else if (typeAttr.EndsWith(XmlConstants.ATTR_SECONDARY_TYPE, StringComparison.Ordinal))
                    {
                        result = new SecondaryTypeDefinition();
                    }
                }

                if (result == null)
                {
                    throw new CmisInvalidArgumentException("Cannot read type definition!");
                }

                return result;
            }

            protected override bool Read(XmlReader parser, string localname, string ns, AbstractTypeDefinition target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_TYPE_ID))
                    {
                        target.Id = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_LOCALNAME))
                    {
                        target.LocalName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_LOCALNAMESPACE))
                    {
                        target.LocalNamespace = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_DISPLAYNAME))
                    {
                        target.DisplayName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_QUERYNAME))
                    {
                        target.QueryName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_DESCRIPTION))
                    {
                        target.Description = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_BASE_ID))
                    {
                        BaseTypeId? baseType = ReadEnum<BaseTypeId?>(parser);
                        if (baseType == null)
                        {
                            throw new CmisInvalidArgumentException("Invalid base type!");
                        }

                        target.BaseTypeId = baseType.Value;
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_PARENT_ID))
                    {
                        target.ParentTypeId = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_CREATABLE))
                    {
                        target.IsCreatable = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_FILEABLE))
                    {
                        target.IsFileable = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_QUERYABLE))
                    {
                        target.IsQueryable = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_FULLTEXT_INDEXED))
                    {
                        target.IsFulltextIndexed = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_INCLUDE_IN_SUPERTYPE_QUERY))
                    {
                        target.IsIncludedInSupertypeQuery = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_CONTROLABLE_POLICY))
                    {
                        target.IsControllablePolicy = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_CONTROLABLE_ACL))
                    {
                        target.IsControllableAcl = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_TYPE_MUTABILITY))
                    {
                        target.TypeMutability = TYPE_MUTABILITY_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_STRING) || IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_ID)
                            || IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_BOOLEAN) || IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_INTEGER)
                            || IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_DATETIME) || IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_DECIMAL)
                            || IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_HTML) || IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_URI))
                    {
                        target.AddPropertyDefinition(PROPERTY_TYPE_PARSER.Walk(parser));
                        return true;
                    }

                    if (target is DocumentTypeDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_TYPE_VERSIONABLE))
                        {
                            ((DocumentTypeDefinition)target).IsVersionable = ReadBoolean(parser);
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_TYPE_CONTENTSTREAM_ALLOWED))
                        {
                            ((DocumentTypeDefinition)target).ContentStreamAllowed = ReadEnum<ContentStreamAllowed>(parser);
                            return true;
                        }
                    }

                    if (target is RelationshipTypeDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_TYPE_ALLOWED_SOURCE_TYPES))
                        {
                            RelationshipTypeDefinition relTarget = (RelationshipTypeDefinition)target;
                            relTarget.AllowedSourceTypeIds = AddToList(relTarget.AllowedSourceTypeIds, ReadText(parser));
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_TYPE_ALLOWED_TARGET_TYPES))
                        {
                            RelationshipTypeDefinition relTarget = (RelationshipTypeDefinition)target;
                            relTarget.AllowedTargetTypeIds = AddToList(relTarget.AllowedTargetTypeIds, ReadText(parser));
                            return true;
                        }
                    }
                }

                return false;
            }
        };


        private static readonly TypeMutabilityParser TYPE_MUTABILITY_PARSER = new TypeMutabilityParser();
        private class TypeMutabilityParser : XmlWalker<TypeMutability>
        {
            protected override TypeMutability PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new TypeMutability();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, TypeMutability target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_TYPE_TYPE_MUTABILITY_CREATE))
                    {
                        target.CanCreate = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_TYPE_MUTABILITY_UPDATE))
                    {
                        target.CanUpdate = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_TYPE_TYPE_MUTABILITY_DELETE))
                    {
                        target.CanDelete = ReadBoolean(parser);
                        return true;
                    }
                }

                return false;
            }
        };

        private static readonly PropertyTypeParser PROPERTY_TYPE_PARSER = new PropertyTypeParser();
        private class PropertyTypeParser : XmlWalker<PropertyDefinition>
        {
            protected override PropertyDefinition PrepareTarget(XmlReader parser, string localname, string ns)
            {
                PropertyDefinition result = null;

                if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_STRING))
                {
                    result = new PropertyStringDefinition();
                }
                else if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_ID))
                {
                    result = new PropertyIdDefinition();
                }
                else if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_BOOLEAN))
                {
                    result = new PropertyBooleanDefinition();
                }
                else if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_INTEGER))
                {
                    result = new PropertyIntegerDefinition();
                }
                else if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_DATETIME))
                {
                    result = new PropertyDateTimeDefinition();
                }
                else if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_DECIMAL))
                {
                    result = new PropertyDecimalDefinition();
                }
                else if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_HTML))
                {
                    result = new PropertyHtmlDefinition();
                }
                else if (IsTag(localname, XmlConstants.TAG_TYPE_PROP_DEF_URI))
                {
                    result = new PropertyUriDefinition();
                }

                if (result == null)
                {
                    throw new CmisInvalidArgumentException("Cannot read property type definition!");
                }

                return result;
            }

            protected override bool Read(XmlReader parser, string localname, string ns, PropertyDefinition target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_ID))
                    {
                        target.Id = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_LOCALNAME))
                    {
                        target.LocalName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_LOCALNAMESPACE))
                    {
                        target.LocalNamespace = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DISPLAYNAME))
                    {
                        target.DisplayName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_QUERYNAME))
                    {
                        target.QueryName = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DESCRIPTION))
                    {
                        target.Description = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_PROPERTY_TYPE))
                    {
                        PropertyType? propType = ReadEnum<PropertyType?>(parser);
                        if (propType == null)
                        {
                            throw new CmisInvalidArgumentException("Invalid property data type!");
                        }

                        target.PropertyType = propType.Value;
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CARDINALITY))
                    {
                        Cardinality? cardinality = ReadEnum<Cardinality?>(parser);
                        if (cardinality == null)
                        {
                            throw new CmisInvalidArgumentException("Invalid cardinality!");
                        }

                        target.Cardinality = cardinality.Value;
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_UPDATABILITY))
                    {
                        Updatability? updatability = ReadEnum<Updatability?>(parser);
                        if (updatability == null)
                        {
                            throw new CmisInvalidArgumentException("Invalid updatability!");
                        }

                        target.Updatability = updatability.Value;
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_INHERITED))
                    {
                        target.IsInherited = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_REQUIRED))
                    {
                        target.IsRequired = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_QUERYABLE))
                    {
                        target.IsQueryable = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_ORDERABLE))
                    {
                        target.IsOrderable = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_OPENCHOICE))
                    {
                        target.IsOpenChoice = ReadBoolean(parser);
                        return true;
                    }

                    if (target is PropertyStringDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE))
                        {
                            PropertyData prop = PROPERTY_STRING_PARSER.Walk(parser);
                            if (prop.Values != null)
                            {
                                ((PropertyStringDefinition)target).DefaultValue = prop.Values.Cast<string>().ToList();
                            }
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE))
                        {
                            CHOICE_STRING_PARSER.addToChoiceList(parser, (PropertyStringDefinition)target);
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_MAX_LENGTH))
                        {
                            ((PropertyStringDefinition)target).MaxLength = ReadInteger(parser);
                            return true;
                        }
                    }
                    else if (target is PropertyIdDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE))
                        {
                            PropertyData prop = PROPERTY_ID_PARSER.Walk(parser);
                            if (prop.Values != null)
                            {
                                ((PropertyIdDefinition)target).DefaultValue = prop.Values.Cast<string>().ToList();
                            }
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE))
                        {
                            CHOICE_STRING_PARSER.addToChoiceList(parser, (PropertyIdDefinition)target);
                            return true;
                        }
                    }
                    else if (target is PropertyBooleanDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE))
                        {
                            PropertyData prop = PROPERTY_BOOLEAN_PARSER.Walk(parser);
                            if (prop.Values != null)
                            {
                                ((PropertyBooleanDefinition)target).DefaultValue = prop.Values.Cast<bool?>().ToList();
                            }
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE))
                        {
                            CHOICE_BOOLEAN_PARSER.addToChoiceList(parser, (PropertyBooleanDefinition)target);
                            return true;
                        }
                    }
                    else if (target is PropertyIntegerDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE))
                        {
                            PropertyData prop = PROPERTY_INTEGER_PARSER.Walk(parser);
                            if (prop.Values != null)
                            {
                                ((PropertyIntegerDefinition)target).DefaultValue = prop.Values.Cast<BigInteger?>().ToList();
                            }
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE))
                        {
                            CHOICE_INTEGER_PARSER.addToChoiceList(parser, (PropertyIntegerDefinition)target);
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_MAX_VALUE))
                        {
                            ((PropertyIntegerDefinition)target).MaxValue = ReadInteger(parser);
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_MIN_VALUE))
                        {
                            ((PropertyIntegerDefinition)target).MinValue = ReadInteger(parser);
                            return true;
                        }
                    }
                    else if (target is PropertyDateTimeDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE))
                        {
                            PropertyData prop = PROPERTY_DATETIME_PARSER.Walk(parser);
                            if (prop.Values != null)
                            {
                                ((PropertyDateTimeDefinition)target).DefaultValue = prop.Values.Cast<DateTime?>().ToList();
                            }
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE))
                        {
                            CHOICE_DATETIME_PARSER.addToChoiceList(parser, (PropertyDateTimeDefinition)target);
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_RESOLUTION))
                        {
                            ((PropertyDateTimeDefinition)target).DateTimeResolution = ReadEnum<DateTimeResolution>(parser);
                            return true;
                        }
                    }
                    else if (target is PropertyDecimalDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE))
                        {
                            PropertyData prop = PROPERTY_DECIMAL_PARSER.Walk(parser);
                            if (prop.Values != null)
                            {
                                ((PropertyDecimalDefinition)target).DefaultValue = prop.Values.Cast<decimal?>().ToList();
                            }
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE))
                        {
                            CHOICE_DECIMAL_PARSER.addToChoiceList(parser, (PropertyDecimalDefinition)target);
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_MAX_VALUE))
                        {
                            ((PropertyDecimalDefinition)target).MaxValue = ReadDecimal(parser);
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_MIN_VALUE))
                        {
                            ((PropertyDecimalDefinition)target).MinValue = ReadDecimal(parser);
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_PRECISION))
                        {
                            ((PropertyDecimalDefinition)target).Precision = ReadEnum<DecimalPrecision>(parser);
                            return true;
                        }
                    }
                    else if (target is PropertyHtmlDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE))
                        {
                            PropertyData prop = PROPERTY_HTML_PARSER.Walk(parser);
                            if (prop.Values != null)
                            {
                                ((PropertyHtmlDefinition)target).DefaultValue = prop.Values.Cast<string>().ToList();
                            }
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE))
                        {
                            CHOICE_STRING_PARSER.addToChoiceList(parser, (PropertyHtmlDefinition)target);
                            return true;
                        }
                    }
                    else if (target is IPropertyUriDefinition)
                    {
                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_DEAULT_VALUE))
                        {
                            PropertyData prop = PROPERTY_URI_PARSER.Walk(parser);
                            if (prop.Values != null)
                            {
                                ((PropertyUriDefinition)target).DefaultValue = prop.Values.Cast<string>().ToList();
                            }
                            return true;
                        }

                        if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE))
                        {
                            CHOICE_STRING_PARSER.addToChoiceList(parser, (PropertyUriDefinition)target);
                            return true;
                        }
                    }
                }

                return false;
            }
        };

        private static readonly ChoiceStringParser CHOICE_STRING_PARSER = new ChoiceStringParser();
        private class ChoiceStringParser : ChoiceXmlWalker<string>
        {
            protected override Choice<string> CreateTarget(XmlReader parser, string localname, string ns)
            {
                return new Choice<string>();
            }

            protected override void addValue(XmlReader parser, Choice<string> target)
            {
                target.Value = AddToList(target.Value, ReadText(parser));
            }

            protected override void addChoice(XmlReader parser, Choice<string> target)
            {
                target.Choices = AddToList(target.Choices, CHOICE_STRING_PARSER.Walk(parser));
            }

            public void addToChoiceList(XmlReader parser, PropertyDefinition propDef)
            {
                if (propDef is PropertyStringDefinition)
                {
                    PropertyStringDefinition pd = (PropertyStringDefinition)propDef;
                    pd.Choices = AddToList(pd.Choices, CHOICE_STRING_PARSER.Walk(parser));
                }
                else if (propDef is PropertyIdDefinition)
                {
                    PropertyIdDefinition pd = (PropertyIdDefinition)propDef;
                    pd.Choices = AddToList(pd.Choices, CHOICE_STRING_PARSER.Walk(parser));
                }
                else if (propDef is PropertyUriDefinition)
                {
                    PropertyUriDefinition pd = (PropertyUriDefinition)propDef;
                    pd.Choices = AddToList(pd.Choices, CHOICE_STRING_PARSER.Walk(parser));
                }
                else if (propDef is PropertyHtmlDefinition)
                {
                    PropertyHtmlDefinition pd = (PropertyHtmlDefinition)propDef;
                    pd.Choices = AddToList(pd.Choices, CHOICE_STRING_PARSER.Walk(parser));
                }
            }
        };

        private static readonly ChoiceBooleanParser CHOICE_BOOLEAN_PARSER = new ChoiceBooleanParser();
        private class ChoiceBooleanParser : ChoiceXmlWalker<bool?>
        {
            protected override Choice<bool?> CreateTarget(XmlReader parser, string localname, string ns)
            {
                return new Choice<bool?>();
            }

            protected override void addValue(XmlReader parser, Choice<bool?> target)
            {
                target.Value = AddToList(target.Value, ReadBoolean(parser));
            }

            protected override void addChoice(XmlReader parser, Choice<bool?> target)
            {
                target.Choices = AddToList(target.Choices, CHOICE_BOOLEAN_PARSER.Walk(parser));
            }

            public void addToChoiceList(XmlReader parser, PropertyBooleanDefinition propDef)
            {
                propDef.Choices = AddToList(propDef.Choices, CHOICE_BOOLEAN_PARSER.Walk(parser));
            }
        };

        private static readonly ChoiceIntegerParser CHOICE_INTEGER_PARSER = new ChoiceIntegerParser();
        private class ChoiceIntegerParser : ChoiceXmlWalker<BigInteger?>
        {
            protected override Choice<BigInteger?> CreateTarget(XmlReader parser, string localname, string ns)
            {
                return new Choice<BigInteger?>();
            }

            protected override void addValue(XmlReader parser, Choice<BigInteger?> target)
            {
                target.Value = AddToList(target.Value, ReadInteger(parser));
            }

            protected override void addChoice(XmlReader parser, Choice<BigInteger?> target)
            {
                target.Choices = AddToList(target.Choices, CHOICE_INTEGER_PARSER.Walk(parser));
            }

            public void addToChoiceList(XmlReader parser, PropertyIntegerDefinition propDef)
            {
                propDef.Choices = AddToList(propDef.Choices, CHOICE_INTEGER_PARSER.Walk(parser));
            }
        };

        private static readonly ChoiceDateTimeParser CHOICE_DATETIME_PARSER = new ChoiceDateTimeParser();
        private class ChoiceDateTimeParser : ChoiceXmlWalker<DateTime?>
        {
            protected override Choice<DateTime?> CreateTarget(XmlReader parser, string localname, string ns)
            {
                return new Choice<DateTime?>();
            }

            protected override void addValue(XmlReader parser, Choice<DateTime?> target)
            {
                target.Value = AddToList(target.Value, ReadDateTime(parser));
            }

            protected override void addChoice(XmlReader parser, Choice<DateTime?> target)
            {
                target.Choices = AddToList(target.Choices, CHOICE_DATETIME_PARSER.Walk(parser));
            }

            public void addToChoiceList(XmlReader parser, PropertyDateTimeDefinition propDef)
            {
                propDef.Choices = AddToList(propDef.Choices, CHOICE_DATETIME_PARSER.Walk(parser));
            }
        };

        private static readonly ChoiceDecimalParser CHOICE_DECIMAL_PARSER = new ChoiceDecimalParser();
        private class ChoiceDecimalParser : ChoiceXmlWalker<decimal?>
        {
            protected override Choice<decimal?> CreateTarget(XmlReader parser, string localname, string ns)
            {
                return new Choice<decimal?>();
            }

            protected override void addValue(XmlReader parser, Choice<decimal?> target)
            {
                target.Value = AddToList(target.Value, ReadDecimal(parser));
            }

            protected override void addChoice(XmlReader parser, Choice<decimal?> target)
            {
                target.Choices = AddToList(target.Choices, CHOICE_DECIMAL_PARSER.Walk(parser));
            }

            public void addToChoiceList(XmlReader parser, PropertyDecimalDefinition propDef)
            {
                propDef.Choices = AddToList(propDef.Choices, CHOICE_DECIMAL_PARSER.Walk(parser));
            }
        };

        private abstract class ChoiceXmlWalker<T> : XmlWalker<Choice<T>>
        {
            protected abstract Choice<T> CreateTarget(XmlReader parser, string localname, string ns);

            protected override Choice<T> PrepareTarget(XmlReader parser, string localname, string ns)
            {
                Choice<T> result = CreateTarget(parser, localname, ns);

                if (parser.HasAttributes)
                {
                    result.DisplayName = parser.GetAttribute(XmlConstants.ATTR_PROPERTY_TYPE_CHOICE_DISPLAYNAME);
                }

                return result;
            }

            protected override bool Read(XmlReader parser, string localname, string ns, Choice<T> target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE_VALUE))
                    {
                        try
                        {
                            addValue(parser, target);
                        }
                        catch (CmisInvalidArgumentException e)
                        {
                            // a few repositories send invalid values here
                            if (Logger.IsWarnEnabled)
                            {
                                Logger.Warn("Found invalid choice value for choice entry \"" + target.DisplayName + "\"!", e);
                            }
                        }
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_TYPE_CHOICE_CHOICE))
                    {
                        addChoice(parser, target);
                        return true;
                    }
                }

                return false;
            }

            protected abstract void addValue(XmlReader parser, Choice<T> target);

            protected abstract void addChoice(XmlReader parser, Choice<T> target);
        }

        // ---------------------------------
        // --- objects and lists parsers ---
        // ---------------------------------

        private static readonly ObjectParser OBJECT_PARSER = new ObjectParser();
        private class ObjectParser : XmlWalker<ObjectData>
        {

            protected override ObjectData PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new ObjectData();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, ObjectData target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_OBJECT_PROPERTIES))
                    {
                        target.Properties = PROPERTIES_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_OBJECT_ALLOWABLE_ACTIONS))
                    {
                        target.AllowableActions = ALLOWABLE_ACTIONS_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_OBJECT_RELATIONSHIP))
                    {
                        target.Relationships = AddToList(target.Relationships, OBJECT_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_OBJECT_CHANGE_EVENT_INFO))
                    {
                        target.ChangeEventInfo = CHANGE_EVENT_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_OBJECT_ACL))
                    {
                        target.Acl = ACL_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_OBJECT_EXACT_ACL))
                    {
                        target.IsExactAcl = ReadBoolean(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_OBJECT_POLICY_IDS))
                    {
                        target.PolicyIds = POLICY_IDS_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_OBJECT_RENDITION))
                    {
                        target.Renditions = AddToList(target.Renditions, RENDITION_PARSER.Walk(parser));
                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly PropertiesParser PROPERTIES_PARSER = new PropertiesParser();
        private class PropertiesParser : XmlWalker<Properties>
        {

            protected override Properties PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new Properties();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, Properties target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_PROP_STRING))
                    {
                        target.AddProperty(PROPERTY_STRING_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROP_ID))
                    {
                        target.AddProperty(PROPERTY_ID_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROP_BOOLEAN))
                    {
                        target.AddProperty(PROPERTY_BOOLEAN_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROP_INTEGER))
                    {
                        target.AddProperty(PROPERTY_INTEGER_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROP_DATETIME))
                    {
                        target.AddProperty(PROPERTY_DATETIME_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROP_DECIMAL))
                    {
                        target.AddProperty(PROPERTY_DECIMAL_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROP_HTML))
                    {
                        target.AddProperty(PROPERTY_HTML_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_PROP_URI))
                    {
                        target.AddProperty(PROPERTY_URI_PARSER.Walk(parser));
                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly AllowableActionsParser ALLOWABLE_ACTIONS_PARSER = new AllowableActionsParser();
        private class AllowableActionsParser : XmlWalker<AllowableActions>
        {

            protected override AllowableActions PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new AllowableActions();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, AllowableActions target)
            {
                if (IsCmisNamespace(ns))
                {
                    PortCMIS.Enums.Action? action = localname.GetCmisEnum<PortCMIS.Enums.Action?>();
                    if (action != null)
                    {
                        ISet<PortCMIS.Enums.Action> actions = target.Actions;
                        if (actions == null)
                        {
                            target.Actions = new HashSet<PortCMIS.Enums.Action>();
                            actions = target.Actions;
                        }

                        if (ReadBoolean(parser) == true)
                        {
                            actions.Add(action.Value);
                        }

                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly ChangeEventParser CHANGE_EVENT_PARSER = new ChangeEventParser();
        private class ChangeEventParser : XmlWalker<ChangeEventInfo>
        {
            protected override ChangeEventInfo PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new ChangeEventInfo();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, ChangeEventInfo target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_CHANGE_EVENT_TYPE))
                    {
                        target.ChangeType = ReadEnum<ChangeType>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_CHANGE_EVENT_TIME))
                    {
                        target.ChangeTime = ReadDateTime(parser);
                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly AclParser ACL_PARSER = new AclParser();
        private class AclParser : XmlWalker<Acl>
        {
            protected override Acl PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new Acl();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, Acl target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_ACL_PERMISSISONS))
                    {
                        target.Aces = AddToList(target.Aces, ACE_PARSER.Walk(parser));
                        return true;
                    }
                }

                return false;
            }
        };

        private static readonly AceParser ACE_PARSER = new AceParser();
        private class AceParser : XmlWalker<Ace>
        {
            protected override Ace PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new Ace();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, Ace target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_ACE_PRINCIPAL))
                    {
                        target.Principal = PRINCIPAL_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_ACE_PERMISSIONS))
                    {
                        target.Permissions = AddToList(target.Permissions, ReadText(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_ACE_IS_DIRECT))
                    {
                        bool? isDirect = ReadBoolean(parser);
                        target.IsDirect = (isDirect != null ? (bool)isDirect : true);

                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly PrincipalParser PRINCIPAL_PARSER = new PrincipalParser();
        private class PrincipalParser : XmlWalker<Principal>
        {
            protected override Principal PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new Principal();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, Principal target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_ACE_PRINCIPAL_ID))
                    {
                        target.Id = ReadText(parser);
                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly PolicyIdsParser POLICY_IDS_PARSER = new PolicyIdsParser();
        private class PolicyIdsParser : XmlWalker<PolicyIdList>
        {
            protected override PolicyIdList PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new PolicyIdList();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, PolicyIdList target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_POLICY_ID))
                    {
                        target.PolicyIds = AddToList(target.PolicyIds, ReadText(parser));
                        return true;
                    }
                }

                return false;
            }
        };

        private static readonly RenditionParser RENDITION_PARSER = new RenditionParser();
        private class RenditionParser : XmlWalker<RenditionData>
        {
            protected override RenditionData PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new RenditionData();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, RenditionData target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_RENDITION_STREAM_ID))
                    {
                        target.StreamId = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_RENDITION_MIMETYPE))
                    {
                        target.MimeType = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_RENDITION_LENGTH))
                    {
                        target.Length = ReadInteger(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_RENDITION_KIND))
                    {
                        target.Kind = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_RENDITION_TITLE))
                    {
                        target.Title = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_RENDITION_HEIGHT))
                    {
                        target.Height = ReadInteger(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_RENDITION_WIDTH))
                    {
                        target.Width = ReadInteger(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_RENDITION_DOCUMENT_ID))
                    {
                        target.RenditionDocumentId = ReadText(parser);
                        return true;
                    }
                }

                return false;
            }
        };

        // ------------------------
        // --- property parsers ---
        // ------------------------


        private static readonly PropertyXmlWalker PROPERTY_STRING_PARSER = new PropertyStringParser();
        private class PropertyStringParser : PropertyStringXmlWalker
        {
            protected override PropertyData createTarget(XmlReader parser, string localname, string ns)
            {
                return new PropertyData(PropertyType.String);
            }
        };

        private static readonly PropertyXmlWalker PROPERTY_ID_PARSER = new PropertyIdParser();
        private class PropertyIdParser : PropertyStringXmlWalker
        {
            protected override PropertyData createTarget(XmlReader parser, string localname, string ns)
            {
                return new PropertyData(PropertyType.Id);
            }
        };

        private static readonly PropertyXmlWalker PROPERTY_HTML_PARSER = new PropertyHtmlParser();
        private class PropertyHtmlParser : PropertyStringXmlWalker
        {
            protected override PropertyData createTarget(XmlReader parser, string localname, string ns)
            {
                return new PropertyData(PropertyType.Html);
            }
        };

        private static readonly PropertyXmlWalker PROPERTY_URI_PARSER = new PropertyUriParser();
        private class PropertyUriParser : PropertyStringXmlWalker
        {
            protected override PropertyData createTarget(XmlReader parser, string localname, string ns)
            {
                return new PropertyData(PropertyType.Uri);
            }
        };

        private static readonly PropertyXmlWalker PROPERTY_BOOLEAN_PARSER = new PropertyBooleanParser();
        private class PropertyBooleanParser : PropertyXmlWalker
        {
            protected override PropertyData createTarget(XmlReader parser, string localname, string ns)
            {
                return new PropertyData(PropertyType.Boolean);
            }

            protected override void addValue(XmlReader parser, PropertyData target)
            {
                target.Values = AddToList(target.Values, ReadBoolean(parser));
            }
        };

        private static readonly PropertyXmlWalker PROPERTY_INTEGER_PARSER = new PropertyIntegerParser();
        private class PropertyIntegerParser : PropertyXmlWalker
        {
            protected override PropertyData createTarget(XmlReader parser, string localname, string ns)
            {
                return new PropertyData(PropertyType.Integer);
            }

            protected override void addValue(XmlReader parser, PropertyData target)
            {
                target.Values = AddToList(target.Values, ReadInteger(parser));
            }
        };

        private static readonly PropertyXmlWalker PROPERTY_DECIMAL_PARSER = new PropertyDecimalParser();
        private class PropertyDecimalParser : PropertyXmlWalker
        {
            protected override PropertyData createTarget(XmlReader parser, string localname, string ns)
            {
                return new PropertyData(PropertyType.Decimal);
            }

            protected override void addValue(XmlReader parser, PropertyData target)
            {
                target.Values = AddToList(target.Values, ReadDecimal(parser));
            }
        };

        private static readonly PropertyXmlWalker PROPERTY_DATETIME_PARSER = new PropertyDateTimeParser();
        private class PropertyDateTimeParser : PropertyXmlWalker
        {
            protected override PropertyData createTarget(XmlReader parser, string localname, string ns)
            {
                return new PropertyData(PropertyType.DateTime);
            }

            protected override void addValue(XmlReader parser, PropertyData target)
            {
                target.Values = AddToList(target.Values, ReadDateTime(parser));
            }
        };

        private abstract class PropertyXmlWalker : XmlWalker<PropertyData>
        {
            protected abstract PropertyData createTarget(XmlReader parser, string localname, string ns);

            protected override PropertyData PrepareTarget(XmlReader parser, string localname, string ns)
            {
                PropertyData result = createTarget(parser, localname, ns);

                if (parser.HasAttributes)
                {
                    result.Id = parser.GetAttribute(XmlConstants.ATTR_PROPERTY_ID);
                    result.LocalName = parser.GetAttribute(XmlConstants.ATTR_PROPERTY_LOCALNAME);
                    result.DisplayName = parser.GetAttribute(XmlConstants.ATTR_PROPERTY_DISPLAYNAME);
                    result.QueryName = parser.GetAttribute(XmlConstants.ATTR_PROPERTY_QUERYNAME);
                }

                return result;
            }

            protected abstract void addValue(XmlReader parser, PropertyData target);

            protected override bool Read(XmlReader parser, string localname, string ns, PropertyData target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_PROPERTY_VALUE))
                    {
                        try
                        {
                            addValue(parser, target);
                        }
                        catch (CmisInvalidArgumentException e)
                        {
                            // a few repositories send invalid values here
                            // for example, in some cases SharePoint sends an empty
                            // "value" tag instead of omitting the "value" tag to
                            // indicate a "not set" value
                            // -> being tolerant is better than breaking an
                            // application because of this
                            if (Logger.IsWarnEnabled)
                            {
                                Logger.Warn("Found invalid property value for property " + target.Id + "!", e);
                            }
                        }
                        return true;
                    }
                }

                return false;
            }

        }

        private abstract class PropertyStringXmlWalker : PropertyXmlWalker
        {
            protected override void addValue(XmlReader parser, PropertyData target)
            {
                target.Values = AddToList(target.Values, ReadText(parser));
            }
        }

        // --------------------
        // --- query parser ---
        // --------------------


        private static readonly QueryParser QUERY_PARSER = new QueryParser();
        private class QueryParser : XmlWalker<QueryType>
        {
            protected override QueryType PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new QueryType();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, QueryType target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_QUERY_STATEMENT))
                    {
                        target.Statement = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_QUERY_SEARCHALLVERSIONS))
                    {
                        target.SearchAllVersions = ReadBoolean(parser) ?? false;
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_QUERY_INCLUDEALLOWABLEACTIONS))
                    {
                        target.IncludeAllowableActions = ReadBoolean(parser) ?? false;
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_QUERY_INCLUDERELATIONSHIPS))
                    {
                        target.IncludeRelationships = ReadEnum<IncludeRelationships>(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_QUERY_RENDITIONFILTER))
                    {
                        target.RenditionFilter = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_QUERY_MAXITEMS))
                    {
                        target.MaxItems = ReadInteger(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_QUERY_SKIPCOUNT))
                    {
                        target.SkipCount = ReadInteger(parser);
                        return true;
                    }
                }

                return false;
            }
        };

        // --------------------------
        // --- bulk update parser ---
        // --------------------------


        private static readonly BulkUpdateParser BULK_UPDATE_PARSER = new BulkUpdateParser();
        private class BulkUpdateParser : XmlWalker<BulkUpdate>
        {
            protected override BulkUpdate PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new BulkUpdate();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, BulkUpdate target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_BULK_UPDATE_ID_AND_TOKEN))
                    {
                        target.ObjectIdAndChangeToken = AddToList(target.ObjectIdAndChangeToken, ID_AND_TOKEN_PARSER.Walk(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_BULK_UPDATE_PROPERTIES))
                    {
                        target.Properties = PROPERTIES_PARSER.Walk(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_BULK_UPDATE_ADD_SECONDARY_TYPES))
                    {
                        target.AddSecondaryTypeIds = AddToList(target.AddSecondaryTypeIds, ReadText(parser));
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_BULK_UPDATE_REMOVE_SECONDARY_TYPES))
                    {
                        target.RemoveSecondaryTypeIds = AddToList(target.RemoveSecondaryTypeIds, ReadText(parser));
                        return true;
                    }
                }

                return false;
            }
        };


        private static readonly IdAndTokenParser ID_AND_TOKEN_PARSER = new IdAndTokenParser();
        private class IdAndTokenParser : XmlWalker<BulkUpdateObjectIdAndChangeToken>
        {
            protected override BulkUpdateObjectIdAndChangeToken PrepareTarget(XmlReader parser, string localname, string ns)
            {
                return new BulkUpdateObjectIdAndChangeToken();
            }

            protected override bool Read(XmlReader parser, string localname, string ns, BulkUpdateObjectIdAndChangeToken target)
            {
                if (IsCmisNamespace(ns))
                {
                    if (IsTag(localname, XmlConstants.TAG_IDANDTOKEN_ID))
                    {
                        target.Id = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_IDANDTOKEN_NEWID))
                    {
                        target.NewId = ReadText(parser);
                        return true;
                    }

                    if (IsTag(localname, XmlConstants.TAG_IDANDTOKEN_CHANGETOKEN))
                    {
                        target.ChangeToken = ReadText(parser);
                        return true;
                    }
                }

                return false;
            }
        };
    }
}
