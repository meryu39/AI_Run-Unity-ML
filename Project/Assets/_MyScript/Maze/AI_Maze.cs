using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class AI_Maze : Agent
{
    public Transform target; // 목표 지점
    public float moveSpeed = 7f;
    public float maxEpisodeTime = 45.0f; // 에피소드 최대 시간
    private float currentEpisodeTime;
    //초기화 지점
    public Vector3 startPosition;
    public Vector3 targetPosition;
    //타일 레이캐스트
    public RaycastHit tileHit;
    public LayerMask TileLayerMask;
    //방문한 적 있는 타일
    private HashSet<GameObject> visitedTiles = new HashSet<GameObject>();
    int duplicateVisitCount = 0;
    public override void OnEpisodeBegin()
    {
        // 에피소드 초기화
        transform.localPosition = startPosition;
        target.localPosition = targetPosition;
        currentEpisodeTime = 0f; // 에피소드 타이머 초기화
        visitedTiles.Clear();
    }
    private void Start()
    {
        TileLayerMask = LayerMask.GetMask("tile");
    }

    private void Update()
    {
        // 시간 초과 체크
        currentEpisodeTime += Time.deltaTime;
        if (currentEpisodeTime >= maxEpisodeTime)
        {
            //AddReward(-1f); // 시간 초과 페널티
            EndEpisode();
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        //에이전트 위치
        sensor.AddObservation(transform.localPosition);

        //목표지점 - 플레이어 사이 가장 가까운 장애물(플레이어 입장)
        float normalizedTime = currentEpisodeTime / maxEpisodeTime;
        sensor.AddObservation(normalizedTime); // 남은 시간 비율
        Physics.Raycast(transform.position + new Vector3(0, -0.05f, 0), -Vector3.up, out tileHit, 1f, TileLayerMask);
        if (tileHit.collider != null && tileHit.collider.CompareTag("tile"))
        {
            sensor.AddObservation(tileHit.collider.GetComponent<TileNumber>().tileNumber);
        }
        else
        {
            sensor.AddObservation(-1);
        }
            
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 행동 수집
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]).normalized;
        Vector3 Nextpos = move * moveSpeed * Time.deltaTime;
        //rb.MovePosition(Nextpos);
        rb.velocity = Nextpos + new Vector3(0, -9.8f, 0);

        AddReward(-0.0003f);
        
        Physics.Raycast(transform.position + new Vector3(0, -0.05f, 0), -Vector3.up, out tileHit, 1f, TileLayerMask);
        //Debug.Log("액션 수행!");
        if (tileHit.collider != null && tileHit.collider.CompareTag("tile"))
        {
            GameObject nowTile = tileHit.collider.gameObject;
            if(visitedTiles.Contains(nowTile))
            {
                //Debug.Log("이미 방문한 타일");
                duplicateVisitCount++;
                if(duplicateVisitCount >= 5000)
                {
                    //AddReward(-100f);
                    //EndEpisode();
                }
            }
            else
            {
                Debug.Log("새로운 타일!" + visitedTiles.Count + ", " + tileHit.collider.GetComponent<TileNumber>().tileNumber);
                visitedTiles.Add(nowTile);
                AddReward(2f);
                duplicateVisitCount = 0;

                int wallCount = 0;
                RaycastHit wallHit;
                LayerMask WallLayerMask = LayerMask.GetMask("wall");
                Physics.Raycast(tileHit.collider.transform.position + new Vector3(0, 1, 0), Vector3.forward, out wallHit, 1.5f, WallLayerMask);
                if(wallHit.collider != null && wallHit.collider.CompareTag("wall"))
                {
                    //Debug.Log("새로운 타일에서 +z쪽에 벽 확인");
                    wallCount++;
                }
                Physics.Raycast(tileHit.collider.transform.position + new Vector3(0, 1, 0), Vector3.back, out wallHit, 1.5f, WallLayerMask);
                if (wallHit.collider != null && wallHit.collider.CompareTag("wall"))
                {
                    //Debug.Log("새로운 타일에서 -z쪽에 벽 확인");
                    wallCount++;
                }
                Physics.Raycast(tileHit.collider.transform.position + new Vector3(0, 1, 0), Vector3.right, out wallHit, 1.5f, WallLayerMask);
                if (wallHit.collider != null && wallHit.collider.CompareTag("wall"))
                {
                    //Debug.Log("새로운 타일에서 +x쪽에 벽 확인");
                    wallCount++;
                }
                Physics.Raycast(tileHit.collider.transform.position + new Vector3(0, 1, 0), Vector3.left, out wallHit, 1.5f, WallLayerMask);
                if (wallHit.collider != null && wallHit.collider.CompareTag("wall"))
                {
                    //Debug.Log("새로운 타일에서 -x쪽에 벽 확인");
                    wallCount++;
                }
                if(wallCount >= 3)
                {
                    Debug.Log("막다른 길!!!");
                    AddReward(-10f);
                }
                if(wallCount <= 1)
                {
                    //Debug.Log("갈림길!!!!");
                   // AddReward(3f);
                }
            }
        }
    }
    //가장 가까운 장애물 찾기

    public override void Heuristic(in ActionBuffers actionsOut)
    {

        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DieZone")) // 다이존일경우
        {
            SetReward(-100.0f); // 충돌 시 리워드 감소 및 에피소드 종료
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("goal"))
        {
            Debug.Log("도착!!!!!!");
            float timeBonus = Mathf.Max(0, (maxEpisodeTime - currentEpisodeTime)); //남은 시간 계산
            SetReward(100f + 0.2f * timeBonus); // 보상 + 시간
            EndEpisode();
        }
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("wall"))
        {
            // 패널티 부여
            AddReward(-0.05f);

            // 로그 출력 (디버깅용)
            Debug.Log("벽에 닿았습니다! 패널티 적용: ");
        }
    }*/

    /*private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("wall"))
        {
            // 패널티 부여
            AddReward(-0.002f);

            // 로그 출력 (디버깅용)
            Debug.Log("벽에 닿았습니다! 패널티 지속 적용: ");
        }
    }*/


}
