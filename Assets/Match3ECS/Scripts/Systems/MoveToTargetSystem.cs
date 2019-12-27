using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Assertions.Must;
using SharedCore;

public partial class SystemEvents
{
    public const string OnMoveToTargetSystemStartedRunning = "MoveToTargetOnStartRunning";
    public const string OnMoveToTargetSystemStopedRunning = "MoveToTargetOnStopRunning";
}

public class MoveToTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Translation translation, ref MoveToTargetComponent moveToTargetComponent) =>
        {
            Translation targetTranslation = World.Active.EntityManager.GetComponentData<Translation>(moveToTargetComponent.Target);

            float3 targetDirection = math.normalize(targetTranslation.Value - translation.Value);
            float moveSpeed = 1f;

            translation.Value += targetDirection * moveSpeed * Time.deltaTime;

            if (math.distance(translation.Value, targetTranslation.Value) <= 0.01f)
            {
                PostUpdateCommands.RemoveComponent(entity, typeof(MoveToTargetComponent));
            } 
        });
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        Messenger.Broadcast(SystemEvents.OnMoveToTargetSystemStartedRunning);
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
        Messenger.Broadcast(SystemEvents.OnMoveToTargetSystemStopedRunning);
    }
}
