using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 move;
    public Transform playerBody;
    public float moveSpeed = 7f;
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 위아래 회전 제한 (X축)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 고개가 위아래로 90도 이상 움직이지 않도록 제한

        // 카메라와 캐릭터 몸체 회전 적용
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // 카메라 회전 (Pitch)
        transform.Rotate(Vector3.up * mouseX); // 몸체 회전 (Yaw)

        float horizontal = Input.GetAxis("Horizontal"); // A(-1)와 D(+1) 또는 화살표 키 좌우
        float vertical = Input.GetAxis("Vertical");     // W(+1)와 S(-1) 또는 화살표 키 상하

        // 입력 벡터 생성 (카메라 방향 고려하지 않은 로컬 방향)
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
        // 플레이어가 보는 방향으로 변환
        Vector3 move = transform.TransformDirection(inputDirection);

        transform.localPosition += move * moveSpeed * Time.deltaTime;
    }
}
