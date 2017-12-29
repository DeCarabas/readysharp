using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ReadyGo
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

    public class Runner
    {
        const int OuterIterations = 16;
        static readonly double MinimumTimeMs = 3;
        static readonly IBenchmark NullBenchmark = new NullBenchmark();
        readonly IBenchmark[] benchmarks;
        readonly IBenchmarkTimer timer;

        public Runner(IBenchmark[] benchmark, IBenchmarkTimer timer = null)
        {
            this.benchmarks = benchmark;
            this.timer = timer ?? new BenchmarkTimer();
        }

        static double MeasureRuntime(
            IBenchmark benchmark, int iterations, IBenchmarkTimer timer)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
            timer.Restart();
            for (int i = 0; i < iterations; i++)
            {
                benchmark.Go();
            }
            timer.Stop();
            return timer.ElapsedMilliseconds;
        }

        // Run the given benchmark but be sure to null out any overhead, so that
        // we're only measuring the method contents. This returns a `double` 
        // instead of a TimeSpan because, strangely enough, TimeSpan doesn't 
        // have enough resolution.
        static double RunBenchmark(
            IBenchmark benchmark,
            IBenchmark nullBenchmark,
            int iterations,
            IBenchmarkTimer timer)
        {
            double rawTime = MeasureRuntime(benchmark, iterations, timer);
            double constantTime = MeasureRuntime(nullBenchmark, iterations, timer);

            return rawTime - constantTime;
        }

        static void Prime(IBenchmark benchmark)
        {
            benchmark.Setup();
            benchmark.Go();
            benchmark.Cleanup();
        }

        // This returns the time in ms.
        internal static double CaptureTime(
            IBenchmark benchmark,
            IBenchmark nullBenchmark,
            int iterations,
            IBenchmarkTimer timer,
            bool throwOnTooFast = false)
        {
            benchmark.Setup();
            double result = RunBenchmark(
                benchmark,
                nullBenchmark,
                iterations,
                timer);
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
                          this.timer,
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
                        iterations[j],
                        this.timer);
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
