using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class EscapeTime
    {

        public EscapeTime()
        {
        }

        public uint CalcEscapeTime(BigComplex pt)
        {
            var constant = new BigComplex(pt.r, pt.i);
            var current = BigComplex.Origin;

            var rSquared = (BigDecimal)0;
            var iSquared = (BigDecimal)0;

            uint iter = 0;
            while (rSquared + iSquared <= new BigDecimal(4) && iter < Config.MaxIterations)
            {
                current = new BigComplex(rSquared - iSquared, current.r * current.i + current.i * current.r) + constant;
                rSquared = current.r * current.r;
                iSquared = current.i * current.i;
                iter++;
            }
            return iter;
        }

        public bool CheckShapes(BigComplex pos)
        {
            var iSquared = pos.i * pos.i;
            var a = pos.r - (BigDecimal)0.25;
            var q = a * a + iSquared;
            if (q * (q + a) < iSquared * (BigDecimal)0.25
                || (pos.r * pos.r) + ((BigDecimal)2 * pos.r) + (BigDecimal)1 + iSquared < (BigDecimal)0.0625)
            {
                return true;
            }
            return false;
        }
    }
}

