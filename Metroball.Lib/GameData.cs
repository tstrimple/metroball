using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metroball.Lib
{
    public class GameData
    {
        public string UserId { get; set; }
        public Session Session { get; set; }
        public Results Results { get; private set; }

        public GameData()
        {
            Results = new Results();
        }
    }
}
