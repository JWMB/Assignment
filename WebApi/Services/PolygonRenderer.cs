using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WebApi.Services
{
    public static class PolygonRenderer
    {
        public static Image Render(IEnumerable<IEnumerable<GeoLibrary.Model.Point>> data, (float x, float y) sizeFactors)
        {
            var polygons = data
                .Select(p => new SixLabors.ImageSharp.Drawing.Polygon(p.Select(o => new PointF((float)o.Longitude, (float)-o.Latitude)).ToArray()))
                .ToList();

            var totalBounds = polygons.Select(o => o.Bounds).Aggregate(RectangleF.Union);

            var sizeFactor = new PointF(sizeFactors.x, sizeFactors.y);

            var scaledBounds = totalBounds.Multiply(sizeFactor);
            var offset = new PointF(-scaledBounds.Left, -scaledBounds.Top);

            var img = new Image<Rgba32>((int)Math.Ceiling(scaledBounds.Width), (int)Math.Ceiling(scaledBounds.Height));
            foreach (var item in polygons.Select((o, i) => new { Index = i, Polygon = o }))
            {
                var offsetPoints = item.Polygon.Points.ToArray().Select(o => o.Multiply(sizeFactor).Add(offset)).ToArray();
                var rgb = ColorSpaceConverter.ToRgb(new Hsl((float)(item.Index * 30 % 360), 0.7f, 0.2f));
                var color = new Color((Rgba32)rgb);
                img.Mutate(o => o
                    .DrawPolygon(Pens.Solid(Color.Red, 1), offsetPoints)
                    .FillPolygon(Brushes.Solid(color), offsetPoints)
                    );
            }

            return img;
        }
    }

    public static class RectangleFExtensions
    {
        public static RectangleF Multiply(this RectangleF r, PointF p) => new RectangleF(r.Left * p.X, r.Top * p.Y, r.Width * p.X, r.Height * p.Y);
    }

    public static class PointFExtensions
    {
        public static PointF Add(this PointF p1, PointF p2) => new PointF(p1.X + p2.X, p1.Y + p2.Y);
        public static PointF Multiply(this PointF p1, PointF p2) => new PointF(p1.X * p2.X, p1.Y * p2.Y);
    }
}
