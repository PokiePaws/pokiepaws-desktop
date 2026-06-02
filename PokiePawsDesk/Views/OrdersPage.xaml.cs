using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        private void ExpandRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn)
            {
                var row = DataGridRow.GetRowContainingElement(btn);
                if (row != null)
                {
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
            }
        }
    }
}
