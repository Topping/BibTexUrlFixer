using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BibTexUrlFixer
{
    internal class Program
    {
        private static readonly string OldIdentifier = "url = ";
        private static readonly string NewIdentifyer = "note = ";
        private static readonly Regex BibtexEntryRegex = new Regex(@"(@.*{(.*|\s)*?(}\s))", RegexOptions.Multiline);
        public static void Main(string[] args)
        {
            if (args?.Length == 0 || args?.Length > 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Insufficient arguments");
                Console.ResetColor();
                PrintUsage();
                return;
            }
            var bibliographyFilePath = args[0];
            if (!File.Exists(bibliographyFilePath))
            {
                Console.WriteLine($"File not found: {bibliographyFilePath}");
                return;
            }
            if (!Path.GetExtension(bibliographyFilePath).Equals(".bib", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine($"File is not a .bib file: {bibliographyFilePath}");
                return;
            }
            
            var bibliography = File.ReadAllText(bibliographyFilePath);
            var entries = BibtexEntryRegex
                .Split(bibliography)
                .Skip(1)
                .Where(s => s.StartsWith("@"));
            
            var stringBuilder = new StringBuilder();
            int updatedEntries = 0;
            foreach (var entry in entries)
            {
                if (entry.Contains(OldIdentifier))
                {
                    var lineGroups = Regex.Match(entry, @"(url) = {(.*)}").Groups;
                    var dateGroups = Regex.Match(entry, @"urldate = ({.*})").Groups;
                    var replacementLine = $"{NewIdentifyer}{{\\url{{{lineGroups[2]}}} (visited at {dateGroups[1]})}}";
                    stringBuilder.AppendLine(Regex.Replace(entry, @"url = {.*}", replacementLine));
                    updatedEntries++;
                }
                else
                {
                    stringBuilder.AppendLine(entry);
                }
            }

            //My regex is shot, so I miss a closing curly brace...
            stringBuilder.Append("}");
            File.WriteAllText(bibliographyFilePath, stringBuilder.ToString(), Encoding.UTF8);
            Console.WriteLine($"Updated {updatedEntries} entries");
        }

        public static void PrintUsage()
        {
            Console.WriteLine("Usage: BibTexUrlFixer [Path to .bib file]");
        }
    }
}