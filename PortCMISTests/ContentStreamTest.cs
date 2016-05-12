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
using PortCMIS.Client.Impl;

namespace PortCMISTests
{
    [TestClass]
    public class ContentStreamHashTest
    {
        [TestMethod]
        public void TestContentStreamHash()
        {
            ContentStreamHash hash1 = new ContentStreamHash("{MD5} 12345  67890 ABC");
            Assert.AreEqual("{MD5} 12345  67890 ABC", hash1.PropertyValue);
            Assert.AreEqual("md5", hash1.Algorithm);
            Assert.AreEqual("1234567890abc", hash1.Hash);

            ContentStreamHash hash2 = new ContentStreamHash("sHa1", " 12345  67890 ABC");
            Assert.AreEqual("{sha1}1234567890abc", hash2.PropertyValue);
            Assert.AreEqual("sha1", hash2.Algorithm);
            Assert.AreEqual("1234567890abc", hash2.Hash);
        }
    }
}
