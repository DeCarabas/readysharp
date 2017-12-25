using System;
using ReadyGo;

namespace examples {

  class StringFormatBenchmark : Benchmark {
    public override string Name => "Formatting a string";

    public override void Go() {
      String.Format("{0}", 42);
    }
  }

  class Program {
    static void Main(string[] args) {
      Ready.Go(args, new StringFormatBenchmark());
    }
  }
}
