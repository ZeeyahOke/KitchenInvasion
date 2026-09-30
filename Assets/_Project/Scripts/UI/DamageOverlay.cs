using UnityEngine;
using UnityEngine.UI;

namespace KitchenInvasion {
    public class DamageOverlay : MonoBehaviour {
        [SerializeField] Image overlay;
        [SerializeField] float flashAlpha = 0.45f;
        [SerializeField] float fadeSpeed = 2f;

        void OnEnable() { GameEvents.OnPlayerDamaged += Flash; }

        void OnDisable() { GameEvents.OnPlayerDamaged -= Flash; }

        void Flash() { SetAlpha(flashAlpha); }

        void Update() {
            if (overlay.color.a > 0f) SetAlpha(Mathf.Max(overlay.color.a - fadeSpeed * Time.deltaTime, 0f));
        }

        void SetAlpha(float alpha) {
            Color color = overlay.color;
            color.a = alpha;
            overlay.color = color;
        }
    }
}
