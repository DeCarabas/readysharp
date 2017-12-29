using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ReadyGo
{
    /// <summary>
    /// Provides command-line configuation options to the benchmark runner.
    /// </summary>
    /// <remarks>
    /// <para>This class is suitable for use when you have your own harness, or
    /// your own command line parsing, or you need to tweak things just a little 
    /// bit.</para>
    /// <para>If you have your entirely own command line handling, just set the
    /// properties on this object.</para>
    /// <para>If you want to use the default handling but tweak the results a
    /// bit, create one of these and pass the args to the constructor, or call
    /// <see cref="ParseArgs" />.</para>
    /// </remarks>
    public class BenchmarkArguments
    {
        /// <summary>
        /// Construct a new instance of the <see cref="BenchmarkArguments" /> 
        /// class.
        /// </summary>
        public BenchmarkArguments()
        {
        }

        /// <summary>
        /// Construct a new instance of the <see cref="BenchmarkArguments" />
        /// class and set the properties by parsing the provided command line
        /// arguments.
        /// </summary>
        /// <param name="args">The command line arguments to parse.</param>
        public BenchmarkArguments(string[] args)
        {
            ParseArgs(args);
        }

        /// <summary>
        /// Gets or sets whether or not the run should compare against the 
        /// baseline.
        /// </summary>
        /// <returns><c>true</c> if the run should compare against the baseline, 
        /// otherwise <c>false</c>.</returns>
        /// <remarks>The default value is <c>false</c>.</remarks>
        public bool Compare { get; set; }
        /// <summary>
        /// Gets or sets whether or not the runner should show command line 
        /// help.
        /// </summary>
        /// <returns><c>true</c> if the run should show help, otherwise 
        /// <c>false</c>.</returns>
        /// <remarks>The default value is <c>false</c>.</remarks>
        public bool Help { get; set; }
        /// <summary>
        /// Gets or sets whether or not the run should record its results in 
        /// the baseline file.
        /// </summary>
        /// <returns><c>true</c> if the run should record results, otherwise 
        /// <c>false</c>.</returns>
        /// <remarks>The default value is <c>false</c>.</remarks>
        public bool Record { get; set; }

        /// <summary>
        /// Get the help string describing the valid command line arguments.
        /// </summary>
        /// <returns>The help string describing the valid command line 
        /// arguments.</returns>
        public static string GetHelp()
        {
            return @"
Options are: 
  --record    Record the current run as a new baseline.
  --compare   Compare the current run against the stored baseline.
  --help      Display this help.
";
        }

        /// <summary>
        /// Set properties by parsing the provided commmand line arguments.
        /// </summary>
        /// <param name="args">The arguments to parse.</param>
        /// <exception cref="BenchmarkArgumentException">The arguments are not
        /// in the right format.</exception>
        /// <remarks>See <see cref="GetHelp" /> for the format of the command
        /// line arguments this method supports.</remarks>
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

        /// <summary>
        /// Ensure that the current set of properties is valid.
        /// </summary>
        /// <exception cref="BenchmarkArgumentException">The current set of
        /// property values is invalid.</exception>
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

    /// <summary>
    /// The exception raised when there is a problem with the arguments supplied
    /// to the runner.
    /// </summary>
    /// <remarks>See <see cref="BenchmarkArguments" />for more information.
    /// </remarks>
    public class BenchmarkArgumentException : Exception
    {
        /// <summary>
        /// Construct a new instance of the 
        /// <see cref="BenchmarkArgumentException" /> class.
        /// </summary>
        /// <param name="message">The message explaining what went wrong.
        /// </param>
        public BenchmarkArgumentException(string message) : base(message) { }
    }

    /// <summary>
    /// The public entry point for running benchmarks.
    /// </summary>
    /// <remarks>This is the entry point for running benchmarks. Start with 
    /// the <see cref="Ready.Go(string[], IBenchmark[])" /> methods.</remarks>
    public static class Ready
    {
        /// <summary>
        /// Run the specified benchmarks, and write the results to 
        /// <see cref="Console.Out" />.
        /// </summary>
        /// <param name="args">The command line arguments to be parsed before 
        /// running the benchmarks.</param>
        /// <param name="benchmarks">The benchmarks to run.</param>
        /// <returns><c>0</c> if the benchmarks all ran successfully, otherwise
        /// a nonzero return code indicating failure.</returns>
        /// <remarks>
        /// <para>If the benchmarks succeed, the appropriate report will be
        /// written out to <see cref="Console.Out" />. Otherwise, an appropriate
        /// failure message will be written instead.</para>
        /// <para>If you need custom command line handling, create a new 
        /// instance of the <see cref="BenchmarkArguments" /> class and call the
        /// <see cref="Ready.Go(BenchmarkArguments, IBenchmark[])" /> overload.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Run the specified benchmarks, and write the results to 
        /// <see cref="Console.Out" />.
        /// </summary>
        /// <param name="arguments">The parsed arguments to configure the 
        /// benchmark run.</param>
        /// <param name="benchmarks">The benchmarks to run.</param>
        /// <returns><c>0</c> if the benchmarks all ran successfully, otherwise
        /// a nonzero return code indicating failure.</returns>
        /// <remarks>
        /// <para>If the benchmarks succeed, the appropriate report will be
        /// written out to <see cref="Console.Out" />. Otherwise, an appropriate
        /// failure message will be written instead.</para>
        /// </remarks>

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

        class Baseline
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
