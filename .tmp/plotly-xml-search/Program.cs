using System;
using System.IO;
using System.Xml.Linq;

class Program
{
    static void Main()
    {
        var xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "plotly.blazor", "6.0.2", "lib", "net6.0", "Plotly.Blazor.xml");
        if (!File.Exists(xmlPath)) { Console.WriteLine("XML not found: " + xmlPath); return; }
        var doc = XDocument.Load(xmlPath);
        var members = doc.Root.Element("members").Elements("member");
        Console.WriteLine("--- PlotlyChart members ---");
        Console.WriteLine();
        var dataMember = members.FirstOrDefault(x => ((string)x.Attribute("name")).Equals("P:Plotly.Blazor.PlotlyChart.Data"));
        if (dataMember != null)
        {
            Console.WriteLine("PlotlyChart.Data XML element:");
            Console.WriteLine(dataMember);
        }
        else
        {
            Console.WriteLine("PlotlyChart.Data member not found in XML.");
        }
        foreach (var m in members)
        {
            var name = (string)m.Attribute("name");
            if (name.StartsWith("M:Plotly.Blazor.PlotlyChart") || name.StartsWith("P:Plotly.Blazor.PlotlyChart") || name.StartsWith("T:Plotly.Blazor.PlotlyChart"))
            {
                Console.WriteLine(name);
            }
        }
    }
}
