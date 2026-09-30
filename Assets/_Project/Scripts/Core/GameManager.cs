using UnityEngine;

namespace KitchenInvasion {
    public class GameManager : ManagerBase<GameManager> {
        [SerializeField] DifficultySettings easySettings;
        [SerializeField] DifficultySettings hardSettings;
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] EnemySpawner enemySpawner;
        [SerializeField] PotPlacer potPlacer;
        [SerializeField] float roundLength = 90f;

        IGameState currentState;

        public DifficultySettings Difficulty { get; private set; }
        public PlayerHealth Player => playerHealth;
        public EnemySpawner Spawner => enemySpawner;
        public PotPlacer PotPlacer => potPlacer;
        public float RoundLength => roundLength;
        public bool IsPlaying { get; set; }
        public int Score { get; private set; }
        public int EnemiesDefeated { get; private set; }
        public float TimeLeft { get; set; }

        void OnEnable() {
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            GameEvents.OnPlayerDied += HandlePlayerDied;
        }

        void OnDisable() {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
        }

        void Start() {
            Difficulty = easySettings;
            ChangeState(new StartState());
        }

        void Update() { currentState.Tick(this); }

        public void ChangeState(IGameState nextState) {
            if (currentState != null) currentState.Exit(this);
            currentState = nextState;
            currentState.Enter(this);
        }

        public void StartGame() { ChangeState(new PlayState()); }

        public void OpenMenu() { ChangeState(new StartState()); }

        public void SetHardMode(bool isHard) { Difficulty = isHard ? hardSettings : easySettings; }

        public void ResetStats() {
            Score = 0;
            EnemiesDefeated = 0;
            TimeLeft = roundLength;
            GameEvents.ScoreChanged(Score);
            GameEvents.TimeChanged(TimeLeft);
        }

        void HandleEnemyKilled(Enemy enemy) {
            Score += enemy.ScoreValue;
            EnemiesDefeated++;
            GameEvents.ScoreChanged(Score);
        }

        void HandlePlayerDied() {
            if (IsPlaying) ChangeState(new EndState(false));
        }
    }
}
