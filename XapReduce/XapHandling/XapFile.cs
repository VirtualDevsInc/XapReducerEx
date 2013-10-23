using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

using MVeldhuizen.XapReduce.IO;

namespace MVeldhuizen.XapReduce.XapHandling
{
    public class XapFile
    {
        #region Static Fields

        protected static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        private static readonly XNamespace DeploymentNamespace = "http://schemas.microsoft.com/client/2007/deployment";

        #endregion

        #region Fields

        private readonly List<AssemblyPartInfo> _assemblyParts = new List<AssemblyPartInfo>();

        #endregion

        #region Constructors and Destructors

        public XapFile(string xapPath, IFileSystem fileSystem)
        {
            this.InputPath = xapPath;
            using (this.InputArchive = fileSystem.OpenArchive(xapPath, ZipArchiveMode.Read))
            {
                this.ReadXapManifest(this.InputArchive);
            }
        }

        protected XapFile(string xapPath, ZipArchive archive)
        {
            this.InputPath = xapPath;
            this.InputArchive = archive;
            this.ReadXapManifest(archive);
        }

        #endregion

        #region Public Properties

        public IList<AssemblyPartInfo> AssemblyParts
        {
            get
            {
                return this._assemblyParts.AsReadOnly();
            }
        }

        #endregion

        //public long GetAssemblyPartSize(string assemblyPart)
        //{
        //    XElement element = AssemblyPartsElements.
        //        Where(el => el.Attribute(XamlNamespace + "Name") != null).
        //        Single(el => el.Attribute(XamlNamespace + "Name").Value == assemblyPart);

        //    string fileName = element.Attribute("Source").Value;

        //    return InputArchive.GetEntry(fileName).Length;
        //}

        #region Properties

        protected XDocument AppManifest { get; private set; }

        protected IEnumerable<XElement> AssemblyPartsElements
        {
            get
            {
                var xElement = this.AppManifest.Element(DeploymentNamespace + "Deployment");
                if (xElement != null)
                {
                    var element = xElement.Element(DeploymentNamespace + "Deployment.Parts");
                    if (element != null)
                    {
                        return element.Elements();
                    }
                }

                return null;
            }
        }

        protected ZipArchive InputArchive { get; private set; }
        protected string InputPath { get; private set; }

        #endregion

        #region Methods

        private void ReadXapManifest(ZipArchive xap)
        {
            var appManifestEntry = xap.GetEntry("AppManifest.xaml");
            using (var stream = appManifestEntry.Open())
            {
                this.AppManifest = XDocument.Load(stream);
            }

            foreach (var e in this.AssemblyPartsElements.Where(e => e.Attribute(XamlNamespace + "Name") != null))
            {
                var name = e.Attribute(XamlNamespace + "Name").Value;
                var source = e.Attribute("Source").Value;
                var size = xap.GetEntry(source).Length;

                this._assemblyParts.Add(new AssemblyPartInfo(name, source, size));
            }
        }

        #endregion
    }
}