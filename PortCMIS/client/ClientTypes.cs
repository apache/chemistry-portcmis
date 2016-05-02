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
using System;
using System.Collections.Generic;

namespace PortCMIS.Client.Impl
{
    /// <summary>
    /// Helper for all type implementations.
    /// </summary>
    internal class ObjectTypeHelper
    {
        private ISession session;
        private IObjectType objectType;
        private IObjectType baseType;
        private IObjectType parentType;

        public ObjectTypeHelper(ISession session, IObjectType objectType)
        {
            this.session = session;
            this.objectType = objectType;
        }

        public ISession Session { get { return session; } }

        public bool IsBaseType { get { return objectType.ParentTypeId == null || objectType.ParentTypeId.Length == 0; } }

        public IObjectType BaseType
        {
            get
            {
                if (IsBaseType) { return null; }
                if (baseType != null) { return baseType; }

                baseType = session.GetTypeDefinition(objectType.BaseTypeId.GetCmisValue());

                return baseType;
            }
        }

        public IObjectType ParentType
        {
            get
            {
                if (parentType != null) { return parentType; }
                if (objectType.ParentTypeId == null) { return null; }

                parentType = session.GetTypeDefinition(objectType.ParentTypeId);

                return parentType;
            }
        }

        public IItemEnumerable<IObjectType> GetChildren()
        {
            return session.GetTypeChildren(objectType.Id, true);
        }

        public IList<ITree<IObjectType>> GetDescendants(int depth)
        {
            return session.GetTypeDescendants(objectType.Id, depth, true);
        }
    }

    /// <summary>
    /// Document type implementation.
    /// </summary>
    public class DocumentType : DocumentTypeDefinition, IDocumentType
    {
        private ObjectTypeHelper helper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the related session object</param>
        /// <param name="typeDefinition">the low-level type definition</param>
        public DocumentType(ISession session, IDocumentTypeDefinition typeDefinition)
        {
            Initialize(typeDefinition);
            ContentStreamAllowed = typeDefinition.ContentStreamAllowed;
            IsVersionable = typeDefinition.IsVersionable;
            helper = new ObjectTypeHelper(session, this);
        }

        /// <inheritdoc/>
        public virtual IObjectType BaseType { get { return helper.BaseType; } }

        /// <inheritdoc/>
        public virtual IObjectType ParentType { get { return helper.ParentType; } }

        /// <inheritdoc/>
        public virtual IItemEnumerable<IObjectType> GetChildren() { return helper.GetChildren(); }

        /// <inheritdoc/>
        public virtual IList<ITree<IObjectType>> GetDescendants(int depth) { return helper.GetDescendants(depth); }

        /// <inheritdoc/>
        public virtual bool IsBaseType { get { return helper.IsBaseType; } }
    }

    /// <summary>
    /// Folder type implementation.
    /// </summary>
    public class FolderType : FolderTypeDefinition, IFolderType
    {
        private ObjectTypeHelper helper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the related session object</param>
        /// <param name="typeDefinition">the low-level type definition</param>
        public FolderType(ISession session, IFolderTypeDefinition typeDefinition)
        {
            Initialize(typeDefinition);
            helper = new ObjectTypeHelper(session, this);
        }

        /// <inheritdoc/>
        public virtual IObjectType BaseType { get { return helper.BaseType; } }

        /// <inheritdoc/>
        public virtual IObjectType ParentType { get { return helper.ParentType; } }

        /// <inheritdoc/>
        public virtual IItemEnumerable<IObjectType> GetChildren() { return helper.GetChildren(); }

        /// <inheritdoc/>
        public virtual IList<ITree<IObjectType>> GetDescendants(int depth) { return helper.GetDescendants(depth); }

        /// <inheritdoc/>
        public virtual bool IsBaseType { get { return helper.IsBaseType; } }
    }

    /// <summary>
    /// Relationship type implementation.
    /// </summary>
    public class RelationshipType : RelationshipTypeDefinition, IRelationshipType
    {
        private ObjectTypeHelper helper;
        private IList<IObjectType> allowedSourceTypes;
        private IList<IObjectType> allowedTargetTypes;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the related session object</param>
        /// <param name="typeDefinition">the low-level type definition</param>
        public RelationshipType(ISession session, IRelationshipTypeDefinition typeDefinition)
        {
            Initialize(typeDefinition);
            helper = new ObjectTypeHelper(session, this);
        }

        /// <inheritdoc/>
        public virtual IObjectType BaseType { get { return helper.BaseType; } }

        /// <inheritdoc/>
        public virtual IObjectType ParentType { get { return helper.ParentType; } }

        /// <inheritdoc/>
        public virtual IItemEnumerable<IObjectType> GetChildren() { return helper.GetChildren(); }

        /// <inheritdoc/>
        public virtual IList<ITree<IObjectType>> GetDescendants(int depth) { return helper.GetDescendants(depth); }

        /// <inheritdoc/>
        public virtual bool IsBaseType { get { return helper.IsBaseType; } }

        /// <inheritdoc/>
        public virtual IList<IObjectType> GetAllowedSourceTypes
        {
            get
            {
                if (allowedSourceTypes == null)
                {
                    IList<string> ids = AllowedSourceTypeIds;
                    IList<IObjectType> types = new List<IObjectType>(ids == null ? 0 : ids.Count);
                    if (ids != null)
                    {
                        foreach (String id in ids)
                        {
                            types.Add(helper.Session.GetTypeDefinition(id));
                        }
                    }
                    allowedSourceTypes = types;
                }
                return allowedSourceTypes;
            }
        }

        /// <inheritdoc/>
        public virtual IList<IObjectType> GetAllowedTargetTypes
        {
            get
            {
                if (allowedTargetTypes == null)
                {
                    IList<string> ids = AllowedTargetTypeIds;
                    IList<IObjectType> types = new List<IObjectType>(ids == null ? 0 : ids.Count);
                    if (ids != null)
                    {
                        foreach (String id in ids)
                        {
                            types.Add(helper.Session.GetTypeDefinition(id));
                        }
                    }
                    allowedTargetTypes = types;
                }
                return allowedTargetTypes;
            }
        }
    }

    /// <summary>
    /// Policy type implementation.
    /// </summary>
    public class PolicyType : PolicyTypeDefinition, IPolicyType
    {
        private ObjectTypeHelper helper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the related session object</param>
        /// <param name="typeDefinition">the low-level type definition</param>
        public PolicyType(ISession session, IPolicyTypeDefinition typeDefinition)
        {
            Initialize(typeDefinition);
            helper = new ObjectTypeHelper(session, this);
        }

        /// <inheritdoc/>
        public virtual IObjectType BaseType { get { return helper.BaseType; } }

        /// <inheritdoc/>
        public virtual IObjectType ParentType { get { return helper.ParentType; } }

        /// <inheritdoc/>
        public virtual IItemEnumerable<IObjectType> GetChildren() { return helper.GetChildren(); }

        /// <inheritdoc/>
        public virtual IList<ITree<IObjectType>> GetDescendants(int depth) { return helper.GetDescendants(depth); }

        /// <inheritdoc/>
        public virtual bool IsBaseType { get { return helper.IsBaseType; } }
    }

    /// <summary>
    /// Item type implementation.
    /// </summary>
    public class ItemType : ItemTypeDefinition, IItemType
    {
        private ObjectTypeHelper helper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the related session object</param>
        /// <param name="typeDefinition">the low-level type definition</param>
        public ItemType(ISession session, IItemTypeDefinition typeDefinition)
        {
            Initialize(typeDefinition);
            helper = new ObjectTypeHelper(session, this);
        }

        /// <inheritdoc/>
        public virtual IObjectType BaseType { get { return helper.BaseType; } }

        /// <inheritdoc/>
        public virtual IObjectType ParentType { get { return helper.ParentType; } }

        /// <inheritdoc/>
        public virtual IItemEnumerable<IObjectType> GetChildren() { return helper.GetChildren(); }

        /// <inheritdoc/>
        public virtual IList<ITree<IObjectType>> GetDescendants(int depth) { return helper.GetDescendants(depth); }

        /// <inheritdoc/>
        public virtual bool IsBaseType { get { return helper.IsBaseType; } }
    }

    /// <summary>
    /// Secondary type implementation.
    /// </summary>
    public class SecondaryType : SecondaryTypeDefinition, ISecondaryType
    {
        private ObjectTypeHelper helper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the related session object</param>
        /// <param name="typeDefinition">the low-level type definition</param>
        public SecondaryType(ISession session, ISecondaryTypeDefinition typeDefinition)
        {
            Initialize(typeDefinition);
            helper = new ObjectTypeHelper(session, this);
        }

        /// <inheritdoc/>
        public virtual IObjectType BaseType { get { return helper.BaseType; } }

        /// <inheritdoc/>
        public virtual IObjectType ParentType { get { return helper.ParentType; } }

        /// <inheritdoc/>
        public virtual IItemEnumerable<IObjectType> GetChildren() { return helper.GetChildren(); }

        /// <inheritdoc/>
        public virtual IList<ITree<IObjectType>> GetDescendants(int depth) { return helper.GetDescendants(depth); }

        /// <inheritdoc/>
        public virtual bool IsBaseType { get { return helper.IsBaseType; } }
    }
}
