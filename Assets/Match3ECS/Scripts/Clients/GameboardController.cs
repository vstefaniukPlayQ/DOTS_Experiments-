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
    [Space, Header("Pieces"), Space]
    public Mesh PieceMesh;

    public List<PieceMaterialByColor> Pieces;

    // todo: it is just a array for the simplicity and fast access,
    // not quite sure if it need to be separate entity or something
    // also we don't care to much about other data except of PieceColor
    private PieceColor[,] BoardPieces;
    private PieceColor[] PieceColors;

    [Space, Header("Board"), Space]
    public Mesh SlotMesh;
    public Material SlotMaterial;
    public int SlotLayer = 8;

    [Space, Header("Properties"), Space]
    public int BoardWidth = 8;
    public int BoardHeight = 8;

    public float SlotPhysicalSpace = 1;

    public float PieceFallingSpeed = 9.8f;
    public int GamePieceLayer = 9;

    #region "ECS variables"

    private EntityManager _entityManager;

    private NativeArray<Entity> _generatedChips = new NativeArray<Entity>();
    private NativeArray<Entity> _generatedSlots = new NativeArray<Entity>();

    #endregion

    protected void Start()
    {
        _entityManager = World.Active.EntityManager;

        // game piece (aka crystal, fruit, honey, etc.)
        EntityArchetype chipArchetype = _entityManager.CreateArchetype
        (
typeof(ChipColorComponent),
            typeof(ChipPositionComponent),
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(GravityComponent),
            typeof(ChipEntity)
        );

        // slot
        EntityArchetype slotArchetype = _entityManager.CreateArchetype
        (
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(SlotPosition),
            typeof(SlotEntity)
        );

        int chipsAmount = BoardHeight * BoardWidth;
        int slotsAmount = BoardHeight * BoardWidth;

        _generatedChips = new NativeArray<Entity>(chipsAmount, Allocator.Persistent);
        _generatedSlots = new NativeArray<Entity>(chipsAmount, Allocator.Persistent);

        _entityManager.CreateEntity(chipArchetype, _generatedChips);
        _entityManager.CreateEntity(slotArchetype, _generatedSlots);

        BoardPieces = new PieceColor[BoardWidth, BoardHeight];
        PieceColors = new PieceColor[BoardHeight * BoardWidth];

        // generating slots

        for (int i = 0; i < slotsAmount; i++)
        {
            var currentEntity = _generatedSlots[i];

            var x = i % BoardWidth;
            var y = (int) Mathf.Floor(i / (float) BoardHeight);

            // set renderer
            _entityManager.SetSharedComponentData(currentEntity, new RenderMesh
            {
                mesh = SlotMesh,
                material = SlotMaterial,
                layer = SlotLayer
            });

            var coordinatePosition = new Vector3(x * SlotPhysicalSpace, y * SlotPhysicalSpace, 0);

            // set world coordinates
            _entityManager.SetComponentData(currentEntity, new Translation { Value = coordinatePosition });

            // set piece position
            _entityManager.SetComponentData(currentEntity, new SlotPosition
            {
                X = x,
                Y = y
            });
        }

        // generating chips

        for (int i = 0; i < chipsAmount; i++)
        {
            var currentEntity = _generatedChips[i];
            var slot = _generatedSlots[i];

            var x = i % BoardWidth;
            var y = (int) Mathf.Floor(i / (float) BoardHeight);

            var randomIndex = UnityEngine.Random.Range(0, Pieces.Count);
            var randomPiece = Pieces[randomIndex];

            var pieceColor = randomPiece.Color;

            // set renderer
            _entityManager.SetSharedComponentData(currentEntity, new RenderMesh
            {
                mesh = PieceMesh,
                material = randomPiece.Material,
                layer = GamePieceLayer
            });

            var piecePosition = new Vector3(x * SlotPhysicalSpace, y * SlotPhysicalSpace, 0);

            // set world coordinates
            _entityManager.SetComponentData(currentEntity, new Translation { Value = piecePosition });

            // set piece color
            _entityManager.SetComponentData(currentEntity, new ChipColorComponent
            {
                Color = pieceColor
            });

            // set piece position
            _entityManager.SetComponentData(currentEntity, new ChipPositionComponent
            {
                X = x,
                Y = y
            });

            // set gravity
            _entityManager.SetComponentData(currentEntity, new GravityComponent
            {
                FallingSpeed = PieceFallingSpeed,
                Falling = false
            });


            // add created pieces to array of pieces
            BoardPieces[x, y] = pieceColor;
            PieceColors[i] = pieceColor;
        }
 
        // assign chips to slots
        for (int i = 0; i < _generatedSlots.Length && i < _generatedChips.Length; i++)
        {
            var chip = _generatedChips[i];
            var slot = _generatedSlots[i];
            _entityManager.SetComponentData(slot, new SlotEntity()
            {
                m_Chip = chip
            });
        }

        // todo: call this after board settle
        ScheduleBoardMatchingJob();

        
        FallChipsToColumnBellow(new HashSet<int>());
        
        Test();
    }

    private void OnDestroy()
    {
        _generatedChips.Dispose();
        _generatedSlots.Dispose();
    }

    void ScheduleBoardMatchingJob()
    {
        var boardPieces = new NativeArray<PieceColor>(PieceColors, Allocator.Persistent);

        var horizontalMatchesResult = new NativeArray<int>(PieceColors.Length, Allocator.TempJob);

        var horizontalMatchesJob = new FindHorizontalMatchesJob
        {
            Board = boardPieces,
            SlotsPerRow = BoardWidth,
            Output = horizontalMatchesResult
        };

        JobHandle jobHandle = horizontalMatchesJob.Schedule(PieceColors.Length, BoardWidth);

        jobHandle.Complete();

        PrintMatches(horizontalMatchesResult);

        horizontalMatchesResult.Dispose();

        boardPieces.Dispose();
    }

    private void PrintMatches(NativeArray<int> matches)
    {
        bool matchHasBeenFound = false;
        List<int> chipsToDestroy = new List<int>();

        int index = 0;
        foreach(var match in matches)
        {
            if(match > 0)
            {
                Debug.Log("Match found " + match);
                matchHasBeenFound = true;
                chipsToDestroy.Add(index);
            }
            index++;
        }

        if(matchHasBeenFound)
        {
            Debug.LogFormat("<color=green> ==================== </color>");
            Debug.LogFormat("<color=green> Match has been found!</color>");
            Debug.LogFormat("<color=green> ==================== </color>");

            DestroyPieces(chipsToDestroy);
        }
    }

    private void DestroyPieces(List<int> chipsToDestroy)
    {
        HashSet<int> columnToUpdate = new HashSet<int>();
        
        foreach (var chip in chipsToDestroy)
        {
            var chipToDestroy = _generatedChips[chip];
            var slotWithChip = _generatedSlots[chip];

            _entityManager.DestroyEntity(chipToDestroy);
            _entityManager.SetComponentData(slotWithChip, new SlotEntity
            {
                m_Chip = Entity.Null
            });

            columnToUpdate.Add(chip % BoardWidth);
        }

        //FallChipsToColumnBellow(columnToUpdate);
    }

    private void Test()
    {
        int height = 9;
        int width = 9;
        
        for (int i = height; i >= 0; i--)
        {
            for (int j = width - 1; j >= 0; j--)
            {
                Debug.Log((i + j * width - 1) + " ");
            }
        }
    }

    private void FallChipsToColumnBellow(HashSet<int> columnToUpdate)
    {
        for (int i = 0; i < BoardHeight; i++)
        { 
            // rowIndex * numberOfColumns + columnIndex.
            int previousSlotIndex = i;

            for (int j = 0; j < BoardWidth; j++)
            {
                // rowIndex * numberOfColumns + columnIndex.
                int index =  (j * BoardHeight) + i;
                
                Entity currentSlotEntity = _generatedSlots[index];
                Entity previousSlotEntity = _generatedSlots[previousSlotIndex];

                SlotEntity currentSlot =  _entityManager.GetComponentData<SlotEntity>(currentSlotEntity);
                SlotEntity previousSlot =  _entityManager.GetComponentData<SlotEntity>(previousSlotEntity);

                if (previousSlot.m_Chip == Entity.Null && currentSlot.m_Chip != Entity.Null)
                {
                    var fallingChip = currentSlot.m_Chip;

                    _entityManager.SetComponentData(currentSlotEntity,  new SlotEntity { m_Chip = Entity.Null });

                    _entityManager.SetComponentData(previousSlotEntity, new SlotEntity { m_Chip = fallingChip });

                    _entityManager.AddComponent(fallingChip, typeof(MoveToTargetComponent));
                    _entityManager.SetComponentData(fallingChip, new MoveToTargetComponent
                    {
                        Target = previousSlotEntity
                    });

                    previousSlotIndex = index;
                    Debug.Log("Adding move to target component");
                }

                if(previousSlot.m_Chip != Entity.Null)
                    previousSlotIndex = index;
            }
        }

    }
}


/// todo: destroy matched pieces
/// set all pieces above to falling state
/// repeat
