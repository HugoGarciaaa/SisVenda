using Sisvenda.Models;
using Sisvenda.Database;

namespace Sisvenda.Views
{
    [QueryProperty(nameof(ClienteId), "clienteId")]
    public partial class ClienteDetailPage : ContentPage
    {
        private int _clienteId = -1;
        private Cliente _cliente;
        private DatabaseService _dbService = new DatabaseService();

        public Cliente Cliente
        {
            get => _cliente;
            set
            {
                if (_cliente != value)
                {
                    _cliente = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ClienteId
        {
            get => _clienteId;
            set
            {
                _clienteId = value;
                LoadClienteDetails(_clienteId);
            }
        }

        public ClienteDetailPage()
        {
            InitializeComponent();
            _cliente = new Cliente();
            BindingContext = this;
        }

        private async void LoadClienteDetails(int clienteId)
        {
            if (clienteId <= 0)
            {
                Title = "Novo Cliente";
                _cliente = new Cliente();
                return;
            }

            try
            {
                _cliente = await _dbService.GetClienteAsync(clienteId);
                Title = "Editar Cliente";
                OnPropertyChanged(nameof(Cliente));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar cliente: {ex.Message}", "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_cliente == null || string.IsNullOrWhiteSpace(_cliente.Nome))
            {
                await DisplayAlert("Validacao", "Preencha o nome do cliente", "OK");
                return;
            }

            try
            {
                if (_cliente.Id <= 0)
                {
                    await _dbService.AddClienteAsync(_cliente);
                }
                else
                {
                    await _dbService.UpdateClienteAsync(_cliente);
                }

                await Shell.Current.GoToAsync("///clientes", true);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar cliente: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///clientes", true);
        }
    }
}
