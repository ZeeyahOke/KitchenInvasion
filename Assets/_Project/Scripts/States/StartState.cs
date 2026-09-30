namespace KitchenInvasion {
    public class StartState : IGameState {
        public void Enter(GameManager game) {
            game.IsPlaying = false;
            GameEvents.MenuOpened();
        }

        public void Tick(GameManager game) { }

        public void Exit(GameManager game) { }
    }
}
