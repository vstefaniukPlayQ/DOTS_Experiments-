using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public static class JobHelpers
{
    public static IEnumerator WaitForJobToBeFinished(this JobHandle jobHandle, Action onComplete)
    {
        while (!jobHandle.IsCompleted)
            yield return null;

        onComplete();
    }
}
