using CK3Analyser.Core.Resources;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CK3Analyser.LspInterface
{
    public class Program
    {
        public Stream StdIn;
        public Stream StdOut;

        public static async Task Main()
        {
            var program = new Program();
            await program.MainLoop();
        }

        public async Task MainLoop()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            StdIn = Console.OpenStandardInput();
            StdOut = Console.OpenStandardOutput();
            var commandHandler = new CommandHandler(this);

            while (true)
            {
                string message = null;
                try
                {
                    message = await ReadMessageAsync();
                    if (message == null) break;
                }
                catch (Exception ex)
                {
                    var crashLogPath = Path.Combine(GlobalResources.Modded?.Path ?? "", "CK3BEAGLE_crash.log");
                    File.WriteAllText(crashLogPath, ex.ToString());

                    await SendMessageAsync(GetErrorMessage(ex.Message));
                }

                try
                {
                    var request = JsonSerializer.Deserialize<JsonElement>(message);

                    if (request.TryGetProperty("command", out var commandType))
                    {
                        var cmd = commandType.GetString();
                        request.TryGetProperty("payload", out var payload);
                        if (cmd == "exit")
                        {
                            await SendMessageAsync(GetBasicMessage("goodbye"));
                            break;
                        }
                        await commandHandler.HandleCommand(cmd, payload);
                    }
                    else
                    {
                        await SendMessageAsync(GetErrorMessage("malformed command"));
                    }
                }
                catch (Exception ex)
                {
                    var crashLogPath = Path.Combine(GlobalResources.Modded?.Path ?? "", "CK3BEAGLE_crash.log");
                    File.WriteAllText(crashLogPath, ex.ToString());
                    await SendMessageAsync(GetErrorMessage(ex.Message));
                }
            }
        }

        public object GetBasicMessage(string message)
        {
            return new { type = "basic", payload = new { message } };
        }

        public object GetErrorMessage(string message)
        {
            return new { type = "error", payload = new { message } };
        }

        public async Task<string> ReadMessageAsync()
        {
            using var reader = new StreamReader(StdIn, Encoding.UTF8, false, 1024, true);

            string headerLine;
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

        public async Task SendMessageAsync(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var content = Encoding.UTF8.GetBytes(json);
            var header = Encoding.ASCII.GetBytes($"Content-Length: {content.Length}\r\n\r\n");
            await StdOut.WriteAsync(header, 0, header.Length);
            await StdOut.WriteAsync(content, 0, content.Length);
            await StdOut.FlushAsync();
            Thread.Sleep(50); //Always wait at least a little bit
        }
    }
}
