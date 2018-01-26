using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SBM_CommitFix
{
    internal class Logger
    {
        private readonly Options _options;
        private int Counter { get; set; }
        private bool WasDot { get; set; }

        public Logger(Options options)
        {
            _options = options;
        }
        public void PrintProgress(int amount = 1)
        {
            Counter += amount;
            if (Counter % 1000 == 0)
            {
                Console.Error.Write('.');
                WasDot = true;
            }
        }


        public void WriteLineVerbose(string text)
        {
            if (_options.Verbose)
            {
                MoveOutputToNextLine();

                Console.WriteLine(text);
            }
        }

        public void WriteLine(string text)
        {
            MoveOutputToNextLine();

            Console.WriteLine(text);
        }

        private void MoveOutputToNextLine()
        {
            if (WasDot && !Console.IsOutputRedirected)
            {
                // Make sure normal log output starts on a new line
                WasDot = false;
                Console.Error.WriteLine();
            }
        }

    }

    public static class CommitExtensions
    {
        public static JArray AsJArray(this string json)
        {
            return JArray.Parse(json);
        }

        public static JObject AsJObject(this string json)
        {
            return JObject.Parse(json);
        }

        public static string EventType(this JToken @event)
        {
            return @event.Body()["$type"].ToString();
        }

        public static JToken Body(this JToken @event)
        {
            return @event["Body"];
        }

        public static JToken Headers(this JToken @event)
        {
            return @event["Headers"];
        }

        public static JToken SetHeaders(this JToken @event, JToken headers)
        {
            @event["Headers"] = headers;

            return @event;
        }

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
}