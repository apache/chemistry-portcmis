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
using PortCMIS.Client;
using PortCMIS.Client.Impl;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
