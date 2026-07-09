using Sisvenda.Models;
using Sisvenda.ViewModels;
using Sisvenda.Services;
using Sisvenda.Database;

namespace Sisvenda.Views
{
    public partial class VendaListPage : ContentPage
    {
        private VendaViewModel _viewModel;

        public VendaListPage()
        {
            InitializeComponent();
            _viewModel = new VendaViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadVendasAsync();
        }

        private async void OnAddVendaClicked(object sender, EventArgs e)
        {
            var page = new VendaDetailPage();
            await Navigation.PushAsync(page);
        }

        private async void OnImportFromApiClicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;
                button.IsEnabled = false;
                button.Text = "Importando...";

                var apiService = new FakeStoreApiService();
                var dbService = new DatabaseService();
                var importedCount = await apiService.ImportProductsAsync(dbService);

                await DisplayAlert("Sucesso", $"{importedCount} produtos importados da API!\n\nAgora você pode usar estes produtos ao criar vendas.", "OK");

                button.Text = "Import API";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao importar: {ex.Message}", "OK");
            }
            finally
            {
                var button = sender as Button;
                button.IsEnabled = true;
            }
        }

        private async void OnEditVendaClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Venda venda)
            {
                var page = new VendaDetailPage { BindingContext = venda };
                await Navigation.PushAsync(page);
            }
        }

        private async void OnDeleteVendaClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Venda venda)
            {
                await _viewModel.DeleteVendaAsync(venda);
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                await _viewModel.LoadVendasAsync();
                return;
            }

            // Tentativa de buscar por nome de cliente
            var vendas = await _viewModel.GetVendasByClienteAsync(-1);
            _viewModel.Vendas.Clear();
            foreach (var venda in vendas)
            {
                _viewModel.Vendas.Add(venda);
            }
        }

        private async void OnOptionClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Venda venda)
            {
                var action = await DisplayActionSheet("Opcoes", "Cancelar", null, "Ver", "Deletar");

                if (action == "Ver")
                {
                    await Shell.Current.GoToAsync($"vendadetail?id={venda.Id}");
                }
                else if (action == "Deletar")
                {
                    var confirm = await DisplayAlert("Confirmacao", "Deseja deletar esta venda?", "Sim", "Nao");
                    if (confirm)
                    {
                        await _viewModel.DeleteVendaAsync(venda);
                    }
                }
            }
        }
    }
}
