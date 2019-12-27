using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ChipSwipeSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref ChipSwipeComponent swipeComponent) =>
        {

            // check if we can move in the desired directon

            // get chip/slot for desired move position

            // add move to component for each of them

            // when animation has finished trigger find mathes code

            // if match has been found then do nothing,

            // if match has not been found then play reverse animation


        });
    }
}
