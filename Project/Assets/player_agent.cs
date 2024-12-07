using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using UnityEngine;

public class player_agent : Agent
{
    public Transform target; // 목표 지점
    public GameObject[] obstacles; // 장애물 배열
    public float moveSpeed = 10f;
    public float maxEpisodeTime = 60f; // 에피소드 최대 시간
    private float currentEpisodeTime;

    public override void OnEpisodeBegin()
    {
        // 에피소드 초기화
        transform.localPosition = new Vector3(-8.34f, 0.94f, 36.24f);
        target.localPosition = new Vector3(-8.30000019f, 0.230000004f, -17.3109055f);
        currentEpisodeTime = 0f; // 에피소드 타이머 초기화

    }

    private void Update()
    {
        // 시간 초과 체크
        currentEpisodeTime += Time.deltaTime;
        if (currentEpisodeTime >= maxEpisodeTime)
        {
            AddReward(-1f); // 시간 초과 페널티
            EndEpisode();
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // 에이전트와 목표 지점 사이의 거리
        sensor.AddObservation(target.localPosition - transform.localPosition);

        //목표지점 - 플레이어 사이 가장 가까운 장애물(플레이어 입장)
        GameObject closestObstacle = GetClosestObstacleOnPath();
        if (closestObstacle != null)
        {
            sensor.AddObservation(closestObstacle.transform.localPosition - transform.localPosition);
            sensor.AddObservation(closestObstacle.GetComponent<Rigidbody>().velocity);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 행동 수집
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        

        if (Mathf.Approximately(move.x, 0) && Mathf.Approximately(move.z, 0))
        {
            // 대기 상태일 때는 움직이지 않음
            AddReward(-0.003f); // 대기중에 패널티
        }
        else
        {
            transform.localPosition += move * moveSpeed * Time.deltaTime;
        }

        // 보상 설정
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
        AddReward(-0.01f); // 시간 패널티
        if (distanceToTarget < 1.5f)
        {
            SetReward(100f); // 목표 도달
            EndEpisode();
        }

        foreach (var obstacle in obstacles)
        {
            if (Vector3.Distance(transform.localPosition, obstacle.transform.localPosition) < 1.0f)
            {
                SetReward(-1f); // 장애물 충돌
            }
        }
    }
    //가장 가까운 장애물 찾기
    private GameObject GetClosestObstacleOnPath()
    {
        GameObject closestObstacle = null; //closestObstacle 가장 가까운 장애물
        float closestDistance = float.MaxValue;
        //목표지점까지의 방향벡터
        Vector3 directionToTarget = (target.localPosition - transform.localPosition).normalized;

        foreach (var obstacle in obstacles)
        {
            //현재 에이전트에서 장애물까지의 방향 벡터
            Vector3 directionToObstacle = (obstacle.transform.localPosition - transform.localPosition).normalized;

            // 목표 지점과 플레이어 에이전트의 방향과 장애물까지의 방향이 비슷한지, 1에 가까울 수록 같은 방향임
            if (Vector3.Dot(directionToTarget, directionToObstacle) > 0.9f) 
            {
                //에이전트 - 장애물 거리계산
                float distanceToObstacle = Vector3.Distance(transform.localPosition, obstacle.transform.localPosition);
                if (distanceToObstacle < closestDistance)
                {
                    //가장 가까운 장애물 거리 업데이트
                    closestDistance = distanceToObstacle;
                    closestObstacle = obstacle;
                }
            }
        }

        return closestObstacle;
    }
    /*
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
    */
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 객체가 장애물이라면
        if (collision.gameObject.CompareTag("DieZone")) // 다이존일경우
        {
            SetReward(-1.5f); // 충돌 시 리워드 감소 및 에피소드 종료
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("rewardzone")) //리워드존일경우
        {
            SetReward(0.5f); //중간마다 리워드 증가
        }
    }
}
