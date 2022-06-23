using System;
using System.Drawing;

namespace Mandelbrot.Core
{
	public class ConsoleTests
	{
        public static void Colors()
        {
            var start = Color.Black;
            var end = Color.FromArgb(255, 200, 100);

            Console.WriteLine($"Interpolating from ({ColorString(start)}) to ({ColorString(end)}).");

            foreach (var c in GenerateColors.GetGradients(200, 0.7))
            {
                Console.BackgroundColor = GetConsoleColor(c);
                Console.Write("  ");
                Console.ResetColor();
                Console.WriteLine($"{ColorString(c)}");
            }

            Console.WriteLine("\nNote: console colors are very limited, so the color blocks are just a rough approximations.");
        }

		public static ConsoleColor GetConsoleColor(Color c)
        {
            int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
            index |= (c.R > 64) ? 4 : 0;
            index |= (c.G > 64) ? 2 : 0;
            index |= (c.B > 64) ? 1 : 0;

            return (ConsoleColor)index;
        }

        public static string ColorString(Color c)
        {
            return $"{c.R}, {c.G}, {c.B}";
        }
	}
}

