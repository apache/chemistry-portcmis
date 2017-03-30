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

using PortCMIS.Binding;
using PortCMIS.Binding.Services;
using PortCMIS.Data;
using PortCMIS.Data.Extensions;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace PortCMIS.Client.Impl
{
    /// <summary>
    /// CMIS object base class.
    /// </summary>
    public abstract class AbstractCmisObject : ICmisObject
    {
        /// <value>
        /// Gets the current session.
        /// </value>
        protected ISession Session { get; private set; }

        /// <value>
        /// Gets the current repository ID.
        /// </value>
        protected string RepositoryId { get { return Session.RepositoryInfo.Id; } }

        /// <value>
        /// Gets the current binding.
        /// </value>
        protected ICmisBinding Binding { get { return Session.Binding; } }

        private IObjectType objectType;

        /// <inheritdoc/>
        public virtual IObjectType ObjectType
        {
            get
            {
                lock (objectLock)
                {
                    return objectType;
                }
            }
        }

        /// <inheritdoc/>
        public virtual IList<ISecondaryType> SecondaryTypes
        {
            get
            {
                lock (objectLock)
                {
                    return secondaryTypes;
                }
            }
        }

        /// <inheritdoc/>
        protected virtual string ObjectId
        {
            get
            {
                string objectId = Id;
                if (objectId == null)
                {
                    throw new InvalidOperationException("Object ID is unknown!");
                }

                return objectId;
            }
        }

        /// <summary>
        /// Gets the operation context that was used to fetch this object.
        /// </summary>
        protected virtual IOperationContext CreationContext { get; private set; }

        private IDictionary<string, IProperty> properties;
        private IAllowableActions allowableActions;
        private IList<IRendition> renditions;
        private IAcl acl;
        private IList<string> policyIds;
        private IList<IPolicy> policies;
        private IList<IRelationship> relationships;
        private IDictionary<ExtensionLevel, IList<ICmisExtensionElement>> extensions;
        private IList<ISecondaryType> secondaryTypes;

        /// <summary>
        /// An object used for locking.
        /// </summary>
        protected object objectLock = new object();

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="session">the current session</param>
        /// <param name="objectType">the object type</param>
        /// <param name="objectData">the low-level object data</param>
        /// <param name="context">the operation context that was used to fetch this object</param>
        protected void Initialize(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            if (objectType.PropertyDefinitions == null || objectType.PropertyDefinitions.Count < 9)
            {
                // there must be at least the 9 standard properties that all objects have
                throw new ArgumentException("Object type must have property definitions!");
            }

            if (objectData == null)
            {
                throw new ArgumentNullException("objectData");
            }

            if (objectData.Properties == null)
            {
                throw new ArgumentException("Object Data properties must be set!");
            }

            if (objectData.Id == null)
            {
                throw new ArgumentException("Object ID must be set!");
            }

            this.Session = session;
            this.objectType = objectType;
            this.extensions = new Dictionary<ExtensionLevel, IList<ICmisExtensionElement>>();
            this.CreationContext = new OperationContext(context);
            this.RefreshTimestamp = DateTime.UtcNow;

            IObjectFactory of = Session.ObjectFactory;

            if (objectData != null)
            {
                // handle properties
                if (objectData.Properties != null)
                {
                    // search secondaryObjectTypes
                    foreach (IPropertyData property in objectData.Properties.PropertyList)
                    {
                        if (property.Id == PropertyIds.SecondaryObjectTypeIds)
                        {
                            IList<object> stids = property.Values as IList<object>;
                            if (stids != null && stids.Count > 0)
                            {
                                secondaryTypes = new List<ISecondaryType>();
                                foreach (object stid in stids)
                                {
                                    IObjectType type = Session.GetTypeDefinition(stid.ToString());
                                    if (type is ISecondaryType)
                                    {
                                        secondaryTypes.Add(type as ISecondaryType);
                                    }
                                }
                            }
                            else
                            {
                                secondaryTypes = null;
                            }
                            break;
                        }
                    }

                    properties = of.ConvertProperties(objectType, secondaryTypes, objectData.Properties);
                    extensions[ExtensionLevel.Properties] = objectData.Properties.Extensions;
                }

                // handle allowable actions
                if (objectData.AllowableActions != null)
                {
                    allowableActions = objectData.AllowableActions;
                    extensions[ExtensionLevel.AllowableActions] = objectData.AllowableActions.Extensions;
                }
                else
                {
                    allowableActions = null;
                }

                // handle renditions
                if (objectData.Renditions != null)
                {
                    renditions = new List<IRendition>();
                    foreach (IRenditionData rd in objectData.Renditions)
                    {
                        renditions.Add(of.ConvertRendition(Id, rd));
                    }
                }
                else
                {
                    renditions = null;
                }

                // handle Acl
                if (objectData.Acl != null)
                {
                    acl = objectData.Acl;
                    extensions[ExtensionLevel.Acl] = objectData.Acl.Extensions;
                }
                else
                {
                    acl = null;
                }

                // handle policies
                policies = null;
                if (objectData.PolicyIds != null && objectData.PolicyIds.PolicyIds != null)
                {
                    if (objectData.PolicyIds.PolicyIds.Count == 0)
                    {
                        policyIds = null;
                    }
                    else
                    {
                        policyIds = objectData.PolicyIds.PolicyIds;
                    }
                    extensions[ExtensionLevel.Policies] = objectData.PolicyIds.Extensions;
                }
                else
                {
                    policyIds = null;
                }

                // handle relationships
                if (objectData.Relationships != null)
                {
                    relationships = new List<IRelationship>();
                    foreach (IObjectData rod in objectData.Relationships)
                    {
                        IRelationship relationship = of.ConvertObject(rod, CreationContext) as IRelationship;
                        if (relationship != null)
                        {
                            relationships.Add(relationship);
                        }
                    }
                }
                else
                {
                    relationships = null;
                }

                extensions[ExtensionLevel.Object] = objectData.Extensions;
            }
        }

        /// <summary>
        /// Returns the query name of a property.
        /// </summary>
        /// <param name="propertyId">the property ID</param>
        /// <returns>the query name or <c>null</c> if the property doesn't exist or the property has no query name</returns>
        protected virtual string GetPropertyQueryName(string propertyId)
        {
            lock (objectLock)
            {
                IPropertyDefinition propDef = objectType[propertyId];
                if (propDef == null)
                {
                    return null;
                }

                return propDef.QueryName;
            }
        }

        // --- object ---

        /// <inheritdoc/>
        public virtual void Delete()
        {
            Delete(true);
        }

        /// <inheritdoc/>
        public virtual void Delete(bool allVersions)
        {
            lock (objectLock)
            {
                Session.Delete(this, allVersions);
            }
        }

        /// <inheritdoc/>
        public virtual ICmisObject UpdateProperties(IDictionary<string, object> properties)
        {
            IObjectId objectId = UpdateProperties(properties, true);
            if (objectId == null)
            {
                return null;
            }

            if (ObjectId != objectId.Id)
            {
                return Session.GetObject(objectId, CreationContext);
            }

            return this;
        }

        /// <inheritdoc/>
        public virtual IObjectId UpdateProperties(IDictionary<String, object> properties, bool refresh)
        {
            if (properties == null || properties.Count == 0)
            {
                throw new ArgumentException("Properties must not be empty!");
            }

            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                string changeToken = ChangeToken;

                HashSet<Updatability> updatebility = new HashSet<Updatability>();
                updatebility.Add(Updatability.ReadWrite);

                // check if checked out
                bool? isCheckedOut = GetPropertyValue(PropertyIds.IsVersionSeriesCheckedOut) as bool?;
                if (isCheckedOut.HasValue && isCheckedOut.Value)
                {
                    updatebility.Add(Updatability.WhenCheckedOut);
                }

                // it's time to update
                Binding.GetObjectService().UpdateProperties(RepositoryId, ref objectId, ref changeToken,
                    Session.ObjectFactory.ConvertProperties(properties, this.objectType, this.secondaryTypes, updatebility), null);

                newObjectId = objectId;
            }

            if (refresh)
            {
                Refresh();
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        /// <inheritdoc/>
        public virtual ICmisObject Rename(string newName)
        {
            if (newName == null || newName.Length == 0)
            {
                throw new ArgumentException("New name must not be empty!", "newName");
            }

            IDictionary<string, object> prop = new Dictionary<string, object>();
            prop[PropertyIds.Name] = newName;

            return UpdateProperties(prop);
        }

        /// <inheritdoc/>
        public virtual IObjectId Rename(string newName, bool refresh)
        {
            IDictionary<string, object> prop = new Dictionary<string, object>();
            prop[PropertyIds.Name] = newName;

            return UpdateProperties(prop, refresh);
        }

        // --- properties ---

        /// <inheritdoc/>
        public virtual IObjectType BaseType { get { return Session.GetTypeDefinition(GetPropertyAsStringValue(PropertyIds.BaseTypeId)); } }

        /// <inheritdoc/>
        public virtual BaseTypeId BaseTypeId
        {
            get
            {
                string baseType = GetPropertyAsStringValue(PropertyIds.BaseTypeId);
                if (baseType == null) { throw new InvalidOperationException("Base type not set!"); }

                return baseType.GetCmisEnum<BaseTypeId>();
            }
        }

        /// <inheritdoc/>
        public virtual string Id { get { return GetPropertyAsStringValue(PropertyIds.ObjectId); } }

        /// <inheritdoc/>
        public virtual string Name { get { return GetPropertyAsStringValue(PropertyIds.Name); } }

        /// <inheritdoc/>
        public virtual string CreatedBy { get { return GetPropertyAsStringValue(PropertyIds.CreatedBy); } }

        /// <inheritdoc/>
        public virtual DateTime? CreationDate { get { return GetPropertyAsDateTimeValue(PropertyIds.CreationDate); } }

        /// <inheritdoc/>
        public virtual string LastModifiedBy { get { return GetPropertyAsStringValue(PropertyIds.LastModifiedBy); } }

        /// <inheritdoc/>
        public virtual DateTime? LastModificationDate { get { return GetPropertyAsDateTimeValue(PropertyIds.LastModificationDate); } }

        /// <inheritdoc/>
        public virtual string ChangeToken { get { return GetPropertyAsStringValue(PropertyIds.ChangeToken); } }

        /// <inheritdoc/>
        public virtual IList<IProperty> Properties
        {
            get
            {
                lock (objectLock)
                {
                    return new List<IProperty>(properties.Values);
                }
            }
        }

        /// <inheritdoc/>
        public virtual IProperty this[string propertyId]
        {
            get
            {
                if (propertyId == null)
                {
                    throw new ArgumentNullException("propertyId");
                }

                lock (objectLock)
                {
                    IProperty property;
                    if (properties.TryGetValue(propertyId, out property))
                    {
                        return property;
                    }
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public virtual object GetPropertyValue(string propertyId)
        {
            IProperty property = this[propertyId];
            if (property == null) { return null; }

            return property.Value;
        }

        /// <inheritdoc/>
        public virtual string GetPropertyAsStringValue(string propertyId)
        {
            object value = GetPropertyValue(propertyId);
            if (value == null)
            {
                return null;
            }

            return Convert.ToString(value);
        }

        /// <inheritdoc/>
        public virtual long? GetPropertyAsLongValue(string propertyId)
        {
            object value = GetPropertyValue(propertyId);
            if (value == null)
            {
                return null;
            }

            if (value is BigInteger)
            {
                return (long)((BigInteger)value);
            }

            return Convert.ToInt64(value);
        }

        /// <inheritdoc/>
        public virtual bool? GetPropertyAsBoolValue(string propertyId)
        {
            object value = GetPropertyValue(propertyId);
            if (value == null)
            {
                return null;
            }

            if (value is BigInteger)
            {
                return Convert.ToBoolean((long)((BigInteger)value));
            }

            return Convert.ToBoolean(value);
        }

        /// <inheritdoc/>
        public virtual DateTime? GetPropertyAsDateTimeValue(string propertyId)
        {
            object value = GetPropertyValue(propertyId);
            if (value == null)
            {
                return null;
            }

            return Convert.ToDateTime(value);
        }

        // --- allowable actions ---

        /// <inheritdoc/>
        public virtual IAllowableActions AllowableActions
        {
            get
            {
                lock (objectLock)
                {
                    return allowableActions;
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool HasAllowableAction(PortCMIS.Enums.Action action)
        {
            IAllowableActions currentAllowableActions = AllowableActions;
            if (currentAllowableActions == null || currentAllowableActions.Actions == null)
            {
                throw new Exception("Allowable Actions are not available!");
            }

            return currentAllowableActions.Actions.Contains(action);
        }

        // --- renditions ---

        /// <inheritdoc/>
        public virtual IList<IRendition> Renditions
        {
            get
            {
                lock (objectLock)
                {
                    return renditions;
                }
            }
        }

        // --- Acl ---

        /// <inheritdoc/>
        public virtual IAcl GetAcl(bool onlyBasicPermissions)
        {
            return Binding.GetAclService().GetAcl(RepositoryId, ObjectId, onlyBasicPermissions, null);
        }

        /// <inheritdoc/>
        public virtual IAcl ApplyAcl(IList<IAce> addAces, IList<IAce> removeAces, AclPropagation? aclPropagation)
        {
            IAcl result = Session.ApplyAcl(this, addAces, removeAces, aclPropagation);

            Refresh();

            return result;
        }

        /// <inheritdoc/>
        public virtual IAcl AddAcl(IList<IAce> addAces, AclPropagation? aclPropagation)
        {
            return ApplyAcl(addAces, null, aclPropagation);
        }

        /// <inheritdoc/>
        public virtual IAcl RemoveAcl(IList<IAce> removeAces, AclPropagation? aclPropagation)
        {
            return ApplyAcl(null, removeAces, aclPropagation);
        }

        /// <inheritdoc/>
        public virtual IAcl Acl
        {
            get
            {
                lock (objectLock)
                {
                    return acl;
                }
            }
        }

        // --- policies ---

        /// <inheritdoc/>
        public virtual void ApplyPolicy(params IObjectId[] policyId)
        {
            lock (objectLock)
            {
                Session.ApplyPolicy(this, policyId);
            }

            Refresh();
        }

        /// <inheritdoc/>
        public virtual void RemovePolicy(params IObjectId[] policyId)
        {
            lock (objectLock)
            {
                Session.RemovePolicy(this, policyId);
            }

            Refresh();
        }

        /// <inheritdoc/>
        public virtual IList<IPolicy> Policies
        {
            get
            {
                lock (objectLock)
                {
                    if (policies != null || policyIds == null)
                    {
                        return policies;
                    }

                    policies = new List<IPolicy>();
                    foreach (string pid in policyIds)
                    {
                        try
                        {
                            ICmisObject policy = Session.GetObject(pid);

                            if (policy is IPolicy)
                            {
                                policies.Add((IPolicy)policy);
                            }
                        }
                        catch (CmisObjectNotFoundException)
                        {
                            // ignore
                        }
                    }

                    return policies;
                }
            }
        }

        /// <inheritdoc/>
        public virtual IList<string> PolicyIds
        {
            get
            {
                lock (objectLock)
                {
                    return policyIds;
                }
            }
        }


        // --- relationships ---

        /// <inheritdoc/>
        public virtual IList<IRelationship> Relationships
        {
            get
            {
                lock (objectLock)
                {
                    return relationships;
                }
            }
        }

        // --- extensions ---

        /// <inheritdoc/>
        public virtual IList<ICmisExtensionElement> GetExtensions(ExtensionLevel level)
        {
            IList<ICmisExtensionElement> ext;
            if (extensions.TryGetValue(level, out ext))
            {
                return ext;
            }

            return null;
        }

        // --- other ---

        /// <inheritdoc/>
        public virtual DateTime RefreshTimestamp { get; private set; }

        /// <inheritdoc/>
        public virtual void Refresh()
        {
            lock (objectLock)
            {
                IOperationContext oc = CreationContext;

                // get the latest data from the repository
                IObjectData objectData = Binding.GetObjectService().GetObject(RepositoryId, ObjectId, oc.FilterString, oc.IncludeAllowableActions,
                    oc.IncludeRelationships, oc.RenditionFilterString, oc.IncludePolicies, oc.IncludeAcls, null);

                // reset this object
                Initialize(Session, ObjectType, objectData, CreationContext);
            }
        }

        /// <inheritdoc/>
        public virtual void RefreshIfOld(long durationInMillis)
        {
            lock (objectLock)
            {
                if (((DateTime.UtcNow - RefreshTimestamp).Ticks / 10000) > durationInMillis)
                {
                    Refresh();
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            lock (objectLock)
            {
                if (objectType == null)
                {
                    return "<unknown>";
                }

                return objectType.BaseTypeId + " (" + objectType.Id + "): " + Id;
            }
        }
    }

    /// <summary>
    /// Fileable object base class.
    /// </summary>
    public abstract class AbstractFileableCmisObject : AbstractCmisObject, IFileableCmisObject
    {
        /// <inheritdoc/>
        public virtual IFileableCmisObject Move(IObjectId sourceFolderId, IObjectId targetFolderId)
        {
            string objectId = ObjectId;

            if (sourceFolderId == null || sourceFolderId.Id == null)
            {
                throw new ArgumentException("Source folder ID must be set!", "sourceFolderId");
            }

            if (targetFolderId == null || targetFolderId.Id == null)
            {
                throw new ArgumentException("Target folder ID must be set!", "targetFolderId");
            }

            Binding.GetObjectService().MoveObject(RepositoryId, ref objectId, targetFolderId.Id, sourceFolderId.Id, null);

            if (objectId == null)
            {
                return null;
            }

            IFileableCmisObject movedObject = Session.GetObject(Session.CreateObjectId(objectId)) as IFileableCmisObject;
            if (movedObject == null)
            {
                throw new CmisRuntimeException("Moved object is invalid!");
            }

            return movedObject;
        }

        /// <inheritdoc/>
        public virtual IList<IFolder> Parents
        {
            get
            {
                // get object ids of the parent folders
                IList<IObjectParentData> bindingParents = Binding.GetNavigationService().GetObjectParents(RepositoryId, ObjectId,
                    GetPropertyQueryName(PropertyIds.ObjectId), false, IncludeRelationships.None, null, false, null);

                IList<IFolder> parents = new List<IFolder>();

                foreach (IObjectParentData p in bindingParents)
                {
                    if (p == null || p.Object == null || p.Object.Properties == null)
                    {
                        // should not happen...
                        throw new CmisInvalidServerData("Repository sent invalid data!");
                    }

                    // get id property
                    IPropertyData idProperty = p.Object.Properties[PropertyIds.ObjectId];
                    if (idProperty == null || idProperty.PropertyType != PropertyType.Id)
                    {
                        // the repository sent an object without a valid object id...
                        throw new CmisInvalidServerData("Repository sent invalid data! No object ID!");
                    }

                    // fetch the object and make sure it is a folder
                    IObjectId parentId = Session.CreateObjectId(idProperty.FirstValue as string);
                    IFolder parentFolder = Session.GetObject(parentId) as IFolder;
                    if (parentFolder == null)
                    {
                        // the repository sent an object that is not a folder...
                        throw new CmisInvalidServerData("Repository sent invalid data! Object is not a folder!");
                    }

                    parents.Add(parentFolder);
                }

                return parents;
            }
        }

        /// <inheritdoc/>
        public virtual IList<string> Paths
        {
            get
            {
                // get object paths of the parent folders
                IList<IObjectParentData> parents = Binding.GetNavigationService().GetObjectParents(
                        RepositoryId, ObjectId, GetPropertyQueryName(PropertyIds.Path), false, IncludeRelationships.None,
                        null, true, null);

                IList<string> paths = new List<string>();

                foreach (IObjectParentData p in parents)
                {
                    if (p == null || p.Object == null || p.Object.Properties == null)
                    {
                        // should not happen...
                        throw new CmisInvalidServerData("Repository sent invalid data!");
                    }

                    // get path property
                    IPropertyData pathProperty = p.Object.Properties[PropertyIds.Path];
                    if (pathProperty == null || pathProperty.PropertyType != PropertyType.String)
                    {
                        // the repository sent a folder without a valid path...
                        throw new CmisInvalidServerData("Repository sent invalid data! No path property!");
                    }

                    if (p.RelativePathSegment == null)
                    {
                        // the repository didn't send a relative path segment
                        throw new CmisInvalidServerData("Repository sent invalid data! No relative path segment!");
                    }

                    string folderPath = pathProperty.FirstValue as string;
                    if (folderPath == null)
                    {
                        // the repository sent a folder without a valid path...
                        throw new CmisInvalidServerData("Repository sent invalid data! No path property value!");
                    }
                    paths.Add(folderPath + (folderPath.EndsWith("/") ? "" : "/") + p.RelativePathSegment);
                }

                return paths;
            }
        }

        /// <inheritdoc/>
        public virtual void AddToFolder(IObjectId folderId, bool allVersions)
        {
            if (folderId == null || folderId.Id == null)
            {
                throw new ArgumentException("Folder ID must be set!");
            }

            Binding.GetMultiFilingService().AddObjectToFolder(RepositoryId, ObjectId, folderId.Id, allVersions, null);
        }

        /// <inheritdoc/>
        public virtual void RemoveFromFolder(IObjectId folderId)
        {
            Binding.GetMultiFilingService().RemoveObjectFromFolder(RepositoryId, ObjectId, folderId == null ? null : folderId.Id, null);
        }
    }

    /// <summary>
    /// Document implementation.
    /// </summary>
    public class Document : AbstractFileableCmisObject, IDocument
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the session object</param>
        /// <param name="objectType">the object type</param>
        /// <param name="objectData">the low-level data object</param>
        /// <param name="context">the operation context used to fetch this object</param>
        public Document(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        /// <inheritdoc/>
        public virtual IDocumentType DocumentType
        {
            get
            {
                if (ObjectType is IDocumentType)
                {
                    return (IDocumentType)ObjectType;
                }
                else
                {
                    throw new InvalidCastException("Object type is not a document type.");
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool IsVersionable
        {
            get
            {
                return DocumentType.IsVersionable == true;
            }
        }

        /// <inheritdoc/>
        public virtual bool? IsVersionSeriesPrivateWorkingCopy
        {
            get
            {
                if (DocumentType.IsVersionable == false)
                {
                    return false;
                }

                if (IsVersionSeriesCheckedOut == false)
                {
                    return false;
                }

                bool? isPWC = IsPrivateWorkingCopy;
                if (isPWC != null)
                {
                    return isPWC;
                }

                string vsCoId = VersionSeriesCheckedOutId;
                if (vsCoId == null)
                {
                    // we don't know ...
                    return null;
                }

                return vsCoId == Id;
            }
        }


        // properties

        /// <inheritdoc/>
        public virtual bool? IsImmutable { get { return GetPropertyAsBoolValue(PropertyIds.IsImmutable); } }

        /// <inheritdoc/>
        public virtual bool? IsLatestVersion { get { return GetPropertyAsBoolValue(PropertyIds.IsLatestVersion); } }

        /// <inheritdoc/>
        public virtual bool? IsMajorVersion { get { return GetPropertyAsBoolValue(PropertyIds.IsMajorVersion); } }

        /// <inheritdoc/>
        public virtual bool? IsLatestMajorVersion { get { return GetPropertyAsBoolValue(PropertyIds.IsLatestMajorVersion); } }

        /// <inheritdoc/>
        public virtual bool? IsPrivateWorkingCopy { get { return GetPropertyAsBoolValue(PropertyIds.IsPrivateWorkingCopy); } }

        /// <inheritdoc/>
        public virtual string VersionLabel { get { return GetPropertyAsStringValue(PropertyIds.VersionLabel); } }

        /// <inheritdoc/>
        public virtual string VersionSeriesId { get { return GetPropertyAsStringValue(PropertyIds.VersionSeriesId); } }

        /// <inheritdoc/>
        public virtual bool? IsVersionSeriesCheckedOut { get { return GetPropertyAsBoolValue(PropertyIds.IsVersionSeriesCheckedOut); } }

        /// <inheritdoc/>
        public virtual string VersionSeriesCheckedOutBy { get { return GetPropertyAsStringValue(PropertyIds.VersionSeriesCheckedOutBy); } }

        /// <inheritdoc/>
        public virtual string VersionSeriesCheckedOutId { get { return GetPropertyAsStringValue(PropertyIds.VersionSeriesCheckedOutId); } }

        /// <inheritdoc/>
        public virtual string CheckinComment { get { return GetPropertyAsStringValue(PropertyIds.CheckinComment); } }

        /// <inheritdoc/>
        public virtual long? ContentStreamLength { get { return GetPropertyAsLongValue(PropertyIds.ContentStreamLength); } }

        /// <inheritdoc/>
        public virtual string ContentStreamMimeType { get { return GetPropertyAsStringValue(PropertyIds.ContentStreamMimeType); } }

        /// <inheritdoc/>
        public virtual string ContentStreamFileName { get { return GetPropertyAsStringValue(PropertyIds.ContentStreamFileName); } }

        /// <inheritdoc/>
        public virtual string ContentStreamId { get { return GetPropertyAsStringValue(PropertyIds.ContentStreamId); } }

        /// <inheritdoc/>
        public virtual string LatestAccessibleStateId { get { return GetPropertyAsStringValue(PropertyIds.LatestAccessibleStateId); } }

        /// <inheritdoc/>
        public virtual IList<IContentStreamHash> ContentStreamHashes
        {
            get
            {
                IProperty hashes = this[PropertyIds.ContentStreamHash];
                if (hashes == null || hashes.Values == null || hashes.Values.Count == 0)
                {
                    return null;
                }

                IList<IContentStreamHash> result = new List<IContentStreamHash>();
                foreach (object hash in hashes.Values)
                {
                    result.Add(new ContentStreamHash(Convert.ToString(hash)));
                }

                return result;
            }
        }

        // operations

        /// <inheritdoc/>
        public virtual IDocument Copy(IObjectId targetFolderId, IDictionary<string, object> properties, VersioningState? versioningState,
            IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId;
            try
            {
                newId = Session.CreateDocumentFromSource(this, properties, targetFolderId, versioningState, policies, addAces, removeAces);
            }
            catch (CmisNotSupportedException)
            {
                newId = CopyViaClient(targetFolderId, properties, versioningState, policies, addAces, removeAces);
            }

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }
            // get the new object
            IDocument newDoc = Session.GetObject(newId, context) as IDocument;
            if (newDoc == null)
            {
                throw new CmisRuntimeException("Newly created object is not a document! New ID: " + newId);
            }

            return newDoc;
        }

        /// <inheritdoc/>
        public virtual IDocument Copy(IObjectId targetFolderId)
        {
            return Copy(targetFolderId, null, null, null, null, null, Session.DefaultContext);
        }


        /// <summary>
        /// Copies the document manually. The content is streamed from the repository and back.
        /// </summary>
        protected virtual IObjectId CopyViaClient(IObjectId targetFolderId, IDictionary<string, object> properties,
                VersioningState? versioningState, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces)
        {
            IDictionary<string, object> newProperties = new Dictionary<string, object>();

            IOperationContext allPropsContext = Session.CreateOperationContext();
            allPropsContext.FilterString = "*";
            allPropsContext.IncludeAcls = false;
            allPropsContext.IncludeAllowableActions = false;
            allPropsContext.IncludePathSegments = false;
            allPropsContext.IncludePolicies = false;
            allPropsContext.IncludeRelationships = IncludeRelationships.None;
            allPropsContext.RenditionFilterString = "cmis:none";

            IDocument allPropsDoc = (IDocument)Session.GetObject(this, allPropsContext);

            foreach (IProperty prop in allPropsDoc.Properties)
            {
                if (prop.PropertyDefinition.Updatability == Updatability.ReadWrite
                    || prop.PropertyDefinition.Updatability == Updatability.OnCreate)
                {
                    newProperties.Add(prop.Id, prop.Value);
                }
            }

            if (properties != null)
            {
                foreach (KeyValuePair<string, object> prop in properties)
                {
                    newProperties[prop.Key] = prop.Value;
                }
            }

            IContentStream contentStream = allPropsDoc.GetContentStream();
            try
            {
                return Session.CreateDocument(newProperties, targetFolderId, contentStream, versioningState, policies, addAces, removeAces);
            }
            finally
            {
                if (contentStream != null)
                {
                    Stream stream = contentStream.Stream;
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual void DeleteAllVersions()
        {
            Delete(true);
        }

        // versioning

        /// <inheritdoc/>
        public virtual IObjectId CheckOut()
        {
            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                bool? contentCopied;

                Binding.GetVersioningService().CheckOut(RepositoryId, ref objectId, null, out contentCopied);
                newObjectId = objectId;
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        /// <inheritdoc/>
        public virtual void CancelCheckOut()
        {
            Binding.GetVersioningService().CancelCheckOut(RepositoryId, ObjectId, null);
        }

        /// <inheritdoc/>
        public virtual IObjectId CheckIn(bool major, IDictionary<string, object> properties, IContentStream contentStream,
            string checkinComment, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces)
        {
            String newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;

                IObjectFactory of = Session.ObjectFactory;

                HashSet<Updatability> updatebility = new HashSet<Updatability>();
                updatebility.Add(Updatability.ReadWrite);
                updatebility.Add(Updatability.WhenCheckedOut);

                Binding.GetVersioningService().CheckIn(RepositoryId, ref objectId, major, of.ConvertProperties(properties, ObjectType, SecondaryTypes, updatebility),
                    contentStream, checkinComment, of.ConvertPolicies(policies), of.ConvertAces(addAces), of.ConvertAces(removeAces), null);

                newObjectId = objectId;
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        /// <inheritdoc/>
        public virtual IList<IDocument> GetAllVersions()
        {
            return GetAllVersions(Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IList<IDocument> GetAllVersions(IOperationContext context)
        {
            string objectId;
            string versionSeriesId;

            lock (objectLock)
            {
                objectId = ObjectId;
                versionSeriesId = VersionSeriesId;
            }

            IList<IObjectData> versions = Binding.GetVersioningService().GetAllVersions(RepositoryId, objectId, versionSeriesId,
                context.FilterString, context.IncludeAllowableActions, null);

            IObjectFactory of = Session.ObjectFactory;

            IList<IDocument> result = new List<IDocument>();
            if (versions != null)
            {
                foreach (IObjectData objectData in versions)
                {
                    IDocument doc = of.ConvertObject(objectData, context) as IDocument;
                    if (doc == null)
                    {
                        // should not happen...
                        continue;
                    }

                    result.Add(doc);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual IDocument GetObjectOfLatestVersion(bool major)
        {
            return GetObjectOfLatestVersion(major, Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IDocument GetObjectOfLatestVersion(bool major, IOperationContext context)
        {
            return Session.GetLatestDocumentVersion(this, major, context);
        }

        // content operations

        /// <inheritdoc/>
        public virtual IContentStream GetContentStream()
        {
            return GetContentStream(null);
        }

        /// <inheritdoc/>
        public virtual IContentStream GetContentStream(string streamId)
        {
            return GetContentStream(streamId, null, null);
        }

        /// <inheritdoc/>
        public virtual IContentStream GetContentStream(string streamId, long? offset, long? length)
        {
            IContentStream contentStream = Session.GetContentStream(this, streamId, offset, length);
            if (contentStream == null)
            {
                // no content stream
                return null;
            }

            // the AtomPub binding doesn't return a file name
            // -> get the file name from properties, if present
            if (contentStream.FileName == null && ContentStreamFileName != null)
            {
                ContentStream newContentStream = new ContentStream();
                newContentStream.FileName = ContentStreamFileName;
                newContentStream.Length = contentStream.Length;
                newContentStream.MimeType = contentStream.MimeType;
                newContentStream.Stream = contentStream.Stream;
                newContentStream.Extensions = contentStream.Extensions;

                contentStream = newContentStream;
            }

            return contentStream;
        }

        /// <inheritdoc/>
        public virtual IDocument SetContentStream(IContentStream contentStream, bool overwrite)
        {
            IObjectId objectId = SetContentStream(contentStream, overwrite, true);
            if (objectId == null)
            {
                return null;
            }

            if (ObjectId != objectId.Id)
            {
                return (IDocument)Session.GetObject(objectId, CreationContext);
            }

            return this;
        }

        /// <inheritdoc/>
        public virtual IObjectId SetContentStream(IContentStream contentStream, bool overwrite, bool refresh)
        {
            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                string changeToken = ChangeToken;

                Binding.GetObjectService().SetContentStream(RepositoryId, ref objectId, overwrite, ref changeToken, contentStream, null);

                newObjectId = objectId;
            }

            if (refresh)
            {
                Refresh();
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        /// <inheritdoc/>
        public virtual IDocument AppendContentStream(IContentStream contentStream, bool isLastChunk)
        {
            IObjectId objectId = AppendContentStream(contentStream, isLastChunk, true);
            if (objectId == null)
            {
                return null;
            }

            if (ObjectId != objectId.Id)
            {
                return (IDocument)Session.GetObject(objectId, CreationContext);
            }

            return this;
        }

        /// <inheritdoc/>
        public virtual IObjectId AppendContentStream(IContentStream contentStream, bool isLastChunk, bool refresh)
        {
            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                string changeToken = ChangeToken;

                Binding.GetObjectService().AppendContentStream(RepositoryId, ref objectId, isLastChunk, ref changeToken, contentStream, null);

                newObjectId = objectId;
            }

            if (refresh)
            {
                Refresh();
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        /// <inheritdoc/>
        public virtual IDocument DeleteContentStream()
        {
            IObjectId objectId = DeleteContentStream(true);
            if (objectId == null)
            {
                return null;
            }

            if (ObjectId != objectId.Id)
            {
                return (IDocument)Session.GetObject(objectId, CreationContext);
            }

            return this;
        }

        /// <inheritdoc/>
        public virtual IObjectId DeleteContentStream(bool refresh)
        {
            string newObjectId = null;

            lock (objectLock)
            {
                string objectId = ObjectId;
                string changeToken = ChangeToken;

                Binding.GetObjectService().DeleteContentStream(RepositoryId, ref objectId, ref changeToken, null);

                newObjectId = objectId;
            }

            if (refresh)
            {
                Refresh();
            }

            if (newObjectId == null)
            {
                return null;
            }

            return Session.CreateObjectId(newObjectId);
        }

        /// <inheritdoc/>
        public virtual IObjectId CheckIn(bool major, IDictionary<String, object> properties, IContentStream contentStream, string checkinComment)
        {
            return this.CheckIn(major, properties, contentStream, checkinComment, null, null, null);
        }
    }

    /// <summary>
    /// Folder implementation.
    /// </summary>
    public class Folder : AbstractFileableCmisObject, IFolder
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the session object</param>
        /// <param name="objectType">the object type</param>
        /// <param name="objectData">the low-level data object</param>
        /// <param name="context">the operation context used to fetch this object</param>
        public Folder(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        /// <inheritdoc/>
        public virtual IFolderType FolderType
        {
            get
            {
                if (ObjectType is IFolderType)
                {
                    return (IFolderType)ObjectType;
                }
                else
                {
                    throw new InvalidCastException("Object type is not a folder type.");
                }
            }
        }

        /// <inheritdoc/>
        public virtual IDocument CreateDocument(IDictionary<string, object> properties, IContentStream contentStream, VersioningState? versioningState,
            IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreateDocument(properties, this, contentStream, versioningState, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IDocument newDoc = Session.GetObject(newId, context) as IDocument;
            if (newDoc == null)
            {
                throw new CmisRuntimeException("Newly created object is not a document! New ID: " + newId);
            }

            return newDoc;
        }

        /// <inheritdoc/>
        public virtual IDocument CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, VersioningState? versioningState,
            IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreateDocumentFromSource(source, properties, this, versioningState, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IDocument newDoc = Session.GetObject(newId, context) as IDocument;
            if (newDoc == null)
            {
                throw new CmisRuntimeException("Newly created object is not a document! New ID: " + newId);
            }

            return newDoc;
        }

        /// <inheritdoc/>
        public virtual IFolder CreateFolder(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreateFolder(properties, this, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IFolder newFolder = Session.GetObject(newId, context) as IFolder;
            if (newFolder == null)
            {
                throw new CmisRuntimeException("Newly created object is not a folder! New ID: " + newId);
            }

            return newFolder;
        }

        /// <inheritdoc/>
        public virtual IPolicy CreatePolicy(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreatePolicy(properties, this, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IPolicy newPolicy = Session.GetObject(newId, context) as IPolicy;
            if (newPolicy == null)
            {
                throw new CmisRuntimeException("Newly created object is not a policy! New ID: " + newId);
            }

            return newPolicy;
        }

        /// <inheritdoc/>
        public virtual IItem CreateItem(IDictionary<string, object> properties, IList<IPolicy> policies, IList<IAce> addAces, IList<IAce> removeAces, IOperationContext context)
        {
            IObjectId newId = Session.CreateItem(properties, this, policies, addAces, removeAces);

            // if no context is provided the object will not be fetched
            if (context == null || newId == null)
            {
                return null;
            }

            // get the new object
            IItem newItem = Session.GetObject(newId, context) as IItem;
            if (newItem == null)
            {
                throw new CmisRuntimeException("Newly created object is not an item! New ID: " + newId);
            }

            return newItem;
        }

        /// <inheritdoc/>
        public virtual IList<string> DeleteTree(bool allVersions, UnfileObject? unfile, bool continueOnFailure)
        {
            return Session.DeleteTree(this, allVersions, unfile, continueOnFailure);
        }

        /// <inheritdoc/>
        public virtual string ParentId { get { return GetPropertyAsStringValue(PropertyIds.ParentId); } }

        /// <inheritdoc/>
        public virtual IList<IObjectType> AllowedChildObjectTypes
        {
            get
            {
                IList<IObjectType> result = new List<IObjectType>();

                lock (objectLock)
                {
                    IList<string> otids = GetPropertyValue(PropertyIds.AllowedChildObjectTypeIds) as IList<string>;
                    if (otids == null)
                    {
                        return result;
                    }

                    foreach (string otid in otids)
                    {
                        result.Add(Session.GetTypeDefinition(otid));
                    }
                }

                return result;
            }
        }

        /// <inheritdoc/>
        public virtual IItemEnumerable<IDocument> GetCheckedOutDocs()
        {
            return GetCheckedOutDocs(Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IItemEnumerable<IDocument> GetCheckedOutDocs(IOperationContext context)
        {
            string objectId = ObjectId;
            INavigationService service = Binding.GetNavigationService();
            IObjectFactory of = Session.ObjectFactory;
            IOperationContext ctxt = new OperationContext(context);

            PageFetcher<IDocument>.FetchPage fetchPageDelegate = delegate (BigInteger maxNumItems, BigInteger skipCount)
            {
                // get checked out documents for this folder
                IObjectList checkedOutDocs = service.GetCheckedOutDocs(RepositoryId, objectId, ctxt.FilterString, ctxt.OrderBy, ctxt.IncludeAllowableActions,
                    ctxt.IncludeRelationships, ctxt.RenditionFilterString, maxNumItems, skipCount, null);

                IList<IDocument> page = new List<IDocument>();
                if (checkedOutDocs.Objects != null)
                {
                    foreach (IObjectData objectData in checkedOutDocs.Objects)
                    {
                        IDocument doc = of.ConvertObject(objectData, ctxt) as IDocument;
                        if (doc == null)
                        {
                            // should not happen...
                            continue;
                        }

                        page.Add(doc);
                    }
                }


                return new PageFetcher<IDocument>.Page<IDocument>(page, checkedOutDocs.NumItems, checkedOutDocs.HasMoreItems);
            };

            return new CollectionEnumerable<IDocument>(new PageFetcher<IDocument>(ctxt.MaxItemsPerPage, fetchPageDelegate));
        }

        /// <inheritdoc/>
        public virtual IItemEnumerable<ICmisObject> GetChildren()
        {
            return GetChildren(Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IItemEnumerable<ICmisObject> GetChildren(IOperationContext context)
        {
            string objectId = ObjectId;
            INavigationService service = Binding.GetNavigationService();
            IObjectFactory of = Session.ObjectFactory;
            IOperationContext ctxt = new OperationContext(context);

            PageFetcher<ICmisObject>.FetchPage fetchPageDelegate = delegate (BigInteger maxNumItems, BigInteger skipCount)
            {
                // get the children
                IObjectInFolderList children = service.GetChildren(RepositoryId, objectId, ctxt.FilterString, ctxt.OrderBy, ctxt.IncludeAllowableActions,
                    ctxt.IncludeRelationships, ctxt.RenditionFilterString, ctxt.IncludePathSegments, maxNumItems, skipCount, null);

                // convert objects
                IList<ICmisObject> page = new List<ICmisObject>();
                if (children.Objects != null)
                {
                    foreach (IObjectInFolderData objectData in children.Objects)
                    {
                        if (objectData.Object != null)
                        {
                            page.Add(of.ConvertObject(objectData.Object, ctxt));
                        }
                    }
                }

                return new PageFetcher<ICmisObject>.Page<ICmisObject>(page, children.NumItems, children.HasMoreItems);
            };

            return new CollectionEnumerable<ICmisObject>(new PageFetcher<ICmisObject>(ctxt.MaxItemsPerPage, fetchPageDelegate));
        }

        /// <inheritdoc/>
        public virtual IList<ITree<IFileableCmisObject>> GetDescendants(int depth)
        {
            return GetDescendants(depth, Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IList<ITree<IFileableCmisObject>> GetDescendants(int depth, IOperationContext context)
        {
            IList<IObjectInFolderContainer> bindingContainerList = Binding.GetNavigationService().GetDescendants(RepositoryId, ObjectId, depth,
                context.FilterString, context.IncludeAllowableActions, context.IncludeRelationships, context.RenditionFilterString,
                context.IncludePathSegments, null);

            return ConvertProviderContainer(bindingContainerList, context);
        }

        /// <inheritdoc/>
        public virtual IList<ITree<IFileableCmisObject>> GetFolderTree(int depth)
        {
            return GetFolderTree(depth, Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IList<ITree<IFileableCmisObject>> GetFolderTree(int depth, IOperationContext context)
        {
            IList<IObjectInFolderContainer> bindingContainerList = Binding.GetNavigationService().GetFolderTree(RepositoryId, ObjectId, depth,
                context.FilterString, context.IncludeAllowableActions, context.IncludeRelationships, context.RenditionFilterString,
                context.IncludePathSegments, null);

            return ConvertProviderContainer(bindingContainerList, context);
        }

        private IList<ITree<IFileableCmisObject>> ConvertProviderContainer(IList<IObjectInFolderContainer> bindingContainerList, IOperationContext context)
        {
            if (bindingContainerList == null || bindingContainerList.Count == 0)
            {
                return null;
            }

            IList<ITree<IFileableCmisObject>> result = new List<ITree<IFileableCmisObject>>();
            foreach (IObjectInFolderContainer oifc in bindingContainerList)
            {
                if (oifc.Object == null || oifc.Object.Object == null)
                {
                    // shouldn't happen ...
                    continue;
                }

                // convert the object
                IFileableCmisObject cmisObject = Session.ObjectFactory.ConvertObject(oifc.Object.Object, context) as IFileableCmisObject;
                if (cmisObject == null)
                {
                    // the repository must not return objects that are not fileable, but you never know...
                    continue;
                }

                // convert the children
                IList<ITree<IFileableCmisObject>> children = ConvertProviderContainer(oifc.Children, context);

                // add both to current container
                Tree<IFileableCmisObject> tree = new Tree<IFileableCmisObject>();
                tree.Item = cmisObject;
                tree.Children = children;

                result.Add(tree);
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual bool IsRootFolder { get { return ObjectId == Session.RepositoryInfo.RootFolderId; } }

        /// <inheritdoc/>
        public virtual IFolder FolderParent
        {
            get
            {
                if (IsRootFolder)
                {
                    return null;
                }

                IList<IFolder> parents = Parents;
                if (parents == null || parents.Count == 0)
                {
                    return null;
                }

                return parents[0];
            }
        }

        /// <inheritdoc/>
        public virtual string Path
        {
            get
            {
                string path;

                lock (objectLock)
                {
                    // get the path property
                    path = GetPropertyAsStringValue(PropertyIds.Path);

                    // if the path property isn't set, get it
                    if (path == null)
                    {
                        IObjectData objectData = Binding.GetObjectService().GetObject(RepositoryId, ObjectId,
                                GetPropertyQueryName(PropertyIds.Path), false, IncludeRelationships.None, "cmis:none", false,
                                false, null);

                        if (objectData.Properties != null)
                        {
                            IPropertyData pathProperty = objectData.Properties[PropertyIds.Path];
                            if (pathProperty != null && pathProperty.PropertyType == PropertyType.String)
                            {
                                path = pathProperty.FirstValue as string;
                            }
                        }
                    }
                }

                // we still don't know the path ... it's not a CMIS compliant repository
                if (path == null)
                {
                    throw new CmisInvalidServerData("Repository didn't return " + PropertyIds.Path + "!");
                }

                return path;
            }
        }

        /// <inheritdoc/>
        public override IList<string> Paths
        {
            get
            {
                IList<string> result = new List<string>();
                result.Add(Path);

                return result;
            }
        }

        /// <inheritdoc/>
        public virtual IDocument CreateDocument(IDictionary<string, object> properties, IContentStream contentStream, VersioningState? versioningState)
        {
            return CreateDocument(properties, contentStream, versioningState, null, null, null, Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IDocument CreateDocumentFromSource(IObjectId source, IDictionary<string, object> properties, VersioningState? versioningState)
        {
            return CreateDocumentFromSource(source, properties, versioningState, null, null, null, Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IFolder CreateFolder(IDictionary<string, object> properties)
        {
            return CreateFolder(properties, null, null, null, Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IPolicy CreatePolicy(IDictionary<string, object> properties)
        {
            return CreatePolicy(properties, null, null, null, Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IItem CreateItem(IDictionary<string, object> properties)
        {
            return CreateItem(properties, null, null, null, Session.DefaultContext);
        }
    }

    /// <summary>
    /// Policy implementation.
    /// </summary>
    public class Policy : AbstractFileableCmisObject, IPolicy
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the session object</param>
        /// <param name="objectType">the object type</param>
        /// <param name="objectData">the low-level data object</param>
        /// <param name="context">the operation context used to fetch this object</param>
        public Policy(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        /// <inheritdoc/>
        public virtual IPolicyType PolicyType
        {
            get
            {
                if (ObjectType is IPolicyType)
                {
                    return (IPolicyType)ObjectType;
                }
                else
                {
                    throw new InvalidCastException("Object type is not a policy type.");
                }
            }
        }

        /// <inheritdoc/>
        public virtual string PolicyText { get { return GetPropertyAsStringValue(PropertyIds.PolicyText); } }
    }

    /// <summary>
    /// Relationship implementation.
    /// </summary>
    public class Relationship : AbstractCmisObject, IRelationship
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the session object</param>
        /// <param name="objectType">the object type</param>
        /// <param name="objectData">the low-level data object</param>
        /// <param name="context">the operation context used to fetch this object</param>
        public Relationship(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        /// <inheritdoc/>
        public virtual IRelationshipType RelationshipType
        {
            get
            {
                if (ObjectType is IRelationshipType)
                {
                    return (IRelationshipType)ObjectType;
                }
                else
                {
                    throw new InvalidCastException("Object type is not a relationship type.");
                }
            }
        }

        /// <inheritdoc/>
        public virtual ICmisObject GetSource()
        {
            return GetSource(Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual ICmisObject GetSource(IOperationContext context)
        {
            lock (objectLock)
            {
                IObjectId sourceId = SourceId;
                if (sourceId == null)
                {
                    return null;
                }

                return Session.GetObject(sourceId, context);
            }
        }

        /// <inheritdoc/>
        public virtual IObjectId SourceId
        {
            get
            {
                string sourceId = GetPropertyAsStringValue(PropertyIds.SourceId);
                if (sourceId == null || sourceId.Length == 0)
                {
                    return null;
                }

                return Session.CreateObjectId(sourceId);
            }
        }

        /// <inheritdoc/>
        public virtual ICmisObject GetTarget()
        {
            return GetTarget(Session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual ICmisObject GetTarget(IOperationContext context)
        {
            lock (objectLock)
            {
                IObjectId targetId = TargetId;
                if (targetId == null)
                {
                    return null;
                }

                return Session.GetObject(targetId, context);
            }
        }

        /// <inheritdoc/>
        public virtual IObjectId TargetId
        {
            get
            {
                string targetId = GetPropertyAsStringValue(PropertyIds.TargetId);
                if (targetId == null || targetId.Length == 0)
                {
                    return null;
                }

                return Session.CreateObjectId(targetId);
            }
        }
    }

    /// <summary>
    /// Item implementation.
    /// </summary>
    public class Item : AbstractFileableCmisObject, IItem
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the session object</param>
        /// <param name="objectType">the object type</param>
        /// <param name="objectData">the low-level data object</param>
        /// <param name="context">the operation context used to fetch this object</param>
        public Item(ISession session, IObjectType objectType, IObjectData objectData, IOperationContext context)
        {
            Initialize(session, objectType, objectData, context);
        }

        /// <inheritdoc/>
        public virtual IItemType ItemType
        {
            get
            {
                if (ObjectType is IItemType)
                {
                    return (IItemType)ObjectType;
                }
                else
                {
                    throw new InvalidCastException("Object type is not an item type.");
                }
            }
        }
    }

    /// <summary>
    /// Property implementation.
    /// </summary>
    public class Property : IProperty
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyDefinition">the property definition</param>
        /// <param name="values">the property values</param>
        public Property(IPropertyDefinition propertyDefinition, IList<object> values)
        {
            PropertyDefinition = propertyDefinition;
            Values = values;
        }

        /// <inheritdoc/>
        public string Id { get { return PropertyDefinition.Id; } }

        /// <inheritdoc/>
        public string LocalName { get { return PropertyDefinition.LocalName; } }

        /// <inheritdoc/>
        public string DisplayName { get { return PropertyDefinition.DisplayName; } }

        /// <inheritdoc/>
        public string QueryName { get { return PropertyDefinition.QueryName; } }

        /// <inheritdoc/>
        public bool IsMultiValued { get { return PropertyDefinition.Cardinality == Cardinality.Multi; } }

        /// <inheritdoc/>
        public PropertyType? PropertyType { get { return PropertyDefinition.PropertyType; } }

        /// <inheritdoc/>
        public IPropertyDefinition PropertyDefinition { get; protected set; }

        /// <inheritdoc/>
        public object Value
        {
            get
            {
                if (PropertyDefinition.Cardinality == Cardinality.Single)
                {
                    return Values == null || Values.Count == 0 ? null : Values[0];
                }
                else
                {
                    return Values;
                }
            }
        }

        /// <inheritdoc/>
        public IList<object> Values { get; protected set; }

        /// <inheritdoc/>
        public object FirstValue { get { return Values == null || Values.Count == 0 ? null : Values[0]; } }

        /// <inheritdoc/>
        public string ValueAsString { get { return FormatValue(FirstValue); } }

        /// <inheritdoc/>
        public string ValuesAsString
        {
            get
            {
                if (Values == null)
                {
                    return "[]";
                }
                else
                {
                    StringBuilder result = new StringBuilder();
                    foreach (object value in Values)
                    {
                        if (result.Length > 0)
                        {
                            result.Append(", ");
                        }

                        result.Append(FormatValue(value));
                    }

                    return "[" + result.ToString() + "]";
                }
            }
        }

        private string FormatValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            // for future formating

            return value.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Id + ": " + ValuesAsString;
        }
    }

    /// <summary>
    /// Rendition implementation.
    /// </summary>
    public class Rendition : RenditionData, IRendition
    {
        private ISession session;
        private string objectId;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the session objec</param>
        /// <param name="objectId">the object ID</param>
        /// <param name="streamId">the stream ID</param>
        /// <param name="mimeType">the MIME type</param>
        /// <param name="length">the length in bytes, if known</param>
        /// <param name="kind">the kind</param>
        /// <param name="title">the title</param>
        /// <param name="height">the thumbnail height</param>
        /// <param name="width">the thumbnail width</param>
        /// <param name="renditionDocumentId">the ID of the stand-alone rendition document, if it exists</param>
        public Rendition(ISession session, string objectId, string streamId, string mimeType, BigInteger? length, string kind,
            string title, BigInteger? height, BigInteger? width, string renditionDocumentId)
        {
            this.session = session;
            this.objectId = objectId;

            StreamId = streamId;
            MimeType = mimeType;
            Length = length;
            Kind = kind;
            Title = title;
            Height = height;
            Width = width;
            RenditionDocumentId = renditionDocumentId;
        }

        /// <inheritdoc/>
        public virtual IDocument GetRenditionDocument()
        {
            return GetRenditionDocument(session.DefaultContext);
        }

        /// <inheritdoc/>
        public virtual IDocument GetRenditionDocument(IOperationContext context)
        {
            if (RenditionDocumentId == null)
            {
                return null;
            }

            return session.GetObject(session.CreateObjectId(RenditionDocumentId), context) as IDocument;
        }

        /// <inheritdoc/>
        public virtual IContentStream GetContentStream()
        {
            if (objectId == null || StreamId == null)
            {
                return null;
            }

            return session.Binding.GetObjectService().GetContentStream(session.RepositoryInfo.Id, objectId, StreamId, null, null, null);
        }
    }

    /// <summary>
    /// Content Stream Hash implementation.
    /// </summary>
    public class ContentStreamHash : IContentStreamHash
    {
        /// <summary>Algorithm MD5</summary>
        public const string AlgorithmMD5 = "md5";

        /// <summary>Algorithm sha-1</summary>
        public const string AlgorithmSHA1 = "sha-1";

        /// <summary>Algorithm sha-224</summary>
        public const string AlgorithmSHA224 = "sha-224";

        /// <summary>Algorithm sha-256</summary>
        public const string AlgorithmSHA256 = "sha-256";

        /// <summary>Algorithm sha-384</summary>
        public const string AlgorithmSHA384 = "sha-384";

        /// <summary>Algorithm sha-512</summary>
        public const string AlgorithmSHA512 = "sha-512";

        /// <summary>Algorithm sha-3</summary>
        public const string AlgorithmSHA3 = "sha-3";

        /// <inheritdoc/>
        public string PropertyValue { get; protected set; }

        /// <inheritdoc/>
        public string Algorithm { get; protected set; }

        /// <inheritdoc/>
        public string Hash { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyValue">a property value</param>
        public ContentStreamHash(string propertyValue)
        {
            PropertyValue = propertyValue;

            if (propertyValue == null)
            {
                return;
            }

            string pv = propertyValue.Trim();
            int algEnd = pv.IndexOf('}');
            if (pv[0] != '{' || algEnd < 1)
            {
                return;
            }

            Algorithm = pv.Substring(1, algEnd - 1).ToLowerInvariant();
            Hash = pv.Substring(algEnd + 1).Replace(" ", "").ToLowerInvariant();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="algorithm">the algorithm</param>
        /// <param name="hashStr">the hash string</param>
        public ContentStreamHash(string algorithm, string hashStr)
        {
            if (algorithm == null || algorithm.Trim().Length == 0)
            {
                throw new ArgumentException("Algorithm must be set!", "algorithm");
            }

            if (hashStr == null || hashStr.Trim().Length == 0)
            {
                throw new ArgumentException("Hash must be set!", "hashStr");
            }

            Algorithm = algorithm.ToLowerInvariant();
            Hash = hashStr.Replace(" ", "").ToLowerInvariant();
            PropertyValue = "{" + Algorithm + "}" + Hash;
        }
    }

    /// <summary>
    /// Query Result implementation.
    /// </summary>
    public class QueryResult : IQueryResult
    {
        private IDictionary<string, IPropertyData> propertiesById;
        private IDictionary<string, IPropertyData> propertiesByQueryName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">the current session</param>
        /// <param name="objectData">the query hit</param>
        public QueryResult(ISession session, IObjectData objectData)
        {
            Initialize(session, objectData);
        }

        /// <summary>
        /// Initializes the query result object.
        /// </summary>
        /// <param name="session">the current session</param>
        /// <param name="objectData">the query hit</param>
        protected void Initialize(ISession session, IObjectData objectData)
        {
            if (objectData != null)
            {
                IObjectFactory of = session.ObjectFactory;

                // handle properties
                if (objectData.Properties != null)
                {
                    Properties = new List<IPropertyData>();
                    propertiesById = new Dictionary<string, IPropertyData>();
                    propertiesByQueryName = new Dictionary<string, IPropertyData>();

                    IList<IPropertyData> queryProperties = of.ConvertQueryProperties(objectData.Properties);

                    foreach (IPropertyData property in queryProperties)
                    {
                        Properties.Add(property);
                        if (property.Id != null)
                        {
                            propertiesById[property.Id] = property;
                        }
                        if (property.QueryName != null)
                        {
                            propertiesByQueryName[property.QueryName] = property;
                        }
                    }
                }

                // handle allowable actions
                AllowableActions = objectData.AllowableActions;

                // handle relationships
                if (objectData.Relationships != null)
                {
                    Relationships = new List<IRelationship>();
                    foreach (IObjectData rod in objectData.Relationships)
                    {
                        IRelationship relationship = of.ConvertObject(rod, session.DefaultContext) as IRelationship;
                        if (relationship != null)
                        {
                            Relationships.Add(relationship);
                        }
                    }
                }

                // handle renditions
                if (objectData.Renditions != null)
                {
                    Renditions = new List<IRendition>();
                    foreach (IRenditionData rd in objectData.Renditions)
                    {
                        Renditions.Add(of.ConvertRendition(null, rd));
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IPropertyData this[string queryName]
        {
            get
            {
                if (queryName == null)
                {
                    return null;
                }

                IPropertyData result;
                if (propertiesByQueryName.TryGetValue(queryName, out result))
                {
                    return result;
                }

                return null;
            }
        }

        /// <inheritdoc/>
        public IList<IPropertyData> Properties { get; protected set; }

        /// <inheritdoc/>
        public IPropertyData GetPropertyById(string propertyId)
        {
            if (propertyId == null)
            {
                return null;
            }

            IPropertyData result;
            if (propertiesById.TryGetValue(propertyId, out result))
            {
                return result;
            }

            return null;
        }

        /// <inheritdoc/>
        public object GetPropertyValueByQueryName(string queryName)
        {
            IPropertyData property = this[queryName];
            if (property == null)
            {
                return null;
            }

            return property.FirstValue;
        }

        /// <inheritdoc/>
        public object GetPropertyValueById(string propertyId)
        {
            IPropertyData property = GetPropertyById(propertyId);
            if (property == null)
            {
                return null;
            }

            return property.FirstValue;
        }

        /// <inheritdoc/>
        public IList<object> GetPropertyMultivalueByQueryName(string queryName)
        {
            IPropertyData property = this[queryName];
            if (property == null)
            {
                return null;
            }

            return property.Values;
        }

        /// <inheritdoc/>
        public IList<object> GetPropertyMultivalueById(string propertyId)
        {
            IPropertyData property = GetPropertyById(propertyId);
            if (property == null)
            {
                return null;
            }

            return property.Values;
        }

        /// <inheritdoc/>
        public IAllowableActions AllowableActions { get; protected set; }

        /// <inheritdoc/>
        public IList<IRelationship> Relationships { get; protected set; }

        /// <inheritdoc/>
        public IList<IRendition> Renditions { get; protected set; }
    }

    /// <summary>
    /// Change Event implementation.
    /// </summary>
    public class ChangeEvent : ChangeEventInfo, IChangeEvent
    {
        /// <inheritdoc/>
        public virtual string ObjectId { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, IList<object>> Properties { get; set; }

        /// <inheritdoc/>
        public virtual IList<string> PolicyIds { get; set; }

        /// <inheritdoc/>
        public virtual IAcl Acl { get; set; }
    }

    /// <summary>
    /// Change Events implementation.
    /// </summary>
    public class ChangeEvents : IChangeEvents
    {
        /// <inheritdoc/>
        public virtual string LatestChangeLogToken { get; set; }

        /// <inheritdoc/>
        public virtual IList<IChangeEvent> ChangeEventList { get; set; }

        /// <inheritdoc/>
        public virtual bool? HasMoreItems { get; set; }

        /// <inheritdoc/>
        public virtual BigInteger? TotalNumItems { get; set; }
    }
}
