using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    public float moveSpeed = 5f;         // 플레이어 이동 속도
    public float mouseSensitivity = 100f; // 마우스 회전 감도
    public float rotationSpeed = 10f;   // 플레이어 회전 속도
    public Transform cameraTransform;   // 플레이어에 붙어있는 카메라 Transform

    private Rigidbody rb;               // Rigidbody 컴포넌트
    private float cameraPitch = 0f;     // 카메라 상하 회전 각도
    private Vector3 moveDirection;      // 이동 방향 저장

    private Vector3 savepoint = new Vector3(5.43900013f, 10.3209829f, 22.4939995f);
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform; // 기본 카메라 설정
        }

        // Rigidbody 설정: 중력은 유지하고 회전은 수동으로 처리
        rb.freezeRotation = true;

        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        RotateCamera();  // 마우스 입력으로 카메라 회전
        CalculateMoveDirection(); // 이동 방향 계산
    }

    void FixedUpdate()
    {
        MovePlayer(); // Rigidbody를 이용한 물리 이동
    }

    void RotateCamera()
    {
        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 카메라 상하 회전 (Pitch 제한)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -45f, 45f); // -45도 ~ 45도 제한

        // 카메라와 플레이어 회전
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f); // 상하 회전
        transform.Rotate(Vector3.up * mouseX); // 좌우 회전
    }

    void CalculateMoveDirection()
    {
        // 입력 값 받아오기
        float horizontal = Input.GetAxis("Horizontal"); // A, D 또는 ←, →
        float vertical = Input.GetAxis("Vertical");     // W, S 또는 ↑, ↓

        // 카메라의 앞, 오른쪽 방향 벡터 계산
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // 수직 방향 제거 (y축 제거)
        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // 이동 방향 계산
        moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
    }

    void MovePlayer()
    {
        if (moveDirection.magnitude > 0)
        {
            // Rigidbody를 이용한 이동
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

            // 플레이어 회전 처리
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Lerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("DieZone")) // 다이존일 경우
        {
            // Rigidbody 위치를 저장된 위치로 변경
            rb.position = savepoint;

            // 물리적 이동을 안정화하기 위해 velocity 초기화
            rb.velocity = Vector3.zero;
        }

        else if (other.gameObject.CompareTag("goal_push"))
        {
            // 골 오브젝트의 위치를 세이브포인트로 변경
            savepoint = other.transform.position;

            // 플레이어 위치를 골 위치로 이동
            rb.position = savepoint;

            // 물리적 이동을 안정화하기 위해 velocity 초기화
            rb.velocity = Vector3.zero;
        }

        else if (other.gameObject.CompareTag("goal_jin"))
        {
            // 골 오브젝트의 위치를 세이브포인트로 변경
            savepoint = other.transform.position;

            // 플레이어 위치를 골 위치로 이동
            rb.position = savepoint;

            // 물리적 이동을 안정화하기 위해 velocity 초기화
            rb.velocity = Vector3.zero;
        }
    }
}

