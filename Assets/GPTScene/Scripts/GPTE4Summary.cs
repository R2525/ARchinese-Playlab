using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class GPTE4Summary : MonoBehaviour
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

    // 关键词列表
    private List<string> keywords = new List<string>()
    {
        "操场",
        "草地",
        "楼下",
        "笑容",
        "开心",
        "公德心"
    };

    private string systemPrompt = "你是一位中文学习助手。你将评价学生对图片的描述并引导他们。图片显示一个四格漫画，描述了一个女孩在学习时被窗外踢球的声音打扰，最后小朋友们去了楼下操场踢球，主题是'公德心'。\n\n你的任务是：\n1. 评价学生的回答\n2. 如果学生的回答不完整，提供引导性的问题，鼓励他们根据图片情节补充，但不要明确告诉他们需要哪些具体关键词\n3. 温和地引导学生思考故事的主题是什么，但不要直接说出'公德心'这个词\n4. 如果学生回答包含所有关键词（操场、草地、楼下、笑容、开心、公德心），则判定为正确，然后告诉他们'回答正确，完成探索'\n\n重要：\n- 你的回答必须是纯文本\n- 不要在反馈中列出具体的关键词，而是提出开放性问题引导思考\n- 引导学生关注图片中的情节变化和人物表情\n- 引导学生思考为什么小朋友要到别的地方去踢球以及女孩为什么会露出笑容\n- 只有当学生明确提到所有关键词时，才判定为完全正确";

    private List<ChatMessage> messages = new List<ChatMessage>();

    void Start()
    {
        // 应用中文字体
        ApplyChineseFont();

        // 初始化界面
        gptQuestion.text = "请你给论文加一个结尾";
        gptFeedback.text = "";

        // 确保学生输入框中显示提示文字并设置为多行
        if (studentResponse != null)
        {
            // 设置为多行输入
            studentResponse.lineType = TMP_InputField.LineType.MultiLineNewline;
            studentResponse.lineLimit = 5; // 可以设置最大行数限制

            // 设置提示文字
            if (studentResponse.placeholder is TMP_Text placeholderText)
            {
                placeholderText.text = "请输入您的答案";
            }
        }

        // 初始化消息列表
        messages.Add(new ChatMessage { role = "system", content = systemPrompt });

        // 添加初始问题
        messages.Add(new ChatMessage { role = "assistant", content = "请你给论文加一个结尾" });

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

                // 保留学生回答并添加GPT反馈
                gptFeedback.text = "你的回答：\n" + userText + "\n\n" + "反馈：\n" + content;

                // 确保文本使用正确的字体
                if (chineseFont != null && gptFeedback != null)
                {
                    gptFeedback.font = chineseFont;
                }

                // 检查学生回答是否包含所有关键词
                bool containsAllKeywords = true;
                foreach (string keyword in keywords)
                {
                    if (!userText.Contains(keyword))
                    {
                        containsAllKeywords = false;
                        break;
                    }
                }

                // 判断是否正确或达到最大回合数
                if (containsAllKeywords || round >= maxRounds)
                {
                    EndConversation(containsAllKeywords);
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
            // 提供标准答案示例
            string standardAnswer = "两个小朋友听了小女孩的话，跑去别的楼下操场踢球了。姐姐笑了，继续安心地温习功课。这个故事告诉我们，要做有公德心的人。";

            gptFeedback.text += "\n\n参考结尾：\n" + standardAnswer;

            // 记录对话结束
            Debug.Log("对话结束，学生未找到正确答案。提供标准答案。");
        }
        else
        {
            // 补充完成探索的信息
            gptFeedback.text += "\n\n完成探索。";

            // 记录对话成功结束
            Debug.Log("对话成功结束，学生找到了正确答案。");
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

        // 在控制台输出End变量的值
        Debug.Log("End值: " + End);
    }
}
