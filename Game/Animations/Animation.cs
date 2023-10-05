using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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