using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;

using MVeldhuizen.XapReduce.IO;

namespace MVeldhuizen.XapReduce.XapHandling
{
    public abstract class WritableXapFile : XapFile, IDisposable
    {
        #region Constructors and Destructors

        protected WritableXapFile(string outputPath, IFileSystem fileSystem) : base(outputPath, fileSystem.OpenArchive(outputPath, ZipArchiveMode.Update))
        {
            this.FileSystem = fileSystem;
            this.OutputPath = outputPath;
            this.OutputArchive = this.InputArchive;
        }

        protected WritableXapFile(string inputPath, string outputPath, IFileSystem fileSystem) : base(inputPath, fileSystem)
        {
            this.FileSystem = fileSystem;
            this.OutputPath = outputPath;

            if (fileSystem.FileExists(outputPath))
            {
                fileSystem.FileDelete(outputPath);
            }

            this.OutputArchive = fileSystem.OpenArchive(this.OutputPath, ZipArchiveMode.Create);
            Debug.Assert(this.OutputArchive != null);
        }

        #endregion

        #region Properties

        protected IFileSystem FileSystem { get; private set; }

        protected bool HasChanges { get; set; }

        protected ZipArchive OutputArchive { get; private set; }
        protected string OutputPath { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void Close()
        {
            this.OutputArchive.Dispose();
            this.OutputArchive = null;
        }

        public void Dispose()
        {
            this.Save();
        }

        public void Load()
        {
            this.OutputArchive = this.FileSystem.OpenArchive(this.OutputPath, ZipArchiveMode.Update);
        }

        public void RemoveAssemblyPart(AssemblyPartInfo assemblyPart)
        {
            var element =
                this.AssemblyPartsElements.Where(el => el.Attribute(XamlNamespace + "Name") != null)
                    .Single(el => el.Attribute(XamlNamespace + "Name").Value == assemblyPart.AssemblyName);

            element.Remove();
            this.RemoveFileEntry(assemblyPart.FileName);
            this.HasChanges = true;
        }

        public abstract void Save();

        #endregion

        #region Methods

        protected static void CopyZipEntries(ZipArchive source, ZipArchive target, Func<ZipArchiveEntry, bool> filter)
        {
            foreach (var sourceEntry in source.Entries.Where(filter))
            {
                var targetEntry = target.CreateEntry(sourceEntry.FullName);
                targetEntry.LastWriteTime = sourceEntry.LastWriteTime;

                using (var inStream = sourceEntry.Open())
                {
                    using (var outStream = targetEntry.Open())
                    {
                        inStream.CopyTo(outStream);
                    }
                }
            }
        }

        /// <summary>
        ///     Create a new AppManifest.xaml in the archive.
        /// </summary>
        protected void CreateAppManifestEntry()
        {
            var appManifestEntry = this.OutputArchive.CreateEntry("AppManifest.xaml", CompressionLevel.Optimal);
            using (var stream = appManifestEntry.Open())
            {
                this.AppManifest.Save(stream);
            }
        }

        protected abstract void RemoveFileEntry(string fileName);

        #endregion
    }
}