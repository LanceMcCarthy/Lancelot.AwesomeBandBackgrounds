// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using CommonHelpers.Common;

namespace BandCentral.Uwp.Controls.CustomColorPicker
{
    public class ColorPickerViewModel : ViewModelBase
    {
        private Visibility swatchViewVisibility = Visibility.Visible;
        private Visibility colorsViewVisibility = Visibility.Collapsed;
        private Swatch selectedSwatch;

        public ColorPickerViewModel()
        {
            LoadColors();
        }

        private async void LoadColors()
        {
            Swatches = await GetSwatchesAsync();
            SelectedSwatch = Swatches[0];
        }

        public ObservableCollection<Swatch> Swatches { get; set; }

        public Swatch SelectedSwatch
        {
            get => selectedSwatch;
            set { selectedSwatch = value; OnPropertyChanged(); }
        }

        public Visibility SwatchViewVisibility
        {
            get => swatchViewVisibility;
            set { swatchViewVisibility = value; OnPropertyChanged(); }
        }

        public Visibility ColorsViewVisibility
        {
            get => colorsViewVisibility;
            set { colorsViewVisibility = value; OnPropertyChanged(); }
        }

        public async Task ToggleViewStateAsync()
        {
            if(ColorsViewVisibility == Visibility.Visible)
            {
                ColorsViewVisibility = Visibility.Collapsed;
                SwatchViewVisibility = Visibility.Visible;
            }
            else
            {
                SwatchViewVisibility = Visibility.Collapsed;
                ColorsViewVisibility = Visibility.Visible;
            }
        }

        private async Task<ObservableCollection<Swatch>> GetSwatchesAsync()
        {
            var list = new ObservableCollection<Swatch>();

            list.Add(Palettes.ApexSwatch);
            list.Add(Palettes.HardcoverSwatch);
            list.Add(Palettes.MetroSwatch);
            list.Add(Palettes.ModuleSwatch);
            list.Add(Palettes.OfficeSwatch);
            list.Add(Palettes.PaperSwatch);
            list.Add(Palettes.PushpinSwatch);
            list.Add(Palettes.Solstice);
            list.Add(Palettes.UrbanSwatch);
            list.Add(Palettes.WaveformSwatch);

            return list;
        }
    }
}