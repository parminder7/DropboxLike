using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Server.Services.Tests
{
    [TestClass()]
    public class UserTests
    {
        [TestMethod()]
        public void validateUserTest()
        {
            User user = new User();
            String username = "parmindr@mss.ca";
            String password = "abc";
            Boolean expected = true;
            Boolean actual = user.validateUser(username, password);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void createUserTest()
        {
            User user = new User();
            String username = "unittestt2@mss.ca";
            String password = "poor";
            String fullname = "Unittestt2";
            Boolean expected = true;
            Boolean actual = user.createUser(fullname, username, password);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void listTest()
        {
            User user = new User();
            String username = "parmindr@mss.ca";
            String path = "9023:9023";
            String[] expected = { "test1.txt" };
            //String[] actual = user.list(username, path);
            //CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void uploadTest()
        {
            User user = new User();
            String username = "parmindr@mss.ca";
            String path = "9023:9023";
            String localpath = "E:/test2.txt";
            //user.upload(username, path, localpath);
            Assert.Inconclusive("This method doesn't return anything. But uploads file on azure cloud");
        }

        [TestMethod()]
        public void downloadTest()
        {
            User user = new User();
            String username = "parmindr@mss.ca";
            String path = "9023:9023/test2.txt";
            String localpath = "E:/ui2";
            //user.download(username, path, localpath);
            Assert.Inconclusive("This method doesn't return anything. But uploads file on azure cloud");
        }
    }
}
