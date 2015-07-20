using System;
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
