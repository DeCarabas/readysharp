using System;
using ReadyGo;

namespace examples {

  class StringFormatBenchmark : BenchmarkBase {
    public override string Name => "Formatting a string with class";

    public override void Go() {
      String.Format("{0}", 42);
    }
  }

  class Program {
    static void Main(string[] args) {
      Ready.Go(args, new StringFormatBenchmark());
      Ready
        .To("format strings with a delegate")
        .Go(args, () => String.Format("{0}", 1234));
    }
  }
}
