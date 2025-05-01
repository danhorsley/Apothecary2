using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ApothecaryGame
{
    public class DraggableItemsGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // Font for drawing text
        private SpriteFont _font;
        
        // Draggable items
        private List<DraggableItem> _items;
        
        // Cauldron area in the center
        private Rectangle _cauldron;
        private Texture2D _cauldronTexture;
        private string _mixtureDescription = "Empty cauldron";
        
        // Category areas
        private Rectangle _herbsArea;
        private Rectangle _mineralsArea;
        private Rectangle _oilsArea;
        private Rectangle _spiritsArea;
        
        // UI elements
        private Rectangle _brewButton;
        private Rectangle _tasteButton;
        private Texture2D _buttonTexture;
        
        // Input tracking
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private DraggableItem _draggedItem;
        private Vector2 _dragOffset;
        
        // Placeholder textures
        private Texture2D _pixelTexture;
        
        public DraggableItemsGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Set up the category areas
            _herbsArea = new Rectangle(50, 50, 300, 150);
            _mineralsArea = new Rectangle(450, 50, 300, 150);
            _oilsArea = new Rectangle(50, 350, 300, 150);
            _spiritsArea = new Rectangle(450, 350, 300, 150);
            
            // Set up the cauldron area
            _cauldron = new Rectangle(350, 225, 100, 100);
            
            // Set up the buttons
            _brewButton = new Rectangle(550, 500, 100, 40);
            _tasteButton = new Rectangle(675, 500, 100, 40);
            
            _items = new List<DraggableItem>();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Try to load font, or create a placeholder
            try
            {
                _font = Content.Load<SpriteFont>("Font");
            }
            catch (Exception)
            {
                // If font loading fails, we'll handle text drawing differently
                Console.WriteLine("Font loading failed - using basic rendering");
            }
            
            // Create a 1x1 white texture for drawing shapes
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            // Create cauldron and button textures
            _cauldronTexture = CreateCircleTexture(100, Color.DarkGreen);
            _buttonTexture = CreateRectangleTexture(100, 40, Color.LightGray);
            
            // Create sample ingredients
            CreateSampleIngredients();
        }
        
        private void CreateSampleIngredients()
        {
            // Herbs & Spices
            AddIngredient("Red Herb", new Vector2(80, 80), Color.Red, IngredientType.Herb, "Sweet, Mild");
            AddIngredient("Mint Leaf", new Vector2(180, 80), Color.Green, IngredientType.Herb, "Cool, Bitter");
            AddIngredient("Cinnamon", new Vector2(280, 80), Color.Brown, IngredientType.Herb, "Warm, Spicy");
            AddIngredient("Lavender", new Vector2(80, 130), Color.Purple, IngredientType.Herb, "Floral, Calming");
            
            // Minerals
            AddIngredient("Blue Crystal", new Vector2(480, 80), Color.Blue, IngredientType.Mineral, "Hard, Cold");
            AddIngredient("Iron Filings", new Vector2(580, 80), Color.Gray, IngredientType.Mineral, "Metallic, Heavy");
            AddIngredient("Yellow Sulfur", new Vector2(680, 80), Color.Yellow, IngredientType.Mineral, "Pungent, Powdery");
            
            // Oils & Animal Products
            AddIngredient("Honey", new Vector2(80, 380), Color.Gold, IngredientType.Oil, "Sweet, Sticky");
            AddIngredient("Fish Oil", new Vector2(180, 380), Color.BlanchedAlmond, IngredientType.Oil, "Smelly, Slick");
            AddIngredient("Bone Dust", new Vector2(280, 380), Color.WhiteSmoke, IngredientType.Oil, "Chalky, Fine");
            
            // Spirits
            AddIngredient("Alcohol", new Vector2(480, 380), Color.SkyBlue, IngredientType.Spirit, "Strong, Clear");
            AddIngredient("Rose Extract", new Vector2(580, 380), Color.Pink, IngredientType.Spirit, "Floral, Potent");
            AddIngredient("Dark Essence", new Vector2(680, 380), Color.DarkViolet, IngredientType.Spirit, "Mysterious, Cloudy");
        }
        
        private void AddIngredient(string name, Vector2 position, Color color, IngredientType type, string properties)
        {
            int size = 40;
            Texture2D texture = CreateIngredientTexture(size, color, type);
            _items.Add(new DraggableItem(name, position, texture, size, color, type, properties));
        }
        
        private Texture2D CreateIngredientTexture(int size, Color color, IngredientType type)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            // Fill based on ingredient type
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Calculate position relative to center
                    float relX = (x - size / 2f) / (size / 2f);
                    float relY = (y - size / 2f) / (size / 2f);
                    float distFromCenter = (float)Math.Sqrt(relX * relX + relY * relY);
                    
                    int i = y * size + x;
                    
                    switch (type)
                    {
                        case IngredientType.Herb:
                            // Leaf-like shape
                            if (distFromCenter < 0.8f * (1 + 0.3f * Math.Sin(Math.Atan2(relY, relX) * 5)))
                                data[i] = color;
                            else
                                data[i] = Color.Transparent;
                            break;
                            
                        case IngredientType.Mineral:
                            // Crystal-like shape
                            if (Math.Abs(relX) < 0.7f && Math.Abs(relY) < 0.7f)
                                data[i] = color;
                            else
                                data[i] = Color.Transparent;
                            break;
                            
                        case IngredientType.Oil:
                            // Droplet shape
                            if (distFromCenter < 0.7f * (1 + 0.2f * relY))
                                data[i] = color;
                            else
                                data[i] = Color.Transparent;
                            break;
                            
                        case IngredientType.Spirit:
                            // Bottle-like shape
                            if (Math.Abs(relX) < 0.4f && relY < 0.8f && relY > -0.8f)
                                data[i] = color;
                            else if (Math.Abs(relX) < 0.2f && relY < 0.9f && relY > -0.9f)
                                data[i] = color;
                            else
                                data[i] = Color.Transparent;
                            break;
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        private Texture2D CreateCircleTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size, size);
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
        
        private Texture2D CreateRectangleTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            
            texture.SetData(data);
            return texture;
        }

        protected override void Update(GameTime gameTime)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            
            // Exit on Escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            // Handle mouse input
            HandleMouse();
            
            base.Update(gameTime);
        }
        
        private void HandleMouse()
        {
            Point mousePosition = _currentMouseState.Position;
            bool isClicked = _currentMouseState.LeftButton == ButtonState.Pressed && 
                            _previousMouseState.LeftButton == ButtonState.Released;
            bool isReleased = _currentMouseState.LeftButton == ButtonState.Released && 
                            _previousMouseState.LeftButton == ButtonState.Pressed;
            bool isDragging = _currentMouseState.LeftButton == ButtonState.Pressed;
            
            // Start dragging an item
            if (isClicked && _draggedItem == null)
            {
                foreach (var item in _items)
                {
                    if (item.Contains(mousePosition))
                    {
                        _draggedItem = item;
                        _dragOffset = new Vector2(item.Position.X - mousePosition.X, 
                                                 item.Position.Y - mousePosition.Y);
                        break;
                    }
                }
                
                // Check if buttons clicked
                if (_brewButton.Contains(mousePosition))
                {
                    BrewPotion();
                }
                else if (_tasteButton.Contains(mousePosition))
                {
                    TastePotion();
                }
            }
            
            // Continue dragging
            if (isDragging && _draggedItem != null)
            {
                _draggedItem.Position = new Vector2(mousePosition.X, mousePosition.Y) + _dragOffset;
            }
            
            // Release the dragged item
            if (isReleased && _draggedItem != null)
            {
                // Check if dropped into cauldron
                if (_cauldron.Contains(mousePosition))
                {
                    AddToCauldron(_draggedItem);
                }
                
                _draggedItem = null;
            }
        }
        
        private void AddToCauldron(DraggableItem item)
        {
            // Update mixture description based on the added ingredient
            if (_mixtureDescription == "Empty cauldron")
            {
                _mixtureDescription = $"A {GetColorName(item.Color)} mixture with a {item.Properties.Split(',')[0].Trim()} aroma";
            }
            else
            {
                // Change description based on ingredient type
                switch (item.Type)
                {
                    case IngredientType.Herb:
                        _mixtureDescription += $"\nAdded {item.Name}: The mixture changes color slightly";
                        break;
                    case IngredientType.Mineral:
                        _mixtureDescription += $"\nAdded {item.Name}: The mixture begins to bubble";
                        break;
                    case IngredientType.Oil:
                        _mixtureDescription += $"\nAdded {item.Name}: The texture becomes smoother";
                        break;
                    case IngredientType.Spirit:
                        _mixtureDescription += $"\nAdded {item.Name}: A strong aroma emerges";
                        break;
                }
            }
            
            // Return the item to its original location
            // In a real implementation, you might want to keep track of what's in the cauldron
        }
        
        private void BrewPotion()
        {
            if (_mixtureDescription != "Empty cauldron")
            {
                _mixtureDescription += "\nBrewed into a potion!";
            }
        }
        
        private void TastePotion()
        {
            if (_mixtureDescription != "Empty cauldron")
            {
                _mixtureDescription += "\nTasted: Has a peculiar flavor";
            }
        }
        
        private string GetColorName(Color color)
        {
            if (color == Color.Red) return "red";
            if (color == Color.Green) return "green";
            if (color == Color.Blue) return "blue";
            if (color == Color.Purple) return "purple";
            if (color == Color.Yellow) return "yellow";
            if (color == Color.Brown) return "brown";
            if (color == Color.Gray) return "gray";
            if (color == Color.Gold) return "golden";
            if (color == Color.BlanchedAlmond) return "oily";
            if (color == Color.WhiteSmoke) return "pale";
            if (color == Color.SkyBlue) return "clear";
            if (color == Color.Pink) return "pink";
            if (color == Color.DarkViolet) return "dark purple";
            
            return "mysterious";
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            
            _spriteBatch.Begin();
            
            // Draw category areas
            DrawRectangleWithBorder(_herbsArea, Color.DarkOliveGreen * 0.5f, Color.White);
            DrawRectangleWithBorder(_mineralsArea, Color.SteelBlue * 0.5f, Color.White);
            DrawRectangleWithBorder(_oilsArea, Color.Sienna * 0.5f, Color.White);
            DrawRectangleWithBorder(_spiritsArea, Color.MediumPurple * 0.5f, Color.White);
            
            // Draw category labels
            DrawText("Herbs & Spices", new Vector2(_herbsArea.X + 10, _herbsArea.Y + 10), Color.White);
            DrawText("Minerals", new Vector2(_mineralsArea.X + 10, _mineralsArea.Y + 10), Color.White);
            DrawText("Oils & Animal Products", new Vector2(_oilsArea.X + 10, _oilsArea.Y + 10), Color.White);
            DrawText("Spirits", new Vector2(_spiritsArea.X + 10, _spiritsArea.Y + 10), Color.White);
            
            // Draw cauldron
            _spriteBatch.Draw(_cauldronTexture, _cauldron, Color.White);
            
            // Draw buttons
            _spriteBatch.Draw(_buttonTexture, _brewButton, Color.White);
            _spriteBatch.Draw(_buttonTexture, _tasteButton, Color.White);
            DrawText("BREW", new Vector2(_brewButton.X + 30, _brewButton.Y + 10), Color.Black);
            DrawText("TASTE", new Vector2(_tasteButton.X + 25, _tasteButton.Y + 10), Color.Black);
            
            // Draw mixture description
            DrawRectangleWithBorder(new Rectangle(50, 500, 475, 80), Color.Black * 0.7f, Color.White);
            DrawText("Current Mixture:", new Vector2(60, 510), Color.White);
            
            // Split mixture description into lines
            string[] lines = _mixtureDescription.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                DrawText(lines[i], new Vector2(70, 535 + i * 20), Color.White);
            }
            
            // Draw all items
            foreach (var item in _items)
            {
                // If item is being dragged, draw it last (on top)
                if (item != _draggedItem)
                {
                    DrawItem(item);
                }
            }
            
            // Draw dragged item on top
            if (_draggedItem != null)
            {
                DrawItem(_draggedItem);
            }
            
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
        
        private void DrawItem(DraggableItem item)
        {
            // Draw item texture
            _spriteBatch.Draw(item.Texture, new Rectangle((int)item.Position.X, (int)item.Position.Y, 
                                                         item.Size, item.Size), Color.White);
            
            // Draw item name below
            DrawText(item.Name, new Vector2(item.Position.X, item.Position.Y + item.Size + 5), Color.White);
            
            // Draw tooltip on hover
            Point mousePosition = _currentMouseState.Position;
            if (item.Contains(mousePosition) && _draggedItem == null)
            {
                DrawTooltip(item, mousePosition);
            }
        }
        
        private void DrawTooltip(DraggableItem item, Point position)
        {
            // Tooltip text
            string tooltip = $"{item.Name}\n{item.Properties}";
            
            // Calculate size
            Vector2 tooltipSize = _font != null ? 
                _font.MeasureString(tooltip) : new Vector2(tooltip.Length * 7, 30);
            
            // Add padding
            Rectangle tooltipRect = new Rectangle(
                position.X + 20, 
                position.Y + 20, 
                (int)tooltipSize.X + 20, 
                (int)tooltipSize.Y + 20);
            
            // Draw tooltip background
            DrawRectangleWithBorder(tooltipRect, Color.Black * 0.8f, Color.White);
            
            // Draw tooltip text
            string[] lines = tooltip.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                DrawText(lines[i], new Vector2(tooltipRect.X + 10, tooltipRect.Y + 10 + i * 20), Color.White);
            }
        }
        
        private void DrawText(string text, Vector2 position, Color color)
        {
            if (_font != null)
            {
                _spriteBatch.DrawString(_font, text, position, color);
            }
            else
            {
                // Simple character rendering if font failed to load
                for (int i = 0; i < text.Length; i++)
                {
                    _spriteBatch.Draw(_pixelTexture, new Rectangle(
                        (int)position.X + i * 10, 
                        (int)position.Y, 
                        8, 
                        12), color);
                }
            }
        }
        
        private void DrawRectangleWithBorder(Rectangle rectangle, Color fillColor, Color borderColor)
        {
            // Draw fill
            _spriteBatch.Draw(_pixelTexture, rectangle, fillColor);
            
            // Draw border
            int borderWidth = 2;
            
            // Top
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, borderWidth), borderColor);
            // Bottom
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - borderWidth, rectangle.Width, borderWidth), borderColor);
            // Left
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, borderWidth, rectangle.Height), borderColor);
            // Right
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X + rectangle.Width - borderWidth, rectangle.Y, borderWidth, rectangle.Height), borderColor);
        }
    }
    
    public enum IngredientType
    {
        Herb,
        Mineral,
        Oil,
        Spirit
    }
    
    public class DraggableItem
    {
        public string Name { get; private set; }
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; private set; }
        public int Size { get; private set; }
        public Color Color { get; private set; }
        public IngredientType Type { get; private set; }
        public string Properties { get; private set; }
        
        public DraggableItem(string name, Vector2 position, Texture2D texture, int size, Color color, 
                            IngredientType type, string properties)
        {
            Name = name;
            Position = position;
            Texture = texture;
            Size = size;
            Color = color;
            Type = type;
            Properties = properties;
        }
        
        public bool Contains(Point point)
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Size, Size).Contains(point);
        }
    }
}