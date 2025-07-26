// cspell:disable

namespace Infrastructure.Utilities
{
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
            "fumez", "gaiarips", "galaxyrg", "galaxytv", "ganool", "geckos", "getb8", "glhf", "globe", "gonz0",
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
            "z3r0", "zerohd", "zeus", "zippymovies", "zombie"
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