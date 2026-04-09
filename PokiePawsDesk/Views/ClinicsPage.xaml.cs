using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.ObjectModel;
using System.Windows.Controls;

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

        private async void ClinicsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClinicsList.SelectedItem is not Clinic selected)
                return;

            var orders = await _clinicService.GetClinicOrdersAsync(selected.Id);
            ClinicOrdersGrid.ItemsSource = orders;
        }
    }
}