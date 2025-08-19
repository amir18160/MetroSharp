using System.Globalization;
using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
namespace Persistence.Seeds.OldMetroDbSeeder
{
    public class MetroCsvSeeder
    {

        private readonly static int BatchSize = 1000;
        private static readonly string BasePath = @"C:\Users\HOORI\Desktop\Project-Metropolis\metro-sharp\DataCSV";
        private static readonly List<string> Files =
            ["FollowList.csv",
            "MTPProxy.csv",
            "ImdbSearchItem.csv",
            "Favorite.csv",
            "Advertisement.csv",
            "OmdbItem.csv",
            "Progress.csv",
            "Download.csv",
            "EztvxSeriesList.csv",
            "TitleView.csv",
            "Tag.csv",
            "RequestedShow.csv",
            "Top250IMDbMovies.csv",
            "WatchList.csv",
            "list.csv",
            "V2rayProxy.csv",
            "DownloadedDocument.csv",
            "User.csv",
            "ReportedProblems.csv",
            "TitleViewDetails.csv",
            "Document.csv"
            ];

        public static async Task SeedAsync(DataContext context, UserManager<User> userManager)
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Documents;");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM OmdbItems;");
            
            var omdbItemsPath = Path.Combine(BasePath, "OmdbItem.csv");
            var documentsPath = Path.Combine(BasePath, "Document.csv");

            var parsedOMDbResult = ParseCSV(omdbItemsPath);
            var parsedDocuments = ParseCSV(documentsPath);

            var OMDbItemList = parsedOMDbResult.Select(x =>
            {
                var item = new OmdbItem
                {
                    Id = Guid.NewGuid(),
                    ImdbId = ParseString(x[0]),
                    Title = ParseString(x[2]),
                    Rated = ParseString(x[3]),
                    Released = ParseString(x[4]),
                    Runtime = ParseString(x[5]),
                    Genres = ParsePostgresJson(x[6]),
                    Actors = ParsePostgresJson(x[7]),
                    Plot = ParseString(x[8]),
                    PlotFa = ParseString(x[9]),
                    Languages = ParsePostgresJson(x[10]),
                    Countries = ParsePostgresJson(x[11]),
                    Awards = ParseString(x[12]),
                    Poster = ParseString(x[13]),
                    Metascore = ParseInt(x[14]),
                    ImdbRating = ParseFloat(x[15]),
                    ImdbVotes = ParseInt(x[16]),
                    Type = ParseOMDbType(ParseString(x[19])),
                    BoxOffice = ParseString(x[20]),
                    TotalSeasons = null,
                    Directors = ParsePostgresJson(x[23]),
                    Writers = ParsePostgresJson(x[24]),
                    Year = ParseInt(x[25]),
                };
                return (item, ParseInt(x[1]));
            }).ToList();

            var documentItemList = parsedDocuments.Select(x =>
            {
                var omdbItem = OMDbItemList.FirstOrDefault(y => y.Item2 == ParseInt(x[15])).item;
                return new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = ParseString(x[1]),
                    Encoder = ParseString(ReleaseInfo.GetEncoder(x[2]) ?? x[2]),
                    Codec = ParseString(ReleaseInfo.GetCodec(x[3])) ?? x[3],
                    Resolution = ParseString(ReleaseInfo.GetResolution(x[4])) ?? x[4],
                    IsSubbed = ParseBoolean(x[5]),
                    MimeType = x[6],
                    FileId = x[7],
                    UniqueFileId = x[8],
                    FileSize = long.Parse(x[9]),
                    ChatName = x[10],
                    CreatedAt = ParseDateTime(x[11]),
                    MessageId = ParseInt(x[12]) ?? 0,
                    ChatId = long.Parse(x[13]),
                    Type = ParseDocumentType(x[14]),
                    UpdatedAt = DateTime.UtcNow,
                    OmdbItem = omdbItem,
                    OmdbItemId = omdbItem?.Id
                };
            }).ToList();

      

            await InsertBatchAsync(context, OMDbItemList.Select(x => x.item).ToList(), context.OmdbItems, "OMDbItem");
            await InsertBatchAsync(context, documentItemList, context.Documents, "Document");
        }

        public static async Task InsertBatchAsync<T>(DataContext context, List<T> items, DbSet<T> dbSet, string name) where T : class
        {
            int success = 0, errors = 0;

            for (int i = 0; i < items.Count; i += BatchSize)
            {
                var batch = items.Skip(i).Take(BatchSize).ToList();
                try
                {
                    await dbSet.AddRangeAsync(batch);
                    await context.SaveChangesAsync();
                    success += batch.Count;

                    Console.WriteLine($"Success Counts: {success}");
                }
                catch
                {
                    Console.WriteLine($"Batch insert failed for {name}, falling back to single inserts.");

                    foreach (var item in batch)
                    {
                        try
                        {
                            dbSet.Add(item);
                            await context.SaveChangesAsync();
                            success++;
                            if (success % 50 == 0)
                            {
                                Console.WriteLine($"Success Counts: {success}");
                            }
                        }
                        catch (Exception ex)
                        {
                            errors++;
                            Console.WriteLine($"Failed to insert {name} '{item}': {ex.Message}");
                            context.Entry(item).State = EntityState.Detached;
                        }
                    }
                }
            }

            Console.WriteLine($"{name} insertion: {success} succeeded, {errors} failed.");
        }


        private static List<string> ParsePostgresJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Contains("N/A") || input.Contains("null"))
            {
                return new List<string>();
            }

            string processedString = input.Trim();

            if (processedString.StartsWith('\"') && processedString.EndsWith('\"'))
            {
                processedString = processedString.Substring(1, processedString.Length - 2);
            }

            processedString = processedString.Replace("\"\"", "\"");

            try
            {
                if (processedString.StartsWith("["))
                {
                    return JsonSerializer.Deserialize<List<string>>(processedString) ?? new List<string>();
                }
                else
                {
                    return new List<string> { processedString };
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to parse the string: {processedString}. Error: {ex.Message}");
                return new List<string>();
            }
        }

        private static OmdbItemType ParseOMDbType(string value)
        {
            return OmdbItemType.Movie;
        }

        private static DocumentType ParseDocumentType(string value)
        {
            return DocumentType.Movie;
        }


        private static bool ParseBoolean(string value)
        {
            return value == "t";
        }

        private static string ParseString(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == null || value.Contains("N/A"))
            {
                return null;
            }

            return value;
        }

        private static DateTime ParseDateTime(string value)
        {
            string[] formats = { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.f" };

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt;

            Console.WriteLine("failed to parse date! returning default time.");
            return DateTime.UtcNow;

        }



        private static int? ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "null" || value.Contains("N/A"))
            {
                return null;
            }

            int x = 0;

            if (Int32.TryParse(value, out x))
            {
                return x;
            }
            return null;
        }

        private static float? ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "null" || value.Contains("N/A"))
            {
                return null;
            }

            value = value.Replace(",", "").Trim();

            if (float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float result))
            {
                return (float)Math.Round(result, 1, MidpointRounding.AwayFromZero);
            }

            return null;
        }

        private static List<List<string>> ParseCSV(string path)
        {
            var listOfItems = new List<List<string>>();
            using (var reader = new StreamReader(path, System.Text.Encoding.UTF8))
            using (TextFieldParser csvParser = new TextFieldParser(reader))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    string[] fields = csvParser.ReadFields();
                    var processedFields = fields
                        .Select(f => string.IsNullOrWhiteSpace(f) || f.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : f)
                        .ToList();
                    listOfItems.Add(processedFields);
                }
            }
            return listOfItems;
        }

        private static void SerializeAndDebug<T>(T item)
        {
            Console.WriteLine(JsonSerializer.Serialize(item));
        }
    }

    public static class ReleaseInfo
    {
        private readonly static List<string> CommonCodecs =
        [
            "x264", "x265", "h264", "h.264", "h265", "h.265", "h263", "h.263", "avc", "av1",
            "mpeg1", "mpeg-2", "mpeg2", "mpeg-4", "mpeg4", "xvid", "divx", "vp6", "vp8", "vp9", "vc-1",
            "prores", "dnxhd", "dnxhr", "realvideo", "vob", "webm", "hls", "hevc", "mpeg-1", "vc1"
        ];

        private readonly static List<string> Resolutions =
        [
            "4k", "2160", "uhd", "1440", "1080", "720", "480", "360", "2k", "sd", "dvd"
        ];

        private readonly static List<string> Encoders =
        [
            "1337x", "4fun", "7sins", "8ballrips", "acab", "adrianit0", "afo", "afr", "ajp69", "ak-47",
            "alien", "alliance", "amiable", "aniddl", "aqua", "ardua", "aerial", "armour", "asii", "avicable",
            "axxo", "azrael", "bamboozle", "bauckley", "bae", "bbq", "bbqxl", "better", "big", "bird",
            "birdhouse", "blackbeard", "blitzrg", "bob", "bone", "bone-rg", "boom", "btxide", "butter", "bz2",
            "cakes", "carvedhd", "chd", "cinefile", "cinehub", "cinemagia", "cinemania", "cinevood", "cmct", "cmrg",
            "collective", "coo7", "cpg", "cravers", "crimson", "cryptic", "csl", "ctrlhd", "ctu", "cyberman",
            "d-z0n3", "d3simal", "d3si", "darkmoon", "datub", "ddr", "deceit", "deflate", "desiscr", "diamond",
            "dibya", "dimension", "dirtyburger", "don", "done", "drc", "dreamteam", "dvsk", "dxhd", "eagles",
            "eazy", "egu", "eleanor", "elite-encode", "encodex", "epic", "etrg", "ettv", "evo", "evolve",
            "exclusive", "exren", "extramovies", "eztv", "failed", "fgt", "filmxy", "flux", "fov", "ftuapps",
            "fumez", "gaiarips", "galaxyrg", "galaxytv","galaxy", "ganool", "geckos", "getb8", "glhf", "globe", "gonz0",
            "grym", "hdc", "hdbits", "hdeg", "hdka", "hdlover", "hdscene", "hdrgroup", "heinz", "hellboy",
            "himself", "hindir", "hidt", "highqix", "hlite", "hnr", "hound", "hymin", "icedragon", "ift",
            "imovies", "immortal", "inkg", "ion", "ion10", "iyah", "jyk", "kat", "kiko", "kingdom",
            "kingz", "knives", "lazy", "legi0n", "leethd", "l1nk", "lokio", "lord", "lost", "lucky7",
            "mag", "martyr", "maxpro", "maxspeed", "megusta", "memento", "metis", "meech", "mhd", "minx",
            "mkv-anime", "mkvcage", "mkvcinemas", "modfury", "mooz", "moviesup", "msd", "mss", "mteam", "mystic",
            "mzabi", "mzon", "nexchapter", "nextheavens", "nexus", "nogrp", "nogroup", "no1knows", "noir", "ntb",
            "ntg", "obscurity", "oldteam", "once", "osman", "ozlem", "p2pdl", "pahe", "phobos", "phr0sty",
            "playhd", "playnow", "prism", "psa", "psarips", "pulsar", "pxhd", "qtb", "qxr", "ragemp3",
            "rapta", "rarbg", "rarbg-like", "rarbg.am", "rarbggo", "rarbgx", "rebelhd", "releasebb", "revo", "riddeck",
            "rmt", "ronbo", "ronin", "rtfm", "sainte", "scene-rls", "seed4me", "sfy", "sgf", "shaanig",
            "shanig", "shiningstar", "shitbox", "sic", "sinners", "sixhd", "skrg", "skipper", "smokey", "smurf",
            "sparks", "sprinter", "ssn", "sujaidr", "superhd", "tane", "telly", "telly.tv", "the.x.crew", "thedonk",
            "tigole", "tntvillage", "top4um", "torpedo", "tormhd", "trinity", "trollhd", "tsmk", "tsuki", "tvs",
            "tvsmash", "ultrarip", "unknown", "unreal", "utr", "vagdi", "vedetta", "virgins", "visum", "vxt",
            "vyndros", "wiki", "xanax", "xiso", "xrg", "xtreme", "yeshd", "yify", "yts", "yomvi",
            "z3r0", "zerohd", "zeus", "zippymovies", "zombie", "hevcbay"
        ];

        public static string GetCodec(string input)
        {
            return CommonCodecs
                .FirstOrDefault(codec =>
                    input.Contains(codec, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetResolution(string input)
        {
            return Resolutions
                .FirstOrDefault(res =>
                    input.Contains(res, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetEncoder(string input)
        {
            return Encoders
                .FirstOrDefault(enc =>
                    input.Contains(enc, StringComparison.OrdinalIgnoreCase));
        }
    }
}