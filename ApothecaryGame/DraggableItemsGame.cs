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
        private List<DraggableItem> _cauldronItems; // Items in the cauldron
        
        // Cauldron area in the center
        private Rectangle _cauldron;
        private Texture2D _cauldronTexture;
        private string _mixtureDescription = "Empty cauldron";
        private List<string> _mixtureHistory = new List<string>(); // History log
        private int _historyScrollPosition = 0; // For scrolling the history
        
        // Category areas
        private Rectangle _herbsArea;
        private Rectangle _mineralsArea;
        private Rectangle _oilsArea;
        private Rectangle _spiritsArea;
        
        // UI elements
        private Rectangle _brewButton;
        private Rectangle _tasteButton;
        private Rectangle _gatherButton;
        private Rectangle _scrollUpButton;
        private Rectangle _scrollDownButton;
        private Texture2D _buttonTexture;
        
        // Input tracking
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private DraggableItem _draggedItem;
        private Vector2 _dragOffset;
        
        // Placeholder textures
        private Texture2D _pixelTexture;
        
        // Random for ingredient generation
        private Random _random = new Random();
        
        public DraggableItemsGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            // Set larger window size
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Set up the category areas
            _herbsArea = new Rectangle(50, 50, 400, 180);
            _mineralsArea = new Rectangle(550, 50, 400, 180);
            _oilsArea = new Rectangle(50, 380, 400, 180);
            _spiritsArea = new Rectangle(550, 380, 400, 180);
            
            // Set up the cauldron area
            _cauldron = new Rectangle(430, 260, 140, 140);
            
            // Set up the buttons
            _brewButton = new Rectangle(600, 620, 120, 40);
            _tasteButton = new Rectangle(750, 620, 120, 40);
            _gatherButton = new Rectangle(850, 620, 120, 40);
            _scrollUpButton = new Rectangle(470, 620, 40, 40);
            _scrollDownButton = new Rectangle(470, 700, 40, 40);
            
            _items = new List<DraggableItem>();
            _cauldronItems = new List<DraggableItem>();
            
            // Initialize mixture history with empty message
            _mixtureHistory.Add("Your cauldron awaits ingredients...");
            
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
            _cauldronTexture = CreateCircleTexture(140, Color.DarkGreen);
            _buttonTexture = CreateRectangleTexture(120, 40, Color.LightGray);
            
            // Create sample ingredients
            CreateSampleIngredients();
        }
        
        private void CreateSampleIngredients()
        {
            // Clear existing items
            _items.Clear();
            
            // Herbs & Spices
            AddIngredient("Red Herb", new Vector2(80, 80), Color.Red, IngredientType.Herb, "Sweet, Mild");
            AddIngredient("Mint Leaf", new Vector2(180, 80), Color.Green, IngredientType.Herb, "Cool, Bitter");
            AddIngredient("Cinnamon", new Vector2(280, 80), Color.Brown, IngredientType.Herb, "Warm, Spicy");
            AddIngredient("Lavender", new Vector2(80, 130), Color.Purple, IngredientType.Herb, "Floral, Calming");
            AddIngredient("Black Pepper", new Vector2(180, 130), Color.DarkSlateGray, IngredientType.Herb, "Hot, Sharp");
            AddIngredient("Star Anise", new Vector2(280, 130), Color.Goldenrod, IngredientType.Herb, "Sweet, Licorice");
            
            // Minerals
            AddIngredient("Blue Crystal", new Vector2(580, 80), Color.Blue, IngredientType.Mineral, "Hard, Cold");
            AddIngredient("Iron Filings", new Vector2(680, 80), Color.Gray, IngredientType.Mineral, "Metallic, Heavy");
            AddIngredient("Yellow Sulfur", new Vector2(780, 80), Color.Yellow, IngredientType.Mineral, "Pungent, Powdery");
            AddIngredient("Ruby Dust", new Vector2(580, 130), Color.Red, IngredientType.Mineral, "Glittering, Warm");
            AddIngredient("Sea Salt", new Vector2(680, 130), Color.White, IngredientType.Mineral, "Crystalline, Preserving");
            
            // Oils & Animal Products
            AddIngredient("Honey", new Vector2(80, 410), Color.Gold, IngredientType.Oil, "Sweet, Sticky");
            AddIngredient("Fish Oil", new Vector2(180, 410), Color.BlanchedAlmond, IngredientType.Oil, "Smelly, Slick");
            AddIngredient("Bone Dust", new Vector2(280, 410), Color.WhiteSmoke, IngredientType.Oil, "Chalky, Fine");
            AddIngredient("Beeswax", new Vector2(80, 460), Color.PaleGoldenrod, IngredientType.Oil, "Solid, Binding");
            AddIngredient("Snake Venom", new Vector2(180, 460), Color.YellowGreen, IngredientType.Oil, "Deadly, Potent");
            
            // Spirits
            AddIngredient("Alcohol", new Vector2(580, 410), Color.SkyBlue, IngredientType.Spirit, "Strong, Clear");
            AddIngredient("Rose Extract", new Vector2(680, 410), Color.Pink, IngredientType.Spirit, "Floral, Potent");
            AddIngredient("Dark Essence", new Vector2(780, 410), Color.DarkViolet, IngredientType.Spirit, "Mysterious, Cloudy");
            AddIngredient("Morning Dew", new Vector2(580, 460), Color.LightCyan, IngredientType.Spirit, "Fresh, Revitalizing");
            AddIngredient("Dragon's Breath", new Vector2(680, 460), Color.OrangeRed, IngredientType.Spirit, "Fiery, Volatile");
        }
        
        private void AddIngredient(string name, Vector2 position, Color color, IngredientType type, string properties)
        {
            int size = 40;
            Texture2D texture = CreateIngredientTexture(size, color, type);
            _items.Add(new DraggableItem(name, position, texture, size, color, type, properties));
        }
        
        private void GatherIngredients()
        {
            // Generate some random new ingredients
            AddRandomIngredients(5);
            
            // Add a log entry
            AddToMixtureHistory("You've gathered new ingredients!");
        }
        
        private void AddRandomIngredients(int count)
        {
            string[] herbNames = { "Red Herb", "Green Leaf", "Dried Root", "Mountain Flower", "Black Pepper", 
                                  "Thyme", "Dandelion", "Basil", "Garlic", "Ginger", "Aloe" };
            string[] mineralNames = { "Blue Crystal", "Iron Filings", "Yellow Sulfur", "Quartz", "Salt", 
                                     "Copper Dust", "Diamond Shard", "Obsidian", "Gold Leaf", "Clay" };
            string[] oilNames = { "Honey", "Fish Oil", "Bone Dust", "Beeswax", "Tallow", 
                                 "Snake Venom", "Spider Silk", "Turtle Shell", "Frog Egg", "Wolf's Blood" };
            string[] spiritNames = { "Alcohol", "Rose Extract", "Dark Essence", "Morning Dew", "Dragon's Breath",
                                    "Moonshine", "Ghost Wisp", "Arcane Flux", "Angel's Tear", "Demon's Sweat" };
            
            string[] herbProps = { "Sweet", "Bitter", "Sour", "Pungent", "Mild", "Spicy", "Earthy", "Floral" };
            string[] mineralProps = { "Hard", "Soft", "Cold", "Hot", "Crystalline", "Powdery", "Metallic", "Sharp" };
            string[] oilProps = { "Sticky", "Slick", "Thick", "Thin", "Fragrant", "Smelly", "Viscous", "Clear" };
            string[] spiritProps = { "Strong", "Weak", "Potent", "Dilute", "Mysterious", "Volatile", "Pure", "Tainted" };
            
            Color[] herbColors = { Color.Red, Color.Green, Color.DarkGreen, Color.ForestGreen, Color.Brown, 
                                  Color.DarkOliveGreen, Color.Olive, Color.LightGreen };
            Color[] mineralColors = { Color.Blue, Color.Gray, Color.Yellow, Color.White, Color.Silver, 
                                     Color.Gold, Color.SkyBlue, Color.DarkGray };
            Color[] oilColors = { Color.Gold, Color.BlanchedAlmond, Color.WhiteSmoke, Color.PaleGoldenrod, 
                                 Color.YellowGreen, Color.Tan, Color.Beige, Color.LemonChiffon };
            Color[] spiritColors = { Color.SkyBlue, Color.Pink, Color.DarkViolet, Color.LightCyan, 
                                    Color.OrangeRed, Color.BlueViolet, Color.MediumPurple, Color.LightPink };
            
            for (int i = 0; i < count; i++)
            {
                // Decide what type of ingredient to create
                IngredientType type = (IngredientType)_random.Next(4);
                string name;
                string prop1;
                string prop2;
                Color color;
                Vector2 position;
                
                switch (type)
                {
                    case IngredientType.Herb:
                        name = herbNames[_random.Next(herbNames.Length)];
                        prop1 = herbProps[_random.Next(herbProps.Length)];
                        prop2 = herbProps[_random.Next(herbProps.Length)];
                        color = herbColors[_random.Next(herbColors.Length)];
                        
                        // Position in herbs area
                        position = new Vector2(
                            _herbsArea.X + 50 + _random.Next(_herbsArea.Width - 100),
                            _herbsArea.Y + 50 + _random.Next(_herbsArea.Height - 100));
                        break;
                        
                    case IngredientType.Mineral:
                        name = mineralNames[_random.Next(mineralNames.Length)];
                        prop1 = mineralProps[_random.Next(mineralProps.Length)];
                        prop2 = mineralProps[_random.Next(mineralProps.Length)];
                        color = mineralColors[_random.Next(mineralColors.Length)];
                        
                        // Position in minerals area
                        position = new Vector2(
                            _mineralsArea.X + 50 + _random.Next(_mineralsArea.Width - 100),
                            _mineralsArea.Y + 50 + _random.Next(_mineralsArea.Height - 100));
                        break;
                        
                    case IngredientType.Oil:
                        name = oilNames[_random.Next(oilNames.Length)];
                        prop1 = oilProps[_random.Next(oilProps.Length)];
                        prop2 = oilProps[_random.Next(oilProps.Length)];
                        color = oilColors[_random.Next(oilColors.Length)];
                        
                        // Position in oils area
                        position = new Vector2(
                            _oilsArea.X + 50 + _random.Next(_oilsArea.Width - 100),
                            _oilsArea.Y + 50 + _random.Next(_oilsArea.Height - 100));
                        break;
                        
                    case IngredientType.Spirit:
                    default:
                        name = spiritNames[_random.Next(spiritNames.Length)];
                        prop1 = spiritProps[_random.Next(spiritProps.Length)];
                        prop2 = spiritProps[_random.Next(spiritProps.Length)];
                        color = spiritColors[_random.Next(spiritColors.Length)];
                        
                        // Position in spirits area
                        position = new Vector2(
                            _spiritsArea.X + 50 + _random.Next(_spiritsArea.Width - 100),
                            _spiritsArea.Y + 50 + _random.Next(_spiritsArea.Height - 100));
                        break;
                }
                
                // Avoid adding duplicate names
                bool isDuplicate = false;
                foreach (var item in _items)
                {
                    if (item.Name == name)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                
                if (!isDuplicate)
                {
                    AddIngredient(name, position, color, type, $"{prop1}, {prop2}");
                }
                else
                {
                    // Try again
                    i--;
                }
            }
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
                else if (_gatherButton.Contains(mousePosition))
                {
                    GatherIngredients();
                }
                else if (_scrollUpButton.Contains(mousePosition))
                {
                    ScrollHistory(-1); // Scroll up
                }
                else if (_scrollDownButton.Contains(mousePosition))
                {
                    ScrollHistory(1); // Scroll down
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
        
        private void ScrollHistory(int direction)
        {
            // Calculate max scroll position (depends on how many lines we can display)
            int maxLines = 8; // Number of visible lines in history panel
            int maxScroll = Math.Max(0, _mixtureHistory.Count - maxLines);
            
            // Update scroll position
            _historyScrollPosition = Math.Clamp(_historyScrollPosition + direction, 0, maxScroll);
        }
        
        private void AddToCauldron(DraggableItem item)
        {
            // Add the item to cauldron items list (make a copy)
            _cauldronItems.Add(new DraggableItem(
                item.Name, 
                new Vector2(_cauldron.X + _cauldron.Width / 2, _cauldron.Y + _cauldron.Height / 2), 
                item.Texture, 
                item.Size, 
                item.Color, 
                item.Type, 
                item.Properties
            ));
            
            // Update mixture description based on the added ingredient
            if (_mixtureDescription == "Empty cauldron")
            {
                _mixtureDescription = $"A {GetColorName(item.Color)} mixture with a {item.Properties.Split(',')[0].Trim()} aroma";
                AddToMixtureHistory($"Added {item.Name}: Started a new mixture");
            }
            else
            {
                // Change description based on ingredient type
                switch (item.Type)
                {
                    case IngredientType.Herb:
                        _mixtureDescription += $"\nAdded {item.Name}: The mixture changes color slightly";
                        AddToMixtureHistory($"Added {item.Name}: The mixture shifts to a more {GetColorName(item.Color)} hue");
                        break;
                    case IngredientType.Mineral:
                        _mixtureDescription += $"\nAdded {item.Name}: The mixture begins to bubble";
                        AddToMixtureHistory($"Added {item.Name}: {GetReactionText(item)}");
                        break;
                    case IngredientType.Oil:
                        _mixtureDescription += $"\nAdded {item.Name}: The texture becomes smoother";
                        AddToMixtureHistory($"Added {item.Name}: {GetReactionText(item)}");
                        break;
                    case IngredientType.Spirit:
                        _mixtureDescription += $"\nAdded {item.Name}: A strong aroma emerges";
                        AddToMixtureHistory($"Added {item.Name}: {GetReactionText(item)}");
                        break;
                }
            }
            
            // Scroll to see the latest entry
            ScrollToLatestEntry();
        }
        
        private string GetReactionText(DraggableItem item)
        {
            // Get more varied and interesting reaction texts
            string[] herbReactions = {
                "The mixture absorbs the herb, taking on its scent",
                "Steam rises in small, fragrant puffs",
                "The concoction swirls gently and changes color",
                "The herb dissolves, releasing tiny bubbles",
                "A sweet aroma fills the air around the cauldron"
            };
            
            string[] mineralReactions = {
                "The mixture bubbles vigorously around the mineral",
                "A bright flash occurs as the mineral dissolves",
                "The liquid becomes slightly more viscous",
                "Small sparks appear at the surface of the mixture",
                "The cauldron grows slightly warmer to the touch"
            };
            
            string[] oilReactions = {
                "The texture becomes noticeably smoother",
                "Oily patterns swirl across the surface",
                "The mixture thickens to a silky consistency",
                "The liquid glistens with a new sheen",
                "The contents blend into a more homogeneous state"
            };
            
            string[] spiritReactions = {
                "A strong, distinctive aroma fills the air",
                "The mixture briefly glows with inner light",
                "The liquid clarifies slightly, becoming more transparent",
                "A subtle mist rises from the cauldron",
                "The potion changes subtly in viscosity"
            };
            
            switch (item.Type)
            {
                case IngredientType.Herb:
                    return herbReactions[_random.Next(herbReactions.Length)];
                case IngredientType.Mineral:
                    return mineralReactions[_random.Next(mineralReactions.Length)];
                case IngredientType.Oil:
                    return oilReactions[_random.Next(oilReactions.Length)];
                case IngredientType.Spirit:
                default:
                    return spiritReactions[_random.Next(spiritReactions.Length)];
            }
        }
        
        private void AddToMixtureHistory(string entry)
        {
            _mixtureHistory.Add(entry);
            
            // Keep a reasonable limit on history length
            if (_mixtureHistory.Count > 100)
            {
                _mixtureHistory.RemoveAt(0);
            }
        }
        
        private void ScrollToLatestEntry()
        {
            // Calculate max scroll position
            int maxLines = 8; // Number of visible lines in history panel
            int maxScroll = Math.Max(0, _mixtureHistory.Count - maxLines);
            
            // Scroll to bottom
            _historyScrollPosition = maxScroll;
        }
        
        private void BrewPotion()
        {
            if (_mixtureDescription != "Empty cauldron" && _cauldronItems.Count > 0)
            {
                // Determine potion result based on ingredients
                string potionName = DeterminePotionName();
                string potionEffect = DeterminePotionEffect();
                
                AddToMixtureHistory($"Brewed a potion: {potionName}");
                AddToMixtureHistory($"Possible Effect: {potionEffect}");
                
                // Reset cauldron
                _mixtureDescription = "Empty cauldron";
                _cauldronItems.Clear();
                
                // Scroll to see the result
                ScrollToLatestEntry();
            }
            else
            {
                AddToMixtureHistory("The cauldron is empty. Nothing to brew.");
            }
        }
        
        private string DeterminePotionName()
        {
            // Generate a potion name based on the ingredients
            string[] prefixes = { "Mysterious", "Potent", "Subtle", "Vibrant", "Dubious", "Remarkable", "Curious", "Powerful" };
            string[] suffixes = { "Elixir", "Brew", "Concoction", "Potion", "Tonic", "Mixture", "Solution", "Draught" };
            
            // Use the dominant ingredient type to influence the name
            Dictionary<IngredientType, int> typeCounts = new Dictionary<IngredientType, int>();
            Color dominantColor = Color.Gray;
            float totalR = 0, totalG = 0, totalB = 0;
            
            foreach (var item in _cauldronItems)
            {
                if (!typeCounts.ContainsKey(item.Type))
                {
                    typeCounts[item.Type] = 0;
                }
                typeCounts[item.Type]++;
                
                // Add to color totals
                totalR += item.Color.R;
                totalG += item.Color.G;
                totalB += item.Color.B;
            }
            
            // Get dominant color
            if (_cauldronItems.Count > 0)
            {
                dominantColor = new Color(
                    (byte)(totalR / _cauldronItems.Count),
                    (byte)(totalG / _cauldronItems.Count),
                    (byte)(totalB / _cauldronItems.Count)
                );
            }
            
            // Get dominant type
            IngredientType dominantType = IngredientType.Herb;
            int maxCount = 0;
            foreach (var pair in typeCounts)
            {
                if (pair.Value > maxCount)
                {
                    maxCount = pair.Value;
                    dominantType = pair.Key;
                }
            }
            
            // Get type-specific names
            string typeElement = "";
            switch (dominantType)
            {
                case IngredientType.Herb:
                    string[] herbElements = { "Vitality", "Restoration", "Nature", "Growth", "Healing" };
                    typeElement = herbElements[_random.Next(herbElements.Length)];
                    break;
                case IngredientType.Mineral:
                    string[] mineralElements = { "Fortitude", "Stability", "Earth", "Strength", "Endurance" };
                    typeElement = mineralElements[_random.Next(mineralElements.Length)];
                    break;
                case IngredientType.Oil:
                    string[] oilElements = { "Slickness", "Fluidity", "Viscosity", "Preservation", "Coating" };
                    typeElement = oilElements[_random.Next(oilElements.Length)];
                    break;
                case IngredientType.Spirit:
                    string[] spiritElements = { "Essence", "Soul", "Spirit", "Mind", "Awareness" };
                    typeElement = spiritElements[_random.Next(spiritElements.Length)];
                    break;
            }
            
            string colorName = GetColorName(dominantColor);
            string prefix = prefixes[_random.Next(prefixes.Length)];
            string suffix = suffixes[_random.Next(suffixes.Length)];
            
            return $"{prefix} {colorName} {typeElement} {suffix}";
        }
        
        private string DeterminePotionEffect()
        {
            // Simple random effect generation
            // In a full implementation, this would consider ingredient properties
            string[] positiveEffects = {
                "Restores health gradually",
                "Enhances strength temporarily",
                "Improves vision in darkness",
                "Protects against cold",
                "Increases speed and agility",
                "Calms emotions and focuses mind",
                "Bestows temporary luck",
                "Allows underwater breathing",
                "Makes user lighter than air",
                "Enhances magical abilities"
            };
            
            string[] negativeEffects = {
                "Causes mild nausea",
                "Creates disorienting hallucinations",
                "Temporarily reduces strength",
                "Makes user speak in rhymes",
                "Skin turns unusual color",
                "Causes uncontrollable laughter",
                "Makes user smell like rotten eggs",
                "Temporarily causes hair loss",
                "Induces excessive sleepiness",
                "Shrinks user slightly"
            };
            
            string[] mixedEffects = {
                "Restores health but makes user dizzy",
                "Enhances strength but reduces intelligence",
                "Improves vision but causes color blindness",
                "Protects against cold but makes user overheat",
                "Increases speed but makes user clumsy",
                "Calms emotions but causes temporary amnesia",
                "Bestows luck but attracts minor misfortunes",
                "Allows underwater breathing but skin becomes scaly",
                "Makes user lighter but causes hiccups",
                "Enhances magic but drains energy"
            };
            
            // Determine if effect is positive, negative, or mixed based on ingredient combinations
            if (_cauldronItems.Count <= 1)
            {
                // Simple potions tend to have straightforward effects
                return positiveEffects[_random.Next(positiveEffects.Length)];
            }
            else if (_cauldronItems.Count >= 4)
            {
                // Complex potions often have mixed effects
                return mixedEffects[_random.Next(mixedEffects.Length)];
            }
            else
            {
                // Medium complexity potions could go either way
                return _random.Next(2) == 0 ? 
                    positiveEffects[_random.Next(positiveEffects.Length)] : 
                    negativeEffects[_random.Next(negativeEffects.Length)];
            }
        }
        
        private void TastePotion()
        {
            if (_mixtureDescription != "Empty cauldron" && _cauldronItems.Count > 0)
            {
                // Generate taste result
                string[] tasteResults = {
                    "Bitter with a sweet aftertaste",
                    "Sharp and spicy, numbing the tongue",
                    "Surprisingly pleasant, like berries",
                    "Sour with a metallic tang",
                    "Earthy and rich, somewhat like tea",
                    "Utterly foul, making you gag",
                    "Minty and refreshing",
                    "Hot and spicy, burning the throat",
                    "Sweet and syrupy",
                    "Tasteless but leaves a tingling sensation"
                };
                
                string[] immediateEffects = {
                    "A slight warmth spreads through your body",
                    "Your vision sharpens momentarily",
                    "A brief wave of dizziness passes over you",
                    "Your fingertips tingle pleasantly",
                    "Your breathing becomes deeper and easier",
                    "A strange floating sensation affects you briefly",
                    "Your hearing becomes momentarily more sensitive",
                    "Your mouth feels slightly numb",
                    "A brief surge of energy courses through you",
                    "Your thoughts seem clearer for a moment"
                };
                
                string tasteResult = tasteResults[_random.Next(tasteResults.Length)];
                string effect = immediateEffects[_random.Next(immediateEffects.Length)];
                
                AddToMixtureHistory($"Tasted the mixture: {tasteResult}");
                AddToMixtureHistory($"Immediate Effect: {effect}");
                
                // Scroll to see the result
                ScrollToLatestEntry();
            }
            else
            {
                AddToMixtureHistory("The cauldron is empty. Nothing to taste.");
            }
        }
        
        private string GetColorName(Color color)
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
            if (color.R > 100 && color.G > 100 && color.B > 100 && color.R < 200 && color.G < 200 && color.B < 200) return "gray";
            if (color.R > 200 && color.G > 150 && color.B < 100) return "golden";
            
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
            DrawText("Herbs & Spices", new Vector2(_herbsArea.X + 10, _herbsArea.Y + 10), Color.White, true);
            DrawText("Minerals", new Vector2(_mineralsArea.X + 10, _mineralsArea.Y + 10), Color.White, true);
            DrawText("Oils & Animal Products", new Vector2(_oilsArea.X + 10, _oilsArea.Y + 10), Color.White, true);
            DrawText("Spirits", new Vector2(_spiritsArea.X + 10, _spiritsArea.Y + 10), Color.White, true);
            
            // Draw cauldron
            _spriteBatch.Draw(_cauldronTexture, _cauldron, Color.White);
            
            // Draw cauldron contents (miniature versions of ingredients)
            foreach (var item in _cauldronItems)
            {
                // Calculate position in cauldron (circular arrangement)
                float angle = _cauldronItems.IndexOf(item) * MathHelper.TwoPi / Math.Max(1, _cauldronItems.Count);
                float radius = Math.Min(_cauldron.Width, _cauldron.Height) * 0.25f;
                
                Vector2 position = new Vector2(
                    _cauldron.X + _cauldron.Width / 2 + (float)Math.Cos(angle) * radius,
                    _cauldron.Y + _cauldron.Height / 2 + (float)Math.Sin(angle) * radius
                );
                
                // Draw smaller version of ingredient
                _spriteBatch.Draw(item.Texture, 
                                 new Rectangle((int)position.X - 10, (int)position.Y - 10, 20, 20), 
                                 Color.White);
            }
            
            // Draw buttons
            _spriteBatch.Draw(_buttonTexture, _brewButton, Color.White);
            _spriteBatch.Draw(_buttonTexture, _tasteButton, Color.White);
            _spriteBatch.Draw(_buttonTexture, _gatherButton, Color.White);
            
            // Draw buttons for scrolling
            DrawRectangleWithBorder(_scrollUpButton, Color.LightGray, Color.Black);
            DrawRectangleWithBorder(_scrollDownButton, Color.LightGray, Color.Black);
            
            // Draw scroll symbols on scroll buttons (using simple triangles instead of Unicode arrows)
            // Up arrow
            DrawTriangle(_scrollUpButton.X + 20, _scrollUpButton.Y + 25, 10, true, Color.Black);
            // Down arrow
            DrawTriangle(_scrollDownButton.X + 20, _scrollDownButton.Y + 15, 10, false, Color.Black);
            
            DrawText("BREW", new Vector2(_brewButton.X + 35, _brewButton.Y + 10), Color.Black);
            DrawText("TASTE", new Vector2(_tasteButton.X + 30, _tasteButton.Y + 10), Color.Black);
            DrawText("GATHER", new Vector2(_gatherButton.X + 25, _gatherButton.Y + 10), Color.Black);
            
            // Draw mixture description/history
            Rectangle historyRect = new Rectangle(50, 620, 400, 120);
            DrawRectangleWithBorder(historyRect, Color.Black * 0.7f, Color.White);
            DrawText("Mixing History:", new Vector2(60, 630), Color.White, true);
            
            // Draw scrollable history
            int visibleLines = 5;
            int startLine = Math.Max(0, Math.Min(_historyScrollPosition, _mixtureHistory.Count - visibleLines));
            
            for (int i = 0; i < visibleLines && startLine + i < _mixtureHistory.Count; i++)
            {
                DrawText(_mixtureHistory[startLine + i], new Vector2(60, 660 + i * 20), Color.White);
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
        
        // Draw a simple triangle for arrow buttons
        private void DrawTriangle(int centerX, int centerY, int size, bool pointUp, Color color)
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
                        _spriteBatch.Draw(_pixelTexture, 
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
                        _spriteBatch.Draw(_pixelTexture, 
                                         new Rectangle(leftX + x, centerY + y, 1, 1), 
                                         color);
                    }
                }
            }
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
        
        private void DrawText(string text, Vector2 position, Color color, bool isBold = false)
        {
            if (_font != null)
            {
                // Draw text with font
                _spriteBatch.DrawString(_font, text, position, color);
                
                // Draw bold text (crude approach - draw twice with slight offset)
                if (isBold)
                {
                    _spriteBatch.DrawString(_font, text, position + new Vector2(1, 0), color);
                }
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