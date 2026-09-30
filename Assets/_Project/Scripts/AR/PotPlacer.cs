using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace KitchenInvasion {
    public class PotPlacer : MonoBehaviour {
        [SerializeField] ARRaycastManager raycastManager;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] GameObject potPrefab;
        [SerializeField] Transform playerCamera;

        readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

        public bool HasPot { get; private set; }
        public Transform Pot { get; private set; }
        public int PlacedFrame { get; private set; }

        void Update() {
            if (HasPot || !GameManager.Instance.IsPlaying) return;
            if (!PointerInput.WasTapped() || PointerInput.IsOverUI()) return;
            if (raycastManager.Raycast(PointerInput.Position(), hits, TrackableType.PlaneWithinPolygon))
                PlacePot(hits[0].pose.position);
        }

        void PlacePot(Vector3 position) {
            Vector3 lookDirection = playerCamera.position - position;
            lookDirection.y = 0f;
            Quaternion rotation = lookDirection.sqrMagnitude > 0.001f ? Quaternion.LookRotation(lookDirection) : Quaternion.identity;

            Pot = Instantiate(potPrefab, position, rotation).transform;
            HasPot = true;
            PlacedFrame = Time.frameCount;
            HidePlanes();
            GameEvents.PotPlaced();
        }

        void HidePlanes() {
            planeManager.enabled = false;
            foreach (ARPlane plane in planeManager.trackables) plane.gameObject.SetActive(false);
        }
    }
}
