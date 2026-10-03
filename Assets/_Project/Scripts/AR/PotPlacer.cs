using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace KitchenInvasion {
    public class PotPlacer : MonoBehaviour {
        [SerializeField] ARSession session;
        [SerializeField] ARRaycastManager raycastManager;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] GameObject potPrefab;
        [SerializeField] Camera playerCamera;
        [SerializeField] float lookDownAmount = 0.3f;

        readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

        public bool HasPot { get; private set; }
        public Transform Pot { get; private set; }
        public int PlacedFrame { get; private set; }

        void Update() {
            if (HasPot) return;
            UpdatePlaneVisibility();
            if (!GameManager.Instance.IsPlaying || !PointerInput.WasTapped() || PointerInput.IsOverUI()) return;
            if (raycastManager.Raycast(PointerInput.Position(), hits, TrackableType.PlaneWithinPolygon))
                PlacePot(hits[0].pose.position);
        }

        public void ResetPot() {
            if (Pot == null) return;
            Destroy(Pot.gameObject);
            HasPot = false;
            planeManager.enabled = true;
            session.Reset();
        }

        void UpdatePlaneVisibility() {
            bool lookingDown = playerCamera.transform.forward.y < -lookDownAmount;
            foreach (ARPlane plane in planeManager.trackables) {
                Vector3 view = playerCamera.WorldToViewportPoint(plane.center);
                bool onScreen = view.z > 0f && view.x > -0.2f && view.x < 1.2f && view.y > -0.2f && view.y < 1.2f;
                plane.GetComponent<MeshRenderer>().enabled = lookingDown && onScreen;
            }
        }

        void PlacePot(Vector3 position) {
            Vector3 lookDirection = playerCamera.transform.position - position;
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
