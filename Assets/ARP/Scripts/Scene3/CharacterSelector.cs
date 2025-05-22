using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    [Header("©¦???? ?????? ???????")]
    public GameObject[] characterModels;

    [Header("UI ???")]
    public Button leftButton;
    public Button rightButton;
    public Button selectButton;

    [Header("???? ? Canvas")]
    public Canvas characterSelectCanvas;

    private int currentIndex = 0;
    public static int selectedCharacterIndex = -1;

    void Start()
    {
        leftButton.onClick.AddListener(PreviousCharacter);
        rightButton.onClick.AddListener(NextCharacter);
        selectButton.onClick.AddListener(SelectCharacter);

        ShowCharacter(currentIndex);
    }

    void ShowCharacter(int index)
    {
        for (int i = 0; i < characterModels.Length; i++)
        {
            characterModels[i].SetActive(i == index);
        }
    }

    void PreviousCharacter()
    {
        currentIndex = (currentIndex - 1 + characterModels.Length) % characterModels.Length;
        ShowCharacter(currentIndex);
    }

    void NextCharacter()
    {
        currentIndex = (currentIndex + 1) % characterModels.Length;
        ShowCharacter(currentIndex);
    }

    void SelectCharacter()
    {
        selectedCharacterIndex = currentIndex;
        Debug.Log("????? ©¦???? ?¥å???: " + selectedCharacterIndex);
        FindObjectOfType<CameraChanger>().GenerateCameraSequence();
        FindObjectOfType<TextManager>().SetupDialogueSequence();

        characterSelectCanvas.gameObject.SetActive(false);
    }
}
