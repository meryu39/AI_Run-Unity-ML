using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;

public class player_agent1 : Agent
{

    public Transform target; // 목표 지점
    public float moveSpeed = 7f;
    public float maxEpisodeTime = 45.0f; // 에피소드 최대 시간
    private float currentEpisodeTime;
    private float rewardzone_count = 1.0f;

    // 초기화 지점
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public float rayLength = 10f;  // 레이의 길이
    public float rayAngle = 30f;   // 레이의 좌우 각도

    public GameObject obstaclePrefab; // 장애물 프리팹
    public Transform[] emptyPositions; //장애물 배치할 트랜스폼
    private List<GameObject> currentObstacles = new List<GameObject>(); // 생성된 장애물 목록


    public override void OnEpisodeBegin()
    {
        // 에피소드 초기화
        transform.localPosition = startPosition;
        target.localPosition = targetPosition;
        currentEpisodeTime = 0f; // 에피소드 타이머 초기화
        rewardzone_count = 1.0f;
        //obstacle_repatch();

        //기존의 장애물 제거
         foreach (GameObject obstacle in currentObstacles)
        {
            Destroy(obstacle);
        }
        currentObstacles.Clear(); // 리스트 초기화

        // 장애물 생성
        CreateObstacles();
    }


    private void CreateObstacles()
    {
        // emptyPositions 배열의 위치값으로 장애물 재생성
        foreach (Transform position in emptyPositions)
        {
            
            GameObject newObstacle = Instantiate(obstaclePrefab, position.position, Quaternion.identity);
            currentObstacles.Add(newObstacle);
        }
    }


    private void Update()
    {
        // 시간 초과 체크
        currentEpisodeTime += Time.deltaTime;
        if (currentEpisodeTime >= maxEpisodeTime)
        {
            AddReward(-3.0f); // 시간 초과 페널티
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 에이전트 위치
        sensor.AddObservation(transform.localPosition);
        // 에이전트와 목표 지점 사이의 거리
        sensor.AddObservation(target.localPosition - transform.localPosition);


        // 360도 레이캐스트 쏴서 장애물 식별(최대 5개)
        List<GameObject> detectedObstacles = GetAllObstaclesOnPath();
        int maxObservations = 5;

        //장애물별 좌표값 , 거리 관찰값 저장
        for (int i = 0; i < Mathf.Min(detectedObstacles.Count, maxObservations); i++)
        {
            GameObject obstacle = detectedObstacles[i];
            sensor.AddObservation(obstacle.transform.localPosition - transform.localPosition);
            sensor.AddObservation(obstacle.transform.localPosition);

        }

        //더미저장
        for (int i = detectedObstacles.Count; i < maxObservations; i++)
        {
            sensor.AddObservation(Vector3.zero); // 위치
            sensor.AddObservation(Vector3.zero); // 위치
        }

        float normalizedTime = currentEpisodeTime / maxEpisodeTime;
        sensor.AddObservation(normalizedTime); // 남은 시간 비율
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 행동 수집
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);

        if (Mathf.Approximately(move.x, 0) && Mathf.Approximately(move.z, 0))
        {
            // 대기 상태일 때는 움직이지 않음
            AddReward(-0.001f); // 대기중에 패널티
        }
        else
        {
            transform.localPosition += move * moveSpeed * Time.deltaTime;
        }

        // 목표 지점과의 거리 계산 및 보상 설정
        //float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
         // 시간 패널티
        /*
        if (distanceToTarget < 1.5f)
        {
            SetReward(10f); // 목표 도달
            EndEpisode();
        }
        */
    }

    //레이캐스트를 통한 장애물 식별
    private List<GameObject> GetAllObstaclesOnPath()
    {
        RaycastHit hit;
        List<GameObject> obstacles = new List<GameObject>();

        //해당 장애물 레이어만 레이캐스트
        int obstacleLayerMask = LayerMask.GetMask("Obstacle");

        // 360도 레이캐스트 발사 
        for (int i = -180; i <= 180; i += 30) // 30도마다 발사
        {
            Vector3 direction = Quaternion.Euler(0, i, 0) * transform.forward;

            if (Physics.Raycast(transform.position, direction, out hit, rayLength, obstacleLayerMask))
            {
                //장애물 레이캐스트에 닿을경우 리스트에 추가
                if (hit.collider != null && !obstacles.Contains(hit.collider.gameObject))
                {
                    obstacles.Add(hit.collider.gameObject);
                }
            }
        }

        return obstacles;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DieZone")) // 다이존일 경우
        {
            SetReward(-1.0f); 
            EndEpisode();
        }
        if (other.gameObject.CompareTag("StartDieZone")) // 스타트다이존일 경우
        {
            SetReward(-2.0f); 
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("rewardzone")) // 리워드존일 경우
        {
            rewardzone_count += 0.1f;
            SetReward(1.5f * rewardzone_count); //도착한 리워드존 개수에 따라 보상
            other.gameObject.SetActive(false);
        }
        else if (other.gameObject.CompareTag("goal"))
        {
            float timeBonus = Mathf.Max(0, (maxEpisodeTime - currentEpisodeTime)); // 남은 시간 계산
            SetReward(5.0f + 0.2f * timeBonus); // 보상 + 시간
            EndEpisode();
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 객체가 장애물일경우
        if (collision.gameObject.CompareTag("obstacle"))
        {
            SetReward(-0.4f); // 벽에 부딪히면 패널티
        }
    }

}
