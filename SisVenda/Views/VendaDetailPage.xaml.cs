using Sisvenda.Database;
using Sisvenda.Models;
using System.Collections.ObjectModel;

namespace Sisvenda.Views
{
    public partial class VendaDetailPage : ContentPage
    {
        private Venda _venda;
        private List<Produto> _produtos;
        private List<Cliente> _clientes;
        private DatabaseService _databaseService;
        private decimal _desconto;

        public ObservableCollection<ItemVenda> Itens { get; private set; }

        public Produto ProdutoSelecionado
        {
            get => _venda?.Itens?.LastOrDefault()?.Produto;
            set
            {
                if (value != null)
                {
                    OnPropertyChanged();
                }
            }
        }

        public decimal ValorTotal
        {
            get => (_venda?.Itens?.Sum(i => i.ValorTotal) ?? 0m) - _desconto;
        }

        public DateTime CurrentDate
        {
            get => _venda?.Data ?? DateTime.Now;
            set
            {
                if (_venda != null)
                {
                    _venda.Data = value;
                }
            }
        }

        public Cliente ClienteSelecionado { get; set; }

        public VendaDetailPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _desconto = 0m;

            Itens = new ObservableCollection<ItemVenda>();
            _venda = new Venda
            {
                Data = DateTime.Now,
                ValorTotal = 0m,
                Itens = new List<ItemVenda>()
            };

            BindingContext = this;

            // Inicializar pickers
            LoadClientesAsync();
            LoadProdutosAsync();
        }

        private async void LoadClientesAsync()
        {
            try
            {
                _clientes = await _databaseService.GetClientesAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClientePicker.ItemsSource = _clientes;
                    ClientePicker.ItemDisplayBinding = new Binding("Nome");
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar clientes: {ex.Message}", "OK");
            }
        }

        private async void LoadProdutosAsync()
        {
            try
            {
                _produtos = await _databaseService.GetProdutosAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ProdutoPicker.ItemsSource = _produtos;
                    ProdutoPicker.ItemDisplayBinding = new Binding("Nome");
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar produtos: {ex.Message}", "OK");
            }
        }

        private void OnClienteSelectedChanged(object sender, EventArgs e)
        {
            if (ClientePicker.SelectedItem is Cliente cliente)
            {
                _venda.IdCliente = cliente.Id;
                ClienteSelecionado = cliente;
            }
        }

        private void OnProdutoSelectedChanged(object sender, EventArgs e)
        {
            if (ProdutoPicker.SelectedItem is Produto produto)
            {
                ProdutoSelecionado = produto;
                OnPropertyChanged(nameof(ProdutoSelecionado));
            }
        }

        private async void OnAdicionarItemClicked(object sender, EventArgs e)
        {
            if (ProdutoPicker.SelectedItem == null)
            {
                await DisplayAlert("Validação", "Selecione um produto", "OK");
                return;
            }

            if (!decimal.TryParse(QuantidadeEntry.Text, out var quantidade) || quantidade <= 0)
            {
                await DisplayAlert("Validação", "Informe uma quantidade válida", "OK");
                return;
            }

            if (ProdutoPicker.SelectedItem is Produto produto)
            {
                var item = new ItemVenda
                {
                    IdProduto = produto.Id,
                    Quantidade = quantidade,
                    Valor = produto.Preco,
                    ValorTotal = quantidade * produto.Preco,
                    Produto = produto
                };

                // Verificar se produto já existe no carrinho
                var itemExistente = _venda.Itens.FirstOrDefault(i => i.IdProduto == produto.Id);
                if (itemExistente != null)
                {
                    itemExistente.Quantidade += quantidade;
                    itemExistente.ValorTotal = itemExistente.Quantidade * itemExistente.Valor;

                    var index = Itens.IndexOf(itemExistente);
                    Itens[index] = itemExistente;
                }
                else
                {
                    _venda.Itens.Add(item);
                    Itens.Add(item);
                }

                // Limpar campos
                QuantidadeEntry.Text = "1";
                ProdutoPicker.SelectedItem = null;

                RecalcularTotais();
            }
        }

        private void OnLimparSelecaoClicked(object sender, EventArgs e)
        {
            QuantidadeEntry.Text = "1";
            ProdutoPicker.SelectedItem = null;
            ProdutoSelecionado = null;
            OnPropertyChanged(nameof(ProdutoSelecionado));
        }

        private async void OnRemoverItemClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is ItemVenda item)
            {
                _venda.Itens.Remove(item);
                Itens.Remove(item);
                RecalcularTotais();
            }
        }

        private async void OnAumentarQuantidadeClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is ItemVenda item)
            {
                item.Quantidade += 1;
                item.ValorTotal = item.Quantidade * item.Valor;

                var index = Itens.IndexOf(item);
                Itens[index] = item;

                RecalcularTotais();
            }
        }

        private async void OnDiminuirQuantidadeClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is ItemVenda item)
            {
                if (item.Quantidade > 1)
                {
                    item.Quantidade -= 1;
                    item.ValorTotal = item.Quantidade * item.Valor;

                    var index = Itens.IndexOf(item);
                    Itens[index] = item;

                    RecalcularTotais();
                }
                else
                {
                    await DisplayAlert("Aviso", "A quantidade mínima é 1. Use 'Remover' para excluir o item.", "OK");
                }
            }
        }

        private void OnDescontoChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(e.NewTextValue, out var desconto))
            {
                _desconto = desconto;
            }
            else
            {
                _desconto = 0m;
            }

            RecalcularTotais();
        }

        private void RecalcularTotais()
        {
            if (_venda.Itens.Any())
            {
                _venda.ValorTotal = (_venda.Itens.Sum(i => i.ValorTotal)) - _desconto;
            }
            else
            {
                _venda.ValorTotal = -_desconto;
            }

            OnPropertyChanged(nameof(ValorTotal));
        }

        private async void OnSaveVendaClicked(object sender, EventArgs e)
        {
            if (_venda.IdCliente <= 0)
            {
                await DisplayAlert("Validação", "Selecione um cliente", "OK");
                return;
            }

            if (_venda.Itens == null || !_venda.Itens.Any())
            {
                await DisplayAlert("Validação", "Adicione pelo menos um produto à venda", "OK");
                return;
            }

            try
            {
                _venda.ValorTotal = (_venda.Itens.Sum(i => i.ValorTotal)) - _desconto;

                // Salva venda
                var vendaId = await _databaseService.AddVendaAsync(_venda);

                // Salva itens com IdVenda preenchido
                foreach (var item in _venda.Itens)
                {
                    item.IdVenda = (int)vendaId;
                    await _databaseService.AddItemVendaAsync(item);
                }

                await DisplayAlert("Sucesso", "Venda salva com sucesso!", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao salvar venda: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmar", "Descartar venda e voltar?", "Sim", "Não");
            if (confirm)
            {
                await Navigation.PopAsync();
            }
        }
    }
}
