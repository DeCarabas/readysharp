namespace ReadyGo
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    static class ResultsFormatter
    {
        const int LineLength = 80;
        const int LeftMargin = 12;
        const int BarWidth = LineLength - LeftMargin - 2;

        static string FormatTime(double ms)
        {
            if (ms < 0.001)
            {
                double totalNanoseconds = ms * 100000;
                return string.Format("{0:F2} ns", totalNanoseconds);
            }
            if (ms < 1)
            {
                // Measure in microseconds.
                double totalMicroseconds = ms * 1000;
                return string.Format("{0:F2} us", totalMicroseconds);
            }
            if (ms < 5000)
            {
                return string.Format("{0:F2} ms", ms);
            }

            double totalSeconds = ms / 1000.0;
            return string.Format("{0:F2} s", totalSeconds);
        }

        static string FormatLine(
          string label,
          double min,
          double p80,
          double max
        )
        {
            double msPerChar = (double)max / (double)BarWidth;
            int minPos = (int)Math.Round((double)min / msPerChar);
            int p80Pos = (int)Math.Round((double)p80 / msPerChar);

            var builder = new StringBuilder();
            builder.Append("  ");
            builder.Append(label);
            builder.Append(": ");
            if (builder.Length < LeftMargin)
            {
                builder.Append(' ', LeftMargin - builder.Length);
            }
            builder.Append('|');
            int cursor = 0;
            while (cursor < minPos)
            {
                builder.Append(' ');
                cursor++;
            }
            builder.Append('x');
            cursor++;
            while (cursor < p80Pos)
            {
                builder.Append('-');
                cursor++;
            }
            while (cursor < BarWidth)
            {
                builder.Append(' ');
                cursor++;
            }
            builder.Append('|');
            return builder.ToString();
        }

        static string FormatLegend(double max)
        {
            string maxTime = FormatTime(max);

            var builder = new StringBuilder();
            builder.Append(' ', LeftMargin);
            builder.Append('0');
            builder.Append(' ', LineLength - builder.Length - maxTime.Length);
            builder.Append(maxTime);
            return builder.ToString();
        }

        public static string[] FormatResults(
          BenchmarkResult current,
          BenchmarkResult baseline)
        {
            double max = current.P80;
            if (baseline != null && baseline.P80 > max)
            {
                max = baseline.P80;
            }

            var lines = new List<string>();
            lines.Add(current.Name);
            if (baseline != null)
            {
                lines.Add(FormatLine(
                  "Baseline", baseline.MinimumTime, baseline.P80, max));
            }
            lines.Add(FormatLine("Current", current.MinimumTime, current.P80, max));
            lines.Add(FormatLegend(max));
            return lines.ToArray();
        }
    }
}