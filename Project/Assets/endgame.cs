using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위한 네임스페이스 추가

public class endgame : MonoBehaviour
{
    // 충돌 시 호출되는 메서드
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 객체가 "Player" 태그인 경우
        if (collision.gameObject.CompareTag("User"))
        {
            // 게임 종료
            Application.Quit();

            
            UnityEditor.EditorApplication.isPlaying = false;
        }
        // 충돌한 객체가 "AI" 태그인 경우
        else if (collision.gameObject.CompareTag("Player"))
        {
            // 현재 씬 재시작
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Debug.Log("들어왔습니다");
        }
    }
}
