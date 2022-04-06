// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Diagnostics;
ForPHP.SettingInfo? setting = new ForPHP.SettingInfo();


try {
    using (FileStream fileStream = File.OpenRead("setting.json")) {
        setting = await JsonSerializer.DeserializeAsync<ForPHP.SettingInfo>(fileStream);
    }
} catch (Exception ex) {
    Console.WriteLine(ex.ToString());
    return;
}


if (setting == null) {
    Console.WriteLine("\"setting.json\" is not valid.");
    return;
}

Task subTask = Task.Run(() => AcceptInput());

IPAddress serverAddr = IPAddress.Parse(setting.ServerAddr);
TcpListener server = new TcpListener(serverAddr, 80);

server.Start();

while (true) {
    Console.WriteLine("waiting");
    Console.WriteLine($"Main {Thread.CurrentThread.ManagedThreadId}");
    Task<TcpClient> task = Task.Run(() => ListenClientAsync(server));

    NetworkStream stream = (await task).GetStream();

    using (StreamReader sr = new StreamReader(stream))
    using (StreamWriter sw = new StreamWriter(stream)) {
        List<string> reqStrs = new List<string>();
        string str = "";
        while (sr.Peek() > -1) {
            str += sr.ReadLine() + "\n";
        }
        Console.Write($"{str}");

        sw.WriteLine("HTTP/1.1 200 OK");
        sw.WriteLine("Content-Type: text/html; charset=UTF-8");
        sw.WriteLine("");

        string body = ProcessPHP(setting.PHPFilePath, GetQueryParams(str.Split("\n")[0]));
        try {
            sw.Write(body);
        } catch (Exception ex) {
            Console.WriteLine(ex);
        }

    }
}

async Task<TcpClient> ListenClientAsync(TcpListener server) {
    Console.WriteLine($"Listen {Thread.CurrentThread.ManagedThreadId}");
    return await server.AcceptTcpClientAsync();
}

string ProcessPHP(string filePath, string queryParam) {
    ProcessStartInfo procInfo = new ProcessStartInfo("C:\\xampp\\php\\php-cgi.exe");
    procInfo.Arguments = filePath + " " + queryParam;
    procInfo.UseShellExecute = false;
    procInfo.RedirectStandardOutput = true;

    Process? proc = Process.Start(procInfo);
    if (proc == null) {
        return "failed";
    }

    string result = "undefined";
    result = proc.StandardOutput.ReadToEnd();

    proc.WaitForExit();

    return result;
}

string GetQueryParams(string str) {
    if (!str.Contains("?")) return "";

    string queryStr = str.Split("?")[1];
    queryStr = queryStr.Split(" ")[0].Replace('&', ' ');
    Console.WriteLine($"getQuery: {queryStr}");
    return queryStr;
}

static void AcceptInput() {
    Console.WriteLine($"Accept {Thread.CurrentThread.ManagedThreadId}");
    string? str = Console.ReadLine();
    if (str == "exit") Environment.Exit(0);
}
