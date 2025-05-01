using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ApothecaryGame
{
    /// <summary>
    /// Main game class that coordinates all the component systems
    /// </summary>
    public class AlchemyGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // Component managers
        private RenderHelper _renderHelper;
        private IngredientManager _ingredientManager;
        private CauldronManager _cauldronManager;
        private NotebookManager _notebookManager;
        private UIManager _uiManager;
        
        // Game state
        private List<DraggableItem> _items = new List<DraggableItem>();
        private DraggableItem _draggedItem;
        private Vector2 _dragOffset;
        
        // Resources
        private SpriteFont _font;
        
        // Input tracking
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        
        // Cauldron area
        private Rectangle _cauldronBounds;
        
        // History area
        private Rectangle _historyArea;
        private Rectangle _scrollUpButton;
        private Rectangle _scrollDownButton;
        
        public AlchemyGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Create component managers
            _renderHelper = new RenderHelper(this);
            _ingredientManager = new IngredientManager(this);
            _uiManager = new UIManager(this, _renderHelper);
            
            // Initialize base components
            _renderHelper.Initialize();
            _ingredientManager.Initialize();
            
            // Initialize UI with screen dimensions
            _uiManager.Initialize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            
            // Set up cauldron bounds
            int cauldronSize = 140;
            _cauldronBounds = new Rectangle(
                _graphics.PreferredBackBufferWidth / 2 - cauldronSize / 2,
                _graphics.PreferredBackBufferHeight / 2 - cauldronSize / 2,
                cauldronSize,
                cauldronSize
            );
            
            // Initialize cauldron manager
            _cauldronManager = new CauldronManager(this, _renderHelper);
            _cauldronManager.Initialize(_cauldronBounds);
            
            // Set up history area and scroll buttons
            _historyArea = new Rectangle(50, 620, 400, 120);
            _scrollUpButton = new Rectangle(470, 620, 40, 40);
            _scrollDownButton = new Rectangle(470, 700, 40, 40);
            
            // Initialize notebook manager
            _notebookManager = new NotebookManager(this, _renderHelper);
            _notebookManager.Initialize(_historyArea, _scrollUpButton, _scrollDownButton);
            
            // Set up event handlers
            _cauldronManager.MixtureChanged += OnMixtureChanged;
            _cauldronManager.PotionCreated += OnPotionCreated;
            _cauldronManager.PotionTasted += OnPotionTasted;
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Try to load font
            try
            {
                _font = Content.Load<SpriteFont>("Font");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading font: {ex.Message}");
                // Continue without font - the render helper can fall back to drawing rectangles
            }
            
            // Load content for all components
            _ingredientManager.LoadContent();
            _cauldronManager.LoadContent();
            _uiManager.LoadContent(_font);
            
            // Create initial ingredients
            _items = _ingredientManager.CreateStarterIngredients(
                _uiManager.GetHerbsArea(),
                _uiManager.GetMineralsArea(),
                _uiManager.GetOilsArea(),
                _uiManager.GetSpiritsArea()
            );
        }

        protected override void Update(GameTime gameTime)
        {
            // Update input states
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            
            // Exit on Escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            // Handle mouse input
            HandleMouse();
            
            // Update UI state
            _uiManager.Update(_currentMouseState.Position, _items, _draggedItem);
            
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
                // Check if clicked on an ingredient
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
                if (_uiManager.IsPointInBrewButton(mousePosition))
                {
                    _cauldronManager.BrewPotion();
                }
                else if (_uiManager.IsPointInTasteButton(mousePosition))
                {
                    _cauldronManager.TasteMixture();
                }
                else if (_uiManager.IsPointInGatherButton(mousePosition))
                {
                    GatherIngredients();
                }
                else if (_notebookManager.IsPointInScrollUpButton(mousePosition))
                {
                    _notebookManager.ScrollHistory(-1); // Scroll up
                }
                else if (_notebookManager.IsPointInScrollDownButton(mousePosition))
                {
                    _notebookManager.ScrollHistory(1); // Scroll down
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
                if (_cauldronBounds.Contains(mousePosition))
                {
                    _cauldronManager.AddIngredient(_draggedItem);
                }
                
                _draggedItem = null;
            }
        }
        
        private void GatherIngredients()
        {
            // Generate some random new ingredients
            List<DraggableItem> newItems = _ingredientManager.GenerateRandomIngredients(
                5, // Number of new ingredients
                _uiManager.GetHerbsArea(),
                _uiManager.GetMineralsArea(), 
                _uiManager.GetOilsArea(),
                _uiManager.GetSpiritsArea(),
                _items
            );
            
            // Add to existing items
            _items.AddRange(newItems);
            
            // Add entry to history
            _notebookManager.AddToHistory("You've gathered new ingredients!");
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            
            _spriteBatch.Begin();
            
            // Draw UI elements
            _uiManager.Draw(_spriteBatch);
            
            // Draw cauldron
            _cauldronManager.Draw(_spriteBatch);
            
            // Draw notebook/history
            _notebookManager.Draw(_spriteBatch, _font);
            
            // Draw all items
            foreach (var item in _items)
            {
                // If item is being dragged, draw it last (on top)
                if (item != _draggedItem)
                {
                    _uiManager.DrawItem(_spriteBatch, item);
                }
            }
            
            // Draw dragged item on top
            if (_draggedItem != null)
            {
                _uiManager.DrawItem(_spriteBatch, _draggedItem);
            }
            
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
        
        #region Event Handlers
        
        private void OnMixtureChanged(string description)
        {
            _notebookManager.AddToHistory(description);
        }
        
        private void OnPotionCreated(string name, string effect)
        {
            _notebookManager.AddToHistory($"Brewed a potion: {name}");
            _notebookManager.AddToHistory($"Possible Effect: {effect}");
        }
        
        private void OnPotionTasted(string taste, string effect)
        {
            _notebookManager.AddToHistory($"Tasted the mixture: {taste}");
            _notebookManager.AddToHistory($"Immediate Effect: {effect}");
        }
        
        #endregion
    }
}