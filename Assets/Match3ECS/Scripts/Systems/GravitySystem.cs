using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

// todo : convert to job system
public class GravitySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref SlotEntity slotEntity) =>
        {
            /*
            if (gravity.Falling)
                translation.Value.y -= Time.deltaTime * gravity.FallingSpeed;
            */

            if (slotEntity.m_Chip == Entity.Null)
            {
                
            } 
        });
    }
}
