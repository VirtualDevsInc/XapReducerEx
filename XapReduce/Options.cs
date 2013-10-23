using System;
using System.IO;
using System.Linq;
using System.Reflection;

using CommandLine;
using CommandLine.Text;

namespace MVeldhuizen.XapReduce
{
    /// <summary>
    ///     Command line options for the utility.
    /// </summary>
    public class Options
    {
        #region Public Properties

        public bool Exit { get; set; }

        [Option('m', "ignore-missing", HelpText = "Source file to check for duplicate assemblies.")]
        public bool IgnoreMissing { get; set; }

        [OptionArray('i', "inputs", HelpText = "XAP files to reduce in size.")]
        public string[] Inputs { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [OptionArray('o', "outputs", HelpText = "Output filename(s) after optimization. If omitted, the original file will be updated.")]
        public string[] Outputs { get; set; }

        [Option('r', "recompress", HelpText = "Attempts to recompress the XAP file afterwards.")]
        public bool Recompress { get; set; }

        [OptionArray('s', "sources", HelpText = "Source file to check for duplicate assemblies.")]
        public string[] Sources { get; set; }

        #endregion

        #region Public Methods and Operators

        public static Options ParseCommandLine(string[] args, TextWriter helpWriter)
        {
            var options = new Options();
            var parser = new Parser(settings =>
            {
                settings.HelpWriter = helpWriter;
                settings.IgnoreUnknownArguments = false;
            });

            Action onFail = () =>
            {
                HelpText.AutoBuild(options);
                options.Exit = true;
            };

            if (parser.ParseArgumentsStrict(args, options, onFail))
            {
                return options;
            }

            return options;
        }

        [HelpOption]
        public string GetUsage()
        {
            var version =
                Assembly.GetExecutingAssembly().CustomAttributes.Single(attr => attr.AttributeType == typeof(AssemblyFileVersionAttribute)).ConstructorArguments[0].Value as
                string;

            var help = new HelpText
                           {
                               Heading = new HeadingInfo("XapReduce", version),
                               Copyright = new CopyrightInfo(Res.Res.Author, 2013),
                               AdditionalNewLineAfterOption = true,
                               AddDashesToOption = true
                           };

            help.AddPreOptionsLine("Thorarin.NET - http://thorarin.net/blog");
            help.AddPreOptionsLine("\r\nRemoves unneeded assemblies from a XAP file in modular Silverlight applications.\r\n");

            help.AddPreOptionsLine("Usage: XapReduce -i XapToShrink.xap [...] -s PreviouslyLoadedXap.xap [...]");
            help.AddOptions(this);

            help.AddPostOptionsLine("This software is licensed under the MIT license.");

            return help;
        }

        #endregion
    }
}