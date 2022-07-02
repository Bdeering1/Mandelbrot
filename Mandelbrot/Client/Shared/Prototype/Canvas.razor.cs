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
        public List<string> Colors { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}

