using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Domain.Core;
using Telegram.Bot.Types;

namespace Infrastructure.BackgroundServices.TelegramBot.Utilities
{
    public static partial class IMDbUtilities
    {
        [GeneratedRegex(@"imdb\.com/(?:[a-z]{2}/)?title/(tt\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex IMDbRegex();
        private static readonly Regex ImdbRegex = IMDbRegex();

        public static bool IncludeIMDbURL(Message message)
        {
            if (message == null) return false;

            if (message.Entities != null &&
                message.Entities.Any(x => x.Url != null && GetImdbId(x.Url) != null))
            {
                return true;
            }

            var text = message.Text ?? string.Empty;
            return text.Contains(Constants.IMDbTitleBase, StringComparison.CurrentCultureIgnoreCase) ||
                   text.Contains(Constants.IMDbTitleAlternative, StringComparison.CurrentCultureIgnoreCase) ||
                   ImdbRegex.IsMatch(text);
        }

        public static string GetImdbId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var match = ImdbRegex.Match(input);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string GetImdbIdFromMessage(Message message)
        {
            if (message == null) return null;

            if (message.Entities != null)
            {
                foreach (var entity in message.Entities)
                {
                    if (!string.IsNullOrEmpty(entity.Url))
                    {
                        var id = GetImdbId(entity.Url);
                        if (id != null) return id;
                    }
                }
            }

            return GetImdbId(message.Text);
        }


    }
}
