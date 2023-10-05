using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System;



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
    const int ORC_WIDTH = 280;
    const int ORC_HEIGHT = 180;
    Random random = new Random();

    // Arrows
    List<Vector2> arrows;
    float arrowSpeed = 500;
    float timeSinceLastArrow = 0f;

    // SpritesList
    List<AnimatedSprite> explosionSprites = new List<AnimatedSprite>();
    List<AnimatedSprite> dyingOrcSprites = new List<AnimatedSprite>();

    // Animations
    Animation ExplosionAnimation;
    Animation orcWalkingAnimation;
    Animation orcDieAnimation;

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

        LoadExplosionAnimation();
        LoadOrcWalkingAnimation();
        LoadOrcDieAnimation();
    }

    private void LoadExplosionAnimation()
    {
        string path = "Content/Textures/Animation/Explosion"; // Modified path
        string[] files = Directory.GetFiles(path, "*");

        Texture2D[] ExplosionFrames = new Texture2D[files.Length];
        for (int i = 1; i < files.Length; i++)
        {
            ExplosionFrames[i] = Content.Load<Texture2D>($"Textures/Animation/Explosion/Explosion_{i}");
        }

        ExplosionAnimation = new Animation(0.1f, ExplosionFrames);
    }

    private void LoadOrcWalkingAnimation()
    {
        string path = "Content/Textures/Animation/NPC/Warriors_Orc/OrcWalk"; // Modified path
        string[] files = Directory.GetFiles(path, "*");

        Texture2D[] orcWalkingFrames = new Texture2D[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            orcWalkingFrames[i] = Content.Load<Texture2D>($"Textures/Animation/NPC/Warriors_Orc/OrcWalk/ORK_01_WALK_00{i}"); // Modified file loading logic
        }
        orcWalkingAnimation = new Animation(0.1f, orcWalkingFrames);
    }

    private void LoadOrcDieAnimation()
    {
        string path = "Content/Textures/Animation/NPC/Warriors_Orc/OrcDie";
        string[] files = Directory.GetFiles(path, "*");

        Texture2D[] orcDieFrames = new Texture2D[files.Length];
        for (int i = 0; i < files.Length - 1; i++)
        {
            orcDieFrames[i] = Content.Load<Texture2D>($"Textures/Animation/NPC/Warriors_Orc/OrcDie/ORK_01_DIE_00{i}");
        }
        orcDieAnimation = new Animation(0.1f, orcDieFrames);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        playerPosition.Y = Math.Clamp(playerPosition.Y, MAP_TOP_BOUNDARY, MAP_BOTTOM_BOUNDARY - PLAYER_HEIGHT);

        PlayerMovement(keyboardState);

        ShootArrow(gameTime, keyboardState);

        ClearPastExplosions(gameTime);

        CheckCollisionArrowOrcs();

        OrcsSpawning(gameTime);

        UpdateOrcs(gameTime);

        UpdateArrows(gameTime);

        base.Update(gameTime);
    }

    // Update Methods
    private void PlayerMovement(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Up)) playerPosition.Y -= PLAYER_SPEED;
        if (keyboardState.IsKeyDown(Keys.Down)) playerPosition.Y += PLAYER_SPEED;
        if (keyboardState.IsKeyDown(Keys.Left)) playerPosition.X -= PLAYER_SPEED;
        if (keyboardState.IsKeyDown(Keys.Right)) playerPosition.X += PLAYER_SPEED;
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
        DrawArrows();

        // Draw orcs
        DrawOrcs();

        // Draw Explosions
        DrawExplosions();

        spriteBatch.End();
        base.Draw(gameTime);
    }



    private void ClearPastExplosions(GameTime gameTime)
    {
        for (int i = 0; i < explosionSprites.Count; i++)
        {
            var explosion = explosionSprites[i];
            explosion.Update(gameTime);

            if (explosion.GetCurrentFrame() == ExplosionAnimation.Frames[ExplosionAnimation.Frames.Length - 1])
            {
                explosionSprites.RemoveAt(i);
                i--;
            }
        }
    }

    private void ShootArrow(GameTime gameTime, KeyboardState keyboardState)
    {
        timeSinceLastArrow += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (keyboardState.IsKeyDown(Keys.Space) && timeSinceLastArrow >= 1)
        {
            arrows.Add(new Vector2(playerPosition.X + PLAYER_WIDTH, playerPosition.Y + PLAYER_HEIGHT / 2));
            timeSinceLastArrow = 0f;
        }
    }

    private void CheckCollisionArrowOrcs()
    {
        for (int i = 0; i < orcs.Count; i++)
        {
            var orc = orcs[i];
            if (orc.IsDying) continue; // Skip if the orc is already dying
            Rectangle orcRect = new Rectangle((int)orc.Position.X, (int)orc.Position.Y, orc.Width, ORC_HEIGHT);

            for (int j = 0; j < arrows.Count; j++)
            {
                var arrow = arrows[j];
                Rectangle arrowRect = new Rectangle((int)arrow.X, (int)arrow.Y, ARROW_WIDTH, ARROW_HEIGHT);

                if (orcRect.Intersects(arrowRect))
                {
                    // Suppression de l'orc et de la flèche en cas de collision
                    orc.Die();
                    arrows.RemoveAt(j);

                    // Ajustez les index pour éviter de sauter un élément après suppression
                    i--;
                    j--;
                    break; // sortez de la boucle des flèches pour cet orc
                }
            }
        }
    }

    private void OrcsSpawning(GameTime gameTime)
    {
        spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (spawnTimer >= SPAWN_INTERVAL)
        {
            spawnTimer = 0;
            float randomYPosition = MAP_TOP_BOUNDARY +
                                    (float)random.NextDouble() * (MAP_BOTTOM_BOUNDARY - MAP_TOP_BOUNDARY - ORC_HEIGHT); // Used ORC_HEIGHT
            var newOrc = new Orc(new Vector2(1280, randomYPosition), ORC_WIDTH, ORC_HEIGHT, orcWalkingAnimation);
            orcs.Add(new Orc(new Vector2(1280, randomYPosition), ORC_WIDTH, ORC_HEIGHT, orcWalkingAnimation)); // Passed ORC_HEIGHT
        }
    }

    private void UpdateOrcs(GameTime gameTime)
    {
        for (int i = 0; i < orcs.Count; i++)
        {
            orcs[i].Update(gameTime);

            // Check if orc's death animation has finished
            if (orcs[i].IsDying && orcs[i].DieAnimation.GetCurrentFrame() == orcDieAnimation.Frames[orcDieAnimation.Frames.Length - 1])
            {
                orcs.RemoveAt(i);
                i--;
            }
            else if (orcs[i].Position.X + PLAYER_WIDTH < 0) // if the orc is completely off the left side of the screen
            {
                orcs.RemoveAt(i);
                i--;
            }
        }
    }

    private void UpdateArrows(GameTime gameTime)
    {
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
    }

    // Draw Methods
    private void DrawArrows()
    {
        foreach (var arrow in arrows)
        {
            spriteBatch.Draw(arrowTexture, new Rectangle((int)arrow.X, (int)arrow.Y, ARROW_WIDTH, ARROW_HEIGHT),
                Color.White);
        }
    }

    private void DrawOrcs()
    {
        foreach (var orc in orcs)
        {
            Rectangle destRect = new Rectangle((int)orc.Position.X, (int)orc.Position.Y, orc.Width, orc.Height);
            var frame = orc.WalkingAnimation.GetCurrentFrame(); // This will fetch the correct frame, whether walking or dying
            if (frame != null)
            {
                spriteBatch.Draw(frame, destRect, frame.Bounds, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            }
        }
    }



    private void DrawExplosions()
    {
        foreach (var explosion in dyingOrcSprites)
        {
            var frame = explosion.GetCurrentFrame();
            if (frame != null)
            {
                spriteBatch.Draw(frame,
                    new Rectangle((int)explosion.Position.X, (int)explosion.Position.Y, PLAYER_WIDTH, PLAYER_HEIGHT),
                    Color.White);
            }
        }
    }

    public class Orc
    {
        public Vector2 Position { get; set; }
        public float Speed { get; set; } = 2.0f;
        public int Width { get; set; }
        public int Height { get; set; }  // Added Height property
        public AnimatedSprite WalkingAnimation { get; private set; } // Made it a public property
        public AnimatedSprite DieAnimation { get; set; }
        public bool IsDying { get; set; }


        public Orc(Vector2 startPosition, int width, int height, Animation animation)
        {
            Position = startPosition;
            Width = width;
            Height = height;  // Set Height
            WalkingAnimation = new AnimatedSprite(startPosition);
            WalkingAnimation.Animations.Add("Walking", animation);
            WalkingAnimation.PlayAnimation("Walking");
        }

        public void Update(GameTime gameTime)
        {
            Position = new Vector2(Position.X - Speed, Position.Y);
            WalkingAnimation.Position = Position;
            WalkingAnimation.Update(gameTime);
        }

        public void Die(Vector2 startPosition, int width, int height, Animation animation)
        {
            this.IsDying = true;
            Position = startPosition;
            Width = width;
            Height = height;  // Set Height
            DieAnimation = new AnimatedSprite(startPosition);
            DieAnimation.Animations.Add("Walking", animation);
            DieAnimation.PlayAnimation("OrcDie");
        }
    }

    public class Animation
    {
        public Texture2D[] Frames { get; private set; }
        public float FrameDuration { get; private set; } // temps pour un frame, par exemple 0.1f pour 10 images par seconde

        private float elapsedTime;
        private int currentFrame;

        public Animation(float frameDuration, params Texture2D[] frames)
        {
            Frames = frames;
            FrameDuration = frameDuration;
            elapsedTime = 0;
            currentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (elapsedTime > FrameDuration)
            {
                elapsedTime -= FrameDuration;
                currentFrame = (currentFrame + 1) % Frames.Length;
            }
        }

        public Texture2D GetCurrentFrame()
        {
            Console.WriteLine($"currentFrame: {currentFrame}, Frames.Length: {Frames.Length}");
            return Frames[currentFrame];
        }

        public void Reset()
        {
            currentFrame = 0;
            elapsedTime = 0;
        }
    }

    public class AnimatedSprite
    {
        public Vector2 Position { get; set; }
        public Dictionary<string, Animation> Animations { get; } = new Dictionary<string, Animation>();

        public string currentAnimationKey;

        public AnimatedSprite(Vector2 position)
        {
            Position = position;
        }

        public void PlayAnimation(string animationKey)
        {
            if (!Animations.ContainsKey(animationKey))
                throw new ArgumentException("Invalid animation key");

            if (currentAnimationKey != animationKey)
            {
                Animations[animationKey].Reset();
                currentAnimationKey = animationKey;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (currentAnimationKey != null)
            {
                Animations[currentAnimationKey].Update(gameTime);
            }
        }

        public Texture2D GetCurrentFrame()
        {
            if (currentAnimationKey != null)
            {
                return Animations[currentAnimationKey].GetCurrentFrame();
            }
            return null;
        }
    }
}