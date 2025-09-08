using UnityEngine;

[System.Serializable]
public class BuildingStat
{
    public int Level;
    public int RequiredWood;
    public int RequiredIron;
    public int RequiredGovernmentLevel;
}

[System.Serializable]
public class CapentryStat : BuildingStat
{
    public float CycleTime;
    public float Quantity;
}

[System.Serializable]
public class IronWorksStat : BuildingStat
{
    public float CycleTime;
    public float Quantity;
}

[System.Serializable]
public class PoliceStationStat : BuildingStat
{
    public float SecurityPower;
}

[System.Serializable]
public class FireStationStat : BuildingStat
{
    public float SuppressPower;
}

[System.Serializable]
public class HospitalStat : BuildingStat
{
    public float RecoveryPower;
}