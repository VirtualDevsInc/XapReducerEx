using System.Globalization;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MVeldhuizen.XapReduce.Util.Tests
{
    [TestClass]
    public class StorageUtilTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void PrettyPrintBytes_Number1048576_ShowsMegaBytes()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var actual = StorageUtil.PrettyPrintBytes(1048576);
            Assert.AreEqual("1.0 MB", actual);
        }

        [TestMethod]
        public void PrettyPrintBytes_NumberBelow1024_ShowsBytes()
        {
            var actual = StorageUtil.PrettyPrintBytes(1023);
            Assert.AreEqual("1023 B", actual);
        }

        [TestMethod]
        public void PrettyPrintBytes_NumberBelow1048576_ShowsKiloBytes()
        {
            var actual = StorageUtil.PrettyPrintBytes(1048575);
            Assert.AreEqual("1023 KB", actual);
        }

        #endregion
    }
}