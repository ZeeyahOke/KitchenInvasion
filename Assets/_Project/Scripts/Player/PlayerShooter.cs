using UnityEngine;

namespace KitchenInvasion {
    public class PlayerShooter : MonoBehaviour {
        [SerializeField] ObjectPool bulletPool;
        [SerializeField] PotPlacer potPlacer;
        [SerializeField] float bulletSpeed = 6f;
        [SerializeField] float fireCooldown = 0.15f;

        float cooldownTimer;

        void Update() {
            cooldownTimer -= Time.deltaTime;
            if (!CanShoot() || !PointerInput.WasTapped() || PointerInput.IsOverUI()) return;
            Shoot();
        }

        bool CanShoot() {
            return GameManager.Instance.IsPlaying
                && potPlacer.HasPot
                && potPlacer.PlacedFrame != Time.frameCount
                && cooldownTimer <= 0f;
        }

        void Shoot() {
            Bullet bullet = bulletPool.Get();
            if (bullet == null) return;
            Vector3 startPosition = transform.position + transform.forward * 0.1f;
            bullet.Launch(startPosition, transform.rotation, transform.forward * bulletSpeed);
            cooldownTimer = fireCooldown;
            GameEvents.PlayerShot();
        }
    }
}
