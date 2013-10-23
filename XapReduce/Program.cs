using System;
using System.IO;
using System.Linq;

using MVeldhuizen.XapReduce.IO;
using MVeldhuizen.XapReduce.Res;
using MVeldhuizen.XapReduce.Util;

namespace MVeldhuizen.XapReduce
{
    internal static class Program
    {
        #region Constructors and Destructors

        static Program()
        {
            FileSystem = RealFileSystem.Instance;
            Minifier = new XapMinifier(FileSystem);
            Output = Console.Out;
        }

        #endregion

        #region Properties

        internal static IFileSystem FileSystem { get; set; }

        internal static IXapMinifier Minifier { get; set; }

        internal static TextWriter Output { private get; set; }

        #endregion

        #region Methods

        internal static int CheckCommandLineOptions(Options options, IFileSystem fileSystem)
        {
            var value = 0;
            if (options.Inputs != null && options.Inputs.Length == 1 && !options.Inputs[0].ToLowerInvariant().Equals("all"))
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < options.Inputs.Length; i++)
                {
                    if (!fileSystem.FileExists(options.Inputs[i]))
                    {
                        Console.WriteLine(Errors.InputFileDoesNotExist, options.Inputs[i]);
                        value = 2;
                        break;
                    }

                    if (options.Sources == null || options.Sources.Length == 0)
                    {
                        Console.WriteLine(Errors.AtLeastOneSourceFileRequired);
                        value = 1;
                        break;
                    }

                    var missingFiles = options.Sources.Where(f => !fileSystem.FileExists(f)).ToList();
                    if (missingFiles.Count > 0 && !options.IgnoreMissing)
                    {
                        Console.WriteLine(Errors.SourceFilesMissing, @"  " + String.Join("  \r\n", missingFiles));
                        value = 3;
                        break;
                    }
                }
            }
            else
            {
                if (options.Sources == null || options.Sources.Length == 0)
                {
                    Console.WriteLine(Errors.AtLeastOneSourceFileRequired);
                    return 1;
                }

                var missingFiles = options.Sources.Where(f => !fileSystem.FileExists(f)).ToList();
                if (missingFiles.Count > 0 && !options.IgnoreMissing)
                {
                    Console.WriteLine(Errors.SourceFilesMissing, @"  " + String.Join("  \r\n", missingFiles));
                    return 3;
                }
            }

            return value;
        }

        internal static int Main(string[] args)
        {
            var options = Options.ParseCommandLine(args, Output);
            if (options.Exit)
            {
                return 1;
            }

            var errorCode = CheckCommandLineOptions(options, FileSystem);
            if (errorCode != 0)
            {
                Output.Flush();
                return errorCode;
            }

            try
            {
                Minifier.ReduceXap(options);
            }
            catch (Exception ex)
            {
                Output.WriteLine(Res.Output.UnexpectedException, ex.Message);
                return 1000;
            }

            return 0;
        }

        internal static string ReportFileSizeReduction(long oldSize, long newSize)
        {
            return String.Format(Res.Output.FileSizeReduction, StorageUtil.PrettyPrintBytes(oldSize), StorageUtil.PrettyPrintBytes(newSize),
                StorageUtil.PrettyPrintBytes(oldSize - newSize));
        }

        #endregion
    }
}