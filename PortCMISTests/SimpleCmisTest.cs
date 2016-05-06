using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortCMIS.Client.Impl;
using System.Collections.Generic;
using PortCMIS;
using PortCMIS.Client;
using PortCMIS.Exceptions;
using PortCMIS.Data;
using System.IO;
using PortCMIS.Enums;
using System.Text;
using PortCMIS.Utils;
using PortCMISTests.Framework;
using System.Linq;

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
            Assert.IsTrue(root.AllowableActions.Actions.Contains(PortCMIS.Enums.Action.CanGetProperties));
            Assert.IsFalse(root.AllowableActions.Actions.Contains(PortCMIS.Enums.Action.CanGetFolderParent));
            Assert.IsFalse(root.AllowableActions.Actions.Contains(PortCMIS.Enums.Action.CanMoveObject));

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
                Assert.IsTrue(child.AllowableActions.Actions.Contains(PortCMIS.Enums.Action.CanGetProperties));
            }
        }

        [TestMethod]
        public void TestCreate()
        {
            try
            {
                ICmisObject obj = Session.GetObjectByPath("/porttest");
                obj.Delete();
            }
            catch (CmisConstraintException)
            {
                IFolder folder = Session.GetObjectByPath("/porttest") as IFolder;
                folder.DeleteTree(true, UnfileObject.Delete, true);
            }
            catch (CmisObjectNotFoundException)
            {
                // ignore
            }

            // create folder
            IFolder root = Session.GetRootFolder();
            IFolder newFolder = CreateFolder(root, "porttest");

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
            IOperationContext ctxt = Session.CreateOperationContext();
            ctxt.FilterString = "*";
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

            try
            {
                Session.GetObject(newFolder);
                Assert.Fail("Folder still exists.");
            }
            catch (CmisObjectNotFoundException)
            {
                // expected
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
            }
            finally
            {
                if (doc != null)
                {
                    doc.Delete();
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
        }
    }
}
