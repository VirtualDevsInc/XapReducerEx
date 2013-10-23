namespace MVeldhuizen.XapReduce.XapHandling
{
    public struct AssemblyPartInfo
    {
        #region Fields

        private readonly string _assemblyName;
        private readonly string _fileName;
        private readonly long _size;

        #endregion

        #region Constructors and Destructors

        public AssemblyPartInfo(string assemblyName, string fileName, long size)
        {
            this._assemblyName = assemblyName;
            this._fileName = fileName;
            this._size = size;
        }

        #endregion

        #region Public Properties

        public string AssemblyName
        {
            get
            {
                return this._assemblyName;
            }
        }

        public string FileName
        {
            get
            {
                return this._fileName;
            }
        }

        public long Size
        {
            get
            {
                return this._size;
            }
        }

        #endregion
    }
}