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
        transform.position = FixedPosition; // iso 부동소수 오차 방지
    }

    void Update()
    {
        // 테스트용 키 입력 유지
#if UNITY_EDITOR
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

    // 수정: 모바일 클릭(터치) / 에디터 마우스 클릭으로 변화 트리거 (GameObject에 Collider 붙여야 함)
    private void OnMouseDown()
    {
        if (CurrentState == State.Base || CurrentState == State.Upgraded)
        {
            Upgrade(); // 클릭 시 업그레이드 (모바일 터치 호환)
        }
        else if (CurrentState == State.Ruin)
        {
            StartConstruction(); // 클릭 시 건설 시작
        }
    }

    public void StartConstruction()
    {
        if (CurrentState != State.Ruin) return;
        if (Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount < 50 || Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount < 30) return;
        CurrentState = State.Constructing;
        constructionCoroutine = StartCoroutine(ConstructCoroutine());
    }

    private IEnumerator ConstructCoroutine()
    {
        // 수정: 1초마다 진행도 로그 표시 (for 루프, 5초 기준 0~100%)
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("건설 진행: " + ((i + 1) * 20) + "% - " + gameObject.name); // 추가: 1초마다 로그 (20% 증가)
        }
        CurrentState = State.Base;
        Level = 1;
        SwapModel(basePrefab);
        constructionCoroutine = null;
        Debug.Log("건설 완료: " + gameObject.name + ", Level: " + Level); // 완료 로그
    }

    public void CancelConstruction()
    {
        if (CurrentState != State.Constructing) return;
        StopCoroutine(constructionCoroutine);
        CurrentState = State.Ruin;
        SwapModel(ruinPrefab);
        Managers.Commodity.AddIngredient(IngredientType.Wood, 50);
        Managers.Commodity.AddIngredient(IngredientType.Iron, 30);
        constructionCoroutine = null;
    }

    public void Upgrade()
    {
        // 수정: State 조건 완화 – Base or Upgraded 모두 허용 (레벨 1 후 추가 업그레이드 가능)
        if (Level >= maxLevel) return;
        if (Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount < 100 || Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount < 50) return;
        if (GetGovernmentLevel() < Level + 1) return;
        Level++;
        if (Level == 1) CurrentState = State.Base; // 수정: 레벨 1 시 Base (처음 업그레이드)
        else CurrentState = State.Upgraded; // 레벨 2+ 시 Upgraded
        SwapModel(Level == 1 ? basePrefab : upgradedPrefab); // 수정: 레벨 1 base, 2+ upgraded
        currentModel.transform.localScale = Vector3.one * (1f + Level * 0.1f);

        if (resourceProducer != null) resourceProducer.OnUpgrade(Level);
        if (roleHandler != null) roleHandler.OnUpgrade(Level);
        Debug.Log("업그레이드 완료: " + gameObject.name + ", Level: " + Level);
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

    private int GetGovernmentLevel()
    {
        Building government = FindObjectOfType<Building>();
        if (government != null && government.name == "Government") return government.Level;
        return 0;
    }
}