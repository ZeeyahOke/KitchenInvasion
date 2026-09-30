using UnityEngine;

namespace KitchenInvasion {
    public static class Leaderboard {
        const string SaveKey = "KitchenInvasion.Sessions";
        const int MaxSessions = 5;

        public static SessionHistory Load() {
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrEmpty(json)) return new SessionHistory();
            return JsonUtility.FromJson<SessionHistory>(json);
        }

        public static void AddSession(SessionRecord record) {
            SessionHistory history = Load();
            history.sessions.Insert(0, record);
            while (history.sessions.Count > MaxSessions)
                history.sessions.RemoveAt(history.sessions.Count - 1);
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(history));
            PlayerPrefs.Save();
        }
    }
}
