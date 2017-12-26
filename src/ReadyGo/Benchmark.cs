namespace ReadyGo
{
    using System;
    using System.Runtime.CompilerServices;

    public interface IBenchmark
    {
        string Name
        {
            get;
        }
        void Setup();
        void Cleanup();
        void Go();
    }

    public abstract class BenchmarkBase : IBenchmark
    {
        public abstract string Name
        {
            get;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void Setup()
        {
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void Cleanup()
        {
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public abstract void Go();
    }
}