using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ApothecaryGame
{
    /// <summary>
    /// Manages the cauldron, including mixing ingredients and creating potions
    /// </summary>
    public class CauldronManager
    {
        private readonly Game _game;
        private readonly Random _random = new Random();
        private readonly RenderHelper _renderHelper;
        
        // Cauldron properties
        private Rectangle _cauldronBounds;
        private Texture2D _cauldronTexture;
        private List<DraggableItem> _cauldronContents = new List<DraggableItem>();
        private string _currentDescription = "Empty cauldron";
        
        // Events
        public delegate void MixtureChangedEventHandler(string description);
        public event MixtureChangedEventHandler MixtureChanged;
        
        public delegate void PotionCreatedEventHandler(string name, string effect);
        public event PotionCreatedEventHandler PotionCreated;
        
        public delegate void PotionTastedEventHandler(string taste, string effect);
        public event PotionTastedEventHandler PotionTasted;
        
        public CauldronManager(Game game, RenderHelper renderHelper)
        {
            _game = game;
            _renderHelper = renderHelper;
        }
        
        public void Initialize(Rectangle cauldronBounds)
        {
            _cauldronBounds = cauldronBounds;
        }
        
        public void LoadContent()
        {
            // Create cauldron texture
            _cauldronTexture = _renderHelper.CreateCircleTexture(_cauldronBounds.Width, Color.DarkGreen);
        }
        
        /// <summary>
        /// Adds an ingredient to the cauldron
        /// </summary>
        public void AddIngredient(DraggableItem item)
        {
            // Make a copy for the cauldron
            DraggableItem cauldronItem = new DraggableItem(
                item.Name, 
                new Vector2(_cauldronBounds.Center.X, _cauldronBounds.Center.Y), 
                item.Texture, 
                item.Size, 
                item.Color, 
                item.Type, 
                item.Properties
            );
            
            _cauldronContents.Add(cauldronItem);
            
            // Update mixture description based on the added ingredient
            UpdateMixtureDescription(item);
        }
        
        private void UpdateMixtureDescription(DraggableItem newItem)
        {
            if (_currentDescription == "Empty cauldron")
            {
                _currentDescription = $"A {_renderHelper.GetColorName(newItem.Color)} mixture with a {newItem.Properties.Split(',')[0].Trim()} aroma";
                
                // Notify listeners
                MixtureChanged?.Invoke($"Added {newItem.Name}: Started a new mixture");
            }
            else
            {
                // Get reaction based on ingredient type
                string reaction = GetReactionText(newItem);
                
                // Update description
                _currentDescription += $"\nAdded {newItem.Name}: {reaction}";
                
                // Notify listeners
                MixtureChanged?.Invoke($"Added {newItem.Name}: {reaction}");
            }
        }
        
        /// <summary>
        /// Taste the current mixture without brewing it
        /// </summary>
        public void TasteMixture()
        {
            if (IsCauldronEmpty()) return;
            
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
            
            // Notify listeners
            PotionTasted?.Invoke(tasteResult, effect);
        }
        
        /// <summary>
        /// Brew the current mixture into a potion
        /// </summary>
        public void BrewPotion()
        {
            if (IsCauldronEmpty()) return;
            
            // Determine potion properties based on ingredients
            string potionName = DeterminePotionName();
            string potionEffect = DeterminePotionEffect();
            
            // Notify listeners
            PotionCreated?.Invoke(potionName, potionEffect);
            
            // Reset cauldron
            ResetCauldron();
        }
        
        /// <summary>
        /// Resets the cauldron to empty state
        /// </summary>
        public void ResetCauldron()
        {
            _cauldronContents.Clear();
            _currentDescription = "Empty cauldron";
        }
        
        /// <summary>
        /// Checks if the cauldron is empty
        /// </summary>
        public bool IsCauldronEmpty()
        {
            return _cauldronContents.Count == 0;
        }
        
        /// <summary>
        /// Draw the cauldron and its contents
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw cauldron
            spriteBatch.Draw(_cauldronTexture, _cauldronBounds, Color.White);
            
            // Draw ingredients in the cauldron
            DrawCauldronContents(spriteBatch);
        }
        
        private void DrawCauldronContents(SpriteBatch spriteBatch)
        {
            // Draw a miniature version of each ingredient in a circular arrangement
            for (int i = 0; i < _cauldronContents.Count; i++)
            {
                var item = _cauldronContents[i];
                
                // Calculate position in cauldron (circular arrangement)
                float angle = i * MathHelper.TwoPi / Math.Max(1, _cauldronContents.Count);
                float radius = Math.Min(_cauldronBounds.Width, _cauldronBounds.Height) * 0.25f;
                
                Vector2 position = new Vector2(
                    _cauldronBounds.Center.X + (float)Math.Cos(angle) * radius,
                    _cauldronBounds.Center.Y + (float)Math.Sin(angle) * radius
                );
                
                // Draw smaller version of ingredient
                spriteBatch.Draw(item.Texture, 
                               new Rectangle((int)position.X - 10, (int)position.Y - 10, 20, 20), 
                               Color.White);
            }
        }
        
        #region Potion Creation Helpers
        
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
        
        private string DeterminePotionName()
        {
            // Generate a potion name based on the ingredients
            string[] prefixes = { "Mysterious", "Potent", "Subtle", "Vibrant", "Dubious", "Remarkable", "Curious", "Powerful" };
            string[] suffixes = { "Elixir", "Brew", "Concoction", "Potion", "Tonic", "Mixture", "Solution", "Draught" };
            
            // Use the dominant ingredient type to influence the name
            Dictionary<IngredientType, int> typeCounts = new Dictionary<IngredientType, int>();
            Color dominantColor = Color.Gray;
            float totalR = 0, totalG = 0, totalB = 0;
            
            foreach (var item in _cauldronContents)
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
            if (_cauldronContents.Count > 0)
            {
                dominantColor = new Color(
                    (byte)(totalR / _cauldronContents.Count),
                    (byte)(totalG / _cauldronContents.Count),
                    (byte)(totalB / _cauldronContents.Count)
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
            
            string colorName = _renderHelper.GetColorName(dominantColor);
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
            if (_cauldronContents.Count <= 1)
            {
                // Simple potions tend to have straightforward effects
                return positiveEffects[_random.Next(positiveEffects.Length)];
            }
            else if (_cauldronContents.Count >= 4)
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
        
        #endregion
    }
}