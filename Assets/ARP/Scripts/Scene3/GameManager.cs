using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CameraChanger cameraChanger;
    public TextManager textManager;
   // public FaceController faceController;
    public MultiFaceController multiFaceController;

    public void OnButtonClick()
    {
        cameraChanger.ChangeCamera();
        textManager.ChangeText();
        //faceController.ToggleTalk();
        multiFaceController.ToggleTalk();
    }

}
