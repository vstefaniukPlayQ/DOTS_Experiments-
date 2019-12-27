using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

public struct FindHorizontalMatchesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PieceColor> Board;

    [ReadOnly] public int SlotsPerRow;

    [WriteOnly] public NativeArray<int> Output;

    public void Execute(int index)
    {
        int row = Mathf.FloorToInt(index / (float) SlotsPerRow);
        int convertedRowLeftLimit = row * SlotsPerRow;
        int convertedRowRightLimit = convertedRowLeftLimit + SlotsPerRow;

        // add selected chip to collection of matches as we going to check pieces
        // to left and right and don't include selected chi[
        int matches = 1;

        PieceColor myColor = Board[index];

        // going left
        for (int i = index - 1; i >= convertedRowLeftLimit; i--)
        {
            var selectedPiece = Board[i];
            if (selectedPiece != myColor)
                break;
            matches++;
        }

        // going right
        for (int i = index + 1; i < convertedRowRightLimit; i++)
        {
            var selectedPiece = Board[i];
            if (selectedPiece != myColor)
                break;
            matches++;
        }

        // more than 2 pieces of same color
        if(matches > 2)
            Output[index] = matches;
    }
}
