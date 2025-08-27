using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class Building : MonoBehaviour
{
    [SerializeField] private GameObject ruinPrefab;
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private GameObject upgradedPrefab;
    [SerializeField] private float constructionTime = 30f;
    [SerializeField] private int maxLevel = 10;

    public enum State { Ruin, Constructing, Base, Upgraded }
    public State CurrentState { get; private set; } = State.Ruin;
    public int Level { get; private set; } = 0;
    public Vector3 FixedPosition { get; private set; }

    private GameObject currentModel;
    private Coroutine constructionCoroutine;
    private ObjectPool<GameObject> ruinPool;
    private ObjectPool<GameObject> basePool;
    private ObjectPool<GameObject> upgradedPool;

    // 컴포넌트 참조 (SOLID 맞춤, 역할/자원 분리)
    private ResourceProducer resourceProducer;
    private RoleHandler roleHandler;

    void Awake()
    {
        FixedPosition = transform.position;
        ruinPool = new ObjectPool<GameObject>(() => Instantiate(ruinPrefab), m => m.SetActive(true), m => m.SetActive(false), Destroy, false, 10, 20);
        basePool = new ObjectPool<GameObject>(() => Instantiate(basePrefab), m => m.SetActive(true), m => m.SetActive(false), Destroy, false, 10, 20);
        upgradedPool = new ObjectPool<GameObject>(() => Instantiate(upgradedPrefab), m => m.SetActive(true), m => m.SetActive(false), Destroy, false, 10, 20);
        SwapModel(ruinPrefab); // 초기 잔해

        resourceProducer = GetComponent<ResourceProducer>();
        roleHandler = GetComponent<RoleHandler>();
    }

    void Start()
    {
        // 수정: FixedPosition 재확인 (iso 부동소수 오차 방지)
        transform.position = FixedPosition;
    }

    public void StartConstruction()
    {
        if (CurrentState != State.Ruin) return;
        // 수정: 자원 체크 Managers.Game으로 통합 (팀원 CommodityManager 호환)
        if (!Managers.Game.CheckResources(50, 30)) return;
        CurrentState = State.Constructing;
        constructionCoroutine = StartCoroutine(ConstructCoroutine());
    }

    private IEnumerator ConstructCoroutine()
    {
        yield return new WaitForSeconds(constructionTime);
        CurrentState = State.Base;
        Level = 1;
        SwapModel(basePrefab);
        constructionCoroutine = null;
    }

    public void CancelConstruction()
    {
        if (CurrentState != State.Constructing) return;
        StopCoroutine(constructionCoroutine);
        CurrentState = State.Ruin;
        SwapModel(ruinPrefab);
        // 수정: 환불 Managers.Game으로 통합
        Managers.Game.RefundResources(50, 30);
        constructionCoroutine = null;
    }

    public void Upgrade()
    {
        if (CurrentState != State.Base || Level >= maxLevel) return;
        // 수정: 자원 체크 Managers.Game으로 통합
        if (!Managers.Game.CheckResources(100, 50)) return;
        // 수정: 정부 연동 Managers.Game으로 통합
        if (Managers.Game.GetGovernmentLevel() < Level + 1) return;
        Level++;
        CurrentState = State.Upgraded;
        SwapModel(upgradedPrefab);
        // 레벨별 변형 (진화 업그레이드 컨셉 맞춤)
        currentModel.transform.localScale = Vector3.one * (1f + Level * 0.1f);

        // 컴포넌트 업데이트 (SOLID 준수)
        if (resourceProducer != null) resourceProducer.OnUpgrade(Level);
        if (roleHandler != null) roleHandler.OnUpgrade(Level);
    }


    void Update()
    {
        // 수정: 테스트용 키 입력 (Q, W, E, R 누를 때마다 업그레이드 트리거)
#if UNITY_EDITOR // 에디터에서만 동작 (모바일 빌드 시 무시)
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
        {
            if (CurrentState == State.Base || CurrentState == State.Upgraded)
            {
                Upgrade(); // 업그레이드 즉시 호출 (자원/정부 체크 무시 for 테스트)
            }
            else if (CurrentState == State.Ruin)
            {
                StartConstruction(); // 잔해 시 건설 시작 (테스트 속도 위해 time=0으로 조정 가능)
            }
        }
#endif
    }
    public void OnEvent(string eventType)
    {
        if (CurrentState == State.Constructing) return;
        if (eventType == "Fire" && (CurrentState == State.Base || CurrentState == State.Upgraded))
        {
            Level = 0;
            CurrentState = State.Ruin;
            SwapModel(ruinPrefab);
        }
        if (roleHandler != null) roleHandler.HandleEvent(eventType);
    }

    private void SwapModel(GameObject newPrefab)
    {
        if (currentModel != null)
        {
            if (newPrefab == ruinPrefab) ruinPool.Release(currentModel);
            else if (newPrefab == basePrefab) basePool.Release(currentModel);
            else upgradedPool.Release(currentModel);
        }
        if (newPrefab == ruinPrefab) currentModel = ruinPool.Get();
        else if (newPrefab == basePrefab) currentModel = basePool.Get();
        else currentModel = upgradedPool.Get();
        currentModel.transform.position = FixedPosition;
        currentModel.transform.rotation = Quaternion.identity;
    }
}