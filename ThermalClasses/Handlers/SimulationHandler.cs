using System.Runtime.InteropServices.ComTypes;
using System.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.Particles;
using ThermalClasses.CollisionHandling;
using ThermalClasses.PhysicsLaws;
using ThermalClasses.GameObjects.ObjectCollections;
using System.Globalization;

namespace ThermalClasses.Handlers;

public class SimulationHandler : Handler
{
    #region Fields
    #region Objects
    #region Particles
    private Polygon[] smallParticles, largeParticles; // Total particles present in simulation
    private List<Polygon> activeSmallParticles, activeLargeParticles; // All enabled particles
    private SHG spatialHashGrid;
    #endregion
    private SimulationBox simulationBox;
    #region GUIObjects
    private List<Button> buttonCollection;
    private List<UpDownButton> upDownCollection;
    private List<Label> labelCollection;
    private CheckButton pauseButton;
    private UpDownButton smallParticleControl;
    private UpDownButton largeParticleControl;
    private UpDownButton temperatureControl;
    private Slider volumeSlider;
    private Label dataBox, volumeDisp, temperatureDisp, pressureDisp, numParticlesDisp;
    private RadioButtons keepConstant;
    #endregion
    #endregion
    private PhysicalConstants constants = new PhysicalConstants();
    private double volume, maxVolume, changeInVolume, minVolume; // Measured in metres cubed
    private double pressure, rmsVelocity, avgMass; // Measured in Kelvin, Pascals, metres per second, kilograms
    private double temperature;
    private int NumParticles => activeSmallParticles.Count + activeLargeParticles.Count;
    private bool paused;
    private Color penColour = Color.White;
    #endregion

    #region Properties
    public Color BackgroundColour { get; set; }
    public Color HoverColour { get; set; }
    #endregion

    #region Methods
    public SimulationHandler(Game game, Rectangle renderRectangle)
    {
        this.renderRectangle = renderRectangle;
        this.game = game;
        content = game.Content;

        // Initialising physical properties
        temperature = 300; // Room temperature
        avgMass = 1.107E-24;
        pressure = 0;
        maxVolume = 300;
        minVolume = 50;
        volume = maxVolume;
        changeInVolume = maxVolume - minVolume;
        rmsVelocity = PhysicsEquations.CalcVRMS(temperature, avgMass);
        paused = false;
    }

    #region Initialisation

    #region Particle Initialisation
    private void InitialiseParticles()
    {
        for (var i = 0; i < smallParticles.Length; i++)
        {
            smallParticles[i] = NewSmallCircle(i);
            largeParticles[i] = NewLargeCircle(i);
        }
    }

    private Polygon NewSmallCircle(int identifier)
    {
        return new Polygon(content.Load<Texture2D>("SimulationAssets/YellowParticle"), new Vector2(0, 0), new Vector2(0, 0), avgMass - (0.001*avgMass), 15, Color.White, new Point(5, 5)) // avgMass - (0.001*avgMass)
        {
            Enabled = false,
            Type = "Small",
            Identifier = identifier,
        };
    }

    private Polygon NewLargeCircle(int identifier)
    {
        return new(content.Load<Texture2D>("SimulationAssets/BlueParticle"), new Vector2(0, 0), new Vector2(0, 0), avgMass + (0.001*avgMass), 15, Color.White, new Point(10, 10)) // avgMass + (0.001*avgMass)
        {
            Enabled = false,
            Type = "Large",
            Identifier = identifier,
        };
    }
    #endregion

    #region Game Object Initialisation
    private void InitSimBox()
    {
        Point fixedStart = new(renderRectangle.Left, (int)(renderRectangle.Top + (renderRectangle.Height * 0.15)));
        Point movingSize = new(10, (int)(renderRectangle.Height * 0.7));
        Point fixedSize = new((int)(renderRectangle.Width * 0.7), movingSize.Y);
        Rectangle fixedRect = new(fixedStart, fixedSize);
        Rectangle movingRect = new(fixedStart, movingSize);

        GameObject fixedBox = new GameObject(content.Load<Texture2D>("SimulationAssets/FixedBox"), Color.White, fixedRect);
        GameObject movingBox = new GameObject(content.Load<Texture2D>("SimulationAssets/MovingBox"), Color.White, movingRect);

        simulationBox = new SimulationBox(game, fixedBox, movingBox, (int)(renderRectangle.Width * 0.55), 0);
    }
    #endregion

    public override void Initialize()
    {
        const int listSize = 500;
        smallParticles = new Polygon[listSize];
        largeParticles = new Polygon[listSize];
        activeSmallParticles = new List<Polygon>();
        activeLargeParticles = new List<Polygon>();
        buttonCollection = new List<Button>();
        upDownCollection = new List<UpDownButton>();
        labelCollection = new List<Label>();
    }

    public override void LoadContent()
    {
        SpriteFont font = content.Load<SpriteFont>("GeneralAssets/Arial");
        Color unclickedColour = Color.White;
        // Particle Initialisation
        InitialiseParticles();

        // GameObject Initialisation
        InitSimBox();
        // Buttons Initialisation
        Point pauseSize = new Point(40, 40);
        Vector2 pausePosition = new Vector2(pauseSize.X / 2, simulationBox.BoxRect.Y - 35);
        pauseButton = new CheckButton(content.Load<Texture2D>("GeneralAssets/PauseButton"), content.Load<Texture2D>("GeneralAssets/PlayButton"), font, pausePosition, unclickedColour, penColour, pauseSize)
        {
            HoverColour = HoverColour,
        };
        pauseButton.Click += PauseSimulation_Click;

        Texture2D upTexture = content.Load<Texture2D>("GeneralAssets/UpButton");
        Texture2D downTexture = content.Load<Texture2D>("GeneralAssets/DownButton");
        Texture2D upDownLabelTexture = content.Load<Texture2D>("GeneralAssets/LabelBox1");
        Texture2D sliderLabelTexture = content.Load<Texture2D>("GeneralAssets/SliderFrame2");
        Texture2D sliderButtonTexture = content.Load<Texture2D>("GeneralAssets/SliderButton_Unclicked");
        Texture2D sliderButtonHoverTexture = content.Load<Texture2D>("GeneralAssets/SliderButton_Clicked");
        Texture2D outputBox = content.Load<Texture2D>("GeneralAssets/DataBox");
        Texture2D buttonUnchecked = content.Load<Texture2D>("GeneralAssets/Button_Unchecked");
        Texture2D buttonChecked = content.Load<Texture2D>("GeneralAssets/Button_Checked");

        // Display values
        Rectangle resultsBox = new Rectangle(new Point((int)(renderRectangle.Width * 0.73), (int)(renderRectangle.Height * 0.6)), new Point((int)(renderRectangle.Width * 0.27), (int)(renderRectangle.Height * 0.35)));
        dataBox = new Label(outputBox, unclickedColour, resultsBox, font, penColour);

        Point dispSize = new Point((int)(resultsBox.Width * 0.75), 40);
        Rectangle volumeDispRect = new Rectangle(new Point(resultsBox.X + ((resultsBox.Width / 2) - (dispSize.X / 2)), resultsBox.Top + 20), dispSize);
        Rectangle temperatureRect = new Rectangle(new Point(volumeDispRect.X, volumeDispRect.Y + 50), dispSize);
        Rectangle pressureRect = new Rectangle(new Point(volumeDispRect.X, temperatureRect.Y + 50), dispSize);
        Rectangle numParticlesRect = new Rectangle(new Point(volumeDispRect.X, pressureRect.Y + 50), dispSize);
        volumeDisp = new Label(upDownLabelTexture, unclickedColour, volumeDispRect, font, penColour)
        {
            Text = $"Volume: {volume}m^3"
        };
        temperatureDisp = new Label(upDownLabelTexture, unclickedColour, temperatureRect, font, penColour)
        {
            Text = $"Temperature: {temperature}K / {temperature - 273} Degrees C"
        };
        pressureDisp = new Label(upDownLabelTexture, unclickedColour, pressureRect, font, penColour)
        {
            Text = $"Pressure: {pressure}Pa"
        };
        numParticlesDisp = new Label(upDownLabelTexture, unclickedColour, numParticlesRect, font, penColour)
        {
            Text = $"Number of moles: {PhysicsEquations.NumberToMoles(NumParticles)}mol"
        };

        Point radioSize = new Point(225,120);
        Rectangle radioRect = new Rectangle(new Point(resultsBox.X + ((resultsBox.Width / 2) - (radioSize.X / 2)), numParticlesRect.Y + 50), radioSize);
        string[] text = {"Volume", "Temperature", "Pressure => Volume", "Pressure => Temperature"};
        Vector2 startButton = new Vector2(radioRect.X + 20, radioRect.Y + 20);
        keepConstant = new RadioButtons(buttonUnchecked, buttonChecked, upDownLabelTexture, outputBox, radioRect, startButton, text, font, unclickedColour, HoverColour, penColour, 1);

        Point particleControlSize = new Point(200, 40);
        Rectangle largeParticleButtonRect = new Rectangle(new Point(simulationBox.BoxRect.Right - particleControlSize.X, simulationBox.BoxRect.Bottom + 15), particleControlSize);
        Rectangle smallParticleButtonRect = new Rectangle(new Point(largeParticleButtonRect.X, largeParticleButtonRect.Y + particleControlSize.Y + 10), particleControlSize);
        smallParticleControl = new UpDownButton(upTexture, downTexture, upDownLabelTexture, smallParticleButtonRect, "Small Particles", font, penColour, unclickedColour, HoverColour);
        smallParticleControl.DownButton.Click += RemoveSmallParticles_Click;
        smallParticleControl.UpButton.Click += AddSmallParticles_Click;

        largeParticleControl = new UpDownButton(upTexture, downTexture, upDownLabelTexture, largeParticleButtonRect, "Large Particles", font, penColour, unclickedColour, HoverColour);
        largeParticleControl.DownButton.Click += RemoveLargeParticles_Click;
        largeParticleControl.UpButton.Click += AddLargeParticles_Click;

        Rectangle tempButtonRect = new Rectangle(new Point(simulationBox.BoxRect.Right - particleControlSize.X, simulationBox.BoxRect.Top - 15 - particleControlSize.Y), particleControlSize);
        temperatureControl = new UpDownButton(upTexture, downTexture, upDownLabelTexture, tempButtonRect, "Temperature", font, penColour, unclickedColour, HoverColour);
        temperatureControl.DownButton.Click += DecreaseTemperature_Click;
        temperatureControl.UpButton.Click += IncreaseTemperature_Click;

        // Initialising the volume slider
        Rectangle volumeSliderRect = new Rectangle(new Point(simulationBox.BoxRect.Left, simulationBox.BoxRect.Bottom + 15), new Point((int)(renderRectangle.Width * 0.55) - 10, 20));
        Point volumeFixedTextureSize = new Point((int)(renderRectangle.Width * 0.55) - volumeSliderRect.Height - 10, 4);
        Rectangle volumeFixedTextureRect = new Rectangle(new Point(simulationBox.BoxRect.Left + (volumeSliderRect.Height / 2), simulationBox.BoxRect.Bottom + 15 + 8), volumeFixedTextureSize);
        volumeSlider = new Slider(sliderButtonTexture, sliderButtonHoverTexture, sliderLabelTexture, font, volumeSliderRect, volumeFixedTextureRect, 1, unclickedColour, penColour);
        volumeSlider.sliderButton.Click += ChangeVolume_Click;
        volumeSlider.sliderButton.Click.Invoke(new object(), EventArgs.Empty);

        // Putting all objects into a list for easier updating and drawing
        buttonCollection.Add(pauseButton);

        upDownCollection.Add(smallParticleControl);
        upDownCollection.Add(largeParticleControl);

        labelCollection.Add(dataBox);
    }
    #endregion

    #region Updating & Drawing
    // Calls the update method of all objects that need updating (buttons, particles, sliders etc.)
    public override void Update(GameTime gameTime)
    {
        pauseButton.Update(gameTime);
        keepConstant.Update(gameTime);
        if (keepConstant.ChangedIndex)
        {
            constants.ChangeIndex(keepConstant.CheckedIndex);
        }

        for (var i = 0; i < buttonCollection.Count; i++)
        {
            buttonCollection[i].Update(gameTime);
        }
        for (var i = 0; i < upDownCollection.Count; i++)
        {
            upDownCollection[i].Update(gameTime);
        }
        if (!constants.Volume && !constants.PressureVol)
        {
            volumeSlider.Update(gameTime);
        }
        if (!constants.Temperature && !constants.PressureTemp)
        {
            temperatureControl.Update(gameTime);
        }
        if (!paused)
        {
            UpdateParticles(gameTime);
        }
    }

    // Calls the draw methods of all GameObjects
    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        simulationBox.Draw(_spriteBatch);
        temperatureControl.Draw(_spriteBatch);
        volumeSlider.Draw(_spriteBatch);
        keepConstant.Draw(_spriteBatch);
        for (var i = 0; i < buttonCollection.Count; i++)
        {
            buttonCollection[i].Draw(_spriteBatch);
        }
        for (var i = 0; i < upDownCollection.Count; i++)
        {
            upDownCollection[i].Draw(_spriteBatch);
        }
        // Drawing all active particles to screen
        for (var i = 0; i < activeSmallParticles.Count; i++)
        {
            activeSmallParticles[i].Draw(_spriteBatch);
        }
        for (var x = 0; x < activeLargeParticles.Count; x++)
        {
            activeLargeParticles[x].Draw(_spriteBatch);
        }
        for (var i = 0; i < labelCollection.Count; i++)
        {
            labelCollection[i].Draw(_spriteBatch);
        }

        // Concerning the values of the properties and their displayed values
        if ((constants.PressureTemp || constants.PressureVol) && NumParticles > 0) // Pressure constant so other variables evaluated
        {
            if (constants.PressureTemp)
            {
                temperature = PhysicsEquations.CalcTemperature(pressure, volume, NumParticles);
            }
            else
            {
                volume = PhysicsEquations.CalcVolume(pressure, NumParticles, temperature);
                // Update the slider
                simulationBox.SetVolume(InverseScale(volume));
                volumeSlider.sliderButton.SetPosition(new Vector2(InverseScale(volume), volumeSlider.sliderButton.Position.Y));
            }
        }
        else // Pressure not constant so calculated
        {
            pressure = PhysicsEquations.CalcPressure(volume, NumParticles, temperature, 25);
        }
        volumeDisp.Text = $"Volume: {Convert.ToInt32(volume)} metres cubed";
        volumeDisp.Draw(_spriteBatch);
        temperatureDisp.Text = $"Temperature: {Convert.ToInt32(temperature)}K / {Convert.ToInt32(temperature - 273)} degrees C";
        temperatureDisp.Draw(_spriteBatch);
        pressureDisp.Text = $"Pressure: {pressure.ToString("e2", CultureInfo.InvariantCulture)}Pa";
        pressureDisp.Draw(_spriteBatch);
        numParticlesDisp.Text = $"Number of moles: {PhysicsEquations.NumberToMoles(NumParticles, 25).ToString("e2", CultureInfo.InvariantCulture)}mol";
        numParticlesDisp.Draw(_spriteBatch);
    }

    #region Particle Updates
    private void UpdateParticles(GameTime gameTime)
    {
        for (var i = 0; i < activeSmallParticles.Count; i++)
        {
            activeSmallParticles[i].Update(gameTime);
            activeSmallParticles[i].colliding = false;
        }
        for (var i = 0; i < activeLargeParticles.Count; i++)
        {
            activeLargeParticles[i].Update(gameTime);
            activeLargeParticles[i].colliding = false;
        }

        // Broad phase: generate a spatial hash grid containing all particles
        List<Polygon> allParticles = new();
        allParticles.AddRange(activeSmallParticles);
        allParticles.AddRange(activeLargeParticles);

        spatialHashGrid = new SHG(simulationBox.BoxRect, 15);
        spatialHashGrid.Insert(allParticles);

        // Handling particle-particle collisions
        // Narrow phase: confirm whether pairs of particles are actually colliding
        foreach (var polygonList1 in spatialHashGrid.ReturnParticleCollisions())
        {
            // Check whether each polygon in bucket is colliding with every other polygon
            for (var i = 0; i < polygonList1.Count - 1; i++)
            {
                for (var j = i + 1; j < polygonList1.Count; j++)
                {
                    if (polygonList1[i].Type == "Small")
                    {
                        if (polygonList1[j].Type == "Small")
                        {
                            ParticleCollisionUpdates(ref activeSmallParticles, polygonList1[i].Identifier, ref activeSmallParticles, polygonList1[j].Identifier, gameTime);
                        }
                        else
                        {
                            ParticleCollisionUpdates(ref activeSmallParticles, polygonList1[i].Identifier, ref activeLargeParticles, polygonList1[j].Identifier, gameTime);
                        }
                    }
                    else
                    {
                        if (polygonList1[j].Type == "Small")
                        {
                            ParticleCollisionUpdates(ref activeLargeParticles, polygonList1[i].Identifier, ref activeSmallParticles, polygonList1[j].Identifier, gameTime);
                        }
                        else
                        {
                            ParticleCollisionUpdates(ref activeLargeParticles, polygonList1[i].Identifier, ref activeLargeParticles, polygonList1[j].Identifier, gameTime);
                        }
                    }
                }
            }
        }

        // Handling particle-border collisions
        // Finding all particles potentially colliding with the border and updating them
        var polygonList = spatialHashGrid.ReturnBoundaryCollisions();
        for (var i = 0; i < polygonList.Count; i++)
        {
            foreach (var particle in polygonList[i])
            {
                Polygon myParticle = CollisionFunctions.BoundaryCollisionHandling(particle, simulationBox.BoxRect, gameTime);

                // Teleporting all particles outside of the current border back in to the box
                // All particles outside should just be outside the x region of the box
                if (myParticle.Position.X < simulationBox.BoxRect.Left)
                {
                    myParticle.ChangePositionBy(new Vector2(20, 0));
                }

                // Move particles back into original lists
                if (particle.Type == "Small")
                {
                    activeSmallParticles[myParticle.Identifier] = myParticle;
                }
                else
                {
                    activeLargeParticles[myParticle.Identifier] = myParticle;
                }
            }
        }
    }

    private void ParticleCollisionUpdates(ref List<Polygon> a1, int i1, ref List<Polygon> a2, int i2, GameTime gameTime)
    {
        if (CollisionFunctions.SeparatingAxisTheorem(a1[i1], a2[i2]))
        {
            // Change the position of the particle with the highest velocity for cleaner graphics
            a1[i1].SetPosition(CollisionFunctions.TouchingPosition(a1[i1], a2[i2], gameTime));
            a1[i1].CollisionParticleUpdate(a2[i2], gameTime);

            a2[i2].CollisionParticleUpdate(a1[i1], gameTime);
        }
    }

    private void ChangePenColour(Color colour)
    {
        for (var i = 0; i < buttonCollection.Count; i++)
        {
            buttonCollection[i].PenColour = colour;
        }
        for (var i = 0; i < upDownCollection.Count; i++)
        {
            upDownCollection[i].ChangePenColour(colour);
        }
        foreach (var item in labelCollection)
        {
            item.PenColour = colour;
        }
        temperatureControl.ChangePenColour(colour);
        volumeSlider.ChangePenColour(colour);
        volumeDisp.PenColour = temperatureDisp.PenColour = pressureDisp.PenColour = numParticlesDisp.PenColour = colour;
        keepConstant.ChangePenColour(colour);
    }
    #endregion
    #endregion

    #region Events
    #region Adding Particles
    private void AddSmallParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            AddParticles(10, ref activeSmallParticles, ref smallParticles);
        }
    }

    private void AddLargeParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            AddParticles(10, ref activeLargeParticles, ref largeParticles);
        }
    }

    // Method to add particles to the list of active particles (called by event)
    private void AddParticles(int amount, ref List<Polygon> activeParticles, ref Polygon[] allParticles)
    {
        Random rnd = new Random();
        if (activeParticles.Count + amount <= allParticles.Length)
        {
            Vector2 insertPosition = new Vector2(simulationBox.BoxRect.Right - 10, simulationBox.BoxRect.Top + 10);
            // Creating the input velocities
            float theta = (float)((Math.PI / (2 * amount)) + rnd.NextDouble());
            // Enabling particles to allow them to be drawn and updated
            int indexToEnable = activeParticles.Count;
            for (var i = indexToEnable; i < indexToEnable + amount; i++)
            {
                Vector2 insertionVelocity = new Vector2((float)(-1 * (float)Math.Sin(theta) * rmsVelocity), (float)((float)Math.Cos(theta) * rmsVelocity));
                allParticles[i].Enabled = true;
                allParticles[i].SetPosition(insertPosition);
                allParticles[i].ChangeVelocityTo(insertionVelocity);
                activeParticles.Add(allParticles[i]);
                insertPosition.Y += 20; // Inserts next particle into a space below previous particle
                theta += (float)((Math.PI / 2 / amount) + rnd.NextDouble()); // Modifies angle at which the magnitude of the velocity acts in
            }
        }
        pressure = PhysicsEquations.CalcPressure(volume, NumParticles, temperature, 25);
    }

    #endregion

    #region Removing Particles
    private void RemoveSmallParticles_Click(object sender, EventArgs e)
    {
        RemoveParticles(10, ref activeSmallParticles, ref smallParticles);
    }

    private void RemoveLargeParticles_Click(object sender, EventArgs e)
    {
        RemoveParticles(10, ref activeLargeParticles, ref largeParticles);
    }

    // Method to remove particles from the list of active particles (called by event)
    private void RemoveParticles(int amount, ref List<Polygon> activeList, ref Polygon[] allParticles)
    {
        if (activeList.Count >= amount)
        {
            int indexToDisable = activeList.Count - 1;
            for (var i = indexToDisable; i > indexToDisable - amount; i--)
            {
                activeList.RemoveAt(activeList.Count - 1);
                allParticles[i].Enabled = false;
            }
        }
    }
    #endregion

    private void PauseSimulation_Click(object sender, EventArgs e)
    {
        paused = !paused;
    }

    #region Changing Volume
    private void ChangeVolume_Click(object sender, EventArgs e)
    {
        simulationBox.SetVolume(volumeSlider.sliderButton.Position.X);
        volume = ScaleVolume(volumeSlider.sliderButton.Position.X);
    }

    private double ScaleVolume(float x)
    {
        float maxX = volumeSlider.MaxX;
        float minX = volumeSlider.MinX;
        return (changeInVolume / (minX - maxX) * (x - minX)) + maxVolume;
    }

    private float InverseScale(double y)
    {
        float maxX = volumeSlider.MaxX;
        float minX = volumeSlider.MinX;
        return (float)(((y - maxVolume) / (changeInVolume / (minX - maxX))) + minX);
    }
    #endregion

    #region Changing Temperature
    private void IncreaseTemperature_Click(object sender, EventArgs e)
    {
        if (PhysicsEquations.CalcVolume(pressure, NumParticles, temperature + 10) <= maxVolume)
        {
            temperature += 10;
            if (!constants.PressureTemp && !constants.PressureVol)
            {
                rmsVelocity = PhysicsEquations.CalcVRMS(temperature, avgMass);
                ChangeVelocities(rmsVelocity);
            }
        }
    }

    private void DecreaseTemperature_Click(object sender, EventArgs e)
    {
        if (PhysicsEquations.CalcVolume(pressure, NumParticles, temperature - 10) >= minVolume)
        {
            temperature -= 10;
            if (!constants.PressureTemp && !constants.PressureVol)
            {
                var newRmsVelocity = PhysicsEquations.CalcVRMS(temperature, avgMass);
                ChangeVelocities(newRmsVelocity - rmsVelocity);
                rmsVelocity = newRmsVelocity;
            }
        }
    }

    private void ChangeVelocities(double amount)
    {
        for (var i = 0; i < activeSmallParticles.Count; i++)
        {
            float newLength = (float)(activeSmallParticles[i].CurrentVelocity.Length() + amount);
            activeSmallParticles[i].ChangeVelocityTo(newLength / activeSmallParticles[i].CurrentVelocity.Length() * activeSmallParticles[i].CurrentVelocity);
        }
        for (var i = 0; i < activeLargeParticles.Count; i++)
        {
            float newLength = (float)(activeLargeParticles[i].CurrentVelocity.Length() + amount);
            activeLargeParticles[i].ChangeVelocityTo(newLength / activeLargeParticles[i].CurrentVelocity.Length() * activeLargeParticles[i].CurrentVelocity);
        }
    }
    #endregion
    #endregion
    #endregion
}

/// <summary>
/// This struct manages the physical constants and variables in the simulation
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