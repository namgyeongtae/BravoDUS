using System.Collections;
using UnityEngine;

public class NpcPathWalker : MonoBehaviour
{
    public Transform entrance;

    public Transform startWaypoint;

    public float moveSpeed = 4f;

    private Waypoint currentWP; // 현재 웨이포인트
    private Waypoint nextWP;    // 다음 이동할 웨이포인트
    private Waypoint previousWP;// 이전 웨이포인트

    private void Start()
    {
        // 출입구 위치에서 시작
        transform.position = entrance.position;

        // 처음 출발할 웨이포인트 지정
        currentWP = entrance.GetComponent<Waypoint>();

        // 처음 이동할 웨이포인트 지정
        nextWP = startWaypoint.GetComponent<Waypoint>();
    }

    private void Update()
    {
        MoveToWaypoint();
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

            if (currentWP.neighbors.Length > 0)
            {
                Waypoint candidate; // 후보자
                int safetyCounter = 0; // 무한루프 방지

                do
                {
                    // 이웃 중 랜덤 선택
                    candidate = currentWP.neighbors[Random.Range(0, currentWP.neighbors.Length)];
                    safetyCounter++;
                } while (candidate == previousWP && safetyCounter < 50);

                nextWP = candidate;
            }
        }
    }
}
