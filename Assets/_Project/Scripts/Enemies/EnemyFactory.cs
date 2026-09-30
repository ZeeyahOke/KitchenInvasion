using UnityEngine;

namespace KitchenInvasion {
    public class EnemyFactory : MonoBehaviour {
        [SerializeField] MeleeEnemy tomatoPrefab;
        [SerializeField] ShooterEnemy pepperPrefab;
        [SerializeField] ObjectPool pepperBulletPool;
        [SerializeField] PlayerHealth player;

        public Enemy CreateEnemy(EnemyType type, Vector3 position, float speedMultiplier) {
            Enemy enemy;
            switch (type) {
                case EnemyType.Pepper:
                    ShooterEnemy pepper = Instantiate(pepperPrefab, position, Quaternion.identity);
                    pepper.SetBulletPool(pepperBulletPool);
                    enemy = pepper;
                    break;
                default:
                    enemy = Instantiate(tomatoPrefab, position, Quaternion.identity);
                    break;
            }
            enemy.Setup(player, speedMultiplier);
            return enemy;
        }
    }
}
