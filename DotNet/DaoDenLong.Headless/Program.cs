using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DaoDenLong.Core;

namespace DaoDenLong.Headless
{
    /// <summary>
    /// The Test Contract runner (section A/B): a Night of the game with no window and no GPU, writing
    /// JSONL. Exit 0 when it runs to the end, 2 when an input or scenario file is malformed, 1 for any
    /// other error with the reason on stderr. Same seed + scenario + inputs always produce the same file.
    /// </summary>
    public static class Program
    {
        private const int ExitOk = 0;
        private const int ExitError = 1;
        private const int ExitBadInput = 2;

        public static int Main(string[] args)
        {
            try
            {
                Run(args);
                return ExitOk;
            }
            catch (FormatException e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                return ExitBadInput;
            }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                return ExitBadInput;
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                return ExitBadInput;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("error: " + e.Message);
                return ExitError;
            }
        }

        private static void Run(string[] args)
        {
            int seed = 1;
            int night = 1;
            long ticks = 0;
            int snapshotEvery = 10;
            string scenarioFile = null, inputsFile = null, outPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--seed": seed = ReadInt(args, ref i, "--seed"); break;
                    case "--night": night = ReadInt(args, ref i, "--night"); break;
                    case "--ticks": ticks = ReadInt(args, ref i, "--ticks"); break;
                    case "--snapshot-every": snapshotEvery = ReadInt(args, ref i, "--snapshot-every"); break;
                    case "--scenario": scenarioFile = ReadPath(args, ref i, "--scenario"); break;
                    case "--inputs": inputsFile = ReadPath(args, ref i, "--inputs"); break;
                    case "--out": outPath = ReadPath(args, ref i, "--out"); break;
                    default: throw new FormatException("unknown argument " + args[i]);
                }
            }

            if (string.IsNullOrEmpty(outPath)) throw new FormatException("--out is required");
            if (night != 1) throw new FormatException("Phase 1 ships Night 1 only (got --night " + night.ToString(CultureInfo.InvariantCulture) + ")");

            Scenario scenario = null;
            if (!string.IsNullOrEmpty(scenarioFile))
                scenario = Loader.ScenarioFromJson(File.ReadAllText(Path.GetFullPath(scenarioFile), Encoding.UTF8));

            List<InputFrame> inputs = null;
            if (!string.IsNullOrEmpty(inputsFile))
                inputs = Loader.InputsFromJson(File.ReadAllText(Path.GetFullPath(inputsFile), Encoding.UTF8), (int)ticks);

            List<JsonValue> lines = Engine.Run(seed, scenario, inputs, ticks, snapshotEvery);

            StringBuilder sb = new StringBuilder();
            foreach (JsonValue line in lines)
            {
                sb.Append(line.Serialize());
                sb.Append('\n');
            }
            File.WriteAllText(Path.GetFullPath(outPath), sb.ToString(), new UTF8Encoding(false));
        }

        private static int ReadInt(string[] args, ref int i, string name)
        {
            if (i + 1 >= args.Length) throw new FormatException(name + " needs a value");
            int v;
            if (!int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                throw new FormatException(name + " needs an integer, got " + args[i]);
            return v;
        }

        private static string ReadPath(string[] args, ref int i, string name)
        {
            if (i + 1 >= args.Length) throw new FormatException(name + " needs a path");
            return args[++i];
        }
    }
}
