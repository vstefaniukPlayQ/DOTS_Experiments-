using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

public struct VelocityJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Vector3> velocity;

    public NativeArray<Vector3> position;

    public float deltaTime;

    public void Execute(int index)
    {
        position[index] = position[index] + velocity[index] * deltaTime;
    }
}
