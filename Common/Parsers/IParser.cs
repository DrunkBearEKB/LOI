using System;
using System.Collections.Generic;

namespace Common.Parsers
{
    public interface IParser
    {
        List<Tuple<string, List<string>>> Parse(string text);
    }
}