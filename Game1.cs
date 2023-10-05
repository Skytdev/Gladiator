using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

public class Game1 : Game
{
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    // Textures
    Texture2D playerTexture, arrowTexture, arenaTexture, warriorTexture;

    // Player
    Vector2 playerPosition;
    const float PLAYER_SPEED = 4f;
    const int ARROW_WIDTH = 30;
    const int ARROW_HEIGHT = 10;

    const int PLAYER_WIDTH = 180;
    const int PLAYER_HEIGHT = 100;
    const float MAP_TOP_BOUNDARY = 205;
    const float MAP_BOTTOM_BOUNDARY = 605;

    // Orcs List
    List<Orc> orcs = new List<Orc>();
    float spawnTimer = 0;
    const float SPAWN_INTERVAL = 4.0f;
    const float ORC_SPEED = 3f;
    const int ORC_WIDTH = 180;
    const int ORC_HEIGHT = 100;
    Random random = new Random();

    // Arrows
    List<Vector2> arrows;
    float arrowSpeed = 1280;
    float timeSinceLastArrow = 0f;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        base.Initialize();
        playerPosition = new Vector2(50, 360);
        arrows = new List<Vector2>();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        playerTexture = Content.Load<Texture2D>("Textures/player");
        arrowTexture = Content.Load<Texture2D>("Textures/arrow");
        arenaTexture = Content.Load<Texture2D>("Textures/arena");
        warriorTexture = Content.Load<Texture2D>("Textures/warrior");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        playerPosition.Y = Math.Clamp(playerPosition.Y, MAP_TOP_BOUNDARY, MAP_BOTTOM_BOUNDARY - PLAYER_HEIGHT);

        // Player Movement
        if (keyboardState.IsKeyDown(Keys.Up)) playerPosition.Y -= PLAYER_SPEED;
        if (keyboardState.IsKeyDown(Keys.Down)) playerPosition.Y += PLAYER_SPEED;
        if (keyboardState.IsKeyDown(Keys.Left)) playerPosition.X -= PLAYER_SPEED;
        if (keyboardState.IsKeyDown(Keys.Right)) playerPosition.X += PLAYER_SPEED;

        // Arrow shooting
        timeSinceLastArrow += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (keyboardState.IsKeyDown(Keys.Space) && timeSinceLastArrow >= 1)
        {
            arrows.Add(new Vector2(playerPosition.X + PLAYER_WIDTH, playerPosition.Y + PLAYER_HEIGHT / 2));
            timeSinceLastArrow = 0f;
        }

        // Orc spawning
        spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (spawnTimer >= SPAWN_INTERVAL)
        {
            spawnTimer = 0;
            float randomYPosition = MAP_TOP_BOUNDARY + (float)random.NextDouble() * (MAP_BOTTOM_BOUNDARY - MAP_TOP_BOUNDARY - PLAYER_HEIGHT);
            System.Diagnostics.Debug.WriteLine("Orc spawned at position: " + randomYPosition);
            orcs.Add(new Orc(new Vector2(1280, randomYPosition), ORC_WIDTH));
        }

        // Update Orcs
        for (int i = 0; i < orcs.Count; i++)
        {
            orcs[i].Update(gameTime);
            if (orcs[i].Position.X + PLAYER_WIDTH < 0) // if the orc is completely off the left side of the screen
            {
                orcs.RemoveAt(i);
                i--;
            }
        }

        // Update arrows
        for (int i = 0; i < arrows.Count; i++)
        {
            var arrow = arrows[i];
            arrow.X += arrowSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (arrow.X > 1280)
            {
                arrows.RemoveAt(i);
                i--;
            }
            else
            {
                arrows[i] = arrow;
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();

        // Draw arena
        spriteBatch.Draw(arenaTexture, new Rectangle(0, 0, 1280, 720), Color.White);

        // Draw player
        spriteBatch.Draw(playerTexture, new Rectangle((int)playerPosition.X, (int)playerPosition.Y, PLAYER_WIDTH, PLAYER_HEIGHT), Color.White);

        // Draw arrows
        foreach (var arrow in arrows)
        {
            spriteBatch.Draw(arrowTexture, new Rectangle((int)arrow.X, (int)arrow.Y, ARROW_WIDTH, ARROW_HEIGHT), Color.White);
        }

        // Draw orcs
        foreach (var orc in orcs)
        {
            Rectangle destRect = new Rectangle((int)orc.Position.X, (int)orc.Position.Y, orc.Width, 150);
            spriteBatch.Draw(warriorTexture, destRect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
        }


        spriteBatch.End();
        base.Draw(gameTime);
    }
}

public class Orc
{
    public Vector2 Position { get; set; }
    public float Speed { get; set; } = 2.0f;
    public int Width { get; set; }

    public Orc(Vector2 startPosition, int width)
    {
        Position = startPosition;
        Width = width;
    }

    public void Update(GameTime gameTime)
    {
        Position = new Vector2(Position.X - Speed, Position.Y);
    }
}
