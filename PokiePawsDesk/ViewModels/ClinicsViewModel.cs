using CommunityToolkit.Mvvm.ComponentModel;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.ObjectModel;

namespace PokiePawsDesk.ViewModels
{
    public partial class ClinicsViewModel : ObservableObject
    {
        private readonly ClinicService _clinicService;
        private readonly OrderService _orderService;

        [ObservableProperty]
        private ObservableCollection<Clinic> clinics = new();

        [ObservableProperty]
        private Clinic? selectedClinic;

        [ObservableProperty]
        private ObservableCollection<Order> clinicOrders = new();

        [ObservableProperty]
        private bool isClinicSelected = false;

        [ObservableProperty]
        private bool isClinicNotSelected = true;

        public ClinicsViewModel(ClinicService clinicService, OrderService orderService)
        {
            _clinicService = clinicService;
            _orderService = orderService;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            var local = _clinicService.GetLocalClinics();
            if (local.Count > 0)
                Clinics = new ObservableCollection<Clinic>(local);

            var fresh = await _clinicService.GetClinicsAsync();
            Clinics = new ObservableCollection<Clinic>(fresh);
        }

        partial void OnSelectedClinicChanged(Clinic? value)
        {
            IsClinicSelected = value != null;
            IsClinicNotSelected = value == null;
            ClinicOrders = new ObservableCollection<Order>();
            if (value != null)
                _ = LoadClinicOrdersAsync(value);
        }

        private async Task LoadClinicOrdersAsync(Clinic clinic)
        {
            var local = _orderService.GetLocalOrders()
                .Where(o => o.ClinicId == clinic.Id)
                .ToList();
            ClinicOrders = new ObservableCollection<Order>(local);

            var fresh = await _orderService.GetOrdersAsync();
            ClinicOrders = new ObservableCollection<Order>(
                fresh.Where(o => o.ClinicId == clinic.Id));
        }
    }
}