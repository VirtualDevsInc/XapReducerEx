using System;

using MVeldhuizen.XapReduce.Res;

namespace MVeldhuizen.XapReduce.Util
{
    public static class StorageUtil
    {
        // Considered using kibibytes and mebibytes for more correctness, but most people don't know them.

        #region Constants

        private const int KibiByte = 1024;
        private const int MebiByte = 1048576;

        #endregion

        #region Public Methods and Operators

        public static string PrettyPrintBytes(long bytes)
        {
            if (bytes >= MebiByte)
            {
                return String.Format("{0:0.0} " + Output.MebiBytes, (decimal)bytes / MebiByte);
            }

            if (bytes >= KibiByte)
            {
                return String.Format("{0} " + Output.KibiBytes, bytes / KibiByte);
            }

            return String.Format("{0} " + Output.Bytes, bytes);
        }

        #endregion
    }
}