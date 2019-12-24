using System;
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

            Entities.ForEach((Entity entity, ref Translation translation, ref ChipEntity chip) =>
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

                    PostUpdateCommands.DestroyEntity(entity);
                }
            });

            Debug.Log(startPosition + " " + endPosition + " swipe direction: " + swipeDirection);
        }
    }

    public SwipeDirection DetectSwipeDirection(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))  // horizontal
        {
            if (direction.x > 0)
                return SwipeDirection.RIGHT;
            return SwipeDirection.LEFT;
        }
        else // vertical
        {
            if (direction.y > 0)
                return SwipeDirection.UP;
            return SwipeDirection.DOWN;
        }
    }
}
