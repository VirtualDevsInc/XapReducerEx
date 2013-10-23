using System;
using System.IO.Compression;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MVeldhuizen.XapReduce.IO;
using MVeldhuizen.XapReduce.Tests.Harness;

using NSubstitute;

namespace MVeldhuizen.XapReduce.XapHandling.Tests
{
    [TestClass]
    public class UpdateableXapFileTests
    {
        #region Public Methods and Operators

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Recompress_FileHasPendingChanges_ThrowsException()
        {
            var fileSystem = Substitute.For<IFileSystem>();

            var builder = new XapBuilder(CompressionLevel.NoCompression);
            builder.AddAssemblyPart("A", "A.dll", 10000);
            builder.AddAssemblyPart("B", "B.dll", 10000);
            builder.Build();

            fileSystem.FileExists("Input.xap").Returns(true);
            fileSystem.OpenArchive("Input.xap", ZipArchiveMode.Update).Returns(a => builder.GetArchive(ZipArchiveMode.Update));

            var target = new UpdateableXapFile("Input.xap", fileSystem);
            target.RemoveAssemblyPart(target.AssemblyParts[0]);
            target.Recompress();
        }

        #endregion
    }
}