using System.IO.Compression;

namespace MVeldhuizen.XapReduce.IO
{
    public interface IFileSystem
    {
        #region Public Methods and Operators

        void FileDelete(string path);
        bool FileExists(string path);
        long FileSize(string path);
        void FileWriteAllBytes(string path, byte[] buffer);

        ZipArchive OpenArchive(string path, ZipArchiveMode mode);

        #endregion
    }
}