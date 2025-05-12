using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPTextInputTrigger : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text tmpText;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        KeyboardManager.Instance.OpenKeyboard(OnInputComplete);
    }

    private void OnInputComplete(string input)
    {
        tmpText.text = input;
    }
}