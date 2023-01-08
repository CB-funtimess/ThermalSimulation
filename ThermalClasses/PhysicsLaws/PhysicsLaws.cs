namespace ThermalClasses.PhysicsLaws;

public static class PhysicsEquations
{
    const float avgMass = 150;
    public static float CalcVRMS(float pressure, int volume, int numParticles)
    {
        return (float)Math.Sqrt(3 * pressure * volume / (numParticles * avgMass));
    }
}
