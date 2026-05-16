using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PokiePawsDesk.Views
{
    public partial class ClinicsPage : Page
    {
        private readonly ClinicService _clinicService;

        public ClinicsPage(ClinicService clinicService)
        {
            InitializeComponent();
            _clinicService = clinicService;
            LoadClinics();
        }

        private async void LoadClinics()
        {
            var clinics = await _clinicService.GetClinicsAsync();
            ClinicsList.ItemsSource = new ObservableCollection<Clinic>(clinics);
        }

        private void ClinicsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClinicsList.SelectedItem is not Clinic clinic) return;

            DetailsContent.Children.Clear();

            void AddRow(string label, string? value)
            {
                var wrapper = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f5f5f4")),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(0, 0, 0, 16),
                    Margin = new Thickness(0, 0, 0, 16)
                };
                var panel = new StackPanel();
                panel.Children.Add(new TextBlock
                {
                    Text = label,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#78716c")),
                    Margin = new Thickness(0, 0, 0, 4)
                });
                panel.Children.Add(new TextBlock
                {
                    Text = value ?? "—",
                    FontSize = 15,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1c1917"))
                });
                wrapper.Child = panel;
                DetailsContent.Children.Add(wrapper);
            }

            AddRow("Nazwa gabinetu", clinic.ClinicName);
            AddRow("Miasto", clinic.City);
            AddRow("Ulica", clinic.Street);
            AddRow("Godziny otwarcia", clinic.WorkingHours);

            var statusPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 16)
            };
            statusPanel.Children.Add(new TextBlock
            {
                Text = "Status",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#78716c")),
                Margin = new Thickness(0, 0, 0, 6)
            });
            var badge = new Border
            {
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(12, 4, 12, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = clinic.StatusBadgeBrush,
                Child = new TextBlock
                {
                    Text = clinic.StatusText,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = clinic.StatusTextBrush
                }
            };
            statusPanel.Children.Add(badge);
            DetailsContent.Children.Add(statusPanel);
        }
    }
}