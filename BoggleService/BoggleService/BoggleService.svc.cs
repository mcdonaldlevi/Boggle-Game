using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Threading;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class GameStatusInfo
    {
        public string moreInfo { get; set; }
    }
    public class UserIDandPlayWord
    {
        public string UserToken { get; set; }
        public string Word { get; set; }
    }
    public class UserID
    {
        public string UserToken { get; set; }
    }
    public class UserInfo
    {
        public string NickName { get; set; }
    }
    public class JoinGameInfo
    {
        public string UserToken { get; set; }
        public int TimeLimit { get; set; }
    }
    public class GameInfo
    {
        public string GameId { get; set; }
        public string GameState { get; set; }
        public string Board { get; set; }
        public int TimeLimit { get; set; }
        public int TimeLeft { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
    }
    public class Player
    {
        public string UserToken { get; set; }
        public string Nickname { get;set; }
        public int Score { get; set; }
        public List<WordPlayed> WordsPlayed { get; set; }
    }
    public class WordPlayed
    {
        public string Word { get; set; }
        public int Score { get; set; }
    }
    public class BoggleService : IBoggleService
    {
        private static Dictionary<String, GameInfo> games = new Dictionary<string, GameInfo>();
        private static Dictionary<String, UserInfo> users = new Dictionary<String, UserInfo>();
        private static readonly object sync = new object();
        private GameInfo pendingGame = null;
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
        public string CreateUser(UserInfo user)
        {
            lock (sync)
            {
                if (user.NickName == "stall")
                {
                    Thread.Sleep(5000);
                }
                if (user.NickName == null || user.NickName.Trim().Length == 0)
                {
                    SetStatus(Forbidden);
                    return null;
                }
                else
                {
                    string userID = Guid.NewGuid().ToString();
                    users.Add(userID, user);
                    SetStatus(Created);
                    return userID;
                }
            }
        }
        public string JoinGame(JoinGameInfo user)
        {
            lock (sync)
            {
                if (user.UserToken == null || user.UserToken.Trim().Length == 0)
                {
                    SetStatus(Forbidden);
                    return null;
                }

                if (pendingGame == null)
                {
                    pendingGame.Player1.UserToken = user.UserToken;
                    pendingGame.TimeLimit = user.TimeLimit;
                    pendingGame.GameState = "pending";
                    BoggleBoard board = new BoggleBoard();
                    pendingGame.Board = board.ToString();
                    pendingGame.Player1.Nickname = users[user.UserToken].NickName;
                    string gameID = Guid.NewGuid().ToString();
                    SetStatus(Created);
                    return gameID;
                }
                if (pendingGame.Player1.UserToken == user.UserToken)
                {
                    SetStatus(Conflict);
                    return null;
                }
                else
                {
                    pendingGame.Player2.UserToken = user.UserToken;
                    pendingGame.GameState = "active";
                    pendingGame.Player2.Nickname = users[user.UserToken].NickName;
                    games.Add(pendingGame.GameId, pendingGame);
                    SetStatus(Accepted);
                    string gameID = pendingGame.GameId;
                    pendingGame = null;
                    return gameID;
                }
            }

        }
        public void CancelJoinGame(UserID user)
        {
            lock (sync)
            {
                if (user.UserToken == null || user.UserToken.Trim().Length == 0 || pendingGame.Player1.UserToken != user.UserToken)
                {
                    SetStatus(Forbidden);
                }
                else
                {
                    pendingGame = null;
                    SetStatus(OK);
                }
            }
        }
        public int PlayWord(UserIDandPlayWord user, string gameID)
        {
            lock (sync)
            {
                if (user.UserToken == null || user.UserToken.Trim().Length == 0 || user.Word == null || user.Word.Trim().Length == 0)
                {
                    SetStatus(Forbidden);
                    return 0;
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
                            return getScore(user.Word);
                        }
                        else if(games[gameID].Player2.UserToken == user.UserToken)
                        {
                            WordPlayed wordScore = new WordPlayed { Word = user.Word, Score = getScore(user.Word) };
                            games[gameID].Player2.WordsPlayed.Add(wordScore);
                            return getScore(user.Word);
                        }
                        else
                        {
                            SetStatus(Forbidden);
                            return 0;
                        }
                    }
                    else
                    {
                        if (games[gameID].Player1.UserToken == user.UserToken)
                        {
                            WordPlayed wordScore = new WordPlayed { Word = user.Word, Score = -1 };
                            games[gameID].Player1.WordsPlayed.Add(wordScore);
                            return -1;
                        }
                        else if (games[gameID].Player2.UserToken == user.UserToken)
                        {
                            WordPlayed wordScore = new WordPlayed { Word = user.Word, Score = -1 };
                            games[gameID].Player2.WordsPlayed.Add(wordScore);
                            return -1;
                        }
                        else
                        {
                            SetStatus(Forbidden);
                            return 0;
                        }
                    }
                }
            }
        }
        private GameInfo GameStatus(GameStatusInfo moreInfo, string gameID)
        {
            if(pendingGame.GameId == gameID)
            {
                SetStatus(OK);
                return pendingGame;
            }
            else if(games.ContainsKey(gameID))
            {
                SetStatus(OK);
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
