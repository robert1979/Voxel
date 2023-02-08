using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class JobTest : MonoBehaviour
{
    public bool useJobs;

    private void Update()
    {
        var t = Time.realtimeSinceStartup;
        if (!useJobs)
        {
            for (int i = 0; i < 10; i++)
            {
                ToughJob();
            }
        }
        else
        {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            for (int i = 0; i < 10; i++)
            {
                var job = new HardJob();
                var handle = job.Schedule();
                jobHandleList.Add(handle);
            }
            JobHandle.CompleteAll(jobHandleList);
            jobHandleList.Dispose();
        }

        var eTime = (Time.realtimeSinceStartup - t)*1000f;
        Debug.Log(eTime + "ms");
    }
    
    
    [BurstCompile]
    private struct HardJob : IJob
    {
        public void Execute()
        {
            for (int i = 0; i < 50000; i++)
            {
                var x =math.exp10(math.sqrt(2));
            }
        }
    }


    private void ToughJob()
    {
        for (int i = 0; i < 50000; i++)
        {
            var x =math.exp10(math.sqrt(2));
        }
    }
}



