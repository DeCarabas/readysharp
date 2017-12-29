using System;
using System.IO;
using ReadyGo;

namespace examples
{
    class BigFiles : IBenchmark
    {
        public string Name => "big files";
        public void Setup() => File.WriteAllText("foo", new String('X', 1000000));
        public void Cleanup() => File.Delete("foo");
        public void Go() => File.ReadAllText("foo");
    }

    class StringFormatBenchmark : BenchmarkBase
    {
        public override string Name => "Formatting a string with class";
        public override void Go() => String.Format("{0}", 42);
    }

    class StringConcatBenchmark : BenchmarkBase
    {
        public override string Name => "Concatenating strings";
        public override void Go() => String.Concat("asdf", "Ooogabooga");
    }

    class Program
    {
        static void Main(string[] args)
        {
            Ready.Go(
              args,
              new BigFiles(),
              new StringFormatBenchmark(),
              new StringConcatBenchmark());
        }
    }
}
