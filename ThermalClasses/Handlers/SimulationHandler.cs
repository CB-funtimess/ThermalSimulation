using System.Runtime.InteropServices;
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
    private Queue<ParticleType> addParticlesQueue;
    private double timeSinceDequeue;
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
    private Button add50Small, add50Large, remove50Small, remove50Large;
    private UpDownButton temperatureControl;
    private Slider volumeSlider;
    private Label dataBox, volumeDisp, temperatureDisp, pressureDisp, numParticlesDisp, constantLabel;
    private RadioButtons keepConstant;
    private CollisionCounter counter;
    private NumInput temperatureInput;
    #endregion
    #endregion
    private PhysicalConstants constants = new PhysicalConstants();
    private double volume, maxVolume, changeInVolume, minVolume; // Measured in metres cubed
    private double pressure, rmsVelocity, avgMass; // Measured in Kelvin, Pascals, metres per second, kilograms
    private double temperature, minTemperature, maxTemperature; // Measured in Kelvin
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
        minTemperature = 0;
        maxTemperature = 600;
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
        return new Polygon(content.Load<Texture2D>("SimulationAssets/YellowParticle"), new Vector2(0, 0), new Vector2(0, 0), avgMass - (0.001 * avgMass), 15, Color.White, new Point(5, 5)) // avgMass - (0.001*avgMass)
        {
            Enabled = false,
            Type = ParticleType.Small,
            Identifier = identifier,
        };
    }

    private Polygon NewLargeCircle(int identifier)
    {
        return new(content.Load<Texture2D>("SimulationAssets/BlueParticle"), new Vector2(0, 0), new Vector2(0, 0), avgMass + (0.001 * avgMass), 15, Color.White, new Point(10, 10)) // avgMass + (0.001*avgMass)
        {
            Enabled = false,
            Type = ParticleType.Large,
            Identifier = identifier,
        };
    }
    #endregion

    #region Game Object Initialisation
    private void InitialiseSimBox()
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
        addParticlesQueue = new Queue<ParticleType>();
        timeSinceDequeue = 0;
    }

    public override void LoadContent()
    {
        SpriteFont font = content.Load<SpriteFont>("GeneralAssets/Arial");
        Texture2D upTexture = content.Load<Texture2D>("GeneralAssets/UpButton");
        Texture2D downTexture = content.Load<Texture2D>("GeneralAssets/DownButton");
        Texture2D upDownLabelTexture = content.Load<Texture2D>("GeneralAssets/LabelBox1");
        Texture2D sliderLabelTexture = content.Load<Texture2D>("GeneralAssets/SliderFrame2");
        Texture2D sliderButtonTexture = content.Load<Texture2D>("GeneralAssets/SliderButton_Unclicked");
        Texture2D sliderButtonHoverTexture = content.Load<Texture2D>("GeneralAssets/SliderButton_Clicked");
        Texture2D outputBox = content.Load<Texture2D>("GeneralAssets/DataBox");
        Texture2D buttonUnchecked = content.Load<Texture2D>("GeneralAssets/Button_Unchecked");
        Texture2D buttonChecked = content.Load<Texture2D>("GeneralAssets/Button_Checked");
        Texture2D pauseTexture = content.Load<Texture2D>("GeneralAssets/PauseButton");
        Texture2D playTexture = content.Load<Texture2D>("GeneralAssets/PlayButton");
        Texture2D resetTexture = content.Load<Texture2D>("GeneralAssets/ResetButton");
        Texture2D textInputTexture = content.Load<Texture2D>("GeneralAssets/TextInputBox");
        Texture2D upTextureBox = content.Load<Texture2D>("GeneralAssets/UpButton_Box");
        Texture2D downTextureBox = content.Load<Texture2D>("GeneralAssets/DownButton_Box");

        Color unclickedColour = Color.White;
        // Particle Initialisation
        InitialiseParticles();

        // GameObject Initialisation
        InitialiseSimBox();
        // Buttons Initialisation
        Point pauseSize = new Point(40, 40);
        Vector2 pausePosition = new Vector2(pauseSize.X / 2, simulationBox.BoxRect.Y - 35);
        pauseButton = new CheckButton(pauseTexture, playTexture, font, pausePosition, unclickedColour, penColour, pauseSize)
        {
            HoverColour = HoverColour
        };
        pauseButton.Click += PauseSimulation_Click;
        Vector2 resetPos = new Vector2(pausePosition.X + pauseSize.X + 5, pausePosition.Y);
        resetButton = new Button(resetTexture, font, resetPos, unclickedColour, penColour, pauseSize)
        {
            HoverColour = HoverColour
        };
        resetButton.Click += ResetSimulation_Click;

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
            Text = $"Number of moles: {PhysicsEquations.NumberToMoles(NumParticles, 25)}mol"
        };

        Point radioSize = new Point(230, 120);
        Rectangle radioRect = new Rectangle(new Point(resultsBox.X + ((resultsBox.Width / 2) - (radioSize.X / 2)), numParticlesRect.Y + 70), radioSize);
        string[] text = { "Volume", "Temperature", "Pressure => Volume", "Pressure => Temperature" };
        Vector2 startButton = new Vector2(radioRect.X + 20, radioRect.Y + 20);
        keepConstant = new RadioButtons(buttonUnchecked, buttonChecked, upDownLabelTexture, outputBox, radioRect, startButton, text, font, unclickedColour, HoverColour, penColour, 1);
        Point constSize = new Point(radioSize.X, 20);
        Rectangle constRect = new Rectangle(new Point(radioRect.X, radioRect.Y - 20), constSize);
        constantLabel = new Label(upDownLabelTexture, unclickedColour, constRect, font, penColour)
        {
            Text = "Choose what to keep constant:"
        };

        // Particle controls
        Point add50Size = new Point(40,40);
        Point particleControlSize = new Point(200, 40);
        Rectangle largeParticleButtonRect = new Rectangle(new Point(simulationBox.BoxRect.Right - particleControlSize.X - add50Size.X, simulationBox.BoxRect.Bottom + 15), particleControlSize);
        Rectangle smallParticleButtonRect = new Rectangle(new Point(largeParticleButtonRect.X, largeParticleButtonRect.Y + particleControlSize.Y + 10), particleControlSize);
        smallParticleControl = new UpDownButton(upTextureBox, downTextureBox, upDownLabelTexture, smallParticleButtonRect, "Small Particles", font, penColour, unclickedColour, HoverColour);
        smallParticleControl.DownButton.Click += RemoveSmallParticles_Click;
        smallParticleControl.UpButton.Click += AddSmallParticles_Click;

        largeParticleControl = new UpDownButton(upTextureBox, downTextureBox, upDownLabelTexture, largeParticleButtonRect, "Large Particles", font, penColour, unclickedColour, HoverColour);
        largeParticleControl.DownButton.Click += RemoveLargeParticles_Click;
        largeParticleControl.UpButton.Click += AddLargeParticles_Click;

        Rectangle smallAdd50 = new Rectangle(new Point(smallParticleButtonRect.Right, smallParticleButtonRect.Y), add50Size);
        Rectangle smallRemove50 = new Rectangle(new Point(smallParticleButtonRect.X - add50Size.X, smallParticleButtonRect.Y), add50Size);
        Rectangle largeAdd50 = new Rectangle(new Point(largeParticleButtonRect.Right, largeParticleButtonRect.Y), add50Size);
        Rectangle largeRemove50 = new Rectangle(new Point(largeParticleButtonRect.X - add50Size.X, largeParticleButtonRect.Y), add50Size);
        add50Small = new Button(upTexture, font, smallAdd50, unclickedColour, penColour)
        {
            HoverColour = HoverColour,
        };
        add50Small.Click += Add50SmallParticles_Click;
        remove50Small = new Button(downTexture, font, smallRemove50, unclickedColour, penColour)
        {
            HoverColour = HoverColour,
        };
        remove50Small.Click += Remove50Small_Click;
        add50Large = new Button(upTexture, font, largeAdd50, unclickedColour, penColour)
        {
            HoverColour = HoverColour,
        };
        add50Large.Click += Add50LargeParticles_Click;
        remove50Large = new Button(downTexture, font, largeRemove50, unclickedColour, penColour)
        {
            HoverColour = HoverColour,
        };
        remove50Large.Click += Remove50Large_Click;

        // Temperature controls
        Rectangle temperatureButtonRect = new Rectangle(new Point(simulationBox.BoxRect.Right - particleControlSize.X, simulationBox.BoxRect.Top - 15 - particleControlSize.Y), particleControlSize);
        temperatureControl = new UpDownButton(upTexture, downTexture, upDownLabelTexture, temperatureButtonRect, "Temperature", font, penColour, unclickedColour, HoverColour);
        temperatureControl.DownButton.Click += DecreaseTemperature_Click;
        temperatureControl.UpButton.Click += IncreaseTemperature_Click;

        Rectangle temperatureInputRect = new Rectangle(new Point(temperatureButtonRect.X - particleControlSize.X, temperatureButtonRect.Y), particleControlSize);
        temperatureInput = new NumInput(textInputTexture, temperatureInputRect, font, unclickedColour, penColour, "Enter a temperature:")
        {
            HoverColour = HoverColour
        };
        temperatureInput.Enter += ChangeTemperatureText_Enter;

        // Volume controls
        Rectangle volumeSliderRect = new Rectangle(new Point(simulationBox.BoxRect.Left, simulationBox.BoxRect.Bottom + 15), new Point((int)(renderRectangle.Width * 0.55) - 10, 20));
        Point volumeFixedTextureSize = new Point((int)(renderRectangle.Width * 0.55) - volumeSliderRect.Height - 10, 4);
        Rectangle volumeFixedTextureRect = new Rectangle(new Point(simulationBox.BoxRect.Left + (volumeSliderRect.Height / 2), simulationBox.BoxRect.Bottom + 15 + 8), volumeFixedTextureSize);
        volumeSlider = new Slider(sliderButtonTexture, sliderButtonHoverTexture, sliderLabelTexture, font, volumeSliderRect, volumeFixedTextureRect, 1, unclickedColour, penColour);
        volumeSlider.sliderButton.Click += ChangeVolume_Click;
        volumeSlider.sliderButton.Click.Invoke(new object(), EventArgs.Empty);

        // Collision counter
        Rectangle counterPos = new Rectangle(new Point(300, 0), new Point(126, 100));
        counter = new CollisionCounter(counterPos, upDownLabelTexture, upDownLabelTexture, pauseTexture, playTexture, resetTexture, unclickedColour, HoverColour, penColour, font);

        // Putting most objects into a list for easier updating and drawing (some must be updated manually)
        buttonCollection.Add(pauseButton);
        buttonCollection.Add(resetButton);
        buttonCollection.Add(remove50Small);
        buttonCollection.Add(remove50Large);
        buttonCollection.Add(add50Small);
        buttonCollection.Add(add50Large);

        upDownCollection.Add(smallParticleControl);
        upDownCollection.Add(largeParticleControl);

        labelCollection.Add(dataBox);
        labelCollection.Add(constantLabel);
        labelCollection.Add(volumeDisp);
        labelCollection.Add(temperatureDisp);
        labelCollection.Add(pressureDisp);
        labelCollection.Add(numParticlesDisp);
    }
    #endregion

    #region Updating & Drawing
    // Calls the update method of all objects that need updating (buttons, particles, sliders etc.)
    public override void Update(GameTime gameTime)
    {
        constants.ChangeParticles = false;

        timeSinceDequeue += gameTime.ElapsedGameTime.TotalSeconds;
        // Adding particles according to the queue and the time since last dequeue
        if (addParticlesQueue.Count > 0)
        {
            if (timeSinceDequeue > 1) // If the last dequeue was greater than 1 second ago, add particles
            {
                ParticleType typeToAdd = addParticlesQueue.Dequeue();
                if (typeToAdd == ParticleType.Small)
                {
                    AddParticles(10, ref smallParticles, ref indexSmall);
                }
                else
                {
                    AddParticles(10, ref largeParticles, ref indexLarge);
                }
                timeSinceDequeue = 0;
            }
        }

        pauseButton.Update(gameTime);
        keepConstant.Update(gameTime);
        counter.Update(gameTime);
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
            if ((NumParticles > 0 && constants.PressureTemp) || !constants.PressureTemp)
            {
                volumeSlider.Update(gameTime);
            }
        }
        if (!constants.Temperature && !constants.PressureTemp)
        {
            temperatureControl.Update(gameTime);
            temperatureInput.Update(gameTime);
        }
        if (!paused)
        {
            UpdateParticles(gameTime);
        }

        SetProperties();
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
                Polygon myParticle = CollisionFunctions.BoundaryCollisionHandling(particle, simulationBox.BoxRect);

                // Teleporting all particles outside of the current border back in to the box
                // All particles outside should just be outside the x region of the box
                if (myParticle.Position.X + myParticle.XRadius < simulationBox.BoxRect.Left)
                {
                    myParticle.ChangePositionBy(new Vector2(22, 0));
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
            if (timeOfCollision <= gameTime.ElapsedGameTime.TotalSeconds)
            {
                activeParticles[i1].SetPosition(activeParticles[i1].PreviousPosition + (activeParticles[i1].CurrentVelocity * (float)timeOfCollision));
                activeParticles[i2].SetPosition(activeParticles[i2].PreviousPosition + (activeParticles[i2].CurrentVelocity * (float)timeOfCollision));
            }

            Vector2[] newVelocities = CollisionFunctions.NewCollisionVelocity(activeParticles[i1], activeParticles[i2]);
            activeParticles[i1].SetVelocity(newVelocities[0]);
            activeParticles[i2].SetVelocity(newVelocities[1]);
            if (counter.IsCounting)
            {
                counter.NoCollisions++;
            }
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

    /// <summary>
    /// Sets the properties of the simulation.
    /// </summary>
    private void SetProperties()
    {
        // ADDING PARTICLES SHOULD ONLY CHANGE THE PRESSURE
        if ((constants.PressureTemp || constants.PressureVol) && NumParticles > 0 && !constants.ChangeParticles) // Pressure constant so other variables evaluated
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

        // Updating the text of the display labels
        volumeDisp.Text = $"Volume: {Convert.ToInt32(volume)} metres cubed";
        temperatureDisp.Text = $"Temperature: {Convert.ToInt32(temperature)}K / {Convert.ToInt32(temperature - 273)} degrees C";
        pressureDisp.Text = $"Pressure: {pressure.ToString("e2", CultureInfo.InvariantCulture)}Pa";
        numParticlesDisp.Text = $"Number of moles: {PhysicsEquations.NumberToMoles(NumParticles, 25).ToString("e2", CultureInfo.InvariantCulture)}mol";
    }
    #endregion

    // Calls the draw methods of all GameObjects
    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        simulationBox.Draw(_spriteBatch);
        temperatureControl.Draw(_spriteBatch);
        temperatureInput.Draw(_spriteBatch);
        volumeSlider.Draw(_spriteBatch);
        keepConstant.Draw(_spriteBatch);
        counter.Draw(_spriteBatch);
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
        foreach (var label in labelCollection)
        {
            label.PenColour = colour;
        }
        counter.ChangePenColour(colour);
        temperatureControl.ChangePenColour(colour);
        volumeDisp.PenColour = temperatureDisp.PenColour = pressureDisp.PenColour = numParticlesDisp.PenColour = colour;
        keepConstant.ChangePenColour(colour);
        temperatureInput.PenColour = colour;
    }
    #endregion

    #region Events
    #region Adding Particles
    private void AddSmallParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            addParticlesQueue.Enqueue(ParticleType.Small);
        }
    }

    private void Add50SmallParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            for (var i = 0; i < 5; i++)
            {
                addParticlesQueue.Enqueue(ParticleType.Small);
            }
        }
    }

    private void AddLargeParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            addParticlesQueue.Enqueue(ParticleType.Large);
        }
    }

    private void Add50LargeParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            for (var i = 0; i < 5; i++)
            {
                addParticlesQueue.Enqueue(ParticleType.Large);
            }
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
                particleType[index].SetPreviousPosition(new Vector2(simulationBox.BoxRect.Right, insertPosition.Y));
                particleType[index].SetVelocity(insertionVelocity);
                activeParticles.Add(particleType[index]);
                insertPosition.Y += particleType[0].YRadius * 4; // Inserts next particle into a space below previous particle
                theta += (float)((Math.PI / 2 / amount) + rnd.NextDouble()); // Modifies angle at which the magnitude of the velocity acts in
                index++;
            }
        }
        constants.ChangeParticles = true;
    }
    #endregion

    #region Removing Particles
    private void RemoveSmallParticles_Click(object sender, EventArgs e)
    {
        RemoveParticles(10, ref smallParticles, ref indexSmall, ParticleType.Small);
    }

    private void Remove50Small_Click(object sender, EventArgs e)
    {
        RemoveParticles(50, ref smallParticles, ref indexSmall, ParticleType.Small);
    }

    private void RemoveLargeParticles_Click(object sender, EventArgs e)
    {
        RemoveParticles(10, ref largeParticles, ref indexLarge, ParticleType.Large);
    }

    private void Remove50Large_Click(object sender, EventArgs e)
    {
        RemoveParticles(50, ref largeParticles, ref indexLarge, ParticleType.Large);
    }

    private void RemoveParticles(int amount, ref Polygon[] particleType, ref int index, ParticleType type)
    {
        if (index - amount >= 0)
        {
            for (var i = 0; i < amount; i++)
            {
                index--;
                activeParticles.RemoveAt(GetIndex(type, index));
                particleType[index].Enabled = false;
            }
        }
        constants.ChangeParticles = true;
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
        if ((NumParticles > 0 && constants.PressureTemp) || !constants.PressureTemp)
        {
            simulationBox.SetVolume(volumeSlider.sliderButton.Position.X);
            volume = ScaleVolume(volumeSlider.sliderButton.Position.X);
        }
    }

    /// <summary>
    /// Scales the value of the volume according to the position of the mouse
    /// </summary>
    /// <param name="x">Mouse x coordinate</param>
    /// <returns>The value of the volume</returns>
    private double ScaleVolume(float x)
    {
        float maxX = volumeSlider.MaxX;
        float minX = volumeSlider.MinX;
        return (changeInVolume / (minX - maxX) * (x - minX)) + maxVolume;
    }

    /// <summary>
    /// Finds the position of the slider according to the inputted volume
    /// </summary>
    /// <param name="v">Volume</param>
    /// <returns>The x position of the slider</returns>
    private float InverseScale(double v)
    {
        float maxX = volumeSlider.MaxX;
        float minX = volumeSlider.MinX;
        return (float)(((v - maxVolume) / (changeInVolume / (minX - maxX))) + minX);
    }
    #endregion

    #region Changing Temperature
    private void IncreaseTemperature_Click(object sender, EventArgs e)
    {
        if ((!constants.Volume && PhysicsEquations.CalcVolume(pressure, NumParticles, temperature + 10) <= maxVolume) || (constants.Volume && temperature + 10 <= maxTemperature))
        {
            temperature += 10;
            if (!constants.PressureTemp && !constants.PressureVol)
            {
                ChangeVRMS();
            }
        }
    }

    private void DecreaseTemperature_Click(object sender, EventArgs e)
    {
        if ((!constants.Volume && PhysicsEquations.CalcVolume(pressure, NumParticles, temperature - 10) >= minVolume) || (constants.Volume && temperature - 10 >= minTemperature))
        {
            temperature -= 10;
            if (!constants.PressureTemp && !constants.PressureVol)
            {
                ChangeVRMS();
            }
        }
    }

    private void ChangeVRMS()
    {
        var newRmsVelocity = PhysicsEquations.CalcVRMS(temperature, avgMass);
        ChangeVelocities(newRmsVelocity - rmsVelocity);
        rmsVelocity = newRmsVelocity;
    }

    private void ChangeVelocities(double amount)
    {
        for (var i = 0; i < activeParticles.Count; i++)
        {
            float newLength = (float)(activeParticles[i].CurrentVelocity.Length() + amount);
            activeParticles[i].SetVelocity(newLength / activeParticles[i].CurrentVelocity.Length() * activeParticles[i].CurrentVelocity);
        }
    }

    private void ChangeTemperatureText_Enter(object sender, EventArgs e)
    {
        double newTemperature;
        try
        {
            newTemperature = Convert.ToDouble(temperatureInput.Text);
        }
        catch (FormatException)
        {
            newTemperature = temperature;
        }

        if (newTemperature <= maxTemperature && newTemperature >= minTemperature)
        {
            if ((!constants.Volume && PhysicsEquations.CalcVolume(pressure, NumParticles, newTemperature) >= minVolume && PhysicsEquations.CalcVolume(pressure, NumParticles, newTemperature) <= maxVolume) || constants.Volume)
            {
                temperature = newTemperature;
                ChangeVRMS();
            }
        }
    }
    #endregion
    #endregion
    #endregion
}