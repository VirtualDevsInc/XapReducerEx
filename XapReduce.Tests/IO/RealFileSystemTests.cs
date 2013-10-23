using System.IO;
using System.IO.Compression;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MVeldhuizen.XapReduce.IO.Tests
{
    [TestClass]
    public class RealFileSystemTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void FileDelete_FileExists_FileIsDeleted()
        {
            IFileSystem fileSystem = RealFileSystem.Instance;
            var tempFile = Path.GetTempFileName();

            try
            {
                Assert.IsTrue(File.Exists(tempFile));

                fileSystem.FileDelete(tempFile);
                Assert.IsFalse(File.Exists(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [TestMethod]
        public void FileExists_FileDoesNotExist_ReturnsFalse()
        {
            IFileSystem fileSystem = RealFileSystem.Instance;
            var tempFile = Path.GetTempFileName();
            File.Delete(tempFile);

            var actual = fileSystem.FileExists(tempFile);
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void FileExists_FileExists_ReturnsTrue()
        {
            IFileSystem fileSystem = RealFileSystem.Instance;
            var tempFile = Path.GetTempFileName();

            try
            {
                var actual = fileSystem.FileExists(tempFile);
                Assert.IsTrue(actual);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void FileSize_EmptyFile_ReturnsZero()
        {
            IFileSystem fileSystem = RealFileSystem.Instance;

            var tempFile = Path.GetTempFileName();
            var actual = fileSystem.FileSize(tempFile);
            Assert.AreEqual(0, actual);

            File.Delete(tempFile);
        }

        [TestMethod]
        public void FileWriteAllBytesTest_WritingMemoryBuffer_FileOverwritten()
        {
            IFileSystem fileSystem = RealFileSystem.Instance;
            var tempFile = Path.GetTempFileName();

            try
            {
                var buffer = new byte[4096];
                fileSystem.FileWriteAllBytes(tempFile, buffer);
                Assert.AreEqual(buffer.Length, fileSystem.FileSize(tempFile));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void OpenArchiveTest_FileAlreadyExists_ThrowsException()
        {
            IFileSystem fileSystem = RealFileSystem.Instance;
            var tempFile = Path.GetTempFileName();

            try
            {
                fileSystem.OpenArchive(tempFile, ZipArchiveMode.Create);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void OpenArchiveTest_VariousArchiveModes_ReturnsArchive()
        {
            IFileSystem fileSystem = RealFileSystem.Instance;
            var tempFile = Path.GetTempFileName();
            File.Delete(tempFile);

            try
            {
                using (var actual = fileSystem.OpenArchive(tempFile, ZipArchiveMode.Create))
                {
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(ZipArchiveMode.Create, actual.Mode);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }

            try
            {
                using (var actual = fileSystem.OpenArchive(tempFile, ZipArchiveMode.Update))
                {
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(ZipArchiveMode.Update, actual.Mode);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        #endregion
    }
}