using UnityEngine;

public class QuitManager : MonoBehaviour
{
    //사용 이유 PlayerPrefs 초기화
    
    // void Awake()
    // {
    //     if (!PlayerPrefs.HasKey("AppInitialized"))
    //     {
    //         Debug.Log("앱 첫 실행입니다. PlayerPrefs 초기화");
    //         PlayerPrefs.DeleteAll(); // 전체 초기화
    //         PlayerPrefs.SetInt("AppInitialized", 1); // 플래그 설정
    //         PlayerPrefs.Save();
    //     }
    // }

    void OnApplicationQuit() {
        Debug.Log("앱이 종료됩니다.");
        // 데이터 저장, 로그, 정리 작업 등 수행
        PlayerPrefs.DeleteAll(); //초기화
        PlayerPrefs.Save();
    }
 
}
