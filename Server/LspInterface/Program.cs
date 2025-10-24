using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK3Analyser.LspInterface
{
    public class Program
    {
        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var stdin = Console.OpenStandardInput();
            var stdout = Console.OpenStandardOutput();

            while (true)
            {
                var message = await ReadMessageAsync(stdin);
                if (message == null) break;

                try
                {
                    var request = JsonSerializer.Deserialize<JsonElement>(message);

                    if (request.TryGetProperty("command", out var command))
                    {
                        var cmd = command.GetString();
                        if (cmd == "ping")
                        {
                            await SendMessageAsync(stdout, new { status = "ok", message = "pong" });
                        }
                        else if (cmd == "exit")
                        {
                            await SendMessageAsync(stdout, new { status = "ok", message = "goodbye" });
                            break;
                        }
                        else
                        {
                            await SendMessageAsync(stdout, new { status = "error", message = "unknown command" });
                        }
                    }
                    else
                    {
                        await SendMessageAsync(stdout, new { status = "error", message = "missing command" });
                    }
                }
                catch (Exception ex)
                {
                    await SendMessageAsync(stdout, new { status = "error", message = ex.Message });
                }
            }
        }

        public static async Task<string> ReadMessageAsync(Stream input)
        {
            using var reader = new StreamReader(input, Encoding.UTF8, false, 1024, true);

            // Example header: "Content-Length: 123\r\n\r\n"
            string? headerLine;
            int contentLength = 0;

            while (!string.IsNullOrEmpty(headerLine = await reader.ReadLineAsync()))
            {
                if (headerLine.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    var lenStr = headerLine.Substring("Content-Length:".Length).Trim();
                    if (int.TryParse(lenStr, out var len))
                        contentLength = len;
                }
            }

            if (contentLength == 0) return null;

            var buffer = new char[contentLength];
            int read = 0;
            while (read < contentLength)
            {
                int n = await reader.ReadAsync(buffer, read, contentLength - read);
                if (n == 0) break; // EOF
                read += n;
            }

            return new string(buffer, 0, read);
        }

        public static async Task SendMessageAsync(Stream output, object message)
        {
            var json = JsonSerializer.Serialize(message);
            var content = Encoding.UTF8.GetBytes(json);
            var header = Encoding.ASCII.GetBytes($"Content-Length: {content.Length}\r\n\r\n");

            await output.WriteAsync(header, 0, header.Length);
            await output.WriteAsync(content, 0, content.Length);
            await output.FlushAsync();
        }
    }
}
