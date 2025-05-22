using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public bool isFadeIn;
    public GameObject panel;
    public Button targetButton; // ������ ��ư
    public string sceneName;
    private CanvasGroup canvasGroup;
    private Action onCompleteCallback;
    

    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // FadeController ������Ʈ�� �� ��ȯ �Ŀ��� �����ǵ��� ����
        SceneManager.sceneLoaded += OnSceneLoaded; // �� �ε� �̺�Ʈ�� ���
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // �� �ε� �̺�Ʈ���� ����
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �� ��ȯ �� panel�� canvasGroup�� �ٽ� ã��
        if (panel == null)
        {
            panel = GameObject.Find("YourPanelName"); // �� ������ panel�� ã�� (�г��� ���� �̸����� ���� �ʿ�)
        }
        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        if (!panel)
        {
            Debug.LogError("Panel ������Ʈ ã�� �� ����");
            throw new MissingComponentException();
        }

        // CanvasGroup ������Ʈ�� ������ �߰�
        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        // ��ư�� OnClick�� FadeOutAndLoadScene�� �������� �߰�
        if (targetButton != null)
        {
            targetButton.onClick.RemoveAllListeners(); // �ߺ� ������ ���� ��� ������ ����
            targetButton.onClick.AddListener(() => FadeOutAndLoadScene(sceneName));
        }

        // �ʱ� ����
        if (isFadeIn)
        {
            panel.SetActive(true);
            canvasGroup.alpha = 1f;
            StartCoroutine(CoFadeIn());
        }
        else
        {
            canvasGroup.alpha = 0f;
            panel.SetActive(false);
        }

        
    }

    public void FadeOutAndLoadScene(string sceneName)
    {
        if (panel == null || canvasGroup == null)
        {
            Debug.LogWarning("�г� �Ǵ� CanvasGroup�� �������� ����, ���̵� �ƿ��� ������ �� ����");
            return;
        }

        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        StartCoroutine(CoFadeOut(() =>
        {
            
            
            SceneManager.LoadScene(sceneName); // �� ��ȯ
            StartCoroutine(CoFadeIn()); // �� ��ȯ �� ���̵� �� ����
        }));
    }

    IEnumerator CoFadeIn()
    {
        float elapsedTime = 0f;

        // canvasGroup�� ��ȿ���� Ȯ���Ͽ� null ����
        if (canvasGroup == null) yield break;

        while (elapsedTime <= fadeDuration)
        {
            if (canvasGroup == null) yield break; // �� ��ȯ �� canvasGroup�� �ı��� ��� ����
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            panel.SetActive(false);
            Debug.Log("Fade In completed.");
        }
    }

    IEnumerator CoFadeOut(Action onComplete)
    {
        float elapsedTime = 0f;

        // canvasGroup�� ��ȿ���� Ȯ���Ͽ� null ����
        if (canvasGroup == null) yield break;

        while (elapsedTime <= fadeDuration)
        {
            if (canvasGroup == null) yield break; // �� ��ȯ �� canvasGroup�� �ı��� ��� ����
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            onComplete?.Invoke(); // ���̵� �ƿ� �Ϸ� �� �ݹ� ȣ��
            Debug.Log("Fade Out completed.");
        }
    }
}
