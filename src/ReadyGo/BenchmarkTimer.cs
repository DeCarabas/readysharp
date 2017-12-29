namespace ReadyGo
{
    using System.Diagnostics;

    public interface IBenchmarkTimer
    {
        double ElapsedMilliseconds { get; }
        void Restart();
        void Stop();
    }

    public sealed class BenchmarkTimer : IBenchmarkTimer
    {
        readonly Stopwatch stopwatch = new Stopwatch();
        public double ElapsedMilliseconds => this.stopwatch.ElapsedMilliseconds;
        public void Restart() => this.stopwatch.Restart();
        public void Stop() => this.stopwatch.Stop();
    }
}
