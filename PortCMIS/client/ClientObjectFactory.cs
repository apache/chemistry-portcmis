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
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PortCMIS.Client.Impl
{
    public class ObjectFactory : IObjectFactory
    {
        private ISession session;

        public virtual void Initialize(ISession session, IDictionary<string, string> parameters)
        {
            this.session = session;
        }

        // Acl and ACE
        public virtual IAcl ConvertAces(IList<IAce> aces)
        {
            if (aces == null) { return null; }

            Acl result = new Acl();
            result.Aces = new List<IAce>();

            foreach (IAce ace in aces)
            {
                result.Aces.Add(ace);
            }

            return result;
        }

        public virtual IAcl CreateAcl(IList<IAce> aces)
        {
            Acl acl = new Acl();
            acl.Aces = aces;

            return acl;
        }

        public virtual IAce CreateAce(string principal, IList<string> permissions)
        {
            Ace ace = new Ace();
            ace.IsDirect = true;
            Principal acePrincipal = new Principal();
            acePrincipal.Id = principal;
            ace.Principal = acePrincipal;
            ace.Permissions = permissions;

            return ace;
        }

        // policies
        public virtual IList<string> ConvertPolicies(IList<IPolicy> policies)
        {
            if (policies == null) { return null; }

            IList<string> result = new List<string>();
            foreach (IPolicy policy in policies)
            {
                if (policy != null && policy.Id != null)
                {
                    result.Add(policy.Id);
                }
            }

            return result;
        }

        // renditions
        public virtual IRendition ConvertRendition(string objectId, IRenditionData rendition)
        {
            if (rendition == null)
            {
                throw new ArgumentException("rendition");
            }

            return new Rendition(this.session, objectId, rendition.StreamId, rendition.MimeType, rendition.Length, rendition.Kind,
                      rendition.Title, rendition.Height, rendition.Height, rendition.RenditionDocumentId);
        }

        // content stream
        public virtual IContentStream CreateContentStream(string filename, long length, string mimetype, Stream stream)
        {
            ContentStream result = new ContentStream();
            result.FileName = filename;
            result.Length = length;
            result.MimeType = mimetype;
            result.Stream = stream;

            return result;
        }

        // types
        public virtual IObjectType ConvertTypeDefinition(ITypeDefinition typeDefinition)
        {
            switch (typeDefinition.BaseTypeId)
            {
                case BaseTypeId.CmisDocument:
                    return new DocumentType(session, (IDocumentTypeDefinition)typeDefinition);
                case BaseTypeId.CmisFolder:
                    return new FolderType(session, (IFolderTypeDefinition)typeDefinition);
                case BaseTypeId.CmisRelationship:
                    return new RelationshipType(session, (IRelationshipTypeDefinition)typeDefinition);
                case BaseTypeId.CmisPolicy:
                    return new PolicyType(session, (IPolicyTypeDefinition)typeDefinition);
                case BaseTypeId.CmisItem:
                    return new ItemType(session, (IItemTypeDefinition)typeDefinition);
                case BaseTypeId.CmisSecondary:
                    return new SecondaryType(session, (ISecondaryTypeDefinition)typeDefinition);
                default:
                    throw new CmisRuntimeException("Unknown base type!");
            }
        }

        public virtual IObjectType GetTypeFromObjectData(IObjectData objectData)
        {
            if (objectData == null || objectData.Properties == null)
            {
                return null;
            }

            IPropertyData typeProperty = objectData.Properties[PropertyIds.ObjectTypeId];
            if (typeProperty == null)
            {
                return null;
            }

            string typeId = typeProperty.FirstValue as string;
            if (typeId == null)
            {
                return null;
            }

            return session.GetTypeDefinition(typeId);
        }

        // properties
        public virtual IProperty CreateProperty<T>(IPropertyDefinition type, IList<T> values)
        {
            return new Property(type, (IList<object>)values);
        }

        protected virtual IProperty ConvertProperty(IObjectType objectType, IList<ISecondaryType> secondaryTypes, IPropertyData pd)
        {
            IPropertyDefinition definition = objectType[pd.Id];
            if (definition == null)
            {
                // search secondary types
                if (secondaryTypes != null)
                {
                    foreach (ISecondaryType secondaryType in secondaryTypes)
                    {
                        if (secondaryType.PropertyDefinitions != null)
                        {
                            foreach (IPropertyDefinition propertyDefinition in secondaryType.PropertyDefinitions)
                            {
                                if (propertyDefinition.Id == pd.Id)
                                {
                                    definition = propertyDefinition;
                                    break;
                                }
                            }
                        }
                    }
                }
                // property without definition
                if (definition == null)
                    throw new CmisRuntimeException("Property '" + pd.Id + "' doesn't exist!");
            }

            return CreateProperty(definition, pd.Values);
        }

        public virtual IDictionary<string, IProperty> ConvertProperties(IObjectType objectType, IList<ISecondaryType> secondaryTypes, IProperties properties)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            if (objectType.PropertyDefinitions == null)
            {
                throw new ArgumentException("Object type has no property definitions!");
            }

            if (properties == null || properties.PropertyList == null)
            {
                throw new ArgumentException("Properties must be set!");
            }

            // iterate through properties and convert them
            IDictionary<string, IProperty> result = new Dictionary<string, IProperty>();
            foreach (IPropertyData property in properties.PropertyList)
            {
                // find property definition
                IProperty apiProperty = ConvertProperty(objectType, secondaryTypes, property);
                result[property.Id] = apiProperty;
            }

            return result;
        }

        public virtual IProperties ConvertProperties(IDictionary<string, object> properties, IObjectType type, IList<ISecondaryType> secondaryTypes, HashSet<Updatability> updatabilityFilter)
        {
            // check input
            if (properties == null)
            {
                return null;
            }

            // get the type
            if (type == null)
            {
                object typeIdObject;
                if (!properties.TryGetValue(PropertyIds.ObjectTypeId, out typeIdObject))
                {
                    throw new ArgumentException("Type or type property must be set!");
                }

                string typeId = typeIdObject as string;
                if (typeId == null)
                {
                    throw new ArgumentException("Type or type property must be set!");
                }

                type = session.GetTypeDefinition(typeId);
            }

            // get secondary types
            IList<ISecondaryType> allSecondaryTypes = null;
            object secondaryTypeIds;
            properties.TryGetValue(PropertyIds.SecondaryObjectTypeIds, out secondaryTypeIds);
            if (secondaryTypeIds is IList<string>)
            {
                allSecondaryTypes = new List<ISecondaryType>();

                foreach (string secondaryTypeId in (secondaryTypeIds as IList<string>))
                {
                    if (!(secondaryTypeId is string))
                    {
                        throw new ArgumentException("Secondary types property contains an invalid entry: "
                                                    + secondaryTypeId);
                    }

                    IObjectType secondaryType = session.GetTypeDefinition(secondaryTypeId.ToString());
                    if (!(secondaryType is ISecondaryType))
                    {
                        throw new ArgumentException(
                            "Secondary types property contains a type that is not a secondary type: " + secondaryTypeId);
                    }

                    allSecondaryTypes.Add((ISecondaryType)secondaryType);
                }
            }

            if (secondaryTypes != null && allSecondaryTypes == null)
            {
                allSecondaryTypes = secondaryTypes;
            }

            Properties result = new Properties();

            // the big loop
            foreach (KeyValuePair<string, object> property in properties)
            {
                string id = property.Key;
                object value = property.Value;

                if (value is IProperty)
                {
                    IProperty p = (IProperty)value;
                    if (id != p.Id)
                    {
                        throw new ArgumentException("Property id mismatch: '" + id + "' != '" + p.Id + "'!");
                    }
                    value = (p.PropertyDefinition.Cardinality == Cardinality.Single ? p.FirstValue : p.Values);
                }

                // get the property definition
                IPropertyDefinition definition = type[id];
                if (definition == null && allSecondaryTypes != null)
                {
                    foreach (ISecondaryType secondaryType in allSecondaryTypes)
                    {
                        if (secondaryType.PropertyDefinitions != null)
                        {
                            foreach (IPropertyDefinition propertyDefinition in secondaryType.PropertyDefinitions)
                            {
                                if (propertyDefinition.Id == id)
                                {
                                    definition = propertyDefinition;
                                    break;
                                }
                            }
                            if (definition != null)
                            {
                                break;
                            }
                        }
                    }
                    if (definition == null)
                        throw new ArgumentException("Property +'" + id + "' is not valid for this type!");
                }

                // check updatability
                if (updatabilityFilter != null)
                {
                    if (definition.Updatability.HasValue && !updatabilityFilter.Contains(definition.Updatability.Value))
                    {
                        continue;
                    }
                }

                PropertyData propertyData = new PropertyData(definition.PropertyType);
                propertyData.Id = id;

                // single and multi value check
                if (value == null)
                {
                    propertyData.Values = null;
                }
                else if (value is IList)
                {
                    if (definition.Cardinality != Cardinality.Multi)
                    {
                        throw new ArgumentException("Property '" + id + "' is not a multi value property!");
                    }

                    IList valueList = (IList)value;

                    // check if the list is homogeneous and does not contain null values
                    foreach (object o in valueList)
                    {
                        if (o == null)
                        {
                            throw new ArgumentException("Property '" + id + "' contains null values!");
                        }

                        propertyData.AddValue(o);
                    }
                }
                else
                {
                    if (definition.Cardinality != Cardinality.Single)
                    {
                        throw new ArgumentException("Property '" + id + "' is not a single value property!");
                    }

                    propertyData.AddValue(value);
                }

                result.AddProperty(propertyData);
            }

            return result;
        }

        public virtual IList<IPropertyData> ConvertQueryProperties(IProperties properties)
        {
            if ((properties == null) || (properties.PropertyList == null))
            {
                throw new ArgumentException("Properties must be set!");
            }

            return properties.PropertyList;
        }

        // objects
        public virtual ICmisObject ConvertObject(IObjectData objectData, IOperationContext context)
        {
            if (objectData == null)
            {
                throw new ArgumentNullException("objectData");
            }

            if (objectData.Id == null)
            {
                throw new ArgumentException("Object ID property not set!");
            }

            if (objectData.BaseTypeId == null)
            {
                throw new ArgumentException("Base type ID property not set!");
            }

            IObjectType type = GetTypeFromObjectData(objectData);

            switch (objectData.BaseTypeId)
            {
                case BaseTypeId.CmisDocument:
                    return new Document(session, type, objectData, context);
                case BaseTypeId.CmisFolder:
                    return new Folder(session, type, objectData, context);
                case BaseTypeId.CmisPolicy:
                    return new Policy(session, type, objectData, context);
                case BaseTypeId.CmisRelationship:
                    return new Relationship(session, type, objectData, context);
                case BaseTypeId.CmisItem:
                    return new Item(session, type, objectData, context);
                default:
                    throw new CmisRuntimeException("Unsupported type: " + objectData.BaseTypeId.ToString());
            }
        }

        public virtual IQueryResult ConvertQueryResult(IObjectData objectData)
        {
            if (objectData == null)
            {
                throw new ArgumentException("Object data is null!");
            }

            return new QueryResult(session, objectData);
        }

        public virtual IChangeEvent ConvertChangeEvent(IObjectData objectData)
        {
            ChangeEvent result = new ChangeEvent();

            if (objectData.ChangeEventInfo != null)
            {
                result.ChangeType = objectData.ChangeEventInfo.ChangeType;
                result.ChangeTime = objectData.ChangeEventInfo.ChangeTime;
            }

            if (objectData.Properties != null && objectData.Properties.PropertyList != null)
            {
                result.Properties = new Dictionary<string, IList<object>>();

                foreach (IPropertyData property in objectData.Properties.PropertyList)
                {
                    if (property.Id != null)
                    {
                        result.Properties[property.Id] = property.Values;
                    }
                }

                IList<object> objectIdList;
                if (result.Properties.TryGetValue(PropertyIds.ObjectId, out objectIdList))
                {
                    if (objectIdList != null && objectIdList.Count > 0)
                    {
                        result.ObjectId = objectIdList[0] as string;
                    }
                }

                if (objectData.PolicyIds != null && objectData.PolicyIds.PolicyIds != null)
                {
                    result.PolicyIds = objectData.PolicyIds.PolicyIds;
                }

                if (objectData.Acl != null)
                {
                    result.Acl = objectData.Acl;
                }
            }

            return result;
        }

        public virtual IChangeEvents ConvertChangeEvents(string changeLogToken, IObjectList objectList)
        {
            if (objectList == null)
            {
                return null;
            }

            ChangeEvents result = new ChangeEvents();
            result.LatestChangeLogToken = changeLogToken;

            result.ChangeEventList = new List<IChangeEvent>();
            if (objectList.Objects != null)
            {
                foreach (IObjectData objectData in objectList.Objects)
                {
                    if (objectData == null)
                    {
                        continue;
                    }

                    result.ChangeEventList.Add(ConvertChangeEvent(objectData));
                }
            }

            result.HasMoreItems = objectList.HasMoreItems;
            result.TotalNumItems = objectList.NumItems;

            return result;
        }
    }
}
