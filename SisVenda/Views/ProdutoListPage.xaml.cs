using Sisvenda.Models;
using Sisvenda.ViewModels;
using Sisvenda.Services;
using Sisvenda.Database;

namespace Sisvenda.Views
{
    public partial class ProdutoListPage : ContentPage
    {
        private ProdutoViewModel _viewModel;

        public ProdutoListPage()
        {
            InitializeComponent();
            _viewModel = new ProdutoViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProdutosAsync();
        }

        private async void OnAddProdutoClicked(object sender, EventArgs e)
        {
            var page = new ProdutoDetailPage();
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

                await DisplayAlert("Sucesso", $"{importedCount} produtos importados da API!", "OK");
                await _viewModel.LoadProdutosAsync();

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

        private async void OnEditProdutoClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Produto produto)
            {
                var page = new ProdutoDetailPage { BindingContext = produto };
                await Navigation.PushAsync(page);
            }
        }

        private async void OnDeleteProdutoClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Produto produto)
            {
                await _viewModel.DeleteProdutoAsync(produto);
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                await _viewModel.LoadProdutosAsync();
                return;
            }

            var produtos = await _viewModel.GetProdutosByNomeAsync(e.NewTextValue);
            _viewModel.Produtos.Clear();
            foreach (var produto in produtos)
            {
                _viewModel.Produtos.Add(produto);
            }
        }

        private async void OnOptionClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Produto produto)
            {
                var action = await DisplayActionSheet("Opcoes", "Cancelar", null, "Editar", "Deletar");

                if (action == "Editar")
                {
                    await Shell.Current.GoToAsync($"///produtodetail?id={produto.Id}");
                }
                else if (action == "Deletar")
                {
                    var confirm = await DisplayAlert("Confirmacao", "Deseja deletar este produto?", "Sim", "Nao");
                    if (confirm)
                    {
                        await _viewModel.DeleteProdutoAsync(produto);
                    }
                }
            }
        }
    }
}
