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
        public static void CompressData(byte[] inData, out byte[] outData)
        {
            using (System.IO.MemoryStream outMemoryStream = new System.IO.MemoryStream())
            using (Zlib.ZOutputStream outZStream = new Zlib.ZOutputStream(outMemoryStream, Zlib.zlibConst.Z_DEFAULT_COMPRESSION))
            using (System.IO.Stream inMemoryStream = new System.IO.MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        /// <summary>
        /// Decompresses data.
        /// </summary>
        public static void DecompressData(byte[] inData, out byte[] outData)
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

        private static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public void compressFile(string inFile, string outFile)
        {
            System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
            System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);
            try
            {
                CopyStream(inFileStream, outZStream);
            }
            finally
            {
                outZStream.Close();
                outFileStream.Close();
                inFileStream.Close();
            }
        }

        public void decompressFile(string inFile, string outFile)
        {
            System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream);
            System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);
            try
            {
                CopyStream(inFileStream, outZStream);
            }
            finally
            {
                outZStream.Close();
                outFileStream.Close();
                inFileStream.Close();
            }
        }
    }
}
