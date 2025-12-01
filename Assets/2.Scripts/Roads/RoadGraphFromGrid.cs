using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// GridHandler의 논리타입(TileType)을 읽어와 
/// <para> Road 셀마다 Waypoint를 자동 생성/삭제하고, 상하좌우로 이웃을 연결한다.</para>
/// <para> RoadSystem이 도로를 설치/제거할 때 이벤트를 쏘면, 여기서 "지연(디바운스) 리빌드'로 반영.</para>
/// Waypoint 프리팹 없이, 빈 GameObject + Waypoint 컴포넌트만 붙여서 생성해도 충분.
/// </summary>
public class RoadGraphFromGrid : MonoBehaviour
{
    [Header("Essential reference")]
    [SerializeField] private GridHandler gridHandler;   // GridHandler 참조 - 도로 /필드 타입 정보 및 좌표 변환 제공
    [SerializeField] private Transform waypointRoot;    // 생성되는 Waypoint들의 부모 (정리용 empty)

    [Header("Rebuild Settings")]
    [Tooltip("변경 이벤트 수신 후 얼마나 기다렸다 리빌드할지(연속편집 안정화)")]
    [SerializeField] private float rebuildDelay = 0.2f; // RoadSystem 변경 이벤트 수신 후 지연 시간(디바운스)

    // 내부 데이터: 격자좌표(Vector2Int) -> 웨이포인트(Waypoint) 매핑
    private readonly Dictionary<Vector2Int, Waypoint> nodes = new();

    // 디바운스용 타이머
    private float rebuildTimer = -1f;
    private bool pendingRebuild = false;

    // 상하좌우 이웃 방향 정의 (4방향)
    private static readonly Vector2Int[] dirs =
    {
        new Vector2Int( 1,0), // Right : x+1
        new Vector2Int(-1,0), // Left  : x-1
        new Vector2Int(0,-1), // Down  : y-1
        new Vector2Int(0, 1), // Up    : y+1
    }; // <- 배열 초기화 문장의 끝

    private void OnEnable()
    {
        // RoadSystem이 쏘는 "변경됨" 이벤트를 구독
        RoadSystem.RoadsChanged += OnRoadsChanged;
    }

    private void OnDisable()
    {
        // 씬 해제/비활성화 시 반드시 구독 해제(메모리/콜백 누수 방지)
        RoadSystem.RoadsChanged -= OnRoadsChanged;
    }

    void Update()
    {
        // 디바운스 : 변경이 잦을 때 매번 리빌드 대신, rebuildDelay 지난 뒤 1회 RebuildAll
        if (pendingRebuild)
        {
            rebuildTimer -= Time.deltaTime;
            if (rebuildTimer <= 0f)
            {
                pendingRebuild = false;
                RebuildAll(); // 전체 스캔 리빌드(간단/안정). 필요 시 "부분 리빌드"로 최적화 가능.
            }
        }
    }

    // Road 변경 이벤트 도착 -> "잠깐 기다렸다가 한 번에 리빌드하자"
    private void OnRoadsChanged()
    {
        pendingRebuild = true;
        rebuildTimer = rebuildDelay;
    }

    /// <summary>
    /// 전체 맵을 스캔하여 Road 셀만 Waypoint로 동기화하고
    /// 상하좌우 이웃 연결 구성.
    /// </summary>
    [ContextMenu("RebuildAll")]
    public void RebuildAll()
    {
        var desired = CollectDesiredRoadCells();    // 1) (수집) 현재 Road 셀 좌표 수집
        RemoveObsoleteNodes(desired);                // 2) (삭제) 기존 캐시 중 더는 Road가 아닌 노드 삭제
        CreateMissingNodes(desired);                // 3) (생성) 새로 Road가 된 좌표에 waypoint 생성
        RebuildNeighbors();                         // 4) (연결) 모든 노드의 상하좌우 이웃 연결 재구성

        // 1) 수집 : 타일맵 전체를 스캔하여 "Road"인 좌표만 모은다.
        HashSet<Vector2Int> CollectDesiredRoadCells()
        {
            var set = new HashSet<Vector2Int>();

            // 가독성을 위해 지역 변수로 캐시
            int w = gridHandler.Width;
            int h = gridHandler.Height;

            // 그리드는 중앙 원점 기준(-w/2 ~ w/2 - 1, -h/2 ~ h/2 - 1)
            for (int y = -h / 2; y < h / 2; y++)
            {
                for (int x = -w / 2; x < w / 2; x++)
                {
                    // 논리 타입이 Road인 셀만 대상
                    if (gridHandler.GetGridTileType(x, y) == TileType.Road)
                        set.Add(new Vector2Int(x, y));
                }
            }

            return set;
        }

        // 2) 삭제 : 캐시에 남아 있으나 더는 Road가 아닌 좌표의 WayPoint를 제거
        void RemoveObsoleteNodes(HashSet<Vector2Int> desiredSet)
        {
            // 딕셔너리를 순회하며 삭제 리스트를 별도로 만들어 한 번에 처리(반복 중 변경 방지)
            var toRemove = new List<Vector2Int>();
            foreach (var key in nodes.Keys)
            {
                if (!desiredSet.Contains(key))
                    toRemove.Add(key);
            }

            // 실제 제거(하이어라키 오브젝트 + 캐시)
            foreach (var key in toRemove)
            {
                if (nodes.TryGetValue(key, out var wp) && wp != null)
                    Destroy(wp.gameObject); // Waypoint 게임오브젝트 삭제

                nodes.Remove(key);              // 캐시에서 제거
            }
        }

        // 3) 생성 : 새롭게 Road가 된 좌표에는 Waypoint를 만든 뒤 캐시에 등록
        void CreateMissingNodes(HashSet<Vector2Int> desiredSet)
        {
            foreach (var key in desiredSet)
            {
                if (nodes.ContainsKey(key))
                    continue; // 이미 있으면 skip

                // 셀 중심 월드 좌표로 Waypoint 배치
                Vector3 world = gridHandler.CellToWorld(key.x, key.y);

                var go = new GameObject($"WP_{key.x}_{key.y}");
                go.transform.SetParent(waypointRoot, worldPositionStays: true); // 월드 포지션 유지
                go.transform.position = world;

                nodes[key] = go.AddComponent<Waypoint>(); // 컴포넌트 붙이기, go의 좌표도 등록됨
            }
        }

        // 4) 연결 : 모든 노드의 neighbors를 초기화하고 상하좌우 이웃을 연결
        void RebuildNeighbors()
        {
            // [1] 모든 노드의 이웃 목록을 초기화
            // - 리빌드 시점마다 이전 연결 상태를 싹 지우고, 현재 그리드 상태 기준으로 다시 연결한다.
            foreach (var kv in nodes)
            {
                Waypoint wp = kv.Value;
                if (wp != null)
                    wp.ClearNeighbors(); // Waypoint 클래스에 이미 준비해둔 초기화 함수
            }

            // [2] 각 노드에 대해 4방향(dirs)으로 이웃 후보를 확인하고, 있으면 양방향으로 연결
            // - 키(격자좌표) + 방향벡터 = 이웃좌표
            // - nodes.TryGetValue로 해당 좌표에 Waypoint가 실제로 있는지 확인
            // - AddNeighbor는 (자기참조/중복) 방어가 되어 있으므로 그대로 호출해도 안전
            foreach (var kv in nodes)
            {
                Vector2Int key = kv.Key;    // 현재 노드의 격자 좌표 (예: (x, y))
                Waypoint wp = kv.Value;     // 현재 노드의 Waypoint 컴포넌트

                if (wp == null)
                    continue; // 안전장치: 혹시라도 null이면 스킵

                // 상하좌우 4방향을 순회하며 이웃을 찾는다.
                for (int i = 0; i < dirs.Length; i++)
                {
                    Vector2Int dir = dirs[i];           // 예: (1,0), (-1,0), (0,-1), (0,1)
                    Vector2Int neighborKey = key + dir; // 이웃 셀의 격자 좌표

                    // 해당 좌표에 Waypoint가 존재하면 연결
                    if (nodes.TryGetValue(neighborKey, out Waypoint neighbor) && neighbor != null)
                    {
                        // 단방향으로만 연결해도, 전체 순회 과정에서 반대 방향에서 다시 연결되지만
                        // 명시적으로 "양방향" 연결을 보장하면 직관적이고 안전하다.
                        wp.AddNeighbor(neighbor);   // 현재 -> 이웃
                        neighbor.AddNeighbor(wp);   // 이웃 -> 현재
                    }
                }
            }
        }
    }

    /// <summary>
    /// 월드 좌표 기준 가장 가까운 Waypoint 찾기 (입구/스폰지점 스냅용)
    /// </summary>
    public Waypoint FindNearestWaypoint(Vector3 worldPos)
    {
        Waypoint best = null;
        float bestSqr = float.MaxValue;

        foreach (var kv in nodes)
        {
            var wp = kv.Value;
            float sqr = (wp.transform.position - worldPos).sqrMagnitude; // 제곱 연산
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = wp;
            }
        }
        return best;
    }

    /// <summary>
    /// 모든 웨이포인트 나열 (랜덤 목적지 선택 등 유틸)
    /// </summary>
    public IEnumerable<Waypoint> AllWayPoints()
    {
        return nodes.Values;
    }
}
