using System.Drawing;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class GenerateSet
    {
        public static List<Color> MandelBrot(List<Color> colors, int width, int height)
        {
            Camera camera = new Camera(new BigComplex((BigDecimal)0, (BigDecimal)0), 1.0);

            var set = new List<Color>();
            for(int y = 0; y < height/2; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    var escTime = EscapeTime.CalcEscapeTime(camera.TranslatePixel(x + 1, y + 1));
                    set.Add(colors[escTime - 1]);
                }
                Console.WriteLine(y);
            }

            Console.WriteLine("Done!");

            return set;
        }
    }
}
