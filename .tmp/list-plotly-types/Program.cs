using System;
using System.IO;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        var asmPath = Path.GetFullPath("bin/Debug/net10.0/Plotly.Blazor.dll");
        if (!File.Exists(asmPath))
        {
            Console.WriteLine("Assembly not found: " + asmPath);
            return;
        }
        var asm = Assembly.LoadFrom(asmPath);
        Console.WriteLine("Loaded: " + asm.FullName);
        var types = asm.GetExportedTypes().Where(t => t.Namespace != null && (t.Namespace.Contains("Plotly") || t.Name.Contains("Plot") || t.Name.Contains("Trace") || t.Name.Contains("Scatter"))).OrderBy(t => t.FullName).ToList();
        foreach (var t in types)
        {
            Console.WriteLine(t.FullName);
        }
    }
}
