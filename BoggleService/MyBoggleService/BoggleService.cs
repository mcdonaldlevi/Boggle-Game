using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Threading;
using static System.Net.HttpStatusCode;

namespace Boggle
{

    public class BoggleService 
    {
        private static string BoggleDB;
        static BoggleService()
        {
            BoggleDB = "Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = |DataDirectory|\\BoggleDB.mdf; Integrated Security = True";
        }
        
        public UserID CreateUser(UserInfo user, out HttpStatusCode status)
        {

            if (user.Nickname == "stall")
            {
                Thread.Sleep(5000);
            }
            if (user.Nickname == null || user.Nickname.Trim().Length == 0 || user.Nickname.Trim().Length > 50)
            {
                status =Forbidden;
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
                        status = Created;
                        trans.Commit();
                        UserID returnUser = new UserID { UserToken = userID };
                        return returnUser;
                    }
                }
            }
        }

        public GameIDInfo JoinGame(JoinGameInfo user, out HttpStatusCode status)
        {
            if (user.UserToken == null || user.UserToken.Trim().Length == 0)
            {
                status = Forbidden;
                return null;
            }

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select GameID from Games where Player1 = @UserID or Player2 = @UserID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", user.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                status = Conflict;
                                reader.Close();
                                trans.Commit();
                                return null;
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("select UserID from Users where UserID = @UserID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", user.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                status = Forbidden;
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
                            status = Accepted;
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
                            status = Created;
                            trans.Commit();
                            return new GameIDInfo { GameID = gameID.ToString() };
                        }
                    }
                }
            }
        }



        public void CancelJoinRequest(UserID user, out HttpStatusCode status)
        {
            if (user.UserToken == null || user.UserToken.Trim().Length == 0)
            {
                status = Forbidden;
                return;
            }
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    Boolean userExists;
                    using (SqlCommand command = new SqlCommand("select UserID from Users where UserID = @UserID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", user.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            userExists = reader.Read();
                            reader.Close();
                        }
                    }
                    using (SqlCommand command = new SqlCommand("select Player1 from Games where Player1 = @Player1 and Player2 is null", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player1", user.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!userExists || !reader.Read() || user.UserToken == null)
                            {
                                status = Forbidden;
                                reader.Close();
                                trans.Commit();
                                return;
                            }
                        }
                    }
                    using (SqlCommand cmd = new SqlCommand("delete from Games where Player1 = @Player1", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Player1", user.UserToken);
                        cmd.ExecuteNonQuery();
                        status = OK;
                        trans.Commit();
                    }
                }
            }
        }

        public ScoreInfo PlayWord(UserIDandPlayWord user, string gameID, out HttpStatusCode status)
        {
            if (user.UserToken == null || user.UserToken.Trim().Length == 0 || user.Word == null || user.Word.Trim().Length == 0)
            {
                status = Forbidden;
                return null;
            }
            int score = 0;
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select Player2 from Games where GameID = @GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", gameID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader["Player2"] == DBNull.Value)
                                {
                                    status = Conflict;
                                    reader.Close();
                                    trans.Commit();
                                    return null;
                                }
                            }
                            else
                            {
                                status = Forbidden;
                                reader.Close();
                                trans.Commit();
                                return null;
                            }
                            
                        }
                    }
                        using (SqlCommand command = new SqlCommand("select GameID,Player1, Player2, TimeLimit, StartTime, Board from Games where GameID = @GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", gameID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                status = Forbidden;
                                reader.Close();
                                trans.Commit();
                                return null;
                            }
                            reader.Read();
                            if ((string)reader["Player1"] != user.UserToken && (string)reader["Player2"] != user.UserToken)
                            {
                                status = Forbidden;
                                return null;
                            }
                            DateTime time = (DateTime)reader["StartTime"];
                            TimeSpan limit = new TimeSpan(0, 0, (int)reader["TimeLimit"]);
                            if (DateTime.Now >= time.Add(limit))
                            {
                                status = Conflict;
                                return null;
                            }
                            BoggleBoard board = new BoggleBoard((string)reader["Board"]);
                            if (user.Word.Length < 3)
                            {
                                score = 0;
                            }
                            else if (!board.CanBeFormed(user.Word))
                            {
                                score = -1;
                            }
                            else
                            {
                                score = getScore(user.Word);
                            }
                        }
                    }

                    using (SqlCommand command = new SqlCommand("select Word from Words where Player = @Player", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player", user.Word);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if ((string)reader["Word"] == user.Word)
                                {
                                    score = 0;
                                }
                            }
                        }
                    }

                    using (SqlCommand command = new SqlCommand("insert into Words(Word, GameID, Player, Score) values(@Word, @GameID, @Player, @Score)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Word", user.Word);
                        command.Parameters.AddWithValue("@GameID", gameID);
                        command.Parameters.AddWithValue("@Player", user.UserToken);
                        command.Parameters.AddWithValue("@Score", score);
                        command.ExecuteNonQuery();
                        trans.Commit();
                        ScoreInfo returnscore = new ScoreInfo { Score = score };
                        status = OK;
                        return returnscore;
                    }
                }
            }

        }
        public GameInfo GameStatus(string brief, string gameID, out HttpStatusCode status)
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
                                status = Forbidden;
                                reader.Close();
                                trans.Commit();
                                status = OK;
                                return null;
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("select Player1, Player2 from Games where Player2 is null", conn, trans))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                returnGame.GameState = "pending";
                                returnGame.Player1 = new Player { UserToken = (string)reader["Player1"] };
                                reader.Close();
                                using (SqlCommand cmd = new SqlCommand("select Nickname from Users where UserID = @UserID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", returnGame.Player1.UserToken);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        rdr.Read();
                                        returnGame.Player1.Nickname = (string)rdr["Nickname"];
                                        rdr.Close();
                                        trans.Commit();
                                        status = OK;
                                        return returnGame;
                                    }
                                }
                            }
                            else
                            {
                                reader.Close();
                                using (SqlCommand cmd = new SqlCommand("select GameID, Player1, Player2, Board, TimeLimit, StartTime from Games where GameID = @GameID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@GameID", gameID);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        rdr.Read();
                                        returnGame.Board = (string)rdr[3];
                                        int gameid = (int)rdr["GameID"];
                                        returnGame.GameId = gameid.ToString();
                                        returnGame.Player1 = new Player { UserToken = (string)rdr[1] };
                                        returnGame.Player2 = new Player { UserToken = (string)rdr[2] };
                                        returnGame.TimeLimit = (int)rdr[4];
                                        DateTime timeNow = (DateTime)rdr["StartTime"];
                                        timeNow = timeNow.AddSeconds(returnGame.TimeLimit);
                                        TimeSpan time = timeNow - DateTime.Now;
                                        returnGame.TimeLeft = time.Seconds;
                                        if (returnGame.TimeLeft <= 0)
                                        {
                                            returnGame.TimeLeft = 0;
                                            returnGame.GameState = "completed";
                                        }
                                        else
                                        {
                                            returnGame.GameState = "active";
                                        }
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
                                using (SqlCommand cmd = new SqlCommand("select Word, Score from Words where Player = @UserID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", returnGame.Player1.UserToken);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        returnGame.Player1.WordsPlayed = new List<WordPlayed>();
                                        while (rdr.Read())
                                        {
                                            WordPlayed wordscore = new WordPlayed { Word = (string)rdr["Word"], Score = (int)rdr["Score"] };

                                            returnGame.Player1.WordsPlayed.Add(wordscore);
                                            returnGame.Player1.Score += (int)rdr["Score"];
                                        }
                                    }
                                }
                                using (SqlCommand cmd = new SqlCommand("select Word, Score from Words where Player = @UserID", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", returnGame.Player2.UserToken);
                                    using (SqlDataReader rdr = cmd.ExecuteReader())
                                    {
                                        returnGame.Player2.WordsPlayed = new List<WordPlayed>();
                                        while (rdr.Read())
                                        {
                                            returnGame.Player2.WordsPlayed.Add(new WordPlayed { Word = (string)rdr["Word"], Score = (int)rdr["Score"] });
                                            returnGame.Player2.Score += (int)rdr["Score"];
                                        }
                                    }
                                }
                                status = OK;
                                return returnGame;
                            }

                        }
                    }
                }
            }
        }
        private int getScore(string word)
        {
            switch (word.Length)
            {
                case 1:
                case 2:
                    return 0;
                case 3:
                case 4:
                    return 1;
                case 5:
                    return 2;
                case 6:
                    return 3;
                case 7:
                    return 5;
                default:
                    return 11;
            }
        }
    }
}
