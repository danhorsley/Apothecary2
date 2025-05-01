using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ApothecaryGame
{
    /// <summary>
    /// Provides common rendering utilities for the game
    /// </summary>
    public class RenderHelper
    {
        private readonly Game _game;
        private Texture2D _pixelTexture;
        
        public RenderHelper(Game game)
        {
            _game = game;
        }
        
        public void Initialize()
        {
            // Create a 1x1 white texture for drawing shapes
            _pixelTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        
        /// <summary>
        /// Draws a rectangle with a border
        /// </summary>
        public void DrawRectangleWithBorder(SpriteBatch spriteBatch, Rectangle rectangle, 
                                         Color fillColor, Color borderColor, int borderWidth = 2)
        {
            // Draw fill
            spriteBatch.Draw(_pixelTexture, rectangle, fillColor);
            
            // Draw border
            // Top
            spriteBatch.Draw(_pixelTexture, 
                new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, borderWidth), 
                borderColor);
            
            // Bottom
            spriteBatch.Draw(_pixelTexture, 
                new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - borderWidth, 
                            rectangle.Width, borderWidth), 
                borderColor);
            
            // Left
            spriteBatch.Draw(_pixelTexture, 
                new Rectangle(rectangle.X, rectangle.Y, borderWidth, rectangle.Height), 
                borderColor);
            
            // Right
            spriteBatch.Draw(_pixelTexture, 
                new Rectangle(rectangle.X + rectangle.Width - borderWidth, rectangle.Y, 
                            borderWidth, rectangle.Height), 
                borderColor);
        }
        
        /// <summary>
        /// Creates a circle texture
        /// </summary>
        public Texture2D CreateCircleTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float radius = size / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Calculate position relative to center
                    float distFromCenter = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(size / 2f, size / 2f));
                    
                    int i = y * size + x;
                    
                    if (distFromCenter < radius)
                    {
                        // Inside circle
                        data[i] = color;
                    }
                    else
                    {
                        // Outside circle
                        data[i] = Color.Transparent;
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Creates a rectangle texture
        /// </summary>
        public Texture2D CreateRectangleTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(_game.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Draws text with an option for bold
        /// </summary>
        public void DrawText(SpriteBatch spriteBatch, SpriteFont font, string text, 
                          Vector2 position, Color color, bool isBold = false)
        {
            if (font != null)
            {
                // Draw text with font
                spriteBatch.DrawString(font, text, position, color);
                
                // Draw bold text (crude approach - draw twice with slight offset)
                if (isBold)
                {
                    spriteBatch.DrawString(font, text, position + new Vector2(1, 0), color);
                }
            }
            else
            {
                // Simple character rendering if font failed to load
                for (int i = 0; i < text.Length; i++)
                {
                    spriteBatch.Draw(_pixelTexture, new Rectangle(
                        (int)position.X + i * 10, 
                        (int)position.Y, 
                        8, 
                        12), color);
                }
            }
        }
        
        /// <summary>
        /// Draws a triangle pointing up or down
        /// </summary>
        public void DrawTriangle(SpriteBatch spriteBatch, int centerX, int centerY, 
                               int size, bool pointUp, Color color)
        {
            if (pointUp)
            {
                // Up-pointing triangle
                for (int y = 0; y < size; y++)
                {
                    int width = y * 2;
                    int leftX = centerX - width / 2;
                    
                    for (int x = 0; x < width + 1; x++)
                    {
                        spriteBatch.Draw(_pixelTexture, 
                                     new Rectangle(leftX + x, centerY + size - y, 1, 1), 
                                     color);
                    }
                }
            }
            else
            {
                // Down-pointing triangle
                for (int y = 0; y < size; y++)
                {
                    int width = y * 2;
                    int leftX = centerX - width / 2;
                    
                    for (int x = 0; x < width + 1; x++)
                    {
                        spriteBatch.Draw(_pixelTexture, 
                                     new Rectangle(leftX + x, centerY + y, 1, 1), 
                                     color);
                    }
                }
            }
        }
        
        /// <summary>
        /// Returns a human-readable color name for a given color
        /// </summary>
        public string GetColorName(Color color)
        {
            if (color.R > 200 && color.G < 100 && color.B < 100) return "red";
            if (color.R < 100 && color.G > 200 && color.B < 100) return "green";
            if (color.R < 100 && color.G < 100 && color.B > 200) return "blue";
            if (color.R > 200 && color.G > 200 && color.B < 100) return "yellow";
            if (color.R > 150 && color.G < 100 && color.B > 150) return "purple";
            if (color.R > 150 && color.G > 100 && color.B < 50) return "orange";
            if (color.R > 150 && color.G > 100 && color.B > 150) return "pink";
            if (color.R < 100 && color.G > 150 && color.B > 150) return "cyan";
            if (color.R > 100 && color.G > 75 && color.B < 75) return "brown";
            if (color.R > 200 && color.G > 200 && color.B > 200) return "white";
            if (color.R < 100 && color.G < 100 && color.B < 100) return "black";
            if (color.R > 100 && color.G > 100 && color.B > 100 && 
                color.R < 200 && color.G < 200 && color.B < 200) return "gray";
            if (color.R > 200 && color.G > 150 && color.B < 100) return "golden";
            
            return "mysterious";
        }
    }
}