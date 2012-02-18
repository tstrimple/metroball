using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metroball.Lib
{
    public enum GameStatus
    {
        InProgress,
        Abandoned,
        Completed
    }

    public class Results : EventArgs
    {
        public string GameId { get; private set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Score { get; set; }
        public int? Rank { get; set; }
        public GameStatus GameStatus { get; set; }
        public int PlayerLives { get; set; }
        public int ComputerLives { get; set; }

        public int Started { get; set; }
        public int Ended { get; set; }

        public Results()
        {
            NewGame();
        }

        public void NewGame()
        {
            GameId = Guid.NewGuid().ToString();
            Level = 1;
            Score = 0;
            Rank = null;
            Started = DateTime.UtcNow.ToUnixTime();
            Ended = -1;
            GameStatus = GameStatus.InProgress;
            PlayerLives = 3;
            ComputerLives = 3;
        }
    }
}
