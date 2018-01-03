namespace Els_kom_Core.Classes
{
    /// <summary>
    /// Zlib Compression and Decompression Helper Class.
    /// </summary>
    public class ZlibHelper
    {
        /// <summary>
        /// Compresses data.
        /// </summary>
        public static void CompressData(byte[] inData, out byte[] outData, int size)
        {
            using (System.IO.MemoryStream outMemoryStream = new System.IO.MemoryStream())
            using (Zlib.ZOutputStream outZStream = new Zlib.ZOutputStream(outMemoryStream, Zlib.zlibConst.Z_DEFAULT_COMPRESSION))
            using (System.IO.Stream inMemoryStream = new System.IO.MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream, size);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        /// <summary>
        /// Decompresses data.
        /// </summary>
        public static void DecompressData(byte[] inData, out byte[] outData, int size)
        {
            using (System.IO.MemoryStream outMemoryStream = new System.IO.MemoryStream())
            using (Zlib.ZOutputStream outZStream = new Zlib.ZOutputStream(outMemoryStream))
            using (System.IO.Stream inMemoryStream = new System.IO.MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream, size);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        private static void CopyStream(System.IO.Stream input, System.IO.Stream output, int size)
        {
            byte[] buffer = new byte[size];
            int len;
            while ((len = input.Read(buffer, 0, size)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }
    }
}
