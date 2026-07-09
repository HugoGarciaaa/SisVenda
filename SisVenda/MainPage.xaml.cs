using Sisvenda.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;

namespace Sisvenda
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnProdutosClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("produtos");
        }

        private async void OnClientesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("clientes");
        }

        private async void OnVendasClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("vendas");
        }
    }
}
