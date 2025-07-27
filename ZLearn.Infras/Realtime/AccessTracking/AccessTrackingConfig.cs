using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Infras.Realtime.AccessTracking
{
    public class AccessTrackingConfig
    {
        public int AutoSaveIntervalMinutes { get; set; }
        public bool Enabled { get; set; }
        public string TrackingPath { get; set; }
    }
}
