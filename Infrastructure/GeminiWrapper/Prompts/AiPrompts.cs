// cspell: disable

using System.Linq;

namespace Infrastructure.GeminiWrapper.Prompts
{
    public static class AiPrompts
    {
        public static string WriteReview(string title, string year)
        {
            return $$"""
            من از تو میخوهام که یک نقد در مورد یک فیلم بنویسی.
            وظیفه تو این است که:
            1. متن تولید شده بسیار کوتاه باشد. کمتر از 400 حرف.
            2. کاملا فارسی باشد و علایم نگارشی رعایت شده باشد.
            3. هیچ متن اضافی، توضیح یا معرفی در پاسخ نیار؛ فقط خروجی نهایی خواسته شده را تولید کن.
            4. چیزی که در اینجا اهمیت بسیار زیادی دارد این است که متن همواره معنای بسیار درست و واضعی داشته باشد.
            5. درصورتی که فیلم را نشناختی و اطلاعی از آن نداشتی یا دچار گمراهی شدی  فقط و فقط این را در پاسخ بده: "----------------"

            فیلم مورد نظر این است:
            {{title}}
            این عنوان در سال
            {{year}}
            منتشر شده است.
            """;
        }

        public static string TranslateNews(string text)
        {
            return $$"""
            یک متن به فرمت JSON در اختیار داری که مربوط به اخبار هالیوود و سینما است. این JSON شامل یک آرایه با نام "paragraphs" است که محتوای کامل خبر در آن قرار دارد.

            وظیفه تو این است که:
            1. ابتدا مقدار موجود در "title" را به فارسی ترجمه کن و در ابتدای متن بنویس.
            2.هرگز ابتدای متن را با کلمه ی انگلیسی شروع نکن. همواره باید کلمه ی اول فارسی باشد.
            3. سپس با استفاده از محتوای موجود در "paragraphs"، یک متن واحد، منسجم و کاملاً قابل فهم به زبان فارسی تولید کن.
            4. متن نهایی باید بسیار خلاصه باشد (کمتر از 1024 کاراکتر) و فقط مفهوم اصلی را منتقل کند. ای موضوع که کمتر از 1024 حرف باشد بسیار بسیار بسیار ضروری است.
            5. تمامی علائم نگارشی فارسی رعایت شود.
            6. از ایموجی فقط به میزان خیلی کم استفاده کن.
            7. تیتر خبر را بین تگ <b> قرار بده.
            8. در بخش‌هایی از متن، برای تأکید، از تگ <i> برای ایتالیک استفاده کن.
            9. پس از پایان متن، یک خط خالی ایجاد کن (نیولاین) و سپس چند هشتگ مرتبط با موضوع خبر اضافه کن.
            10. هیچ متن اضافی، توضیح یا معرفی در پاسخ نیار؛ فقط خروجی نهایی خواسته شده را تولید کن.
            11. ساختار متن در صورت لزون دارای نیولاین و پاراگراف بندی باشد و تمام متن چسبیده نباشد.
            12. چیزی که در اینجا اهمیت بسیار زیادی دارد این است که متن همواره معنای بسیار درست و واضعی داشته باشد.
            13. اگر برای کلمه ای معنای فارسی درستی و واضحی وجود ندارد یا معادل فارسی آن رایج نیست حتما حتما همان کلمه ی انگلیسی رو به فارسی بنویس، به عنوان مثلا: easter egg: ایستراگ. آن استفاده کن. در سربرگ خبر اصلا از کلمات انگلیسی استفده نشود و تنها کلمات فارسی و فینگلیش مجاز است.

            این هم JSON مورد نظر:
            {{text}}
            """;
        }

        public static string TranslateRawText(string text)
        {
            return $$"""
            من یک متن انگلیسی برایت می‌فرستم. وظیفه تو این است که آن را به فارسی ترجمه کنی، با رعایت نکات زیر:

            1. ترجمه باید **کاملاً دارای معنی** باشد، نه صرفاً واژه به واژه.
            2. اگر کلمه‌ای در متن وجود داشت که **معادل فارسی رایج یا دقیقی ندارد**، همان کلمه را به **صورت فینگلیش** بنویس.
            - مثال: به جای "randerman"، بنویس: رندرمن.
            3. **تمام نکات نگارشی فارسی** باید به‌درستی رعایت شود.
            4. از **هیچ‌گونه Markup یا Markdown** مثل `<i>`، `#`، `*` و... استفاده نکن.
            5. پاسخ تو باید **کاملاً متن خالص** باشد.
            6. استفاده از **ایموجی** مجاز است ولی **فقط در حد بسیار کم**.
            7. فقط و فقط از تگ `<b>` برای بولد کردن استفاده کن (اگر نیاز بود).
            8. در پاسخ خودت **هیچ اشاره‌ای به این دستورالعمل‌ها نکن**.
            - از این لحظه به بعد، متن اصلی شروع خواهد شد.
            {{text}}
            """;
        }

        public static string GeneratePairJson(List<string> moviePaths, List<string> subtitlePaths)
        {
            string videoList = string.Join(", ", moviePaths.Select(s => $"\"{s}\""));
            string subtitleList = string.Join(", ", subtitlePaths.Select(s => $"\"{s}\""));

            return $$"""
                You are a smart assistant designed to match subtitle files to video files.

                I will provide you with two lists:
                - A list of **video file paths**
                - A list of **subtitle file paths**

                You must return a **JSON array** where:
                - Each object represents a video and its most appropriate subtitle
                - If no subtitle matches, set "SubtitlePath": null

                ## RULES YOU MUST FOLLOW STRICTLY:

                1. You must **never** include subtitle paths in the video list or vice versa.
                2. Match each video to **only one** subtitle (the best match based on filename similarity).
                3. Use filename similarity rules (season, episode, resolution, etc.) to match subtitles to videos.
                4. If a video contains season/episode info → it's a TV episode:
                - Set "IsMovie": false
                - Set SeasonNumber and EpisodeNumber as integers
                5. If a video contains **no season/episode info** → it's a movie:
                - Set "IsMovie": true
                - Set SeasonNumber and EpisodeNumber to null
                6. Your result MUST be a **pure JSON array only**. Do not explain anything. Do not add extra characters or text.
                7. Use **exact property names**:
                - VideoPath
                - SubtitlePath
                - IsMovie
                - EpisodeNumber
                - SeasonNumber

                ## EXAMPLES YOU MUST FOLLOW:

                ### Example 1 (TV series, good match):
                VideoPaths = [
                "/home/user/Downloads/The.Boys.S01E01.720p.WEB-DL.mkv",
                "/home/user/Downloads/The.Boys.S01E02.720p.WEB-DL.mkv",
                "/home/user/Downloads/The.Boys.S01E03.720p.WEB-DL.mkv"
                ]
                SubtitlePaths = [
                "/home/user/Downloads/The.Boys.S01E01.srt",
                "/home/user/Downloads/The.Boys.S01E02.srt"
                ]
                Expected Output:
                [
                {
                    "VideoPath": "/home/user/Downloads/The.Boys.S01E01.720p.WEB-DL.mkv",
                    "SubtitlePath": "/home/user/Downloads/The.Boys.S01E01.srt",
                    "IsMovie": false,
                    "EpisodeNumber": 1,
                    "SeasonNumber": 1
                },
                {
                    "VideoPath": "/home/user/Downloads/The.Boys.S01E02.720p.WEB-DL.mkv",
                    "SubtitlePath": "/home/user/Downloads/The.Boys.S01E02.srt",
                    "IsMovie": false,
                    "EpisodeNumber": 2,
                    "SeasonNumber": 1
                },
                {
                    "VideoPath": "/home/user/Downloads/The.Boys.S01E03.720p.WEB-DL.mkv",
                    "SubtitlePath": null,
                    "IsMovie": false,
                    "EpisodeNumber": 3,
                    "SeasonNumber": 1
                }
                ]

                ### Example 2 (Movie, no subtitle):
                VideoPaths = [
                "/home/user/Downloads/Big.Buck.Bunny.1080p.Bluray.mkv"
                ]
                SubtitlePaths = []
                Expected Output:
                [
                {
                    "VideoPath": "/home/user/Downloads/Big.Buck.Bunny.1080p.Bluray.mkv",
                    "SubtitlePath": null,
                    "IsMovie": true,
                    "EpisodeNumber": null,
                    "SeasonNumber": null
                }
                ]

                ### Example 3 (Wrong season):
                VideoPaths = [
                "/home/user/Downloads/The.Boys.S02E01.720p.WEB-DL.mkv"
                ]
                SubtitlePaths = [
                "/home/user/Downloads/The.Boys.S01E01.srt"
                ]
                Expected Output:
                [
                {
                    "VideoPath": "/home/user/Downloads/The.Boys.S02E01.720p.WEB-DL.mkv",
                    "SubtitlePath": null,
                    "IsMovie": false,
                    "EpisodeNumber": 1,
                    "SeasonNumber": 2
                }
                ]

                ### Example 4 (Messy filenames, still match):
                VideoPaths = [
                "/home/user/Downloads/The.Boys.S01E01.720p.WEB-DL.mkv",
                "/home/user/Downloads/The.Boys.S01E02.720p.WEB-DL.mkv"
                ]
                SubtitlePaths = [
                "/home/user/Downloads/s01 e01.srt",
                "/home/user/Downloads/s1e2.final.subs.srt"
                ]
                Expected Output:
                [
                {
                    "VideoPath": "/home/user/Downloads/The.Boys.S01E01.720p.WEB-DL.mkv",
                    "SubtitlePath": "/home/user/Downloads/s01 e01.srt",
                    "IsMovie": false,
                    "EpisodeNumber": 1,
                    "SeasonNumber": 1
                },
                {
                    "VideoPath": "/home/user/Downloads/The.Boys.S01E02.720p.WEB-DL.mkv",
                    "SubtitlePath": "/home/user/Downloads/s1e2.final.subs.srt",
                    "IsMovie": false,
                    "EpisodeNumber": 2,
                    "SeasonNumber": 1
                }
                ]

                NOW YOUR TASK:

                Based on the following lists, return the correct JSON array according to the rules and format above.

                VideoPaths = [{{videoList}}]  
                SubtitlePaths = [{{subtitleList}}]
                """;
        }
    }
}