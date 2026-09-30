# Kitchen Invasion

Kitchen Invasion is a small mobile AR survival shooter I made for iPhone with Unity 6, AR Foundation and ARKit. You point your phone at the floor, place a cooking pot, and angry food climbs out of it and comes for you. You are the camera, so you aim by moving the phone and shoot by tapping. Survive 90 seconds to win.

Made by Fawziyyah Oke.

## How to play

1. Pick Easy or Hard on the start screen and press Start.
2. Point the phone at the floor and move it slowly. The floor gets covered with my kitchen tile pattern (with my name on it) when a plane is found.
3. Tap the tiles once to place the pot. After that the tiles disappear and the round timer starts.
4. Tomatoes run at you and hit you for 5 damage when they get close (about half a metre), once every 1.5 seconds. Peppers stop about 2 metres away and shoot green seeds at you every 2 seconds (5 damage each).
5. Tap anywhere to shoot a bullet straight out of the camera. Aim with the crosshair in the middle of the screen.
6. A tomato takes 3 hits (10 points), a pepper takes 2 hits (20 points).
7. Survive 90 seconds to win. If your health reaches 0 you lose. Either way you see the end screen, and the game is saved to the leaderboard (last 5 games).

| | Easy | Hard |
|---|---|---|
| Spawn interval | 2.5 s | 1.5 s |
| Enemy speed | x1.0 | x1.3 |
| Player health | 150 | 100 |

## Opening the project

- Unity 6 (I used 6000.6.0f1) with the iOS Build Support module.
- Open the `KitchenInvasion` folder in Unity Hub. The main scene is `Assets/_Project/Scenes/Main.unity`.
- To test in the editor, press Play. XR Simulation is enabled for the editor, so you can walk around a fake room with the right mouse button and WASD, and click to place the pot and shoot.

## Building for iPhone

1. In Unity, File > Build Profiles > iOS > Build (or use the Xcode project already in `Builds/iOS` if you have it locally, it is not in git).
2. Open `Builds/iOS/Unity-iPhone.xcodeproj` in Xcode.
3. Select the Unity-iPhone target > Signing & Capabilities, tick "Automatically manage signing" and pick your Apple ID team.
4. Plug in the iPhone, select it as the run destination and press Run.

The app needs an ARKit iPhone (iPhone 6s or newer) running iOS 15 or later.

## Folder structure

```
Assets/
  _Project/
    Audio/              sound effects and music
    Fonts/              Lilita One font and its TMP font asset
    Materials/          URP materials (food, bullets, plane tiles)
    Models/             tomato, pepper and pot (Kenney Food Kit)
    Prefabs/            plane, pot, enemies and bullets
    Scenes/             Main.unity
    ScriptableObjects/  Easy and Hard difficulty assets
    Scripts/
      AR/               PotPlacer
      Audio/            AudioManager
      Core/             ManagerBase, GameManager, GameEvents
      Data/             DifficultySettings, SessionRecord, Leaderboard
      Enemies/          Enemy, MeleeEnemy, ShooterEnemy, EnemyFactory, EnemySpawner
      Interfaces/       IDamageable, IGameState
      Player/           PlayerHealth, PlayerShooter, Bullet, ObjectPool, PointerInput
      States/           StartState, PlayState, EndState
      UI/               UIManager, DamageOverlay, FpsCounter
    Textures/           kitchen tile texture with my name, colormaps, UI sprite
  Settings/             URP pipeline assets (from the Universal 3D template)
  TextMesh Pro/         TMP essential resources
  XR/                   XR Plug-in Management and ARKit settings
docs/                   technical document, diagrams, video script and notes
tools/                  Python scripts I used to make the texture and convert the models
```

## Architecture in short

- `GameManager` (a singleton through `ManagerBase<T>`) owns the current game state. The states are classes (`StartState`, `PlayState`, `EndState`) that implement `IGameState`, so there is no big switch statement.
- `GameEvents` is a static class of C# events. Gameplay code raises events like `OnEnemyKilled` or `OnPlayerShot`, and `UIManager` and `AudioManager` listen to them. The UI and audio never get called directly by gameplay code.
- `Enemy` is an abstract class with health, speed, score, hit feedback and death. `MeleeEnemy` (tomato) and `ShooterEnemy` (pepper) override `Move()` and `Attack()`.
- `EnemyFactory` creates enemies and `EnemySpawner` asks it for a new one on a timer, placed in a ring around the pot.
- `ObjectPool` pre-makes all bullets at the start. Player bullets and pepper bullets both come from pools, so nothing is instantiated or destroyed while shooting.
- `DifficultySettings` is a ScriptableObject with an Easy and a Hard asset.
- `Leaderboard` saves the last 5 games as JSON in PlayerPrefs.

More detail, the class diagram and pseudocode are in the [Technical Document](docs/TechnicalDocument.pdf). Asset credits are in [docs/ASSETS.md](docs/ASSETS.md).
