using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public TextMeshProUGUI characterText;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField dialogueInput;
    public TextMeshProUGUI infoMessageText;
    public GameObject canvasInfo;

    private enum Role { Girl = 0, Boy1 = 1, Boy2 = 2, Outro = 3 }
    private List<Role> dialogueSequence = new List<Role>();


    [SerializeField] private CameraChanger cameraChanger;

    public string[] characterNames = { "依诺", "宇轩", "浩宇", }; // girl, boy1, boy2, outro
    public string[] dialogues = {
        "你们在走廊上踢球真是太危险了！如果打到人怎么办？打碎玻璃窗怎么办？你们这样吵闹还会影响邻居休息，太没有公德心了！",
        "我们并不是故意的啊，我们只是觉得好玩。",
        "对不起，我们错了。",
    };
    public string[] infoMessages = {
        "然后，他们带着心爱的球一声不响地离开了。让我们一起去看看他们去了哪里吧"
    };

    private int currentStepIndex = -1;
    private int selectedIndex => CharacterSelector.selectedCharacterIndex; // 0: girl, 1: boy1, 2: boy2

    private void Start()
    {
        canvasInfo.SetActive(false);
    }

    public void SetupDialogueSequence()
    {
        dialogueSequence.Clear();
        switch (selectedIndex)
        {
            case 0:
                dialogueSequence.Add(Role.Girl); // input
                dialogueSequence.Add(Role.Boy1); // 텍스트
                dialogueSequence.Add(Role.Boy2);
                break;
            case 1:
                dialogueSequence.Add(Role.Girl);
                dialogueSequence.Add(Role.Boy1); // input
                dialogueSequence.Add(Role.Boy2);
                break;
            case 2:
                dialogueSequence.Add(Role.Girl);
                dialogueSequence.Add(Role.Boy2); // input
                dialogueSequence.Add(Role.Boy1);
                break;
        }
    }

    public void ChangeText()
    {
        currentStepIndex++;

        // ✅ 바로 초과되는 시점에 종료 처리
        if (currentStepIndex == dialogueSequence.Count)
        {
            canvasInfo.SetActive(true);
            infoMessageText.text = infoMessages[0];
            return;
        }

        // ✅ 이 아래는 무조건 대사 보여주는 단계
        Role currentRole = dialogueSequence[currentStepIndex];

        characterText.text = currentRole switch
        {
            Role.Girl => characterNames[0],
            Role.Boy1 => characterNames[1],
            Role.Boy2 => characterNames[2],
        };

        if (IsInputStep(currentRole))
        {
            dialogueInput.gameObject.SetActive(true);
            dialogueText.gameObject.SetActive(false);
            dialogueInput.text = "";
            dialogueInput.ActivateInputField();
        }
        else
        {
            dialogueInput.gameObject.SetActive(false);
            dialogueText.gameObject.SetActive(true);

            dialogueText.text = currentRole switch
            {
                Role.Girl => dialogues[0],
                Role.Boy1 => dialogues[1],
                Role.Boy2 => dialogues[2],
            };
        }
    }




    bool IsInputStep(Role role)
    {
        return (selectedIndex == 0 && role == Role.Girl) ||
               (selectedIndex == 1 && role == Role.Boy1) ||
               (selectedIndex == 2 && role == Role.Boy2);
    }

}