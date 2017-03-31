using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int TimeLeft
        {
           get { return (TimeLimit - (int)timeLeft.ElapsedTicks) / 1000; }
        }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Stopwatch timeLeft = new Stopwatch();
        public System.Timers.Timer aTimer = new System.Timers.Timer();
        public GameInfo() 
        {
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.Enabled = false;
            
        }
        public GameInfo(GameInfo copyFrom)
        {
            GameId = copyFrom.GameId ;
            Board = copyFrom.Board;
            aTimer = copyFrom.aTimer;
            GameState = copyFrom.GameState;
            Player1 = copyFrom.Player1;
            Player2 = copyFrom.Player2;
            TimeLimit = copyFrom.TimeLimit;
            timeLeft = copyFrom.timeLeft;
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.Enabled = false;
        }

        private void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            GameState = "completed";
            timeLeft.Stop();
        }
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
        public string GameID { get; set; }
    }
    public class ScoreInfo
    {
        public int Score { get; set; }
    }
}