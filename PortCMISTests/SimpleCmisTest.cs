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
                obj.Delete(true);
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
            IDictionary<string, object> props = new Dictionary<string, object>();
            props[PropertyIds.Name] = "porttest";
            props[PropertyIds.ObjectTypeId] = "cmis:folder";

            IFolder root = Session.GetRootFolder();
            IFolder newFolder = root.CreateFolder(props);
            Assert.IsNotNull(newFolder);

            // create document
            props = new Dictionary<string, object>();
            props[PropertyIds.Name] = "test.txt";
            props[PropertyIds.ObjectTypeId] = "cmis:document";

            byte[] contentBytes = Encoding.UTF8.GetBytes("Hello World");

            ContentStream content = new ContentStream();
            content.MimeType = "text/plain";
            content.Stream = new MemoryStream(contentBytes);

            IDocument newDoc = newFolder.CreateDocument(props, content, VersioningState.None);
            Assert.IsNotNull(newDoc);

            // get content
            IContentStream newContent = newDoc.GetContentStream();
            Assert.IsNotNull(newContent);
            Assert.IsNotNull(newContent.Stream);

            MemoryStream memStream = new MemoryStream();
            newContent.Stream.CopyTo(memStream);
            byte[] newContentBytes = memStream.ToArray();

            Assert.AreEqual(contentBytes.Length, newContentBytes.Length);
            for (int i = 0; i < contentBytes.Length; i++)
            {
                Assert.AreEqual(contentBytes[i], newContentBytes[i]);
            }

            IOperationContext ctxt = Session.CreateOperationContext();
            ctxt.FilterString = "*";
            ICmisObject newObj = Session.GetObject(newDoc, ctxt);

            Assert.IsTrue(newObj is IDocument);
            IDocument newDoc2 = (IDocument)newObj;

            Assert.AreEqual(newDoc.Name, newDoc2.Name);
            Assert.AreEqual(contentBytes.Length, newDoc2.ContentStreamLength);
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
                IDictionary<string, object> props = new Dictionary<string, object>();
                props[PropertyIds.Name] = name1;
                props[PropertyIds.ObjectTypeId] = "cmis:folder";

                IFolder root = Session.GetRootFolder();
                newFolder = root.CreateFolder(props);
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
        }
    }
}
