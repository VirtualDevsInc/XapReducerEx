using System;
using System.Collections.Generic;
using System.IO.Compression;

using MVeldhuizen.XapReduce.IO;

namespace MVeldhuizen.XapReduce.XapHandling
{
    public class RecreatedXapFile : WritableXapFile
    {
        #region Fields

        private readonly List<string> _deletedEntries;

        #endregion

        #region Constructors and Destructors

        public RecreatedXapFile(string inputPath, string outputPath, IFileSystem fileSystem) : base(inputPath, outputPath, fileSystem)
        {
            this._deletedEntries = new List<string>();
        }

        #endregion

        #region Public Methods and Operators

        public override void Save()
        {
            if (!this.HasChanges)
            {
                return;
            }

            this.CreateAppManifestEntry();

            using (var inputFile = this.FileSystem.OpenArchive(this.InputPath, ZipArchiveMode.Read))
            {
                CopyZipEntries(inputFile, this.OutputArchive, this.FilterNewXapContent);
            }

            this.HasChanges = false;
        }

        #endregion

        #region Methods

        protected override void RemoveFileEntry(string fileName)
        {
            this._deletedEntries.Add(fileName);
        }

        /// <summary>
        ///     Filters out the undesired entries from the copying process when calling CopyZipEntries method.
        /// </summary>
        /// <param name="entry">ZipArchiveEntry to filter.</param>
        /// <returns>True if entry should be included in the archive.</returns>
        private bool FilterNewXapContent(ZipArchiveEntry entry)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(entry.FullName, "AppManifest.xaml"))
            {
                return false;
            }

            if (this._deletedEntries.Contains(entry.FullName))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}