﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Threading;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    
    public class BoggleService : IBoggleService
    {
        private static string BoggleDB;
        static BoggleService()
        {
            BoggleDB = ConfigurationManager.ConnectionStrings["BoggleDB"].ConnectionString;
        }
        /// <summary>
        /// The most recent call to SetStatus determines the response code used when
        /// an http response is sent.
        /// </summary>
        /// <param name="status"></param>
        private static void SetStatus(HttpStatusCode status)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        }

        /// <summary>
        /// Returns a Stream version of index.html.
        /// </summary>
        /// <returns></returns>
        public Stream API()
        {
            SetStatus(OK);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "index.html");
        }

        /// <summary>
        /// Demo.  You can delete this.
        /// </summary>
        public string WordAtIndex(int n)
        {
            if (n < 0)
            {
                SetStatus(Forbidden);
                return null;
            }

            string line;
            using (StreamReader file = new System.IO.StreamReader(AppDomain.CurrentDomain.BaseDirectory + "dictionary.txt"))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (n == 0) break;
                    n--;
                }
            }

            if (n == 0)
            {
                SetStatus(OK);
                return line;
            }
            else
            {
                SetStatus(Forbidden);
                return null;
            }
        }
        public UserID CreateUser(UserInfo user)
        {

                if (user.Nickname == "stall")
                {
                    Thread.Sleep(5000);
                }
                if (user.Nickname == null || user.Nickname.Trim().Length == 0 || user.Nickname.Trim().Length > 50)
                {
                    SetStatus(Forbidden);
                    return null;
                }
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command =
                        new SqlCommand("insert into Users (UserID, Nickname) values(@UserID, @Nickname)",
                                        conn,
                                        trans))
                    {
                        string userID = Guid.NewGuid().ToString();
                        command.Parameters.AddWithValue("@UserID", userID);
                        command.Parameters.AddWithValue("@Nickname", user.Nickname.Trim());
                        command.ExecuteNonQuery();
                        SetStatus(Created);
                        trans.Commit();
                        UserID returnUser = new UserID { UserToken = userID };
                        return returnUser;
                    }
                }
            }
            }

        public GameIDInfo JoinGame(JoinGameInfo user)
        {
            if (user.UserToken == null || user.UserToken.Trim().Length == 0)
            {
                SetStatus(Forbidden);
                return null;
            }

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select UserID from Users where UserID = @UserID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", user.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                SetStatus(Forbidden);
                                reader.Close();
                                trans.Commit();
                                return null;
                            }
                        }
                    }
                    int gameID = 0;
                    int player1TimeLimit = 0;
                    using (SqlCommand command = new SqlCommand("select GameID, TimeLimit from Games where Player2 is null", conn, trans))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                gameID = (int)reader["GameID"];
                                player1TimeLimit = (int)reader["TimeLimit"];
                            }
                        }
                    }
                    if (gameID == 0)
                    {
                        using (SqlCommand command = new SqlCommand("insert into Games (Player1, TimeLimit) values (@Player1, @TimeLimit)", conn, trans))
                        {
                            command.Parameters.AddWithValue("@Player1", user.UserToken);
                            command.Parameters.AddWithValue("@TimeLimit", user.TimeLimit);
                            command.ExecuteNonQuery();
                            SetStatus(Accepted);
                            trans.Commit();
                        }
                        using (SqlCommand command = new SqlCommand("select GameID from Games where Player1 = @Player1", conn, trans))
                        {
                            command.Parameters.AddWithValue("@Player1", user.UserToken);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                reader.Read();
                                gameID = (int)reader["GameID"];
                            }
                        }
                        return new GameIDInfo { GameID = gameID.ToString() };
                    }
                    else
                    {
                        using (SqlCommand command = new SqlCommand(
                            "update Games set Player2 = @Player2, TimeLimit = @TimeLimit, Board = @Board, StartTime = @StartTime where GameID = @GameID"
                            , conn, trans))
                        {
                            int newTimeLimit = (player1TimeLimit + user.TimeLimit) / 2;
                            command.Parameters.AddWithValue("@GameID", gameID);
                            command.Parameters.AddWithValue("@Player2", user.UserToken);
                            command.Parameters.AddWithValue("@TimeLimit", newTimeLimit);
                            command.Parameters.AddWithValue("@Board", new BoggleBoard().ToString());
                            command.Parameters.AddWithValue("@StartTime", DateTime.Now);
                            command.ExecuteNonQuery();
                            SetStatus(Created);
                            trans.Commit();
                            return new GameIDInfo { GameID = gameID.ToString() };
                        }
                    }
                }
            }
        }
        
                            if (!reader.HasRows)
                            {
                                using (SqlCommand cmd = new SqlCommand("insert into Games (Player1, TimeLimit) values (@Player1, @TimeLimit)", conn, trans))
                                {
     
                                    cmd.Parameters.AddWithValue("@Player1", user.UserToken);
                                    cmd.Parameters.AddWithValue("@TimeLimit", user.TimeLimit);
                                    cmd.ExecuteNonQuery();
                                    SetStatus(Accepted);
                                }
                                using (SqlCommand cmd = new SqlCommand("select GameID from Games where Player2 == @Player2"))
                                {
                                    command.Parameters.AddWithValue("@Player2", null);
                                    using (SqlDataReader rdr = command.ExecuteReader())
                                    {
                                        return new GameIDInfo { GameID = (string)rdr["GameID"] };
                                    }
                                }
                            }
                            else
                            {
                                using (SqlCommand cmd = new SqlCommand("insert into Games ( Player2, Board, StartTime,TimeLimit) values(@TimeLimit)", conn, trans))
                                {
                                    string gameID = new SqlCommand("select 'GameID' from Table", conn, trans).ExecuteReader().ToString();
                                    string player1TimeLimit = new SqlCommand("select 'TimeLimit' from Table", conn, trans).ExecuteReader().ToString();
                                    int newTimeLimit = (Int32.Parse(player1TimeLimit) + user.TimeLimit) / 2;
                                    cmd.Parameters.AddWithValue("@TimeLimit", newTimeLimit);
                                    SetStatus(Created);                                    
                                    trans.Commit();
                                    return new GameIDInfo { GameID = gameID };
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CancelJoinRequest(UserID user)
        {

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select UserID from users where UserID = @UserID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", user.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows || user.UserToken == null ||
                                user.UserToken.Trim().Length == 0)
                            {
                                SetStatus(Forbidden);
                                trans.Commit();
                            }
                            else
                            {
                                using (SqlCommand cmd = new SqlCommand("delete GameID from Games where Player1 = @Player1"))
                                {
                                    cmd.Parameters.AddWithValue("@Player1", user.UserToken);
                                    cmd.ExecuteNonQuery();
                                    trans.Commit();
                                }
                                SetStatus(OK);
                            }
                        }
                    }
                }
            }
        }
        public ScoreInfo PlayWord(UserIDandPlayWord user, string gameID)
        {
            if (user.UserToken == null || user.UserToken.Trim().Length == 0 || user.Word == null || user.Word.Trim().Length == 0)
            {
                SetStatus(Forbidden);
                return null;
            }
            int score = 0;
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select GameID, TimeLimit, StartTime, Board from Games where GameID = @GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", gameID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                SetStatus(Forbidden);
                                trans.Commit();
                                return null;
                            }
                            reader.Read();
                            DateTime time = (DateTime)reader["StartTime"];
                            TimeSpan limit = new TimeSpan(0, 0, (int)reader["TimeLimit"]);
                            if (DateTime.Now >= time.Add(limit))
                            {
                                SetStatus(Conflict);
                                return null;
                            }
                            BoggleBoard board = new BoggleBoard((string)reader["Board"]);
                            if(!board.CanBeFormed(user.Word))
                            {
                                score = -1;
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("select Word from Words where Player = @Player"))
                    {
                        command.Parameters.AddWithValue("@Player", user.Word);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                if ((string)reader["Word"] == user.Word)
                                {
                                    score = 0;
                                }
                                else
                                {
                                    score = getScore(user.Word);
                                }
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("insert into Words(Word, GameID, Player, Score) values(@Word, @GameID, @Player, @Score "))
                    {
                        command.Parameters.AddWithValue("@Word", user.Word);
                        command.Parameters.AddWithValue("@GameID", gameID);
                        command.Parameters.AddWithValue("@Player", user.UserToken);
                        command.Parameters.AddWithValue("@Score", score);
                        command.ExecuteNonQuery();
                        trans.Commit();
                        ScoreInfo returnscore = new ScoreInfo { Score = score };
                        return returnscore;
                    }
                }
            }           
            
        }
        public GameInfo GameStatus(string brief, string gameID)
        {
            GameInfo returnGame = new GameInfo();
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select GameID from Games where GameID = @GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", gameID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                SetStatus(Forbidden);
                                reader.Close();
                                trans.Commit();
                                return null;
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("select Player1, Player2 from Games where Player2 = @Player2", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player2", null);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                returnGame.GameState = "pending";
                                returnGame.Player1 = new Player { UserToken = (string)reader["Player1"] };
                                using (SqlCommand cmd = new SqlCommand("select Nickname from Users where UserID = @UserID", conn, trans))
                                {
                                    using (SqlDataReader rdr = command.ExecuteReader())
                                    {
                                        rdr.Read();
                                        returnGame.Player1.Nickname = (string)rdr["Nickname"];
                                        trans.Commit();
                                        return returnGame;
                                    }
                                }
                            }
                            else
                            {
                                using (SqlCommand cmd = new SqlCommand("select GameID, Player1, Player2, Board, TimeLimit, StartTime from Games where GameID = @GameID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@GameID", gameID);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        rdr.Read();
                                        returnGame.Board = (string)rdr[3];
                                        returnGame.GameId = (string)rdr[0];
                                        returnGame.Player1 = new Player { UserToken = (string)rdr[1] };
                                        returnGame.Player2 = new Player { UserToken = (string)rdr[2] };
                                        returnGame.TimeLimit = (int)rdr[4];
                                        DateTime timeNow = DateTime.Now;
                                        timeNow.AddSeconds(returnGame.TimeLimit);
                                        TimeSpan time = timeNow - DateTime.Now;
                                        returnGame.TimeLeft = time.Seconds;
                                        if (returnGame.TimeLeft < 0)
                                            returnGame.TimeLeft = 0;
                                    }
                                }
                                using (SqlCommand cmd = new SqlCommand("select Nickname from Users where UserID = @UserID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", returnGame.Player1.UserToken);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        rdr.Read();
                                        returnGame.Player1.Nickname = (string)rdr["Nickname"];
                                    }
                                }
                                using (SqlCommand cmd = new SqlCommand("select Nickname from Users where UserID = @UserID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", returnGame.Player2.UserToken);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        rdr.Read();
                                        returnGame.Player2.Nickname = (string)rdr["Nickname"];
                                    }
                                }
                                using (SqlCommand cmd = new SqlCommand("select Word, Score from Words where UserID = @UserID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", returnGame.Player1.UserToken);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        while (rdr.Read())
                                        {
                                            returnGame.Player1.WordsPlayed.Add(new WordPlayed { Word = (string)rdr["Word"], Score = (int)rdr["Score"] });
                                            returnGame.Player1.Score += (int)rdr["Score"];
                                        }
                                    }
                                }
                                using (SqlCommand cmd = new SqlCommand("select Word, Score from Words where UserID = @UserID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", returnGame.Player2.UserToken);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        while (rdr.Read())
                                        {
                                            returnGame.Player2.WordsPlayed.Add(new WordPlayed { Word = (string)rdr["Word"], Score = (int)rdr["Score"] });
                                            returnGame.Player2.Score += (int)rdr["Score"];
                                        }
                                    }
                                }
                                return returnGame;
                            }

                        }
                    }
                }
            }
        }
        private int getScore(string word)
        {
            if (word.Length < 3)
                return 0;
            else if (word.Length == 3)
                return 1;
            else
            {
                return word.Length - 3;
            }

        }
    }
}
