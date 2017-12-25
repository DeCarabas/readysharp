namespace ReadyGo {
  using System;

  public class BenchmarkResult {
    public BenchmarkResult(string name, TimeSpan minimumTime, TimeSpan p80) {
      MinimumTime = minimumTime;
      Name = name;
      P80 = p80;
    }
    public TimeSpan MinimumTime {
      get; private set;
    }
    public string Name {
      get; private set;
    }
    public TimeSpan P80 {
      get; private set;
    }
  }
}