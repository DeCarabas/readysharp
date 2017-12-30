namespace ReadySharp
{
    using System.Diagnostics;

    /// <summary>
    /// An abstraction over a stopwatch used to measure benchmarks.
    /// </summary>
    public interface IBenchmarkTimer
    {
        /// <summary>
        /// Gets the number of milliseconds elapsed since the last call to 
        /// <see cref="Restart" />.
        /// </summary>
        /// <returns>The number of milliseconds.</returns>
        double ElapsedMilliseconds { get; }
        /// <summary>
        /// Resets <see cref="ElapsedMilliseconds" /> to 0 and starts the timer.
        /// </summary>
        void Restart();
        /// <summary>Stops the timer.</summary>
        void Stop();
    }

    /// <summary>
    /// The default implementation of <see cref="IBenchmarkTimer" />.
    /// </summary>
    /// <remarks>This implementation works in terms of a
    /// <see cref="System.Diagnostics.Stopwatch" /> instance.</remarks>
    public sealed class BenchmarkTimer : IBenchmarkTimer
    {
        readonly Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// Gets the number of milliseconds elapsed since the last call to 
        /// <see cref="Restart" />.
        /// </summary>
        /// <returns>The number of milliseconds.</returns>
        public double ElapsedMilliseconds => this.stopwatch.ElapsedMilliseconds;
        /// <summary>
        /// Resets <see cref="ElapsedMilliseconds" /> to 0 and starts the timer.
        /// </summary>
        public void Restart() => this.stopwatch.Restart();
        /// <summary>Stops the timer.</summary>
        public void Stop() => this.stopwatch.Stop();
    }
}
