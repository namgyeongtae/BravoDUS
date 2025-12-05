using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// RoadGraphFromGrid가 만든 '도로 전용 Waypoint 그래프'만 따라 이동하는 NPC.
/// <para> Entrance(Transform) 근처 가장 가까운 웨이포인트에서 시작.</para>
/// <para> current -> neighbors 중 랜덤 선택(이전 노드 복귀 편향 최소화).</para>
/// 도로 타일 편집 상태에 따라 자동 갱신된 그래프 위에서만 이동.
/// </summary>
public class NpcPathWalker : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private RoadGraphFromGrid roadGraph;
    [SerializeField] private Transform entrance; // 출입구

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float arriveThreshold = 0.08f; // 도착임계값

    private Waypoint currentWP; // 현재 노드
    private Waypoint nextWP;    // 다음 노드
    private Waypoint previousWP;// 직전 노드

    private void Awake()
    {
        entrance = GameObject.FindWithTag("Entrance").transform;
        roadGraph = FindAnyObjectByType<RoadGraphFromGrid>();
    }

    private void Start()
    {
        // 1) 시작 노드 결정: entrance 근처 가장 가까운 waypoint
        currentWP = roadGraph.FindNearestWaypoint(entrance.position);

        // 2) NPC를 시작 노드 위치로 스냅
        transform.position = currentWP.transform.position;

        // 3) 첫 다음 노드 선택(이전 노드 제외 없음)
        nextWP = PickNext(currentWP, exclude: null);
    }

    private void Update()
    {
        if (currentWP == null || nextWP == null) return;

        // 이동 처리(목표까지 직선 이동)
        Vector3 target = nextWP.transform.position;
        Vector3 dir = target - transform.position;
        float dist = dir.magnitude;

        if (dist > arriveThreshold)
        {
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            // 도착 -> 상태 갱신 -> 다음 노드 고르기
            previousWP = currentWP;
            currentWP = nextWP;
            nextWP = PickNext(currentWP, exclude: previousWP); // 직전 노드는 우선 제외해 왕복 줄이기
        }
    }

    // 인접 웨이포인트에서만 랜덤 선택하여 이동
    void MoveToWaypoint()
    {
        if (nextWP == null) return;

        // NPC 이동
        transform.position = Vector3.MoveTowards(transform.position, nextWP.transform.position, moveSpeed * Time.deltaTime);

        // 웨이포인트 도착 시 다음 웨이포인트 선택
        if (Vector3.Distance(transform.position, nextWP.transform.position) < 0.1f)
        {
            previousWP = currentWP;
            currentWP = nextWP;

            if (currentWP.neighbors.Count > 0)
            {
                Waypoint candidate; // 후보자
                int safetyCounter = 0; // 무한루프 방지

                do
                {
                    // 이웃 중 랜덤 선택
                    candidate = currentWP.neighbors[Random.Range(0, currentWP.neighbors.Count)];
                    safetyCounter++;
                } while (candidate == previousWP && safetyCounter < 50);

                nextWP = candidate;
            }
        }
    }

    private Waypoint PickNext(Waypoint node, Waypoint exclude)
    {
        if (node == null || node.neighbors == null || node.neighbors.Count == 0) return null;

        // 1) 제외 대상(exclude)을 빼고, null이 아닌 이웃만 후보로
        var candidates = node.neighbors.Where(n => n != null && n != exclude).ToList();

        // 2) 만약 후보가 0개면 (= 막다른 길이면), 되돌아가기 허용 : 전체 이웃 중 null이 아닌 것만 다시 후보로
        if (candidates.Count == 0)
            candidates = node.neighbors.Where(n => n != null).ToList();

        // 3) 그래도 후보가 없으면 (갈곳이 아예 없는 경우) null 반환 (이동 불가)
        if (candidates.Count == 0) return null;

        // 균등 랜덤
        return candidates[Random.Range(0, candidates.Count)];
    }
}
