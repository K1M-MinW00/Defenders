using System;
using UnityEngine;

[Serializable]
public sealed class MonsterBehaviorSettings
{
    [Min(0.02f)] public float idleTargetAcquireInterval = 0.5f;
    [Min(0.02f)] public float moveTargetRefreshInterval = 0.25f;
}
