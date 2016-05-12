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

using PortCMIS;
using PortCMIS.Client;
using System.Collections.Generic;

namespace PortCMISTests.Framework
{
    public class DefaultTestValues
    {
        public const string DefaultDocumentType = "cmis:document";
        public const string DefaulFolderType = "cmis:folder";
        public const string TestRootFolder = "/";

        public static IDictionary<string, string> SessionParameters = new Dictionary<string, string>()
        {
            {SessionParameter.BindingType , BindingType.Browser},
            {SessionParameter.BrowserUrl , "http://localhost:8080/inmemory/browser"},
            {SessionParameter.AtomPubUrl , "http://localhost:8080/inmemory/atom11"},
            {SessionParameter.RepositoryId , "A1"},
            {SessionParameter.User , "user"},
            {SessionParameter.Password , "password"}
        };
    }
}
