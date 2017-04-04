using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Threading;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    
    public class BoggleService : IBoggleService
    {
        private static Dictionary<String, GameInfo> games = new Dictionary<string, GameInfo>();
        private static Dictionary<String, UserInfo> users = new Dictionary<string, UserInfo>();
        private static readonly object sync = new object();
        private static GameInfo pendingGame = new GameInfo { GameState = "inactive" };
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
            lock (sync)
            {
                if (user.Nickname == "stall")
                {
                    Thread.Sleep(5000);
                }
                if (user.Nickname == null || user.Nickname.Trim().Length == 0)
                {
                    SetStatus(Forbidden);
                    return null;
                }
                else
                {
                    string userID = Guid.NewGuid().ToString();
                    users.Add(userID, user);
                    SetStatus(Created);
                    UserID returnUser = new UserID { UserToken = userID };
                    return returnUser;
                }
            }
        }
        public GameIDInfo JoinGame(JoinGameInfo user)
        {
            lock (sync)
            {
                if (user.UserToken == null || user.UserToken.Trim().Length == 0)
                {
                    SetStatus(Forbidden);
                    return null;
                }

                if (pendingGame.GameState == "inactive" )
                {
                    pendingGame = new GameInfo();
                    pendingGame.Player1 = new Player();
                    pendingGame.Player1.WordsPlayed = new List<WordPlayed>();
                    pendingGame.Player1.UserToken = user.UserToken;
                    pendingGame.TimeLimit = user.TimeLimit;
                    pendingGame.GameState = "pending";
                    BoggleBoard board = new BoggleBoard();
                    pendingGame.Board = board.ToString();
                    pendingGame.Player1.Nickname = users[user.UserToken].Nickname;
                    string gameID = Guid.NewGuid().ToString();
                    pendingGame.GameId = gameID;
                    SetStatus(Accepted);
                    GameIDInfo returnGame = new GameIDInfo { GameID = gameID };
                    return returnGame;
                }
                if (pendingGame.Player1.UserToken == user.UserToken)
                {
                    SetStatus(Conflict);
                    return null;
                }
                else
                {
                    pendingGame.Player2 = new Player { UserToken = user.UserToken };
                    pendingGame.Player2.WordsPlayed = new List<WordPlayed>();
                    pendingGame.Player2.Nickname = users[user.UserToken].Nickname;
                    pendingGame.TimeLimit = (pendingGame.TimeLimit + user.TimeLimit)/ 2;
                    GameInfo newGame = new GameInfo(pendingGame);
                    newGame.aTimer.Interval = pendingGame.TimeLimit * 1000;
                    newGame.aTimer.Enabled = true;
                    newGame.myStopWatch.Start();                   
                    newGame.GameState = "active";
                    games.Add(newGame.GameId, newGame);
                    SetStatus(Created);
                    string gameID = pendingGame.GameId;
                    pendingGame.GameState = "inactive";
                    pendingGame.aTimer.Enabled = false;
                    GameIDInfo returnGame = new GameIDInfo { GameID = gameID };
                    return returnGame;
                }
            }

        }
        public void CancelJoinRequest(UserID user)
        {
            lock (sync)
            {
                if (user.UserToken == null || user.UserToken.Trim().Length == 0 || pendingGame.Player1.UserToken != user.UserToken)
                {
                    SetStatus(Forbidden);
                }
                else
                {
                    pendingGame.GameState = "inactive";
                    SetStatus(OK);
                }
            }
        }
        public ScoreInfo PlayWord(UserIDandPlayWord user, string gameID)
        {
            lock (sync)
            {
                if (user.UserToken == null || user.UserToken.Trim().Length == 0 || user.Word == null || user.Word.Trim().Length == 0 || !games.ContainsKey(gameID))
                {
                    SetStatus(Forbidden);
                    return null;
                }
                if(games[gameID].GameState != "active")
                {
                    SetStatus(Conflict);
                    return null;
                }
                else
                {
                    BoggleBoard board = new BoggleBoard(games[gameID].Board);
                    if (board.CanBeFormed(user.Word))
                    {
                        if (games[gameID].Player1.UserToken == user.UserToken)
                        {
                            WordPlayed wordScore = new WordPlayed { Word = user.Word, Score = getScore(user.Word) };
                            games[gameID].Player1.WordsPlayed.Add(wordScore);
                            games[gameID].Player1.Score += getScore(user.Word);
                            ScoreInfo returnscore = new ScoreInfo { Score = getScore(user.Word) };
                            return returnscore;
                        }
                        else if(games[gameID].Player2.UserToken == user.UserToken)
                        {
                            WordPlayed wordScore = new WordPlayed { Word = user.Word, Score = getScore(user.Word) };
                            games[gameID].Player2.WordsPlayed.Add(wordScore);
                            ScoreInfo returnscore = new ScoreInfo { Score = getScore(user.Word) };
                            return returnscore;
                        }
                        else
                        {
                            SetStatus(Forbidden);
                            return null;
                        }
                    }
                    else
                    {
                        if (games[gameID].Player1.UserToken == user.UserToken)
                        {
                            WordPlayed wordScore = new WordPlayed { Word = user.Word, Score = -1 };
                            games[gameID].Player1.WordsPlayed.Add(wordScore);
                            ScoreInfo returnscore = new ScoreInfo { Score = -1 };
                            return returnscore;
                        }
                        else if (games[gameID].Player2.UserToken == user.UserToken)
                        {
                            WordPlayed wordScore = new WordPlayed { Word = user.Word, Score = -1 };
                            games[gameID].Player2.WordsPlayed.Add(wordScore);
                            ScoreInfo returnscore = new ScoreInfo { Score = -1 };
                            return returnscore;
                        }
                        else
                        {
                            SetStatus(Forbidden);
                            return null;
                        }
                    }
                }
            }
        }
        public GameInfo GameStatus(string brief, string gameID)
        {
            if(pendingGame.GameState == "pending")
            {
                SetStatus(OK);
                return pendingGame;
            }
            else if(pendingGame.GameState == "inactive" && games.ContainsKey(gameID))
            {
                SetStatus(OK);
                games[gameID].TimeLeft = games[gameID].TimeLimit - ((int)games[gameID].myStopWatch.ElapsedMilliseconds) / 1000;
                return games[gameID];
            }
            else
            {
                SetStatus(Forbidden);
                return null;
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
