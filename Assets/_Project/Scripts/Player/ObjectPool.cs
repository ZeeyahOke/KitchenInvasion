using System.Collections.Generic;
using UnityEngine;

namespace KitchenInvasion {
    public class ObjectPool : MonoBehaviour {
        [SerializeField] Bullet bulletPrefab;
        [SerializeField] int poolSize = 20;

        readonly Queue<Bullet> freeBullets = new Queue<Bullet>();
        readonly List<Bullet> allBullets = new List<Bullet>();

        void Awake() {
            for (int i = 0; i < poolSize; i++) {
                Bullet bullet = Instantiate(bulletPrefab, transform);
                bullet.SetPool(this);
                bullet.gameObject.SetActive(false);
                allBullets.Add(bullet);
                freeBullets.Enqueue(bullet);
            }
        }

        void OnEnable() { GameEvents.OnGameEnded += HandleGameEnded; }

        void OnDisable() { GameEvents.OnGameEnded -= HandleGameEnded; }

        public Bullet Get() {
            if (freeBullets.Count == 0) return null;
            return freeBullets.Dequeue();
        }

        public void Return(Bullet bullet) {
            if (!bullet.gameObject.activeSelf) return;
            bullet.gameObject.SetActive(false);
            freeBullets.Enqueue(bullet);
        }

        void HandleGameEnded(SessionRecord result) {
            foreach (Bullet bullet in allBullets) Return(bullet);
        }
    }
}
