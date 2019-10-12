using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;

public class TestingEntity : MonoBehaviour
{
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    // Start is called before the first frame update
    void Start()
    {
        EntityManager entityManager = World.Active.EntityManager;


        EntityArchetype entityArchetype = entityManager.CreateArchetype
        (
            typeof(LevelComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MoveSpeedComponent)
        );


        NativeArray<Entity> entityArray = new NativeArray<Entity>(5, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            entityManager.SetComponentData(entity, new LevelComponent { Level =  i * 10 });
            entityManager.SetComponentData(entity, new MoveSpeedComponent { Speed = Random.RandomRange(1.0f, 2.0f)});
            entityManager.SetComponentData(entity, new Translation
            {
                Value = new Vector3(Random.Range(-8, 8f), Random.Range(-4f, 4f), 0)
            });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = _mesh,
                material = _material
            });
        }

        entityArray.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
