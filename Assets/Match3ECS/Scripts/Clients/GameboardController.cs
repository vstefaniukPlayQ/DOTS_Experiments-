using System;
using System.Collections.Generic;
using System.Linq;
using SharedCore;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public enum PieceColor
{
    Undefined = - 1,
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

    [Space, Header("Board"), Space]
    public Mesh SlotMesh;
    public Material SlotMaterial;
    public int SlotLayer = 8;

    [Space, Header("Properties"), Space]
    public int BoardWidth = 8;
    public int BoardHeight = 8;
    public int BoardSize
    {
        get { return BoardHeight * BoardWidth; }
    }

    public float SlotPhysicalSpace = 1;

    public float PieceFallingSpeed = 9.8f;
    public int GamePieceLayer = 9;

    #region "ECS variables"

    private EntityManager _entityManager;

    private NativeArray<Entity> _generatedSlots;

    #endregion

    private bool IsChipsMoving;

    protected void Start()
    {
        _entityManager = World.Active.EntityManager;

        GenerateBoard();
    }
    private void OnDestroy()
    {
        _generatedSlots.Dispose();
    }

    public void OnEnable()
    {
        Messenger.AddListener<SlotPosition, Vector2Int>(PieceControlSystem.OnChipSwipeRequested, OnChipSwipeRequested);

        Messenger.AddListener(SystemEvents.OnMoveToTargetSystemStartedRunning, OnMoveToTargetSystemStartedRunning);
        Messenger.AddListener(SystemEvents.OnMoveToTargetSystemStopedRunning, OnMoveToTargetSystemStopedRunning);
    }

    public void OnDisable()
    {
        Messenger.RemoveListener<SlotPosition, Vector2Int>(PieceControlSystem.OnChipSwipeRequested, OnChipSwipeRequested);

        Messenger.RemoveListener(SystemEvents.OnMoveToTargetSystemStartedRunning, OnMoveToTargetSystemStartedRunning);
        Messenger.RemoveListener(SystemEvents.OnMoveToTargetSystemStopedRunning, OnMoveToTargetSystemStopedRunning);
    }

 #region "Board ECS generation code"
    private void GenerateBoard()
    {
        _generatedSlots = GenerateSlots(BoardSize);
        NativeArray<Entity> generatedChips = GenerateChips(BoardSize);
        AssociateChipsWithSlots
        (
            slots : _generatedSlots,
            chips : generatedChips
            );
        generatedChips.Dispose();
    }

    private NativeArray<Entity> GenerateSlots(int slotsAmount)
    {
        // slot
        EntityArchetype slotArchetype = _entityManager.CreateArchetype
        (
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(SlotPosition),
            typeof(SlotEntity)
        );

        var generatedSlots = new NativeArray<Entity>(slotsAmount, Allocator.Persistent);
        _entityManager.CreateEntity(slotArchetype, generatedSlots);

        // generating slots
        for (int i = 0; i < slotsAmount; i++)
        {
            var currentEntity = generatedSlots[i];

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
            _entityManager.SetComponentData(currentEntity, new Translation {Value = coordinatePosition});

            // set piece position
            _entityManager.SetComponentData(currentEntity, new SlotPosition
            {
                X = x,
                Y = y
            });
        }

        return generatedSlots;
    }

    private NativeArray<Entity> GenerateChips(int chipsAmount)
    {
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

        NativeArray<Entity> generatedChips = new NativeArray<Entity>(chipsAmount, Allocator.Temp);

        _entityManager.CreateEntity(chipArchetype, generatedChips);

        // generating chips

        for (int i = 0; i < chipsAmount; i++)
        {
            var currentEntity = generatedChips[i];

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
            _entityManager.SetComponentData(currentEntity, new Translation {Value = piecePosition});

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
        }

        return generatedChips;
    }

    private void AssociateChipsWithSlots(NativeArray<Entity> slots, NativeArray<Entity> chips)
    {
        // assign chips to slots
        for (int i = 0; i < slots.Length && i < slots.Length; i++)
        {
            var chip = chips[i];
            var slot = slots[i];

            _entityManager.SetComponentData(slot, new SlotEntity()
            {
                m_Chip = chip
            });
        }
    }


#endregion

    private void OnMoveToTargetSystemStartedRunning()
    {
        // disable input
        IsChipsMoving = true;
    }

    private void OnMoveToTargetSystemStopedRunning()
    {
        // enable input
        IsChipsMoving = false;

        // Core logic
        ScheduleBoardMatchingJob();
    }

    private void OnChipSwipeRequested(SlotPosition slotPosition, Vector2Int direction)
    {
        if (IsChipsMoving)
            return;

        DoSlotChipSwapWithAnimation(slotPosition, direction);

        // when animation has finished trigger find mathes code

        // if match has been found then do nothing,

        // if match has not been found then play reverse animation

    }

    private void DoSlotChipSwapWithAnimation(SlotPosition slotPosition, Vector2Int direction)
    {
        Debug.LogFormat("<color=green> OnChipSwiped </color>");

        // check if we can move in the desired direction
        var moveToX = slotPosition.X + direction.x;
        var moveToY = slotPosition.Y + direction.y;

        if (moveToX < 0 || moveToX >= BoardWidth)
        {
            Debug.LogFormat("<color=yellow> OnChipSwiped - Swiped outside the board </color>");
            return;
        }

        if (moveToY < 0 || moveToY >= BoardHeight)
        {
            Debug.LogFormat("<color=yellow> OnChipSwiped - Swiped outside the board </color>");
            return;
        }

        // get chip/slot for desired move position
        var moveToSlotIndex = moveToX + moveToY * BoardWidth;
        var moveFromSlotIndex = slotPosition.X + slotPosition.Y * BoardWidth;


        var moveToSlot = _generatedSlots[moveToSlotIndex];
        var moveFromSlot = _generatedSlots[moveFromSlotIndex];

        var moveToSlotEntity = _entityManager.GetComponentData<SlotEntity>(moveToSlot);
        var moveFromSlotEntity = _entityManager.GetComponentData<SlotEntity>(moveFromSlot);

        var moveToChip = moveToSlotEntity.m_Chip;
        var moveFromChip = moveFromSlotEntity.m_Chip;

        // add move to component for each of them


        _entityManager.SetComponentData(moveToSlot,  new SlotEntity { m_Chip = moveFromChip });
        _entityManager.SetComponentData(moveFromSlot,  new SlotEntity { m_Chip = moveToChip });


        // make chips move
        if(!_entityManager.HasComponent<MoveToTargetComponent>(moveToChip))
            _entityManager.AddComponent(moveToChip, typeof(MoveToTargetComponent));
        else
            Debug.LogError($"{nameof(MoveToTargetComponent)} Component already exist");
        _entityManager.SetComponentData(moveToChip, new MoveToTargetComponent
        {
            Target = moveFromSlot
        });

        if(!_entityManager.HasComponent<MoveToTargetComponent>(moveFromChip))
            _entityManager.AddComponent(moveFromChip, typeof(MoveToTargetComponent));
        else
            Debug.LogError($"{nameof(MoveToTargetComponent)} Component already exist");
        _entityManager.SetComponentData(moveFromChip, new MoveToTargetComponent
        {
            Target = moveToSlot
        });
    }

    void ScheduleBoardMatchingJob()
    {
        PieceColor[] pieceColors = new PieceColor[_generatedSlots.Length];
        for (int i = 0; i < _generatedSlots.Length; i++)
        {
            pieceColors[i] = PieceColor.Undefined;
            if (_generatedSlots[i] != Entity.Null)
            {
                var slotEntity = _entityManager.GetComponentData<SlotEntity>(_generatedSlots[i]);
                var chip = slotEntity.m_Chip;
                if (chip != Entity.Null)
                {
                    var chipColor = _entityManager.GetComponentData<ChipColorComponent>(chip);
                    pieceColors[i] = chipColor.Color;
                }
            }
        }

        var boardPieces = new NativeArray<PieceColor>(pieceColors, Allocator.TempJob);

        // ==============================================================
        //                         Horizontal matches
        // ==============================================================

        var horizontalMatchesResult = new NativeArray<int>(BoardSize, Allocator.TempJob);
        var horizontalMatchesJob = new FindHorizontalMatchesJob
        {
            Board = boardPieces,
            SlotsPerRow = BoardWidth,
            Output = horizontalMatchesResult
        };
        JobHandle jobHandle = horizontalMatchesJob.Schedule(BoardSize, BoardWidth);
        jobHandle.Complete();

        var horizontalMatches = horizontalMatchesResult.ToArray();

        // ==============================================================
        //                         Vertical matches
        // ==============================================================

        var verticalMatchesResult = new NativeArray<int>(BoardSize, Allocator.TempJob);
        var verticalMatchesJob = new FindVerticalMatchesJob
        {
            Board = boardPieces,
            SlotsPerColumn = BoardHeight,
            Output = verticalMatchesResult
        };

        JobHandle verticaljobHandle = verticalMatchesJob.Schedule(BoardSize, BoardHeight);
        verticaljobHandle.Complete();

        var verticallMatches = verticalMatchesResult.ToArray();

        // ==============================================================
        //  Find union of two matched collection and proceed to destroy
        // ==============================================================

        var verticalMatchedChips   = GetChipsToDestroyFromMatchesMatrix(verticallMatches);
        var horizontalMatchedChips = GetChipsToDestroyFromMatchesMatrix(horizontalMatches);
        var mergedMatchedChips = verticalMatchedChips.Union(horizontalMatchedChips).ToList();

        horizontalMatchesResult.Dispose();
        verticalMatchesResult.Dispose();

        boardPieces.Dispose();

        DestroyPieces(mergedMatchedChips);
    }

    private List<int> GetChipsToDestroyFromMatchesMatrix(int[] matches)
    {
        var chipsToDestroy = new List<int>();
        int index = 0;
        foreach(var match in matches)
        {
            if(match > 0)
            {
                Debug.Log("Match found " + match);
                chipsToDestroy.Add(index);
            }
            index++;
        }
        return chipsToDestroy;
    }

    private void DestroyPieces(List<int> chipsToDestroy)
    {
        HashSet<int> columnToUpdate = new HashSet<int>();
        
        foreach (var chip in chipsToDestroy)
        {
            var slotWithChip = _generatedSlots[chip];
            var chipToDestroy = _entityManager.GetComponentData<SlotEntity>(slotWithChip).m_Chip;

            _entityManager.DestroyEntity(chipToDestroy);
            _entityManager.SetComponentData(slotWithChip, new SlotEntity
            {
                m_Chip = Entity.Null
            });

            columnToUpdate.Add(chip % BoardWidth);
        }

        FallChipsToColumnBellow(columnToUpdate);
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
                int index =  (j * BoardWidth) + i;
                
                Entity currentSlotEntity = _generatedSlots[index];
                Entity previousSlotEntity = _generatedSlots[previousSlotIndex];

                SlotEntity currentSlot =  _entityManager.GetComponentData<SlotEntity>(currentSlotEntity);
                SlotEntity previousSlot =  _entityManager.GetComponentData<SlotEntity>(previousSlotEntity);

                if (previousSlot.m_Chip == Entity.Null && currentSlot.m_Chip != Entity.Null)
                {
                    var fallingChip = currentSlot.m_Chip;

                    _entityManager.SetComponentData(currentSlotEntity,  new SlotEntity { m_Chip = Entity.Null });

                    _entityManager.SetComponentData(previousSlotEntity, new SlotEntity { m_Chip = fallingChip });

                    if(!_entityManager.HasComponent<MoveToTargetComponent>(fallingChip))
                        _entityManager.AddComponent(fallingChip, typeof(MoveToTargetComponent));
                    else
                        Debug.LogError($"{nameof(MoveToTargetComponent)} Component already exist");
                    _entityManager.SetComponentData(fallingChip, new MoveToTargetComponent
                    {
                        Target = previousSlotEntity
                    });

                    previousSlotIndex += BoardWidth;
                    Debug.Log("Adding move to target component");
                }
                else if(previousSlot.m_Chip != Entity.Null)
                    previousSlotIndex = index;
            }
        }
    }

    public static int TwoDimentionToOneDimention(int x, int y, int width)
    {
        // rowIndex * numberOfColumns + columnIndex.
        return (x * width) + y;
    }
}


/// todo: destroy matched pieces
/// set all pieces above to falling state
/// repeat
