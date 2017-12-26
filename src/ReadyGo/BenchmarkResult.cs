namespace ReadyGo
{
    using System;

    public class BenchmarkResult
    {
        public BenchmarkResult(string name, double minimumTime, double p80)
        {
            MinimumTime = minimumTime;
            Name = name;
            P80 = p80;
        }
        public double MinimumTime { get; private set; }
        public string Name { get; private set; }
        public double P80 { get; private set; }
    }
}