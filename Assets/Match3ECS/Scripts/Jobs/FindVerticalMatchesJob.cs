using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct FindVerticalMatchesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PieceColor> Board;

    [ReadOnly] public int SlotsPerCollumn;

    [WriteOnly] public NativeArray<int> Output;

    public void Execute(int index)
    {
        NativeList<int> matches = new NativeList<int>(Allocator.Temp);

        PieceColor myColor = Board[index];

        // add selected chip to collection of matches as we going to check pieces
        // to left and right and don't include selected chi[
        matches.Add(index);

        // going top
        for (int i = index - SlotsPerCollumn;  i >= 0; i -= SlotsPerCollumn)
        {
            var selectedPiece = Board[i];
            if (selectedPiece == myColor) // matched !
            {
                matches.Add(i);
            }
            else
            {
                break;
            }
        }

        // going bottom
        for (int i = index + SlotsPerCollumn; i < SlotsPerCollumn * SlotsPerCollumn; i += SlotsPerCollumn)
        {
            var selectedPiece = Board[i];
            if (selectedPiece == myColor) // matched !
            {
                matches.Add(i);
            }
            else
            {
                break;
            }
        }

        // more than 2 pieces of same color
        if (matches.Length > 2)
        {
            Output[index] = matches.Length;

            //for (int m = 0; m < matches.Count; m++)
            // write all match list somewhere
            //Output.Enqueue(m);
        }

        matches.Dispose();
    }
}
