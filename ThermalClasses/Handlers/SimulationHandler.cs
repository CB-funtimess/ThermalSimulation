using System.Transactions;
using System.Xml.Schema;
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
    private int indexSmall, indexLarge; // The index of the next particle to add
    private List<Polygon> activeParticles;
    private SHG spatialHashGrid;
    #endregion
    private SimulationBox simulationBox;
    #region GUIObjects
    private List<Button> buttonCollection;
    private List<UpDownButton> upDownCollection;
    private List<Label> labelCollection;
    private CheckButton pauseButton;
    private Button resetButton;
    private UpDownButton smallParticleControl;
    private UpDownButton largeParticleControl;
    private UpDownButton temperatureControl;
    private Slider volumeSlider;
    private Label dataBox, volumeDisp, temperatureDisp, pressureDisp, numParticlesDisp, constantLabel;
    private RadioButtons keepConstant;
    #endregion
    #endregion
    private PhysicalConstants constants = new PhysicalConstants();
    private double volume, maxVolume, changeInVolume, minVolume; // Measured in metres cubed
    private double pressure, rmsVelocity, avgMass; // Measured in Kelvin, Pascals, metres per second, kilograms
    private double temperature;
    private bool paused;
    private Color penColour = Color.White;
    #endregion

    #region Properties
    public Color BackgroundColour { get; set; }
    public Color HoverColour { get; set; }
    public int NumParticles => activeParticles.Count;
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
            Type = ParticleType.Small,
            Identifier = identifier,
        };
    }

    private Polygon NewLargeCircle(int identifier)
    {
        return new(content.Load<Texture2D>("SimulationAssets/BlueParticle"), new Vector2(0, 0), new Vector2(0, 0), avgMass + (0.001*avgMass), 15, Color.White, new Point(10, 10)) // avgMass + (0.001*avgMass)
        {
            Enabled = false,
            Type = ParticleType.Large,
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

        simulationBox = new SimulationBox(fixedBox, movingBox, (int)(renderRectangle.Width * 0.55), 0);
    }
    #endregion

    public override void Initialize()
    {
        const int listSize = 500;
        smallParticles = new Polygon[listSize];
        largeParticles = new Polygon[listSize];
        buttonCollection = new List<Button>();
        upDownCollection = new List<UpDownButton>();
        labelCollection = new List<Label>();
        indexLarge = indexSmall = 0;
        activeParticles = new List<Polygon>();
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
            HoverColour = HoverColour
        };
        pauseButton.Click += PauseSimulation_Click;
        Vector2 resetPos = new Vector2(pausePosition.X + pauseSize.X + 5, pausePosition.Y);
        resetButton = new Button(content.Load<Texture2D>("GeneralAssets/ResetButton"), font, resetPos, unclickedColour, penColour, pauseSize)
        {
            HoverColour = HoverColour
        };
        resetButton.Click += ResetSimulation_Click;

        Texture2D upTexture = content.Load<Texture2D>("GeneralAssets/UpButton");
        Texture2D downTexture = content.Load<Texture2D>("GeneralAssets/DownButton");
        Texture2D upDownLabelTexture = content.Load<Texture2D>("GeneralAssets/LabelBox1");
        Texture2D sliderLabelTexture = content.Load<Texture2D>("GeneralAssets/SliderFrame2");
        Texture2D sliderButtonTexture = content.Load<Texture2D>("GeneralAssets/SliderButton_Unclicked");
        Texture2D sliderButtonHoverTexture = content.Load<Texture2D>("GeneralAssets/SliderButton_Clicked");
        Texture2D outputBox = content.Load<Texture2D>("GeneralAssets/DataBox");
        Texture2D buttonUnchecked = content.Load<Texture2D>("GeneralAssets/Button_Unchecked");
        Texture2D buttonChecked = content.Load<Texture2D>("GeneralAssets/Button_Checked");

        Texture2D whiteTexture = content.Load<Texture2D>("Colours/WhiteColour");
        Texture2D greenTexture = content.Load<Texture2D>("Colours/GreenColour");
        Texture2D yellowTexture = content.Load<Texture2D>("Colours/YellowColour");
        Texture2D orangeTexture = content.Load<Texture2D>("Colours/OrangeColour");
        Texture2D cyanTexture = content.Load<Texture2D>("Colours/CyanColour");

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

        Point radioSize = new Point(230,120);
        Rectangle radioRect = new Rectangle(new Point(resultsBox.X + ((resultsBox.Width / 2) - (radioSize.X / 2)), numParticlesRect.Y + 70), radioSize);
        string[] text = {"Volume", "Temperature", "Pressure => Volume", "Pressure => Temperature"};
        Vector2 startButton = new Vector2(radioRect.X + 20, radioRect.Y + 20);
        keepConstant = new RadioButtons(buttonUnchecked, buttonChecked, upDownLabelTexture, outputBox, radioRect, startButton, text, font, unclickedColour, HoverColour, penColour, 1);
        Point constSize = new Point(radioSize.X, 20);
        Rectangle constRect = new Rectangle(new Point(radioRect.X, radioRect.Y - 20), constSize);
        constantLabel = new Label(upDownLabelTexture, unclickedColour, constRect, font, penColour)
        {
            Text = "Choose what to keep constant:"
        };

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
        buttonCollection.Add(resetButton);

        upDownCollection.Add(smallParticleControl);
        upDownCollection.Add(largeParticleControl);

        labelCollection.Add(dataBox);
        labelCollection.Add(constantLabel);
    }
    #endregion

    #region Updating & Drawing
    // Calls the update method of all objects that need updating (buttons, particles, sliders etc.)
    public override void Update(GameTime gameTime)
    {
        pauseButton.Update(gameTime);
        keepConstant.Update(gameTime);
        for (var i = 0; i < buttonCollection.Count; i++)
        {
            buttonCollection[i].Update(gameTime);
        }
        for (var i = 0; i < upDownCollection.Count; i++)
        {
            upDownCollection[i].Update(gameTime);
        }

        if (keepConstant.ChangedIndex)
        {
            constants.ChangeIndex(keepConstant.CheckedIndex);
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

    #region Particle Updates

    private void UpdateParticles(GameTime gameTime)
    {
        // Broad phase: generate a spatial hash grid containing all particles
        spatialHashGrid = new SHG(simulationBox.BoxRect, 15);
        spatialHashGrid.Insert(activeParticles);

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
                if (myParticle.Position.X + myParticle.XRadius < simulationBox.BoxRect.Left)
                {
                    myParticle.ChangePositionBy(new Vector2(20, 0));
                }

                // Move particles back into original lists
                activeParticles[GetIndex(myParticle.Type, myParticle.Identifier)] = myParticle;
            }
        }

        // Handling particle-particle collisions
        // Narrow phase: confirm whether pairs of particles are actually colliding
        foreach (var polygonList1 in spatialHashGrid.ReturnParticleCollisions())
        {
            // Check whether each polygon in bucket is colliding with every other polygon
            for (var i = 0; i < polygonList1.Count - 1; i++)
            {
                for (var j = i + 1; j < polygonList1.Count; j++)
                {
                    ParticleCollisionUpdates(GetIndex(polygonList1[i].Type, polygonList1[i].Identifier), GetIndex(polygonList1[j].Type, polygonList1[j].Identifier), gameTime);
                }
            }
        }

        for (var i = 0; i < activeParticles.Count; i++)
        {
            activeParticles[i].Update(gameTime);
        }
    }

    private void ParticleCollisionUpdates(int i1, int i2, GameTime gameTime)
    {
        if (CollisionFunctions.SeparatingAxisTheorem(activeParticles[i1], activeParticles[i2]))
        {
            double timeOfCollision = CollisionFunctions.TimeOfCollision(activeParticles[i1], activeParticles[i2], gameTime);
            if (timeOfCollision < gameTime.ElapsedGameTime.TotalSeconds)
            {
                activeParticles[i1].SetPosition(activeParticles[i1].PreviousPosition + (activeParticles[i1].CurrentVelocity * (float)timeOfCollision));
                activeParticles[i2].SetPosition(activeParticles[i2].PreviousPosition + (activeParticles[i2].CurrentVelocity * (float)timeOfCollision));
            }

            activeParticles[i1].CollisionParticleUpdate(activeParticles[i2], gameTime);
            activeParticles[i2].CollisionParticleUpdate(activeParticles[i1], gameTime);
        }
    }

    /// <summary>
    /// Gets the index of a specific particle in the active particle list
    /// </summary>
    /// <param name="type">Type of particle (small or large)</param>
    /// <param name="index">Index of particle in particle type array</param>
    /// <returns>Index of the particle</returns>
    private int GetIndex(ParticleType type, int index)
    {
        for (var i = 0; i < activeParticles.Count; i++)
        {
            if (activeParticles[i].Type == type && activeParticles[i].Identifier == index)
            {
                return i;
            }
        }
        return -1;
    }
    #endregion

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
        for (var i = 0; i < activeParticles.Count; i++)
        {
            activeParticles[i].Draw(_spriteBatch);
        }
        for (var i = 0; i < labelCollection.Count; i++)
        {
            labelCollection[i].Draw(_spriteBatch);
        }

        // THIS SECTION SHOULD BE AN INDIVIDUAL FUNCTION
        // SETTING PROPERTIES SHOULD BE HANDLED IN UPDATE()
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

    public override void ChangePenColour(Color colour)
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

    #region Events
    #region Adding Particles
    private void AddSmallParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            AddParticles(10, ref smallParticles, ref indexSmall);
        }
    }

    private void AddLargeParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            AddParticles(10, ref largeParticles, ref indexLarge);
        }
    }

    private void AddParticles(int amount, ref Polygon[] particleType, ref int index)
    {
        Random rnd = new Random();
        if (index + amount <= particleType.Length)
        {
            Vector2 insertPosition = new Vector2(simulationBox.BoxRect.Right - 10, simulationBox.BoxRect.Top + 10);
            // Creating the input velocities
            float theta = (float)((Math.PI / (2 * amount)) + rnd.NextDouble());
            // Enabling particles to allow them to be drawn and updated
            for (var i = 0; i < amount; i++)
            {
                Vector2 insertionVelocity = new Vector2((float)(-1 * (float)Math.Sin(theta) * rmsVelocity), (float)((float)Math.Cos(theta) * rmsVelocity));
                particleType[index].Enabled = true;
                particleType[index].SetPosition(insertPosition);
                particleType[index].SetVelocity(insertionVelocity);
                activeParticles.Add(particleType[index]);
                insertPosition.Y += 20; // Inserts next particle into a space below previous particle
                theta += (float)((Math.PI / 2 / amount) + rnd.NextDouble()); // Modifies angle at which the magnitude of the velocity acts in
                index++;
            }
        }
    }
    #endregion

    #region Removing Particles
    private void RemoveSmallParticles_Click(object sender, EventArgs e)
    {
        RemoveParticles(10, ref smallParticles, ref indexSmall, ParticleType.Small);
    }

    private void RemoveLargeParticles_Click(object sender, EventArgs e)
    {
        RemoveParticles(10, ref largeParticles, ref indexLarge, ParticleType.Large);
    }

    private void RemoveParticles(int amount, ref Polygon[] particleType, ref int typeIndex, ParticleType type)
    {
        if (typeIndex - amount >= 0)
        {
            for (var i = 0; i < amount; i++)
            {
                typeIndex--;
                activeParticles.RemoveAt(GetIndex(type, typeIndex));
                particleType[typeIndex].Enabled = false;
            }
        }
    }
    #endregion

    private void PauseSimulation_Click(object sender, EventArgs e)
    {
        paused = !paused;
    }

    private void ResetSimulation_Click(object sender, EventArgs e)
    {
        temperature = 300; // Room temperature
        pressure = 0;
        volume = maxVolume;
        rmsVelocity = PhysicsEquations.CalcVRMS(temperature, avgMass);

        simulationBox.SetVolume(InverseScale(volume));
        volumeSlider.sliderButton.SetPosition(new Vector2(InverseScale(volume), volumeSlider.sliderButton.Position.Y));

        RemoveAllParticles();
    }

    private void RemoveAllParticles()
    {
        // Set all small particles to disabled
        for (var i = 0; i < indexSmall; i++)
        {
            smallParticles[i].Enabled = false;
        }
        for (var i = 0; i < indexLarge; i++)
        {
            largeParticles[i].Enabled = false;
        }
        activeParticles.Clear();
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
        if (PhysicsEquations.CalcVolume(pressure, NumParticles, temperature + 10) <= maxVolume && !constants.Volume)
        {
            temperature += 10;
            if (!constants.PressureTemp && !constants.PressureVol)
            {
                rmsVelocity = PhysicsEquations.CalcVRMS(temperature, avgMass);
                ChangeVelocities(rmsVelocity);
            }
        }

        if (constants.Volume)
        {
            if (temperature + 10 < 450)
            {
                temperature += 10;
                if (!constants.PressureTemp && !constants.PressureVol)
                {
                    rmsVelocity = PhysicsEquations.CalcVRMS(temperature, avgMass);
                    ChangeVelocities(rmsVelocity);
                }
            }
        }
    }

    private void DecreaseTemperature_Click(object sender, EventArgs e)
    {
        if (PhysicsEquations.CalcVolume(pressure, NumParticles, temperature - 10) >= minVolume && !constants.Volume)
        {
            temperature -= 10;
            if (!constants.PressureTemp && !constants.PressureVol)
            {
                var newRmsVelocity = PhysicsEquations.CalcVRMS(temperature, avgMass);
                ChangeVelocities(newRmsVelocity - rmsVelocity);
                rmsVelocity = newRmsVelocity;
            }
        }

        if (constants.Volume)
        {
            if (temperature - 10 >= 0)
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
    }

    private void ChangeVelocities(double amount)
    {
        for (var i = 0; i < activeParticles.Count; i++)
        {
            float newLength = (float)(activeParticles[i].CurrentVelocity.Length() + amount);
            activeParticles[i].SetVelocity(newLength / activeParticles[i].CurrentVelocity.Length() * activeParticles[i].CurrentVelocity);
        }
    }
    #endregion
    #endregion
    #endregion
}