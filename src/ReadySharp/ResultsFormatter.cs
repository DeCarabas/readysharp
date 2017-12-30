using System;
using System.Collections.Generic;
using System.Text;

namespace ReadySharp
{
    /// <summary>
    /// Format benchmark results for display on a terminal.
    /// </summary>
    static class ResultsFormatter
    {
        const int LineLength = 80;
        internal const int LeftMargin = 12;

        internal static string FormatTime(double ms)
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

        internal static void FormatBar(
            StringBuilder builder,
            double min,
            double p80,
            double max,
            int len)
        {
            // Mapping [0, max] to [0, len-1]
            double msPerChar = (double)max / (double)(len - 1);
            int minPos = (int)Math.Round((double)min / msPerChar);
            int p80Pos = (int)Math.Round((double)p80 / msPerChar);

            builder.Append('|');
            int cursor = 0;
            while (cursor < minPos)
            {
                builder.Append(' ');
                cursor++;
            }
            builder.Append('x');
            cursor++;
            if (cursor < p80Pos)
            {
                builder.Append('-');
                cursor++;
                while (cursor <= p80Pos)
                {
                    builder.Append('-');
                    cursor++;
                }
            }
            while (cursor < len)
            {
                builder.Append(' ');
                cursor++;
            }
            builder.Append('|');
        }

        internal static string FormatLine(
          string label,
          double min,
          double p80,
          double max,
          int len = LineLength
        )
        {
            int barWidth = len - LeftMargin - 2;

            var builder = new StringBuilder();
            builder.Append("  ");
            builder.Append(label);
            builder.Append(": ");
            if (builder.Length < LeftMargin)
            {
                builder.Append(' ', LeftMargin - builder.Length);
            }
            FormatBar(builder, min, p80, max, barWidth);
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

        /// <summary>
        /// Format benchmark results for display on a terminal.
        /// </summary>
        /// <param name="current">The benchmark results just measured.</param>
        /// <param name="baseline">The benchmark results to compare against.
        /// </param>
        /// <returns>An array of strings, one for each line of the report.
        /// </returns>
        public static string[] FormatResults(
          BenchmarkResult current,
          BenchmarkResult baseline)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

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
