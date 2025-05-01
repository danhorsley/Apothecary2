using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ApothecaryGame
{
    /// <summary>
    /// Manages all UI elements and interactions
    /// </summary>
    public class UIManager
    {
        private readonly Game _game;
        private readonly RenderHelper _renderHelper;
        private SpriteFont _font;
        
        // UI areas
        private Rectangle _herbsArea;
        private Rectangle _mineralsArea;
        private Rectangle _oilsArea;
        private Rectangle _spiritsArea;
        
        // Button areas
        private Rectangle _brewButton;
        private Rectangle _tasteButton;
        private Rectangle _gatherButton;
        
        // Button textures
        private Texture2D _buttonTexture;
        
        // Tooltip handling
        private DraggableItem _hoveredItem;
        
        public UIManager(Game game, RenderHelper renderHelper)
        {
            _game = game;
            _renderHelper = renderHelper;
        }
        
        public void Initialize(int screenWidth, int screenHeight)
        {
            // Calculate UI layout based on screen size
            InitializeUILayout(screenWidth, screenHeight);
        }
        
        private void InitializeUILayout(int screenWidth, int screenHeight)
        {
            // Category areas - adjust for different screen sizes
            int categoryWidth = (int)(screenWidth * 0.4f);
            int categoryHeight = (int)(screenHeight * 0.24f);
            int horizontalSpacing = (int)(screenWidth * 0.05f);
            int verticalSpacing = (int)(screenHeight * 0.25f);
            
            _herbsArea = new Rectangle(horizontalSpacing, horizontalSpacing, 
                                      categoryWidth, categoryHeight);
            
            _mineralsArea = new Rectangle(screenWidth - horizontalSpacing - categoryWidth, 
                                         horizontalSpacing, categoryWidth, categoryHeight);
            
            _oilsArea = new Rectangle(horizontalSpacing, 
                                     screenHeight - verticalSpacing - categoryHeight, 
                                     categoryWidth, categoryHeight);
            
            _spiritsArea = new Rectangle(screenWidth - horizontalSpacing - categoryWidth, 
                                        screenHeight - verticalSpacing - categoryHeight, 
                                        categoryWidth, categoryHeight);
            
            // Action buttons
            int buttonWidth = 120;
            int buttonHeight = 40;
            int buttonY = (int)(screenHeight * 0.85f);
            int buttonSpacing = 30;
            
            _brewButton = new Rectangle((int)(screenWidth * 0.55f), buttonY, 
                                       buttonWidth, buttonHeight);
            
            _tasteButton = new Rectangle(_brewButton.Right + buttonSpacing, buttonY, 
                                        buttonWidth, buttonHeight);
            
            _gatherButton = new Rectangle(_tasteButton.Right + buttonSpacing, buttonY, 
                                         buttonWidth, buttonHeight);
        }
        
        public void LoadContent(SpriteFont font)
        {
            _font = font;
            
            // Create button texture
            _buttonTexture = _renderHelper.CreateRectangleTexture(120, 40, Color.LightGray);
        }
        
        /// <summary>
        /// Updates UI state based on mouse position
        /// </summary>
        public void Update(Point mousePosition, List<DraggableItem> items, DraggableItem draggedItem)
        {
            // Update hover state for tooltips
            _hoveredItem = null;
            
            if (draggedItem == null)
            {
                foreach (var item in items)
                {
                    if (item.Contains(mousePosition))
                    {
                        _hoveredItem = item;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Checks if a point is within the brew button
        /// </summary>
        public bool IsPointInBrewButton(Point point)
        {
            return _brewButton.Contains(point);
        }
        
        /// <summary>
        /// Checks if a point is within the taste button
        /// </summary>
        public bool IsPointInTasteButton(Point point)
        {
            return _tasteButton.Contains(point);
        }
        
        /// <summary>
        /// Checks if a point is within the gather button
        /// </summary>
        public bool IsPointInGatherButton(Point point)
        {
            return _gatherButton.Contains(point);
        }
        
        /// <summary>
        /// Gets the category area that contains the given point
        /// </summary>
        public IngredientType? GetCategoryAtPoint(Point point)
        {
            if (_herbsArea.Contains(point))
                return IngredientType.Herb;
            if (_mineralsArea.Contains(point))
                return IngredientType.Mineral;
            if (_oilsArea.Contains(point))
                return IngredientType.Oil;
            if (_spiritsArea.Contains(point))
                return IngredientType.Spirit;
            
            return null;
        }
        
        /// <summary>
        /// Gets the herbs area rectangle
        /// </summary>
        public Rectangle GetHerbsArea()
        {
            return _herbsArea;
        }
        
        /// <summary>
        /// Gets the minerals area rectangle
        /// </summary>
        public Rectangle GetMineralsArea()
        {
            return _mineralsArea;
        }
        
        /// <summary>
        /// Gets the oils area rectangle
        /// </summary>
        public Rectangle GetOilsArea()
        {
            return _oilsArea;
        }
        
        /// <summary>
        /// Gets the spirits area rectangle
        /// </summary>
        public Rectangle GetSpiritsArea()
        {
            return _spiritsArea;
        }
        
        /// <summary>
        /// Draws all UI elements
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw category areas
            DrawCategoryAreas(spriteBatch);
            
            // Draw buttons
            DrawButtons(spriteBatch);
            
            // Draw tooltip if needed
            if (_hoveredItem != null)
            {
                DrawTooltip(spriteBatch, _hoveredItem, new Point(
                    (int)_hoveredItem.Position.X, (int)_hoveredItem.Position.Y));
            }
        }
        
        private void DrawCategoryAreas(SpriteBatch spriteBatch)
        {
            // Draw each category area with appropriate color and label
            _renderHelper.DrawRectangleWithBorder(spriteBatch, _herbsArea, 
                                                Color.DarkOliveGreen * 0.5f, Color.White);
            _renderHelper.DrawRectangleWithBorder(spriteBatch, _mineralsArea, 
                                                Color.SteelBlue * 0.5f, Color.White);
            _renderHelper.DrawRectangleWithBorder(spriteBatch, _oilsArea, 
                                               Color.Sienna * 0.5f, Color.White);
            _renderHelper.DrawRectangleWithBorder(spriteBatch, _spiritsArea, 
                                               Color.MediumPurple * 0.5f, Color.White);
            
            // Draw category labels
            _renderHelper.DrawText(spriteBatch, _font, "Herbs & Spices", 
                                 new Vector2(_herbsArea.X + 10, _herbsArea.Y + 10), 
                                 Color.White, true);
            _renderHelper.DrawText(spriteBatch, _font, "Minerals", 
                                 new Vector2(_mineralsArea.X + 10, _mineralsArea.Y + 10), 
                                 Color.White, true);
            _renderHelper.DrawText(spriteBatch, _font, "Oils & Animal Products", 
                                 new Vector2(_oilsArea.X + 10, _oilsArea.Y + 10), 
                                 Color.White, true);
            _renderHelper.DrawText(spriteBatch, _font, "Spirits", 
                                 new Vector2(_spiritsArea.X + 10, _spiritsArea.Y + 10), 
                                 Color.White, true);
        }
        
        private void DrawButtons(SpriteBatch spriteBatch)
        {
            // Draw the action buttons
            spriteBatch.Draw(_buttonTexture, _brewButton, Color.White);
            spriteBatch.Draw(_buttonTexture, _tasteButton, Color.White);
            spriteBatch.Draw(_buttonTexture, _gatherButton, Color.White);
            
            // Draw button labels
            _renderHelper.DrawText(spriteBatch, _font, "BREW", 
                                 new Vector2(_brewButton.X + 35, _brewButton.Y + 10), 
                                 Color.Black);
            _renderHelper.DrawText(spriteBatch, _font, "TASTE", 
                                 new Vector2(_tasteButton.X + 30, _tasteButton.Y + 10), 
                                 Color.Black);
            _renderHelper.DrawText(spriteBatch, _font, "GATHER", 
                                 new Vector2(_gatherButton.X + 25, _gatherButton.Y + 10), 
                                 Color.Black);
        }
        
        private void DrawTooltip(SpriteBatch spriteBatch, DraggableItem item, Point position)
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
            _renderHelper.DrawRectangleWithBorder(spriteBatch, tooltipRect, 
                                               Color.Black * 0.8f, Color.White);
            
            // Draw tooltip text
            string[] lines = tooltip.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                _renderHelper.DrawText(spriteBatch, _font, lines[i], 
                                     new Vector2(tooltipRect.X + 10, tooltipRect.Y + 10 + i * 20), 
                                     Color.White);
            }
        }
        
        /// <summary>
        /// Draws an individual draggable item
        /// </summary>
        public void DrawItem(SpriteBatch spriteBatch, DraggableItem item)
        {
            // Draw item texture
            spriteBatch.Draw(item.Texture, 
                           new Rectangle((int)item.Position.X, (int)item.Position.Y, item.Size, item.Size), 
                           Color.White);
            
            // Draw item name below
            _renderHelper.DrawText(spriteBatch, _font, item.Name, 
                                 new Vector2(item.Position.X, item.Position.Y + item.Size + 5), 
                                 Color.White);
        }
    }
}