using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Green.DocumentIntelligence
{
    public static class DocumentUtils
    {
        // Regular expressions to match the ID number pattern
        private const string PATTERN_WITH_SPACES = @"\b\d{6} \d{4} \d\b";
        private const string PATTERN_WITHOUT_SPACES = @"\b\d{11}\b";
        
        // Search for the pattern in the text
        internal static Match MatchIdWithSpaces(string text) => Regex.Match(text, PATTERN_WITH_SPACES);
        internal static Match MatchIdWithoutSpaces(string text) => Regex.Match(text, PATTERN_WITHOUT_SPACES);
    }
}
