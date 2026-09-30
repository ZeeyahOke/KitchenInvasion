using TMPro;
using UnityEngine;

namespace KitchenInvasion {
    public class FpsCounter : MonoBehaviour {
        [SerializeField] TMP_Text fpsText;
        float smoothDelta = 0.016f;

        void Update() {
            smoothDelta = Mathf.Lerp(smoothDelta, Time.unscaledDeltaTime, 0.1f);
            fpsText.text = Mathf.RoundToInt(1f / smoothDelta) + " FPS";
        }
    }
}
