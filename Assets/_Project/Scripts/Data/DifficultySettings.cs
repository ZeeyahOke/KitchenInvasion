using UnityEngine;

namespace KitchenInvasion {
    [CreateAssetMenu(fileName = "Difficulty", menuName = "Kitchen Invasion/Difficulty Settings")]
    public class DifficultySettings : ScriptableObject {
        public string difficultyName = "Easy";
        public float spawnInterval = 2.5f;
        public float enemySpeedMultiplier = 1f;
        public int playerMaxHealth = 100;
    }
}
