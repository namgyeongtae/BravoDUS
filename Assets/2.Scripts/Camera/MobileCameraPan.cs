using UnityEngine;
using UnityEngine.InputSystem;   // New Input System (마우스 휠)
using UITouch = UnityEngine.Touch;
using UITouchPhase = UnityEngine.TouchPhase;

[RequireComponent(typeof(Camera))]
public class MobileCameraPan : MonoBehaviour
{
    [Header("Pan")]
    [SerializeField] float panSpeed = 1f;                 // 손가락 이동 감도
    [SerializeField] Vector3 boundsPadding = new Vector3(0, 0, 0);

    [Header("Clamp (Min/Max)")] // X/Z 이동 한계 (Min, Max)
    [SerializeField] Vector2 clampX = new Vector2(-74f, -24f);
    [SerializeField] Vector2 clampZ = new Vector2(-74f, -24f);

    [Header("Camera Fixed Settings")]
    [SerializeField] float fixedY = 43f;                  // 항상 유지할 높이
    [SerializeField] Vector3 fixedRotation = new Vector3(30f, 45f, 0f); // 항상 유지할 각도

    [Header("Camera Zoom (Ortho)")]
    [SerializeField] float zoomSpeed = 0.05f;   // 핀치/휠/키보드 감도 기본값
    [SerializeField] float minZoom = 10f;       // 최소 줌 사이즈
    [SerializeField] float maxZoom = 30f;       // 최대 줌 사이즈

    private Camera cam;
    private Plane ground;                                 // y=0 평면
    private Vector3 lastHit;
    private bool dragging;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ground = new Plane(Vector3.up, Vector3.zero);

        // 시작 시 고정 각도/높이 적용
        transform.rotation = Quaternion.Euler(fixedRotation);
        var p = transform.position; p.y = fixedY; transform.position = p;
    }

    void Update()
    {
        HandlePan();
        HandleZoomPinch();       // 모바일 핀치
        HandleWheelZoom();       // 가능하면 휠
        HandleEditorFallbackZoom(); // Device Simulator 등 휠 막힐 때 대체 입력
    }

    // -------------------------
    // 팬 (터치 1손가락 / 에디터 마우스)
    // -------------------------
    void HandlePan()
    {
#if UNITY_EDITOR
        if (UIUtils.IsPointerOverUIObject(Input.mousePosition)) return;

        if (Input.GetMouseButtonDown(0)) dragging = TryScreenToGround(Input.mousePosition, out lastHit);
        else if (Input.GetMouseButton(0) && dragging) PanTo(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) dragging = false;
#endif

        if (Input.touchCount == 1)
        {
            UITouch t = Input.GetTouch(0);

            if (UIUtils.IsPointerOverUIObject(t.position)) return;

            if (t.phase == UITouchPhase.Began) dragging = TryScreenToGround(t.position, out lastHit);
            else if (t.phase == UITouchPhase.Moved && dragging) PanTo(t.position);
            else if (t.phase == UITouchPhase.Ended || t.phase == UITouchPhase.Canceled) dragging = false;
        }
    }

    // -------------------------
    // 핀치 줌(모바일)
    // -------------------------
    void HandleZoomPinch()
    {
        if (Input.touchCount != 2) return;

        UITouch t0 = Input.GetTouch(0);
        UITouch t1 = Input.GetTouch(1);

        if (UIUtils.IsPointerOverUIObject(t0.position)) return;
        if (UIUtils.IsPointerOverUIObject(t1.position)) return;

        Vector2 prev0 = t0.position - t0.deltaPosition;
        Vector2 prev1 = t1.position - t1.deltaPosition;

        float prevDist = (prev0 - prev1).magnitude;
        float currDist = (t0.position - t1.position).magnitude;
        float diff = currDist - prevDist;

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - diff * zoomSpeed,
            minZoom, maxZoom
        );
    }

    // -------------------------
    // 휠 줌(가능하면 두 시스템 다 시도)
    // -------------------------
    void HandleWheelZoom()
    {
        float wheel = 0f;

        // New Input System
        try
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                // 환경마다 단위가 커서 스케일링
                wheel += mouse.scroll.ReadValue().y * 0.01f;
            }
        }
        catch { /* 패키지 미설치 시 무시 */ }

        // Old Input Manager도 동시에 시도
        wheel += Input.GetAxis("Mouse ScrollWheel") * 5f;

        if (Mathf.Abs(wheel) > 0.001f)
        {
            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - wheel * zoomSpeed * 10f,
                minZoom, maxZoom
            );
        }
    }

    // -------------------------
    // 에디터/시뮬레이터 대체 줌(키보드/가운데버튼 드래그)
    // -------------------------
    void HandleEditorFallbackZoom()
    {
#if UNITY_EDITOR
        float step = zoomSpeed * 5f;

        // + / - / Keypad +/-
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - step, minZoom, maxZoom);
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + step, minZoom, maxZoom);

        // Q/E 또는 PageUp/PageDown
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.PageUp))
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - step, minZoom, maxZoom);
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.PageDown))
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + step, minZoom, maxZoom);

        // 가운데 버튼 눌러 위/아래 드래그로 줌
        if (Input.GetMouseButton(2))
        {
            float dy = -Input.GetAxis("Mouse Y"); // 위로 밀면 줌 인
            if (Mathf.Abs(dy) > 0.001f)
            {
                cam.orthographicSize = Mathf.Clamp(
                    cam.orthographicSize - dy * step,
                    minZoom, maxZoom
                );
            }
        }
#endif
    }

    // -------------------------
    // 실제 팬 이동
    // -------------------------
    void PanTo(Vector2 screenPos)
    {
        if (!TryScreenToGround(screenPos, out var hitNow)) return;

        Vector3 move = (lastHit - hitNow) * panSpeed;
        Vector3 next = transform.position + move;

        // X/Z만 이동, Y/Rotation 고정
        next.y = fixedY;
        transform.position = next;
        transform.rotation = Quaternion.Euler(fixedRotation);

        ClampWithinMinMax();
        lastHit = hitNow;
    }

    // -------------------------
    // 스크린 → 바닥 평면 좌표
    // -------------------------
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

    // -------------------------
    // 클램프 (줌에 따른 자동 보정)
    // -------------------------
    void ClampWithinMinMax()
    {
        Vector3 pos = transform.position;

        if (cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            // 화면 전체가 바깥으로 나가지 않도록 반폭/반높이 반영
            pos.x = Mathf.Clamp(pos.x,
                clampX.x + boundsPadding.x ,
                clampX.y - boundsPadding.x );

            pos.z = Mathf.Clamp(pos.z,
                clampZ.x + boundsPadding.z ,
                clampZ.y - boundsPadding.z );
        }
        else
        {
            pos.x = Mathf.Clamp(pos.x, clampX.x + boundsPadding.x, clampX.y - boundsPadding.x);
            pos.z = Mathf.Clamp(pos.z, clampZ.x + boundsPadding.z, clampZ.y - boundsPadding.z);
        }

        pos.y = fixedY; // 고정 높이
        transform.position = pos;
    }
}
