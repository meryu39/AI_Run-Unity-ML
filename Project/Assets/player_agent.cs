using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using UnityEngine;

public class player_agent : Agent
{
    public Transform target; // 목표 지점
    public GameObject[] obstacles; // 장애물 배열
    public float moveSpeed = 2f;
    public float observationRange = 10f; // 관측 범위

    public override void OnEpisodeBegin()
    {
        // 에피소드 초기화
        transform.localPosition = new Vector3(0, 0, -5);
        target.localPosition = new Vector3(Random.Range(-3, 3), 0, 5);

        foreach (var obstacle in obstacles)
        {
            obstacle.transform.localPosition = new Vector3(
                Random.Range(-5, 5),
                0,
                Random.Range(-3, 3)
            );
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 에이전트와 목표 지점 사이의 상대 위치
        sensor.AddObservation(target.localPosition - transform.localPosition);

        // 가장 가까운 N개의 장애물 관측
        foreach (var obstacle in GetClosestObstacles())
        {
            sensor.AddObservation(obstacle.transform.localPosition - transform.localPosition);
            sensor.AddObservation(obstacle.GetComponent<Rigidbody>().velocity);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 행동 수집
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        transform.localPosition += move * moveSpeed * Time.deltaTime;

        // 보상 설정
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
        AddReward(-0.01f); // 시간 페널티
        if (distanceToTarget < 1.5f)
        {
            SetReward(1f); // 목표 도달
            EndEpisode();
        }

        foreach (var obstacle in obstacles)
        {
            if (Vector3.Distance(transform.localPosition, obstacle.transform.localPosition) < 1.0f)
            {
                SetReward(-1f); // 장애물 충돌
                EndEpisode();
            }
        }
    }

    private GameObject[] GetClosestObstacles()
    {
        // 장애물 배열을 거리 기준으로 정렬 후 상위 N개 반환
        int numToObserve = 5; // 관측할 장애물 수
        return obstacles
            .OrderBy(o => Vector3.Distance(transform.localPosition, o.transform.localPosition))
            .Take(numToObserve)
            .ToArray();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 사람이 직접 조작하는 경우
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}
