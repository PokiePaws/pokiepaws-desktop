using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;
using System.Windows.Controls;

namespace PokiePawsDesk.Views
{
    public partial class OverviewPage : Page
    {
        public OverviewPage(OrderService orderService, ProductService productService, ClinicService clinicService)
        {
            InitializeComponent();
            DataContext = new OverviewViewModel(orderService, productService, clinicService);
        }
    }
}