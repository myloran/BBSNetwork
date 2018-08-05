using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
[Sync]
public struct WeaponState : IComponentData {
    [HideInInspector] public float fireTimer;
    [HideInInspector] public float reloadTimer;
    [HideInInspector] public float effectTimer;
    [HideInInspector] public boolean reloading;

    [FieldSync]
    public int magazine;
}

public class WeaponStateComponent : ComponentDataWrapper<WeaponState> { }
