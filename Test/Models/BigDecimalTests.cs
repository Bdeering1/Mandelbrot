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
    public void Constructor_Double_SetsValuesCorrectly()
    {
        outputHelper.WriteLine("Acting...");
        var sut = new BigDecimal(1.01);

        outputHelper.WriteLine("Asserting...");
        Assert.Equal(101, sut.Significand);
        Assert.Equal(-2, sut.Exponent);
    }

    [Fact]
    public void Truncate_LowPrecision_RemovesExtraDigits()
    {
        outputHelper.WriteLine("Arranging...");
        BigDecimal.Precision = 2;
        var sut = new BigDecimal(1.2345);

        outputHelper.WriteLine("Acting...");
        sut.Truncate();

        outputHelper.WriteLine("Asserting...");
        Assert.Equal(12, sut.Significand);
        Assert.Equal(-1, sut.Exponent);
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