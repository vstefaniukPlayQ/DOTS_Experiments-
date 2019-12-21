using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

public struct GravityComponent : IComponentData
{
    public bool Falling;
    public float FallingSpeed;
}
