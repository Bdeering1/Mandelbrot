using System.Drawing;
using Microsoft.AspNetCore.Components;

namespace Mandelbrot.Client.Shared.Prototype
{
    public partial class Canvas
    {
        [Parameter]
        public int Width { get; set; }

        [Parameter]
        public int PointSize { get; set; }

        [Parameter]
        public List<Color> Colors { get; set; }

        protected override void OnInitialized()
        {
            Console.WriteLine("Canvas:");
            foreach(var color in Colors)
            {
                Console.WriteLine($"{color} ({GetHexString(color)})");
            }
            base.OnInitialized();
        }

        private string GetHexString(Color c) =>
            $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
    }
}

