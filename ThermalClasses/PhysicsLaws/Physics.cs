using System.Text.RegularExpressions;
namespace ThermalClasses.PhysicsLaws;

public static class PhysicsEquations
{
    public static double CalcVRMS(double pressure, int volume, int numParticles, double avgMass)
    {
        return (double)Math.Sqrt(3 * pressure * volume / (numParticles * avgMass));
    }

    private const double k = 1.23E-23f;

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
    /// <returns></returns>
    public static double CalcVolume(double p, int N, double T)
    {
        return N * k * T/p;
    }

    /// <summary>
    /// Calculates the pressure using p = NkT/V
    /// </summary>
    /// <param name="V">Volume</param>
    /// <param name="N">Number of particles</param>
    /// <param name="T">Temperature</param>
    /// <returns></returns>
    public static double CalcPressure(double V, int N, double T, int roundTo)
    {
        return RoundDouble(N*k*T/V, roundTo);
    }

    /// <summary>
    /// Calculates the temperature using T = pV/Nk
    /// </summary>
    /// <param name="p">Pressure</param>
    /// <param name="V">Volume</param>
    /// <param name="N">Number of particles</param>
    /// <returns></returns>
    public static double CalcTemperature(double p, double V, int N)
    {
        return p*V/(N*k);
    }

    /// <summary>
    /// Calculates the temperature of the simulation using T = m(vRMS)^2/3k
    /// </summary>
    /// <param name="vRMS">Average velocity</param>
    /// <param name="avgMass">Average mass of particles</param>
    /// <returns></returns>
    public static double CalcTemperature(double vRMS, double avgMass)
    {
        return (double)Math.Round(avgMass * Math.Pow(vRMS, 2) / (3 * k));
    }

    /// <summary>
    /// Returns the number of moles from a given number of particles using n = Nk/R
    /// </summary>
    /// <param name="N">Number of particles</param>
    public static double NumberToMoles(int N, int roundTo)
    {
        return RoundDouble(N * k / 8.31, roundTo);
    }

    public static double NumberToMoles(int N)
    {
        return (N *k / 8.31);
    }

    private static double RoundDouble(double value, int roundTo)
    {
        return Convert.ToDouble(Math.Round(Convert.ToDecimal(value), roundTo));
    }
}
