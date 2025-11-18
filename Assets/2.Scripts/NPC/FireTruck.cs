using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FireTruck : MonoBehaviour
{
    private Building _targetBuilding = null;

    private Vector3Int _startCell = Vector3Int.zero;
    private Vector3Int _destinationCell = Vector3Int.zero;

    private List<Vector3Int> _path = new List<Vector3Int>();
    private Coroutine _moveCoroutine = null;
    private bool _isMoving = false;

    [SerializeField] private float _moveSpeed = 5f;

    public Building TargetBuilding => _targetBuilding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDestination(Vector3Int startCell, Building target)
    {
        int[] dirX = {0, 1, 0, -1};
        int[] dirY = {-1, 0, 1, 0};
        
        List<Vector3Int> cells = Managers.Construct.GridHandler.GetCellsInRange(target.transform.position, target.BuildingSize);

        bool isFound = false;
        Vector3Int destinationCell = Vector3Int.zero;

        foreach (var cell in cells)
        {
            for (int i = 0; i < 4; i++)
            {
                int nextX = cell.x + dirX[i];
                int nextY = cell.y + dirY[i];

                TileType tileType = Managers.Construct.GridHandler.GetGridTileType(nextX, nextY);
                if (tileType == TileType.Field || tileType == TileType.Road)
                {
                    isFound = true;
                    _destinationCell = new Vector3Int(nextX, nextY, 0);
                    _targetBuilding = target;
                    break;
                }
            }

            if (isFound) break;
        }

        _startCell = startCell;

        if (!isFound) 
        {
            Managers.UI.OpenToastPopup("소방차 목적지를 설정할 수 없습니다.");
        }
        else
        {
            FindPath();
        }
    }

    private void FindPath()
    {
        _path = AStar.AStarPathFinding(_startCell, _destinationCell);

        Dispatch();
    }

    private void Dispatch()
    {
        if (_path.Count <= 0)
        {
            Managers.UI.OpenToastPopup("소방차 경로를 찾을 수 없습니다.");
            return;
        }

        // 이미 이동 중이면 중지하고 새 경로로 시작
        if (_isMoving && _moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        _moveCoroutine = StartCoroutine(MoveAlongPath());
    }

    public void Return()
    {
        if (_path.Count <= 0)
        {
            Managers.UI.OpenToastPopup("소방차 경로를 찾을 수 없습니다.");
            return;
        }

        // 이미 이동 중이면 중지하고 새 경로로 시작
        if (_isMoving && _moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        _moveCoroutine = StartCoroutine(ReturnAlongPath());
    }

    private IEnumerator MoveAlongPath()
    {
        _isMoving = true;

        foreach (var cell in _path)
        {
            Vector3 targetPosition = Managers.Construct.GridHandler.CellToWorld(cell.x, cell.y);
            targetPosition.y = transform.position.y; // Y 좌표는 유지

            // 현재 셀에 도달할 때까지 이동
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            // 정확한 위치로 설정
            transform.position = targetPosition;
        }

        _isMoving = false;
        _moveCoroutine = null;

        // 목적지 도달 후 처리
        OnReachedDestination();
    }

    private IEnumerator ReturnAlongPath()
    {
        _isMoving = true;

        for (int i = _path.Count - 1; i >= 0; i--)
        {
            Vector3Int cell = _path[i];
            Vector3 targetPosition = Managers.Construct.GridHandler.CellToWorld(cell.x, cell.y);
            targetPosition.y = transform.position.y;

            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPosition;
        }

        _isMoving = false;
        _moveCoroutine = null;

        OnReturnedToFireStation();
    }

    private void OnReachedDestination()
    {
        if (_targetBuilding == null)
        {
            Debug.LogError("FireTruck: Target building is null");
            return;
        }

        var inc = Managers.Event.Fire.IncidentBuildings[_targetBuilding];

        if (inc == null)
        {
            Debug.LogError("FireTruck: Incident not found");
            return;
        }

        inc.State = IncidentState.Resolving;

        // 경고 UI 제거
        var uiWarning = Managers.Event.Fire.IncidentUIWarnings[inc];
        Managers.UI.RemovePanel(uiWarning);
        Managers.Event.Fire.IncidentUIWarnings.Remove(inc);

        // 진압 중 UI 추가
        Managers.UI.AddPanel<UIFireResolve>(this, true);
    }

    private void OnReturnedToFireStation()
    {
        var inc = Managers.Event.Fire.IncidentBuildings[_targetBuilding];

        if (Managers.Event.Fire.ResolvingWorkForces.ContainsKey(inc))
        {
            Managers.Event.Fire.ResolvingWorkForces.Remove(inc);
        }

        Managers.Resource.Destroy(this.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(Managers.Construct.GridHandler.CellToWorld(_startCell.x, _startCell.y), 1f);

        for (int i = 0; i < _path.Count; i++)
        {
            Gizmos.DrawSphere(Managers.Construct.GridHandler.CellToWorld(_path[i].x, _path[i].y), 1f);
        }
    }
}
