using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var pkg = Path.Combine(userProfile, ".nuget", "packages", "plotly.blazor", "6.0.2", "lib", "net6.0", "Plotly.Blazor.dll");
        if (!File.Exists(pkg)) { Console.WriteLine("missing: " + pkg); return; }
        Console.WriteLine("Reading: " + pkg);
        var xmlPath = Path.Combine(Path.GetDirectoryName(pkg), "Plotly.Blazor.xml");
        if (File.Exists(xmlPath))
        {
            Console.WriteLine("Found XML: " + xmlPath);
            var xml = File.ReadAllText(xmlPath);
            var snippet = xml.Length > 3000 ? xml.Substring(0, 3000) : xml;
            Console.WriteLine(snippet);
            return;
        }
        var data = File.ReadAllBytes(pkg);
        var sb = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            var b = data[i];
            if (b >= 32 && b < 127) sb.Append((char)b); else sb.Append('\n');
        }
        var text = sb.ToString();
        var terms = new[] { "PlotlyChart", "PlotlyChart", "Plotly.Blazor.Components", "Traces.", "TraceBase", "Trace", "Scatter", "Plotly" };
        foreach (var t in terms)
        {
            var idx = text.IndexOf(t, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                Console.WriteLine($"FOUND '{t}' at {idx}");
                var start = Math.Max(0, idx - 400);
                var len = Math.Min(800, text.Length - start);
                Console.WriteLine(text.Substring(start, len));
                Console.WriteLine("----");
            }
            else
            {
                Console.WriteLine($"NOT FOUND: {t}");
            }
        }
    }
}
