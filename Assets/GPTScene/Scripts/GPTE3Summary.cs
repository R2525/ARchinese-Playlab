using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class GPTE3Summary : MonoBehaviour
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

    // 系统提示
    private string systemPrompt = "你是一位中文学习助手。你的任务是引导学生描述图片中的情景。\n\n图片显示一个小女孩从窗户向外看，两个小男孩在走廊上踢足球。\n\n你的任务是：\n1. 引导学生描述小男孩们对话后的心情变化\n2. 评估学生的回答是否包含以下关键词：\n   - '知道自己错了'\n3. 如果缺少关键词，提示学生补充，但不要直接告诉答案\n   - 例如：'你能描述一下小男孩们和小女孩对话后的感受吗？'\n   - 或：'你觉得小男孩们意识到了什么？'\n\n重要：\n- 你的回答必须是纯文本\n- 当学生的回答包含所有关键词时，告诉他们回答正确，并告知'请继续在场景中探索。'\n- 五轮对话后，如果学生仍未提供包含所有关键词的回答，给出例句：'经过和小女孩的对话，两个小男孩知道自己错了。'";

    private List<ChatMessage> messages = new List<ChatMessage>();

    void Start()
    {
        // 应用中文字体
        ApplyChineseFont();

        // 初始化界面，设置问题
        gptQuestion.text = "在刚才的对话中，小男孩们的心情是怎么样的？";
        gptFeedback.text = "";

        // 确保学生输入框中显示提示文字
        if (studentResponse != null && studentResponse.placeholder is TMP_Text placeholderText)
        {
            placeholderText.text = "请输入您的答案";
        }

        // 初始化消息列表，使用系统提示
        messages.Add(new ChatMessage { role = "system", content = systemPrompt });

        // 添加初始问题
        messages.Add(new ChatMessage { role = "assistant", content = "在刚才的对话中，小男孩们的心情是怎么样的？" });

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
        string userText = studentResponse.text.Trim();
        Debug.Log("学生回答: " + userText);

        // 记录当前回合数
        Debug.Log("当前回合: " + round + "/" + maxRounds);

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

                // 检查学生回答中是否包含关键词
                bool containsRealization = userText.Contains("知道自己错了") || userText.Contains("意识到错误") ||
                                          userText.Contains("后悔") || userText.Contains("明白自己做错了");

                // 如果包含所有关键词或达到最大回合数
                if (containsRealization || round >= maxRounds)
                {
                    if (containsRealization)
                    {
                        // 回答正确
                        gptFeedback.text = "回答正确！请继续在场景中探索。";
                    }
                    else if (round >= maxRounds)
                    {
                        // 达到最大回合数但答案不完整
                        gptFeedback.text = "这是一个很好的尝试，但完整的描述应该是：经过和小女孩的对话，两个小男孩知道自己错了。";
                    }

                    // 结束对话
                    EndConversation();
                }
                else
                {
                    // 清空学生输入框，准备下一轮
                    studentResponse.text = "";
                }
            }
        }
    }

    void EndConversation()
    {
        conversationEnded = true;

        // 对话结束，等待10秒后将End变量设置为"1"
        StartCoroutine(DelayedEndSignal());

        Debug.Log("对话完成，10秒后End变量将设置为1");
        submitButton.interactable = false;
    }

    // 协程：延迟10秒后设置End变量并在控制台输出
    IEnumerator DelayedEndSignal()
    {
        // 等待10秒
        yield return new WaitForSeconds(10f);

        // 设置End变量为"1"
        End = "1";

        // 在控制台输出End变量的值
        Debug.Log("End值: " + End);
    }
}
