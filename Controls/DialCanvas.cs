using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.Platform;

namespace Ebertin.Controls
{
    public class DialCanvas : Control
    {
        private double radius;
        private double arrowAngleLeft = 180;
        private double arrowAngleRight = 180;
        private bool isDraggingLeft = false;
        private bool isDraggingRight = false;
        private Point leftCenterPoint;
        private Point rightCenterPoint;
        
        // Cached render bitmaps
        private RenderTargetBitmap leftDialStaticCache;
        private RenderTargetBitmap rightDialStaticCache;
        private bool cacheInvalidated = true;
        
        // Planetary Font
        private readonly FontFamily _planetFontFamily;
        
        // Planet data structure - updated with font symbols
        private readonly List<PlanetInfo> planets = new()
        {
            new PlanetInfo { Id = "Su", Symbol = "a", Value = 10 },
            new PlanetInfo { Id = "Mo", Symbol = "b", Value = 20 },
            new PlanetInfo { Id = "Me", Symbol = "c", Value = 30 },
            new PlanetInfo { Id = "Ve", Symbol = "d", Value = 40 },
            new PlanetInfo { Id = "Ma", Symbol = "e", Value = 50 },
            new PlanetInfo { Id = "Ju", Symbol = "f", Value = 0 },
            new PlanetInfo { Id = "Sa", Symbol = "g", Value = 0 },
            new PlanetInfo { Id = "Ur", Symbol = "h", Value = 0 },
            new PlanetInfo { Id = "Ne", Symbol = "i", Value = 0 },
            new PlanetInfo { Id = "Pl", Symbol = "j", Value = 0 },
            new PlanetInfo { Id = "Ce", Symbol = "k", Value = 0 },
            new PlanetInfo { Id = "Jn", Symbol = "l", Value = 0 },
            new PlanetInfo { Id = "Pa", Symbol = "m", Value = 0 },
            new PlanetInfo { Id = "Vs", Symbol = "n", Value = 0 },
            new PlanetInfo { Id = "Ch", Symbol = "o", Value = 0 }
        };

        public DialCanvas()
        {
            ClipToBounds = true;
            RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);
            
            try
            {
                // Try multiple approaches to load the font
                string[] fontAttempts = {
                    "avares://Ebertin/Assets/Fonts/PlanetaryGlyph.otf#PlanetaryGlyph",
                    "avares://Ebertin/Assets/Fonts/PlanetaryGlyph.otf#Planetary Glyph",
                    "avares://Ebertin/Assets/Fonts/PlanetaryGlyph.otf#Planetary",
                    "resm:Ebertin.Assets.Fonts.PlanetaryGlyph.otf?assembly=Ebertin#PlanetaryGlyph",
                    "avares://Ebertin/Assets/Fonts/PlanetaryGlyph.otf"
                };

                bool fontLoaded = false;
                foreach (var attempt in fontAttempts)
                {
                    try
                    {
                        _planetFontFamily = new FontFamily(attempt);
                        System.Diagnostics.Debug.WriteLine($"Successfully loaded font with: {attempt}");
                        fontLoaded = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load font with '{attempt}': {ex.Message}");
                    }
                }

                if (!fontLoaded)
                {
                    System.Diagnostics.Debug.WriteLine("All font loading attempts failed, using default font");
                    _planetFontFamily = FontFamily.Default;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error in font loading: {ex.Message}");
                _planetFontFamily = FontFamily.Default;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var position = e.GetPosition(this);
                
                // Determine which dial is being clicked
                double distanceToLeft = Math.Abs(position.X - leftCenterPoint.X);
                double distanceToRight = Math.Abs(position.X - rightCenterPoint.X);
                
                isDraggingLeft = distanceToLeft < distanceToRight;
                isDraggingRight = !isDraggingLeft;
                
                e.Handled = true;
                e.Pointer.Capture(this); // Capture the pointer for smooth dragging
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            
            if (isDraggingLeft || isDraggingRight)
            {
                var position = e.GetPosition(this);
                UpdateArrowAngle(position, isDraggingLeft);
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            
            isDraggingLeft = false;
            isDraggingRight = false;
            e.Handled = true;
            e.Pointer.Capture(null); // Release the pointer capture
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Invalidate cache when size changes
            cacheInvalidated = true;
            return base.MeasureOverride(availableSize);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            
            // Calculate dimensions
            double width = Bounds.Width;
            double height = Bounds.Height;
            
            if (width <= 0 || height <= 0) return;
            
            // Calculate positioning for two dials side by side with maximized sizes
            radius = Math.Min(width / 2.2, height / 2.2); // Allow for some margin
            double centerY = height / 2;
            double leftCenterX = width / 4;
            double rightCenterX = (width * 3) / 4;
            
            leftCenterPoint = new Point(leftCenterX, centerY);
            rightCenterPoint = new Point(rightCenterX, centerY);
            
            // Update cache if needed
            if (cacheInvalidated || leftDialStaticCache == null || rightDialStaticCache == null)
            {
                CacheStaticElements();
                cacheInvalidated = false;
            }
            
            // Draw cached static elements
            if (leftDialStaticCache != null)
            {
                context.DrawImage(leftDialStaticCache, 
                    new Rect(0, 0, leftDialStaticCache.PixelSize.Width, leftDialStaticCache.PixelSize.Height),
                    new Rect(0, 0, width / 2, height));
            }
            
            if (rightDialStaticCache != null)
            {
                context.DrawImage(rightDialStaticCache, 
                    new Rect(0, 0, rightDialStaticCache.PixelSize.Width, rightDialStaticCache.PixelSize.Height),
                    new Rect(width / 2, 0, width / 2, height));
            }
            
            // Draw only dynamic elements (arrows)
            DrawArrow(context, leftCenterPoint, radius * 0.885, arrowAngleLeft);
            DrawArrow(context, rightCenterPoint, radius * 0.885, arrowAngleRight);
        }

        private void CacheStaticElements()
        {
            double width = Bounds.Width;
            double height = Bounds.Height;
            
            if (width <= 0 || height <= 0) return;
            
            var pixelSize = new PixelSize((int)(width / 2), (int)height);
            var dpi = new Vector(96, 96);
            
            // Cache left dial
            leftDialStaticCache = new RenderTargetBitmap(pixelSize, dpi);
            using (var cacheContext = leftDialStaticCache.CreateDrawingContext())
            {
                Point center = new Point(pixelSize.Width / 2, pixelSize.Height / 2);
                DrawStaticElements90(cacheContext, center, radius);
            }
            
            // Cache right dial
            rightDialStaticCache = new RenderTargetBitmap(pixelSize, dpi);
            using (var cacheContext = rightDialStaticCache.CreateDrawingContext())
            {
                Point center = new Point(pixelSize.Width / 2, pixelSize.Height / 2);
                DrawStaticElements120(cacheContext, center, radius);
            }
        }

        private void DrawStaticElements90(DrawingContext context, Point center, double radius)
        {
            double outerRadius = radius;
            double innerRadius = radius * 0.885;

            // Draw outer circle
            var pen = new Pen(Brushes.White, 2);
            context.DrawEllipse(null, pen, center, outerRadius, outerRadius);

            // Draw inner circle
            context.DrawEllipse(null, pen, center, innerRadius, innerRadius);

            // Draw tick marks
            double markLength = (outerRadius - innerRadius) * 0.2;
            double longMarkLength = markLength * 1.4;

            for (int degrees = 0; degrees < 360; degrees += 4)
            {
                double radians = DegreesToRadians(90 + degrees);
                bool isLongMark = degrees == 0 || degrees % 20 == 0;
                double currentMarkLength = isLongMark ? longMarkLength : markLength;

                var startPoint = new Point(
                    center.X + innerRadius * Math.Cos(radians),
                    center.Y + innerRadius * Math.Sin(radians)
                );

                var endPoint = new Point(
                    center.X + (innerRadius + currentMarkLength) * Math.Cos(radians),
                    center.Y + (innerRadius + currentMarkLength) * Math.Sin(radians)
                );

                var markPen = new Pen(Brushes.White, isLongMark ? 3 : 1.5);
                context.DrawLine(markPen, startPoint, endPoint);

                // Draw degree labels for long marks
                if (isLongMark)
                {
                    double labelValue = degrees <= 180 ? 45 - (degrees / 4) : 45 + ((360 - degrees) / 4);
                    double textDistance = longMarkLength + 14;
                    
                    var textPoint = new Point(
                        center.X + (innerRadius + textDistance) * Math.Cos(radians),
                        center.Y + (innerRadius + textDistance) * Math.Sin(radians)
                    );

                    var formattedText = new FormattedText(
                        $"{Math.Round(labelValue)}°",
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        14,
                        Brushes.White
                    );

                    // Center the text
                    textPoint = new Point(
                        textPoint.X - formattedText.Width / 2,
                        textPoint.Y - formattedText.Height / 2
                    );

                    context.DrawText(formattedText, textPoint);
                }
            }

            // Draw center dot
            context.DrawEllipse(Brushes.White, null, center, 5, 5);

            // Plot planets
            PlotPlanetsInside(context, center, radius * 0.73, innerRadius);
        }

        private void DrawStaticElements120(DrawingContext context, Point center, double radius)
        {
            double outerRadius = radius;
            double innerRadius = radius * 0.885;

            // Draw outer circle
            var pen = new Pen(Brushes.White, 2);
            context.DrawEllipse(null, pen, center, outerRadius, outerRadius);

            // Draw inner circle
            context.DrawEllipse(null, pen, center, innerRadius, innerRadius);

            // Draw tick marks
            double markLength = (outerRadius - innerRadius) * 0.2;
            double longMarkLength = markLength * 1.4;

            for (int degrees = 0; degrees < 360; degrees += 3)
            {
                double radians = DegreesToRadians(90 + degrees);
                bool isLongMark = degrees == 0 || degrees % 15 == 0;
                double currentMarkLength = isLongMark ? longMarkLength : markLength;

                var startPoint = new Point(
                    center.X + innerRadius * Math.Cos(radians),
                    center.Y + innerRadius * Math.Sin(radians)
                );

                var endPoint = new Point(
                    center.X + (innerRadius + currentMarkLength) * Math.Cos(radians),
                    center.Y + (innerRadius + currentMarkLength) * Math.Sin(radians)
                );

                var markPen = new Pen(Brushes.White, isLongMark ? 3 : 1.5);
                context.DrawLine(markPen, startPoint, endPoint);

                // Draw degree labels for long marks
                if (isLongMark)
                {
                    double labelValue = degrees <= 180 ? 60 - (degrees / 3) : 60 + ((360 - degrees) / 3);
                    double textDistance = longMarkLength + 14;
                    
                    var textPoint = new Point(
                        center.X + (innerRadius + textDistance) * Math.Cos(radians),
                        center.Y + (innerRadius + textDistance) * Math.Sin(radians)
                    );

                    var formattedText = new FormattedText(
                        $"{Math.Round(labelValue)}°",
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        14,
                        Brushes.White
                    );

                    // Center the text
                    textPoint = new Point(
                        textPoint.X - formattedText.Width / 2,
                        textPoint.Y - formattedText.Height / 2
                    );

                    context.DrawText(formattedText, textPoint);
                }
            }

            // Draw center dot
            context.DrawEllipse(Brushes.White, null, center, 5, 5);
        }

        private void PlotPlanetsInside(DrawingContext context, Point center, double planetsCircleRadius, double innerRadius)
        {
            var adjustedPositions = new Dictionary<string, PlanetPosition>();
            
            // Initial positioning
            foreach (var planet in planets.Where(p => p.Value > 0))
            {
                adjustedPositions[planet.Id] = new PlanetPosition
                {
                    OriginalValue = planet.Value,
                    AdjustedValue = planet.Value,
                    Symbol = planet.Symbol
                };
            }

            // Simple collision detection
            var orderedPlanets = adjustedPositions.Keys.OrderBy(k => adjustedPositions[k].OriginalValue).ToList();
            
            for (int i = 1; i < orderedPlanets.Count; i++)
            {
                var currentId = orderedPlanets[i];
                var previousId = orderedPlanets[i - 1];
                var current = adjustedPositions[currentId];
                var previous = adjustedPositions[previousId];

                double degreeDiff = Math.Abs(current.AdjustedValue - previous.AdjustedValue);
                if (degreeDiff < 1.5)
                {
                    current.AdjustedValue = previous.AdjustedValue + 1.5;
                    adjustedPositions[currentId] = current;
                }
            }

            // Draw all planets
            foreach (var planet in adjustedPositions)
            {
                PlotPlanetInside(context, center, planet.Key, planet.Value, planetsCircleRadius, innerRadius);
            }
        }

        private void PlotPlanetInside(DrawingContext context, Point center, string id, PlanetPosition position, 
            double planetsCircleRadius, double innerRadius)
        {
            // Calculate positions
            double mappedAngle = -position.AdjustedValue * 4;
            double angleRadians = DegreesToRadians(mappedAngle - 90);

            double originalMappedAngle = -position.OriginalValue * 4;
            double originalAngleRadians = DegreesToRadians(originalMappedAngle - 90);

            // Position for symbol - between the two circles
            double symbolRadius = (planetsCircleRadius + innerRadius) / 2;
            var symbolPoint = new Point(
                center.X + symbolRadius * Math.Cos(angleRadians),
                center.Y + symbolRadius * Math.Sin(angleRadians)
            );

            // Position for pointer on inner circle
            var pointerPoint = new Point(
                center.X + innerRadius * Math.Cos(originalAngleRadians),
                center.Y + innerRadius * Math.Sin(originalAngleRadians)
            );

            // Draw shorter connecting line that doesn't touch the glyph
            // Calculate a point slightly offset from the symbol point towards the pointer
            double offsetDistance = 20; // Adjust this value to control how far from the glyph the line starts
            double lineOffsetAngle = Math.Atan2(pointerPoint.Y - symbolPoint.Y, pointerPoint.X - symbolPoint.X);
            
            var lineStartPoint = new Point(
                symbolPoint.X + offsetDistance * Math.Cos(lineOffsetAngle),
                symbolPoint.Y + offsetDistance * Math.Sin(lineOffsetAngle)
            );
            
            context.DrawLine(new Pen(Brushes.White, 1), lineStartPoint, pointerPoint);

            // Draw dot on inner circle
            context.DrawEllipse(Brushes.White, null, pointerPoint, 3, 3);

            // Draw planet symbol using the custom font
            try
            {
                var typeface = new Typeface(_planetFontFamily);
                System.Diagnostics.Debug.WriteLine($"Drawing symbol '{position.Symbol}' with font: {_planetFontFamily.Name}");
                
                var symbolText = new FormattedText(
                    position.Symbol,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    48,  // Increased from 16 to 48
                    Brushes.White
                );

                // Debug information
                System.Diagnostics.Debug.WriteLine($"Symbol width: {symbolText.Width}, height: {symbolText.Height}");

                context.DrawText(symbolText, 
                    new Point(symbolPoint.X - symbolText.Width / 2, 
                             symbolPoint.Y - symbolText.Height / 2));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error drawing planet symbol: {ex.Message}");
                // Fallback to default font
                var fallbackText = new FormattedText(
                    position.Symbol,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    48,  // Increased from 16 to 48
                    Brushes.White
                );
                context.DrawText(fallbackText, 
                    new Point(symbolPoint.X - fallbackText.Width / 2, 
                             symbolPoint.Y - fallbackText.Height / 2));
            }
        }

        private void DrawArrow(DrawingContext context, Point center, double radius, double angleDegrees)
        {
            double arrowRadians = DegreesToRadians(90 + angleDegrees);

            var startPoint = new Point(
                center.X - radius * Math.Cos(arrowRadians),
                center.Y - radius * Math.Sin(arrowRadians)
            );

            var endPoint = new Point(
                center.X + radius * Math.Cos(arrowRadians),
                center.Y + radius * Math.Sin(arrowRadians)
            );

            // Draw arrow line with anti-aliasing
            var pen = new Pen(Brushes.White, 3);
            pen.LineJoin = PenLineJoin.Round;
            pen.LineCap = PenLineCap.Round;
            context.DrawLine(pen, startPoint, endPoint);

            // Draw arrowhead
            double headLength = 20;
            double headAngle = Math.PI / 6;

            var arrowPoints = new List<Point>
            {
                endPoint,
                new Point(
                    endPoint.X - headLength * Math.Cos(arrowRadians - headAngle),
                    endPoint.Y - headLength * Math.Sin(arrowRadians - headAngle)
                ),
                new Point(
                    endPoint.X - headLength * Math.Cos(arrowRadians + headAngle),
                    endPoint.Y - headLength * Math.Sin(arrowRadians + headAngle)
                )
            };

            var arrowGeometry = new PolylineGeometry(arrowPoints, true);
            context.DrawGeometry(Brushes.White, null, arrowGeometry);
        }

        private void UpdateArrowAngle(Point position, bool isLeft)
        {
            // Use the correct center based on which dial is being dragged
            Point centerPoint = isLeft ? leftCenterPoint : rightCenterPoint;
            
            double dx = position.X - centerPoint.X;
            double dy = position.Y - centerPoint.Y;
            
            double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
            angle = (angle - 90 + 360) % 360;
            
            if (isLeft)
                arrowAngleLeft = angle;
            else
                arrowAngleRight = angle;
            
            // Immediate update for smooth response
            Dispatcher.UIThread.InvokeAsync(() => InvalidateVisual(), DispatcherPriority.Render);
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private class PlanetInfo
        {
            public string Id { get; set; } = "";
            public string Symbol { get; set; } = "";
            public double Value { get; set; }
        }

        private struct PlanetPosition
        {
            public double OriginalValue { get; set; }
            public double AdjustedValue { get; set; }
            public string Symbol { get; set; }
        }
    }
}