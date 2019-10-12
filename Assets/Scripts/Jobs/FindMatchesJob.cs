using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

public struct FindHorizontalMatchesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PieceColor> Board;

    [ReadOnly] public int SlotsPerRow;

    [WriteOnly] public NativeArray<int> Matches;

    public void Execute(int index)
    {
        int row = Mathf.FloorToInt(index / (float) SlotsPerRow);
        int convertedRowLeftLimit = row * SlotsPerRow;
        int convertedRowRightLimit = convertedRowLeftLimit + SlotsPerRow;

        PieceColor myColor = Board[index];

        // going left
        for (int i = index - 1; i >= convertedRowLeftLimit; i--)
        {
            var selectedPiece = Board[i];
            if (selectedPiece == myColor) // matched !
            {
               // Matches.Add(i);
            }
            else
            {
                break;
            }
        }

        // going right
        for (int i = index + 1; i < convertedRowRightLimit; i++)
        {
            var selectedPiece = Board[i];
            if (selectedPiece == myColor) // matched !
            {
               // Matches.Add(i);
            }
            else
            {
                break;
            }
        }

        // more than 2 pieces of same color
        //if (matches.Length > 2)
        //    Matches = matches;

        //matches.Dispose();
    }
}


public struct FindVerticalMatchesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PieceColor> Board;
    [ReadOnly] public int SlotsPerColumn;
    [WriteOnly] public NativeArray<Vector3> Matches;

    public void Execute(int index)
    {



    }
}
