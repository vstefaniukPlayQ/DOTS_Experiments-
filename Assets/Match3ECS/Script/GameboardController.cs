using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = System.Random;

public enum PieceColor
{
    Been = 1,
    Butterfly = 2,
    Cloud = 3,
    Crystal = 4,
    Fruit = 5,
    Honey = 6,
    Leaf = 7,
    Mushroom = 8
}

[Serializable]
public struct PieceMaterialByColor
{
    public PieceColor Color;
    public Material Material;
}

public class GameboardController : MonoBehaviour
{
    public Mesh PieceMesh;

    public List<PieceMaterialByColor> Pieces;

    // todo: it is just a array for the simplicity and fast access,
    // not quite sure if it need to be separate entity or something
    // also we don't care to much about other data except of PieceColor
    private PieceColor[,] BoardPieces;
    private PieceColor[] PieceColors;

    [Header("Board"), Space]
    public int BoardWidth = 8;
    public int BoardHeight = 8;

    public float SlotPhysicalSpace = 1;

    public float PieceFallingSpeed = 9.8f;

    protected void Start()
    {
        var entityManager = World.Active.EntityManager;

        EntityArchetype gameboardChipArchetype = entityManager.CreateArchetype
        (
typeof(ChipColorComponent),
            typeof(ChipPositionComponent),
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(GravityComponent)
        );

        int entitiesNeeded = BoardHeight * BoardWidth;

        var createdEntities = new NativeArray<Entity>(entitiesNeeded, Allocator.Temp);

        entityManager.CreateEntity(gameboardChipArchetype, createdEntities);

        BoardPieces = new PieceColor[BoardWidth, BoardHeight];
        PieceColors = new PieceColor[BoardHeight * BoardWidth];

        for (int i = 0; i < entitiesNeeded; i++)
        {
            var currentEntity = createdEntities[i];

            var x = i % BoardWidth;
            var y = (int) Mathf.Floor(i / (float) BoardHeight);

            var randomIndex = UnityEngine.Random.Range(0, Pieces.Count);
            var randomPiece = Pieces[randomIndex];

            var pieceColor = randomPiece.Color;

            // set renderer
            entityManager.SetSharedComponentData(currentEntity, new RenderMesh
            {
                mesh = PieceMesh,
                material = randomPiece.Material
            });

            var piecePosition = new Vector3(x * SlotPhysicalSpace, y * SlotPhysicalSpace, 0);

            // set world coordinates
            entityManager.SetComponentData(currentEntity, new Translation { Value = piecePosition });

            // set piece color
            entityManager.SetComponentData(currentEntity, new ChipColorComponent
            {
                Color = pieceColor
            });

            // set piece position
            entityManager.SetComponentData(currentEntity, new ChipPositionComponent
            {
                X = x,
                Y = y
            });

            // set gravity
            entityManager.SetComponentData(currentEntity, new GravityComponent
            {
                FallingSpeed = PieceFallingSpeed,
                Falling = false
            });

            // add created pieces to array of pieces
            BoardPieces[x, y] = pieceColor;
            PieceColors[i] = pieceColor;
        }

        createdEntities.Dispose();


        // todo: call this after board settle
        ScheduleBoardMatchingJob();
    }


    void ScheduleBoardMatchingJob()
    {
        var boardPieces = new NativeArray<PieceColor>(PieceColors, Allocator.Persistent);

        var matches = new NativeArray<int>(PieceColors.Length, Allocator.Persistent);

        var horizontalMatchesJob = new FindHorizontalMatchesJob
        {
            Board = boardPieces,
            SlotsPerRow = BoardWidth,
            Matches = matches
        };

        JobHandle jobHandle = horizontalMatchesJob.Schedule(PieceColors.Length, BoardWidth);

        jobHandle.Complete();

        //PrintMatches(matches);

        matches.Dispose();

        boardPieces.Dispose();
    }

    private void PrintMatches(NativeList<int> matches)
    {
        foreach(var match in matches)
        {
            Debug.Log("Match found" + match);
        }
    }
}
