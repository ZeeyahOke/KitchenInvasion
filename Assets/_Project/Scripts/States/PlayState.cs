using UnityEngine;

namespace KitchenInvasion {
    public class PlayState : IGameState {
        public void Enter(GameManager game) {
            game.IsPlaying = true;
            game.PotPlacer.ResetPot();
            game.ResetStats();
            game.Player.ResetHealth(game.Difficulty.playerMaxHealth);
            game.Spawner.StartSpawning(game.Difficulty);
            GameEvents.GameStarted();
        }

        public void Tick(GameManager game) {
            if (!game.PotPlacer.HasPot) return;
            game.TimeLeft -= Time.deltaTime;
            GameEvents.TimeChanged(game.TimeLeft);
            if (game.TimeLeft <= 0f) {
                game.TimeLeft = 0f;
                game.ChangeState(new EndState(true));
            }
        }

        public void Exit(GameManager game) {
            game.IsPlaying = false;
            game.Spawner.StopAndClear();
        }
    }
}
