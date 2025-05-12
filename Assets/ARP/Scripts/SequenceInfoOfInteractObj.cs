using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SequenceInfoOfInteractObj : MonoBehaviour
{
    private GameObject object1;
    private GameObject object2;
    private GameObject object3;
    private GameObject scene1;
    private GameObject scene2;

    public int currentStep = -1;
    private GameObject[] objectSequence;

    public static SequenceInfoOfInteractObj Instance;

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
        }
    }

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
        StartCoroutine(DelayedSetTouchInable());
        Debug.Log("📣 씬 로드됨: " + scene.name);
        SetTouchInable();
    }
    private IEnumerator DelayedSetTouchInable()
    {
        yield return new WaitForSeconds(0.1f); // 한 프레임 대기
        
    }
    private GameObject FindInactiveObject(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name && obj.scene.IsValid()) // 에디터 내 prefab은 무시
            {
                return obj;
            }
        }
        return null;
    }
    //상호작용한 오브젝트 지정 
    public void SetTouchInable()
    {
        currentStep = LoadData();
        Debug.Log("📥 SetTouchInable: currentStep = " + currentStep);
        
        if(object1 ==null) object1 = GameObject.Find("Book And Pencil No light");
        if (object2 == null) object2 = GameObject.Find("window");
        if (object3 == null)object3 = GameObject.Find("chair");
        //AR씬 같은 경우 편의성을 위해 같은 씬에서 오브젝트만 변경하여 씬 변화를 준것처럼 
        if (scene1 == null)scene1 = FindInactiveObject("Bedroom");
        if (scene2 == null)scene2 = FindInactiveObject("HDB corridor");
        
        //오브젝트 터치 순서
        objectSequence = new GameObject[] { object1, object2, object3 };
        
        
        //bedroom - outdoor scene
        if (scene1 != null && scene2 != null)
        {
            if (currentStep <= 0)
            {
                scene1.gameObject.SetActive(true); //bedroomScene 활성화
                scene2.gameObject.SetActive(false);
            }

            if (currentStep >= 3)
            {
                scene1.gameObject.SetActive(false); //bedroomScene 비활성화
                scene2.gameObject.SetActive(true);
            }
            
            
        }
        
        //오브젝트 순서를 정하고 collider를 활성화
        for (int i = 0; i < objectSequence.Length; i++)
        {
            if (objectSequence[i] == null)
            {
                Debug.LogWarning($"⚠️ objectSequence[{i}]가 null입니다. 이름 확인 필요.");
                continue;
            }

            objectSequence[i].GetComponent<BoxCollider>().enabled = (i == currentStep);
        }
    }

    //상호작용 가능한 오브젝트 재설정
    public void AdvanceToNextObject()
    {
        if (objectSequence == null || currentStep >= objectSequence.Length)
            return;

        if (objectSequence[currentStep] != null)
            objectSequence[currentStep].GetComponent<BoxCollider>().enabled = false;
        

        currentStep++;

        if (currentStep < objectSequence.Length && objectSequence[currentStep] != null)
            objectSequence[currentStep].GetComponent<BoxCollider>().enabled = true;

        SaveData(currentStep);
    }

    public void LogCurrentStep()
    {
        Debug.Log("📌 Current Step: " + currentStep);
        
    }
    
    //데이터 로더
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
}