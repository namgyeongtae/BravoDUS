using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shuriken ParticleSystem → Mesh Bake → CanvasRenderer로 그려서
/// uGUI(마스크/정렬)에 자연스럽게 통합하는 최소 구현.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
[DisallowMultipleComponent]
public class UIParticle : MaskableGraphic
{
    [Header("Target ParticleSystem (child allowed)")]
    public ParticleSystem particleSystemRef;           // 지정 안하면 자동 검색
    private ParticleSystemRenderer _psr;

    [Header("UI Material (use shader: UI/Particle Additive)")]
    public Material uiMaterial;                        // UI용 Additive/Alpha 등

    [Header("Trails")]
    public bool bakeTrails = true;                     // 파티클 트레일도 베이크

    private Mesh _mesh;
    private Mesh _trailsMesh;

    protected override void Awake()
    {
        base.Awake();

        if (!particleSystemRef)
            particleSystemRef = GetComponent<ParticleSystem>();

        if (particleSystemRef)
            _psr = particleSystemRef.GetComponent<ParticleSystemRenderer>();

        if (_mesh == null) _mesh = new Mesh();
        _mesh.name = "UIParticleProxy Mesh";
        _mesh.MarkDynamic();

        if (_trailsMesh == null) _trailsMesh = new Mesh();
        _trailsMesh.name = "UIParticleProxy TrailsMesh";
        _trailsMesh.MarkDynamic();

        // Graphic이 기본 머티리얼/텍스처를 찾을 수 있도록
        if (!uiMaterial)
        {
            Debug.LogWarning("[UIParticleProxy] uiMaterial이 비어있습니다. UI/Particle Additive 셰이더 머티리얼을 지정하세요.", this);
        }
    }

    public override Texture mainTexture
    {
        get
        {
            if (uiMaterial && uiMaterial.mainTexture) return uiMaterial.mainTexture;
            if (material && material.mainTexture) return material.mainTexture;
            return s_WhiteTexture;
        }
    }

    // uGUI 기본 메시는 쓰지 않고, CanvasRenderer.SetMesh를 사용.
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }

    void LateUpdate()
    {
        if (_psr == null)
        {
            if (particleSystemRef)
                _psr = particleSystemRef.GetComponent<ParticleSystemRenderer>();
            if (_psr == null) return;
        }

        // Overlay면 null, Camera/World면 worldCamera 사용
        Camera cam = null;
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        // 파티클 메시 베이크

        if (cam == null)
        {
            _psr.BakeMesh(_mesh, Camera.main, true);
        }
        else
        {
            _psr.BakeMesh(_mesh, cam, true);
        }

        
        if (bakeTrails)
        {
            try { _psr.BakeTrailsMesh(_trailsMesh, cam, true); }
            catch { _trailsMesh.Clear(); } // 트레일 미사용일 경우 대비
        }
        else _trailsMesh.Clear();

        // CanvasRenderer에 메시/머티리얼 세팅
        // (메시 2개면 2패스로 그리거나, 간단히 합쳐도 되지만 최소예제에선 두 번 호출)
        canvasRenderer.Clear();

        var mat = uiMaterial ? uiMaterial : material; // uiMaterial 우선
        if (!mat) { mat = defaultMaterial; }

        // 본체
        canvasRenderer.SetMaterial(mat, mainTexture);
        canvasRenderer.SetMesh(_mesh);

        // 트레일(같은 머티리얼로 2번째 드로우)
        if (_trailsMesh.vertexCount > 0)
        {
            canvasRenderer.SetMaterial(mat, mainTexture);
            canvasRenderer.SetMesh(_trailsMesh);
        }

        if (Input.GetKeyDown(KeyCode.Space)) Play();
    }

    // 외부에서 파티클을 재생/정지할 때 편의용
    public void Play()
    {
        if (particleSystemRef) particleSystemRef.Play(true);
    }
    public void Stop(bool withChildren = true, ParticleSystemStopBehavior behavior = ParticleSystemStopBehavior.StopEmittingAndClear)
    {
        if (particleSystemRef) particleSystemRef.Stop(withChildren, behavior);
    }
}
