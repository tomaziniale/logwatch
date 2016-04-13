using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace logwatch
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.Length > 0 ? args[0] : "u_ex160412.log";
            var lines = new List<string>();
            var logs = new List<log>();
            int charPos = 0;

            if (!File.Exists(file))
            {
                Console.WriteLine("Path not found: " + file);
                return;
            }

            Console.WriteLine("");
            Console.WriteLine("LogWatcher initializing... press Ctrl+C to break");
            Console.WriteLine("Reading file: " + file);
            Console.WriteLine("");

            while (true)
            {
                using (var fstream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8, FileOptions.Asynchronous))
                using (var stream = new StreamReader(fstream))
                {
                    char[] CRLF = new char[1] { '\r' };
                    lines = stream.ReadToEnd().Replace("\n", string.Empty).Split(CRLF).ToList();
                    int count = lines.Count;
                    lines.RemoveRange(0, charPos);
                    charPos = count;
                }

                if (lines.Count == 0) continue;

                var newFields = lines.Where(e => e.Contains("#Fields:")).LastOrDefault();
                if (newFields != null)
                {
                    logs.Clear();
                    newFields.Split(' ').Skip(1).ToList().ForEach(e => logs.Add(new log(e, null)));
                }

                foreach (var line in lines.Where(e => e[0] != '#').ToList())
                {
                    var result = line.Split(' ').ToList();
                    for (int i = 0; i < logs.Count; i++)
                    {
                        logs[i].Value = result[i];
                    }

                    int k = 0;
                    logs.ForEach(e =>
                    {
                        if (k % 2 == 0) Console.ForegroundColor = ConsoleColor.DarkGreen;
                        else Console.ResetColor();
                        Console.WriteLine(string.Format("{0}: {1} {2}", e.Header, new String(' ', 20 - e.Header.Length), e.Value));
                        k++;
                    });

                    Console.WriteLine(new String('_', Console.WindowWidth));
                    Console.WriteLine();
                    Thread.Sleep(1000);
                }

                //wait
                Thread.Sleep(1000);
            }
        }

        class log
        {
            public log(string h, string v)
            {
                Header = h;
                Value = v;
            }

            public string Header { get; set; }
            public string Value { get; set; }
        }
    }
}
