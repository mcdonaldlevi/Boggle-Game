﻿using System;
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

            string word = (string)r.Data;
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
            Response r2 = client.DoPostAsync("games", file1).Result;

            //Sends cancel request
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r1.Data;
            Response r3 = client.DoPutAsync("games", file2).Result;
            Assert.AreEqual(OK, r3.Status);
        }

        /// <summary>
        /// Attempts to cancel game when player is not in a game
        /// </summary>
        [TestMethod]
        public void CancelJoinRequestTest2()
        {
            //Creates user
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "McLovin";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Sends cancel request
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            Response r2 = client.DoPutAsync("games", file1).Result;
            Assert.AreEqual(Forbidden, r2.Status);
        }

        /// <summary>
        /// User joins active game and attempts to cancel with invalid token
        /// </summary>
        [TestMethod]
        public void CancelJoinRequestTest3()
        {
            //Creates User
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Kareem";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 45;
            Response r2 = client.DoPostAsync("games", file1).Result;

            //Sends invalid token to cancel
            dynamic file2 = new ExpandoObject();
            file2.UserToken = "super real not fake token";
            Response r3 = client.DoPutAsync("games", file2).Result;
            Assert.AreEqual(Forbidden, r3.Status);
        }

        /// <summary>
        /// User joins active game and tries to send a Word with a null word value
        /// </summary>
        [TestMethod]
        public void PlayWordTest1()
        {
            //Creates User 1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Betty";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates User 2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Mike";
            Response r2 = client.DoPostAsync("user", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 15;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data;
            file2.TimeLimit = 30;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends play word command without word played
            dynamic file3 = new ExpandoObject();
            file3.UserToken = r1.Data;
            Response r5 = client.DoPutAsync("games/" + r1.Data, file3);
            Assert.AreEqual(Forbidden, r5.Status);
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r2.Data;
            Response r6 = client.DoPutAsync("games/" + r2.Data, file4);
            Assert.AreEqual(Forbidden, r6.Status);
        }

        /// <summary>
        /// User joins game and tries to send a word that is empty afer/before trim
        /// </summary>
        [TestMethod]
        public void PlayWordTest2()
        {
            //Creates User 1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Philip";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates User 2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Sam";
            Response r2 = client.DoPostAsync("user", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 25;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data;
            file2.TimeLimit = 20;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends play word command when string is empty after trim
            dynamic file3 = new ExpandoObject();
            file3.UserToken = r1.Data;
            file3.Word = "    ";
            Response r5 = client.DoPutAsync("games/" + r1.Data, file3);
            Assert.AreEqual(Forbidden, r5.Status);
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r2.Data;
            file4.Word = "";
            Response r6 = client.DoPutAsync("games/" + r2.Data, file4);
            Assert.AreEqual(Forbidden, r6.Status);
        }

        /// <summary>
        /// User joins game and tries to send a word with an invalid token and with an empty token
        /// </summary>
        [TestMethod]
        public void PlayWordTest3()
        {
            //Creates User 1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Helga";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates User 2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Walter";
            Response r2 = client.DoPostAsync("user", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 44;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data;
            file2.TimeLimit = 63;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends play word command with nonexistent token
            dynamic file3 = new ExpandoObject();
            file3.UserToken = "Fake token";
            file3.Word = "slurp";
            Response r5 = client.DoPutAsync("games/" + r1.Data, file3);
            Assert.AreEqual(Forbidden, r5.Status);

            //Sends play word command without token
            dynamic file4 = new ExpandoObject();
            file4.Word = "knock";
            Response r6 = client.DoPutAsync("games/" + r2.Data, file4);
            Assert.AreEqual(Forbidden, r6.Status);
        }

        /// <summary>
        /// User joins game, cancels game, then tries to send a word to game
        /// </summary>
        [TestMethod]
        public void PlayWordTest4()
        {
            //Creates User 1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Yona";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates User 2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Dennis";
            Response r2 = client.DoPostAsync("user", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 10;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data;
            file2.TimeLimit = 55;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Cancels game
            dynamic file3 = new ExpandoObject();
            file1.UserToken = r3.Data;
            Response r5 = client.DoPutAsync("games", file1).Result;
            Assert.AreEqual(Forbidden, r5.Status);

            //Sends play word command after game is already cancelled
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r3.Data;
            file4.Word = "noted";
            Response r6 = client.DoPutAsync("games/" + r1.Data, file4);
            Assert.AreEqual(Conflict, r6.Status);

            //Sends play word command after other user cancelled game
            dynamic file5 = new ExpandoObject();
            file5.UserToken = r4.Data;
            file5.Word = "fish";
            Response r7 = client.DoPutAsync("games/" + r2.Data, file5);
            Assert.AreEqual(Conflict, r7.Status);
        }

        /// <summary>
        /// User joins a game and gets one point for a correct word and loses a point for an incorrect word
        /// </summary>
        [TestMethod]
        public void PlayWordTest5()
        {
            //Creates User 1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Samantha";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates User 2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Roberto";
            Response r2 = client.DoPostAsync("user", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 70;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data;
            file2.TimeLimit = 32;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends correct play word command
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r3.Data;
            file4.Word = "noted";
            Response r6 = client.DoPutAsync("games/" + r1.Data, file4);
            Assert.AreEqual(r6.Data, 1);

            //Sends incorrect play word command
            dynamic file5 = new ExpandoObject();
            file5.UserToken = r3.Data;
            file5.Word = "einvc";
            Response r7 = client.DoPutAsync("games/" + r1.Data, file5);
            Assert.AreEqual(r7.Data, 0);
        }

        /// <summary>
        /// User joins game and sends a word that would be correct after whitespace and capitals are ignored
        /// </summary>
        [TestMethod]
        public void PlayWordTest6()
        {
            //Creates User 1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Joey";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates User 2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Krillin";
            Response r2 = client.DoPostAsync("user", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 44;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data;
            file2.TimeLimit = 2;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends correct play word command before trim
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r3.Data;
            file4.Word = "EnD iN g";
            Response r6 = client.DoPutAsync("games/" + r1.Data, file4);
            Assert.AreEqual(r6.Data, 1);
        }

        /// <summary>
        /// User joins game and sends word, expecting an OK status to be returned
        /// </summary>
        [TestMethod]
        public void PlayWordTest7()
        {
            //Creates User 1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Saul";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates User 2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Fred";
            Response r2 = client.DoPostAsync("user", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data;
            file1.TimeLimit = 04;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data;
            file2.TimeLimit = 100;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends correct play word command before trim
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r3.Data;
            file4.Word = "okay";
            Response r6 = client.DoPutAsync("games/" + r1.Data, file4);
            Assert.AreEqual(OK, r6.Status);
        }
    }
}
