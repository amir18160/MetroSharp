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
            string videoList = string.Join(", ", moviePaths.Select(s => s));
            string subtitleList = string.Join(", ", subtitlePaths.Select(s => s));

            return $$"""
            i will provide you with two list of strings. both lists include one or more paths of files on my disk.
            one list is paths of videos and the other is a list of path to subtitles.
            i want you to match videos to related subtitles and return an array of jsons.
            you need to consider these:
            1. each video can only match one subtitle by their name.
            2. some videos might match multiple subtitles, but you only pick one and generate only one json for it in that array.
            3. some videos might have no matching subtitles so in json you put null for subtitle value.
            4. sometimes you have to look at the patern of names to match subtitles with videos. for example we have this data: Videos = [path/seven.samurai.720p.x264.duubed.bluray.AC3.mkv] subtitles = [path/seven.samurai.srt, seven.samurai.720p.rarbg.srt]. both subtitle examples are both a match to our subtitle. so you pick closest match whch is the second example in this case: seven.samurai.720p.rarbg.srt.
            some other times pattern is not similar since both subtitles and videos are closely related you should pick with closes one.
            this is another example you must consider:
            Videos    = [path/the.boys.s01.E01.bluray.720p, path/the.boys.s01.E03.bluray.720p, path/the.boys.s01.E02.bluray.720p]  
            Subtitles = [path/the.boys.s01.E01.bluray.srt, path/the.boys.s01.E2.bluray.srt, path/s1.E3.bluray.720p]
            based on above exmaple each video is a match to exaclt one subtitle.
            and even we might have this example:
            Videos    = [the.boys.s01.E01.bluray.720p, the.boys.s01.E03.bluray.720p, the.boys.s01.E02.bluray.720p]  
            Subtitles = [s01 E01.srt, s01.E2.srt, s1 E3.720p]
            and in this we can find a match for each videos in subtitle list. what matter's here is that we already know subtitles are related to the videos.
            this is another example:
            Videos    = [the.boys.s02.E01.bluray.720p, the.boys.s02.E03.bluray.720p, the.boys.s02.E02.bluray.720p]  
            Subtitles = [s01 E01.srt, s01.E2.srt, s1 E3.720p]
            in this example we how no match for any of our videos becuase the season is diffrent in our subtitles which means we have completely wring subtitles.
            this is another example:
            Videos    = [the.boys.s02.E01.bluray.720p, the.boys.s02.E03.bluray.720p, the.boys.s02.E02.bluray.720p]  
            Subtitles = [s01 E01.srt, s01.E2.srt]
            in this example one of our videos has no matching subtitle. 
            5. the result must be in this format and you must absolutely not return any otherthing or charchter at all.
            you must purly return a response based in the provides data:
            data example: 
            Videos    = [path/the.boys.s01.E01.bluray.720p.mp4, path/the boys s01 E03 720p.mkv, path/the.boys.s01.E02.bluray.720p.avi]  
            Subtitles = [path/s01 E01.srt, path/the boys s01 E2.srt]
            response example:
            [
                {
                    "VideoPath": "path/the.boys.s01.E01.bluray.720p.mp4",
                    "SubtitlePath": "path/s01 E01.srt"
                },
                {
                    "VideoPath": "path/the.boys.s01.E02.bluray.720p.avi",
                    "SubtitlePath": "path/the boys s01 E2.srt"
                },
                {
                    "VideoPath": "path/the boys s01 E03 720p.mkv",
                    "SubtitlePath": null
                }
            ]

            Another Example:
            Videos    = [path/the.boys.s01.E01.bluray.720p.mp4, path/the boys s01 E03 720p.mkv, path/the.boys.s01.E02.bluray.720p.avi]  
            Subtitles = [] no subtitle is provided here
            response example:
            [
                {
                    "VideoPath": "path/the.boys.s01.E01.bluray.720p.mp4",
                    "SubtitlePath": null
                },
                {
                    "VideoPath": "path/the.boys.s01.E02.bluray.720p.avi",
                    "SubtitlePath": null
                },
                {
                    "VideoPath": "path/the boys s01 E03 720p.mkv",
                    "SubtitlePath": null
                }
            ]

            This is the actual list that you have to response to it:
            VideoPaths = [{{videoList}}]
            VideoPaths = [{{subtitlePaths}}]
            """;
        }

    }
}