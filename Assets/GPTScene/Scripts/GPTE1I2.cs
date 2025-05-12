using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GPTE1I2 : MonoBehaviour
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

    private string systemPrompt = "你是一位中文学习助手。你将评价学生对图片的描述并引导他们。图片显示一个小女孩坐在桌前学习，她的表情显示她似乎听到了窗外传来的吵闹声或嘈杂声，表情有些疑惑或困扰。\n\n你的任务是：\n1. 评价学生的回答\n2. 如果回答不够具体或不完全正确，提供引导性提示，帮助学生思考出确切的描述。例如：\n   - 如果学生只说'窗外的声音'或类似模糊答案，引导他们更具体地描述声音的性质：'你能描述一下这是什么类型的声音吗？从小女孩的表情看，这声音是安静的还是嘈杂的？'\n   - 如果学生提到'声音'但没有描述其特性，可以引导：'小女孩的表情似乎表明她听到的不是普通的声音，你能更具体地描述这声音的特点吗？'\n3. 如果学生回答包含以下关键词之一，才判定为正确：'吵闹声'、'喧闹声'、'嘈杂声'、'噪音'、'喧哗声'、'嘈杂'、'吵杂'、'吵闹'，然后告诉他们'回答正确，请继续去房间里探索'\n\n重要：\n- 你的回答必须是纯文本\n- 不接受模糊或一般性的答案，如'窗外的声音'、'外面的声音'等，这些都不够具体\n- 引导学生思考声音的性质和特点，但不直接提供答案\n- 只有当学生明确提到表示嘈杂、吵闹性质的词语时，才判定为正确";

    private List<ChatMessage> messages = new List<ChatMessage>();

    void Start()
    {
        // 应用中文字体
        ApplyChineseFont();

        // 初始化界面
        gptQuestion.text = "小女孩听见了什么？";
        gptFeedback.text = "";

        // 确保学生输入框中显示提示文字
        if (studentResponse != null && studentResponse.placeholder is TMP_Text placeholderText)
        {
            placeholderText.text = "请输入您的答案";
        }

        // 初始化消息列表
        messages.Add(new ChatMessage { role = "system", content = systemPrompt });

        // 添加初始问题
        messages.Add(new ChatMessage { role = "assistant", content = "小女孩听见了什么？" });

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

                // 检查内容是否表示正确回答 - 必须明确包含关键词的正确回答
                bool isCorrect = content.Contains("正确") && content.Contains("继续") &&
                    (content.Contains("回答正确") ||
                     studentResponse.text.Contains("吵闹声") ||
                     studentResponse.text.Contains("喧闹声") ||
                     studentResponse.text.Contains("嘈杂声") ||
                     studentResponse.text.Contains("噪音") ||
                     studentResponse.text.Contains("喧哗声") ||
                     studentResponse.text.Contains("嘈杂") ||
                     studentResponse.text.Contains("吵杂") ||
                     studentResponse.text.Contains("吵闹"));

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
            // 提供更丰富的标准答案示例
            string[] exampleAnswers = new string[] {
                "窗外传来了吵闹声。",
                "她听见了窗外的喧闹声。",
                "小女孩听到了外面的嘈杂声。",
                "她听见窗外传来的噪音。"
            };

            // 随机选择一个示例答案
            string standardAnswer = exampleAnswers[Random.Range(0, exampleAnswers.Length)];

            gptFeedback.text += "\n\n标准答案：" + standardAnswer + "请继续去房间里探索。";

            // 记录对话结束
            Debug.Log("对话结束，学生未找到正确答案。提供标准答案。");
        }
        else
        {
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
        if (End == "1")
        {
            SceneManager.LoadScene("ARScene");
        }

        // 在控制台输出End变量的值
        Debug.Log("End值: " + End);
    }
    
}
