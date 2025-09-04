using UnityEngine;

public class SecurityEventController : EventController
{
    public SecurityEventController(float timer) : base(EventType.SecurityEvent)
    {
    }

    protected override float ScheduleNext(float now, CityStat stats)
    {
        return 0;
    }

    protected override Incident ExecuteSpawn(float now, CityStat stat)
    {
        return null;
    }
}
