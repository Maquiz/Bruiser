using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.PerformanceTesting;

namespace Unity.Entities.PerformanceTests
{
    [Category("Performance")]
    public class NativeArrayIterationPerformanceTests
    {
        AllocatorHelper<RewindableAllocator> m_AllocatorHelper;
        protected ref RewindableAllocator RwdAllocator => ref m_AllocatorHelper.Allocator;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            m_AllocatorHelper = new AllocatorHelper<RewindableAllocator>(Allocator.Persistent);
            m_AllocatorHelper.Allocator.Initialize(128 * 1024, true);
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            m_AllocatorHelper.Allocator.Dispose();
            m_AllocatorHelper.Dispose();
        }

        [TearDown]
        public virtual void TearDown()
        {
            RwdAllocator.Rewind();
            // This is test only behavior for determinism.  Rewind twice such that all
            // tests start with an allocator containing only one memory block.
            RwdAllocator.Rewind();
        }

        [BurstCompile(CompileSynchronously = true)]
        struct AddDeltaAndReset : IJobParallelFor
        {
            public NativeArray<int> Source;
            public int Delta;
            public int ResetThreshold;

            public void Execute(int index)
            {
                var projectedValue = Source[index] + Delta;
                Source[index] = math.@select(0, projectedValue, projectedValue < ResetThreshold);
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        unsafe struct AddDeltaAndResetPtr : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction]
            public int* Source;
            public int Delta;
            public int ResetThreshold;

            public void Execute(int index)
            {
                var projectedValue = Source[index] + Delta;
                Source[index] = math.@select(0, projectedValue, projectedValue < ResetThreshold);
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        struct AddDelta : IJobParallelFor
        {
            public NativeArray<int> Source;
            public int Delta;

            public void Execute(int index)
            {
                var projectedValue = Source[index] + Delta;
                Source[index] = projectedValue;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        struct Reset : IJobParallelFor
        {
            public NativeArray<int> Source;
            public int ResetThreshold;

            public void Execute(int index)
            {
                var value = Source[index];
                Source[index] = math.@select(0, value, value < ResetThreshold);
            }
        }

        void SingleIterationWork(NativeArray<int> source, int delta, int resetThreshold, int batchSize)
        {
            var addDeltaAndResetJob = new AddDeltaAndReset
            {
                Source = source,
                Delta = delta,
                ResetThreshold = resetThreshold
            };
            var addDeltaAndResetJobHandle = addDeltaAndResetJob.Schedule(source.Length, batchSize);
            addDeltaAndResetJobHandle.Complete();
        }

        unsafe void SingleIterationWorkPtr(NativeArray<int> source, int delta, int resetThreshold, int batchSize)
        {
            var sourcePtr = (int*)source.GetUnsafePtr();
            var addDeltaAndResetJob = new AddDeltaAndResetPtr
            {
                Source = sourcePtr,
                Delta = delta,
                ResetThreshold = resetThreshold
            };
            var addDeltaAndResetJobHandle = addDeltaAndResetJob.Schedule(source.Length, batchSize);
            addDeltaAndResetJobHandle.Complete();
        }

        void SplitIterationWork(NativeArray<int> source, int delta, int resetThreshold, int batchSize)
        {
            var addDeltaJob = new AddDelta
            {
                Source = source,
                Delta = delta
            };
            var addDeltaJobHandle = addDeltaJob.Schedule(source.Length, batchSize);
            var resetJob = new Reset
            {
                Source = source,
                ResetThreshold = resetThreshold
            };
            var resetJobHandle = resetJob.Schedule(source.Length, batchSize, addDeltaJobHandle);
            resetJobHandle.Complete();
        }

        [Test, Performance]
        public void NAI_SingleVsSplitIterationJob()
        {
            var count = 10 * 1024 * 1024;
            var source = CollectionHelper.CreateNativeArray<int>(count, RwdAllocator.ToAllocator);
            var delta = 1;
            var resetThreshold = 1;
            int batchSize = 1024;

            Measure.Method(() => { SingleIterationWork(source, delta, resetThreshold, batchSize); })
                .SampleGroup("SingleIteration")
                .WarmupCount(1)
                .MeasurementCount(100)
                .Run();

            Measure.Method(() => { SingleIterationWorkPtr(source, delta, resetThreshold, batchSize); })
                .SampleGroup("SingleIterationPtr")
                .WarmupCount(1)
                .MeasurementCount(100)
                .Run();

            Measure.Method(() => { SplitIterationWork(source, delta, resetThreshold, batchSize); })
                .SampleGroup("SplitIteration")
                .WarmupCount(1)
                .MeasurementCount(100)
                .Run();

            source.Dispose();
        }
    }
}
