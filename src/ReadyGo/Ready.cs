
namespace ReadyGo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;

    public class BenchmarkArguments
    {
        public BenchmarkArguments()
        {
        }
        public BenchmarkArguments(string[] args)
        {
            ParseArgs(args);
        }

        public bool Compare { get; set; }
        public bool Help { get; set; }
        public bool Record { get; set; }

        public static string GetHelp()
        {
            return @"
Options are: 
  --record    Record the current run as a new baseline.
  --compare   Compare the current run against the stored baseline.
  --help      Display this help.
";
        }

        public void ParseArgs(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
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
                        throw new BenchmarkArgumentException(
                          "Unknown switch: " + args[i]);
                }
            }
        }

        public void Validate()
        {
            if (Record && Compare)
            {
                throw new BenchmarkArgumentException(
                  "You cannot specify both 'record' and 'compare' at the " +
                  "same time.");
            }
        }
    }

    public class BenchmarkArgumentException : Exception
    {
        public BenchmarkArgumentException(string message) : base(message) { }
    }

    public static class Ready
    {
        public static int Go(string[] args, params IBenchmark[] benchmarks)
        {
            try
            {
                var arguments = new BenchmarkArguments(args);
                return Go(arguments, benchmarks);
            }
            catch (BenchmarkArgumentException bae)
            {
                Console.WriteLine(bae.Message);
                Console.WriteLine(BenchmarkArguments.GetHelp());
                return -1;
            }
        }

        public static int Go(
          BenchmarkArguments arguments,
          params IBenchmark[] benchmarks)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }
            if (benchmarks == null)
            {
                throw new ArgumentNullException(nameof(benchmarks));
            }
            if (benchmarks.Length == 0)
            {
                throw new ArgumentException(
                  "You must provide at least one benchmark to run.",
                  nameof(benchmarks));
            }

            try
            {
                arguments.Validate();
            }
            catch (BenchmarkArgumentException bae)
            {
                Console.WriteLine(bae.Message);
                Console.WriteLine(BenchmarkArguments.GetHelp());
                return -1;
            }

            if (arguments.Help)
            {
                Console.WriteLine(BenchmarkArguments.GetHelp());
                return 0;
            }

            var baseline = arguments.Compare ? Baseline.Load() : new Baseline();
            var results = new Runner(benchmarks).Run();
            foreach (var result in results)
            {
                BenchmarkResult baselineResult = baseline.TryGet(result.Name);
                string[] lines = ResultsFormatter.FormatResults(
                  result, baselineResult);
                foreach (string line in lines)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine();
            }

            if (arguments.Record)
            {
                baseline.Results.Clear();
                baseline.Results.AddRange(results);
                baseline.Save();
            }

            return 0;
        }

        public class Baseline
        {
            const string FileName = ".readysharp";

            readonly List<BenchmarkResult> results = new List<BenchmarkResult>();

            [JsonProperty("benchmark_results")]
            public List<BenchmarkResult> Results => this.results;

            public static Baseline Load()
            {
                if (File.Exists(FileName))
                {
                    string text = File.ReadAllText(FileName, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<Baseline>(text);
                }
                else
                {
                    return new Baseline();
                }
            }

            public void Save()
            {
                string text = JsonConvert.SerializeObject(this);
                File.WriteAllText(FileName, text, Encoding.UTF8);
            }

            public BenchmarkResult TryGet(string name) => this.results.Find(
                r => String.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}