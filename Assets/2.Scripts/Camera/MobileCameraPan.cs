using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MobileCameraPan : MonoBehaviour
{
    [Header("Pan")]
    [SerializeField] float panSpeed = 1f;                 // 손가락 이동량 스케일
    [SerializeField] Vector3 boundsPadding = new Vector3(0, 0, 0);

    // 원하는 이동 한계(월드 좌표). 예) (-74, -24)
    [Header("Clamp (Min/Max)")]
    [SerializeField] Vector2 clampX = new Vector2(-74f, -24f);
    [SerializeField] Vector2 clampZ = new Vector2(-74f, -24f);

    [Header("Camera Fixed Settings")]
    [SerializeField] float fixedY = 43f;                  // 항상 유지할 높이
    [SerializeField] Vector3 fixedRotation = new Vector3(30f, 45f, 0f); // 항상 유지할 각도

    private Camera cam;
    private Plane ground;                                 // y=0 평면
    private Vector3 lastHit;
    private bool dragging;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ground = new Plane(Vector3.up, Vector3.zero);

        // 시작 시 고정 각도 적용
        transform.rotation = Quaternion.Euler(fixedRotation);
        // 시작 시 고정 높이 적용
        var p = transform.position; p.y = fixedY; transform.position = p;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) dragging = TryScreenToGround(Input.mousePosition, out lastHit);
        else if (Input.GetMouseButton(0) && dragging) PanTo(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) dragging = false;
#endif
        if (Input.touchCount == 1)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) dragging = TryScreenToGround(t.position, out lastHit);
            else if (t.phase == TouchPhase.Moved && dragging) PanTo(t.position);
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) dragging = false;
        }
    }

    void PanTo(Vector2 screenPos)
    {
        if (!TryScreenToGround(screenPos, out var hitNow)) return;

        // 손가락이 가리킨 바닥 좌표의 실제 차이만큼 반대로 이동(감각 고정)
        Vector3 move = (lastHit - hitNow) * panSpeed;  // 드래그는 deltaTime 필요 없음

        Vector3 next = transform.position + move;

        // X/Z만 이동, Y/Rotation은 고정
        next.y = fixedY;
        transform.position = next;
        transform.rotation = Quaternion.Euler(fixedRotation);

        ClampWithinMinMax();    // 원하는 범위로 딱 제한
        lastHit = hitNow;
    }

    bool TryScreenToGround(Vector2 screenPos, out Vector3 worldOnGround)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (ground.Raycast(ray, out float enter))
        {
            worldOnGround = ray.GetPoint(enter);
            return true;
        }
        worldOnGround = default;
        return false;
    }

    void ClampWithinMinMax()
    {
        Vector3 pos = transform.position;

        if (cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            // 화면 전체가 영역 밖으로 나가지 않도록 여유(halfW/halfH) 반영
            pos.x = Mathf.Clamp(pos.x,
                clampX.x + boundsPadding.x ,
                clampX.y - boundsPadding.x );

            pos.z = Mathf.Clamp(pos.z,
                clampZ.x + boundsPadding.z ,
                clampZ.y - boundsPadding.z );
        }
        else
        {
            // 퍼스펙티브라면 halfW/halfH 제외
            pos.x = Mathf.Clamp(pos.x, clampX.x + boundsPadding.x, clampX.y - boundsPadding.x);
            pos.z = Mathf.Clamp(pos.z, clampZ.x + boundsPadding.z, clampZ.y - boundsPadding.z);
        }

        pos.y = fixedY;                 // 고정 높이 유지
        transform.position = pos;
    }
}
