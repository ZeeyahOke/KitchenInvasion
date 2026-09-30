using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KitchenInvasion {
    public static class PointerInput {
        static readonly List<RaycastResult> uiHits = new List<RaycastResult>();

        public static bool WasTapped() {
            return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        }

        public static Vector2 Position() { return Pointer.current.position.ReadValue(); }

        public static bool IsOverUI() {
            if (EventSystem.current == null) return false;
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = Position();
            uiHits.Clear();
            EventSystem.current.RaycastAll(data, uiHits);
            return uiHits.Count > 0;
        }
    }
}
