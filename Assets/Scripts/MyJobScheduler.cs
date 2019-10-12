using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class MyJobScheduler : MonoBehaviour
{

#region "UnityEvents"

    void Awake()
    {
        ScheduleTestJob();
        ScheduleParallelTestJobs();
    }

    void Update()
    {

    }

#endregion

    void ScheduleTestJob()
    {
        // Create a native array of a single float to store the result. This example waits for the job to complete for illustration purposes
        NativeArray <float> result = new NativeArray<float>(1, Allocator.TempJob);

        MyJob tempJob = new MyJob();
        tempJob.a = 10;
        tempJob.b = 10;
        tempJob.result = result;

        JobHandle tempJobHandle = tempJob.Schedule();

        // Setup the data for job #2
        AddOneJob addOneJob = new AddOneJob();
        addOneJob.result = result;

        JobHandle secondJobHandle = addOneJob.Schedule(tempJobHandle);

        secondJobHandle.Complete();

        float resultValue = result[0];
        Debug.Log("Result of the myJob: " + resultValue);

        result.Dispose();
    }


    void ScheduleParallelTestJobs()
    {
        var positions = new NativeArray<Vector3>(500, Allocator.Persistent);

        var velocities = new NativeArray<Vector3>(500, Allocator.Persistent);

        for(var i = 0; i < velocities.Length; i++)
            velocities[i] = new Vector3(0, 10, 0);

        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            position = positions,
            velocity = velocities
        };

        JobHandle jobHandle = job.Schedule(positions.Length, 64);

        StartCoroutine(WaitForJobToBeFinished(jobHandle,
            () =>
            {
                jobHandle.Complete();

                Debug.Log(job.position[0]);

                positions.Dispose();
                velocities.Dispose();
            }
            ));

    }

    private IEnumerator WaitForJobToBeFinished(JobHandle jobHandle, Action onComplete)
    {
        while (!jobHandle.IsCompleted)
            yield return null;

        onComplete();
    }
}
