using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Delaytime(0.1f);
        Debug.Log("앱이 시작됩니다.");
        // 데이터 저장, 로그, 정리 작업 등 수행
        PlayerPrefs.DeleteAll(); //초기화
        PlayerPrefs.Save();

        Delaytime(0.1f);
        
        SceneManager.LoadScene("ARScene");

    }
    private IEnumerator Delaytime(float sec)
    {
        yield return new WaitForSeconds(sec); // 한 프레임 대기
        
    }

    
}
