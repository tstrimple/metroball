using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metroball.Lib
{
    public class Session
    {
        public string SessionId { get; private set; }
        public int StartTime { get; private set; }
        public int EndTime { get; private set; }

        public Session()
        {
            SessionId = Guid.NewGuid().ToString();
            StartTime = DateTime.UtcNow.ToUnixTime();
            EndTime = -1;
        }
    }
}
