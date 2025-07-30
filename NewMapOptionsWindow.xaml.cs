using System.Windows;
using System.Windows.Controls;

namespace WwXMapEditor
{
    public class MapOptions
    {
        public string Name { get; set; } = "UntitledMap";
        public int Length { get; set; } = 100;
        public int Width { get; set; } = 100;
        public string Terrain { get; set; } = "Plain";
        public string Season { get; set; } = "Summer";
        public string Weather { get; set; } = "Random";
    }

    public partial class NewMapOptionsWindow : Window
    {
        // Make MapOptions nullable and set to null in constructor to satisfy non-nullable warning and best practice.
        public MapOptions? MapOptions { get; private set; } = null;

        public NewMapOptionsWindow()
        {
            InitializeComponent();

            // Set default values
            NameBox.Text = "UntitledMap";
            LengthBox.Text = "100";
            WidthBox.Text = "100";
            TerrainCombo.SelectedIndex = 0; // Plain
            SeasonCombo.SelectedIndex = 0;  // Summer
            WeatherCombo.SelectedIndex = 0; // Random
        }

        private void CreateMap_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(LengthBox.Text, out int length) || length < 20 || length > 2000)
            {
                MessageBox.Show("Length must be between 20 and 2000.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(WidthBox.Text, out int width) || width < 20 || width > 2000)
            {
                MessageBox.Show("Width must be between 20 and 2000.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var terrain = (TerrainCombo.SelectedItem as ComboBoxItem)?.Content as string ?? "Plain";
            var season = (SeasonCombo.SelectedItem as ComboBoxItem)?.Content as string ?? "Summer";
            var weather = (WeatherCombo.SelectedItem as ComboBoxItem)?.Content as string ?? "Random";
            var name = string.IsNullOrWhiteSpace(NameBox.Text) ? "UntitledMap" : NameBox.Text.Trim();

            MapOptions = new MapOptions
            {
                Name = name,
                Length = length,
                Width = width,
                Terrain = terrain,
                Season = season,
                Weather = weather
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}