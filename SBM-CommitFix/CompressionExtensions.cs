using System.IO;
using System.IO.Compression;

public static class CompressionExtensions
{
    /// <summary>
    /// Compresses the specified <paramref name="rawBytes"/>.
    /// </summary>
    /// <param name="rawBytes">The bytes that must be compressed</param>
    /// <returns>
    /// A byte[] containing the compressed equivalent of the <paramref name="rawBytes"/>
    /// </returns>
    public static byte[] Compress(this byte[] rawBytes)
    {
        using (var output = new MemoryStream())
        {
            using (var deflateStream = new DeflateStream(output, CompressionMode.Compress, true))
            {
                deflateStream.Write(rawBytes, 0, rawBytes.Length);
            }
            return output.ToArray();
        }
    }

    /// <summary>
    /// Decompresses the specified <paramref name="compressedBytes"/>.
    /// </summary>
    /// <param name="compressedBytes">The bytes that must be decompressed</param>
    /// <returns>
    /// A byte[] containing the decompressed equivalent of the <paramref name="compressedBytes"/>
    /// </returns>
    public static byte[] Decompress(this byte[] compressedBytes)
    {
        using (var stream = new DeflateStream(new MemoryStream(compressedBytes), CompressionMode.Decompress, true))
        {
            const int size = 4096;
            var buffer = new byte[size];
            using (var memory = new MemoryStream())
            {
                int count;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                } while (count > 0);
                return memory.ToArray();
            }
        }
    }
}