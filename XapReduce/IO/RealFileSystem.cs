using System.IO;
using System.IO.Compression;

namespace MVeldhuizen.XapReduce.IO
{
    public class RealFileSystem : IFileSystem
    {
        #region Static Fields

        private static readonly RealFileSystem _instance = new RealFileSystem();

        #endregion

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit

        #region Constructors and Destructors

        static RealFileSystem()
        {
        }

        #endregion

        #region Public Properties

        public static RealFileSystem Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region Public Methods and Operators

        public void FileDelete(string path)
        {
            File.Delete(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public long FileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public void FileWriteAllBytes(string path, byte[] buffer)
        {
            File.WriteAllBytes(path, buffer);
        }

        public ZipArchive OpenArchive(string path, ZipArchiveMode mode)
        {
            return ZipFile.Open(path, mode);
        }

        #endregion
    }
}