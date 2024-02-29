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
            var constant = new BigComplex(pt.R, pt.I);
            var current = BigComplex.Origin;

            var rSquared = (BigDecimal)0;
            var iSquared = (BigDecimal)0;

            uint iter = 0;
            while (rSquared + iSquared <= (BigDecimal)4 && iter < Config.MaxIterations)
            {
                current = new BigComplex(rSquared - iSquared, (BigDecimal)2 * current.R * current.I) + constant;
                rSquared = current.R * current.R;
                iSquared = current.I * current.I;
                iter++;
            }
            return iter;
        }

        public bool CheckShapes(BigComplex pos)
        {
            var iSquared = pos.I * pos.I;
            var a = pos.R - (BigDecimal)0.25;
            var q = a * a + iSquared;
            if (q * (q + a) < iSquared * (BigDecimal)0.25
                || (pos.R * pos.R) + ((BigDecimal)2 * pos.R) + (BigDecimal)1 + iSquared < (BigDecimal)0.0625)
            {
                return true;
            }
            return false;
        }
    }
}

