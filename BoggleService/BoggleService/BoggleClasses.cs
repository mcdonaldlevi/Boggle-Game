using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public string Nickname { get; set; }
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
        public string Nickname { get; set; }
        public int Score { get; set; }
        public List<WordPlayed> WordsPlayed { get; set; }
    }
    public class WordPlayed
    {
        public string Word { get; set; }
        public int Score { get; set; }
    }
    public class GameIDInfo
    {
        public int GameID { get; set; }
    }
    public class ScoreInfo
    {
        public int Score { get; set; }
    }
}