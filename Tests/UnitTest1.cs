using System.Reflection.Emit;
using ThermalClasses.PhysicsLaws;
using Xunit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ThermalClasses.QuestionClasses;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public void KineticEnergyTemp()
    {
        Assert.Equal(300f, PhysicsEquations.CalcTemperature(300, 1.23E-25f));
    }

    private static double RoundDouble(double value, int roundTo)
    {
        return Convert.ToDouble(Math.Round(Convert.ToDecimal(value), roundTo));
    }

    [Fact]
    public void CorrectRounding()
    {
        Assert.Equal(1.0E-10, RoundDouble(1.123E-10, 10));
    }

    [Fact]
    public void EnumTest()
    {
        string type = nameof(QuestionType.Mathematical);
        Assert.Equal("Mathematical", type);
    }
}