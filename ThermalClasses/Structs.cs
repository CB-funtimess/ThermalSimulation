using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects.Particles;

namespace ThermalClasses;

public struct BorderCollisions
{
    public bool left;
    public bool right;
    public bool top;
    public bool bottom;
    public BorderCollisions(bool left, bool right, bool top, bool bottom)
    {
        this.left = left;
        this.right = right;
        this.top = top;
        this.bottom = bottom;
    }

    public BorderCollisions()
    {
        left = right = top = bottom = false;
    }
}

/// <summary>
/// A struct to record what is being kept constant within the simulation
/// </summary>
public struct PhysicalConstants
{
    public bool PressureVol, PressureTemp, Temperature, Volume;
    public PhysicalConstants()
    {
        PressureVol = false; // Pressure kept constant (V can only be changed by modifying T) if true
        PressureTemp = false; // Pressure kept constant (T can only be changed by modifying V) if true
        Temperature = true; // Temperature kept constant if true
        Volume = false; // Volume kept constant if true
    }

    public void ChangeIndex(int index)
    {
        PressureTemp = PressureVol = Temperature = Volume = false;
        if(index == 0)
        {
            Volume = true;
        }
        else if (index == 1)
        {
            Temperature = true;
        }
        else if (index == 2)
        {
            PressureVol = true;
        }
        else if(index == 3)
        {
            PressureTemp = true;
        }
    }
}

/// <summary>
/// An enum to track the type of particle
/// </summary>
public enum ParticleType
{
    Small,
    Large
}