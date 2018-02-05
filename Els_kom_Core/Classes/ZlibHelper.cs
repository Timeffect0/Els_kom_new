namespace Els_kom_Core.Classes
{
    /// <summary>
    /// Zlib Compression and Decompression Helper Class.
    /// </summary>
    public static class ZlibHelper
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
                try
                {
                    outZStream.finish();
                }
                catch (Zlib.ZStreamException)
                {
                    //MessageManager.ShowError("Stream Error.", "Error!");
                }
                outData = outMemoryStream.ToArray();
            }
        }

        private static void CopyStream(System.IO.Stream input, System.IO.Stream output, int size)
        {
            //input.CopyTo(output, size);
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                if (len == size)
                {
                    MessageManager.ShowInfo("Size and len are the same.", "Debug!");
                }
                try
                {
                    // this oddly causes and overflow exception.
                    output.Write(buffer, 0, len);
                }
                catch (System.OverflowException)
                {
                    //MessageManager.ShowError("The decompression resulted in an data overflow.", "Error!");
                }
            }
            output.Flush();
        }

        /// <summary>
        /// Compresses a file.
        /// </summary>
        public static void CompressFile(string inFile, string outFile, int size)
        {
            System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
            Zlib.ZOutputStream outZStream = new Zlib.ZOutputStream(outFileStream, Zlib.zlibConst.Z_DEFAULT_COMPRESSION);
            System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);
            try
            {
                CopyStream(inFileStream, outZStream, size);
            }
            finally
            {
                outZStream.Close();
                outFileStream.Close();
                inFileStream.Close();
            }
        }

        /// <summary>
        /// Decompresses a file.
        /// </summary>
        public static void DecompressFile(string inFile, string outFile, int size)
        {
            System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
            Zlib.ZOutputStream outZStream = new Zlib.ZOutputStream(outFileStream);
            System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);
            try
            {
                CopyStream(inFileStream, outZStream, size);
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
