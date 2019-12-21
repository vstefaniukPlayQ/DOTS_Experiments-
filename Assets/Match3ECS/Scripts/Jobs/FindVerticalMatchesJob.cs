using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct FindVerticalMatchesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PieceColor> Board;
    [ReadOnly] public int SlotsPerColumn;
    [WriteOnly] public NativeArray<Vector3> Matches;

    public void Execute(int index)
    {



    }
}