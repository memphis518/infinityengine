using System;
using InfinityEngine.Business.WebPoller;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfinityEngineTest.UnitTests
{
    [TestClass]
    public class JsonWebPollerTest
    {
        [TestMethod]
        public void TestJsonWebPoller()
        {
            IWebPoller webPoller = new JsonWebPoller();
            string url = "http://localhost:8124/names";
            List<Dictionary<string, string>> results = (List<Dictionary<string, string>>) webPoller.PollURL(url);

            Assert.IsNotNull(results);
        }

        public void TestJsonWebPollerFail()
        {
            try
            {
                IWebPoller webPoller = new JsonWebPoller();
                string url = "http://google.com";
                List<Dictionary<string, string>> results = (List<Dictionary<string, string>>)webPoller.PollURL(url);
            }
            catch (InvalidOperationException e)
            {
                Assert.IsTrue(true);
            }
        }

    }
}
