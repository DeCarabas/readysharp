using System;
using ReadyGo;

namespace examples {

  class StringFormatBenchmark : BenchmarkBase {
    public override string Name => "Formatting a string with class";
    public override void Go() => String.Format("{0}", 42);
  }

  class StringConcatBenchmark : BenchmarkBase {
    public override string Name => "Concatenating strings";
    public override void Go() => String.Concat("asdf", "Ooogabooga");
  }

  class Program {
    static void Main(string[] args) {
      Ready.Go(
        args,
        new StringFormatBenchmark(),
        new StringConcatBenchmark());
    }
  }
}
