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
    public static class PermissionMappingKeys
    {
        public const string CanGetDescendentsFolder = "canGetDescendents.Folder";
        public const string CanGetChildrenFolder = "canGetChildren.Folder";
        public const string CanGetParentsFolder = "canGetParents.Folder";
        public const string CanGetFolderParentObject = "canGetFolderParent.Object";
        public const string CanCreateDocumentFolder = "canCreateDocument.Folder";
        public const string CanCreateFolderFolder = "canCreateFolder.Folder";
        public const string CanCreateRelationshipSource = "canCreateRelationship.Source";
        public const string CanCreateRelationshipTarget = "canCreateRelationship.Target";
        public const string CanGetPropertiesObject = "canGetProperties.Object";
        public const string CanViewContentObject = "canViewContent.Object";
        public const string CanUpdatePropertiesObject = "canUpdateProperties.Object";
        public const string CanMoveObject = "canMove.Object";
        public const string CanMoveTarget = "canMove.Target";
        public const string CanMoveSource = "canMove.Source";
        public const string CanDeleteObject = "canDelete.Object";
        public const string CanDeleteTreeFolder = "canDeleteTree.Folder";
        public const string CanSetContentDocument = "canSetContent.Document";
        public const string CanDeleteContentDocument = "canDeleteContent.Document";
        public const string CanAddToFolderObject = "canAddToFolder.Object";
        public const string CanAddToFolderFolder = "canAddToFolder.Folder";
        public const string CanRemoveFromFolderObject = "canRemoveFromFolder.Object";
        public const string CanRemoveFromFolderFolder = "canRemoveFromFolder.Folder";
        public const string CanCheckoutDocument = "canCheckout.Document";
        public const string CanCancelCheckoutDocument = "canCancelCheckout.Document";
        public const string CanCheckinDocument = "canCheckin.Document";
        public const string CanGetAllVersionsVersionSeries = "canGetAllVersions.VersionSeries";
        public const string CanGetObjectRelationshipSObject = "canGetObjectRelationships.Object";
        public const string CanAddPolicyObject = "canAddPolicy.Object";
        public const string CanAddPolicyPolicy = "canAddPolicy.Policy";
        public const string CanRemovePolicyObject = "canRemovePolicy.Object";
        public const string CanRemovePolicyPolicy = "canRemovePolicy.Policy";
        public const string CanGetAppliesPoliciesObject = "canGetAppliedPolicies.Object";
        public const string CanGetAclObject = "canGetAcl.Object";
        public const string CanApplyAclObject = "canApplyAcl.Object";
    }
}
