using System.IO;
using System.IO.Compression;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MVeldhuizen.XapReduce.IO;
using MVeldhuizen.XapReduce.Tests.Harness;

using NSubstitute;

namespace MVeldhuizen.XapReduce.Tests
{
    [TestClass]
    public class XapMinifierTests
    {
        #region Constructors and Destructors

        public XapMinifierTests()
        {
            // Force some jitting to be done prior to executing the actual tests
            // ReSharper disable EmptyGeneralCatchClause
            try
            {
                this.ReduceXap_UpdateExistingFile_Test();
            }
            catch
            {
            }
            // ReSharper restore EmptyGeneralCatchClause
        }

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void ReduceXap_CreateNewFile_Test()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            var console = new StringWriter();

            var inputBuilder = this.CreateFakeInputXap(fileSystem, ZipArchiveMode.Read, "A", "B");
            var sourceBuilder = this.CreateFakeSourceXap(fileSystem, "A", "C");

            var outputStream = new MemoryStream();
            fileSystem.FileExists("Output.xap").Returns(true);
            fileSystem.OpenArchive("Output.xap", ZipArchiveMode.Create).Returns(new ZipArchive(outputStream, ZipArchiveMode.Create, true));

            var options = new Options
                              {
                                  Inputs = new[]
                                               {
                                                   "Input.xap"
                                               },
                                  Sources = new[]
                                                {
                                                    "Source.xap"
                                                },
                                  Outputs = new[]
                                                {
                                                    "Output.xap"
                                                }
                              };

            var builder = new XapBuilder();
            builder.AddAssemblyPart("A", "A.dll", 1000);

            var minifier = new XapMinifier(fileSystem, console);
            minifier.ReduceXap(options);

            var output = new ZipArchive(outputStream, ZipArchiveMode.Read, true);
            Assert.AreEqual(2, output.Entries.Count);
            Assert.IsNotNull(output.GetEntry("B.dll"));
        }

        [TestMethod]
        public void ReduceXap_UpdateExistingFileWithRecompress_RecompressionCanceled()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            var console = new StringWriter();

            var inputBuilder = this.CreateFakeInputXap(fileSystem, ZipArchiveMode.Update, "A", "B");
            var sourceBuilder = this.CreateFakeSourceXap(fileSystem, "A", "C");

            var options = new Options
                              {
                                  Inputs = new[]
                                               {
                                                   "Input.xap"
                                               },
                                  Sources = new[]
                                                {
                                                    "Source.xap"
                                                },
                                  Recompress = true
                              };

            var builder = new XapBuilder();
            builder.AddAssemblyPart("A", "A.dll", 1000);

            var minifier = new XapMinifier(fileSystem, console);
            minifier.ReduceXap(options);

            var output = inputBuilder.GetArchive();
            Assert.AreEqual(2, output.Entries.Count);
            Assert.IsNotNull(output.GetEntry("B.dll"));
        }

        [TestMethod]
        public void ReduceXap_UpdateExistingFileWithRecompress_RecompressionSuccessful()
        {
            var fileSystem = Substitute.For<IFileSystem>();

            var consoleBuilder = new StringBuilder();
            var consoleOutput = new StringWriter(consoleBuilder);

            var inputBuilder = this.CreateFakeInputXap(fileSystem, ZipArchiveMode.Update, CompressionLevel.NoCompression, "A", "B");
            var sourceBuilder = this.CreateFakeSourceXap(fileSystem, "A", "C");

            fileSystem.FileSize("Input.xap").Returns(s => inputBuilder.GetSize());

            var options = new Options
                              {
                                  Inputs = new[]
                                               {
                                                   "Input.xap"
                                               },
                                  Sources = new[]
                                                {
                                                    "Source.xap"
                                                },
                                  Recompress = true
                              };

            var builder = new XapBuilder();
            builder.AddAssemblyPart("A", "A.dll", 1000);

            var minifier = new XapMinifier(fileSystem, consoleOutput);
            minifier.ReduceXap(options);

            var console = consoleBuilder.ToString();
            var output = inputBuilder.GetArchive();
            Assert.AreEqual(2, output.Entries.Count);
            Assert.IsNotNull(output.GetEntry("B.dll"));
        }

        [TestMethod]
        public void ReduceXap_UpdateExistingFile_Test()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            var console = new StringWriter();

            var inputBuilder = this.CreateFakeInputXap(fileSystem, ZipArchiveMode.Update, "A", "B");
            var sourceBuilder = this.CreateFakeSourceXap(fileSystem, "A", "C");

            var options = new Options
                              {
                                  Inputs = new[]
                                               {
                                                   "Input.xap"
                                               },
                                  Sources = new[]
                                                {
                                                    "Source.xap"
                                                }
                              };

            var builder = new XapBuilder();
            builder.AddAssemblyPart("A", "A.dll", 1000);

            var minifier = new XapMinifier(fileSystem, console);
            minifier.ReduceXap(options);

            var output = inputBuilder.GetArchive();
            Assert.AreEqual(2, output.Entries.Count);
            Assert.IsNotNull(output.GetEntry("B.dll"));
        }

        #endregion

        #region Methods

        private XapBuilder CreateFakeInputXap(IFileSystem fileSystem, ZipArchiveMode mode, params string[] assemblies)
        {
            return this.CreateFakeInputXap(fileSystem, mode, CompressionLevel.Optimal, assemblies);
        }

        private XapBuilder CreateFakeInputXap(IFileSystem fileSystem, ZipArchiveMode mode, CompressionLevel compressionLevel, params string[] assemblies)
        {
            var builder = new XapBuilder(compressionLevel);

            foreach (var assembly in assemblies)
            {
                builder.AddAssemblyPart(assembly, assembly + ".dll", 10000);
            }

            fileSystem.FileExists("Input.xap").Returns(true);
            fileSystem.OpenArchive("Input.xap", mode).Returns(a => new ZipArchive(builder.Build(), mode, true));

            return builder;
        }

        private XapBuilder CreateFakeSourceXap(IFileSystem fileSystem, params string[] assemblies)
        {
            var builder = new XapBuilder();

            foreach (var assembly in assemblies)
            {
                builder.AddAssemblyPart(assembly, assembly + ".dll", 10000);
            }

            fileSystem.FileExists("Source.xap").Returns(true);
            fileSystem.OpenArchive("Source.xap", ZipArchiveMode.Read).Returns(new ZipArchive(builder.Build()));

            return builder;
        }

        #endregion
    }
}