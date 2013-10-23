using System.Collections.Generic;

using MVeldhuizen.XapReduce.XapHandling;

namespace MVeldhuizen.XapReduce
{
    public interface IXapMinifier
    {
        #region Public Methods and Operators

        IList<AssemblyPartInfo> GetRedundantAssemblyParts(string[] sources, XapFile xap);
        void RecompressXap(UpdateableXapFile xap);
        void ReduceXap(Options options);
        void RemoveAssemblyParts(WritableXapFile xap, IEnumerable<AssemblyPartInfo> assemblyParts);

        #endregion
    }
}