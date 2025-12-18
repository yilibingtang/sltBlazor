using System;
using System.IO;
using System.Text;

class Helper
{
    public static void DoSearch()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var pkg = Path.Combine(userProfile, ".nuget", "packages", "plotly.blazor", "6.0.2", "lib", "net6.0", "Plotly.Blazor.dll");
        if (!File.Exists(pkg)) { Console.WriteLine("missing: " + pkg); return; }
        var data = File.ReadAllBytes(pkg);
        var sb = new StringBuilder();
        for (int i=0;i<data.Length;i++){
            var b = data[i];
            if (b>=32 && b<127) sb.Append((char)b); else { sb.Append('\n'); }
        }
        var text = sb.ToString();
        string[] terms = new[]{"TraceBase","Trace","Scatter","ScatterLib","Layout","LayoutLib","Plotly","PlotlyChart","PlotData","PlotLayout","Plotly.Blazor"};
        foreach(var t in terms){
            if (text.IndexOf(t,StringComparison.OrdinalIgnoreCase)>=0) {
                Console.WriteLine($"FOUND TERM: {t}");
                var idx = text.IndexOf(t,StringComparison.OrdinalIgnoreCase);
                var start = Math.Max(0, idx-60);
                var len = Math.Min(180, text.Length - start);
                Console.WriteLine(text.Substring(start,len));
                Console.WriteLine("----");
            }
        }
    }
}
