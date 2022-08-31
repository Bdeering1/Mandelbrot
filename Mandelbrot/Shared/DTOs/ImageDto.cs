using System;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Shared.DTOs
{
    public class ImageDto
    {
        public string Image { get; set; }

        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Zoom { get; set; }
        public int Precision { get; set; }
        public uint MaxIterations { get; set; }

        public ImageDto(string image, double positionX, double positionY, double zoom, int precision, uint maxIterations)
        {
            Image = image;
            PositionX = positionX;
            PositionY = positionY;
            Zoom = zoom;
            Precision = precision;
            MaxIterations = maxIterations;
        }

        public ImageDto() {}
    }
}

