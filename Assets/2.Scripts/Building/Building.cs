using UnityEngine;
using System.Collections;
using UnityEngine.Pool;
using System.Collections.Generic;
using System;

public class Building : MonoBehaviour
{
    [SerializeField] private GameObject ruinPrefab; // 잔해 모델 Prefab
    [SerializeField] private GameObject basePrefab; // 기본 건물 모델 Prefab
    [SerializeField] private GameObject upgradedPrefab; // 업그레이드 모델 Prefab
    [SerializeField] private GameObject constructionEffectPrefab; // 건축 중 이펙트 Prefab (Particle System)
    [SerializeField] private float upgradeTime = 1f; // 업그레이드 시간 (초, 테스트용 짧게 설정)
    [SerializeField] private int maxLevel = 10; // 업그레이드 최대 레벨
    [SerializeField] private bool _isTestMode = false; // 기본 false
    [SerializeField] public float constructionTime = 5f; // 건설 시간 (초)

    private Animator _animator;

    private List<WorkForce> _workForceList = new(); // 건물에 할당된 인력 리스트
    public Action OnWorkForceChanged;

    public enum State { Ruin, Constructing, Base, Upgrading, Upgraded }
    public State CurrentState { get; private set; } = State.Ruin; // 현재 상태
    public int Level { get; private set; } = 0; // 레벨
    public Vector3 FixedPosition { get; private set; } // 고정 위치
    public BuildingType BuildingType; // 건물 타입
    public List<WorkForce> WorkForceList => _workForceList; // 건물에 할당된 인력 리스트

    private GameObject currentModel; // 현재 모델 인스턴스
    private Coroutine constructionCoroutine; // 건설 코루틴 참조
    private Coroutine upgradeCoroutine; // 업그레이드 코루틴 참조

    private GameObject currentEffect; // 현재 이펙트 인스턴스

    private ResourceProducer resourceProducer;
    private RoleHandler roleHandler;

    void Awake()
    {
        // FixedPosition = transform.position;

        roleHandler = GetComponent<RoleHandler>();
        _animator = GetComponent<Animator>();

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
        // transform.position = FixedPosition;
    }

    void Update()
    {
        /* #if UNITY_EDITOR
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
                {
                    Debug.Log($"Update detected key press for {gameObject.name} - CurrentState: {CurrentState}");
                    if (CurrentState == State.Ruin)
                    {
                        StartConstruction();
                    }
                    // else if (CurrentState == State.Base)
                    // {
                    // Upgrade();
                    // }
                }
        #endif */
    }

    public void StartConstruction()
    {
        Managers.Commodity.LogAmounts(); // 추가: 체크 직전 Amount 로그 (필요 시 유지/삭제)
        Debug.Log($"StartConstruction - CurrentState: {CurrentState}, isTestMode: {_isTestMode}");
        if (CurrentState != State.Ruin) return;
        CurrentState = State.Constructing;

        Debug.Log($"State changed to Constructing: {gameObject.name}");
        constructionCoroutine = StartCoroutine(ConstructCoroutine());
        Debug.Log($"StartConstruction called for {gameObject.name}");
    }

    private IEnumerator ConstructCoroutine()
    {
        Debug.Log($"ConstructCoroutine started for {gameObject.name}");

        // CreateConstructor(); // 건설자 프리팹 생성

        // basePrefab.SetActive(false);

        _animator.SetTrigger("Construct");

        // 건물 숨김
        if (currentModel != null)
        {
            currentModel.SetActive(false);
            Debug.Log($"Building hidden: {currentModel.name}");
        }

        // 이펙트 생성 및 활성화 (위치 조정)
        CreateConstructionEffect();

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(constructionTime / 5f);
            Debug.Log("건설 진행: " + ((i + 1) * 20) + "% - " + gameObject.name + " (State: " + CurrentState + ")");
        }

        CompleteConstruction();
    }

    public void CompleteConstruction()
    {
        DestroyConstructor();  // 건설자 프리팹 제거

        basePrefab.SetActive(true);
        ruinPrefab.SetActive(false);

        _animator.SetTrigger("Construct");

        CurrentState = State.Base;
        Level = 0; // Base 상태에서 레벨 0부터 시작으로 초기화
        // SwapModel(basePrefab);
        StopCoroutine(constructionCoroutine);
        constructionCoroutine = null;

        // 이펙트 및 건물 상태 복구
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
            Debug.Log($"Construction effect removed for {gameObject.name}");
        }

        Debug.Log($"건설 완료: {gameObject.name}, Level: {Level}, State: {CurrentState}");

        OnWorkForceChanged?.Invoke();
    }

    public void CancelConstruction()
    {
        if (CurrentState != State.Constructing) return;
        StopCoroutine(constructionCoroutine);
        CurrentState = State.Ruin;
        // SwapModel(ruinPrefab);
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
        if (upgradeCoroutine != null) return; // 업그레이드 코루틴이 이미 실행 중이면 무시
        if (CurrentState != State.Base) return; // Base 상태에서만 업그레이드 가능
                                                // --- 최대 레벨 체크 ---
        if (Level >= maxLevel)
        {
            Debug.Log($"최대 레벨 도달: {gameObject.name}, Level: {Level}");
            return;
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
            // SwapModel(basePrefab);
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
            yield return new WaitForSeconds(upgradeTime / 5f);
            Debug.Log($"업그레이드 진행: {((i + 1) * 20)}% - {gameObject.name}, Level: {Level}");
        }
        CurrentState = State.Upgraded;
        //SwapModel(upgradedPrefab);
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

    #region Swap Model
    /* private void SwapModel(GameObject newPrefab)
    {
        // 🔹 1. 현재 모델 반환
        if (currentModel != null)
        {
            // 현재 모델이 어떤 풀에서 나온 건지에 따라 반환
            if (currentModel.CompareTag("Ruin"))
            {
                ruinPool.Release(currentModel);
            }
            else if (currentModel.CompareTag("Base"))
            {
                // basePool.Release(currentModel);
            }
            else if (currentModel.CompareTag("Upgraded"))
            {
                upgradedPool.Release(currentModel);
            }
            currentModel = null;
        }

        // 🔹 2. 새 모델 생성
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

        // 🔹 3. 배치 및 설정
        if (currentModel != null)
        {
            currentModel.transform.SetParent(transform, false);
            currentModel.transform.position = FixedPosition;
            currentModel.transform.rotation = Quaternion.identity;
            currentModel.SetActive(true);
            Debug.Log($"✅ 모델 교체 완료: {gameObject.name}, 새 모델: {currentModel.name}, Position: {currentModel.transform.position}");
        }
        else
        {
            Debug.LogError($"❌ 모델 생성 실패: {gameObject.name}, 프리팹: {newPrefab.name}");
        }
    } */
    #endregion

    private void CreateConstructionEffect()
    {
        if (constructionEffectPrefab != null)
        {
            currentEffect = Instantiate(constructionEffectPrefab, FixedPosition, Quaternion.identity);
            if (currentEffect == null)
            {
                Debug.LogError($"Failed to instantiate effect prefab: {constructionEffectPrefab.name}");
                return;
            }
            currentEffect.transform.SetParent(transform, false);
            Renderer modelRenderer = currentModel != null ? currentModel.GetComponent<Renderer>() : null;
            float heightOffset = modelRenderer != null ? modelRenderer.bounds.size.y / 2f : 1f; // 모델 중앙 높이
            // currentEffect.transform.position = FixedPosition + Vector3.up * heightOffset;
            currentEffect.transform.localPosition = new Vector3(0, 1.5f, 0f);
            ParticleSystem ps = currentEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Debug.Log($"Effect instantiated: {currentEffect.name}, Position: {currentEffect.transform.position}, Offset: {heightOffset}, Active: {currentEffect.activeSelf}");
        }
        else
        {
            Debug.LogWarning($"constructionEffectPrefab is null for {gameObject.name}");
        }
    }

    private void CreateConstructor()
    {
        var collider = ruinPrefab.GetComponent<BoxCollider>();

        Debug.Log($"Log Bounds : {collider.size}");

        GameObject constructor = Managers.Resource.Instantiate("Avatar/Constructor");
        constructor.transform.SetParent(transform);
        constructor.transform.position = transform.position
                    + transform.forward * (collider.size.z / 2f * 3.5f) * -1
                    + transform.up * (collider.size.y / 2f * 0.5f) * -1;
    }

    private void DestroyConstructor()
    {
        var constructors = transform.Find("Constructor");

        if (constructors != null)
            Managers.Resource.Destroy(constructors.gameObject);
    }

    public void AssignWorkForce(WorkForce workForce)
    {
        _workForceList.Add(workForce);
        OnWorkForceChanged?.Invoke();
    }

    public void UnassignWorkForce(WorkForce workForce)
    {
        _workForceList.Remove(workForce);
        OnWorkForceChanged?.Invoke();
    }

    // ============================
    // 🔽🔽🔽 여기부터 Save/Load 관련 추가 코드 🔽🔽🔽
    // ============================

    /// <summary>
    /// 현재 Building 상태를 세이브용 데이터로 변환
    /// </summary>
    public BuildingSaveData ToSaveData()
    {
        var data = new BuildingSaveData();

        // 어떤 건물인지 구분용
        data.buildingName = gameObject.name;
        data.buildingType = (int)BuildingType;

        // 위치 / 회전
        Vector3 pos = transform.position;
        data.posX = pos.x;
        data.posY = pos.y;
        data.posZ = pos.z;

        data.rotY = transform.rotation.eulerAngles.y;

        // 상태 / 레벨
        data.level = this.Level;
        data.state = (int)this.CurrentState;

        data.isConstructing = (this.CurrentState == State.Constructing);
        data.isUpgrading = (this.CurrentState == State.Upgrading);

        // 공사 남은 시간, 업그레이드 남은 시간 등은
        // 나중에 필요해지면 필드 추가해서 채우면 됨
        return data;
    }

    /// <summary>
    /// 세이브 데이터 기반으로 Building 상태 복원
    /// </summary>
    public void ApplySaveData(BuildingSaveData data)
    {
        // 위치 / 회전 복원
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        transform.rotation = Quaternion.Euler(0f, data.rotY, 0f);

        // 상태 / 레벨 복원
        Level = data.level;
        CurrentState = (State)data.state;

        // 상태에 맞게 ruin/base/upgraded 오브젝트 활성화
        ApplyVisualByStateInstant();
    }

    /// <summary>
    /// CurrentState에 맞게 ruin/base/upgraded 오브젝트의 Active 상태를 정리
    /// (세이브 로드 직후 한 번 호출해 주는 용도)
    /// </summary>
    private void ApplyVisualByStateInstant()
    {
        if (ruinPrefab != null) ruinPrefab.SetActive(false);
        if (basePrefab != null) basePrefab.SetActive(false);
        if (upgradedPrefab != null) upgradedPrefab.SetActive(false);

        switch (CurrentState)
        {
            case State.Ruin:
                if (ruinPrefab != null) ruinPrefab.SetActive(true);
                break;

            case State.Constructing:
            case State.Base:
                if (basePrefab != null) basePrefab.SetActive(true);
                break;

            case State.Upgrading:
                // 업그레이드 중일 때도 일단 base 모델 보여주기
                if (basePrefab != null) basePrefab.SetActive(true);
                break;

            case State.Upgraded:
                if (upgradedPrefab != null)
                    upgradedPrefab.SetActive(true);
                else if (basePrefab != null)
                    basePrefab.SetActive(true);
                break;
        }
    }
}
