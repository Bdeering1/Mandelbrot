using System;
using System.Numerics;
using System.Reflection;
using Mandelbrot.Shared.Models;
using Xunit.Sdk;

namespace Test.Models
{
	public class BigDecimalTestData : DataAttribute
	{
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] { (BigDecimal)12345, 2, 12, 3 };
            yield return new object[] { (BigDecimal)12.3456789, 2, 12, 0 };
        }
    }
}

