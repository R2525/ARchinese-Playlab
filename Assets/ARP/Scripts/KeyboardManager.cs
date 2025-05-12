using UnityEngine;
using UnityEngine.UI;
using System;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager Instance { get; private set; }

    private TouchScreenKeyboard keyboard;
    private Action<string> onInputComplete;

#if UNITY_EDITOR
    private GameObject editorInputCanvas;
    private InputField editorInputField;
    private Button editorSubmitButton;
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Editor에서는 Update로 특별히 할 일 없음
#else
        if (keyboard == null)
            return;

        if (keyboard.status == TouchScreenKeyboard.Status.Done || keyboard.status == TouchScreenKeyboard.Status.Canceled)
        {
            onInputComplete?.Invoke(keyboard.text);
            keyboard = null;
        }
#endif
    }

    public void OpenKeyboard(Action<string> onComplete)
    {
#if UNITY_EDITOR
        Debug.LogWarning("TouchScreenKeyboard does not work in Unity Editor. Using fake InputField.");

        CreateEditorInputField(onComplete);

#else
        if (keyboard == null)
        {
            keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
            onInputComplete = onComplete;
        }
#endif
    }

#if UNITY_EDITOR
    private void CreateEditorInputField(Action<string> onComplete)
    {
        // 캔버스 만들기
        editorInputCanvas = new GameObject("EditorInputCanvas");
        var canvas = editorInputCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        editorInputCanvas.AddComponent<CanvasScaler>();
        editorInputCanvas.AddComponent<GraphicRaycaster>();

        // 배경 Panel
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(editorInputCanvas.transform);
        var panel = panelObj.AddComponent<Image>();
        panel.color = new Color(0, 0, 0, 0.5f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // InputField
        GameObject inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(panelObj.transform);
        editorInputField = inputObj.AddComponent<InputField>();
        var inputImage = inputObj.AddComponent<Image>();
        inputImage.color = Color.white;

        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(400, 60);
        inputRect.anchoredPosition = new Vector2(0, 50);
        inputRect.localPosition = Vector3.zero;

        // Placeholder
        var placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform);
        var placeholderText = placeholderObj.AddComponent<Text>();
        placeholderText.text = "Enter text...";
        placeholderText.color = Color.gray;
        placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholderText.alignment = TextAnchor.MiddleCenter;
        editorInputField.placeholder = placeholderText;

        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        // Text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform);
        var inputText = textObj.AddComponent<Text>();
        inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.color = Color.black;
        inputText.alignment = TextAnchor.MiddleCenter;
        editorInputField.textComponent = inputText;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Submit Button
        GameObject buttonObj = new GameObject("SubmitButton");
        buttonObj.transform.SetParent(panelObj.transform);
        editorSubmitButton = buttonObj.AddComponent<Button>();
        var buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.green;

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
        buttonRect.anchoredPosition = new Vector2(0, -100);
        buttonRect.localPosition = new Vector3(0, -100, 0);

        // Button Text
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform);
        var buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.text = "Submit";
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.black;

        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        // 버튼 클릭 이벤트
        editorSubmitButton.onClick.AddListener(() =>
        {
            onComplete?.Invoke(editorInputField.text);
            Destroy(editorInputCanvas);
        });
    }
#endif
}