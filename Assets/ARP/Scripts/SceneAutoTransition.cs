using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAutoTransition : MonoBehaviour
{
    public string arSceneName = "ARScene";
    public string nextSceneName = "BalconyTalkingSummary";
    public float delayBeforeNextScene = 10f;
    private int currentStep;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🎬 씬 로드 감지됨: {scene.name}");
        currentStep = LoadData();
        currentStep++;
        if (scene.name == arSceneName)
        {
            int fromGPT = PlayerPrefs.GetInt("cameFromGPTScene", 0);
            Debug.Log($"🔍 cameFromGPTScene: {fromGPT}");

            if (fromGPT == 1)
            {
                Debug.Log("✅ GPT 씬에서 왔으므로 ARScene에서 자동 전환 준비");
                StartCoroutine(DelayedSceneLoad());
            }
            else
            {
                Debug.Log("⛔ GPT 씬에서 온 게 아니므로 자동 전환 안 함");
            }
        }
    }
    public void SaveData(int step)
    {
        Debug.Log("💾 Saving step: " + step);
        PlayerPrefs.SetInt("step", step);
        
        PlayerPrefs.Save();
    }

    public int LoadData()
    {
        int step = PlayerPrefs.GetInt("step", 0);
        
        Debug.Log("📤 LoadData: step = " + step);
        
        return step;
    }

    private IEnumerator DelayedSceneLoad()
    {
        Debug.Log(currentStep+"sdsdvsdsd");
        SaveData(currentStep);
        yield return new WaitForSeconds(delayBeforeNextScene);
        
        // 상태 초기화
        PlayerPrefs.SetInt("cameFromGPTScene", 0);
        PlayerPrefs.Save();

        Debug.Log($"⏱️ {delayBeforeNextScene}초 후 {nextSceneName} 씬으로 전환");
        SceneManager.LoadScene(nextSceneName);
    }
}