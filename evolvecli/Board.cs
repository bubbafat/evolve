using System;
using System.Collections.Generic;
using System.IO;
using evolve;
using SkiaSharp;

namespace evolvecli
{
    public class Board : IDisposable
    {
        private readonly SKSurface _surface;
        private readonly SKCanvas _canvas;
        private readonly World _world;
        private readonly Dictionary<int, SKPaint> _paintCache;
        private readonly SKPaint _zoneColor;
        
        private const int CellMultiple = 9;
        private const int NodeRadius = 5;

        public Board(World world)
        {
            _world = world;

            var info = new SKImageInfo(_world.Dimension * CellMultiple, _world.Dimension * CellMultiple);
            _surface = SKSurface.Create(info);
            _canvas = _surface.Canvas;
            _paintCache = new Dictionary<int, SKPaint>();
            _zoneColor = new SKPaint
            {
                Color = SKColors.Red
            };
        }
        
        private void renderNode(Node n)
        {
            SKPoint point = new SKPoint(
                n.Location.X * CellMultiple + NodeRadius,
                n.Location.Y * CellMultiple + NodeRadius);

            int paintKey = n.GetHashCode();
            if (!_paintCache.TryGetValue(paintKey, out SKPaint paint))
            {
                var bytes = BitConverter.GetBytes(n.GetHashCode());
                SKColor color = new SKColor(bytes[0], bytes[1], bytes[2]);

                paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill
                };

                _paintCache.Add(paintKey, paint);
            }
            
            _canvas.DrawCircle(point, NodeRadius, paint);
        }

        private void renderFrame()
        {
            _canvas.Clear(SKColors.White);
            foreach (Node n in _world.Nodes)
            {
                renderNode(n);
            }
        }

        public void ExportFrame(FileInfo path)
        {
            renderFrame();
            
            path.Directory.Create();
            
            using (var stream = path.OpenWrite())
            using (var snapshot = _surface.Snapshot())
            using (var data = snapshot.Encode(SKEncodedImageFormat.Png, 90))
            {
                data.SaveTo(stream);
            }
        }

        public void Dispose()
        {
            _surface?.Dispose();
        }
    }
}