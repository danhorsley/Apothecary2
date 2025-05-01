using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ApothecaryGame
{
    /// <summary>
    /// Manages the player's notebook including mixture history,
    /// discovered effects, and recipes
    /// </summary>
    public class NotebookManager
    {
        private readonly Game _game;
        private readonly RenderHelper _renderHelper;
        
        // History tracking
        private List<string> _mixtureHistory = new List<string>();
        private int _historyScrollPosition = 0;
        private Rectangle _historyArea;
        
        // Scroll buttons
        private Rectangle _scrollUpButton;
        private Rectangle _scrollDownButton;
        
        // Recipe book
        private Dictionary<string, IngredientEffect> _knownIngredientEffects = new Dictionary<string, IngredientEffect>();
        private List<PotionRecipe> _discoveredRecipes = new List<PotionRecipe>();
        
        // Constants
        private const int MaxHistoryEntries = 100;
        private const int VisibleHistoryLines = 5;
        
        public NotebookManager(Game game, RenderHelper renderHelper)
        {
            _game = game;
            _renderHelper = renderHelper;
        }
        
        public void Initialize(Rectangle historyArea, Rectangle scrollUpButton, Rectangle scrollDownButton)
        {
            _historyArea = historyArea;
            _scrollUpButton = scrollUpButton;
            _scrollDownButton = scrollDownButton;
            
            // Initialize with empty message
            _mixtureHistory.Add("Your cauldron awaits ingredients...");
        }
        
        /// <summary>
        /// Adds an entry to the mixture history
        /// </summary>
        public void AddToHistory(string entry)
        {
            _mixtureHistory.Add(entry);
            
            // Keep a reasonable limit on history length
            if (_mixtureHistory.Count > MaxHistoryEntries)
            {
                _mixtureHistory.RemoveAt(0);
            }
            
            // Scroll to see the latest entry
            ScrollToLatestEntry();
        }
        
        /// <summary>
        /// Records a discovered potion recipe
        /// </summary>
        public void RecordRecipe(string name, List<DraggableItem> ingredients, string effect)
        {
            // Check if this recipe is already known
            bool isKnown = false;
            foreach (var recipe in _discoveredRecipes)
            {
                if (recipe.Name == name)
                {
                    isKnown = true;
                    break;
                }
            }
            
            if (!isKnown)
            {
                // Create a new recipe entry
                PotionRecipe recipe = new PotionRecipe
                {
                    Name = name,
                    Effect = effect,
                    IngredientNames = new List<string>()
                };
                
                // Record the ingredient names
                foreach (var ingredient in ingredients)
                {
                    recipe.IngredientNames.Add(ingredient.Name);
                }
                
                _discoveredRecipes.Add(recipe);
                AddToHistory($"Recipe recorded: {name}");
            }
        }
        
        /// <summary>
        /// Records an effect for a particular ingredient
        /// </summary>
        public void RecordIngredientEffect(string ingredientName, string effectDescription)
        {
            if (!_knownIngredientEffects.ContainsKey(ingredientName))
            {
                _knownIngredientEffects[ingredientName] = new IngredientEffect
                {
                    IngredientName = ingredientName,
                    ObservedEffects = new List<string>()
                };
            }
            
            // Add this effect if not already known
            if (!_knownIngredientEffects[ingredientName].ObservedEffects.Contains(effectDescription))
            {
                _knownIngredientEffects[ingredientName].ObservedEffects.Add(effectDescription);
                AddToHistory($"Learned: {ingredientName} - {effectDescription}");
            }
        }
        
        /// <summary>
        /// Gets known effects for an ingredient (if any)
        /// </summary>
        public List<string> GetKnownEffectsForIngredient(string ingredientName)
        {
            if (_knownIngredientEffects.TryGetValue(ingredientName, out var effect))
            {
                return effect.ObservedEffects;
            }
            
            return new List<string>();
        }
        
        /// <summary>
        /// Handles scrolling of history
        /// </summary>
        public void ScrollHistory(int direction)
        {
            // Calculate max scroll position
            int maxScroll = Math.Max(0, _mixtureHistory.Count - VisibleHistoryLines);
            
            // Update scroll position
            _historyScrollPosition = Math.Clamp(_historyScrollPosition + direction, 0, maxScroll);
        }
        
        /// <summary>
        /// Scrolls to see the latest history entry
        /// </summary>
        public void ScrollToLatestEntry()
        {
            // Calculate max scroll position
            int maxScroll = Math.Max(0, _mixtureHistory.Count - VisibleHistoryLines);
            
            // Scroll to bottom
            _historyScrollPosition = maxScroll;
        }
        
        /// <summary>
        /// Checks if a point is within the scroll up button
        /// </summary>
        public bool IsPointInScrollUpButton(Point point)
        {
            return _scrollUpButton.Contains(point);
        }
        
        /// <summary>
        /// Checks if a point is within the scroll down button
        /// </summary>
        public bool IsPointInScrollDownButton(Point point)
        {
            return _scrollDownButton.Contains(point);
        }
        
        /// <summary>
        /// Draws the notebook in its current state
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw history area
            _renderHelper.DrawRectangleWithBorder(spriteBatch, _historyArea, 
                                               Color.Black * 0.7f, Color.White);
            
            // Draw history title
            _renderHelper.DrawText(spriteBatch, font, "Mixing History:", 
                                 new Vector2(_historyArea.X + 10, _historyArea.Y + 10), 
                                 Color.White, true);
            
            // Draw scrollable history
            int startLine = Math.Max(0, Math.Min(_historyScrollPosition, 
                                                _mixtureHistory.Count - VisibleHistoryLines));
            
            for (int i = 0; i < VisibleHistoryLines && startLine + i < _mixtureHistory.Count; i++)
            {
                _renderHelper.DrawText(spriteBatch, font, _mixtureHistory[startLine + i], 
                                     new Vector2(_historyArea.X + 10, _historyArea.Y + 40 + i * 20), 
                                     Color.White);
            }
            
            // Draw scroll buttons
            _renderHelper.DrawRectangleWithBorder(spriteBatch, _scrollUpButton, 
                                               Color.LightGray, Color.Black);
            _renderHelper.DrawRectangleWithBorder(spriteBatch, _scrollDownButton, 
                                               Color.LightGray, Color.Black);
            
            // Draw arrow symbols on scroll buttons
            _renderHelper.DrawTriangle(spriteBatch, _scrollUpButton.Center.X, _scrollUpButton.Y + 10, 
                                     10, true, Color.Black);
            _renderHelper.DrawTriangle(spriteBatch, _scrollDownButton.Center.X, _scrollDownButton.Y + 10, 
                                     10, false, Color.Black);
        }
    }
    
    /// <summary>
    /// Stores information about a known ingredient effect
    /// </summary>
    public class IngredientEffect
    {
        public string IngredientName { get; set; }
        public List<string> ObservedEffects { get; set; }
    }
    
    /// <summary>
    /// Stores information about a discovered potion recipe
    /// </summary>
    public class PotionRecipe
    {
        public string Name { get; set; }
        public string Effect { get; set; }
        public List<string> IngredientNames { get; set; }
    }
}