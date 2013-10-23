using System;
using System.IO;
using System.IO.Compression;

using MVeldhuizen.XapReduce.IO;

namespace MVeldhuizen.XapReduce.XapHandling
{
    public class UpdateableXapFile : WritableXapFile
    {
        #region Constructors and Destructors

        public UpdateableXapFile(string outputPath, IFileSystem fileSystem) : base(outputPath, fileSystem)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Attempts to recompress an existing XAP file. If this results in a smaller file, it is replaced.
        /// </summary>
        /// <returns>true if file was succesfully recompressed.</returns>
        public Tuple<long, long> Recompress()
        {
            if (this.HasChanges)
            {
                throw new InvalidOperationException("Archive has pending changes. Save it first before attempting recompression.");
            }

            using (var ms = new MemoryStream())
            {
                using (var recompressed = new ZipArchive(ms, ZipArchiveMode.Create))
                {
                    CopyZipEntries(this.OutputArchive, recompressed, entry => true);
                }

                var buffer = ms.ToArray();
                var existingLength = this.FileSystem.FileSize(this.OutputPath);

                if (buffer.Length < existingLength)
                {
                    this.Close();
                    this.FileSystem.FileWriteAllBytes(this.OutputPath, buffer);
                    this.Load();

                    return Tuple.Create(existingLength, buffer.LongLength);
                }
                this.Close();
                return Tuple.Create(existingLength, existingLength);
            }
        }

        public override void Save()
        {
            if (!this.HasChanges)
            {
                return;
            }

            var oldManifestEntry = this.OutputArchive.GetEntry("AppManifest.xaml");
            oldManifestEntry.Delete();

            this.CreateAppManifestEntry();

            this.HasChanges = false;
        }

        #endregion

        #region Methods

        protected override void RemoveFileEntry(string fileName)
        {
            this.OutputArchive.GetEntry(fileName).Delete();
        }

        #endregion
    }
}