using System;
using System.Collections.Generic;
using SharedCore;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public enum SwipeDirection
{
    LEFT,
    UP,
    RIGHT,
    DOWN
}

public class PieceControlSystem : ComponentSystem
{
    public static string OnChipSwipeRequested = "OnChipSwiped";

    public static Dictionary<SwipeDirection, Vector2Int> SwipeDirectionToVector2 = new Dictionary<SwipeDirection, Vector2Int>
    {
        { SwipeDirection.LEFT, Vector2Int.left},
        { SwipeDirection.RIGHT, Vector2Int.right},
        { SwipeDirection.UP, Vector2Int.up},
        { SwipeDirection.DOWN, Vector2Int.down}
    };

    private float3 startPosition;
    private float3 endPosition;

    public float chipHeigth = .5f;
    public float chipWidth = .5f;

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //
            startPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        }
        else  if (Input.GetMouseButtonUp(0))
        {
            endPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));


            var direction = endPosition - startPosition;

            var swipeDirection = DetectSwipeDirection(direction);

            var swipeVector = SwipeDirectionToVector2[swipeDirection];

            Entities.ForEach((Entity entity, ref Translation translation, ref SlotEntity slotEntity, ref SlotPosition slotPosition) =>
            {
                float3 entityPosition = translation.Value;

                if (
                    entityPosition.x >= startPosition.x - chipWidth &&
                    entityPosition.x <= startPosition.x + chipWidth &&
                    entityPosition.y >= startPosition.y - chipHeigth &&
                    entityPosition.y <= startPosition.y + chipHeigth
                    )
                {
                    // Entity inside selection area
                    Debug.LogFormat("<color=green>" + entity + "</color>");

                    // PostUpdateCommands.AddComponent(entity, new ChipSwipeComponent { SwipeDirection = swipeDirection });

                    Messenger.Broadcast(OnChipSwipeRequested, slotPosition, swipeVector);
                }
            });

            Debug.Log(startPosition + " " + endPosition + " swipe direction: " + swipeDirection);
        }
    }

    public SwipeDirection DetectSwipeDirection(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            // horizontal swipe
        {
            if (direction.x > 0)
                return SwipeDirection.RIGHT;
            return SwipeDirection.LEFT;
        }
        else
            // vertical swipe
        {
            if (direction.y > 0)
                return SwipeDirection.UP;
            return SwipeDirection.DOWN;
        }
    }
}
