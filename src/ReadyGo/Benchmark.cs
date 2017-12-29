using System;
using System.Runtime.CompilerServices;

namespace ReadyGo
{
    /// <summary>
    /// The core interface for a benchmark.
    /// </summary>
    /// <remarks>
    /// <para>Implement this interface (or derive from 
    /// <see cref="BenchmarkBase" />) to implement a benchmark. Each run, your
    /// benchmark:</para>
    /// <list type="number">
    ///     <item>
    ///         <description>Will have it's <see cref="Setup" /> method called
    ///         once.</description>
    ///     </item>
    ///     <item>
    ///         <description>Will have it's <see cref="Go" /> method called as
    ///         many times as necessary in order to meet the minimum runtime
    ///         requirement.</description>
    ///     </item>
    ///     <item>
    ///         <description>Will have it's <see cref="Cleanup" /> method called
    ///         once.</description>
    ///     </item>
    /// </list>
    /// <para>Because your <see cref="Go" /> method will be called in a loop, 
    /// you can have it perform as little work as you'd like-- the runtime will
    /// make sure it gets measured appropriately. The downside of this is that 
    /// the <see cref="Go" /> method must actually make sense in between a 
    /// single pair of <see cref="Setup" /> and <see cref="Cleanup" /> methods.
    /// This is normally not a problem, since a fast <see cref="Go" /> method
    /// should not have so many side-effects that it needs <see cref="Setup" />
    /// or <see cref="Cleanup" /> work done in-between.
    /// </remarks>
    public interface IBenchmark
    {
        /// <summary>
        /// Gets the name of the benchmark.
        /// </summary>
        /// <returns>The name of the benchmark.</returns>
        /// <remarks>This should be short, fit on one line, and succinctly
        /// describe what the benchmark is measuring. e.g., "Formatting strings"
        /// </remarks>
        string Name
        {
            get;
        }
        /// <summary>
        /// Performs any necessary setup work before running the benchmark.
        /// </summary>
        /// <remarks> This time is not counted in the runtime of the benchmark.
        /// Note that the <see cref="Go" /> method may be called multiple times
        /// in between calls to <see cref="Setup" /> and <see cref="Cleanup" />.
        /// </remarks>
        void Setup();

        /// <summary>
        /// Performs any necessary cleanup work after running the benchmark.
        /// </summary>
        /// <remarks> This time is not counted in the runtime of the benchmark.
        /// Note that the <see cref="Go" /> method may be called multiple times
        /// in between calls to <see cref="Setup" /> and <see cref="Cleanup" />.
        /// </remarks>
        void Cleanup();

        /// <summary>
        /// Performs the work of the benchmark.
        /// </summary>
        /// <remarks>
        /// <para>This is where you put the code you'd like to measure. The
        /// code can be arbitrarily small or fast-- the harness will ensure that
        /// this method is run in a loop until we get a good measurement from
        /// the timer.</para>
        /// <para>(Note that this means that this function must be able to be run
        /// multiple times in a loop in between calls to <see cref="Setup" />
        /// and <see cref="Cleanup" />. This shouldn't be too big a restriction,
        /// but it's worth metioning.</para>
        /// </remarks>
        void Go();
    }

    /// <summary>
    /// Provides a basic implementation of <see cref="IBenchmark" />.
    /// </summary>
    /// <remarks>This class provides default do-nothing implementations of 
    /// benchmark methods, where appropriate. See the documentation for 
    /// <see cref="IBenchmark" /> for more details on the structure of 
    /// benchmarks in general.</remarks>
    public abstract class BenchmarkBase : IBenchmark
    {
        /// <summary>
        /// Gets the name of this benchmark.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>See <see cref="IBenchmark.Name" /> for more details.
        /// </remarks>
        public abstract string Name { get; }
        /// <summary>
        /// Performs any necessary setup work before running the benchmark.
        /// </summary>
        /// <remarks>See <see cref="IBenchmark.Setup" /> for more details. The 
        /// default implementation of this method does nothing, unless 
        /// overridden by a derived class. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void Setup()
        {
        }
        /// <summary>
        /// Performs any necessary cleanup work after running the benchmark.
        /// </summary>
        /// <remarks>See <see cref="IBenchmark.Cleanup" /> for more details. The 
        /// default implementation of this method does nothing, unless 
        /// overridden by a derived class. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void Cleanup()
        {
        }
        /// <summary>
        /// When overridden by a derived class, performs the actual work of the
        /// benchmark.
        /// </summary>
        /// <remarks>See <see cref="IBenchmark.Go" /> for more details.
        /// </remarks>

        [MethodImpl(MethodImplOptions.NoInlining)]
        public abstract void Go();
    }
}
