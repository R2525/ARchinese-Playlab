using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GPTE1Summary : MonoBehaviour
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
    private int taskStage = 1; // 1 = 第一个任务, 2 = 第二个任务

    // 添加End变量，初始化为0
    private string End = "0";

    // 添加全局变量存储女孩的名字，可以被其他脚本访问
    public static string girlName = ""; // 静态变量，可被其他脚本访问

    // 第一个任务的系统提示
    private string systemPrompt1 = "你是一位中文学习助手。你的任务是引导学生创建一个关于图片中小女孩的基本故事情境。\n\n图片显示一个小女孩坐在桌前学习，她的眉头紧皱，对窗外传来的吵闹声感到不耐烦。\n\n你的任务是：\n1. 引导学生给女孩取一个名字，并描述时间、地点和她在做什么\n2. 评估学生的回答是否包含以下所有要素：\n   - 时间（例如：早上、下午、晚上、周末等）\n   - 地点（例如：家里、学校、图书馆等）\n   - 人物（女孩的名字）\n   - 活动（例如：学习、写作业、温习功课等）\n3. 如果缺少任何要素，提示学生补充，但不要直接告诉答案\n   - 例如：'你还没有提到是什么时候发生的，可以加上这个信息吗？'\n   - 或：'你能说一下这件事发生在哪里吗？'\n\n重要：\n- 你的回答必须是纯文本\n- 当学生的回答包含所有要素时，告诉他们回答正确，并告知会进入下一个问题\n- 记住学生给女孩取的名字，后续问题中会使用这个名字\n- 五轮对话后，如果学生仍未提供完整回答，给出例句：'有一天下午，[学生取的名字]在家专心地温习功课，突然窗外传来了吵闹声。'";

    // 第二个任务的系统提示
    private string systemPrompt2 = "你是一位中文学习助手。现在进入第二个问题。\n\n你的任务是：\n1. 引导学生表达如果他是[女孩名字]，他的心情会如何\n2. 评估学生的回答是否表达了负面情绪，尤其是'不耐烦'或类似情绪（如：烦躁、生气、恼火等）\n3. 如果学生没有表达适当的情绪，提供场景化的提示：\n   - 例如：'如果你在写作业的时候，有人在窗外喧哗，你的感受是什么？'\n   - 或：'想象一下，当你需要集中注意力时，外面却很吵，你会有什么感觉？'\n\n重要：\n- 你的回答必须是纯文本\n- 使用学生在第一个问题中给女孩取的名字\n- 当学生表达了适当的负面情绪时，告诉他们回答正确\n- 五轮对话后，如果学生仍未提供合适的回答，给出例句：'如果在我温习功课的时候，有人在窗外喧哗。我会觉得很不耐烦。'";

    private List<ChatMessage> messages = new List<ChatMessage>();

    void Start()
    {
        // 应用中文字体
        ApplyChineseFont();

        // 初始化界面，设置第一个问题
        gptQuestion.text = "请你给女孩取一个名字，然后写出什么时候，小女孩在哪里，正在做什么。";
        gptFeedback.text = "";

        // 确保学生输入框中显示提示文字
        if (studentResponse != null && studentResponse.placeholder is TMP_Text placeholderText)
        {
            placeholderText.text = "请输入您的答案";
        }

        // 初始化消息列表，使用第一个任务的系统提示
        messages.Add(new ChatMessage { role = "system", content = systemPrompt1 });

        // 添加初始问题
        messages.Add(new ChatMessage { role = "assistant", content = "请你给女孩取一个名字，然后写出什么时候，小女孩在哪里，正在做什么。" });

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

        // 记录当前回合数和任务阶段
        Debug.Log("当前回合: " + round + "/" + maxRounds + ", 任务阶段: " + taskStage);

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

                // 第一个任务：检查内容是否表示正确回答（包含所有必要元素）
                if (taskStage == 1)
                {
                    // 如果GPT回复中包含"回答正确"或者达到最大回合数
                    bool isCorrect = content.Contains("回答正确") || round >= maxRounds;

                    // 尝试提取学生给女孩取的名字
                    if (isCorrect || round >= maxRounds)
                    {
                        // 从学生的所有回答中尝试提取名字
                        ExtractGirlName();

                        if (round >= maxRounds && !content.Contains("回答正确"))
                        {
                            // 如果达到最大回合数但答案不完整，提供完整的例句
                            string exampleSentence = "有一天下午，" + girlName + "在家专心地温习功课，突然窗外传来了吵闹声。";
                            gptFeedback.text += "\n\n完整的句子应该是：" + exampleSentence;
                        }

                        // 切换到第二个任务
                        StartCoroutine(SwitchToSecondTask());
                    }
                    else
                    {
                        // 清空学生输入框，准备下一轮
                        studentResponse.text = "";
                    }
                }
                // 第二个任务：检查内容是否表示正确的情绪回答
                else if (taskStage == 2)
                {
                    // 检查回答是否包含负面情绪词汇
                    bool hasNegativeEmotion = content.Contains("回答正确") ||
                                            studentResponse.text.Contains("不耐烦") ||
                                            studentResponse.text.Contains("烦躁") ||
                                            studentResponse.text.Contains("生气") ||
                                            studentResponse.text.Contains("恼火") ||
                                            studentResponse.text.Contains("讨厌") ||
                                            studentResponse.text.Contains("不高兴") ||
                                            studentResponse.text.Contains("不满") ||
                                            studentResponse.text.Contains("生气");

                    // 如果回答包含负面情绪或达到最大回合数
                    if (hasNegativeEmotion || round >= maxRounds)
                    {
                        // 如果达到最大回合数但未表达合适的情绪
                        if (round >= maxRounds && !hasNegativeEmotion)
                        {
                            // 提供标准答案
                            string standardAnswer = "如果在我温习功课的时候，有人在窗外喧哗。我会觉得很不耐烦。";
                            gptFeedback.text += "\n\n正确的回答可以是：" + standardAnswer;
                        }

                        // 结束对话
                        EndConversation(hasNegativeEmotion);
                    }
                    else
                    {
                        // 清空学生输入框，准备下一轮
                        studentResponse.text = "";
                    }
                }
            }
        }
    }

    // 从学生的对话中提取女孩的名字
    void ExtractGirlName()
    {
        // 默认名字，以防无法提取
        if (string.IsNullOrEmpty(girlName))
        {
            girlName = "小明";
        }

        // 遍历学生的所有回答，尝试找出名字
        foreach (ChatMessage message in messages)
        {
            if (message.role == "user")
            {
                // 这里需要更复杂的逻辑来提取名字
                // 简化版：假设第一个出现的人名就是给女孩取的名字
                // 实际应用中，可能需要使用NLP或更复杂的模式匹配
                string[] commonChineseNames = new string[] {
                    "小明", "小红", "小华", "小丽", "小芳", "小燕", "小玲", "小雪",
                    "小芳", "小薇", "小琳", "晓晓", "明明", "丽丽", "华华", "冰冰"
                };

                foreach (string name in commonChineseNames)
                {
                    if (message.content.Contains(name))
                    {
                        girlName = name;
                        Debug.Log("提取到的女孩名字: " + girlName);
                        return;
                    }
                }

                // 另一种提取方式：查找"叫"、"名字是"等关键词后的内容
                string[] nameIndicators = new string[] { "叫", "名字是", "名叫", "取名" };
                foreach (string indicator in nameIndicators)
                {
                    int index = message.content.IndexOf(indicator);
                    if (index >= 0 && index + indicator.Length + 3 < message.content.Length)
                    {
                        // 尝试提取2-3个字的名字
                        girlName = message.content.Substring(index + indicator.Length, 3).Trim('，', ',', '。', '、', ' ', ':', '：', '"', '"', '的');
                        Debug.Log("通过关键词提取到的女孩名字: " + girlName);
                        return;
                    }
                }
            }
        }

        Debug.Log("无法提取名字，使用默认名字: " + girlName);
    }

    // 切换到第二个任务
    IEnumerator SwitchToSecondTask()
    {
        yield return new WaitForSeconds(2f); // 给学生时间阅读反馈

        taskStage = 2;
        round = 0; // 重置回合数

        // 使用女孩的名字更新系统提示
        string updatedSystemPrompt2 = systemPrompt2.Replace("[女孩名字]", girlName);

        // 清空之前的消息记录，创建新的对话
        messages.Clear();
        messages.Add(new ChatMessage { role = "system", content = updatedSystemPrompt2 });

        // 设置第二个问题
        string question = "如果你是" + girlName + "，你的心情会如何？";
        gptQuestion.text = question;
        messages.Add(new ChatMessage { role = "assistant", content = question });

        gptFeedback.text = ""; // 清空反馈
        studentResponse.text = ""; // 清空输入框

        Debug.Log("切换到第二个任务，女孩名字: " + girlName);
    }

    void EndConversation(bool isCorrect)
    {
        conversationEnded = true;

        // 对话结束条件已达到，无论回答正确与否，都等待10秒后将End变量设置为"1"
        StartCoroutine(DelayedEndSignal());

        if (!isCorrect)
        {
            Debug.Log("对话结束，学生未找到正确答案。提供标准答案。");
        }
        else
        {
            Debug.Log("对话成功结束，学生找到了正确答案。");
        }

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
        Debug.Log("End值: " + End + ", 女孩名字: " + girlName);
        if (End == "1")
        {
            SceneManager.LoadScene("ARScene");
        }
    }

    // 提供一个公共方法让其他脚本获取女孩的名字
    public static string GetGirlName()
    {
        return girlName;
    }
}