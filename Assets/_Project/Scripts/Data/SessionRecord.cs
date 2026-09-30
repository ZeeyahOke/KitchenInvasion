using System;
using System.Collections.Generic;

namespace KitchenInvasion {
    [Serializable]
    public class SessionRecord {
        public bool won;
        public int score;
        public int enemiesDefeated;
        public float timeSurvived;
        public string date;
    }

    [Serializable]
    public class SessionHistory {
        public List<SessionRecord> sessions = new List<SessionRecord>();
    }
}
