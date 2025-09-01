using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        // 원본에 Release 관련 없음, 필요시 추가
    }
}