using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;
using System.Windows.Controls;

namespace PokiePawsDesk.Views
{
    public partial class OrdersPage : Page
    {
        private readonly OrdersViewModel _viewModel;

        public OrdersPage(IOrderService orderService, IClinicService clinicService)
        {
            InitializeComponent();
            _viewModel = new OrdersViewModel(orderService, clinicService);
            DataContext = _viewModel;
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null) return;
            if (StatusFilter.SelectedItem is ComboBoxItem item)
                _viewModel.SetStatusFilter(item.Tag?.ToString());
            else
                _viewModel.SetStatusFilter(null);
        }
    }
}