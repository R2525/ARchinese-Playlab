using ARFoundationRemoteExamples;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;

public class InteractWithPlacedObject : MonoBehaviour {
    public GameObject targetObject;
    [SerializeField] XROrigin origin = null;

    private RaycastHit hit;
    public SequenceInfoOfInteractObj sequenceInfo;
    
    void Start() {
        sequenceInfo = SequenceInfoOfInteractObj.Instance;
    }
    void Update() {
        for (int i = 0; i < InputWrapper.touchCount; i++) {
            var touch = InputWrapper.GetTouch(i);
            if (touch.phase == TouchPhase.Began) {
                Ray ray = origin.GetCamera().ScreenPointToRay(touch.position);

                if (Physics.Raycast(ray, out hit)) {
                    Debug.Log("Raycast hit: " + hit.collider.tag+"\n Name : "+hit.transform.gameObject);
                    
                    //interactive tag를 가진 오브젝트만 기능
                    if (hit.collider.tag == "InteractiveObject") {
                        targetObject = hit.collider.gameObject;
                        Debug.Log("🎯 상호작용 대상: " + targetObject.name);
                        //오브젝트 상호작용후 currentstep++ & 상호작용 가능한 오브젝트 재설정
                        if (SequenceInfoOfInteractObj.Instance != null) {
                            SequenceInfoOfInteractObj.Instance.LogCurrentStep();
                            
                            SequenceInfoOfInteractObj.Instance.AdvanceToNextObject();
                        }
                        Debug.Log(sequenceInfo.currentStep);
                        //currentstep(상호작용 순서)에 맞게 각 맞는 Scene로드
                        if (sequenceInfo.currentStep == 1)
                        {
                            SceneManager.LoadScene("BedRoomTalking1");
                            
                        }
                        else if (sequenceInfo.currentStep == 2)
                        {
                            
                            SceneManager.LoadScene("BedRoomTalking2");
                        }
                        else if (sequenceInfo.currentStep == 3)
                        {
                            
                            SceneManager.LoadScene("BedRoomTalking3");
                            
                            
                        }
                    }
                }
            }
        }
    }
} 
