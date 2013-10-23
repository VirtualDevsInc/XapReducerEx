using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MVeldhuizen.XapReduce.IO;
using MVeldhuizen.XapReduce.Res;
using MVeldhuizen.XapReduce.Util;
using MVeldhuizen.XapReduce.XapHandling;

namespace MVeldhuizen.XapReduce
{
    internal class XapMinifier : IXapMinifier
    {
        #region Fields

        private readonly TextWriter _console;
        private readonly IFileSystem _fileSystem;

        #endregion

        #region Constructors and Destructors

        public XapMinifier(IFileSystem fileSystem) : this(fileSystem, Console.Out)
        {
        }

        public XapMinifier(IFileSystem fileSystem, TextWriter console)
        {
            this._fileSystem = fileSystem;
            this._console = console;
        }

        #endregion

        #region Public Methods and Operators

        public IList<AssemblyPartInfo> GetRedundantAssemblyParts(string[] sources, XapFile xap)
        {
            var sourceXaps = sources.Where(this._fileSystem.FileExists).Select(file => new XapFile(file, this._fileSystem)).ToList();

            var redundantAssemblyParts = sourceXaps.SelectMany(source => source.AssemblyParts).Distinct().Intersect(xap.AssemblyParts).ToList();

            return redundantAssemblyParts.Distinct().ToList();
        }

        public void RecompressXap(UpdateableXapFile xap)
        {
            xap.Load();
            var recompressed = xap.Recompress();
            if (recompressed.Item1 > recompressed.Item2)
            {
                this._console.WriteLine(Output.RecompressSucceeded, StorageUtil.PrettyPrintBytes(recompressed.Item1),
                    StorageUtil.PrettyPrintBytes(recompressed.Item1 - recompressed.Item2));
            }
            else
            {
                this._console.WriteLine(Output.RecompressCanceled);
            }
        }

        public void ReduceXap(Options options)
        {
            if (options.Inputs != null && options.Inputs.Length == 1 && options.Inputs[0].ToLowerInvariant().Equals("all"))
            {
                if (options.Sources != null && options.Sources.Length > 1)
                {
                    this.ReduceXapSourceRedundancy(options);
                }

                foreach (var file in (from item in Directory.EnumerateFiles(Environment.CurrentDirectory)
                                      select new FileInfo(item)))
                {
                    if (options.Sources != null && file != null && (from item in options.Sources
                                                                    where item.Equals(file.Name)
                                                                    select item).Any())
                    {
                        continue;
                    }

                    if (file != null && file.Extension.Equals(".xap"))
                    {
                        var oldSize = this._fileSystem.FileSize(file.Name);

                        WritableXapFile xap = new UpdateableXapFile(file.Name, this._fileSystem);

                        using (xap)
                        {
                            var redundantAssemblyParts = this.GetRedundantAssemblyParts(options.Sources, xap);

                            this._console.WriteLine(Output.RedundantAssemblyParts, redundantAssemblyParts.Count);
                            this._console.Write(Environment.NewLine);

                            this.RemoveAssemblyParts(xap, redundantAssemblyParts);
                            xap.Save();
                            xap.Close();

                            var newSize = this._fileSystem.FileSize(file.Name);

                            this._console.Write(Environment.NewLine);
                            this._console.WriteLine(Program.ReportFileSizeReduction(oldSize, newSize));

                            if (options.Recompress)
                            {
                                this.RecompressXap((UpdateableXapFile)xap);
                            }
                        }
                    }
                }
            }
            else
            {
                if (options.Inputs != null && options.Inputs.Any())
                {
                    for (var i = 0; i < options.Inputs.Length; i++)
                    {
                        var oldSize = this._fileSystem.FileSize(options.Inputs[i]);

                        WritableXapFile xap;

                        if (options.Outputs != null)
                        {
                            xap = new RecreatedXapFile(options.Inputs[i], options.Outputs[i], this._fileSystem);
                        }
                        else
                        {
                            xap = new UpdateableXapFile(options.Inputs[i], this._fileSystem);
                        }

                        using (xap)
                        {
                            var redundantAssemblyParts = this.GetRedundantAssemblyParts(options.Sources, xap);

                            this._console.WriteLine(Output.RedundantAssemblyParts, redundantAssemblyParts.Count);
                            this._console.Write(Environment.NewLine);

                            this.RemoveAssemblyParts(xap, redundantAssemblyParts);
                            xap.Save();
                            xap.Close();

                            var newSize = this._fileSystem.FileSize(options.Inputs[i]);

                            this._console.Write(Environment.NewLine);
                            this._console.WriteLine(Program.ReportFileSizeReduction(oldSize, newSize));

                            if (options.Recompress)
                            {
                                this.RecompressXap((UpdateableXapFile)xap);
                            }
                        }
                    }
                }
            }
        }

        public Options ReduceXapSourceRedundancy(Options options)
        {
            for (var i = 1; i < options.Sources.Length; i++)
            {
                var oldSize = this._fileSystem.FileSize(options.Sources[i]);

                WritableXapFile xap;

                if (options.Outputs != null)
                {
                    xap = new RecreatedXapFile(options.Inputs[i], options.Outputs[i], this._fileSystem);
                }
                else
                {
                    xap = new UpdateableXapFile(options.Sources[i], this._fileSystem);
                }

                using (xap)
                {
                    var strings = new List<string>();

                    for (var i2 = 0; i2 < i; i2++)
                    {
                        strings.Add(options.Sources[i2]);
                    }

                    var redundantAssemblyParts = this.GetRedundantAssemblyParts(strings.ToArray(), xap);

                    this._console.WriteLine(Output.RedundantAssemblyParts, redundantAssemblyParts.Count);
                    this._console.Write(Environment.NewLine);

                    this.RemoveAssemblyParts(xap, redundantAssemblyParts);
                    xap.Save();
                    xap.Close();

                    var newSize = this._fileSystem.FileSize(options.Sources[i]);

                    this._console.Write(Environment.NewLine);
                    this._console.WriteLine(Program.ReportFileSizeReduction(oldSize, newSize));

                    if (options.Recompress)
                    {
                        this.RecompressXap((UpdateableXapFile)xap);
                    }
                }
            }
            return new Options();
        }

        public void RemoveAssemblyParts(WritableXapFile xap, IEnumerable<AssemblyPartInfo> assemblyParts)
        {
            foreach (var assemblyPart in assemblyParts)
            {
                xap.RemoveAssemblyPart(assemblyPart);

                this._console.WriteLine(Output.RemovedAssemblyPart, assemblyPart.FileName, StorageUtil.PrettyPrintBytes(assemblyPart.Size));
            }
        }

        #endregion
    }
}