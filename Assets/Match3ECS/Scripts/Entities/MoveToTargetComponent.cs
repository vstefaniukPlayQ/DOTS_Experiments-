using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

 public struct MoveToTargetComponent : IComponentData
 {
     public Entity Target;
 }