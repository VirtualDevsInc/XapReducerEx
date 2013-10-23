using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MVeldhuizen.XapReduce.IO;

using NSubstitute;

namespace MVeldhuizen.XapReduce.Tests
{
    /// <summary>
    ///     Summary description for XapReduceTests
    /// </summary>
    [TestClass]
    public class ProgramTests
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #region Public Methods and Operators

        [TestMethod]
        public void CheckCommandLineOptions_MissingInputFile_ReturnsError()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.FileExists("Input.xap").Returns(false);

            var options = new Options
                              {
                                  Inputs = new[]
                                               {
                                                   "Input.xap"
                                               },
                                  Sources = new[]
                                                {
                                                    "Source1.xap",
                                                    "Source2.xap"
                                                }
                              };

            var actual = Program.CheckCommandLineOptions(options, fileSystem);

            Assert.AreEqual(2, actual);
        }

        [TestMethod]
        public void CheckCommandLineOptions_MissingSourceFile_Ignored()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.FileExists("Input.xap").Returns(true);
            fileSystem.FileExists("Source1.xap").Returns(true);
            fileSystem.FileExists("Source2.xap").Returns(false);

            var options = new Options
                              {
                                  Inputs = new[]
                                               {
                                                   "Input.xap"
                                               },
                                  Sources = new[]
                                                {
                                                    "Source1.xap",
                                                    "Source2.xap"
                                                },
                                  IgnoreMissing = true
                              };

            var actual = Program.CheckCommandLineOptions(options, fileSystem);

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void CheckCommandLineOptions_MissingSourceFile_ReturnsError()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.FileExists("Input.xap").Returns(true);
            fileSystem.FileExists("Source1.xap").Returns(true);
            fileSystem.FileExists("Source2.xap").Returns(false);

            var options = new Options
                              {
                                  Inputs = new[]
                                               {
                                                   "Input.xap"
                                               },
                                  Sources = new[]
                                                {
                                                    "Source1.xap",
                                                    "Source2.xap"
                                                }
                              };

            var actual = Program.CheckCommandLineOptions(options, fileSystem);

            Assert.AreEqual(3, actual);
        }

        [TestMethod]
        public void Main_HelpCommandLineOption_DisplaysHelp()
        {
            var outputBuilder = new StringBuilder();
            Program.Output = new StringWriter(outputBuilder);

            const string commandLine = "--help";
            var exitCode = Program.Main(commandLine.Split(' '));
            var output = outputBuilder.ToString();

            StringAssert.Contains(output, "Copyright");
            StringAssert.Contains(output, "Marcel Veldhuizen");

            StringAssert.Contains(output, "-i,");
            StringAssert.Contains(output, "--input ");

            StringAssert.Contains(output, "-s,");
            StringAssert.Contains(output, "--sources ");

            StringAssert.Contains(output, "-r,");
            StringAssert.Contains(output, "--recompress ");

            StringAssert.Contains(output, "-m,");
            StringAssert.Contains(output, "--ignore-missing ");

            StringAssert.Contains(output, "--help ");
        }

        [TestMethod]
        public void Main_InputAndTwoSourceFilesThatExist_CallsMinifier()
        {
            Options options = null;
            var output = new StringBuilder();

            Program.FileSystem = Substitute.For<IFileSystem>();
            Program.FileSystem.FileExists("Input.xap").Returns(true);
            Program.FileSystem.FileExists("Source.xap").Returns(true);
            Program.FileSystem.FileExists("Source2.xap").Returns(true);

            Program.Minifier = Substitute.For<IXapMinifier>();
            Program.Output = new StringWriter(output);

            Program.Minifier.WhenForAnyArgs(m => m.ReduceXap(null)).Do(ci =>
            {
                options = (Options)ci.Args()[0];
            });

            const string commandLine = "-i Input.xap -s Source.xap Source2.xap";
            var exitCode = Program.Main(commandLine.Split(' '));

            Assert.AreEqual(0, exitCode);
            Assert.IsNotNull(options);
            Assert.AreEqual("Input.xap", options.Inputs[0]);
            Assert.AreEqual(2, options.Sources.Length);
        }

        [TestMethod]
        public void Main_InputThatDoesNotExist_ExitsWithErrorCode()
        {
            var output = new StringBuilder();

            Program.FileSystem = Substitute.For<IFileSystem>();
            Program.FileSystem.FileExists("Input.xap").Returns(false);
            Program.FileSystem.FileExists("Source.xap").Returns(true);
            Program.FileSystem.FileExists("Source2.xap").Returns(true);

            Program.Minifier = Substitute.For<IXapMinifier>();
            Program.Output = new StringWriter(output);

            const string commandLine = "-i Input.xap -s Source.xap Source2.xap";
            var exitCode = Program.Main(commandLine.Split(' '));

            Assert.AreEqual(2, exitCode);
            Program.Minifier.DidNotReceiveWithAnyArgs().ReduceXap(null);
        }

        [TestMethod]
        public void Main_MinifierThrowsException_ShowsExceptionOnConsole()
        {
            var outputBuilder = new StringBuilder();

            Program.FileSystem = Substitute.For<IFileSystem>();
            Program.FileSystem.FileExists("Input.xap").Returns(true);
            Program.FileSystem.FileExists("Source.xap").Returns(true);
            Program.FileSystem.FileExists("Source2.xap").Returns(true);

            Program.Minifier = Substitute.For<IXapMinifier>();
            Program.Output = new StringWriter(outputBuilder);

            Program.Minifier.WhenForAnyArgs(m => m.ReduceXap(null)).Do(ci =>
            {
                throw new Exception("Unit Test Exception.");
            });

            const string commandLine = "-i Input.xap -s Source.xap Source2.xap";
            var exitCode = Program.Main(commandLine.Split(' '));
            var output = outputBuilder.ToString();

            Assert.AreEqual(1000, exitCode);
            StringAssert.Contains(output, "Unit Test Exception.");
        }

        [TestMethod]
        public void Main_NoInputFileSpecified_ExitsWithErrorCode()
        {
            var output = new StringBuilder();

            Program.FileSystem = Substitute.For<IFileSystem>();
            Program.Minifier = Substitute.For<IXapMinifier>();
            Program.Output = new StringWriter(output);

            const string commandLine = "-s Source.xap Source2.xap";
            var exitCode = Program.Main(commandLine.Split(' '));

            Assert.AreEqual(1, exitCode);
            Program.Minifier.DidNotReceiveWithAnyArgs().ReduceXap(null);
        }

        [TestMethod]
        public void Main_NoInputFile_ExistsWithErrorCode()
        {
            var output = new StringBuilder();

            Program.FileSystem = Substitute.For<IFileSystem>();
            Program.FileSystem.FileExists("Source.xap").Returns(true);
            Program.FileSystem.FileExists("Source2.xap").Returns(true);

            Program.Minifier = Substitute.For<IXapMinifier>();
            Program.Output = new StringWriter(output);

            const string commandLine = "-s Source.xap Source2.xap";
            var exitCode = Program.Main(commandLine.Split(' '));

            Assert.AreEqual(1, exitCode);
            Program.Minifier.DidNotReceiveWithAnyArgs().ReduceXap(null);
        }

        [TestMethod]
        public void Main_NoSourceFilesSpecified_ExitsWithErrorCode()
        {
            var output = new StringBuilder();

            Program.FileSystem = Substitute.For<IFileSystem>();
            Program.FileSystem.FileExists("Input.xap").Returns(true);

            Program.Minifier = Substitute.For<IXapMinifier>();
            Program.Output = new StringWriter(output);

            const string commandLine = "-i Input.xap";
            var exitCode = Program.Main(commandLine.Split(' '));

            Assert.AreEqual(1, exitCode);
            Program.Minifier.DidNotReceiveWithAnyArgs().ReduceXap(null);
        }

        [TestMethod]
        public void Main_SomeSourcesThatDoNotExistWithIgnoreOption_CallsMinifier()
        {
            var output = new StringBuilder();

            Program.FileSystem = Substitute.For<IFileSystem>();
            Program.FileSystem.FileExists("Input.xap").Returns(true);
            Program.FileSystem.FileExists("Source.xap").Returns(true);
            Program.FileSystem.FileExists("Source2.xap").Returns(false);

            Program.Minifier = Substitute.For<IXapMinifier>();
            Program.Output = new StringWriter(output);

            const string commandLine = "-i Input.xap -s Source.xap Source2.xap -m";
            var exitCode = Program.Main(commandLine.Split(' '));

            Assert.AreEqual(0, exitCode);
            Program.Minifier.ReceivedWithAnyArgs().ReduceXap(null);
        }

        [TestMethod]
        public void Main_SomeSourcesThatDoNotExist_ExitsWithErrorCode()
        {
            var output = new StringBuilder();

            Program.FileSystem = Substitute.For<IFileSystem>();
            Program.FileSystem.FileExists("Input.xap").Returns(true);
            Program.FileSystem.FileExists("Source.xap").Returns(true);
            Program.FileSystem.FileExists("Source2.xap").Returns(false);

            Program.Minifier = Substitute.For<IXapMinifier>();
            Program.Output = new StringWriter(output);

            const string commandLine = "-i Input.xap -s Source.xap Source2.xap";
            var exitCode = Program.Main(commandLine.Split(' '));

            Assert.AreEqual(3, exitCode);
            Program.Minifier.DidNotReceiveWithAnyArgs().ReduceXap(null);
        }

        #endregion
    }
}