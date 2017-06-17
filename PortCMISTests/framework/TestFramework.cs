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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PortCMISTests.Framework
{
    public class TestFramework
    {
        public static string DefaultDocumentType { get; set; }
        public static string DefaultFolderType { get; set; }
        public static IFolder TestFolder { get; set; }

        public static ISession Connect()
        {
            IDictionary<string, string> parameters = DefaultTestValues.SessionParameters;

            DefaultDocumentType = DefaultTestValues.DefaultDocumentType;
            DefaultFolderType = DefaultTestValues.DefaulFolderType;

            SessionFactory factory = SessionFactory.NewInstance();

            ISession session = null;
            if (parameters.ContainsKey(SessionParameter.RepositoryId))
            {
                session = factory.CreateSession(parameters);
            }
            else
            {
                session = factory.GetRepositories(parameters)[0].CreateSession();
            }

            Assert.IsNotNull(session);
            Assert.IsNotNull(session.Binding);
            Assert.IsNotNull(session.RepositoryInfo);
            Assert.IsNotNull(session.RepositoryInfo.Id);

            string testRootFolderPath = DefaultTestValues.TestRootFolder;
            if (testRootFolderPath == null)
            {
                TestFolder = session.GetRootFolder();
            }
            else
            {
                TestFolder = session.GetObjectByPath(testRootFolderPath) as IFolder;
            }

            Assert.IsNotNull(TestFolder);
            Assert.IsNotNull(TestFolder.Id);

            foreach (ICmisObject cmisObject in TestFolder.GetChildren())
            {
                IFolder folder = cmisObject as IFolder;
                if (folder == null)
                {
                    cmisObject.Delete(true);
                }
                else
                {
                    folder.DeleteTree(true, null, true);
                }
            }

            return session;
        }

        public IFolder CreateFolder(IFolder parent, string name)
        {
            IDictionary<string, object> props = new Dictionary<string, object>();
            props[PropertyIds.Name] = name;
            props[PropertyIds.ObjectTypeId] = "cmis:folder";

            IFolder newFolder = parent.CreateFolder(props);

            Assert.IsNotNull(newFolder);
            Assert.AreEqual(BaseTypeId.CmisFolder, newFolder.BaseTypeId);
            Assert.AreEqual("cmis:folder", newFolder.FolderType.Id);
            Assert.AreEqual(name, newFolder.Name);
            Assert.AreEqual(parent.Id, newFolder.ParentId);
            Assert.IsFalse(newFolder.IsRootFolder);
            Assert.IsNotNull(newFolder.CreationDate);
            Assert.IsNotNull(newFolder.CreatedBy);
            Assert.IsNotNull(newFolder.LastModificationDate);
            Assert.IsNotNull(newFolder.LastModifiedBy);

            return newFolder;
        }

        public IDocument CreateTextDocument(IFolder parent, string name, string content)
        {
            IDictionary<string, object> props = new Dictionary<string, object>();
            props[PropertyIds.Name] = name;
            props[PropertyIds.ObjectTypeId] = "cmis:document";

            IContentStream contentStream = ContentStreamUtils.CreateTextContentStream(name, content);

            IDocument newDoc = parent.CreateDocument(props, contentStream, VersioningState.None);


            Assert.IsNotNull(newDoc);
            Assert.AreEqual(BaseTypeId.CmisDocument, newDoc.BaseTypeId);
            Assert.AreEqual("cmis:document", newDoc.DocumentType.Id);
            Assert.AreEqual(name, newDoc.Name);
            Assert.AreEqual(parent.Id, newDoc.Parents[0].Id);
            Assert.IsNotNull(newDoc.CreationDate);
            Assert.IsNotNull(newDoc.CreatedBy);
            Assert.IsNotNull(newDoc.LastModificationDate);
            Assert.IsNotNull(newDoc.LastModifiedBy);

            return newDoc;
        }

        public IItem CreateItem(IFolder parent, string name)
        {
            IDictionary<string, object> props = new Dictionary<string, object>();
            props[PropertyIds.Name] = name;
            props[PropertyIds.ObjectTypeId] = "cmis:item";

            IItem newItem = parent.CreateItem(props);

            Assert.IsNotNull(newItem);
            Assert.AreEqual(BaseTypeId.CmisItem, newItem.BaseTypeId);
            Assert.AreEqual("cmis:item", newItem.ItemType.Id);
            Assert.AreEqual(name, newItem.Name);
            Assert.IsNotNull(newItem.CreationDate);
            Assert.IsNotNull(newItem.CreatedBy);
            Assert.IsNotNull(newItem.LastModificationDate);
            Assert.IsNotNull(newItem.LastModifiedBy);

            return newItem;
        }

        public byte[] ConvertStreamToByteArray(Stream stream)
        {
            MemoryStream memStream = new MemoryStream();
            stream.CopyTo(memStream);
            return memStream.ToArray();
        }

        public string ConvertStreamToString(Stream stream)
        {
            return Encoding.UTF8.GetString(ConvertStreamToByteArray(stream));
        }

        // --- asserts ---

        public void AreEqual(byte[] ba1, byte[] ba2)
        {
            Assert.AreEqual(ba1.Length, ba2.Length);
            for (int i = 0; i < ba1.Length; i++)
            {
                Assert.AreEqual(ba1[i], ba2[i]);
            }
        }
    }
}
