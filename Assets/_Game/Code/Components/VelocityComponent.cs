using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Velocity : IComponentData {
    public Vector3 Value;
}

public class VelocityComponent : ComponentDataWrapper<Velocity> { }