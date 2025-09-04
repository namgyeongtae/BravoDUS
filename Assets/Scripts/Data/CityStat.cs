using UnityEngine;

public class CityStat
{
    public float SecurityRate; // 치안율(0~1, 높을수록 안전)
    public float ResponsePower;// 경찰 대응력
    public float FireRate;     // 화재율(0~1)
    public float SuppressPower;// 소방 진압력

    // Temp Code
    // 나중에 DB에서 로드하도록 수정 필요
    // 이렇게 생성자로 안할 거임
    public CityStat()
    {
        SecurityRate = 0.5f;
        ResponsePower = 10f;
        FireRate = 0.5f;
        SuppressPower = 10f;
    }
}
