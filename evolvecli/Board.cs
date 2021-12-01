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
        private readonly Dictionary<int, SKPaint> _colorCache;
        private readonly SKPaint _zoneColor;
        
        private const int CellMultiple = 9;
        private const int NodeRadius = 5;

        public Board(World world)
        {
            _world = world;

            var info = new SKImageInfo(_world.Dimension * CellMultiple, _world.Dimension * CellMultiple);
            _surface = SKSurface.Create(info);
            _canvas = _surface.Canvas;
            _colorCache = new Dictionary<int, SKPaint>();
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
            
            _canvas.DrawCircle(point, NodeRadius, _zoneColor);
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