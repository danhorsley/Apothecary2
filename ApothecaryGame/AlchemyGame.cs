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
        private ScreenManager _screenManager; // New screen manager
        
        // Game state
        private GameState _currentState = GameState.Loading; // Start with loading screen
        private List<DraggableItem> _items = new List<DraggableItem>();
        private DraggableItem _draggedItem;
        private Vector2 _dragOffset;
        
        // Resources
        private SpriteFont _font;
        private SpriteFont _titleFont; // New font for titles
        
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
            _screenManager = new ScreenManager(this, _renderHelper); // Initialize screen manager
            
            // Initialize base components
            _renderHelper.Initialize();
            _ingredientManager.Initialize();
            
            // Initialize UI with screen dimensions
            _uiManager.Initialize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            
            // Initialize screen manager
            _screenManager.Initialize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            
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
            
            // Try to load fonts
            try
            {
                _font = Content.Load<SpriteFont>("Font");
                
                // Try to load title font (we'll fall back to regular font if not found)
                try
                {
                    _titleFont = Content.Load<SpriteFont>("TitleFont");
                }
                catch
                {
                    // Use regular font as fallback
                    _titleFont = _font;
                }
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
            _screenManager.LoadContent(); // Load content for screen manager
            
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
            
            // Update screen manager
            GameState newState = _screenManager.Update(gameTime);
            
            // If game state has changed, handle transition
            if (newState != _currentState)
            {
                HandleStateTransition(newState);
                _currentState = newState;
            }
            
            // Only process gameplay if we're in a gameplay state
            if (_currentState == GameState.MixingScreen || 
                _currentState == GameState.ShopScreen || 
                _currentState == GameState.ExplorationScreen)
            {
                // Handle mouse input
                HandleMouse();
                
                // Update UI state
                _uiManager.Update(_currentMouseState.Position, _items, _draggedItem);
            }
            
            base.Update(gameTime);
        }
        
        private void HandleStateTransition(GameState newState)
        {
            // Handle transitions between game states
            // This is where we'd initialize specific components for each gameplay mode
            switch (newState)
            {
                case GameState.MixingScreen:
                    // Initialize mixing interface
                    _notebookManager.AddToHistory("Welcome to your alchemy lab!");
                    break;
                    
                case GameState.ShopScreen:
                    // Initialize shop interface (not implemented in prototype)
                    _notebookManager.AddToHistory("Shop mode not implemented in prototype");
                    // Return to mixing for now
                    _currentState = GameState.MixingScreen;
                    break;
                    
                case GameState.ExplorationScreen:
                    // Initialize exploration interface (not implemented in prototype)
                    _notebookManager.AddToHistory("Exploration mode not implemented in prototype");
                    // Return to mixing for now
                    _currentState = GameState.MixingScreen;
                    break;
            }
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
            
            // Draw current screen based on game state
            switch (_currentState)
            {
                case GameState.Loading:
                case GameState.MainMenu:
                    // Draw loading screen or main menu
                    _screenManager.Draw(_spriteBatch);
                    break;
                    
                case GameState.MixingScreen:
                case GameState.ShopScreen:
                case GameState.ExplorationScreen:
                    // Draw gameplay UI
                    DrawGameplayScreen(gameTime);
                    break;
            }
            
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
        
        private void DrawGameplayScreen(GameTime gameTime)
        {
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