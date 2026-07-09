using Sisvenda.Database;
using Sisvenda.Database;
using Sisvenda.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Sisvenda.Views
{
    [QueryProperty(nameof(VendaId), "id")]
    public partial class VendaDetailPage : ContentPage
    {
        private Venda _venda;
        private List<Produto> _produtos;
        private List<Cliente> _clientes;
        private DatabaseService _databaseService;
        private decimal _desconto;
        private Produto _produtoSelecionado;
        private int _vendaId = -1;
        private bool _isLoaded = false;

        public int VendaId
        {
            get => _vendaId;
            set
            {
                _vendaId = value;
                // Não carrega aqui, pois os controles podem não estar prontos
                // O carregamento acontecerá em OnAppearing()
            }
        }

        public ObservableCollection<ItemVenda> Itens { get; private set; }

        public Produto ProdutoSelecionado
        {
            get => _produtoSelecionado;
            set
            {
                if (_produtoSelecionado != value)
                {
                    _produtoSelecionado = value;
                    OnPropertyChanged(nameof(ProdutoSelecionado));
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
                    OnPropertyChanged(nameof(CurrentDate));
                }
            }
        }

        private Cliente _clienteSelecionado;
        public Cliente ClienteSelecionado 
        { 
            get => _clienteSelecionado;
            set
            {
                if (_clienteSelecionado != value)
                {
                    _clienteSelecionado = value;
                    OnPropertyChanged(nameof(ClienteSelecionado));
                }
            }
        }

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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_isLoaded)
                return;

            _isLoaded = true;

            // Carregar dados quando a página aparecer
            await LoadClientesAsync();
            await LoadProdutosAsync();

            // Se for editar uma venda, carregar seus dados
            if (_vendaId > 0)
            {
                await LoadVendaAsync();
            }
        }

        private async Task LoadClientesAsync()
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
                MainThread.BeginInvokeOnMainThread(() =>
                    DisplayAlert("Erro", $"Erro ao carregar clientes: {ex.Message}", "OK")
                );
            }
        }

        private async Task LoadVendaAsync()
        {
            try
            {
                var venda = await _databaseService.GetVendaAsync(_vendaId);
                if (venda != null)
                {
                    _venda = venda;
                    var itens = await _databaseService.GetItensPorVendaAsync(_vendaId);

                    // Carregar produtos para os itens
                    var itensComProdutos = new List<ItemVenda>();
                    foreach (var item in itens)
                    {
                        var produto = await _databaseService.GetProdutoAsync(item.IdProduto);
                        item.Produto = produto;
                        itensComProdutos.Add(item);
                    }

                    var cliente = await _databaseService.GetClienteAsync(_venda.IdCliente);

                    // Agora fazer todos os updates na thread principal
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            Itens.Clear();
                            _venda.Itens.Clear();

                            // Adicionar itens
                            foreach (var item in itensComProdutos)
                            {
                                Itens.Add(item);
                                _venda.Itens.Add(item);
                            }

                            // Carregar cliente
                            if (cliente != null)
                            {
                                ClienteSelecionado = cliente;
                                if (ClientePicker != null)
                                {
                                    ClientePicker.SelectedItem = cliente;
                                }
                            }

                            // Carregar desconto (inicialmente 0)
                            _desconto = 0m;
                            Title = $"Editar Venda #{_venda.Id}";

                            if (DescontoEntry != null)
                            {
                                DescontoEntry.Text = "0";
                            }

                            // Notificar todas as propriedades que mudaram
                            OnPropertyChanged(nameof(CurrentDate));
                            OnPropertyChanged(nameof(ValorTotal));
                            OnPropertyChanged(nameof(Itens));
                            OnPropertyChanged(nameof(ClienteSelecionado));
                        }
                        catch (Exception ex)
                        {
                            DisplayAlert("Erro UI", $"Erro ao atualizar UI: {ex.Message}", "OK");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    DisplayAlert("Erro", $"Erro ao carregar venda: {ex.Message}", "OK")
                );
            }
        }

        private async Task LoadProdutosAsync()
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
                MainThread.BeginInvokeOnMainThread(() =>
                    DisplayAlert("Erro", $"Erro ao carregar produtos: {ex.Message}", "OK")
                );
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

                if (_vendaId > 0)
                {
                    // Atualizar venda existente
                    await _databaseService.UpdateVendaAsync(_venda);
                }
                else
                {
                    // Salvar nova venda
                    var vendaId = await _databaseService.AddVendaAsync(_venda);

                    // Salva itens com IdVenda preenchido
                    foreach (var item in _venda.Itens)
                    {
                        item.IdVenda = (int)vendaId;
                        await _databaseService.AddItemVendaAsync(item);
                    }
                }

                await DisplayAlert("Sucesso", "Venda salva com sucesso!", "OK");
                await Shell.Current.GoToAsync("///vendas");
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
                await Shell.Current.GoToAsync("///vendas");
            }
        }
    }
}
