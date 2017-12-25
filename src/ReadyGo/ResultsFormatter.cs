namespace ReadyGo {
  using System;
  using System.Collections.Generic;
  using System.Text;

  static class ResultsFormatter {
    const int LineLength = 80;
    const int LeftMargin = 12;
    const int BarWidth = LineLength - LeftMargin - 2;

    static string FormatTime(TimeSpan time) {
      if (time < TimeSpan.FromMilliseconds(1)) {
        // Measure in microseconds.
        double totalMicroseconds = (double)time.Ticks / 10.0;
        return string.Format("{0:F2} us", totalMicroseconds);
      }
      if (time < TimeSpan.FromSeconds(5)) {
        double totalMilliseconds = Math.Round(time.TotalMilliseconds, 2);
        return string.Format("{0:F2} ms", totalMilliseconds);
      }
      if (time < TimeSpan.FromMinutes(5)) {
        double totalSeconds = Math.Round(time.TotalSeconds, 2);
        return string.Format("{0:F2} s", totalSeconds);
      }
      double totalMinutes = Math.Round(time.TotalMinutes, 2);
      return string.Format("{0:F2} min", totalMinutes);
    }

    static string FormatLine(
      string label,
      TimeSpan min,
      TimeSpan p80,
      TimeSpan max
    ) {
      double ticksPerChar = (double)max.Ticks / (double)BarWidth;
      int minPos = (int)Math.Round((double)min.Ticks / ticksPerChar);
      int p80Pos = (int)Math.Round((double)p80.Ticks / ticksPerChar);

      var builder = new StringBuilder();
      builder.Append("  ");
      builder.Append(label);
      builder.Append(": ");
      if (builder.Length < LeftMargin) {
        builder.Append(' ', LeftMargin - builder.Length);
      }
      builder.Append('|');
      int cursor = 0;
      while (cursor < minPos) {
        builder.Append(' ');
        cursor++;
      }
      builder.Append('x');
      cursor++;
      while (cursor < p80Pos) {
        builder.Append('-');
        cursor++;
      }
      while (cursor < BarWidth) {
        builder.Append(' ');
      }
      builder.Append('|');
      return builder.ToString();
    }

    static string FormatLegend(TimeSpan max) {
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
      BenchmarkResult baseline) {
      TimeSpan max = current.P80;
      if (baseline != null && baseline.P80 > max) {
        max = baseline.P80;
      }

      var lines = new List<string>();
      lines.Add(current.Name);
      if (baseline != null) {
        lines.Add(FormatLine("Baseline", current.MinimumTime, current.P80, max));
      }
      lines.Add(FormatLine("Current", current.MinimumTime, current.P80, max));
      lines.Add(FormatLegend(max));
      return lines.ToArray();
    }
  }
}