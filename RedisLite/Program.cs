using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

class Program
{
    static Dictionary<string, string> dataStore = new Dictionary<string, string>();

    static void Main(string[] args)
    {
        var server = new TcpListener(IPAddress.Any, 6379);
        server.Start();
        Console.WriteLine("Server started on port 6379");

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected");

            var stream = client.GetStream();
            var reader = new System.IO.StreamReader(stream, Encoding.UTF8);
            var writer = new System.IO.StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            string request;
            while ((request = reader.ReadLine()) != null)
            {
                Console.WriteLine($"Received: {request.Trim()}");

                var response = HandleCommand(request.Trim());
                writer.WriteLine(response);
            }

            client.Close();
        }
    }

    static string HandleCommand(string command)
    {
        var parts = command.Split(' ');
        var cmd = parts[0].ToUpper();
        switch (cmd)
        {
            case "PING":
                return "+PONG";
            case "SET":
                if (parts.Length < 3) return "-ERR wrong number of arguments for 'set' command";
                dataStore[parts[1]] = parts[2];
                return "+OK";
            case "GET":
                if (parts.Length < 2) return "-ERR wrong number of arguments for 'get' command";
                if (dataStore.TryGetValue(parts[1], out var value))
                {
                    return $"${value.Length}\r\n{value}";
                }
                return "$-1";
            default:
                return "-ERR unknown command";
        }
    }
}
