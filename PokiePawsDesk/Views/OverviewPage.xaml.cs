using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;
using System.Windows.Controls;

namespace PokiePawsDesk.Views
{
    public partial class OverviewPage : Page
    {
        public OverviewPage(IOrderService orderService, IProductService productService, IClinicService clinicService)
        {
            InitializeComponent();
            DataContext = new OverviewViewModel(orderService, productService, clinicService);
        }
    }
}