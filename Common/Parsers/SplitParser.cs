using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Parsers
{
    public class SplitParser : IParser
    {
        public List<Tuple<string, List<string>>> Parse(string text)
        {
            List<Tuple<string, List<string>>> result = new List<Tuple<string, List<string>>>();
            
            foreach (string l in text.Split("\r\n"))
            {
                var line = l.TrimStart();
                if (line.StartsWith("#") || line.Length == 0)
                {
                    continue;
                }
                if (!line.Contains("->"))
                {
                    return null;
                }
                
                var lineParsed = line.Replace(" ", "").Split("->");
                var left = lineParsed[0];
                var rightParsed = lineParsed[1].Split("|");
                result.Add(new Tuple<string, List<string>>(left, rightParsed.ToList()));
            }

            return result;
        }
    }
}