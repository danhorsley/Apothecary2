using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ApothecaryGame
{
    /// <summary>
    /// Manages all ingredient-related functionality, including creation, 
    /// properties, and generation of random ingredients.
    /// </summary>
    public class IngredientManager
    {
        private readonly Game _game;
        private readonly Random _random = new Random();
        
        // Collections of ingredient data
        private readonly Dictionary<string, Texture2D> _ingredientTextures = new Dictionary<string, Texture2D>();
        private readonly List<Ingredient> _availableIngredients = new List<Ingredient>();
        
        // Ingredient data
        private readonly string[] _herbNames = { 
            "Red Herb", "Green Leaf", "Dried Root", "Mountain Flower", "Black Pepper", 
            "Thyme", "Dandelion", "Basil", "Garlic", "Ginger", "Aloe" 
        };
        
        private readonly string[] _mineralNames = { 
            "Blue Crystal", "Iron Filings", "Yellow Sulfur", "Quartz", "Salt", 
            "Copper Dust", "Diamond Shard", "Obsidian", "Gold Leaf", "Clay" 
        };
        
        private readonly string[] _oilNames = { 
            "Honey", "Fish Oil", "Bone Dust", "Beeswax", "Tallow", 
            "Snake Venom", "Spider Silk", "Turtle Shell", "Frog Egg", "Wolf's Blood" 
        };
        
        private readonly string[] _spiritNames = { 
            "Alcohol", "Rose Extract", "Dark Essence", "Morning Dew", "Dragon's Breath",
            "Moonshine", "Ghost Wisp", "Arcane Flux", "Angel's Tear", "Demon's Sweat" 
        };
        
        private readonly string[] _herbProps = { 
            "Sweet", "Bitter", "Sour", "Pungent", "Mild", "Spicy", "Earthy", "Floral" 
        };
        
        private readonly string[] _mineralProps = { 
            "Hard", "Soft", "Cold", "Hot", "Crystalline", "Powdery", "Metallic", "Sharp" 
        };
        
        private readonly string[] _oilProps = { 
            "Sticky", "Slick", "Thick", "Thin", "Fragrant", "Smelly", "Viscous", "Clear" 
        };
        
        private readonly string[] _spiritProps = { 
            "Strong", "Weak", "Potent", "Dilute", "Mysterious", "Volatile", "Pure", "Tainted" 
        };
        
        private readonly Color[] _herbColors = { 
            Color.Red, Color.Green, Color.DarkGreen, Color.ForestGreen, Color.Brown, 
            Color.DarkOliveGreen, Color.Olive, Color.LightGreen 
        };
        
        private readonly Color[] _mineralColors = { 
            Color.Blue, Color.Gray, Color.Yellow, Color.White, Color.Silver, 
            Color.Gold, Color.SkyBlue, Color.DarkGray 
        };
        
        private readonly Color[] _oilColors = { 
            Color.Gold, Color.BlanchedAlmond, Color.WhiteSmoke, Color.PaleGoldenrod, 
            Color.YellowGreen, Color.Tan, Color.Beige, Color.LemonChiffon 
        };
        
        private readonly Color[] _spiritColors = { 
            Color.SkyBlue, Color.Pink, Color.DarkViolet, Color.LightCyan, 
            Color.OrangeRed, Color.BlueViolet, Color.MediumPurple, Color.LightPink 
        };

        // Constants
        public const int IngredientSize = 40;

        public IngredientManager(Game game)
        {
            _game = game;
        }

        public void Initialize()
        {
            // Initial setup if needed
        }

        public void LoadContent()
        {
            // Create base textures for each ingredient type
            CreateBaseTextures();
        }

        private void CreateBaseTextures()
        {
            // Create textures for each ingredient type
            foreach (IngredientType type in Enum.GetValues(typeof(IngredientType)))
            {
                // Get a representative color for this type
                Color color = GetTypeBaseColor(type);
                
                // Create and store the texture
                _ingredientTextures[type.ToString()] = CreateIngredientTexture(IngredientSize, color, type);
            }
        }

        private Color GetTypeBaseColor(IngredientType type)
        {
            return type switch
            {
                IngredientType.Herb => Color.Green,
                IngredientType.Mineral => Color.SkyBlue,
                IngredientType.Oil => Color.Gold,
                IngredientType.Spirit => Color.Purple,
                _ => Color.White
            };
        }

        /// <summary>
        /// Creates texture for an ingredient based on its type
        /// </summary>
        public Texture2D CreateIngredientTexture(int size, Color color, IngredientType type)
        {
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
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

        /// <summary>
        /// Creates a specific ingredient with given properties
        /// </summary>
        public DraggableItem CreateIngredient(string name, Vector2 position, Color color, IngredientType type, string properties)
        {
            Texture2D texture = CreateIngredientTexture(IngredientSize, color, type);
            return new DraggableItem(name, position, texture, IngredientSize, color, type, properties);
        }

        /// <summary>
        /// Creates a starter set of ingredients for the game to begin with
        /// </summary>
        public List<DraggableItem> CreateStarterIngredients(Rectangle herbsArea, Rectangle mineralsArea, 
                                                         Rectangle oilsArea, Rectangle spiritsArea)
        {
            List<DraggableItem> items = new List<DraggableItem>();
            
            // Herbs & Spices
            items.Add(CreateIngredient("Red Herb", new Vector2(herbsArea.X + 30, herbsArea.Y + 30), 
                    Color.Red, IngredientType.Herb, "Sweet, Mild"));
            items.Add(CreateIngredient("Mint Leaf", new Vector2(herbsArea.X + 130, herbsArea.Y + 30), 
                    Color.Green, IngredientType.Herb, "Cool, Bitter"));
            items.Add(CreateIngredient("Cinnamon", new Vector2(herbsArea.X + 230, herbsArea.Y + 30), 
                    Color.Brown, IngredientType.Herb, "Warm, Spicy"));
            items.Add(CreateIngredient("Lavender", new Vector2(herbsArea.X + 30, herbsArea.Y + 80), 
                    Color.Purple, IngredientType.Herb, "Floral, Calming"));
            items.Add(CreateIngredient("Black Pepper", new Vector2(herbsArea.X + 130, herbsArea.Y + 80), 
                    Color.DarkSlateGray, IngredientType.Herb, "Hot, Sharp"));
            items.Add(CreateIngredient("Star Anise", new Vector2(herbsArea.X + 230, herbsArea.Y + 80), 
                    Color.Goldenrod, IngredientType.Herb, "Sweet, Licorice"));
            
            // Minerals
            items.Add(CreateIngredient("Blue Crystal", new Vector2(mineralsArea.X + 30, mineralsArea.Y + 30), 
                    Color.Blue, IngredientType.Mineral, "Hard, Cold"));
            items.Add(CreateIngredient("Iron Filings", new Vector2(mineralsArea.X + 130, mineralsArea.Y + 30), 
                    Color.Gray, IngredientType.Mineral, "Metallic, Heavy"));
            items.Add(CreateIngredient("Yellow Sulfur", new Vector2(mineralsArea.X + 230, mineralsArea.Y + 30), 
                    Color.Yellow, IngredientType.Mineral, "Pungent, Powdery"));
            items.Add(CreateIngredient("Ruby Dust", new Vector2(mineralsArea.X + 30, mineralsArea.Y + 80), 
                    Color.Red, IngredientType.Mineral, "Glittering, Warm"));
            items.Add(CreateIngredient("Sea Salt", new Vector2(mineralsArea.X + 130, mineralsArea.Y + 80), 
                    Color.White, IngredientType.Mineral, "Crystalline, Preserving"));
            
            // Oils & Animal Products
            items.Add(CreateIngredient("Honey", new Vector2(oilsArea.X + 30, oilsArea.Y + 30), 
                    Color.Gold, IngredientType.Oil, "Sweet, Sticky"));
            items.Add(CreateIngredient("Fish Oil", new Vector2(oilsArea.X + 130, oilsArea.Y + 30), 
                    Color.BlanchedAlmond, IngredientType.Oil, "Smelly, Slick"));
            items.Add(CreateIngredient("Bone Dust", new Vector2(oilsArea.X + 230, oilsArea.Y + 30), 
                    Color.WhiteSmoke, IngredientType.Oil, "Chalky, Fine"));
            items.Add(CreateIngredient("Beeswax", new Vector2(oilsArea.X + 30, oilsArea.Y + 80), 
                    Color.PaleGoldenrod, IngredientType.Oil, "Solid, Binding"));
            items.Add(CreateIngredient("Snake Venom", new Vector2(oilsArea.X + 130, oilsArea.Y + 80), 
                    Color.YellowGreen, IngredientType.Oil, "Deadly, Potent"));
            
            // Spirits
            items.Add(CreateIngredient("Alcohol", new Vector2(spiritsArea.X + 30, spiritsArea.Y + 30), 
                    Color.SkyBlue, IngredientType.Spirit, "Strong, Clear"));
            items.Add(CreateIngredient("Rose Extract", new Vector2(spiritsArea.X + 130, spiritsArea.Y + 30), 
                    Color.Pink, IngredientType.Spirit, "Floral, Potent"));
            items.Add(CreateIngredient("Dark Essence", new Vector2(spiritsArea.X + 230, spiritsArea.Y + 30), 
                    Color.DarkViolet, IngredientType.Spirit, "Mysterious, Cloudy"));
            items.Add(CreateIngredient("Morning Dew", new Vector2(spiritsArea.X + 30, spiritsArea.Y + 80), 
                    Color.LightCyan, IngredientType.Spirit, "Fresh, Revitalizing"));
            items.Add(CreateIngredient("Dragon's Breath", new Vector2(spiritsArea.X + 130, spiritsArea.Y + 80), 
                    Color.OrangeRed, IngredientType.Spirit, "Fiery, Volatile"));
            
            return items;
        }

        /// <summary>
        /// Generates a set of random new ingredients
        /// </summary>
        public List<DraggableItem> GenerateRandomIngredients(int count, Rectangle herbsArea, Rectangle mineralsArea, 
                                                         Rectangle oilsArea, Rectangle spiritsArea,
                                                         List<DraggableItem> existingItems)
        {
            List<DraggableItem> newItems = new List<DraggableItem>();
            
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
                        name = _herbNames[_random.Next(_herbNames.Length)];
                        prop1 = _herbProps[_random.Next(_herbProps.Length)];
                        prop2 = _herbProps[_random.Next(_herbProps.Length)];
                        color = _herbColors[_random.Next(_herbColors.Length)];
                        
                        // Position in herbs area
                        position = new Vector2(
                            herbsArea.X + 50 + _random.Next(herbsArea.Width - 100),
                            herbsArea.Y + 50 + _random.Next(herbsArea.Height - 100));
                        break;
                        
                    case IngredientType.Mineral:
                        name = _mineralNames[_random.Next(_mineralNames.Length)];
                        prop1 = _mineralProps[_random.Next(_mineralProps.Length)];
                        prop2 = _mineralProps[_random.Next(_mineralProps.Length)];
                        color = _mineralColors[_random.Next(_mineralColors.Length)];
                        
                        // Position in minerals area
                        position = new Vector2(
                            mineralsArea.X + 50 + _random.Next(mineralsArea.Width - 100),
                            mineralsArea.Y + 50 + _random.Next(mineralsArea.Height - 100));
                        break;
                        
                    case IngredientType.Oil:
                        name = _oilNames[_random.Next(_oilNames.Length)];
                        prop1 = _oilProps[_random.Next(_oilProps.Length)];
                        prop2 = _oilProps[_random.Next(_oilProps.Length)];
                        color = _oilColors[_random.Next(_oilColors.Length)];
                        
                        // Position in oils area
                        position = new Vector2(
                            oilsArea.X + 50 + _random.Next(oilsArea.Width - 100),
                            oilsArea.Y + 50 + _random.Next(oilsArea.Height - 100));
                        break;
                        
                    case IngredientType.Spirit:
                    default:
                        name = _spiritNames[_random.Next(_spiritNames.Length)];
                        prop1 = _spiritProps[_random.Next(_spiritProps.Length)];
                        prop2 = _spiritProps[_random.Next(_spiritProps.Length)];
                        color = _spiritColors[_random.Next(_spiritColors.Length)];
                        
                        // Position in spirits area
                        position = new Vector2(
                            spiritsArea.X + 50 + _random.Next(spiritsArea.Width - 100),
                            spiritsArea.Y + 50 + _random.Next(spiritsArea.Height - 100));
                        break;
                }
                
                // Avoid adding duplicate names
                bool isDuplicate = false;
                foreach (var item in existingItems)
                {
                    if (item.Name == name)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                
                foreach (var item in newItems)
                {
                    if (item.Name == name)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                
                if (!isDuplicate)
                {
                    newItems.Add(CreateIngredient(name, position, color, type, $"{prop1}, {prop2}"));
                }
                else
                {
                    // Try again
                    i--;
                }
            }
            
            return newItems;
        }

        /// <summary>
        /// Gets the appropriate sprite for an ingredient
        /// </summary>
        public Texture2D GetIngredientSprite(Ingredient ingredient)
        {
            if (_ingredientTextures.TryGetValue(ingredient.Type, out Texture2D texture))
            {
                return texture;
            }
            
            // If we don't have a texture for this type, create one
            Color typeColor = GetTypeBaseColor(GetIngredientTypeFromString(ingredient.Type));
            return CreateIngredientTexture(IngredientSize, typeColor, 
                                         GetIngredientTypeFromString(ingredient.Type));
        }

        private IngredientType GetIngredientTypeFromString(string typeString)
        {
            return typeString switch
            {
                "Herb" => IngredientType.Herb,
                "Crystal" => IngredientType.Mineral,
                "Mineral" => IngredientType.Mineral,
                "Oil" => IngredientType.Oil,
                "Spirit" => IngredientType.Spirit,
                "Mushroom" => IngredientType.Herb,
                _ => IngredientType.Herb
            };
        }
    }
}