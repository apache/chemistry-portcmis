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
* KIND, either express or implied. See the License for the
* specific language governing permissions and limitations
* under the License.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortCMIS;
using PortCMIS.Client;
using PortCMIS.Client.Impl;
using PortCMIS.Data;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using PortCMIS.Utils;
using PortCMISTests.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortCMISTests
{
    [TestClass]
    public class SimpleCmisTest : TestFramework
    {
        public static ISession Session { get; set; }

        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        [ClassInitialize]
        public static void CreateSession(TestContext testContext)
        {
            Logger.Level = Logger.LogLevel.Debug;
            Session = Connect();
        }

        [TestMethod]
        public void TestRepositoryInfo()
        {
            Assert.IsNotNull(Session);
            Assert.IsNotNull(Session.RepositoryInfo);
            Assert.IsNotNull(Session.RepositoryInfo.Id);

            TestContext.WriteLine("Repository ID: " + Session.RepositoryInfo.Id);
        }

        [TestMethod]
        public void TestRootFolder()
        {
            IOperationContext oc = Session.CreateOperationContext();
            oc.FilterString = "*";
            oc.IncludeAllowableActions = true;

            IFolder root = Session.GetRootFolder(oc);
            Assert.IsNotNull(root);
            Assert.IsNotNull(root.Id);
            Assert.IsNotNull(root.Name);
            Assert.AreEqual(root.BaseTypeId, BaseTypeId.CmisFolder);
            Assert.IsNotNull(root.AllowableActions);
            Assert.IsNotNull(root.AllowableActions.Actions);
            Assert.IsTrue(root.AllowableActions.Actions.Count > 0);
            Assert.IsTrue(root.HasAllowableAction(PortCMIS.Enums.Action.CanGetProperties));
            Assert.IsFalse(root.HasAllowableAction(PortCMIS.Enums.Action.CanGetFolderParent));
            Assert.IsFalse(root.HasAllowableAction(PortCMIS.Enums.Action.CanMoveObject));

            foreach (ICmisObject child in root.GetChildren(oc))
            {
                Assert.IsNotNull(child);
                Assert.IsNotNull(child.Id);
                Assert.IsNotNull(child.Name);
                Assert.IsNotNull(child.BaseTypeId);
                Assert.IsNotNull(child.ObjectType);
                Assert.IsNotNull(child.Properties);
                Assert.IsTrue(child.Properties.Count > 9);
                Assert.AreEqual(child.Name, child[PropertyIds.Name].Value);

                Assert.IsNotNull(child.AllowableActions);
                Assert.IsNotNull(child.AllowableActions.Actions);
                Assert.IsTrue(child.AllowableActions.Actions.Count > 0);
                Assert.IsTrue(child.HasAllowableAction(PortCMIS.Enums.Action.CanGetProperties));
            }
        }

        [TestMethod]
        public void TestCreate()
        {
            string testFolderName = "porttest";

            if (Session.ExistsPath("/", testFolderName))
            {
                ICmisObject obj = Session.GetObjectByPath("/", testFolderName, OperationContextUtils.CreateMinimumOperationContext());
                if (obj is IFolder)
                {
                    ((IFolder)obj).DeleteTree(true, UnfileObject.Delete, true);
                }
                else
                {
                    obj.Delete();
                }
            }

            // create folder
            IFolder root = Session.GetRootFolder();
            IFolder newFolder = CreateFolder(root, testFolderName);

            Assert.IsNotNull(newFolder);

            // create document
            string contentString = "Hello World";
            IDocument newDoc = CreateTextDocument(newFolder, "test.txt", contentString);
            Assert.IsNotNull(newDoc);

            // get content
            IContentStream newContent = newDoc.GetContentStream();
            Assert.IsNotNull(newContent);
            Assert.IsNotNull(newContent.Stream);

            Assert.AreEqual(contentString, ConvertStreamToString(newContent.Stream));

            // fetch it again to get the updated content stream length property
            IOperationContext ctxt = OperationContextUtils.CreateMaximumOperationContext();
            ICmisObject newObj = Session.GetObject(newDoc, ctxt);

            Assert.IsTrue(newObj is IDocument);
            IDocument newDoc2 = (IDocument)newObj;

            Assert.AreEqual(newDoc.Name, newDoc2.Name);
            Assert.AreEqual(Encoding.UTF8.GetBytes(contentString).Length, newDoc2.ContentStreamLength);

            // delete document
            newDoc.Delete();

            try
            {
                Session.GetObject(newDoc);
                Assert.Fail("Document still exists.");
            }
            catch (CmisObjectNotFoundException)
            {
                // expected
            }

            // delete folder

            newFolder.Delete();

            Assert.IsFalse(Session.Exists(newFolder));
        }

        [TestMethod]
        public void TestCreateBig()
        {
            int docSize = 50 * 1024 * 1024; // 50MiB

            // get root folder
            IFolder root = Session.GetRootFolder();
            IDocument doc = null;

            try
            {
                // create document
                StringBuilder sb = new StringBuilder(docSize);
                for (int i = 0; i < docSize; i++)
                {
                    sb.Append('x');
                }

                string contentString = sb.ToString();

                doc = CreateTextDocument(root, "big.txt", contentString);
                Assert.IsNotNull(doc);

                // get content
                IContentStream newContent = doc.GetContentStream();
                Assert.IsNotNull(newContent);
                Assert.IsNotNull(newContent.Stream);

                Assert.AreEqual(contentString, ConvertStreamToString(newContent.Stream));
            }
            finally
            {
                if (doc != null)
                {
                    doc.Delete();
                    Assert.IsFalse(Session.Exists(doc));
                }
            }
        }

        [TestMethod]
        public void TestUpdateProperties()
        {
            string name1 = "port-test-folder1";
            string name2 = "port-test-folder2";
            string name3 = "port-test-folder3";

            IOperationContext oc = Session.CreateOperationContext();
            oc.CacheEnabled = false;

            IFolder newFolder = null;
            try
            {
                // create folder
                IFolder root = Session.GetRootFolder();
                newFolder = CreateFolder(root, name1);
                Assert.IsNotNull(newFolder);

                IFolder newFolder2 = (IFolder)Session.GetObject(newFolder, oc);
                Assert.IsNotNull(newFolder2);
                Assert.IsNotNull(name1, newFolder2.Name);

                IDictionary<string, object> updateProps = new Dictionary<string, object>();
                updateProps[PropertyIds.Name] = name2;

                newFolder2.UpdateProperties(updateProps);

                IFolder newFolder3 = (IFolder)Session.GetObject(newFolder, oc);
                Assert.IsNotNull(newFolder3);
                Assert.IsNotNull(name2, newFolder3.Name);

                newFolder3.Rename(name3);

                IFolder newFolder4 = (IFolder)Session.GetObject(newFolder, oc);
                Assert.IsNotNull(newFolder4);
                Assert.IsNotNull(name3, newFolder4.Name);
            }
            finally
            {
                if (newFolder != null)
                {
                    newFolder.DeleteTree(true, UnfileObject.Delete, true);
                    Assert.IsFalse(Session.Exists(newFolder));
                }
            }
        }

        [TestMethod]
        public void TestUpdateContent()
        {
            IDocument doc = null;
            try
            {
                // create document
                string contentString1 = "11111";
                doc = CreateTextDocument(Session.GetRootFolder(), "test.txt", contentString1);
                Assert.IsNotNull(doc);

                // get content
                IContentStream content1 = doc.GetContentStream();
                Assert.IsNotNull(content1);
                Assert.IsNotNull(content1.Stream);

                Assert.AreEqual(contentString1, ConvertStreamToString(content1.Stream));

                // update content
                string contentString2 = "22222";
                doc.SetContentStream(ContentStreamUtils.CreateTextContentStream("test2.txt", contentString2), true);

                // get content again
                IContentStream content2 = doc.GetContentStream();
                Assert.IsNotNull(content2);
                Assert.IsNotNull(content2.Stream);

                Assert.AreEqual(contentString2, ConvertStreamToString(content2.Stream));

                // delete the content stream
                doc.DeleteContentStream(true);
                Assert.IsNull(doc.ContentStreamLength);
            }
            finally
            {
                if (doc != null)
                {
                    doc.Delete();
                    Assert.IsFalse(Session.Exists(doc));
                }
            }
        }

        [TestMethod]
        public void TestVersioning()
        {
            IOperationContext noCacheOC = OperationContextUtils.CreateMaximumOperationContext();
            noCacheOC.CacheEnabled = false;

            IFolder rootFolder = Session.GetRootFolder();
            IDocument doc = null;

            try
            {
                // create document
                string name1 = "versioned1.txt";
                IContentStream contentStream1 = ContentStreamUtils.CreateTextContentStream(name1, "v1");

                IDictionary<string, object> props = new Dictionary<string, object>();
                props[PropertyIds.Name] = name1;
                props[PropertyIds.ObjectTypeId] = "VersionableType";

                doc = rootFolder.CreateDocument(props, contentStream1, VersioningState.Major);

                // create next version
                string name2 = "versioned2.txt";
                IContentStream contentStream2 = ContentStreamUtils.CreateTextContentStream(name2, "v2");

                IObjectId pwcId = doc.CheckOut();
                IDocument pwc = (IDocument)Session.GetObject(pwcId, noCacheOC);
                Assert.AreEqual(true, pwc.IsPrivateWorkingCopy);

                pwc.Rename(name2);

                IObjectId newVersId = pwc.CheckIn(true, null, contentStream2, "version 2");
                IDocument newVers = (IDocument)Session.GetObject(newVersId, noCacheOC);

                Assert.AreEqual(name2, newVers.Name);
                Assert.AreEqual("v2", ConvertStreamToString(newVers.GetContentStream().Stream));
                Assert.AreEqual(true, newVers.IsLatestVersion);
                Assert.AreEqual(true, newVers.IsMajorVersion);

                IDocument latestVersion = Session.GetLatestDocumentVersion(doc);
                Assert.AreEqual(newVers.Id, latestVersion.Id);

                // create next version
                string name3 = "versioned3.txt";
                IContentStream contentStream3 = ContentStreamUtils.CreateTextContentStream(name3, "v3");

                pwcId = newVers.CheckOut();
                pwc = (IDocument)Session.GetObject(pwcId, noCacheOC);

                pwc.Rename(name3);

                newVersId = pwc.CheckIn(true, null, contentStream3, "version 3");
                newVers = (IDocument)Session.GetObject(newVersId);

                Assert.AreEqual(name3, newVers.Name);
                Assert.AreEqual("v3", ConvertStreamToString(newVers.GetContentStream().Stream));
                Assert.AreEqual(true, newVers.IsLatestVersion);
                Assert.AreEqual(true, newVers.IsMajorVersion);

                latestVersion = Session.GetLatestDocumentVersion(doc);
                Assert.AreEqual(newVers.Id, latestVersion.Id);

                // create next (minor) version
                string name4 = "versioned4.txt";
                IContentStream contentStream4 = ContentStreamUtils.CreateTextContentStream(name4, "v3.1");

                pwcId = newVers.CheckOut();
                pwc = (IDocument)Session.GetObject(pwcId, noCacheOC);

                pwc.Rename(name4);

                newVersId = pwc.CheckIn(false, null, contentStream4, "version 3.1");
                newVers = (IDocument)Session.GetObject(newVersId);

                Assert.AreEqual(name4, newVers.Name);
                Assert.AreEqual("v3.1", ConvertStreamToString(newVers.GetContentStream().Stream));
                Assert.AreEqual(true, newVers.IsLatestVersion);
                Assert.AreEqual(false, newVers.IsMajorVersion);

                latestVersion = Session.GetLatestDocumentVersion(doc);
                Assert.AreEqual(newVers.Id, latestVersion.Id);

                // check version history
                IList<IDocument> versions = doc.GetAllVersions();
                Assert.AreEqual(4, versions.Count);

                Assert.AreEqual(latestVersion.Id, versions[0].Id);
                Assert.AreEqual(doc.Id, versions[3].Id);
            }
            finally
            {
                if (doc != null)
                {
                    doc.Delete();
                    Assert.IsFalse(Session.Exists(doc));
                }
            }
        }

        [TestMethod]
        public void TestQuery()
        {
            if (Session.RepositoryInfo.Capabilities.QueryCapability == CapabilityQuery.None)
            {
                return;
            }

            foreach (IQueryResult qr in Session.Query("SELECT * FROM cmis:document", false))
            {
                Assert.IsNotNull(qr);
                Assert.IsNotNull(qr.GetPropertyValueByQueryName(PropertyIds.ObjectId));
                Assert.IsNotNull(qr.GetPropertyValueByQueryName(PropertyIds.Name));
            }

            int count = 0;
            foreach (IQueryResult qr in Session.Query("SELECT * FROM cmis:folder", false))
            {
                Assert.IsNotNull(qr);
                Assert.IsNotNull(qr.GetPropertyValueByQueryName(PropertyIds.ObjectId));
                Assert.IsNotNull(qr.GetPropertyValueByQueryName(PropertyIds.Name));
                count++;
            }

            Assert.IsTrue(count > 0);

            IOperationContext oc = OperationContextUtils.CreateMinimumOperationContext(PropertyIds.ObjectTypeId, PropertyIds.Name);

            IFolder rootFolder = Session.GetRootFolder(oc);
            bool found = false;

            foreach (ICmisObject obj in Session.QueryObjects("cmis:folder", null, false, oc))
            {
                Assert.IsNotNull(obj.Id);
                Assert.IsNotNull(obj.Name);

                if (obj.Id == rootFolder.Id)
                {
                    found = true;
                }
            }

            Assert.IsTrue(found);
        }

        [TestMethod]
        public void TestMove()
        {
            // create folder 1
            IDictionary<string, object> folder1prop = new Dictionary<string, object>();
            folder1prop[PropertyIds.Name] = "movefolder1";
            folder1prop[PropertyIds.ObjectTypeId] = "cmis:folder";

            IObjectId folder1 = Session.CreateFolder(folder1prop, Session.GetRootFolder());

            // create folder2
            IDictionary<string, object> folder2prop = new Dictionary<string, object>();
            folder2prop[PropertyIds.Name] = "movefolder2";
            folder2prop[PropertyIds.ObjectTypeId] = "cmis:folder";

            IObjectId folder2 = Session.CreateFolder(folder2prop, Session.GetRootFolder());

            // create item
            IDictionary<string, object> itemProp = new Dictionary<string, object>();
            itemProp[PropertyIds.Name] = "movee";
            itemProp[PropertyIds.ObjectTypeId] = "cmis:item";

            IObjectId item = Session.CreateItem(itemProp, folder1);

            // move
            IItem itemObj = Session.GetObject(item) as IItem;
            itemObj.Move(folder1, folder2);

            itemObj.Refresh();

            // test
            Assert.AreEqual(folder2.Id, itemObj.Parents[0].Id);

            int folderSize1 = Enumerable.Count<ICmisObject>((Session.GetObject(folder1) as IFolder).GetChildren());
            Assert.AreEqual(0, folderSize1);

            int folderSize2 = Enumerable.Count<ICmisObject>((Session.GetObject(folder2) as IFolder).GetChildren());
            Assert.AreEqual(1, folderSize2);

            // clean up
            Session.Delete(item);
            Session.Delete(folder1);
            Session.Delete(folder2);

            Assert.IsFalse(Session.Exists(item));
            Assert.IsFalse(Session.Exists(folder1));
            Assert.IsFalse(Session.Exists(folder2));
        }

        [TestMethod]
        public void TestCopy()
        {
            IFolder rootFolder = Session.GetRootFolder();
            IFolder folder1 = null;
            IFolder folder2 = null;
            IDocument doc1 = null;
            IDocument doc2 = null;

            string content = "I'm unique!";

            try
            {
                folder1 = CreateFolder(rootFolder, "copy1");
                folder2 = CreateFolder(rootFolder, "copy2");
                doc1 = CreateTextDocument(folder1, "copydoc.xt", content);

                doc2 = doc1.Copy(folder2);

                Assert.IsNotNull(doc2);
                Assert.AreEqual(doc1.Name, doc2.Name);
                Assert.AreEqual(ConvertStreamToString(doc1.GetContentStream().Stream), ConvertStreamToString(doc2.GetContentStream().Stream));
                Assert.AreEqual(folder2.Id, doc2.Parents[0].Id);
            }
            finally
            {
                if (folder1 != null)
                {
                    folder1.DeleteTree(true, UnfileObject.Delete, true);
                    Assert.IsFalse(Session.Exists(folder1));
                }
                if (folder2 != null)
                {
                    folder2.DeleteTree(true, UnfileObject.Delete, true);
                    Assert.IsFalse(Session.Exists(folder2));
                }
            }
        }



        [TestMethod]
        public void TestAcl()
        {
            IDocument doc = null;

            try
            {
                doc = CreateTextDocument(Session.GetRootFolder(), "acl.txt", "Hello Joe!");

                Ace joeAce = new Ace()
                {
                    Principal = new Principal() { Id = "joe" },
                    Permissions = new List<string> { "cmis:write" }
                };

                // apply ACL and test result
                IAcl newAcl = doc.ApplyAcl(new List<IAce> { joeAce }, null, AclPropagation.RepositoryDetermined);
                Assert.IsNotNull(newAcl);
                Assert.IsNotNull(newAcl.Aces);
                Assert.IsTrue(newAcl.Aces.Count > 0);

                // retrieve ACL and test
                IAcl acl2 = Session.GetAcl(doc, true);
                Assert.IsNotNull(acl2);
                Assert.IsNotNull(acl2.Aces);
                Assert.IsTrue(acl2.Aces.Count > 0);

                // fetch document and test
                IDocument doc2 = (IDocument)Session.GetObject(doc, OperationContextUtils.CreateMaximumOperationContext());
                Assert.IsNotNull(doc2.Acl);
                Assert.IsNotNull(doc2.Acl.Aces);
                Assert.IsTrue(doc2.Acl.Aces.Count > 0);
            }
            finally
            {
                if (doc != null)
                {
                    doc.Delete();
                    Assert.IsFalse(Session.Exists(doc));
                }
            }
        }
    }
}
