using UnityEngine;

namespace KitchenInvasion {
    public abstract class ManagerBase<T> : MonoBehaviour where T : ManagerBase<T> {
        public static T Instance { get; private set; }

        protected virtual void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = (T)this;
        }

        protected virtual void OnDestroy() {
            if (Instance == this) Instance = null;
        }
    }
}
