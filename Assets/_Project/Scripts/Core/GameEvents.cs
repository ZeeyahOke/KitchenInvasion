using System;

namespace KitchenInvasion {
    public static class GameEvents {
        public static event Action OnMenuOpened;
        public static event Action OnGameStarted;
        public static event Action OnPotPlaced;
        public static event Action<int> OnScoreChanged;
        public static event Action<int, int> OnHealthChanged;
        public static event Action<float> OnTimeChanged;
        public static event Action OnEnemySpawned;
        public static event Action<Enemy> OnEnemyKilled;
        public static event Action OnPlayerShot;
        public static event Action OnEnemyShot;
        public static event Action OnMeleeHit;
        public static event Action OnPlayerDamaged;
        public static event Action OnPlayerDied;
        public static event Action<SessionRecord> OnGameEnded;
        public static event Action OnButtonClicked;

        public static void MenuOpened() { OnMenuOpened?.Invoke(); }
        public static void GameStarted() { OnGameStarted?.Invoke(); }
        public static void PotPlaced() { OnPotPlaced?.Invoke(); }
        public static void ScoreChanged(int score) { OnScoreChanged?.Invoke(score); }
        public static void HealthChanged(int health, int maxHealth) { OnHealthChanged?.Invoke(health, maxHealth); }
        public static void TimeChanged(float timeLeft) { OnTimeChanged?.Invoke(timeLeft); }
        public static void EnemySpawned() { OnEnemySpawned?.Invoke(); }
        public static void EnemyKilled(Enemy enemy) { OnEnemyKilled?.Invoke(enemy); }
        public static void PlayerShot() { OnPlayerShot?.Invoke(); }
        public static void EnemyShot() { OnEnemyShot?.Invoke(); }
        public static void MeleeHit() { OnMeleeHit?.Invoke(); }
        public static void PlayerDamaged() { OnPlayerDamaged?.Invoke(); }
        public static void PlayerDied() { OnPlayerDied?.Invoke(); }
        public static void GameEnded(SessionRecord result) { OnGameEnded?.Invoke(result); }
        public static void ButtonClicked() { OnButtonClicked?.Invoke(); }
    }
}
