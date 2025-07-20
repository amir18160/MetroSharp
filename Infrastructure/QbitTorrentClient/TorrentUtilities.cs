using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.QbitTorrentClient
{
    public partial class TorrentUtilities
    {

        public static string ExtractHashFromMagnet(string magnetLink)
        {
    
            if (string.IsNullOrWhiteSpace(magnetLink))
            {
                return null;
            }

            var regex = MyRegex();
            Match match = regex.Match(magnetLink);

            if (match.Success)
            {
                return match.Groups[1].Value.ToLower();
            }

            return null;
        }

        [GeneratedRegex(@"urn:btih:([a-fA-F0-9]{40})", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex();
    }

}