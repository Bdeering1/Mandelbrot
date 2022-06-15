using Mandelbrot.Models;
using Xunit.Abstractions;

namespace Test.Models;

public class BigDecimalTests
{
    private ITestOutputHelper outputHelper { get; }

    public BigDecimalTests(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
    }

    [Fact]
    public void Constructor_TrailingZeroes_RemovesTrailingZeroes()
    {
        outputHelper.WriteLine("Acting...");
        var sut = new BigDecimal(1000);

        outputHelper.WriteLine("Asserting...");
        Assert.Equal(1, sut.Significand);
        Assert.Equal(3, sut.Exponent);
    }

    [Fact]
    public void Add_DifferentExponents_ReturnsCorrectSum()
    {
        outputHelper.WriteLine("Arranging...");
        var a = new BigDecimal(1000);
        var b = new BigDecimal(1);
        var expected = new BigDecimal(1001);

        outputHelper.WriteLine("Acting...");
        var sum = a + b;

        outputHelper.WriteLine("Asserting...");
        Assert.Equal(expected, sum);
    }
}