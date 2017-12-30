using System;
using FluentAssertions;
using NSpec;

namespace ReadySharp.Test
{
    public class describe_Runner : nspec
    {
        Tardis timer;
        FakeBenchmark benchmark;
        FakeBenchmark nullBenchmark;

        void before_each()
        {
            this.timer = new Tardis();
            this.benchmark = new FakeBenchmark(this.timer);
            this.nullBenchmark = new FakeBenchmark(this.timer);
        }

        void running_with_degenerate_inputs()
        {
            it["returns empty on null"] = ()
                => new Runner(null).Run().Length.Should().Be(0);
            it["returns empty on empty"] = ()
                => new Runner(new IBenchmark[0]).Run().Length.Should().Be(0);
        }

        void capture_time()
        {
            double runtime = 0;
            int iterations = 1;
            act = () =>
            {
                var runner = new Runner(null) { Timer = timer };
                runtime = runner.CaptureTime(
                    benchmark, nullBenchmark, iterations, throwOnTooFast: true);
            };
            context["the benchmark is too fast"] = () =>
            {
                before = () => { benchmark.ExpectedDuration = 1.0; };
                it["should throw"] = expect<Runner.BenchmarkTooFastException>();
                it["should call the right number of times"] = ()
                    => benchmark.GoCount.Should().Be(iterations);
                it["should setup"] = ()
                    => benchmark.SetupCount.Should().Be(1);
                it["should cleanup"] = ()
                    => benchmark.CleanupCount.Should().Be(1);
            };
            context["the overhead is non-zero"] = () =>
            {
                before = () =>
                {
                    benchmark.ExpectedDuration = 10 * 1000;
                    nullBenchmark.ExpectedDuration = 5 * 1000;
                };
                it["should cancel out overhead"] = ()
                    => runtime.Should().Be(5 * 1000);
                it["should call the right number of times"] = ()
                    => benchmark.GoCount.Should().Be(iterations);
                it["should setup"] = ()
                    => benchmark.SetupCount.Should().Be(1);
                it["should cleanup"] = ()
                    => benchmark.CleanupCount.Should().Be(1);
            };
            context["multiple iterations"] = () =>
            {
                before = () =>
                {
                    iterations = 10;
                    benchmark.ExpectedDuration = 100.0;
                };
                it["should measure"] = () => runtime.Should().Be(10.0);
                it["should call the right number of times"] = ()
                    => benchmark.GoCount.Should().Be(iterations);
                it["should setup"] = ()
                    => benchmark.SetupCount.Should().Be(1);
                it["should cleanup"] = ()
                    => benchmark.CleanupCount.Should().Be(1);
            };
        }

        class Tardis : IBenchmarkTimer
        {
            public double ElapsedMilliseconds { get; set; }
            public void Restart() { }
            public void Stop() { }
        }

        class FakeBenchmark : IBenchmark
        {
            readonly Tardis timer;
            public FakeBenchmark(Tardis timer) { this.timer = timer; }
            string IBenchmark.Name => "FakeBenchmark";
            public int CleanupCount { get; set; }
            public double ExpectedDuration { get; set; }
            public int GoCount { get; set; }
            public int SetupCount { get; set; }
            public void Reset()
            {
                CleanupCount = 0;
                SetupCount = 0;
                GoCount = 0;
            }
            void IBenchmark.Cleanup() => CleanupCount++;
            void IBenchmark.Go()
            {
                GoCount++;
                this.timer.ElapsedMilliseconds = ExpectedDuration;
            }
            void IBenchmark.Setup() => SetupCount++;
        }
    }
}
