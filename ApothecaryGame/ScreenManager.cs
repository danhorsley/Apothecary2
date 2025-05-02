using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ApothecaryGame
{
    /// <summary>
    /// Game state enum to track current screen
    /// </summary>
    public enum GameState
    {
        Loading,
        MainMenu,
        MixingScreen,
        ShopScreen,
        ExplorationScreen,
        Exit
    }

    /// <summary>
    /// Loading screen and main menu manager
    /// </summary>
    public class ScreenManager
    {
        private readonly Game _game;
        private readonly RenderHelper _renderHelper;
        private SpriteFont _titleFont;
        private SpriteFont _menuFont;
        
        // Game state
        private GameState _currentState = GameState.Loading;
        private float _loadingProgress = 0f;
        private float _loadingSpeed = 0.005f; // Adjust for faster/slower loading
        
        // Loading screen assets
        private Texture2D _loadingBackground;
        private Texture2D _cauldronTexture;
        private Texture2D _titleTexture;

        private List<Particle> _bubbles = new List<Particle>();
        private List<Particle> _sparkles = new List<Particle>();
        private Random _random = new Random();
        private float _titlePulse = 0f;
        
        // Menu assets
        private Texture2D _menuBackground;
        private Rectangle[] _menuButtons = new Rectangle[3]; // Mix, Shop, Explore
        private Texture2D _buttonTexture;
        private int _selectedButton = 0;
        private bool _buttonPressed = false;
        
        // Animation timers
        private float _animationTimer = 0f;
        private KeyboardState _previousKeyboardState;
        
        // Constants
        private const int MaxParticles = 100;
        private Color[] _sparkleColors = new Color[] 
        {
            Color.Gold,
            Color.Yellow,
            Color.LightGoldenrodYellow,
            Color.White,
            Color.LightYellow
        };

        public ScreenManager(Game game, RenderHelper renderHelper)
        {
            _game = game;
            _renderHelper = renderHelper;
        }
        
        public void Initialize(int screenWidth, int screenHeight)
        {
            // Set up menu buttons along the bottom
            int buttonWidth = 200;
            int buttonHeight = 60;
            int buttonSpacing = 40;
            int buttonY = screenHeight - buttonHeight - 40; // Position near bottom
            
            // Calculate total width of all buttons + spacing
            int totalWidth = (buttonWidth * 3) + (buttonSpacing * 2);
            int startX = (screenWidth - totalWidth) / 2; // Center horizontally
            
            // Position buttons in a row along the bottom
            _menuButtons[0] = new Rectangle(startX, buttonY, buttonWidth, buttonHeight);
            _menuButtons[1] = new Rectangle(startX + buttonWidth + buttonSpacing, buttonY, buttonWidth, buttonHeight);
            _menuButtons[2] = new Rectangle(startX + (buttonWidth + buttonSpacing) * 2, buttonY, buttonWidth, buttonHeight);
            
            // Initialize particles
            CreateParticles();
        }
        
        public void LoadContent()
        {
            // Load fonts
            try
            {
                _titleFont = _game.Content.Load<SpriteFont>("TitleFont");
                _menuFont = _game.Content.Load<SpriteFont>("Font");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading fonts: {ex.Message}");
            }
            
            // Load background images
            try
            {
                // Replace with your own background images - add these to your Content project
                _loadingBackground = _game.Content.Load<Texture2D>("Backgrounds/LoadingBackground");
                _menuBackground = _game.Content.Load<Texture2D>("Backgrounds/MenuBackground");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading backgrounds: {ex.Message} - falling back to generated textures");
                // Create fallback textures
                _loadingBackground = CreateLoadingBackground();
                _menuBackground = CreateMenuBackground();
            }
             // Load title image
            try
            {
                _titleTexture = _game.Content.Load<Texture2D>("UI/TitleLogo");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading title logo: {ex.Message}");
                // We'll handle this in the Draw method by falling back to text
            }
            // Other texture creation
            _buttonTexture = _renderHelper.CreateRectangleTexture(200, 60, Color.DarkGreen);
            _cauldronTexture = CreateCauldronTexture();
        }
        
        private Texture2D CreateLoadingBackground()
        {
            // Create a gradient background texture
            int width = _game.GraphicsDevice.Viewport.Width;
            int height = _game.GraphicsDevice.Viewport.Height;
            
            Texture2D texture = new Texture2D(_game.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            // Dark purple to dark blue gradient
            for (int y = 0; y < height; y++)
            {
                // Calculate gradient factor (0 at top, 1 at bottom)
                float factor = (float)y / height;
                
                // Interpolate between colors
                Color gradientColor = new Color(
                    (byte)(20 + (10 - 20) * factor), // R
                    (byte)(10 + (40 - 10) * factor), // G
                    (byte)(40 + (60 - 40) * factor)  // B
                );
                
                for (int x = 0; x < width; x++)
                {
                    // Add some noise for texture
                    float noise = (float)_random.NextDouble() * 0.1f;
                    Color pixelColor = new Color(
                        (byte)MathHelper.Clamp(gradientColor.R + noise * 255, 0, 255),
                        (byte)MathHelper.Clamp(gradientColor.G + noise * 255, 0, 255),
                        (byte)MathHelper.Clamp(gradientColor.B + noise * 255, 0, 255)
                    );
                    
                    data[y * width + x] = pixelColor;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        private Texture2D CreateMenuBackground()
        {
            // Create a textured background for the menu
            int width = _game.GraphicsDevice.Viewport.Width;
            int height = _game.GraphicsDevice.Viewport.Height;
            
            Texture2D texture = new Texture2D(_game.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            // Dark wooden texture with potion shop vibe
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Base wood color
                    Color baseColor = new Color(60, 30, 10);
                    
                    // Add wood grain
                    float grain = (float)Math.Sin(x * 0.1f) * 0.5f + 
                                 (float)Math.Sin(y * 0.05f) * 0.5f;
                    
                    // Add some random noise
                    float noise = (float)_random.NextDouble() * 0.2f;
                    
                    // Combine effects
                    float effect = (grain + noise) * 0.3f;
                    
                    Color pixelColor = new Color(
                        (byte)MathHelper.Clamp(baseColor.R + effect * 50, 0, 255),
                        (byte)MathHelper.Clamp(baseColor.G + effect * 30, 0, 255),
                        (byte)MathHelper.Clamp(baseColor.B + effect * 10, 0, 255)
                    );
                    
                    data[y * width + x] = pixelColor;
                    
                    // Add some floating dust particles
                    if (_random.NextDouble() < 0.001)
                    {
                        data[y * width + x] = new Color(200, 200, 150, 100);
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        private Texture2D CreateCauldronTexture()
        {
            int size = 128;
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            // Fill with transparent
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            int centerX = size / 2;
            int top = 10;
            int bottom = size - 10;
            
            // Draw cauldron
            for (int y = top; y < bottom; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    
                    // Calculate distance from center horizontally
                    float distanceX = Math.Abs(x - centerX);
                    
                    // Calculate maximum width at this height (ellipse shape)
                    float heightRatio = (float)(y - top) / (bottom - top);
                    float widthAtHeight = (size / 2 - 5) * (1.0f - 0.3f * (1.0f - heightRatio));
                    
                    if (distanceX <= widthAtHeight)
                    {
                        // Inside cauldron - dark gray
                        if (y == top || distanceX >= widthAtHeight - 1 || y >= bottom - 1)
                        {
                            // Edge - black
                            data[index] = Color.Black;
                        }
                        else
                        {
                            // Inside - dark purple with bubbles
                            float bubble = (float)Math.Sin(x * 0.5f + y * 0.3f) * 0.5f + 0.5f;
                            
                            if (bubble > 0.8f && y < bottom - 10)
                            {
                                // Bubble - light purple
                                data[index] = new Color(180, 100, 255);
                            }
                            else
                            {
                                // Potion liquid - dark purple
                                data[index] = new Color(80, 20, 120);
                            }
                        }
                    }
                }
            }
            
            // Draw cauldron handles
            int handleWidth = 10;
            int handleHeight = 30;
            int handleDistance = 50;
            
            // Left handle
            for (int y = top; y < top + handleHeight; y++)
            {
                for (int x = centerX - handleDistance - handleWidth; x < centerX - handleDistance; x++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                    {
                        int index = y * size + x;
                        data[index] = Color.Black;
                    }
                }
            }
            
            // Right handle
            for (int y = top; y < top + handleHeight; y++)
            {
                for (int x = centerX + handleDistance; x < centerX + handleDistance + handleWidth; x++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                    {
                        int index = y * size + x;
                        data[index] = Color.Black;
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        private void CreateParticles()
        {
            _bubbles.Clear();
            _sparkles.Clear();
            
            // Create initial bubbles
            for (int i = 0; i < MaxParticles / 2; i++)
            {
                AddBubble();
            }
            
            // Create initial sparkles
            for (int i = 0; i < MaxParticles / 2; i++)
            {
                AddSparkle();
            }
        }
        
        private void AddBubble()
        {
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Create bubble particle
            _bubbles.Add(new Particle
            {
                Position = new Vector2(
                    screenWidth / 2 + (float)(_random.NextDouble() * 100 - 50),
                    screenHeight / 2 + 50 + (float)(_random.NextDouble() * 30)
                ),
                Velocity = new Vector2(
                    (float)(_random.NextDouble() * 0.4 - 0.2),
                    -(float)(_random.NextDouble() * 1.0 + 0.5)
                ),
                Size = (float)(_random.NextDouble() * 8 + 2),
                Color = new Color(
                    180 + (int)(_random.NextDouble() * 75),
                    100 + (int)(_random.NextDouble() * 100),
                    255,
                    100 + (int)(_random.NextDouble() * 155)
                ),
                Lifetime = 1.0f + (float)(_random.NextDouble() * 2.0)
            });
        }
        
        private void AddSparkle()
        {
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // For the menu, we want to focus sparkles around the title in top left
            bool focusOnTopLeft = _currentState == GameState.MainMenu && _random.NextDouble() > 0.6;
            
            Vector2 basePosition;
            if (focusOnTopLeft)
            {
                // Position around the top-left title area
                basePosition = new Vector2(
                    100, // Approximation of title center X
                    60   // Approximation of title center Y
                );
            }
            else if (_currentState == GameState.Loading && _random.NextDouble() > 0.5)
            {
                // For loading screen, focus on the upper area where title would be
                basePosition = new Vector2(
                    screenWidth / 2,
                    screenHeight / 4
                );
            }
            else if (_random.NextDouble() > 0.7)
            {
                // Some sparkles around the cauldron
                basePosition = new Vector2(
                    screenWidth / 2,
                    screenHeight / 2 + 20
                );
            }
            else
            {
                // Random position throughout the screen
                basePosition = new Vector2(
                    _random.Next(screenWidth),
                    _random.Next(screenHeight)
                );
            }
            
            // Add random offset
            float radius = focusOnTopLeft ? 80 : 150; // Tighter clustering for title area
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            
            // Create sparkle particle
            _sparkles.Add(new Particle
            {
                Position = new Vector2(
                    basePosition.X + (float)Math.Cos(angle) * radius * (float)_random.NextDouble(),
                    basePosition.Y + (float)Math.Sin(angle) * radius * (float)_random.NextDouble()
                ),
                Velocity = new Vector2(
                    (float)(_random.NextDouble() * 0.6 - 0.3),
                    (float)(_random.NextDouble() * 0.6 - 0.3)
                ),
                Size = (float)(_random.NextDouble() * 3 + 1),
                Color = _sparkleColors[_random.Next(_sparkleColors.Length)],
                Lifetime = 0.2f + (float)(_random.NextDouble() * 0.8)
            });
        }
        
        public GameState Update(GameTime gameTime)
        {
            _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _titlePulse = (float)Math.Sin(_animationTimer * 2) * 0.2f + 1.0f; // Pulsating effect
            
            // Update keyboard state
            KeyboardState keyboardState = Keyboard.GetState();
            
            switch (_currentState)
            {
                case GameState.Loading:
                    UpdateLoading(gameTime);
                    break;
                    
                case GameState.MainMenu:
                    UpdateMainMenu(gameTime, keyboardState);
                    break;
            }
            
            _previousKeyboardState = keyboardState;
            
            return _currentState;
        }
        
        private void UpdateLoading(GameTime gameTime)
        {
            // Update loading progress
            _loadingProgress += _loadingSpeed;
            
            // When loading is complete, transition to main menu
            if (_loadingProgress >= 1.0f)
            {
                _currentState = GameState.MainMenu;
                _loadingProgress = 1.0f;
            }
            
            // Update particles
            UpdateParticles(gameTime);
        }
        
        private void UpdateMainMenu(GameTime gameTime, KeyboardState keyboardState)
        {
            // Handle keyboard navigation
            bool upPressed = keyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up);
            bool downPressed = keyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down);
            bool enterPressed = keyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter);
            
            if (upPressed)
            {
                _selectedButton = (_selectedButton + _menuButtons.Length - 1) % _menuButtons.Length;
            }
            
            if (downPressed)
            {
                _selectedButton = (_selectedButton + 1) % _menuButtons.Length;
            }
            
            if (enterPressed)
            {
                // Handle button selection
                switch (_selectedButton)
                {
                    case 0: // Mix
                        _currentState = GameState.MixingScreen;
                        break;
                    case 1: // Shop
                        _currentState = GameState.ShopScreen;
                        break;
                    case 2: // Explore
                        _currentState = GameState.ExplorationScreen;
                        break;
                }
                _buttonPressed = true;
            }
            
            // Mouse handling for buttons
            MouseState mouseState = Mouse.GetState();
            bool isClicked = mouseState.LeftButton == ButtonState.Pressed && 
                            Mouse.GetState().LeftButton == ButtonState.Released;
            
            for (int i = 0; i < _menuButtons.Length; i++)
            {
                if (_menuButtons[i].Contains(mouseState.Position))
                {
                    _selectedButton = i;
                    
                    if (isClicked)
                    {
                        // Handle button click
                        switch (i)
                        {
                            case 0: // Mix
                                _currentState = GameState.MixingScreen;
                                break;
                            case 1: // Shop
                                _currentState = GameState.ShopScreen;
                                break;
                            case 2: // Explore
                                _currentState = GameState.ExplorationScreen;
                                break;
                        }
                        _buttonPressed = true;
                    }
                }
            }
            
            // Update particles
            UpdateParticles(gameTime);
        }
        
        private void UpdateParticles(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update bubbles
            for (int i = _bubbles.Count - 1; i >= 0; i--)
            {
                Particle bubble = _bubbles[i];
                bubble.Lifetime -= deltaTime;
                
                if (bubble.Lifetime <= 0)
                {
                    _bubbles.RemoveAt(i);
                    AddBubble(); // Replace bubble
                }
                else
                {
                    // Update position
                    bubble.Position += bubble.Velocity;
                    
                    // Add some wobble
                    bubble.Position.X += (float)Math.Sin(_animationTimer * 5 + i) * 0.3f;
                    
                    // Update alpha based on lifetime
                    Color color = bubble.Color;
                    bubble.Color = new Color(
                        color.R, color.G, color.B, 
                        (byte)(color.A * (bubble.Lifetime / 3.0f))
                    );
                    
                    _bubbles[i] = bubble;
                }
            }
            
            // Update sparkles
            for (int i = _sparkles.Count - 1; i >= 0; i--)
            {
                Particle sparkle = _sparkles[i];
                sparkle.Lifetime -= deltaTime;
                
                if (sparkle.Lifetime <= 0)
                {
                    _sparkles.RemoveAt(i);
                    AddSparkle(); // Replace sparkle
                }
                else
                {
                    // Update position
                    sparkle.Position += sparkle.Velocity;
                    
                    // Fade out based on lifetime
                    Color color = sparkle.Color;
                    float alphaFactor = sparkle.Lifetime < 0.3f ? 
                                     sparkle.Lifetime / 0.3f : 1.0f;
                    
                    sparkle.Color = new Color(
                        color.R, color.G, color.B, 
                        (byte)(color.A * alphaFactor)
                    );
                    
                    // Pulsate size
                    float pulse = (float)Math.Sin(_animationTimer * 10 + i) * 0.5f + 1.0f;
                    sparkle.Size *= pulse;
                    
                    _sparkles[i] = sparkle;
                }
            }
            
            // Ensure we maintain particle count
            while (_bubbles.Count < MaxParticles / 2)
            {
                AddBubble();
            }
            
            while (_sparkles.Count < MaxParticles / 2)
            {
                AddSparkle();
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            switch (_currentState)
            {
                case GameState.Loading:
                    DrawLoadingScreen(spriteBatch);
                    break;
                    
                case GameState.MainMenu:
                    DrawMainMenu(spriteBatch);
                    break;
            }
        }
        
        private void DrawLoadingScreen(SpriteBatch spriteBatch)
        {
            // Draw background (your custom image will already have the title)
            spriteBatch.Draw(_loadingBackground, 
                        new Rectangle(0, 0, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height), 
                        Color.White);
            
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // No title drawing here since your background already has it
            
            // Draw tagline (you might want to keep this or remove it)
            string tagline = "Brew. Sell. Explore.";
            if (_menuFont != null)
            {
                Vector2 taglineSize = _menuFont.MeasureString(tagline);
                spriteBatch.DrawString(_menuFont, tagline, 
                                    new Vector2(screenWidth / 2 - taglineSize.X / 2, screenHeight / 3 + 50), 
                                    new Color(200, 200, 255) * (0.7f + (float)Math.Sin(_animationTimer * 2) * 0.3f));
            }
            else
            {
                _renderHelper.DrawText(spriteBatch, null, tagline, 
                                    new Vector2(screenWidth / 2 - 80, screenHeight / 3 + 50), 
                                    Color.LightBlue);
            }
            
            // Draw cauldron
            Rectangle cauldronRect = new Rectangle(
                screenWidth / 2 - _cauldronTexture.Width / 2,
                screenHeight / 2 - _cauldronTexture.Height / 2 + 20,
                _cauldronTexture.Width,
                _cauldronTexture.Height
            );
            spriteBatch.Draw(_cauldronTexture, cauldronRect, Color.White);
            
            // Draw bubbles
            foreach (var bubble in _bubbles)
            {
                DrawCircle(spriteBatch, bubble.Position, bubble.Size, bubble.Color);
            }
            
            // Draw sparkles (all positioned over the upper third of the screen where title is)
            foreach (var sparkle in _sparkles)
            {
                // Adjust position of sparkles to focus on the upper area
                Vector2 adjustedPosition = new Vector2(
                    sparkle.Position.X,
                    sparkle.Position.Y * 0.4f  // Compress Y range to upper portion
                );
                DrawStar(spriteBatch, adjustedPosition, sparkle.Size, sparkle.Color);
            }
            
            // Draw loading bar
            int loadingBarWidth = screenWidth / 2;
            int loadingBarHeight = 20;
            Rectangle loadingBarBg = new Rectangle(
                screenWidth / 2 - loadingBarWidth / 2,
                screenHeight * 3 / 4,
                loadingBarWidth,
                loadingBarHeight
            );
            
            Rectangle loadingBarFg = new Rectangle(
                loadingBarBg.X,
                loadingBarBg.Y,
                (int)(loadingBarWidth * _loadingProgress),
                loadingBarHeight
            );
            
            // Draw background of loading bar
            _renderHelper.DrawRectangleWithBorder(spriteBatch, loadingBarBg, 
                                            new Color(20, 20, 20, 200), Color.Gray);
            
            // Draw foreground of loading bar
            if (loadingBarFg.Width > 0)
            {
                spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.Purple), 
                            loadingBarFg, Color.White);
            }
            
            // Draw loading text
            string loadingText = "Loading: " + (int)(_loadingProgress * 100) + "%";
            if (_menuFont != null)
            {
                Vector2 loadingTextSize = _menuFont.MeasureString(loadingText);
                spriteBatch.DrawString(_menuFont, loadingText, 
                                    new Vector2(screenWidth / 2 - loadingTextSize.X / 2, 
                                                loadingBarBg.Y + loadingBarBg.Height + 10), 
                                    Color.White);
            }
            else
            {
                _renderHelper.DrawText(spriteBatch, null, loadingText, 
                                    new Vector2(screenWidth / 2 - 60, loadingBarBg.Y + loadingBarBg.Height + 10), 
                                    Color.White);
            }
            
            // Draw company logo or credit
            string credit = "MonoGame Potion Shop Simulator";
            if (_menuFont != null)
            {
                Vector2 creditSize = _menuFont.MeasureString(credit);
                spriteBatch.DrawString(_menuFont, credit, 
                                    new Vector2(screenWidth / 2 - creditSize.X / 2, 
                                                screenHeight - 40), 
                                    new Color(150, 150, 150));
            }
            else
            {
                _renderHelper.DrawText(spriteBatch, null, credit, 
                                    new Vector2(screenWidth / 2 - 100, screenHeight - 40), 
                                    Color.Gray);
            }
        }
        
        private void DrawMainMenu(SpriteBatch spriteBatch)
        {
            // Draw background
            spriteBatch.Draw(_menuBackground, 
                        new Rectangle(0, 0, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height), 
                        Color.White);
            
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Draw title logo in top left corner, small size
            if (_titleTexture != null)
            {
                // Scale down the title logo
                float titleScale = 0.5f;  // Adjust this value to get the size you want
                int titleWidth = (int)(_titleTexture.Width * titleScale);
                int titleHeight = (int)(_titleTexture.Height * titleScale);
                
                // Position in top left with some margin
                Rectangle titleRect = new Rectangle(
                    20,  // Left margin
                    20,  // Top margin
                    titleWidth,
                    titleHeight
                );
                
                // Draw the title logo
                spriteBatch.Draw(_titleTexture, titleRect, Color.White);
                
                // Draw sparkles over and around the title
                foreach (var sparkle in _sparkles)
                {
                    // Check if this sparkle should be positioned over the title
                    if (sparkle.Position.X < screenWidth / 3 && sparkle.Position.Y < screenHeight / 3)
                    {
                        // Adjust position to cover title area
                        Vector2 adjustedPosition = new Vector2(
                            sparkle.Position.X * 0.5f + titleRect.X,  // Compress X range to title area
                            sparkle.Position.Y * 0.5f + titleRect.Y   // Compress Y range to title area
                        );
                        
                        DrawStar(spriteBatch, adjustedPosition, sparkle.Size, sparkle.Color);
                    }
                }
            }
            else
            {
                // Fallback if title image not loaded
                _renderHelper.DrawText(spriteBatch, null, "APOTHECARY", 
                                    new Vector2(20, 20), 
                                    Color.Gold, true);
            }
            
            // Draw buttons (now at the bottom)
            string[] buttonLabels = { "Mix Potions", "Run Shop", "Explore" };
            
            for (int i = 0; i < _menuButtons.Length; i++)
            {
                Color buttonColor = (_selectedButton == i) ? 
                    new Color(0, 100, 0) : new Color(0, 60, 0);
                
                // Draw button
                _renderHelper.DrawRectangleWithBorder(spriteBatch, _menuButtons[i], 
                                                buttonColor, Color.Gold, 2);
                
                // Draw label
                if (_menuFont != null)
                {
                    Vector2 labelSize = _menuFont.MeasureString(buttonLabels[i]);
                    
                    spriteBatch.DrawString(_menuFont, buttonLabels[i], 
                                        new Vector2(_menuButtons[i].X + _menuButtons[i].Width / 2 - labelSize.X / 2, 
                                                    _menuButtons[i].Y + _menuButtons[i].Height / 2 - labelSize.Y / 2), 
                                        (_selectedButton == i) ? Color.White : new Color(200, 200, 150));
                }
                else
                {
                    _renderHelper.DrawText(spriteBatch, null, buttonLabels[i], 
                                        new Vector2(_menuButtons[i].X + 50, _menuButtons[i].Y + 20), 
                                        (_selectedButton == i) ? Color.White : Color.LightGray);
                }
                
                // Add a glow effect to selected button
                if (_selectedButton == i)
                {
                    Rectangle glowRect = new Rectangle(
                        _menuButtons[i].X - 5,
                        _menuButtons[i].Y - 5,
                        _menuButtons[i].Width + 10,
                        _menuButtons[i].Height + 10
                    );
                    
                    float glowIntensity = (float)Math.Sin(_animationTimer * 4) * 0.5f + 0.5f;
                    DrawGlow(spriteBatch, glowRect, new Color(255, 223, 100) * (0.3f * glowIntensity));
                }
            }
            
            // No decorative bottles - removed as requested
            
            // Draw regular sparkles throughout the screen
            foreach (var sparkle in _sparkles)
            {
                // Don't draw sparkles that were already drawn over the title
                if (!(sparkle.Position.X < screenWidth / 3 && sparkle.Position.Y < screenHeight / 3))
                {
                    DrawStar(spriteBatch, sparkle.Position, sparkle.Size, sparkle.Color);
                }
            }
            
            // Draw footer instructions
            string footer = "Use arrow keys to navigate, Enter to select";
            if (_menuFont != null)
            {
                Vector2 footerSize = _menuFont.MeasureString(footer);
                spriteBatch.DrawString(_menuFont, footer, 
                                    new Vector2(screenWidth / 2 - footerSize.X / 2, 
                                                screenHeight - 40), 
                                    Color.LightGray * 0.7f);
            }
            else
            {
                _renderHelper.DrawText(spriteBatch, null, footer, 
                                    new Vector2(screenWidth / 2 - 150, screenHeight - 40), 
                                    Color.Gray);
            }
        }
        
        private void DrawDecorations(SpriteBatch spriteBatch)
        {
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Draw some bottles and potion elements around the edge of the screen
            
            // Left side decorations - draw some potion bottles
            for (int i = 0; i < 3; i++)
            {
                DrawPotionBottle(spriteBatch, 
                                new Vector2(50, screenHeight / 4 + i * 120), 
                                _random.Next(3),
                                new Color(_random.Next(100, 255), _random.Next(100, 255), _random.Next(100, 255)));
            }
            
            // Right side decorations - draw some potion bottles
            for (int i = 0; i < 3; i++)
            {
                DrawPotionBottle(spriteBatch, 
                                new Vector2(screenWidth - 80, screenHeight / 4 + i * 120), 
                                _random.Next(3),
                                new Color(_random.Next(100, 255), _random.Next(100, 255), _random.Next(100, 255)));
            }
            
            // Draw some herbs hanging from the top
            for (int i = 0; i < 5; i++)
            {
                DrawHangingHerb(spriteBatch, 
                               new Vector2(screenWidth / 6 + i * screenWidth / 6, 30), 
                               _random.Next(3));
            }
        }
        
        private void DrawPotionBottle(SpriteBatch spriteBatch, Vector2 position, int type, Color liquidColor)
        {
            int bottleWidth = 30;
            int bottleHeight = 50;
            
            // Draw bottle shape
            Rectangle bottleRect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                bottleWidth,
                bottleHeight
            );
            
            // Draw bottle neck
            Rectangle neckRect = new Rectangle(
                (int)position.X + bottleWidth / 3,
                (int)position.Y - 15,
                bottleWidth / 3,
                20
            );
            
            // Draw cork
            Rectangle corkRect = new Rectangle(
                (int)position.X + bottleWidth / 3,
                (int)position.Y - 20,
                bottleWidth / 3,
                10
            );
            
            // Draw bottle with different shapes based on type
            switch (type)
            {
                case 0: // Round bottle
                    DrawCircle(spriteBatch, new Vector2(position.X + bottleWidth / 2, position.Y + bottleHeight / 2), 
                              bottleWidth / 2, Color.DarkGray * 0.5f);
                    DrawCircle(spriteBatch, new Vector2(position.X + bottleWidth / 2, position.Y + bottleHeight / 2), 
                              bottleWidth / 2 - 2, liquidColor * 0.8f);
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.DarkGray * 0.5f), neckRect, Color.White);
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, new Color(139, 69, 19)), corkRect, Color.White);
                    break;
                    
                case 1: // Square bottle
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.DarkGray * 0.5f), bottleRect, Color.White);
                    
                    // Draw liquid inside
                    Rectangle liquidRect = new Rectangle(
                        bottleRect.X + 2,
                        bottleRect.Y + 2,
                        bottleRect.Width - 4,
                        bottleRect.Height - 4
                    );
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, liquidColor * 0.8f), liquidRect, Color.White);
                    
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.DarkGray * 0.5f), neckRect, Color.White);
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, new Color(139, 69, 19)), corkRect, Color.White);
                    break;
                    
                case 2: // Triangle bottle
                    // Draw triangle bottle - using rectangle with clipping
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.DarkGray * 0.5f), bottleRect, Color.White);
                    
                    // Draw liquid inside (partial fill)
                    Rectangle triangleLiquidRect = new Rectangle(
                        bottleRect.X + 2,
                        bottleRect.Y + bottleRect.Height / 2,
                        bottleRect.Width - 4,
                        bottleRect.Height / 2 - 2
                    );
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, liquidColor * 0.8f), triangleLiquidRect, Color.White);
                    
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.DarkGray * 0.5f), neckRect, Color.White);
                    spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, new Color(139, 69, 19)), corkRect, Color.White);
                    break;
            }
            
            // Add a shine effect
            int shineSize = 5;
            Rectangle shineRect = new Rectangle(
                (int)position.X + bottleWidth / 4,
                (int)position.Y + bottleHeight / 4,
                shineSize,
                shineSize
            );
            DrawCircle(spriteBatch, new Vector2(shineRect.X + shineSize / 2, shineRect.Y + shineSize / 2), 
                      shineSize / 2, Color.White * 0.5f);
        }
        
        private void DrawHangingHerb(SpriteBatch spriteBatch, Vector2 position, int type)
        {
            Color herbColor = type == 0 ? Color.ForestGreen : 
                            type == 1 ? Color.SaddleBrown : 
                            Color.Purple;
            
            // Draw string
            Rectangle stringRect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                2,
                50
            );
            spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.SandyBrown), stringRect, Color.White);
            
            // Draw herb bundle
            for (int i = 0; i < 5; i++)
            {
                float angle = i * MathHelper.TwoPi / 5;
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * 10,
                    (float)Math.Sin(angle) * 10
                );
                
                Vector2 leafPos = new Vector2(position.X, position.Y + 60) + offset;
                
                Rectangle leafRect = new Rectangle(
                    (int)leafPos.X - 5,
                    (int)leafPos.Y - 10,
                    10,
                    20
                );
                
                spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, herbColor), leafRect, Color.White);
            }
            
            // Draw binding
            Rectangle bindingRect = new Rectangle(
                (int)position.X - 10,
                (int)position.Y + 45,
                20,
                5
            );
            spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.SandyBrown), bindingRect, Color.White);
        }
        
        #region Drawing Helpers
        
        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            Texture2D circleTexture = _renderHelper.CreateCircleTexture((int)(radius * 2), color);
            spriteBatch.Draw(circleTexture, 
                           new Rectangle((int)(center.X - radius), (int)(center.Y - radius), 
                                         (int)(radius * 2), (int)(radius * 2)), 
                           Color.White);
        }
        
        private void DrawStar(SpriteBatch spriteBatch, Vector2 position, float size, Color color)
        {
            // Draw a simple star shape using lines
            float halfSize = size / 2;
            
            // Draw horizontal line
            Rectangle horizRect = new Rectangle(
                (int)(position.X - halfSize),
                (int)(position.Y - 1),
                (int)size,
                2
            );
            
            // Draw vertical line
            Rectangle vertRect = new Rectangle(
                (int)(position.X - 1),
                (int)(position.Y - halfSize),
                2,
                (int)size
            );
            
            // Draw diagonal lines
            Rectangle diag1Rect = new Rectangle(
                (int)(position.X - halfSize),
                (int)(position.Y - halfSize),
                (int)size,
                (int)size
            );
            
            Rectangle diag2Rect = new Rectangle(
                (int)(position.X - halfSize),
                (int)(position.Y - halfSize),
                (int)size,
                (int)size
            );
            
            spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, color), horizRect, Color.White);
            spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, color), vertRect, Color.White);
            
            // Draw a center point for extra sparkle
            Rectangle centerRect = new Rectangle(
                (int)(position.X - 1),
                (int)(position.Y - 1),
                2,
                2
            );
            spriteBatch.Draw(_renderHelper.CreateRectangleTexture(1, 1, Color.White), centerRect, Color.White);
        }
        
        private void DrawGlow(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            // Draw a soft glow around the rectangle
            for (int i = 0; i < 3; i++)
            {
                Rectangle glowRect = new Rectangle(
                    rect.X - i * 2,
                    rect.Y - i * 2,
                    rect.Width + i * 4,
                    rect.Height + i * 4
                );
                
                _renderHelper.DrawRectangleWithBorder(spriteBatch, glowRect, 
                                                   Color.Transparent, 
                                                   new Color(color.R, color.G, color.B, (byte)(color.A / (i + 1))));
            }
        }
        
        #endregion
    }
    
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public Color Color;
        public float Lifetime;
    }
}