using System.Text.RegularExpressions;
namespace ThermalClasses.PhysicsLaws;

public static class PhysicsEquations
{
    #region Constants
    private const double k = 1.23E-23;
    private const double r = 8.31;
    #endregion

    #region Methods
    /// <summary>
    /// Calculates the root mean square velocity according to the pressure, volume, and number of particles
    /// </summary>
    /// <param name="p">Pressure</param>
    /// <param name="V">Volume</param>
    /// <param name="N">Number of particles</param>
    /// <param name="avgMass">Average mass of particles</param>
    /// <returns>The root mean square velocity</returns>
    public static double CalcVRMS(double p, double V, int N, double avgMass)
    {
        return (double)Math.Sqrt(3 * p * V / (N * avgMass));
    }

    /// <summary>
    /// Calculates the root mean square velocity according to the temperature
    /// </summary>
    /// <param name="T">Temperature</param>
    /// <param name="avgMass">Average mass of particles</param>
    /// <returns>The root mean square velocity</returns>
    public static double CalcVRMS(double T, double avgMass)
    {
        return Math.Sqrt(3 * k * T / avgMass);
    }

    // These calculations use the formula pV = NkT
    /// <summary>
    /// Calculates the volume using V = NkT/p
    /// </summary>
    /// <param name="p">Pressure</param>
    /// <param name="N">Number of particles</param>
    /// <param name="T">Temperature</param>
    /// <returns>The volume of the simulation</returns>
    public static double CalcVolume(double p, int N, double T)
    {
        return N * k * T / p;
    }

    /// <summary>
    /// Calculates the pressure using p = NkT/V
    /// </summary>
    /// <param name="V">Volume</param>
    /// <param name="N">Number of particles</param>
    /// <param name="T">Temperature</param>
    /// <returns>The pressure of the simulation</returns>
    public static double CalcPressure(double V, int N, double T, int roundTo)
    {
        return RoundDouble(N * k * T / V, roundTo);
    }

    /// <summary>
    /// Calculates the temperature using T = pV/Nk
    /// </summary>
    /// <param name="p">Pressure</param>
    /// <param name="V">Volume</param>
    /// <param name="N">Number of particles</param>
    /// <returns>The temperature of the simulation</returns>
    public static double CalcTemperature(double p, double V, int N)
    {
        return p * V / (N * k);
    }

    /// <summary>
    /// Calculates the temperature of the simulation using T = m(vRMS)^2/3k
    /// </summary>
    /// <param name="vRMS">Average velocity</param>
    /// <param name="avgMass">Average mass of particles</param>
    /// <returns>The temperature of the simulation</returns>
    public static double CalcTemperature(double vRMS, double avgMass)
    {
        return (double)Math.Round(avgMass * Math.Pow(vRMS, 2) / (3 * k));
    }

    /// <summary>
    /// Returns the number of moles from a given number of particles using n = Nk/R
    /// </summary>
    /// <param name="N">Number of particles</param>
    /// <returns>The number of moles in the simulation</returns>
    public static double NumberToMoles(int N, int roundTo)
    {
        return RoundDouble(N * k / r, roundTo);
    }

    public static double NumberToMoles(int N)
    {
        return N * k / r;
    }

    /// <summary>
    /// Returns the number of particles from a given number of moles using N = nR/k
    /// </summary>
    /// <param name="moles">Number of moles</param>
    /// <returns>The number of particles</returns>
    public static double MolesToNumber(double moles)
    {
        return moles * r / k;
    }

    /// <summary>
    /// Rounds a double type to a specific number of decimal places
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <param name="roundTo">The number of decimal places to round to</param>
    /// <returns>The value rounded to a number of decimal places</returns>
    private static double RoundDouble(double value, int roundTo)
    {
        return Convert.ToDouble(Math.Round(Convert.ToDecimal(value), roundTo));
    }

    /// <summary>
    /// Calculates the volume for a box from the given inputs.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static double CalcVolume(double width, double height, double length)
    {
        return width * height * length;
    }

    /// <summary>
    /// Calculates the volume for a cylinder
    /// </summary>
    /// <param name="length"></param>
    /// <param name="area"></param>
    /// <returns></returns>
    public static double CalcVolume(double length, double radius)
    {
        return length * (Math.Pow(radius, 2) * Math.PI);
    }
    #endregion
}
