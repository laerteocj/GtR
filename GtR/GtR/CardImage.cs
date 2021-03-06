﻿using System;
using System.Drawing;

namespace GtR
{
    public class CardImage : ISaveableImage
    {
        public readonly float cardShortSideInInches;
        public readonly float cardLongSideInInches;
        public readonly float bleedSizeInInches;
        public readonly float borderPaddingInInches;

        private int CardShortSideInPixels => (int)(GraphicsUtilities.dpi * cardShortSideInInches) - 1;
        private int CardLongSideInPixels => (int)(GraphicsUtilities.dpi * cardLongSideInInches) - 1;
        private int BleedSizeInPixels => (int)Math.Round(GraphicsUtilities.dpi * bleedSizeInInches);
        private int BorderPaddingInPixels => (int)(GraphicsUtilities.dpi * borderPaddingInInches);

        private int CardShortSideInPixelsWithBleed => CardShortSideInPixels + BleedSizeInPixels * 2;
        private int CardLongSideInPixelsWithBleed => CardLongSideInPixels + BleedSizeInPixels * 2;

        public Point Origin => new Point(BleedSizeInPixels, BleedSizeInPixels);

        private int WidthInPixels => Orientation == ImageOrientation.Landscape ? CardLongSideInPixels : CardShortSideInPixels;
        private int HeightInPixels => Orientation == ImageOrientation.Portrait ? CardLongSideInPixels : CardShortSideInPixels;
        private int WidthInPixelsWithBleed => Orientation == ImageOrientation.Landscape ? CardLongSideInPixelsWithBleed : CardShortSideInPixelsWithBleed;
        private int HeightInPixelsWithBleed => Orientation == ImageOrientation.Portrait ? CardLongSideInPixelsWithBleed : CardShortSideInPixelsWithBleed;

        public Rectangle FullRectangle => new Rectangle(
            0,
            0,
            WidthInPixelsWithBleed,
            HeightInPixelsWithBleed);

        private Rectangle UsableRectangleWithoutPadding => new Rectangle(
            Origin.X,
            Origin.Y,
            WidthInPixels,
            HeightInPixels);

        public Rectangle UsableRectangle => new Rectangle(
            UsableRectangleWithoutPadding.X + BorderPaddingInPixels,
            UsableRectangleWithoutPadding.Y + BorderPaddingInPixels,
            UsableRectangleWithoutPadding.Width - (BorderPaddingInPixels * 2),
            UsableRectangleWithoutPadding.Height - (BorderPaddingInPixels * 2));

        public void Dispose()
        {
            Bitmap.Dispose();
            Bitmap = null;
        }

        public Bitmap Bitmap { get; private set; }
        private ImageOrientation Orientation { get; set; }

        public string Name { get; private set; }
        public string Subfolder { get; private set; }

        private const int borderRadius = 40;

        public CardImage(GtrConfig config, string name, string subfolder, ImageOrientation orientation)
        {
            cardShortSideInInches = config.CardShortSideInInches;
            cardLongSideInInches = config.CardLongSideInInches;
            bleedSizeInInches = config.BleedSizeInInches;
            borderPaddingInInches = config.BorderPaddingInInches;

            var bitmap = CreateBitmap(orientation);
            Bitmap = bitmap;
            Orientation = orientation;
            Name = name;
            Subfolder = subfolder;
        }

        private Bitmap CreateBitmap(ImageOrientation orientation)
        {
            switch (orientation)
            {
                case ImageOrientation.Landscape:
                    return GraphicsUtilities.CreateBitmap(CardLongSideInPixelsWithBleed, CardShortSideInPixelsWithBleed);
                case ImageOrientation.Portrait:
                    return GraphicsUtilities.CreateBitmap(CardShortSideInPixelsWithBleed, CardLongSideInPixelsWithBleed);
            }
            return null;
        }

        public void PrintCardBorderAndBackground(Color backgroundColor, Color? outerBorderColor = null)
        {
            using (var graphics = Graphics.FromImage(Bitmap))
            {
                if (outerBorderColor.HasValue)
                    graphics.FillRoundedRectangle(
                        new SolidBrush(outerBorderColor.Value),
                        0,
                        0,
                        WidthInPixelsWithBleed,
                        HeightInPixelsWithBleed,
                        borderRadius);
                graphics.FillRoundedRectangle(
                    new SolidBrush(backgroundColor),
                    UsableRectangleWithoutPadding.X,
                    UsableRectangleWithoutPadding.Y,
                    UsableRectangleWithoutPadding.Width,
                    UsableRectangleWithoutPadding.Height,
                    borderRadius);
            }
        }
    }
}
