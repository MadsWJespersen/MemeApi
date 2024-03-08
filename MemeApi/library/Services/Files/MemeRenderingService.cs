using MemeApi.library.Extensions;
using MemeApi.Models.Entity;
using Microsoft.Extensions.Configuration;
using SkiaSharp;
using System.IO;
using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

public class MemeRenderingService : IMemeRenderingService
{
    private readonly MemeApiSettings _settings;
    private readonly IFileLoader _loader;

    public MemeRenderingService(MemeApiSettings settings, IFileLoader loader)
    {
        _settings = settings;
        _loader = loader;
    }

    public async Task<byte[]> RenderMeme(Meme meme)
    {
        var textSize = 40;
        SKImageInfo info = new SKImageInfo(400, 400, SKColorType.Rgba8888, SKAlphaType.Premul);
        
        var path = Path.Combine("visual/", meme.MemeVisual.Filename);
        var data = await _loader.LoadFile(path);
        
        var inputImage = SKBitmap.Decode(data);
        var resized = inputImage.Resize(info, SKFilterQuality.High);
        var canvas = new SKCanvas(resized);

        canvas.DrawBitmap(resized, new SKPoint(0, 0));

        float topTextY = resized.Height / 6;
        DrawText(canvas, meme.ToToptext(), resized.Width, topTextY, SKTypeface.FromFamilyName("Impact"), textSize);

        float bottomTextY = resized.Height - resized.Height / 8;
        DrawText(canvas, meme.ToBottomtext(), resized.Width, bottomTextY, SKTypeface.FromFamilyName("Impact"), textSize);

        using (var stream = new MemoryStream())
        {
            using (var imageStream = new SKManagedWStream(stream))
            {
                resized.Encode(imageStream, SKEncodedImageFormat.Png, quality: 100);
            }

            return stream.ToArray();
        }
    }
    private SKCanvas DrawText(SKCanvas canvas, string text, int canvasWidth, float centerY, SKTypeface font, int textSize)
    {
        using var textPaint = new SKPaint();
        textPaint.Color = SKColors.White;
        textPaint.TextSize = textSize;
        textPaint.Typeface = font;
        textPaint.IsAntialias = true;

        // Outline 
        using var outlinePaint = new SKPaint();
        outlinePaint.Color = SKColors.Black;
        outlinePaint.TextSize = textSize;
        outlinePaint.Typeface = font;
        outlinePaint.IsAntialias = true;
        outlinePaint.StrokeWidth = 4; // Width of the outline
        outlinePaint.Style = SKPaintStyle.Stroke;

        float centerX = (canvasWidth - textPaint.MeasureText(text)) / 2;

        canvas.DrawText(text, centerX, centerY, outlinePaint);
        canvas.DrawText(text, centerX, centerY, textPaint);

        return canvas;
    }
}
