using System;

namespace KitchenInvasion {
    public class EndState : IGameState {
        readonly bool playerWon;

        public EndState(bool won) { playerWon = won; }

        public void Enter(GameManager game) {
            SessionRecord record = new SessionRecord();
            record.won = playerWon;
            record.score = game.Score;
            record.enemiesDefeated = game.EnemiesDefeated;
            record.timeSurvived = game.RoundLength - game.TimeLeft;
            record.date = DateTime.Now.ToString("dd MMM yyyy HH:mm");

            Leaderboard.AddSession(record);
            GameEvents.GameEnded(record);
        }

        public void Tick(GameManager game) { }

        public void Exit(GameManager game) { }
    }
}
