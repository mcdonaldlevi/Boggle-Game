using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Net.HttpStatusCode;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Dynamic;

namespace Boggle
{
    /// <summary>
    /// Provides a way to start and stop the IIS web server from within the test
    /// cases.  If something prevents the test cases from stopping the web server,
    /// subsequent tests may not work properly until the stray process is killed
    /// manually.
    /// </summary>
    public static class IISAgent
    {
        // Reference to the running process
        private static Process process = null;

        /// <summary>
        /// Starts IIS
        /// </summary>
        public static void Start(string arguments)
        {
            if (process == null)
            {
                ProcessStartInfo info = new ProcessStartInfo(Properties.Resources.IIS_EXECUTABLE, arguments);
                info.WindowStyle = ProcessWindowStyle.Minimized;
                info.UseShellExecute = false;
                process = Process.Start(info);
            }
        }

        /// <summary>
        ///  Stops IIS
        /// </summary>
        public static void Stop()
        {
            if (process != null)
            {
                process.Kill();
            }
        }
    }

    [TestClass]
    public class BoggleTests
    {
        /// <summary>
        /// This is automatically run prior to all the tests to start the server
        /// </summary>
        [ClassInitialize()]
        public static void StartIIS(TestContext testContext)
        {
            IISAgent.Start(@"/site:""BoggleService"" /apppool:""Clr4IntegratedAppPool"" /config:""..\..\..\.vs\config\applicationhost.config""");
        }

        /// <summary>
        /// This is automatically run when all tests have completed to stop the server
        /// </summary>
        [ClassCleanup()]
        public static void StopIIS()
        {
            IISAgent.Stop();
        }

        private RestTestClient client = new RestTestClient("http://localhost:60000/BoggleService.svc/");

        /// <summary>
        /// Note that DoGetAsync (and the other similar methods) returns a Response object, which contains
        /// the response Stats and the deserialized JSON response (if any).  See RestTestClient.cs
        /// for details.
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            Response r = client.DoGetAsync("word?index={0}", "-5").Result;
            Assert.AreEqual(Forbidden, r.Status);

            r = client.DoGetAsync("word?index={0}", "5").Result;
            Assert.AreEqual(OK, r.Status);

            string word = (string) r.Data;
            Assert.AreEqual("AAL", word);
        }

        /// <summary>
        /// Creates a test user and asserts that length is equal to 36
        /// </summary>
        [TestMethod]
        public void CreateUserTest1()
        {
            dynamic file = new ExpandoObject();
            file.Nickname = "Sally";
            Response r = client.DoPostAsync("users", file).Result;
            Assert.AreEqual(Created, r.Status);
            Assert.AreEqual(r.Data.Length, 36);
        }

        /// <summary>
        /// Creates 2 test users with the same name and asserts they have unique game ids
        /// </summary>
        [TestMethod]
        public void CreateUserTest2()
        {
            dynamic file = new ExpandoObject();
            file.Nickname = "Sebastian";
            Response r1 = client.DoPostAsync("users", file).Result;
            Response r2 = client.DoPostAsync("users", file).Result;
            Assert.AreEqual(Created, r1.Status);
            Assert.AreEqual(Created, r2.Status);
            Assert.AreNotEqual(r1.Data, r2.Data);
        }

        /// <summary>
        /// Creates a user with a blank name
        /// </summary>
        [TestMethod]
        public void CreateUserTest3()
        {
            dynamic file = new ExpandoObject();
            file.Nickname = "";
            Response r = client.DoPostAsync("users", file).Result;
            Assert.AreEqual(Forbidden, r.Status);

            
        }

        /// <summary>
        /// Creates a user with a null name
        /// </summary>
        [TestMethod]
        public void CreateUserTest4()
        {
            dynamic file = new ExpandoObject();
            file.Nickname = null;
            Response r = client.DoPostAsync("users", file).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Creates user with whitespace only for name
        /// </summary>
        [TestMethod]
        public void CreateUserTest5()
        {
            dynamic file = new ExpandoObject();
            file.Nickname = "     ";
            Response r = client.DoPostAsync("users", file).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Two users are created
        /// When joining a game, first user gets an accepted status
        /// When joining a game, second user gets a created status
        /// </summary>
        [TestMethod]
        public void JoinGameTest1()
        {
            //Creates first user
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Kanye";
            Response p1_token = client.DoPostAsync("users", user1).Result;

            //Creates second user
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Ajay";
            Response p2_token = client.DoPostAsync("users", user2).Result;

            //Player 1 joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = p1_token.Data;
            file1.TimeLimit = 30;
            Response r1 = client.DoPostAsync("games", file1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            //Player 2 joins game
            dynamic file2 = new ExpandoObject();
            file2.UserToken = p2_token.Data;
            file2.TimeLimit = 30;
            Response r2 = client.DoPostAsync("games", file2).Result;
            Assert.AreEqual(Created, r2.Status);

            //Asserts that the two players have the same game id
            Assert.AreEqual(r1.Data, r2.Data);
        }

        /// <summary>
        /// User is created and attempts to join with an invalid token
        /// </summary>
        [TestMethod]
        public void JoinGameTest2()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "James";
            Response p1_token = client.DoPostAsync("users", user1).Result;

            dynamic file1 = new ExpandoObject();
            file1.UserToken = "this token iz legit lulz";
            file1.TimeLimit = 30;
            Response r1 = client.DoPostAsync("games", file1).Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        /// if UserToken is already a player in the pending game, responds with status conflict
        /// </summary>
        [TestMethod]
        public void JoinGameTest3()
        {
            //Creates user
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Billy";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 60;

            //Sends the same user token twice
            Response r2 = client.DoPostAsync("games", file1).Result;
            Assert.AreEqual(Accepted, r2.Status);

            Response r3 = client.DoPostAsync("games", file1).Result;
            Assert.AreEqual(Conflict, r3.Status);
        }

        /// <summary>
        /// Cancels a game after requesting a join
        /// </summary>
        [TestMethod]
        public void CancelJoinRequestTest1()
        {
            //Creates user
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Jon";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 60;
            client.DoPostAsync("games", file1);

            //Sends cancel request
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r1.Data;
            Response r2 = client.DoPutAsync("games", file2).Result;
            Assert.AreEqual(OK, r2.Status);
        }
    }
}
