using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;
using System.Windows.Controls;

namespace PokiePawsDesk.Views
{
    public partial class ClinicsPage : Page
    {
        public ClinicsPage(ClinicService clinicService, OrderService orderService)
        {
            InitializeComponent();
            DataContext = new ClinicsViewModel(clinicService, orderService);
        }
    }
}