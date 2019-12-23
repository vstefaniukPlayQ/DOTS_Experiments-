using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Assertions.Must;

public class MoveToTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Translation translation, ref MoveToTargetComponent moveToTargetComponent) =>
        {
            Translation targetTranslation =
                World.Active.EntityManager.GetComponentData<Translation>(moveToTargetComponent.Target);

            float3 targetDirection = math.normalize(targetTranslation.Value - translation.Value);
            float moveSpeed = 1f;
            translation.Value += targetDirection * moveSpeed * Time.deltaTime;

            if (math.distance(translation.Value, targetTranslation.Value) <= 0.01f)
            {
                PostUpdateCommands.RemoveComponent(entity, typeof(MoveToTargetComponent));
            } 
        });
    }
}
