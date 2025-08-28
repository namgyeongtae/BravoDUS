using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class Building : MonoBehaviour
{
    [SerializeField] private GameObject ruinPrefab; // 잔해 모델 Prefab
    [SerializeField] private GameObject basePrefab; // 기본 건물 모델 Prefab
    [SerializeField] private GameObject upgradedPrefab; // 업그레이드 모델 Prefab
    [SerializeField] private float constructionTime = 30f; // 건설 시간 (초)
    [SerializeField] private int maxLevel = 10; // 업그레이드 최대 레벨

    public enum State { Ruin, Constructing, Base, Upgraded }
    public State CurrentState { get; private set; } = State.Ruin; // 현재 상태
    public int Level { get; private set; } = 0; // 레벨
    public Vector3 FixedPosition { get; private set; } // 고정 위치

    private GameObject currentModel; // 현재 모델 인스턴스
    private Coroutine constructionCoroutine; // 건설 코루틴 참조
    private ObjectPool<GameObject> ruinPool; // 잔해 풀
    private ObjectPool<GameObject> basePool; // 기본 풀
    private ObjectPool<GameObject> upgradedPool; // 업그레이드 풀

    // 컴포넌트 참조 (SOLID 준수, 역할/자원 분리)
    private ResourceProducer resourceProducer;
    private RoleHandler roleHandler;

    void Awake()
    {
        FixedPosition = transform.position; // 고정 위치 설정
        ruinPool = new ObjectPool<GameObject>(() => Instantiate(ruinPrefab), m => m.SetActive(true), m => m.SetActive(false), Destroy, false, 10, 20);
        basePool = new ObjectPool<GameObject>(() => Instantiate(basePrefab), m => m.SetActive(true), m => m.SetActive(false), Destroy, false, 10, 20);
        upgradedPool = new ObjectPool<GameObject>(() => Instantiate(upgradedPrefab), m => m.SetActive(true), m => m.SetActive(false), Destroy, false, 10, 20);
        SwapModel(ruinPrefab); // 초기 잔해 모델 로드

        resourceProducer = GetComponent<ResourceProducer>(); // 자원 생산 컴포넌트
        roleHandler = GetComponent<RoleHandler>(); // 역할 컴포넌트
    }

    void Start()
    {
        transform.position = FixedPosition; // 수정: iso 부동소수 오차 방지
    }

    void Update()
    {
        // 수정: 테스트용 키 입력 (Q, W, E, R)
#if UNITY_EDITOR // 에디터에서만 동작 (모바일 빌드 시 무시)
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
        {
            if (CurrentState == State.Base || CurrentState == State.Upgraded)
            {
                Upgrade(); // 테스트: 자원/정부 체크 무시
            }
            else if (CurrentState == State.Ruin)
            {
                StartConstruction();
            }
        }
#endif
    }

    public void StartConstruction()
    {
        if (CurrentState != State.Ruin) return;
        // 수정: 자원 체크 Managers.Commodity로 직접 (GameManager 원본 돌림으로 호환)
        if (Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount < 50 || Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount < 30) return;
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
        // 수정: 환불 Managers.Commodity로 직접
        Managers.Commodity.AddIngredient(IngredientType.Wood, 50);
        Managers.Commodity.AddIngredient(IngredientType.Iron, 30);
        constructionCoroutine = null;
    }

    public void Upgrade()
    {
        if (CurrentState != State.Base || Level >= maxLevel) return;
        // 수정: 자원 체크 Managers.Commodity로 직접
        if (Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount < 100 || Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount < 50) return;
        // 수정: 정부 연동 placeholder (별도 CraftingManager에 의존 안 함, 씬 Find)
        if (GetGovernmentLevel() < Level + 1) return;
        Level++;
        CurrentState = State.Upgraded;
        SwapModel(upgradedPrefab);
        currentModel.transform.localScale = Vector3.one * (1f + Level * 0.1f);

        if (resourceProducer != null) resourceProducer.OnUpgrade(Level);
        if (roleHandler != null) roleHandler.OnUpgrade(Level);
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

    private int GetGovernmentLevel() // 수정: 정부 레벨 placeholder (씬 FindObjectOfType, GameManager 의존 제거)
    {
        Building government = FindObjectOfType<Building>(); // 예시: 이름 or 태그로 찾기 (나중 최적화)
        if (government != null && government.name == "Government") return government.Level;
        return 0;
    }
}