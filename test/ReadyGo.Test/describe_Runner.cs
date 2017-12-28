using System;
using FluentAssertions;
using NSpec;
using NSubstitute;

namespace ReadyGo.Test
{
    public class describe_Runner : nspec
    {
        void running_with_degenerate_inputs()
        {
            it["returns empty on null"] = ()
                => new Runner(null).Run().Length.Should().Be(0);
            it["returns empty on empty"] = ()
                => new Runner(new IBenchmark[0]).Run().Length.Should().Be(0);
        }

        void core_measurement()
        {
            var benchmark = Substitute.For<IBenchmark>();
            before = () => benchmark.ClearReceivedCalls();
            it["calls Go the right number of times"] = () =>
            {
                Runner.MeasureRuntime(0, benchmark);
                benchmark.Received(0).Go();
                Runner.MeasureRuntime(10, benchmark);
                benchmark.Received(10).Go();
            };
        }
    }
}
