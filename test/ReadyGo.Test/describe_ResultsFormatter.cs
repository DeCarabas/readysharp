using System.Text;
using FluentAssertions;
using NSpec;

namespace ReadyGo.Test
{
    public class describe_ResultsFormatter : nspec
    {
        void formatting_time()
        {
            it["formats nanoseconds"] = ()
                => ResultsFormatter.FormatTime(.000001).Should().EndWith("ns");
            it["formats microseconds"] = ()
                => ResultsFormatter.FormatTime(.001).Should().EndWith("us");
            it["formats milliseconds"] = ()
                => ResultsFormatter.FormatTime(1).Should().EndWith("ms");
            it["formats seconds"] = ()
                => ResultsFormatter.FormatTime(1000).Should().EndWith("s");
            it["keeps formatting as seconds"] = ()
                => ResultsFormatter.FormatTime(10000000).Should().EndWith("s");
        }

        void formatting_bars()
        {
            string bar = null;
            double min = 0;
            double p80 = 1;
            double max = 2;

            act = () =>
            {
                var builder = new StringBuilder();
                ResultsFormatter.FormatBar(builder, min, p80, max, 5);
                bar = builder.ToString();
            };
            context["front"] = () =>
            {
                before = () => { min = 0; p80 = 0; max = 4; };
                it["renders correctly"] = () => bar.Should().Be("|x    |");
            };
            context["midpoint"] = () =>
            {
                before = () => { min = 2; p80 = 2; max = 4; };
                it["renders correctly"] = () => bar.Should().Be("|  x  |");
            };
            context["end"] = () =>
            {
                before = () => { min = 4; p80 = 4; max = 4; };
                it["renders correctly"] = () => bar.Should().Be("|    x|");
            };
            context["span"] = () =>
            {
                before = () => { min = 0; p80 = 4; max = 4; };
                it["renders correctly"] = () => bar.Should().Be("|x----|");
            };
            context["almost"] = () =>
            {
                before = () => { min = 0; p80 = 2; max = 4; };
                it["renders correctly"] = () => bar.Should().Be("|x--  |");
            };
            context["back half"] = () =>
            {
                before = () => { min = 2; p80 = 4; max = 4; };
                it["renders correctly"] = () => bar.Should().Be("|  x--|");
            };
            context["midspan"] = () =>
            {
                before = () => { min = 1; p80 = 3; max = 4; };
                it["renders correctly"] = () => bar.Should().Be("| x-- |");
            };
            context["degenerate"] = () =>
            {
                before = () => { min = 0; p80 = 0; max = 0; };
                it["renders correctly"] = () => bar.Should().Be("|x    |");
            };
        }

        void formatting_lines()
        {
            context["the label"] = () =>
            {
                string line = null;
                string label = null;

                act = () => line = ResultsFormatter.FormatLine(label, 0, 0, 3, len: 17);
                context["with a normal label"] = () =>
                {
                    before = () => label = "Before";
                    it["is right"] = () => line.Should().Be("  Before:   |x  |");
                };
                context["with a long label"] = () =>
                {
                    before = () => label = "ASDFASDFASDFASDF";
                    it["is right"] = () => line.Should().Be("  ASDFASDFASDFASDF: |x  |");
                };
                context["with an empty label"] = () =>
                {
                    before = () => label = "";
                    it["is right"] = () => line.Should().Be("  :         |x  |");
                };
            };
        }
    }
}
