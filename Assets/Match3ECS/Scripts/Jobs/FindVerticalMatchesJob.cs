using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct FindVerticalMatchesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PieceColor> Board;

    [ReadOnly] public int SlotsPerColumn;

    [WriteOnly] public NativeArray<int> Output;

    public void Execute(int index)
    {
        PieceColor myColor = Board[index];

        // add selected chip to collection of matches as we going to check pieces
        // to left and right and don't include selected chi[
        int matches = 1;

        // going top
        for (int i = index - SlotsPerColumn; i >= 0; i -= SlotsPerColumn)
        {
            var selectedPiece = Board[i];
            if (selectedPiece != myColor)
                break;
            matches++;
        }

        // going bottom
        for (int i = index + SlotsPerColumn; i < SlotsPerColumn * SlotsPerColumn; i += SlotsPerColumn)
        {
            var selectedPiece = Board[i];
            if (selectedPiece != myColor)
                break;
            matches++;
        }

        // more than 2 pieces of same color
        if (matches > 2)
            Output[index] = matches;
    }
}
