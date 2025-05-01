using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using FontStashSharp;
using System;

namespace ApothecaryGame
{
    public class UIManager
    {
        private Desktop _desktop = null!;
        private Panel _mainPanel = null!;
        private Label _stateLabel = null!;
        private Label _goldLabel = null!;
        private Label _healthLabel = null!;

        private Game1 _game;
        private SpriteFontBase _defaultFont = null!;

        public UIManager(Game1 game)
        {
            _game = game;

            // Initialize Myra
            MyraEnvironment.Game = game;
        }

        public void LoadContent()
        {
            try
            {
                // Create a default FontSystem for Myra
                var fontSystem = new FontSystem();
                fontSystem.AddFont(TitleContainer.OpenStream("Content/Font.ttf"));
                _defaultFont = fontSystem.GetFont(16);

                // Create UI
                CreateUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading UI content: {ex.Message}");
                // Create a minimal UI anyway
                CreateMinimalUI();
            }
        }

        private void CreateMinimalUI()
        {
            // Create main panel
            _mainPanel = new Panel
            {
                Width = 800,
                Height = 600
            };

            // Create desktop
            _desktop = new Desktop();
            _desktop.Root = _mainPanel;
        }

        private void CreateUI()
        {
            // Create main panel
            _mainPanel = new Panel
            {
                Width = 800,
                Height = 600
            };

            // Create labels for player stats
            _stateLabel = new Label
            {
                Text = "Current State: Shop",
                Font = _defaultFont,
                Left = 10,
                Top = 10
            };

            _goldLabel = new Label
            {
                Text = "Gold: 100",
                Font = _defaultFont,
                Left = 10,
                Top = 40,
                TextColor = Color.Yellow
            };

            _healthLabel = new Label
            {
                Text = "Health: 100",
                Font = _defaultFont,
                Left = 10,
                Top = 70,
                TextColor = Color.Red
            };

            // Add labels to panel
            _mainPanel.Widgets.Add(_stateLabel);
            _mainPanel.Widgets.Add(_goldLabel);
            _mainPanel.Widgets.Add(_healthLabel);

            // Create desktop
            _desktop = new Desktop();
            _desktop.Root = _mainPanel;
        }

        public void UpdateUI(GameState currentState, Player player)
        {
            if (_stateLabel != null)
                _stateLabel.Text = $"Current State: {currentState}";

            if (_goldLabel != null)
                _goldLabel.Text = $"Gold: {player.Gold}";

            if (_healthLabel != null)
                _healthLabel.Text = $"Health: {player.Health}";
        }

        public void Draw()
        {
            _desktop.Render();
        }

        // Add shop UI
        public void CreateShopUI(Customer customer)
        {
            // Clear existing widgets except for the basic stats
            while (_mainPanel.Widgets.Count > 3)
            {
                _mainPanel.Widgets.RemoveAt(3);
            }

            // Customer info
            var customerLabel = new Label
            {
                Text = $"Customer: {customer.Name} ({customer.Type})",
                Font = _defaultFont,
                Left = 200,
                Top = 150
            };

            var needLabel = new Label
            {
                Text = $"Needs: {customer.Need} potion",
                Font = _defaultFont,
                Left = 200,
                Top = 180
            };

            var rewardLabel = new Label
            {
                Text = $"Reward: {customer.Reward} gold",
                Font = _defaultFont,
                Left = 200,
                Top = 210,
                TextColor = Color.Yellow
            };

            // Add to panel
            _mainPanel.Widgets.Add(customerLabel);
            _mainPanel.Widgets.Add(needLabel);
            _mainPanel.Widgets.Add(rewardLabel);

            // Create buttons for state changes
            var mixButton = new Button();
            var mixLabel = new Label
            {
                Text = "Go to Mixing",
                Font = _defaultFont
            };
            mixButton.Content = mixLabel;
            mixButton.Left = 200;
            mixButton.Top = 300;
            mixButton.Width = 150;
            mixButton.Click += (s, e) => _game.ChangeState(GameState.Mixing);

            var exploreButton = new Button();
            var exploreLabel = new Label
            {
                Text = "Go Exploring",
                Font = _defaultFont
            };
            exploreButton.Content = exploreLabel;
            exploreButton.Left = 400;
            exploreButton.Top = 300;
            exploreButton.Width = 150;
            exploreButton.Click += (s, e) => _game.ChangeState(GameState.Exploration);

            _mainPanel.Widgets.Add(mixButton);
            _mainPanel.Widgets.Add(exploreButton);
        }

        // Add potion mixing UI
        public void CreateMixingUI(Player player, RecipeBook recipeBook)
        {
            // Clear existing widgets except for the basic stats
            while (_mainPanel.Widgets.Count > 3)
            {
                _mainPanel.Widgets.RemoveAt(3);
            }

            // Title
            var titleLabel = new Label
            {
                Text = "Potion Mixing",
                Font = _defaultFont,
                Left = 350,
                Top = 120
            };

            _mainPanel.Widgets.Add(titleLabel);

            // Ingredient list
            var inventoryLabel = new Label
            {
                Text = "Your Ingredients:",
                Font = _defaultFont,
                Left = 200,
                Top = 160
            };

            _mainPanel.Widgets.Add(inventoryLabel);

            int y = 190;
            for (int i = 0; i < player.Inventory.Count; i++)
            {
                var ingredient = player.Inventory[i];
                var ingredientButton = new Button();
                var ingredientLabel = new Label
                {
                    Text = $"{ingredient.Name} ({ingredient.Type}, Rarity: {ingredient.Rarity})",
                    Font = _defaultFont
                };
                ingredientButton.Content = ingredientLabel;
                ingredientButton.Left = 200;
                ingredientButton.Top = y;
                ingredientButton.Width = 250;

                int index = i; // Capture for lambda
                ingredientButton.Click += (s, e) => _game.SelectIngredient(index);

                _mainPanel.Widgets.Add(ingredientButton);
                y += 30;
            }

            // Recipe book
            var recipeLabel = new Label
            {
                Text = "Known Recipes:",
                Font = _defaultFont,
                Left = 500,
                Top = 160
            };

            _mainPanel.Widgets.Add(recipeLabel);

            y = 190;
            foreach (var recipe in recipeBook.KnownRecipes)
            {
                var recipeInfo = new Label
                {
                    Text = $"{recipe.Effect} (Value: {recipe.Value})",
                    Font = _defaultFont,
                    Left = 500,
                    Top = y
                };

                _mainPanel.Widgets.Add(recipeInfo);
                y += 30;
            }

            // Navigation buttons
            var shopButton = new Button();
            var shopLabel = new Label
            {
                Text = "Return to Shop",
                Font = _defaultFont
            };
            shopButton.Content = shopLabel;
            shopButton.Left = 200;
            shopButton.Top = 500;
            shopButton.Width = 150;
            shopButton.Click += (s, e) => _game.ChangeState(GameState.Shop);

            var exploreButton = new Button();
            var exploreLabel = new Label
            {
                Text = "Go Exploring",
                Font = _defaultFont
            };
            exploreButton.Content = exploreLabel;
            exploreButton.Left = 400;
            exploreButton.Top = 500;
            exploreButton.Width = 150;
            exploreButton.Click += (s, e) => _game.ChangeState(GameState.Exploration);

            _mainPanel.Widgets.Add(shopButton);
            _mainPanel.Widgets.Add(exploreButton);
        }

        // Create exploration UI
        public void CreateExplorationUI(Forest forest, Player player)
        {
            // Clear existing widgets except for the basic stats
            while (_mainPanel.Widgets.Count > 3)
            {
                _mainPanel.Widgets.RemoveAt(3);
            }

            // Title
            var titleLabel = new Label
            {
                Text = "Forest Exploration",
                Font = _defaultFont,
                Left = 350,
                Top = 120
            };

            _mainPanel.Widgets.Add(titleLabel);

            // Return button
            var returnButton = new Button();
            var returnLabel = new Label
            {
                Text = "Return to Shop",
                Font = _defaultFont
            };
            returnButton.Content = returnLabel;
            returnButton.Left = 350;
            returnButton.Top = 500;
            returnButton.Width = 150;
            returnButton.Click += (s, e) => _game.ChangeState(GameState.Shop);

            _mainPanel.Widgets.Add(returnButton);
        }
    }
}