using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ARFoundationRemoteExamples;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARObjectTouchHandler : MonoBehaviour
{
    [SerializeField] ARRaycastManager raycastManager = null;
    [SerializeField] XROrigin origin = null;
    [SerializeField] TrackableType trackableTypeMask = TrackableType.Planes; // 🔹 Plane만 감
    
    private Camera arCamera;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0 || EventSystem.current.IsPointerOverGameObject(0))
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            Ray ray = origin.GetCamera().ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                // if (interactable != null)
                // {
                //     interactable.Interact();
                // }
            }
        }
    }
}