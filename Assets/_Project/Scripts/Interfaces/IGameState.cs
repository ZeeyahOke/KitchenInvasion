namespace KitchenInvasion {
    public interface IGameState {
        void Enter(GameManager game);
        void Tick(GameManager game);
        void Exit(GameManager game);
    }
}
