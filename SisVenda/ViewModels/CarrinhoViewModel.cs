using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sisvenda.Models;

namespace Sisvenda.ViewModels
{
    public class CarrinhoViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ItemVenda> _itens;
        private decimal _valorTotal;
        private decimal _desconto;
        private Produto _produtoSelecionado;
        private int _quantidadeSelecionada;

        public ObservableCollection<ItemVenda> Itens
        {
            get => _itens;
            set
            {
                if (_itens != value)
                {
                    _itens = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal ValorTotal
        {
            get => _valorTotal;
            set
            {
                if (_valorTotal != value)
                {
                    _valorTotal = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Desconto
        {
            get => _desconto;
            set
            {
                if (_desconto != value)
                {
                    _desconto = value;
                    OnPropertyChanged();
                    RecalcularTotal();
                }
            }
        }

        public Produto ProdutoSelecionado
        {
            get => _produtoSelecionado;
            set
            {
                if (_produtoSelecionado != value)
                {
                    _produtoSelecionado = value;
                    QuantidadeSelecionada = 1;
                    OnPropertyChanged();
                }
            }
        }

        public int QuantidadeSelecionada
        {
            get => _quantidadeSelecionada;
            set
            {
                if (_quantidadeSelecionada != value && value > 0)
                {
                    _quantidadeSelecionada = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal ValorFinal => ValorTotal - Desconto;

        public CarrinhoViewModel()
        {
            _itens = new ObservableCollection<ItemVenda>();
            _valorTotal = 0m;
            _desconto = 0m;
            _quantidadeSelecionada = 1;
        }

        public void AdicionarItem(ItemVenda item)
        {
            if (item == null) return;

            var itemExistente = Itens.FirstOrDefault(i => i.IdProduto == item.IdProduto);

            if (itemExistente != null)
            {
                itemExistente.Quantidade += item.Quantidade;
                itemExistente.ValorTotal = itemExistente.Quantidade * itemExistente.Valor;
            }
            else
            {
                Itens.Add(item);
            }

            RecalcularTotal();
        }

        public void RemoverItem(ItemVenda item)
        {
            if (item == null) return;

            Itens.Remove(item);
            RecalcularTotal();
        }

        public void AtualizarQuantidade(ItemVenda item, decimal novaQuantidade)
        {
            if (item == null || novaQuantidade <= 0) return;

            item.Quantidade = novaQuantidade;
            item.ValorTotal = item.Quantidade * item.Valor;
            RecalcularTotal();
        }

        public void LimparCarrinho()
        {
            Itens.Clear();
            ValorTotal = 0m;
            Desconto = 0m;
            ProdutoSelecionado = null;
        }

        private void RecalcularTotal()
        {
            ValorTotal = Itens.Sum(i => i.ValorTotal);
            OnPropertyChanged(nameof(ValorFinal));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
