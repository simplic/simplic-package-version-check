using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Colorful.Console;

namespace Simplic.PackageVersionCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "";

            WriteLine(new string('-', 80));
            WriteLine($"Simplic nuget package veresion check. Copyright {DateTime.Now.Year} @ SIMPLIC GmbH");
            WriteLine(new string('-', 80));

            if (args.Length == 0)
            {
                Console.Write("Root directory: ");
                path = Console.ReadLine();
            }
            else
                path = args[0];

            if (!path.EndsWith("\\"))
                path = $"{path}\\";

            if (!Directory.Exists(path))
            {
                WriteLine($"Directory does not exists: `{path}`", System.Drawing.Color.Red);
                Environment.Exit(1);
            }

            var failedPackages = new List<string>();

            foreach (var csprojFile in Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories))
            {
                WriteLine($"Check project: {csprojFile}");

                var projectDirectory = Path.GetDirectoryName(csprojFile);
                var packageFile = $"{projectDirectory}\\packages.config";
                var projectFileCode = File.ReadAllLines(csprojFile);

                if (File.Exists(packageFile))
                {
                    WriteLine("  Has nuget packages.", System.Drawing.Color.Green);

                    // Read xml file
                    var packageXmlDocument = new XmlDocument();
                    packageXmlDocument.Load(packageFile);

                    foreach (var packageNode in packageXmlDocument.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "packages"))
                    {
                        foreach (var node in packageNode.ChildNodes.OfType<XmlNode>())
                        {
                            var packageId = node.Attributes["id"].Value;
                            var packageVersion = node.Attributes["version"].Value;

                            // if (!packageId.ToLower().Contains("simplic"))
                            //     continue;

                            WriteLine($"  Check package: {packageId} {packageVersion}");

                            foreach (var line in projectFileCode)
                            {
                                if (line.Contains($"{packageId}, Version=") && !line.Contains(packageVersion))
                                {
                                    WriteLine($" -> FAILED: Package has the wrong version: {line}", System.Drawing.Color.Red);

                                    failedPackages.Add($"Failed package: {Path.GetFileName(csprojFile)}: {line} expected: {packageId}.{packageVersion}");
                                    break;
                                }
                                if (line.Contains($"{packageId}, Version=") && line.Contains(packageVersion))
                                {
                                    WriteLine("  -> Package is correct.", System.Drawing.Color.Green);
                                }
                            }
                        }
                    }
                }
                else
                {
                    WriteLine("Skip project.", System.Drawing.Color.Yellow);
                }
            }

            WriteLine();
            WriteLine();

            if (failedPackages.Count == 0)
                WriteLine($"Failed packages: {failedPackages.Count}", System.Drawing.Color.Green);
            else
                WriteLine($"Failed packages: {failedPackages.Count}", System.Drawing.Color.Yellow);

            foreach (var failedLine in failedPackages)
                WriteLine($" {failedLine}", System.Drawing.Color.Red);

            Console.ReadLine();

            if (failedPackages.Count == 0)
                Environment.Exit(1);
        }
    }
}
