using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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