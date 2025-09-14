using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class Building : MonoBehaviour
{
    [SerializeField] private GameObject ruinPrefab; // 잔해 모델 Prefab
    [SerializeField] private GameObject basePrefab; // 기본 건물 모델 Prefab
    [SerializeField] private GameObject upgradedPrefab; // 업그레이드 모델 Prefab
    [SerializeField] private GameObject constructionEffectPrefab; // 건축 중 이펙트 Prefab (Particle System)
    [SerializeField] private float upgradeTime = 1f; // 업그레이드 시간 (초, 테스트용 짧게 설정)
    [SerializeField] private int maxLevel = 10; // 업그레이드 최대 레벨
    [SerializeField] private bool _isTestMode = true; // 테스트 모드 플래그
    [SerializeField] public float constructionTime = 5f; // 건설 시간 (초)

    public enum State { Ruin, Constructing, Base, Upgrading, Upgraded }
    public State CurrentState { get; private set; } = State.Ruin; // 현재 상태
    public int Level { get; private set; } = 0; // 레벨
    public Vector3 FixedPosition { get; private set; } // 고정 위치

    private GameObject currentModel; // 현재 모델 인스턴스
    private Coroutine constructionCoroutine; // 건설 코루틴 참조
    private Coroutine upgradeCoroutine; // 업그레이드 코루틴 참조
    private ObjectPool<GameObject> ruinPool; // 잔해 풀
    private ObjectPool<GameObject> basePool; // 기본 풀
    private ObjectPool<GameObject> upgradedPool; // 업그레이드 풀
    private GameObject currentEffect; // 현재 이펙트 인스턴스
    private ResourceProducer resourceProducer;
    private RoleHandler roleHandler;

    void Awake()
    {
        FixedPosition = transform.position;
        Debug.Log($"Building Awake: Prefab references - Ruin: {ruinPrefab != null}, Base: {basePrefab != null}, Upgraded: {upgradedPrefab != null}, Effect: {constructionEffectPrefab != null}");

        ruinPool = new ObjectPool<GameObject>(
            () => {
                GameObject obj = Instantiate(ruinPrefab);
                RemoveDuplicateComponents(obj);
                return obj;
            },
            m => m.SetActive(true),
            m => m.SetActive(false),
            Destroy, false, 10, 20);

        basePool = new ObjectPool<GameObject>(
            () => {
                GameObject obj = Instantiate(basePrefab);
                RemoveDuplicateComponents(obj);
                return obj;
            },
            m => m.SetActive(true),
            m => m.SetActive(false),
            Destroy, false, 10, 20);

        upgradedPool = new ObjectPool<GameObject>(
            () => {
                GameObject obj = Instantiate(upgradedPrefab);
                RemoveDuplicateComponents(obj);
                return obj;
            },
            m => m.SetActive(true),
            m => m.SetActive(false),
            Destroy, false, 10, 20);

        SwapModel(ruinPrefab);
        resourceProducer = GetComponent<ResourceProducer>();
        roleHandler = GetComponent<RoleHandler>();
        // 빈 오브젝트의 콜라이더 제거
        var collider = GetComponent<Collider>();
        if (collider != null) Destroy(collider);
    }

    private void RemoveDuplicateComponents(GameObject obj)
    {
        // Building 컴포넌트 제거 (중복 방지)
        Building[] buildings = obj.GetComponentsInChildren<Building>(true);
        foreach (var b in buildings)
        {
            Destroy(b);
            Debug.Log($"Removed duplicate Building from {obj.name}");
        }

        // ChildModelClickHandler 중복 제거 (하나만 유지)
        ChildModelClickHandler[] handlers = obj.GetComponentsInChildren<ChildModelClickHandler>(true);
        if (handlers.Length > 1)
        {
            for (int i = 1; i < handlers.Length; i++)
            {
                Destroy(handlers[i]);
                Debug.Log($"Removed duplicate ChildModelClickHandler from {obj.name}");
            }
        }
    }

    void Start()
    {
        transform.position = FixedPosition;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"Update detected key press for {gameObject.name} - CurrentState: {CurrentState}");
            if (CurrentState == State.Ruin)
            {
                StartConstruction();
            }
           // else if (CurrentState == State.Base)
           // {
           //     Upgrade();
           // }
        }
#endif
    }

    public void StartConstruction()
    {
        Debug.Log($"StartConstruction - CurrentState: {CurrentState}, isTestMode: {_isTestMode}");
        if (CurrentState != State.Ruin) return;
        if (!_isTestMode)
        {
            if (Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount < 50 ||
                Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount < 30) return;
        }
        CurrentState = State.Constructing;
        Debug.Log($"State changed to Constructing: {gameObject.name}");
        constructionCoroutine = StartCoroutine(ConstructCoroutine());
        Debug.Log($"StartConstruction called for {gameObject.name}");
    }

    private IEnumerator ConstructCoroutine()
    {
        Debug.Log($"ConstructCoroutine started for {gameObject.name}");
        // 건물 숨김
        if (currentModel != null)
        {
            currentModel.SetActive(false);
            Debug.Log($"Building hidden: {currentModel.name}");
        }
        // 이펙트 생성 및 활성화 (위치 조정)
        if (constructionEffectPrefab != null)
        {
            currentEffect = Instantiate(constructionEffectPrefab, FixedPosition, Quaternion.identity);
            if (currentEffect == null)
            {
                Debug.LogError($"Failed to instantiate effect prefab: {constructionEffectPrefab.name}");
                yield break;
            }
            currentEffect.transform.SetParent(transform, false);
            Renderer modelRenderer = currentModel != null ? currentModel.GetComponent<Renderer>() : null;
            float heightOffset = modelRenderer != null ? modelRenderer.bounds.size.y / 2f : 1f; // 모델 중앙 높이
            currentEffect.transform.position = FixedPosition + Vector3.up * heightOffset;
            ParticleSystem ps = currentEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Debug.Log($"Effect instantiated: {currentEffect.name}, Position: {currentEffect.transform.position}, Offset: {heightOffset}, Active: {currentEffect.activeSelf}");
        }
        else
        {
            Debug.LogWarning($"constructionEffectPrefab is null for {gameObject.name}");
        }
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(constructionTime / 5f);
            Debug.Log($"건설 진행: {((i + 1) * 20)}% - {gameObject.name} (State: {CurrentState})");
        }
        CurrentState = State.Base;
        Level = 0; // Base 상태에서 레벨 0부터 시작으로 초기화
        SwapModel(basePrefab);
        constructionCoroutine = null;
        // 이펙트 및 건물 상태 복구
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
            Debug.Log($"Construction effect removed for {gameObject.name}");
        }
        if (currentModel != null)
        {
            currentModel.SetActive(true); // 새 모델 활성화
            Debug.Log($"Building restored: {currentModel.name}");
        }
        Debug.Log($"건설 완료: {gameObject.name}, Level: {Level}, State: {CurrentState}");
    }

    public void CancelConstruction()
    {
        if (CurrentState != State.Constructing) return;
        StopCoroutine(constructionCoroutine);
        CurrentState = State.Ruin;
        SwapModel(ruinPrefab);
        if (!_isTestMode)
        {
            Managers.Commodity.AddIngredient(IngredientType.Wood, 50);
            Managers.Commodity.AddIngredient(IngredientType.Iron, 30);
        }
        constructionCoroutine = null;
        // 이펙트 및 건물 상태 복구
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
            Debug.Log($"Construction effect removed on cancel for {gameObject.name}");
        }
        if (currentModel != null)
        {
            currentModel.SetActive(true); // 원래 모델 복구
            Debug.Log($"Building restored on cancel: {currentModel.name}");
        }
        Debug.Log($"건설 취소: {gameObject.name}, Level: {Level}, State: {CurrentState}");
    }

    public void Upgrade()
    {
        Debug.Log($"Upgrade - CurrentState: {CurrentState}, isTestMode: {_isTestMode}");

        // --- 중복 호출 방지 ---
        if (upgradeCoroutine != null) return;       // 업그레이드 코루틴이 이미 실행 중이면 무시
        if (CurrentState != State.Base) return;     // Base 상태에서만 업그레이드 가능

        // --- 최대 레벨 체크 ---
        if (Level >= maxLevel)
        {
            Debug.Log($"최대 레벨 도달: {gameObject.name}, Level: {Level}");
            return;
        }

        // --- 자원 체크 (테스트 모드 아닐 때만) ---
        if (!_isTestMode)
        {
            if (Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount < 100 ||
                Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount < 50) return;
            if (GetGovernmentLevel() < Level + 1) return;
        }

        // --- 레벨 증가 ---
        Level++;
        Debug.Log($"Level increased to {Level} for {gameObject.name}");

        // --- 최종 업그레이드 처리 ---
        if (Level == maxLevel)
        {
            CurrentState = State.Upgrading;
            upgradeCoroutine = StartCoroutine(UpgradeCoroutine());
            Debug.Log($"Upgrade (to Upgraded) called for {gameObject.name}");
        }
        else
        {
            // 레벨업은 즉시 적용, 모델은 Base 유지
            SwapModel(basePrefab);
            Debug.Log($"Level up completed: {gameObject.name}, Level: {Level}, State: {CurrentState}");
        }
    }


    private IEnumerator UpgradeCoroutine()
    {
        Debug.Log($"UpgradeCoroutine started for {gameObject.name}");
        // 건물 숨김
        if (currentModel != null)
        {
            currentModel.SetActive(false);
            Debug.Log($"Building hidden: {currentModel.name}");
        }
        // 이펙트 생성 및 활성화
        if (constructionEffectPrefab != null)
        {
            currentEffect = Instantiate(constructionEffectPrefab, FixedPosition, Quaternion.identity);
            if (currentEffect == null)
            {
                Debug.LogError($"Failed to instantiate effect prefab: {constructionEffectPrefab.name}");
                yield break;
            }
            currentEffect.transform.SetParent(transform, false);
            Renderer modelRenderer = currentModel != null ? currentModel.GetComponent<Renderer>() : null;
            float heightOffset = modelRenderer != null ? modelRenderer.bounds.size.y / 2f : 1f;
            currentEffect.transform.position = FixedPosition + Vector3.up * heightOffset;
            ParticleSystem ps = currentEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Debug.Log($"Effect instantiated: {currentEffect.name}, Position: {currentEffect.transform.position}, Offset: {heightOffset}, Active: {currentEffect.activeSelf}");
        }
        else
        {
            Debug.LogWarning($"constructionEffectPrefab is null for {gameObject.name}");
        }
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(upgradeTime / 5f);
            Debug.Log($"업그레이드 진행: {((i + 1) * 20)}% - {gameObject.name}, Level: {Level}");
        }
        CurrentState = State.Upgraded;
        SwapModel(upgradedPrefab);
        upgradeCoroutine = null;
        // 이펙트 및 건물 상태 복구
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
            Debug.Log($"Upgrade effect removed for {gameObject.name}");
        }
        if (currentModel != null)
        {
            currentModel.SetActive(true);
            Debug.Log($"Building restored: {currentModel.name}");
        }
        Debug.Log($"업그레이드 완료: {gameObject.name}, Level: {Level}, State: {CurrentState}");
    }

    public void CancelUpgrade()
    {
        if (CurrentState != State.Upgrading) return;
        StopCoroutine(upgradeCoroutine);
        CurrentState = State.Base;
        if (!_isTestMode)
        {
            Managers.Commodity.AddIngredient(IngredientType.Wood, 100);
            Managers.Commodity.AddIngredient(IngredientType.Iron, 50);
        }
        upgradeCoroutine = null;
        // 이펙트 및 건물 상태 복구
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
            Debug.Log($"Upgrade effect removed on cancel for {gameObject.name}");
        }
        if (currentModel != null)
        {
            currentModel.SetActive(true);
            Debug.Log($"Building restored on cancel: {currentModel.name}");
        }
        Debug.Log($"업그레이드 취소: {gameObject.name}, Level: {Level}, State: {CurrentState}");
    }

    public void OnEvent(string eventType)
    {
        if (CurrentState == State.Constructing || CurrentState == State.Upgrading) return;
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
            if (ruinPool != null && currentModel.name.Contains(ruinPrefab.name))
            {
                ruinPool.Release(currentModel);
            }
            else if (basePool != null && currentModel.name.Contains(basePrefab.name))
            {
                basePool.Release(currentModel);
            }
            else if (upgradedPool != null && currentModel.name.Contains(upgradedPrefab.name))
            {
                upgradedPool.Release(currentModel);
            }
            currentModel = null;
        }
        if (newPrefab == ruinPrefab)
        {
            currentModel = ruinPool.Get();
        }
        else if (newPrefab == basePrefab)
        {
            currentModel = basePool.Get();
        }
        else
        {
            currentModel = upgradedPool.Get();
        }
        if (currentModel != null)
        {
            currentModel.transform.SetParent(transform, false);
            currentModel.transform.position = FixedPosition;
            currentModel.transform.rotation = Quaternion.identity;
            currentModel.SetActive(true); // 새 모델 활성화
            Debug.Log($"모델 교체 완료: {gameObject.name}, 새 모델: {currentModel.name}, Position: {currentModel.transform.position}");
        }
        else
        {
            Debug.LogError($"모델 생성 실패: {gameObject.name}, 프리팹: {newPrefab.name}");
        }
    }

    private int GetGovernmentLevel()
    {
        Building[] buildings = FindObjectsOfType<Building>();
        foreach (var building in buildings)
        {
            if (building.name == "Government") return building.Level;
        }
        return 0;
    }
}