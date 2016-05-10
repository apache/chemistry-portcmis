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
    /// Keys for permission mappings.
    /// </summary>
    public static class PermissionMappingKeys
    {
        /// <summary>Key <c>canGetDescendents.Folder</c></summary>
        public const string CanGetDescendentsFolder = "canGetDescendents.Folder";
        /// <summary>Key <c>canGetChildren.Folder</c></summary>
        public const string CanGetChildrenFolder = "canGetChildren.Folder";
        /// <summary>Key <c>canGetParents.Folder</c></summary>
        public const string CanGetParentsFolder = "canGetParents.Folder";
        /// <summary>Key <c>canGetFolderParent.Object</c></summary>
        public const string CanGetFolderParentObject = "canGetFolderParent.Object";
        /// <summary>Key <c>canCreateDocument.Folder</c></summary>
        public const string CanCreateDocumentFolder = "canCreateDocument.Folder";
        /// <summary>Key <c>canCreateFolder.Folder</c></summary>
        public const string CanCreateFolderFolder = "canCreateFolder.Folder";
        /// <summary>Key <c>canCreateRelationship.Source</c></summary>
        public const string CanCreateRelationshipSource = "canCreateRelationship.Source";
        /// <summary>Key <c>canCreateRelationship.Target</c></summary>
        public const string CanCreateRelationshipTarget = "canCreateRelationship.Target";
        /// <summary>Key <c>canGetProperties.Object</c></summary>
        public const string CanGetPropertiesObject = "canGetProperties.Object";
        /// <summary>Key <c>canViewContent.Object</c></summary>
        public const string CanViewContentObject = "canViewContent.Object";
        /// <summary>Key <c>canUpdateProperties.Object</c></summary>
        public const string CanUpdatePropertiesObject = "canUpdateProperties.Object";
        /// <summary>Key <c>canMove.Object</c></summary>
        public const string CanMoveObject = "canMove.Object";
        /// <summary>Key <c>canMove.Target</c></summary>
        public const string CanMoveTarget = "canMove.Target";
        /// <summary>Key <c>canMove.Source</c></summary>
        public const string CanMoveSource = "canMove.Source";
        /// <summary>Key <c>canDelete.Objectr</c></summary>
        public const string CanDeleteObject = "canDelete.Object";
        /// <summary>Key <c>canDeleteTree.Folder</c></summary>
        public const string CanDeleteTreeFolder = "canDeleteTree.Folder";
        /// <summary>Key <c>canSetContent.Document</c></summary>
        public const string CanSetContentDocument = "canSetContent.Document";
        /// <summary>Key <c>canDeleteContent.Document</c></summary>
        public const string CanDeleteContentDocument = "canDeleteContent.Document";
        /// <summary>Key <c>canAddToFolder.Object</c></summary>
        public const string CanAddToFolderObject = "canAddToFolder.Object";
        /// <summary>Key <c>canAddToFolder.Folder</c></summary>
        public const string CanAddToFolderFolder = "canAddToFolder.Folder";
        /// <summary>Key <c>canRemoveFromFolder.Object</c></summary>
        public const string CanRemoveFromFolderObject = "canRemoveFromFolder.Object";
        /// <summary>Key <c>canRemoveFromFolder.Folder</c></summary>
        public const string CanRemoveFromFolderFolder = "canRemoveFromFolder.Folder";
        /// <summary>Key <c>canCheckout.Document</c></summary>
        public const string CanCheckoutDocument = "canCheckout.Document";
        /// <summary>Key <c>canCancelCheckout.Document</c></summary>
        public const string CanCancelCheckoutDocument = "canCancelCheckout.Document";
        /// <summary>Key <c>canCheckin.Document</c></summary>
        public const string CanCheckinDocument = "canCheckin.Document";
        /// <summary>Key <c>canGetAllVersions.VersionSeries</c></summary>
        public const string CanGetAllVersionsVersionSeries = "canGetAllVersions.VersionSeries";
        /// <summary>Key <c>canGetObjectRelationships.Object</c></summary>
        public const string CanGetObjectRelationshipSObject = "canGetObjectRelationships.Object";
        /// <summary>Key <c>canAddPolicy.Object</c></summary>
        public const string CanAddPolicyObject = "canAddPolicy.Object";
        /// <summary>Key <c>canAddPolicy.Policy</c></summary>
        public const string CanAddPolicyPolicy = "canAddPolicy.Policy";
        /// <summary>Key <c>canRemovePolicy.Object</c></summary>
        public const string CanRemovePolicyObject = "canRemovePolicy.Object";
        /// <summary>Key <c>canRemovePolicy.Policy</c></summary>
        public const string CanRemovePolicyPolicy = "canRemovePolicy.Policy";
        /// <summary>Key <c>canGetAppliedPolicies.Object</c></summary>
        public const string CanGetAppliesPoliciesObject = "canGetAppliedPolicies.Object";
        /// <summary>Key <c>canGetAcl.Object</c></summary>
        public const string CanGetAclObject = "canGetAcl.Object";
        /// <summary>Key <c>canApplyAcl.Object</c></summary>
        public const string CanApplyAclObject = "canApplyAcl.Object";
    }
}
