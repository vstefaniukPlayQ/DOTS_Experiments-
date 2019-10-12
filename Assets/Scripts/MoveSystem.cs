using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Assertions.Must;

public class MoveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref MoveSpeedComponent moveSpeedComponent) =>
        {
            translation.Value.y += moveSpeedComponent.Speed * Time.deltaTime;
            if (translation.Value.y > 5f)
            {
                moveSpeedComponent.Speed = -math.abs(moveSpeedComponent.Speed);
            }
            if (translation.Value.y < -5f)
            {
                moveSpeedComponent.Speed = +math.abs(moveSpeedComponent.Speed);
            }

        });
    }
}
