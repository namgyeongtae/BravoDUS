using UnityEngine;

public class MobileCameraPan : MonoBehaviour
{
    [SerializeField] private float panSpeed = 30f; // 50에서 낮춤, 대각선 보정 후 조정
    private Bounds groundBounds;
    [SerializeField] private Vector3 boundsPadding = new Vector3(5f, 0f, 5f);

    private Camera cam;
    private Vector2 lastTouchPosition;

    void Start()
    {
        cam = GetComponent<Camera>();
        groundBounds = new Bounds(new Vector3(-6f, 0f, -6f), new Vector3(88f, 0f, 88f)); 
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                Vector3 worldDelta = cam.ScreenToWorldPoint(new Vector3(delta.x, delta.y, cam.nearClipPlane))
                                    - cam.ScreenToWorldPoint(Vector3.zero);
                worldDelta.y = 0f;

                // +Z factor 0.5 유지 (위 느리게)
                if (worldDelta.z > 0) worldDelta.z *= 0.5f;

                // 대각선 속도 보정: normalize 후 magnitude * sqrt(2) 곱 (대각선 느린 거 fix)
                float magnitude = worldDelta.magnitude;
                if (magnitude > 0f) // zero 방지
                {
                    worldDelta = worldDelta.normalized * magnitude * 2.814f; // sqrt(2) factor
                }

                transform.position -= worldDelta * panSpeed * Time.deltaTime;

                Vector3 clampedPos = transform.position;
                clampedPos.x = Mathf.Clamp(clampedPos.x, groundBounds.min.x + boundsPadding.x, groundBounds.max.x - boundsPadding.x);
                clampedPos.z = Mathf.Clamp(clampedPos.z, groundBounds.min.z + boundsPadding.z, groundBounds.max.z - boundsPadding.z);
                transform.position = clampedPos;

                lastTouchPosition = touch.position;
            }
        }
    }
}