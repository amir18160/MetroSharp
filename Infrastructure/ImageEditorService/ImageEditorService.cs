using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.ImageEditorService;

public class ImageEditorService(HttpClient httpClient, IOptions<ImageEditorSettings> imageEditorSettings)
{
    private readonly ImageEditorSettings _imageEditorSettings = imageEditorSettings.Value; // logo is here as LogoPath

    public async Task<MemoryStream> GenerateMediaWatermark(string imageUrl)
    {
        // 1) Download main image
        await using var imgStream = await httpClient.GetStreamAsync(imageUrl);
        using var image = await Image.LoadAsync(imgStream);

        // 2) Create canvas with left margin
        const int marginLeft = 100;
        using var canvas = new Image<Rgba32>(image.Width + marginLeft, image.Height);

        canvas.Mutate(x => x.Fill(Color.White));
        canvas.Mutate(x => x.DrawImage(image, new Point(marginLeft, 0), 1f));

        // 3) Load logo (from local file in settings)
        var logoPath = Path.Combine(AppContext.BaseDirectory, _imageEditorSettings.LogoPath);
        using var logo = await Image.LoadAsync(logoPath);

        // Optionally resize logo (e.g. fit margin width, keep aspect ratio)
        const int maxLogoWidth = marginLeft - 10; // leave some padding
        if (logo.Width > maxLogoWidth)
        {
            logo.Mutate(x => x.Resize(maxLogoWidth, 0)); // 0 keeps aspect ratio
        }

        // 4) Draw logo at top of left margin (x=5, y=5 padding)
        canvas.Mutate(x => x.DrawImage(logo, new Point(5, 5), 1f));

        // 5) Save to memory stream
        var ms = new MemoryStream();
        await canvas.SaveAsJpegAsync(ms);
        ms.Position = 0;

        return ms;
    }
}