using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WwXMapEditor.Models;
using WwXMapEditor.Services;

namespace WwXMapEditor.Controls
{
    public partial class SpriteSelector : UserControl
    {
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(SpriteSelector),
                new PropertyMetadata(-1, OnSelectedIndexChanged));

        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(nameof(SelectionMode), typeof(SpriteSelectorMode), typeof(SpriteSelector),
                new PropertyMetadata(SpriteSelectorMode.Terrain, OnSelectionModeChanged));

        public static readonly DependencyProperty SeasonProperty =
            DependencyProperty.Register(nameof(Season), typeof(string), typeof(SpriteSelector),
                new PropertyMetadata("Summer", OnSeasonChanged));

        public static readonly DependencyProperty OwnerProperty =
            DependencyProperty.Register(nameof(Owner), typeof(string), typeof(SpriteSelector),
                new PropertyMetadata("Neutral", OnOwnerChanged));

        public static readonly DependencyProperty SelectedTerrainProperty =
            DependencyProperty.Register(nameof(SelectedTerrain), typeof(TerrainType), typeof(SpriteSelector),
                new PropertyMetadata(TerrainType.Plain, OnSelectedTerrainChanged));

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public SpriteSelectorMode SelectionMode
        {
            get => (SpriteSelectorMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        public string Season
        {
            get => (string)GetValue(SeasonProperty);
            set => SetValue(SeasonProperty, value);
        }

        public string Owner
        {
            get => (string)GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }

        public TerrainType SelectedTerrain
        {
            get => (TerrainType)GetValue(SelectedTerrainProperty);
            set => SetValue(SelectedTerrainProperty, value);
        }

        public event EventHandler<int>? SpriteSelected;

        private const int SPRITE_SIZE = 32;
        private const int SPRITES_PER_ROW = 2;
        private const int SPRITES_PER_COLUMN = 4;
        private const int SPRITES_PER_SHEET = 8;

        public SpriteSelector()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpriteSelector selector)
            {
                selector.UpdateSelection();
            }
        }

        private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpriteSelector selector)
            {
                selector.UpdateDisplay();
            }
        }

        private static void OnSeasonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpriteSelector selector)
            {
                selector.UpdateDisplay();
            }
        }

        private static void OnOwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpriteSelector selector)
            {
                selector.UpdateDisplay();
            }
        }

        private static void OnSelectedTerrainChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpriteSelector selector)
            {
                selector.UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            // Clear all existing children to prevent memory leaks
            ClearCanvas();

            SelectionRectangle.Visibility = Visibility.Collapsed;

            switch (SelectionMode)
            {
                case SpriteSelectorMode.Terrain:
                    DisplayTerrainSprites();
                    break;
                case SpriteSelectorMode.Property:
                    DisplayPropertySprite();
                    break;
                case SpriteSelectorMode.Unit:
                    DisplayUnitSprite();
                    break;
            }

            UpdateSelection();
        }

        private void ClearCanvas()
        {
            // Remove all children except the selection rectangle
            for (int i = SpriteCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (SpriteCanvas.Children[i] != SelectionRectangle)
                {
                    SpriteCanvas.Children.RemoveAt(i);
                }
            }
        }

        private void DisplayTerrainSprites()
        {
            // Set size for 2x4 grid
            Width = SPRITES_PER_ROW * SPRITE_SIZE + 4;
            Height = SPRITES_PER_COLUMN * SPRITE_SIZE + 4;

            var spriteSheet = SpriteManager.Instance.GetTerrainSpriteSheet(SelectedTerrain, Season);
            if (spriteSheet != null)
            {
                var image = new Image
                {
                    Source = spriteSheet,
                    Width = SPRITES_PER_ROW * SPRITE_SIZE,
                    Height = SPRITES_PER_COLUMN * SPRITE_SIZE
                };
                Canvas.SetLeft(image, 2);
                Canvas.SetTop(image, 2);
                SpriteCanvas.Children.Add(image);

                // Draw grid lines for better sprite separation
                DrawGridLines();
            }
        }

        private void DrawGridLines()
        {
            var gridBrush = new SolidColorBrush(Colors.DarkGray);
            var gridThickness = 0.5;

            // Vertical lines
            for (int i = 1; i < SPRITES_PER_ROW; i++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = i * SPRITE_SIZE + 2,
                    Y1 = 2,
                    X2 = i * SPRITE_SIZE + 2,
                    Y2 = SPRITES_PER_COLUMN * SPRITE_SIZE + 2,
                    Stroke = gridBrush,
                    StrokeThickness = gridThickness,
                    SnapsToDevicePixels = true
                };
                SpriteCanvas.Children.Add(line);
            }

            // Horizontal lines
            for (int i = 1; i < SPRITES_PER_COLUMN; i++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = 2,
                    Y1 = i * SPRITE_SIZE + 2,
                    X2 = SPRITES_PER_ROW * SPRITE_SIZE + 2,
                    Y2 = i * SPRITE_SIZE + 2,
                    Stroke = gridBrush,
                    StrokeThickness = gridThickness,
                    SnapsToDevicePixels = true
                };
                SpriteCanvas.Children.Add(line);
            }
        }

        private void DisplayPropertySprite()
        {
            Width = SPRITE_SIZE + 4;
            Height = SPRITE_SIZE + 4;

            if (SelectedIndex >= 0)
            {
                var propertyType = (PropertyType)SelectedIndex;
                var sprite = SpriteManager.Instance.GetPropertySprite(propertyType, Owner, Season);

                var image = new Image
                {
                    Source = sprite,
                    Width = SPRITE_SIZE,
                    Height = SPRITE_SIZE
                };
                Canvas.SetLeft(image, 2);
                Canvas.SetTop(image, 2);
                SpriteCanvas.Children.Add(image);
            }
        }

        private void DisplayUnitSprite()
        {
            Width = SPRITE_SIZE + 4;
            Height = SPRITE_SIZE + 4;

            if (SelectedIndex >= 0)
            {
                var unitType = (UnitType)SelectedIndex;
                var sprite = SpriteManager.Instance.GetUnitSprite(unitType, Owner, Season);

                var image = new Image
                {
                    Source = sprite,
                    Width = SPRITE_SIZE,
                    Height = SPRITE_SIZE
                };
                Canvas.SetLeft(image, 2);
                Canvas.SetTop(image, 2);
                SpriteCanvas.Children.Add(image);
            }
        }

        private void UpdateSelection()
        {
            if (SelectionMode == SpriteSelectorMode.Terrain && SelectedIndex >= 0 && SelectedIndex < SPRITES_PER_SHEET)
            {
                int col = SelectedIndex % SPRITES_PER_ROW;
                int row = SelectedIndex / SPRITES_PER_ROW;

                Canvas.SetLeft(SelectionRectangle, col * SPRITE_SIZE + 1);
                Canvas.SetTop(SelectionRectangle, row * SPRITE_SIZE + 1);
                SelectionRectangle.Visibility = Visibility.Visible;
            }
            else if ((SelectionMode == SpriteSelectorMode.Property || SelectionMode == SpriteSelectorMode.Unit) && SelectedIndex >= 0)
            {
                Canvas.SetLeft(SelectionRectangle, 1);
                Canvas.SetTop(SelectionRectangle, 1);
                SelectionRectangle.Visibility = Visibility.Visible;
            }
            else
            {
                SelectionRectangle.Visibility = Visibility.Collapsed;
            }
        }

        private void SpriteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectionMode == SpriteSelectorMode.Terrain)
            {
                var pos = e.GetPosition(SpriteCanvas);
                int col = (int)((pos.X - 2) / SPRITE_SIZE);
                int row = (int)((pos.Y - 2) / SPRITE_SIZE);

                if (col >= 0 && col < SPRITES_PER_ROW && row >= 0 && row < SPRITES_PER_COLUMN)
                {
                    int index = row * SPRITES_PER_ROW + col;
                    if (index < SPRITES_PER_SHEET)
                    {
                        SelectedIndex = index;
                        // Safely invoke the event with null check
                        SpriteSelected?.Invoke(this, index);
                    }
                }
            }
        }
    }

    public enum SpriteSelectorMode
    {
        Terrain,
        Property,
        Unit
    }
}