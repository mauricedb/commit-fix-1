using System.IO;
using System.Text;

public static class ConversionExtensions
{
    public static string AsString(this byte[] data)
    {
        using (var memoryStream = new MemoryStream(data))
        {
            using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }

    public static byte[] AsBytes(this string data)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                writer.Write(data);
                writer.Flush();
                return memoryStream.ToArray();
            }
        }
    }

}