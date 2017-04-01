using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Net.HttpStatusCode;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Dynamic;
using System.Threading;

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
            string usertoken = r.Data["UserToken"];
            Assert.AreEqual(usertoken.Length, 36);
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
            Assert.AreNotEqual(r1.Data["UserToken"], r2.Data["UserToken"]);
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
            String p1_token = client.DoPostAsync("users", user1).Result.Data["UserToken"];

            //Creates second user
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Ajay";
            String p2_token = client.DoPostAsync("users", user2).Result.Data["UserToken"];

            //Player 1 joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = p1_token;
            file1.TimeLimit = 30;
            Response r1 = client.DoPostAsync("games", file1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            //Player 2 joins game
            dynamic file2 = new ExpandoObject();
            file2.UserToken = p2_token;
            file2.TimeLimit = 30;
            Response r2 = client.DoPostAsync("games", file2).Result;
            Assert.AreEqual(Created, r2.Status);

            //Asserts that the two players have the same game id
            Assert.AreEqual(r1.Data["GameID"], r2.Data["GameID"]);
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
            file1.UserToken = null;
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
            file1.UserToken = r1.Data["UserToken"];
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
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 60;
            Response r2 = client.DoPostAsync("games", file1).Result;

            //Sends cancel request
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r1.Data["UserToken"];
            Response r3 = client.DoPutAsync(file2, "games").Result;
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
            file1.UserToken = r1.Data["UserToken"];
            Response r2 = client.DoPutAsync(file1, "games").Result;
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
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 45;
            Response r2 = client.DoPostAsync("games", file1).Result;

            //Sends invalid token to cancel
            dynamic file2 = new ExpandoObject();
            file2.UserToken = "super real not fake token";
            Response r3 = client.DoPutAsync(file2, "games").Result;
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
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 30;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 30;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends play word command without word played
            dynamic file3 = new ExpandoObject();
            file3.UserToken = r1.Data["UserToken"];
            file3.Word = "";
            Response r5 = client.DoPutAsync(file3, "games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(Forbidden, r5.Status);
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r2.Data["UserToken"];
            Response r6 = client.DoPutAsync(file4, "games/" + r4.Data["GameID"]).Result;
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
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 25;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 20;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends play word command when string is empty after trim
            dynamic file3 = new ExpandoObject();
            file3.UserToken = r1.Data["UserToken"];
            file3.Word = "    ";
            Response r5 = client.DoPutAsync(file3, "games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(Forbidden, r5.Status);
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r2.Data["UserToken"];
            file4.Word = "";
            Response r6 = client.DoPutAsync(file4, "games/" + r4.Data["GameID"]).Result;
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
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 44;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 63;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends play word command with nonexistent token
            dynamic file3 = new ExpandoObject();
            file3.UserToken = "Fake token";
            file3.Word = "slurp";
            Response r5 = client.DoPutAsync(file3, "games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(Forbidden, r5.Status);

            //Sends play word command without token
            dynamic file4 = new ExpandoObject();
            file4.Word = "knock";
            Response r6 = client.DoPutAsync(file4, "games/" + r4.Data["GameID"]).Result;
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
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 5;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 5;
            Response r4 = client.DoPostAsync("games", file2).Result;

            Thread.Sleep(6000);

            //Sends play word command after game is already ended
            dynamic file3 = new ExpandoObject();
            file3.UserToken = r1.Data["UserToken"];
            file3.Word = "noted";
            Response r5 = client.DoPutAsync(file3, "games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(Conflict, r5.Status);
            
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r2.Data["UserToken"];
            file4.Word = "fish";
            Response r6 = client.DoPutAsync(file4, "games/" + r4.Data["GameID"]).Result;
            Assert.AreEqual(Conflict, r6.Status);
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
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 70;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 32;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends incorrect play word command
            dynamic file3 = new ExpandoObject();
            file3.UserToken = r1.Data["UserToken"];
            file3.Word = "einvc";
            Response r5 = client.DoPutAsync(file3, "games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(Int32.Parse(r5.Data["Score"].ToString()), -1);
        }

        /// <summary>
        /// User joins game and sends a word that would be incorrect after whitespace and capitals are ignored
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
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 44;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 10;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends incorrect play word command before trim
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r1.Data["UserToken"];
            file4.Word = "EnD iN g";
            Response r6 = client.DoPutAsync(file4, "games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(Int32.Parse(r6.Data["Score"].ToString()), -1);
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
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 40;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 50;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends incorrect play word command before trim
            dynamic file4 = new ExpandoObject();
            file4.UserToken = r1.Data["UserToken"];
            file4.Word = "okay";
            Response r5 = client.DoPutAsync(file4, "games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(OK, r5.Status);
        }

        /// <summary>
        /// User joins game and sends an invalid game id for game status
        /// </summary>
        [TestMethod]
        public void GameStatusTest1()
        {
            //Creates User
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "July";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Sends invalid game id
            Response r2 = client.DoGetAsync("games/1000000").Result;
            Assert.AreEqual(Forbidden, r2.Status);
        }

        /// <summary>
        /// User joins game and sends a request for game status while waiting to join a game
        /// </summary>
        [TestMethod]
        public void GameStatusTest2()
        {
            //Creates User
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Demetri";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 40;
            Response r2 = client.DoPostAsync("games", file1).Result;

            //Sends game status request
            Response r3 = client.DoGetAsync("games/" + r2.Data["GameID"]).Result;
            Assert.AreEqual(OK, r3.Status);
            Assert.AreEqual("pending", r3.Data["GameState"].ToString());
        }

        /// <summary>
        /// Tries to get game status when game is active
        /// </summary>
        [TestMethod]
        public void GameStatusTest3()
        {
            //Creates user1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Ron";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates user2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Jill";
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 45;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 40;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends wrong word to game
            dynamic file3 = new ExpandoObject();
            file3.UserToken = r2.Data["UserToken"];
            file3.Word = "iosr";
            Response r5 = client.DoPutAsync(file3, "games/" + r3.Data["GameID"]).Result;

            //Gets game status while active
            Response r6 = client.DoGetAsync("games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(OK, r6.Status);
            Assert.AreEqual("active", r6.Data["GameState"].ToString());
            //Asserts 16 letters given
            Assert.AreEqual(r6.Data["Board"].ToString().Length, 16);
            //Asserts time limit is average of two given
            Assert.AreEqual(Int32.Parse(r6.Data["TimeLimit"].ToString()), 42);
            //Asserts time left is between 25 and 45
            Assert.AreEqual(Int32.Parse(r6.Data["TimeLeft"].ToString()), 35, 9);
            //Asserts name of player 1
            if (r6.Data["Player2"]["Nickname"].ToString().Equals("Jill"))
            {
                Assert.AreEqual(Int32.Parse(r6.Data["Player1"]["Score"].ToString()), 0);
                Assert.AreEqual(r6.Data["Player1"]["Nickname"].ToString(), "Ron");
                Assert.AreEqual(Int32.Parse(r6.Data["Player2"]["Score"].ToString()), -1);
                Assert.AreEqual(r6.Data["Player2"]["Nickname"].ToString(), "Jill");
            }
            else
            {
                Assert.AreEqual(Int32.Parse(r6.Data["Player1"]["Score"].ToString()), -1);
                Assert.AreEqual(r6.Data["Player1"]["Nickname"].ToString(), "Jill");
                Assert.AreEqual(Int32.Parse(r6.Data["Player2"]["Score"].ToString()), 0);
                Assert.AreEqual(r6.Data["Player2"]["Nickname"].ToString(), "Ron");
            }
        }

        /// <summary>
        /// Tries to get brief game status when game is active
        /// </summary>
        [TestMethod]
        public void GamiStatusTest4()
        {
            //Creates user1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Conner";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates user2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Mohammed";
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 30;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 10;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Gets game status while active
            Response r5 = client.DoGetAsync("games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(OK, r5.Status);
            Assert.AreEqual("active", r5.Data["GameState"].ToString());
            //Asserts time left is between 20 and 30
            Assert.AreEqual(Int32.Parse(r5.Data["TimeLeft"].ToString()), 20, 10);
            //Asserts name of player 1
            if (r5.Data["Player2"]["Nickname"].ToString().Equals("Conner"))
                Assert.AreEqual(r5.Data["Player1"]["Nickname"].ToString(), "Mohammed");
            else
                Assert.AreEqual(r5.Data["Player1"]["Nickname"].ToString(), "Conner");
            //Asserts name of player 2
            if (r5.Data["Player1"]["Nickname"].ToString().Equals("Mohammed"))
                Assert.AreEqual(r5.Data["Player2"]["Nickname"].ToString(), "Conner");
            else
                Assert.AreEqual(r5.Data["Player2"]["Nickname"].ToString(), "Mohammed");
            //Asserts score of player 1
            Assert.AreEqual(Int32.Parse(r5.Data["Player1"]["Score"].ToString()), 0);
            //Asserts score of player 2
            Assert.AreEqual(Int32.Parse(r5.Data["Player2"]["Score"].ToString()), 0);
        }

        [TestMethod]
        public void GameStatusTest5()
        {
            //Creates user1
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Ashley";
            Response r1 = client.DoPostAsync("users", user1).Result;

            //Creates user2
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Peter";
            Response r2 = client.DoPostAsync("users", user2).Result;

            //Joins game
            dynamic file1 = new ExpandoObject();
            file1.UserToken = r1.Data["UserToken"];
            file1.TimeLimit = 5;
            Response r3 = client.DoPostAsync("games", file1).Result;
            dynamic file2 = new ExpandoObject();
            file2.UserToken = r2.Data["UserToken"];
            file2.TimeLimit = 5;
            Response r4 = client.DoPostAsync("games", file2).Result;

            //Sends incorrect play word command
            dynamic wordSend = new ExpandoObject();
            wordSend.UserToken = r1.Data["UserToken"];
            wordSend.Word = "einvc";
            Response wordResponse = client.DoPutAsync(wordSend, "games/" + r3.Data["GameID"]).Result;

            Thread.Sleep(6000);

            //Gets game status when completed
            Response r5  = client.DoGetAsync("games/" + r3.Data["GameID"]).Result;
            Assert.AreEqual(OK, r5.Status);
            Assert.AreEqual("completed", r5.Data["GameState"].ToString());

            //Asserts 16 letters given
            Assert.AreEqual(r5.Data["Board"].ToString().Length, 16);
            //Asserts time limit is average of two given
            Assert.AreEqual(Int32.Parse(r5.Data["TimeLimit"].ToString()), 5);
            //Asserts time is up
            Assert.AreEqual(Int32.Parse(r5.Data["TimeLeft"].ToString()), 0);
            //Asserts name of player 1
            if (r5.Data["Player2"]["Nickname"].ToString().Equals("Peter"))
                Assert.AreEqual(r5.Data["Player1"]["Nickname"].ToString(), "Ashley");
            else
                Assert.AreEqual(r5.Data["Player1"]["Nickname"].ToString(), "Peter");
            //Asserts name of player 2
            if (r5.Data["Player1"]["Nickname"].ToString().Equals("Ashley"))
                Assert.AreEqual(r5.Data["Player2"]["Nickname"].ToString(), "Peter");
            else
                Assert.AreEqual(r5.Data["Player2"]["Nickname"].ToString(), "Ashley");
            //Asserts score of player 1
            if (r5.Data["Player2"]["Nickname"].ToString().Equals("Ashley"))
            {
                Assert.AreEqual(Int32.Parse(r5.Data["Player1"]["Score"].ToString()), 0);
                Assert.AreEqual(Int32.Parse(r5.Data["Player2"]["Score"].ToString()), -1);
                //Asserts word score of player 2
                Assert.AreEqual(Int32.Parse(r5.Data["Player2"]["WordsPlayed"][0]["Score"].ToString()), -1);
                //Asserts word played of player 2
                Assert.AreEqual(r5.Data["Player2"]["WordsPlayed"][0]["Word"].ToString(), "einvc");
            }
            else
            {
                Assert.AreEqual(Int32.Parse(r5.Data["Player1"]["Score"].ToString()), -1);
                Assert.AreEqual(Int32.Parse(r5.Data["Player2"]["Score"].ToString()), 0);
                //Asserts word score of player 1
                Assert.AreEqual(Int32.Parse(r5.Data["Player1"]["WordsPlayed"][0]["Score"].ToString()), -1);
                //Asserts word played of player 2
                Assert.AreEqual(r5.Data["Player1"]["WordsPlayed"][0]["Word"].ToString(), "einvc");
            }
            
        }
    }
}
