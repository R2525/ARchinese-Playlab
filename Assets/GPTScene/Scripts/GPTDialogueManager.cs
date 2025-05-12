using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class GPTRequest
{
    public string model = "gpt-3.5-turbo";
    public List<ChatMessage> messages;
}

[System.Serializable]
public class GPTChoice
{
    public ChatMessage message;
}

[System.Serializable]
public class GPTResponse
{
    public List<GPTChoice> choices;
}

[System.Serializable]
public class ChatFeedback
{
    public string feedback;
    public bool is_correct;
}

public class GPTDialogueManager : MonoBehaviour
{
    [Header("API")]
    public string apiKey = ""; // <-- 替换为你的 API Key

    [Header("Font Settings")]
    public TMP_FontAsset chineseFont; // 添加中文字体引用

    [Header("UI Elements")]
    public TMP_Text gptQuestion;             // GPT提问框(G)
    public TMP_InputField studentResponse;   // 学生输入框(S)
    public TMP_Text gptFeedback;             // GPT反馈框(R)
    public Button submitButton;

    private int round = 0;
    private const int maxRounds = 5;
    private bool conversationEnded = false;

    // 添加End变量，初始化为0
    private string End = "0";

    private string systemPrompt = "你是一位中文学习助手。你将评价学生对图片的描述并引导他们。图片显示一个小女孩坐在桌前看书或做作业。\n\n你的任务是：\n1. 评价学生的回答\n2. 如果回答不完全正确，提供引导性提示，帮助学生思考出确切的描述，可以使用'仔细看看图片中小女孩面前有什么？她在做什么动作？'这样的提示\n3. 如果学生回答正确（包含'写作业'、'看书'、'温习功课'、'学习'等关键词），则告诉他们'回答正确，请继续去房间里探索'\n\n重要：你的回答必须是纯文本，不要使用JSON格式。如果学生回答正确，你需要明确表示'回答正确'的字样并鼓励继续探索。如果回答不够准确，提供有帮助的引导，但不要直接给出答案。";

    private List<ChatMessage> messages = new List<ChatMessage>();

    void Start()
    {
        // 应用中文字体
        ApplyChineseFont();

        // 初始化界面
        gptQuestion.text = "小女孩在干什么？";
        gptFeedback.text = "";

        // 确保学生输入框中显示提示文字
        if (studentResponse != null && studentResponse.placeholder is TMP_Text placeholderText)
        {
            placeholderText.text = "请输入您的答案";
        }

        // 初始化消息列表
        messages.Add(new ChatMessage { role = "system", content = systemPrompt });

        // 添加初始问题
        messages.Add(new ChatMessage { role = "assistant", content = "小女孩在干什么？" });

        // 设置按钮监听
        submitButton.onClick.AddListener(OnSubmit);
    }

    // 应用中文字体到所有TMP元素
    void ApplyChineseFont()
    {
        if (chineseFont != null)
        {
            if (gptQuestion != null)
            {
                gptQuestion.font = chineseFont;
            }

            if (gptFeedback != null)
            {
                gptFeedback.font = chineseFont;
            }

            if (studentResponse != null)
            {
                studentResponse.fontAsset = chineseFont;

                // 同时设置占位符文本的字体
                if (studentResponse.placeholder is TMP_Text placeholderText)
                {
                    placeholderText.font = chineseFont;
                }
            }

            // 如果有其他TMP元素，也要设置字体
            TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>();
            foreach (TMP_Text text in allTexts)
            {
                text.font = chineseFont;
            }
        }
        else
        {
            Debug.LogWarning("未设置中文字体资源！请在Inspector中设置chineseFont字段。");
        }
    }

    public void OnSubmit()
    {
        string userText = studentResponse.text.Trim();
        if (string.IsNullOrEmpty(userText) || conversationEnded) return;

        // 增加回合数
        round++;

        // 添加学生消息
        messages.Add(new ChatMessage { role = "user", content = userText });

        // 发送请求
        StartCoroutine(SendChatRequest());
    }

    IEnumerator SendChatRequest()
    {
        GPTRequest requestData = new GPTRequest
        {
            messages = messages
        };

        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                gptFeedback.text = "请求失败：" + request.error;
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("API Response: " + responseText);

                // 解析响应
                GPTResponse response = JsonUtility.FromJson<GPTResponse>(responseText);
                string content = response.choices[0].message.content;

                // 添加到消息列表
                messages.Add(new ChatMessage { role = "assistant", content = content });

                // 设置反馈文本
                gptFeedback.text = content;

                // 确保文本使用正确的字体
                if (chineseFont != null && gptFeedback != null)
                {
                    gptFeedback.font = chineseFont;
                }

                // 检查内容是否表示正确回答
                bool isCorrect = content.Contains("正确") && content.Contains("继续");

                // 如果答案正确或达到最大回合数，结束对话
                if (isCorrect || round >= maxRounds)
                {
                    EndConversation(isCorrect);
                }
                else
                {
                    // 清空学生输入框，准备下一轮
                    studentResponse.text = "";
                }
            }
        }
    }

    void EndConversation(bool isCorrect)
    {
        conversationEnded = true;

        // 对话结束条件已达到，等待10秒后将End变量设置为"1"
        StartCoroutine(DelayedEndSignal());

        if (!isCorrect)
        {
            gptFeedback.text += "\n\n标准答案：小女孩正在专心地温习功课。请继续去房间里探索。";
        }

        submitButton.interactable = false;
    }

    // 协程：延迟10秒后设置End变量并在控制台输出
    IEnumerator DelayedEndSignal()
    {
        // 等待10秒
        yield return new WaitForSeconds(10f);

        // 设置End变量为"1"
        End = "1";
        if (End == "1")
        {
            SceneManager.LoadScene("ARScene");
        }

        // 在控制台输出End变量的值
        Debug.Log("End值: " + End);
    }
    
}