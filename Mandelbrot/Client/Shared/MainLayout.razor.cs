using System;
using MudBlazor;

namespace Mandelbrot.Client.Shared
{
    public partial class MainLayout
    {
        private MudTheme theme;

        public MainLayout()
        {
            theme = new MudTheme()
            {
                Palette = new Palette()
                {
                    Background = "#1A1A1A"
                }
            };
        }
    }
}

