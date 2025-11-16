using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Shuriken ParticleSystem에 'UI 어트랙션' 힘을 적용한다.
/// - target: 끌어당길 UI RectTransform
/// - strength/drag/killRadius 등 파라미터 제공
/// 좌표 변환 절차:
///   UI(RectTransform) → 화면(Screen) → UIParticle(RectTransform Local) → World → PS Local
/// </summary>
[DisallowMultipleComponent]
public class UIParticleAttractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<UIParticle> _uiParticles;                // 반드시 지정 (같은 Canvas 안)
    public RectTransform _target;                 // 빨려들 목표 대상 UI

    [Header("Force Settings")]
    public float _strength = 1600f;               // 가속도 크기 (픽셀/초^2 감각)
    public float _drag = 4.0f;                    // 속도 감쇠 계수
    public float _maxSpeed = 3000f;               // 속도 상한(픽셀/초 감각)
    public float _killRadius = 24f;               // 도착 판정 반경(UI 픽셀 단위)
    public bool _killOnArrive = true;             // 반경 내에 들어오면 제거

    [Header("Delay & Ramp")]
    [Tooltip("파티클이 생성되고 나서 이 시간만큼은 어트랙션이 적용되지 않음(초).")]
    public float startDelay = 0.15f;
    [Tooltip("지연에 ±무작위(초)를 추가하여 더 자연스러운 시작 타이밍 연출.")]
    public float delayJitter = 0.08f;
    [Tooltip("지연이 끝난 후 이 시간 동안 0→1로 가속도를 서서히 증가(초).")]
    public float rampDuration = 0.25f;
    [Tooltip("램프업 커브(0~1 구간). x=정규화된 진행도, y=강도 배율.")]
    public AnimationCurve rampCurve = AnimationCurve.EaseInOut(0,0, 1,1);

    [Header("Scaling")]
    [Tooltip("CanvasScaler의 scaleFactor를 보정에 곱해 속도/힘 감각을 일정하게 유지")]
    public bool useCanvasScaleCompensation = true;

    [Header("Attracted Event")]
    [Tooltip("파티클이 목표 UI에 도달했을 때 발생하는 이벤트.")]
    public UnityEvent OnAttracted;
    // 캐시
    // private ParticleSystem _ps;
    private Camera _uiCam;                       // Canvas가 Overlay면 null
    // private RectTransform _uiParticleRect;

    void Awake()
    {
        if (!_target) _target = GetComponent<RectTransform>();

        _uiCam = Camera.main;
         // 기본 커브 안전장치
        if (rampCurve == null || rampCurve.length == 0)
            rampCurve = AnimationCurve.EaseInOut(0,0, 1,1);
    }

    void LateUpdate()
    {
        foreach (var uiParticle in _uiParticles)
        {
            AttractParticles(uiParticle);
        }
    }

    private void AttractParticles(UIParticle uiParticle)
    {
        var proxy = uiParticle;
        if (!proxy || !proxy.canvas) return;

        // 소스별 좌표계/카메라 (Overlay면 null)
        RectTransform proxyRect = proxy.rectTransform;
        Camera uiCam = (proxy.canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : proxy.canvas.worldCamera;

        // 타겟의 "가시 중앙"을 스크린 → (소스의) proxy-local 로 변환
        Vector3[] corners = new Vector3[4];
        _target.GetWorldCorners(corners);
        Vector3 targetWorldCenter = (corners[0] + corners[2]) * 0.5f;

        Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(uiCam, targetWorldCenter);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(proxyRect, targetScreen, uiCam, out var targetProxyLocal))
            return;

        // Canvas 스케일 보정
        float scaleComp = 1f;
        if (useCanvasScaleCompensation && proxy.canvas)
        {
            var scaler = proxy.canvas.GetComponentInParent<CanvasScaler>();
            if (scaler) scaleComp = scaler.scaleFactor;
        }

        // 파티클 가져오기
        var ps = proxy.GetComponentInChildren<ParticleSystem>(true);
        if (!ps) return;

        var main = ps.main;
        bool simWorld = (main.simulationSpace == ParticleSystemSimulationSpace.World);

        int count = ps.particleCount;
        if (count == 0) return;

        ParticleSystem.Particle[] parts = new ParticleSystem.Particle[count];
        int alive = ps.GetParticles(parts);
        if (alive == 0) return;

        float dt = Time.unscaledDeltaTime; // UI 감각이라 unscaled 권장
        if (dt <= 0f) return;

        float kRadiusSqr = _killRadius * _killRadius;

        for (int i = 0; i < alive; i++)
        {
            // --- 지연 & 램프업 ---
            float startLife = parts[i].startLifetime;
            float age = Mathf.Max(0f, startLife - parts[i].remainingLifetime); // 생성 후 경과 시간
            float jitter = (HashToRangeMinusOneToOne(parts[i].randomSeed == 0 ? 0x9E3779B9u : parts[i].randomSeed) * 2f - 1f) * delayJitter; // [-jitter, +jitter]
            float delay = Mathf.Max(0f, startDelay + jitter);

            float rampT = 0f;
            if (age > delay)
                rampT = (rampDuration > 0f) ? Mathf.Clamp01((age - delay) / rampDuration) : 1f;

            float ramp = rampCurve.Evaluate(rampT); // 0~1

            // 위치/속도 (월드 기준으로 통일)
            Vector3 posPS = parts[i].position;
            Vector3 velPS = parts[i].velocity;

            Vector3 posW = simWorld ? posPS : ps.transform.TransformPoint(posPS);
            Vector3 velW = simWorld ? velPS : ps.transform.TransformVector(velPS);

            // 파티클 → 스크린 → proxy-local  (★ 같은 uiCam 사용!)
            Vector2 partScreen = RectTransformUtility.WorldToScreenPoint(uiCam, posW);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(proxyRect, partScreen, uiCam, out var partProxyLocal);

            // UI 평면 벡터/도달 판정
            Vector2 toTargetUI = (targetProxyLocal - partProxyLocal);
            float distSqr = toTargetUI.sqrMagnitude;

            if (_killOnArrive && distSqr <= kRadiusSqr)
            {
                parts[i].remainingLifetime = 0f;   // 제거
                OnAttracted?.Invoke();             // 이벤트(개별)
                continue;                          // ★ return 금지! 다음 파티클 처리
            }

            // 아직 지연/램프중이면 힘 0 (서서히 끌어들이기)
            if (ramp <= 0f)
            {
                // 드래그만 적용해 살짝 감속시키고 끝
                float dragFactor0 = Mathf.Exp(-_drag * dt);
                velW *= dragFactor0;
            }
            else
            {
                // 힘/드래그/속도상한/적분
                Vector2 dirUI = (distSqr > 1e-6f) ? (toTargetUI / Mathf.Sqrt(distSqr)) : Vector2.zero;
                Vector2 accUI = dirUI * (_strength * ramp * scaleComp);
                Vector3 accWorld = new Vector3(accUI.x, accUI.y, 0f);

                float dragFactor = Mathf.Exp(-_drag * dt);
                velW *= dragFactor;
                velW += accWorld * dt;

                float limit = _maxSpeed * scaleComp;
                if (velW.sqrMagnitude > limit * limit)
                    velW = velW.normalized * limit;
            }

            // 위치 적분 & 좌표 되돌리기
            posW += velW * dt;

            parts[i].position = simWorld ? posW : ps.transform.InverseTransformPoint(posW);
            parts[i].velocity = simWorld ? velW : ps.transform.InverseTransformVector(velW);
        }

        // 수정 반영
        ps.SetParticles(parts, alive);
    }

    public void AddParticle(UIParticle particle)
    {
        _uiParticles.Add(particle);
    }


    // randomSeed -> [-1, +1] 근사 맵핑 (간단 해시)
    private static float HashToRangeMinusOneToOne(uint seed)
    {
        // Xorshift32 간단 변형
        uint x = seed;
        x ^= x << 13; x ^= x >> 17; x ^= x << 5;
        // 0..1
        float u = (x & 0x00FFFFFF) / 16777215f;
        return u * 2f - 1f; // -1..1
    }

    public void Log()
    {
        Debug.Log("OnAttracted");
    }
}
