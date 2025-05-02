using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ApothecaryGame
{
    /// <summary>
    /// A reusable system for creating bubbling effects in potions and cauldrons
    /// </summary>
    public class BubbleSystem
    {
        private Game _game;
        private Random _random = new Random();
        private List<Bubble> _bubbles = new List<Bubble>();
        private Texture2D _bubbleTexture;
        private RenderHelper _renderHelper;
        
        // Configuration
        private Rectangle _containerBounds;
        private Color _liquidColor = Color.Purple;
        private Color _bubbleColor = new Color(180, 100, 255, 150);
        private float _turbulence = 1.0f; // How much horizontal movement (0-2)
        private float _buoyancy = 1.0f;   // How fast bubbles rise (0-2)
        private float _viscosity = 1.0f;  // How slowly bubbles move (0-2, higher means slower)
        private int _maxBubbles = 20;     // Maximum number of bubbles
        private float _spawnRate = 1.0f;  // Base spawn rate (bubbles per second)
        private bool _isBoiling = false;  // Boiling state (more, faster bubbles)
        
        // Animation timer
        private float _animationTimer = 0f;
        private float _spawnTimer = 0f;
        
        // Bubble size range
        private float _minBubbleSize = 3f;
        private float _maxBubbleSize = 8f;
        
        // Effect layers
        private List<Vector2> _surfaceFoam = new List<Vector2>();
        private float _surfaceHeight;
        
        /// <summary>
        /// Creates a new bubble system
        /// </summary>
        public BubbleSystem(Game game, RenderHelper renderHelper)
        {
            _game = game;
            _renderHelper = renderHelper;
        }
        
        /// <summary>
        /// Initialize the bubble system with the container dimensions
        /// </summary>
        public void Initialize(Rectangle containerBounds, Color liquidColor)
        {
            _containerBounds = containerBounds;
            _liquidColor = liquidColor;
            
            // Derive bubble color from liquid color (lighter variant)
            _bubbleColor = new Color(
                (byte)Math.Min(255, liquidColor.R + 50),
                (byte)Math.Min(255, liquidColor.G + 50),
                (byte)Math.Min(255, liquidColor.B + 50),
                150
            );
            
            // Calculate surface height (usually at 80% of container height)
            _surfaceHeight = containerBounds.Y + containerBounds.Height * 0.8f;
            
            // Initial bubbles
            CreateInitialBubbles();
            
            // Create surface foam
            CreateSurfaceFoam();
        }
        
        /// <summary>
        /// Load content for the bubble system
        /// </summary>
        public void LoadContent()
        {
            // Create bubble texture (could be loaded from content if preferred)
            _bubbleTexture = CreateBubbleTexture(32);
        }
        
        /// <summary>
        /// Creates a simple circular bubble texture
        /// </summary>
        private Texture2D CreateBubbleTexture(int size)
        {
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Calculate distance from center
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    
                    // Inside circle boundary
                    if (distance < radius)
                    {
                        // Calculate alpha based on distance from edge (more transparent at center)
                        float alpha = distance / radius;
                        
                        // Make it bubble-like (more transparent in the middle)
                        float edgeFactor = 1.0f - (radius - distance) / radius;
                        edgeFactor = (float)Math.Pow(edgeFactor, 0.5); // Adjust pow for edge thickness
                        
                        // Add shine effect
                        float shine = 0f;
                        float shineX = x - radius * 0.7f;
                        float shineY = y - radius * 0.7f;
                        if (Math.Sqrt(shineX * shineX + shineY * shineY) < radius * 0.3f)
                        {
                            shine = 0.7f;
                        }
                        
                        data[y * size + x] = new Color(
                            255,
                            255,
                            255,
                            (byte)(255 * edgeFactor * (0.3f + 0.7f * shine))
                        );
                    }
                    else
                    {
                        data[y * size + x] = Color.Transparent;
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Creates initial bubbles
        /// </summary>
        private void CreateInitialBubbles()
        {
            _bubbles.Clear();
            
            // Add some initial bubbles
            int initialBubbles = _maxBubbles / 2;
            for (int i = 0; i < initialBubbles; i++)
            {
                AddBubble(true);
            }
        }
        
        /// <summary>
        /// Creates foam particles along the surface
        /// </summary>
        private void CreateSurfaceFoam()
        {
            _surfaceFoam.Clear();
            
            // Add foam particles along the surface
            int foamCount = _containerBounds.Width / 5;
            for (int i = 0; i < foamCount; i++)
            {
                _surfaceFoam.Add(new Vector2(
                    _containerBounds.X + i * 5 + (float)_random.NextDouble() * 5,
                    _surfaceHeight + (float)_random.NextDouble() * 3 - 1.5f
                ));
            }
        }
        
        /// <summary>
        /// Adds a new bubble to the system
        /// </summary>
        private void AddBubble(bool randomizePosition = false)
        {
            // If we have too many bubbles, don't add more
            if (_bubbles.Count >= _maxBubbles)
                return;
            
            // Calculate spawn position
            Vector2 position;
            
            if (randomizePosition)
            {
                // Random position throughout liquid
                position = new Vector2(
                    _containerBounds.X + (float)_random.NextDouble() * _containerBounds.Width,
                    _containerBounds.Y + (_containerBounds.Height * 0.8f) * (float)_random.NextDouble() + (_containerBounds.Height * 0.2f)
                );
            }
            else
            {
                // New bubbles spawn at the bottom
                position = new Vector2(
                    _containerBounds.X + (float)_random.NextDouble() * _containerBounds.Width,
                    _containerBounds.Y + _containerBounds.Height - (float)_random.NextDouble() * 10
                );
            }
            
            // Calculate bubble size
            float size = _minBubbleSize + (float)_random.NextDouble() * (_maxBubbleSize - _minBubbleSize);
            
            // Adjust for boiling state
            if (_isBoiling)
            {
                size *= 1.2f;
            }
            
            // Create bubble
            _bubbles.Add(new Bubble
            {
                Position = position,
                Size = size,
                Velocity = new Vector2(
                    ((float)_random.NextDouble() * 2 - 1) * _turbulence * 0.5f,
                    -((float)_random.NextDouble() * 0.5f + 0.5f) * _buoyancy
                ),
                Alpha = (float)_random.NextDouble() * 0.5f + 0.3f,
                RotationSpeed = ((float)_random.NextDouble() * 2 - 1) * 0.05f,
                Rotation = (float)_random.NextDouble() * MathHelper.TwoPi,
                SpawnTime = _animationTimer
            });
        }
        
        /// <summary>
        /// Update the bubble system
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _animationTimer += deltaTime;
            _spawnTimer += deltaTime;
            
            // Spawn new bubbles based on spawn rate
            float targetSpawnRate = _spawnRate;
            if (_isBoiling)
            {
                targetSpawnRate *= 3f;
            }
            
            if (_spawnTimer > 1f / targetSpawnRate)
            {
                AddBubble();
                _spawnTimer = 0f;
            }
            
            // Update existing bubbles
            for (int i = _bubbles.Count - 1; i >= 0; i--)
            {
                Bubble bubble = _bubbles[i];
                
                // Update position based on velocity
                bubble.Position += bubble.Velocity / _viscosity;
                
                // Add some wobble
                bubble.Position.X += (float)Math.Sin(_animationTimer * 3f + bubble.SpawnTime * 7.3f) * 0.3f * _turbulence;
                
                // Update rotation
                bubble.Rotation += bubble.RotationSpeed;
                
                // Update lifespan - bubbles fade out as they reach the surface
                float distanceFromSurface = _surfaceHeight - bubble.Position.Y;
                if (distanceFromSurface < bubble.Size)
                {
                    // Fade out as it reaches the surface
                    bubble.Alpha = Math.Max(0, bubble.Alpha - deltaTime * 2f);
                    
                    // Slow down
                    bubble.Velocity *= 0.95f;
                    
                    // Remove if completely faded
                    if (bubble.Alpha <= 0.01f)
                    {
                        _bubbles.RemoveAt(i);
                        continue;
                    }
                }
                
                // Check if bubble is outside container (shouldn't happen, but just in case)
                if (!_containerBounds.Contains(bubble.Position))
                {
                    _bubbles.RemoveAt(i);
                    continue;
                }
                
                // Update the bubble
                _bubbles[i] = bubble;
            }
            
            // Update surface foam
            for (int i = 0; i < _surfaceFoam.Count; i++)
            {
                Vector2 foam = _surfaceFoam[i];
                
                // Move slightly based on time
                foam.X += (float)Math.Sin(_animationTimer + i * 0.3f) * 0.2f;
                foam.Y += (float)Math.Cos(_animationTimer * 0.7f + i * 0.4f) * 0.1f;
                
                // Keep within bounds
                if (foam.X < _containerBounds.X)
                    foam.X = _containerBounds.X + _containerBounds.Width;
                if (foam.X > _containerBounds.X + _containerBounds.Width)
                    foam.X = _containerBounds.X;
                
                _surfaceFoam[i] = foam;
            }
        }
        
        /// <summary>
        /// Draw the bubble system
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw surface foam first
            foreach (Vector2 foam in _surfaceFoam)
            {
                float foamSize = 3f + (float)Math.Sin(_animationTimer + foam.X * 0.1f) * 1f;
                
                spriteBatch.Draw(_bubbleTexture,
                       new Rectangle((int)foam.X, (int)foam.Y, (int)foamSize, (int)(foamSize * 0.5f)),
                       null,
                       new Color(_bubbleColor.R, _bubbleColor.G, _bubbleColor.B, 100),
                       0f,
                       new Vector2(_bubbleTexture.Width / 2, _bubbleTexture.Height / 2),
                       SpriteEffects.None,
                       0f);
            }
            
            // Draw bubbles
            foreach (Bubble bubble in _bubbles)
            {
                // Adjust color based on depth and boiling state
                float depthFactor = (_containerBounds.Y + _containerBounds.Height - bubble.Position.Y) / _containerBounds.Height;
                float colorIntensity = 0.6f + depthFactor * 0.4f;
                
                Color bubbleColor = new Color(
                    (byte)(_bubbleColor.R * colorIntensity),
                    (byte)(_bubbleColor.G * colorIntensity),
                    (byte)(_bubbleColor.B * colorIntensity),
                    (byte)(_bubbleColor.A * bubble.Alpha)
                );
                
                spriteBatch.Draw(_bubbleTexture,
                       new Rectangle((int)bubble.Position.X, (int)bubble.Position.Y, (int)bubble.Size, (int)bubble.Size),
                       null,
                       bubbleColor,
                       bubble.Rotation,
                       new Vector2(_bubbleTexture.Width / 2, _bubbleTexture.Height / 2),
                       SpriteEffects.None,
                       0f);
            }
        }
        
        /// <summary>
        /// Set the animation parameters
        /// </summary>
        public void SetParameters(float turbulence, float buoyancy, float viscosity, int maxBubbles, float spawnRate)
        {
            _turbulence = MathHelper.Clamp(turbulence, 0f, 2f);
            _buoyancy = MathHelper.Clamp(buoyancy, 0f, 2f);
            _viscosity = MathHelper.Clamp(viscosity, 0.1f, 2f);
            _maxBubbles = Math.Max(1, maxBubbles);
            _spawnRate = Math.Max(0.1f, spawnRate);
        }
        
        /// <summary>
        /// Set the bubble size range
        /// </summary>
        public void SetBubbleSize(float minSize, float maxSize)
        {
            _minBubbleSize = Math.Max(1, minSize);
            _maxBubbleSize = Math.Max(_minBubbleSize + 1, maxSize);
        }
        
        /// <summary>
        /// Set the liquid color
        /// </summary>
        public void SetLiquidColor(Color liquidColor)
        {
            _liquidColor = liquidColor;
            
            // Derive bubble color (lighter variant)
            _bubbleColor = new Color(
                (byte)Math.Min(255, liquidColor.R + 50),
                (byte)Math.Min(255, liquidColor.G + 50),
                (byte)Math.Min(255, liquidColor.B + 50),
                150
            );
        }
        
        /// <summary>
        /// Set the boiling state - increases activity
        /// </summary>
        public void SetBoiling(bool isBoiling)
        {
            _isBoiling = isBoiling;
        }
        
        /// <summary>
        /// Handle reaction when adding ingredients
        /// </summary>
        public void TriggerReaction(ReactionType type)
        {
            switch (type)
            {
                case ReactionType.Splash:
                    // Add a bunch of bubbles at once
                    for (int i = 0; i < 10; i++)
                    {
                        AddBubble(true);
                    }
                    break;
                    
                case ReactionType.Fizz:
                    // Temporarily increase spawn rate
                    _spawnRate *= 3;
                    break;
                    
                case ReactionType.Explosion:
                    // Add many bubbles and increase turbulence
                    _turbulence = 2.0f;
                    for (int i = 0; i < 20; i++)
                    {
                        AddBubble(true);
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// Represents a single bubble in the system
    /// </summary>
    public struct Bubble
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public float Alpha;
        public float Rotation;
        public float RotationSpeed;
        public float SpawnTime;
    }
    
    /// <summary>
    /// Types of reactions that can be triggered
    /// </summary>
    public enum ReactionType
    {
        Splash,
        Fizz,
        Explosion
    }
}