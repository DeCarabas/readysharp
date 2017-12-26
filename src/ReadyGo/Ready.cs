
namespace ReadyGo {
  using System;

  public class BenchmarkArguments {
    public BenchmarkArguments() {
    }
    public BenchmarkArguments(string[] args) {
      ParseArgs(args);
    }

    public bool Compare {
      get; set;
    }

    public bool Help {
      get; set;
    }

    public bool Record {
      get; set;
    }

    public static string GetHelp() {
      return @"
Options are: 
  --record    Record the current run as a new baseline.
  --compare   Compare the current run against the stored baseline.
  --help      Display this help.
";
    }

    public void ParseArgs(string[] args) {
      if (args == null || args.Length == 0) {
        return;
      }

      for (int i = 0; i < args.Length; i++) {
        switch (args[i].ToLowerInvariant()) {
          case "--record":
            Record = true;
            break;
          case "--compare":
            Compare = true;
            break;
          case "--help":
            Help = true;
            break;
          default:
            throw new BenchmarkArgumentException("Unknown switch: " + args[i]);
        }
      }
    }

    public void Validate() {
      if (Record && Compare) {
        throw new BenchmarkArgumentException(
          "You cannot specify both 'record' and 'compare' at the same time.");
      }
    }
  }

  public class BenchmarkArgumentException : Exception {
    public BenchmarkArgumentException(string message) : base(message) { }
  }

  public static class Ready {
    public static int Go(string[] args, params IBenchmark[] benchmarks)
      => Go(new BenchmarkArguments(args), benchmarks);

    public static int Go(
      BenchmarkArguments arguments,
      params IBenchmark[] benchmarks) {
      if (arguments == null) {
        throw new ArgumentNullException(nameof(arguments));
      }
      if (benchmarks == null) {
        throw new ArgumentNullException(nameof(benchmarks));
      }
      if (benchmarks.Length == 0) {
        throw new ArgumentException(
          "You must provide at least one benchmark to run.",
          nameof(benchmarks));
      }

      try {
        arguments.Validate();
      } catch (BenchmarkArgumentException bae) {
        Console.WriteLine(bae.Message);
        Console.WriteLine(BenchmarkArguments.GetHelp());
        return -1;
      }

      var results = new Runner(benchmarks).Run();
      for (int i = 0; i < results.Length; i++) {
        string[] lines = ResultsFormatter.FormatResults(results[i], null);
        foreach (string line in lines) {
          Console.WriteLine(line);
        }
        Console.WriteLine();
      }
      return 0;
    }
  }
}