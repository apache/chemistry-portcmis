using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortCMIS.Binding;
using PortCMIS.Client;
using PortCMIS.Client.Impl;
using System;
using System.Collections.Generic;

namespace PortCMISTests
{
    [TestClass]
    public class QueryStatementTest
    {
        [TestMethod]
        public void TestStaticQueries()
        {
            Session session = new Session(new Dictionary<string, string>(), null, null, null);
            String query;
            IQueryStatement st;

            query = "SELECT cmis:name FROM cmis:folder";
            st = new QueryStatement(session, query);
            Assert.AreEqual(query, st.ToQueryString());

            query = "SELECT * FROM cmis:document WHERE cmis:createdBy = \'admin\' AND abc:int = 42";
            st = new QueryStatement(session, query);
            Assert.AreEqual(query, st.ToQueryString());

            query = "SELECT * FROM cmis:document WHERE abc:test = 'x?z'";
            st = new QueryStatement(session, query);
            st.SetString(1, "y");
            Assert.AreEqual(query, st.ToQueryString());
        }

        [TestMethod]
        public void TestWherePlacholder()
        {
            Session session = new Session(new Dictionary<string, string>(), null, null, null);
            String query;
            IQueryStatement st;

            // strings
            query = "SELECT * FROM cmis:document WHERE abc:string = ?";
            st = new QueryStatement(session, query);
            st.SetString(1, "test");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:string = 'test'", st.ToQueryString());

            query = "SELECT * FROM cmis:document WHERE abc:string = ?";
            st = new QueryStatement(session, query);
            st.SetString(1, "te'st");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:string = 'te\\'st'", st.ToQueryString());

            // likes
            query = "SELECT * FROM cmis:document WHERE abc:string LIKE ?";
            st = new QueryStatement(session, query);
            st.SetStringLike(1, "%test%");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:string LIKE '%test%'", st.ToQueryString());

            query = "SELECT * FROM cmis:document WHERE abc:string LIKE ?";
            st = new QueryStatement(session, query);
            st.SetStringLike(1, "\\_test\\%blah\\\\blah");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:string LIKE '\\_test\\%blah\\\\\\\\blah'",
                    st.ToQueryString());

            // contains

            // *, ? and - are treated as text search operators: 1st level escaping:
            // none, 2nd level escaping: none
            // \*, \? and \- are used as literals, 1st level escaping: none, 2nd
            // level escaping: \\*, \\?, \\-
            // ' and " are used as literals, 1st level escaping: \', \", 2nd level
            // escaping: \\\', \\\",
            // \ plus any other character, 1st level escaping \\ plus character, 2nd
            // level: \\\\ plus character

            query = "SELECT * FROM cmis:document WHERE CONTAINS(?)";
            st = new QueryStatement(session, query);
            st.SetStringContains(1, "John's");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('John\\\\\\'s')", st.ToQueryString());
            st.SetStringContains(1, "foo -bar");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('foo -bar')", st.ToQueryString());
            st.SetStringContains(1, "foo*");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('foo*')", st.ToQueryString());
            st.SetStringContains(1, "foo?");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('foo?')", st.ToQueryString());
            st.SetStringContains(1, "foo\\-bar");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('foo\\\\-bar')", st.ToQueryString());
            st.SetStringContains(1, "foo\\*");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('foo\\\\*')", st.ToQueryString());
            st.SetStringContains(1, "foo\\?");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('foo\\\\?')", st.ToQueryString());
            st.SetStringContains(1, "\"Cool\"");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('\\\\\\\"Cool\\\\\\\"')", st.ToQueryString());
            st.SetStringContains(1, "c:\\MyDcuments");
            Assert.AreEqual("SELECT * FROM cmis:document WHERE CONTAINS('c:\\\\MyDcuments')", st.ToQueryString());

            // ids
            query = "SELECT * FROM cmis:document WHERE abc:id = ?";
            st = new QueryStatement(session, query);
            st.SetId(1, new ObjectId("123"));
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:id = '123'", st.ToQueryString());

            // booleans
            query = "SELECT * FROM cmis:document WHERE abc:bool = ?";
            st = new QueryStatement(session, query);
            st.SetBoolean(1, true);
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:bool = TRUE", st.ToQueryString());

            // numbers
            query = "SELECT * FROM cmis:document WHERE abc:int = ? AND abc:int2 = 123";
            st = new QueryStatement(session, query);
            st.SetInteger(1, 42);
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:int = 42 AND abc:int2 = 123", st.ToQueryString());

            // dateTime
            query = "SELECT * FROM cmis:document WHERE abc:dateTime = TIMESTAMP ?";
            DateTime cal = new DateTime(2012, 2, 2, 3, 4, 5, DateTimeKind.Utc);

            st = new QueryStatement(session, query);
            st.SetDateTime(1, cal);
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:dateTime = TIMESTAMP '2012-02-02T03:04:05.000Z'",
                    st.ToQueryString());

            st = new QueryStatement(session, query);
            st.SetDateTime(1, DateTimeHelper.ConvertDateTimeToMillis(cal));
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:dateTime = TIMESTAMP '2012-02-02T03:04:05.000Z'",
                    st.ToQueryString());

            // dateTime Timestamp
            query = "SELECT * FROM cmis:document WHERE abc:dateTime = ?";

            st = new QueryStatement(session, query);
            st.SetDateTimeTimestamp(1, cal);
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:dateTime = TIMESTAMP '2012-02-02T03:04:05.000Z'",
                    st.ToQueryString());

            st = new QueryStatement(session, query);
            st.SetDateTimeTimestamp(1, DateTimeHelper.ConvertDateTimeToMillis(cal));
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:dateTime = TIMESTAMP '2012-02-02T03:04:05.000Z'",
                    st.ToQueryString());

            query = "SELECT * FROM cmis:document WHERE abc:dateTime IN (?)";

            st = new QueryStatement(session, query);
            st.SetDateTimeTimestamp(1, cal, cal);
            Assert.AreEqual("SELECT * FROM cmis:document WHERE abc:dateTime "
                    + "IN (TIMESTAMP '2012-02-02T03:04:05.000Z',TIMESTAMP '2012-02-02T03:04:05.000Z')", st.ToQueryString());
        }

    }
}
