using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARFoundationRemoteExamples {
    public class SetTouch : MonoBehaviour {
        [SerializeField] ARRaycastManager raycastManager = null;
        [SerializeField] XROrigin origin = null;
        [SerializeField] GameObject placeablePrefab = null;
        [SerializeField] TrackableType trackableTypeMask = TrackableType.Planes;
        
        static List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
        private bool hasPlaced = false;
        private bool isMarked = false;

        void Start() {
            hasPlaced = PlayerPrefs.GetInt("hasPlaced", 0) == 1;
        }

        void Update() {
            if (hasPlaced) return;
            

            for (int i = 0; i < InputWrapper.touchCount; i++) {
                var touch = InputWrapper.GetTouch(i);

                if (touch.phase == TouchPhase.Began) {
                    var ray = origin.GetCamera().ScreenPointToRay(touch.position);
                    bool hasHit = raycastManager.Raycast(ray, hitResults, trackableTypeMask);

                    if (hasHit) {
                        var hitPose = hitResults[0].pose;
                        GameObject placeObj = Instantiate(placeablePrefab, hitPose.position, hitPose.rotation);

                        hasPlaced = true;
                        SaveData(hasPlaced);
                        

                        if (SequenceInfoOfInteractObj.Instance != null) {
                            SequenceInfoOfInteractObj.Instance.SetTouchInable();
                        } else {
                            Debug.LogError("❌ SequenceInfoOfInteractObj 인스턴스 없음.");
                        }

                        if (!isMarked) {
                            DontDestroyOnLoad(placeObj);
                            isMarked = true;
                        }

                        Debug.Log($"프리팹 설치 완료: 위치 = {hitPose.position}");
                    } else {
                        Debug.Log("❌ Plane 감지 실패.");
                    }
                }
            }
        }

        public void SaveData(bool placed) {
            PlayerPrefs.SetInt("hasPlaced", placed ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}