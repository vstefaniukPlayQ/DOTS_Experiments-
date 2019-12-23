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

        NativeList<int> matches = new NativeList<int>(Allocator.Temp);

        PieceColor myColor = Board[index];

        // add selected chip to collection of matches as we going to check pieces
        // to left and right and don't include selected chi[
        matches.Add(index);

        // going left
        for (int i = index - 1; i >= convertedRowLeftLimit; i--)
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

        // going right
        for (int i = index + 1; i < convertedRowRightLimit; i++)
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
