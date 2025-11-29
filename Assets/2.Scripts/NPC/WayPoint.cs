using UnityEngine;
using System.Collections.Generic;

// 도로 셀 1칸 = waypoint 1개.
// 인접(상하좌우) 웨이포인트를 neighbors에 보관.
public class Waypoint : MonoBehaviour
{
    private void Start()
    {
        transform.position += new Vector3(0, 0.45f, 0);
    }

    // 인스펙터에서 확인하기 쉽게 public 리스트로 둔다.
    public List<Waypoint> neighbors = new();

    // 이웃 초기화(연결 재구성 전에 호출)
    public void ClearNeighbors() => neighbors.Clear();

    // 이웃 추가(자기 자신/중복 방지)
    public void AddNeighbor(Waypoint wp)
    {
        if (wp == null || wp == this || neighbors.Contains(wp)) return;
        neighbors.Add(wp);
    }
}
