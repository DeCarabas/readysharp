using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ReadySharp
{
    sealed class NullBenchmark : BenchmarkBase
    {
        public override string Name => "(null)";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Setup()
        {
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Cleanup()
        {
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Go()
        {
        }
    }

    /// <summary>
    /// Provides more fine-grained control over running benchmarks.
    /// </summary>
    /// <remarks>
    /// You probably don't want this; you probably want to just call
    /// <see cref="Ready.Go(string[], IBenchmark[])"/>. Nevertheless, if you 
    /// want your own control over baselines or reporting results or something,
    /// then you can create one of these and call the <see cref="Runner.Run" /> 
    /// method.
    /// </remarks>
    public class Runner
    {
        const int DefaultOuterIterations = 16;
        const double DefaultMinimumTimeMs = 3;
        static readonly IBenchmark NullBenchmark = new NullBenchmark();
        readonly IBenchmark[] benchmarks;
        double minimumTimeMs;
        int outerIterations;
        IBenchmarkTimer timer;

        /// <summary>
        /// Create a new instance of the <see cref="Runner"/> class.
        /// </summary>
        /// <param name="benchmarks">The benchmarks we will be running.</param>
        public Runner(IBenchmark[] benchmarks)
        {
            this.benchmarks = benchmarks;
            this.minimumTimeMs = DefaultMinimumTimeMs;
            this.outerIterations = DefaultOuterIterations;
            this.timer = new BenchmarkTimer();
        }

        /// <summary>
        /// Gets or sets minimum amount of time to allow a 
        /// <see cref="IBenchmark.Go" /> method to run.
        /// </summary>
        /// <returns>The minimum execution time, in milliseconds.</returns>
        /// <remarks>
        /// <para>The runner enforces a minimum execution time so that we can
        /// ensure that we get a good reading off of the timer, and runs a given
        /// benchmark's <see cref="IBenchmark.Go" /> method in a loop until the
        /// minimum time has been reached.</para>
        /// 
        /// <para>The default value of this property is 3ms. Setting this value
        /// higher increases the accuracy of the benchmark, but can make 
        /// execution time slower. Setting this value lower will have the 
        /// opposite effect.</para>
        /// </remarks>
        public double MinimumTimeMs
        {
            get => this.minimumTimeMs;
            set
            {
                if (value <= double.Epsilon)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                this.minimumTimeMs = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of times to run each benchmark.
        /// </summary>
        /// <returns>The number of times to run each benchmark.</returns>
        /// <remarks>
        /// <para>This property controls the number of times each benchmark 
        /// is run overall. (The <see cref="IBenchmark.Go" /> method may still
        /// be called more times, in order to meet the minimum runtime 
        /// requirements, but this property controls the number of times each
        /// benchmark is measured.)</para>
        /// 
        /// <para>The default value for this property is 16.</para></remarks>
        public int OuterIterations
        {
            get => this.outerIterations;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                this.outerIterations = value;
            }
        }

        /// <summary>
        /// Get or set the timer to use when running benchmarks.
        /// </summary>
        /// <returns>The <see cref="IBenchmarkTimer" /> instance that this 
        /// runner will use.</returns>
        /// <remarks>By default this is an instance of 
        /// <see cref="BenchmarkTimer" />.</remarks>
        public IBenchmarkTimer Timer
        {
            get => this.timer;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.timer = value;
            }
        }

        static double MeasureRuntime(
            IBenchmark benchmark, int iterations, IBenchmarkTimer timer)
        {
            GC.Collect(
                GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
            timer.Restart();
            for (int i = 0; i < iterations; i++)
            {
                benchmark.Go();
            }
            timer.Stop();
            return timer.ElapsedMilliseconds;
        }

        static double RunBenchmark(
            IBenchmark benchmark,
            IBenchmark nullBenchmark,
            int iterations,
            IBenchmarkTimer timer)
        {
            double rawTime = MeasureRuntime(benchmark, iterations, timer);
            double constantTime = MeasureRuntime(
                nullBenchmark, iterations, timer);

            return rawTime - constantTime;
        }

        static void Prime(IBenchmark benchmark)
        {
            benchmark.Setup();
            benchmark.Go();
            benchmark.Cleanup();
        }

        internal double CaptureTime(
            IBenchmark benchmark,
            IBenchmark nullBenchmark,
            int iterations,
            bool throwOnTooFast = false)
        {
            benchmark.Setup();
            double result = RunBenchmark(
                benchmark,
                nullBenchmark,
                iterations,
                this.timer);
            benchmark.Cleanup();

            if (throwOnTooFast)
            {
                if (result < MinimumTimeMs)
                {
                    throw new BenchmarkTooFastException();
                }
            }

            return result / iterations;
        }

        static double Percentile(double[] times, int percentile)
        {
            double[] sortedTimes = new double[times.Length];
            Array.Copy(times, sortedTimes, times.Length);
            Array.Sort(sortedTimes);

            double ratio = percentile * 0.01;
            double h = ratio * ((double)(sortedTimes.Length - 1)) + 1.0;
            int k = (int)(Math.Floor(h) - 1.0);
            double f = h % 1.0;

            return sortedTimes[k] + (f * (sortedTimes[k + 1] - sortedTimes[k]));
        }

        /// <summary>
        /// Run the configured benchmarks.
        /// </summary>
        /// <returns>An array of <see cref="BenchmarkResult" /> objects, which
        /// contain the statistics for the benchmarks.</returns>
        public BenchmarkResult[] Run()
        {
            if (this.benchmarks == null) { return new BenchmarkResult[0]; }
            int[] iterations = new int[this.benchmarks.Length];
            double[][] benchTimes = new double[this.benchmarks.Length][];
            for (int i = 0; i < this.benchmarks.Length; i++)
            {
                benchTimes[i] = new double[OuterIterations];
            }
            for (int i = 0; i < this.benchmarks.Length; i++)
            {
                Prime(this.benchmarks[i]);
            }

            for (int i = 0; i < this.benchmarks.Length; i++)
            {
                IBenchmark benchmark = this.benchmarks[i];
                double[] times = benchTimes[i];

                iterations[i] = 1;
                while (true)
                {
                    try
                    {
                        times[0] = CaptureTime(
                          benchmark,
                          NullBenchmark,
                          iterations[i],
                          throwOnTooFast: true);
                        break;
                    }
                    catch (BenchmarkTooFastException)
                    {
                        Console.Write("!");
                        iterations[i] *= 2;
                        continue;
                    }
                }
                Console.Write(".");
            }

            for (int i = 1; i < OuterIterations; i++)
            {
                for (int j = 0; j < this.benchmarks.Length; j++)
                {
                    IBenchmark benchmark = this.benchmarks[j];
                    double[] times = benchTimes[j];

                    times[i] = CaptureTime(
                        benchmark,
                        NullBenchmark,
                        iterations[j]);
                    Console.Write(".");
                }
            }
            Console.WriteLine();

            var results = new BenchmarkResult[this.benchmarks.Length];
            for (int i = 0; i < this.benchmarks.Length; i++)
            {
                IBenchmark benchmark = this.benchmarks[i];
                double[] times = benchTimes[i];
                Array.Sort(times);

                results[i] = new BenchmarkResult(
                  benchmark.Name,
                  times[0],
                  Percentile(times, 80));
            }
            return results;
        }

        internal class BenchmarkTooFastException : Exception
        {
        }
    }
}
