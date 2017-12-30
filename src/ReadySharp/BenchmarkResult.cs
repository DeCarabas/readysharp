namespace ReadySharp
{
    /// <summary>
    /// Records the amount of time a benchmark takes.
    /// /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// Construct a new instance of the <see cref="BenchmarkResult" /> 
        /// class.
        /// </summary>
        /// <param name="name">The name of the benchmark.</param>
        /// <param name="minimumTime">The minimum time, in milliseconds, any 
        /// given run of the benchmark's <see cref="IBenchmark.Go" /> method 
        /// took.</param>
        /// <param name="p80">The P80 of the elapsed times in millseconds of the
        /// benchmark's <see cref="IBenchmark.Go" /> method.</param>
        public BenchmarkResult(string name, double minimumTime, double p80)
        {
            MinimumTime = minimumTime;
            Name = name;
            P80 = p80;
        }
        /// <summary>
        /// Gets the minimum time that any given run of the benchmark's 
        /// <see cref="IBenchmark.Go" /> method took.
        /// </summary>
        /// <returns>The time in millseconds.</returns>
        public double MinimumTime { get; private set; }
        /// <summary>
        /// Gets the name of the benchmark associated with these results.
        /// </summary>
        /// <returns>The name of the benchmark.</returns>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the P80 of the elapsed times of the benchmark's
        /// <see cref="IBenchmark.Go" /> method.
        /// </summary>
        /// <returns>The time in milliseconds.</returns>
        public double P80 { get; private set; }
    }
}
