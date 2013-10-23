using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace MVeldhuizen.XapReduce.Tests.Harness
{
    internal class XapBuilder
    {
        #region Static Fields

        protected static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        private static readonly XNamespace DeploymentNamespace = "http://schemas.microsoft.com/client/2007/deployment";

        #endregion

        #region Fields

        private readonly CompressionLevel _compressionLevel = CompressionLevel.Optimal;

        private readonly MemoryStream _ms = new MemoryStream();
        private readonly XElement _parts;
        private readonly Random _random = new Random();
        private ZipArchive _archive;

        #endregion

        #region Constructors and Destructors

        public XapBuilder() : this(CompressionLevel.Optimal)
        {
        }

        public XapBuilder(CompressionLevel compressionLevel)
        {
            this._compressionLevel = compressionLevel;

            this.AppManifest =
                new XDocument(new XElement(DeploymentNamespace + "Deployment", new XAttribute("xmlns", DeploymentNamespace),
                    new XAttribute(XNamespace.Xmlns + "x", XamlNamespace), this._parts = new XElement(DeploymentNamespace + "Deployment.Parts")));

            this._archive = new ZipArchive(this._ms, ZipArchiveMode.Create, true);
        }

        #endregion

        #region Properties

        private XDocument AppManifest { get; set; }

        #endregion

        #region Public Methods and Operators

        public void AddAssemblyPart(string name, string source, int fileSize)
        {
            if (this._archive == null)
            {
                throw new InvalidOperationException("XAP is already built.");
            }

            var element = new XElement(DeploymentNamespace + "AssemblyPart", new XAttribute(XamlNamespace + "Name", name), new XAttribute("Source", source));

            this._parts.Add(element);

            var entry = this._archive.CreateEntry(source, this._compressionLevel);
            using (var fileStream = entry.Open())
            {
                var remaining = fileSize;
                while (remaining > 0)
                {
                    var buffer = new byte[Math.Min(remaining, 4096)];
                    this._random.NextBytes(buffer);
                    fileStream.Write(buffer, 0, buffer.Length);
                    remaining -= buffer.Length;

                    if (remaining > buffer.Length)
                    {
                        fileStream.Write(buffer, 0, buffer.Length);
                        remaining -= buffer.Length;
                    }
                }
            }
        }

        public MemoryStream Build()
        {
            if (this._archive != null)
            {
                using (this._archive)
                {
                    // Create AppManifest.xaml
                    var appManifestEntry = this._archive.CreateEntry("AppManifest.xaml", this._compressionLevel);
                    using (var stream = appManifestEntry.Open())
                    {
                        this.AppManifest.Save(stream);
                    }
                }

                this._archive = null;
            }

            return this._ms;
        }

        public ZipArchive GetArchive(ZipArchiveMode mode = ZipArchiveMode.Read)
        {
            var clonedStream = new MemoryStream(this._ms.ToArray());
            return new ZipArchive(clonedStream, mode);
        }

        public long GetSize()
        {
            return this._ms.Length;
        }

        #endregion
    }
}