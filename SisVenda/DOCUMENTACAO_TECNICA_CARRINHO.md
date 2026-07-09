# Documentação Técnica: Painel de Carrinho de Compras

## Arquitetura

### Estrutura do Projeto

```
SisVenda/
??? Models/
?   ??? Venda.cs ...................... Modelo de venda
?   ??? ItemVenda.cs (MODIFICADO) ..... Item da venda com referência ao Produto
?   ??? Produto.cs .................... Modelo de produto
?   ??? Cliente.cs .................... Modelo de cliente
??? Views/
?   ??? VendaDetailPage.xaml (MODIFICADO) ............ Interface visual
?   ??? VendaDetailPage.xaml.cs (MODIFICADO) ........ Lógica de negócio
??? Database/
    ??? DatabaseService.cs ............. Serviço de acesso a dados
```

## Alterações Implementadas

### 1. Model - ItemVenda.cs

#### Antes:
```csharp
[Ignore]
public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
```

#### Depois:
```csharp
[Ignore]
public Produto Produto { get; set; }
```

**Motivo**: Permite que a UI acesse todas as propriedades do produto (nome, código, categoria, descrição, preço, imagem) diretamente do ItemVenda.

---

### 2. UI - VendaDetailPage.xaml

#### Mudanças Principais:

**Layout Principal - De:**
```xaml
<Grid RowDefinitions="auto,auto,*,auto">
  <!-- Layout vertical único -->
</Grid>
```

**Para:**
```xaml
<Grid RowDefinitions="auto,auto,*,auto">
  <Grid Grid.Row="2" ColumnDefinitions="*,*">
    <!-- Coluna 1: Seleção de Produtos -->
    <!-- Coluna 2: Carrinho -->
  </Grid>
</Grid>
```

#### Componentes Adicionados:

1. **Painel de Informações do Produto** (esquerda)
   ```xaml
   <Frame x:Name="ProdutoInfoFrame">
     <VerticalStackLayout x:Name="ProdutoInfoPanel"/>
   </Frame>
   ```
   - Container dinâmico para exibir detalhes
   - Atualizado via code-behind quando produto é selecionado

2. **Template do Carrinho** (direita)
   ```xaml
   <CollectionView ItemsSource="{Binding Itens}">
     <CollectionView.ItemTemplate>
       <DataTemplate>
         <Grid ColumnDefinitions="80,*,auto">
           <!-- Imagem: 80x80 pixels -->
           <!-- Informações: nome, código, categoria, preço, quantidade -->
           <!-- Botão remover: 40x40 pixels -->
         </Grid>
       </DataTemplate>
     </CollectionView.ItemTemplate>
   </CollectionView>
   ```

3. **Resumo do Carrinho**
   ```xaml
   <Frame BorderColor="#34C759" BackgroundColor="#F0F9F0">
     <Grid>
       <Label Text="Quantidade de Itens: " />
       <Label Text="{Binding ItemCount}" />
       <Label Text="Valor Total: " />
       <Label Text="{Binding ValorTotal, StringFormat='R$ {0:F2}'}" />
     </Grid>
   </Frame>
   ```

---

### 3. Code-Behind - VendaDetailPage.xaml.cs

#### Novos Métodos:

##### A. `ExibirInformacoesProduto(Produto produto)`

```csharp
private void ExibirInformacoesProduto(Produto produto)
{
    ProdutoInfoPanel.Clear();

    // 1. Imagem
    if (!string.IsNullOrEmpty(produto.ImageUrl))
    {
        var imagem = new Image { /* ... */ };
        ProdutoInfoPanel.Add(imagem);
    }

    // 2. Nome
    var nomeLbl = new Label { Text = produto.Nome, /* ... */ };
    ProdutoInfoPanel.Add(nomeLbl);

    // 3. Código
    var codLbl = new Label { Text = $"Código: {produto.Codigo}", /* ... */ };
    ProdutoInfoPanel.Add(codLbl);

    // 4. Categoria
    var catLbl = new Label { Text = $"Categoria: {produto.Categoria}", /* ... */ };
    ProdutoInfoPanel.Add(catLbl);

    // 5. Descrição (se disponível)
    if (!string.IsNullOrEmpty(produto.Descricao))
    {
        var descLbl = new Label { Text = $"Descrição: {produto.Descricao}", /* ... */ };
        ProdutoInfoPanel.Add(descLbl);
    }

    // 6. Preço
    var precoLbl = new Label { Text = $"Preço: R$ {produto.Preco:F2}", /* ... */ };
    ProdutoInfoPanel.Add(precoLbl);
}
```

**Propósito**: Renderiza dinamicamente o painel de informações do produto.

**Funcionamento**:
1. Limpa o painel anterior
2. Adiciona a imagem se disponível
3. Adiciona nome, código, categoria
4. Adiciona descrição se disponível
5. Adiciona preço em destaque

##### B. `LimparInformacoesProduto()`

```csharp
private void LimparInformacoesProduto()
{
    ProdutoInfoPanel.Clear();
    var lbl = new Label
    {
        Text = "Selecione um produto para ver detalhes",
        FontSize = 12,
        TextColor = Color.FromArgb("#999999"),
        HorizontalTextAlignment = TextAlignment.Center,
        VerticalTextAlignment = TextAlignment.Center
    };
    ProdutoInfoPanel.Add(lbl);
}
```

**Propósito**: Restaura o estado inicial do painel de informações.

#### Métodos Modificados:

##### A. `OnProdutoSelectedChanged()`

**Antes**:
```csharp
private void OnProdutoSelectedChanged(object sender, EventArgs e)
{
    // nothing for now
}
```

**Depois**:
```csharp
private void OnProdutoSelectedChanged(object sender, EventArgs e)
{
    if (ProdutoPicker.SelectedItem is Produto produto)
    {
        ExibirInformacoesProduto(produto);
    }
    else
    {
        LimparInformacoesProduto();
    }
}
```

**Mudança**: Agora exibe as informações do produto selecionado.

##### B. `OnAdicionarItemClicked()`

**Antes**:
```csharp
var item = new ItemVenda
{
    IdProduto = produto.Id,
    Quantidade = quantidade,
    Valor = produto.Preco,
    ValorTotal = quantidade * produto.Preco
};
```

**Depois**:
```csharp
var item = new ItemVenda
{
    IdProduto = produto.Id,
    Produto = produto,  // ? Novo
    Quantidade = quantidade,
    Valor = produto.Preco,
    ValorTotal = quantidade * produto.Preco
};
```

**Mudança**: Agora inclui a referência ao produto.

**Adição**:
```csharp
LimparInformacoesProduto();  // ? Novo
```

**Mudança**: Limpa o painel de informações após adicionar.

##### C. `RecalcularTotais()`

**Antes**:
```csharp
OnPropertyChanged(nameof(ValorTotal));
```

**Depois**:
```csharp
OnPropertyChanged(nameof(ValorTotal));
OnPropertyChanged(nameof(ItemCount));  // ? Novo
```

**Mudança**: Notifica a UI sobre mudança na quantidade de itens.

#### Novas Propriedades:

```csharp
public int ItemCount => _itens?.Count ?? 0;
```

**Propósito**: Retorna a quantidade de itens no carrinho para exibição no resumo.

---

## Fluxo de Dados

```
???????????????????????????????????????????????????????????
?                    Usuário Seleciona Produto            ?
???????????????????????????????????????????????????????????
                     ?
                     ?
        ??????????????????????????????
        ? OnProdutoSelectedChanged()  ?
        ??????????????????????????????
                     ?
        ??????????????????????????????
        ? ExibirInformacoesProduto() ?
        ? ou LimparInformacoesProduto()
        ??????????????????????????????
                     ?
                     ?
        ??????????????????????????????
        ?  Painel Esquerdo Atualizado?
        ??????????????????????????????

???????????????????????????????????????????????????????????
?           Usuário Clica em Adicionar                    ?
???????????????????????????????????????????????????????????
                     ?
                     ?
        ??????????????????????????????
        ? OnAdicionarItemClicked()    ?
        ? - Validação de quantidade  ?
        ? - Criação de ItemVenda     ?
        ??????????????????????????????
                     ?
                     ?
        ??????????????????????????????
        ? Adiciona à lista _itens    ?
        ? Adiciona à lista _venda    ?
        ??????????????????????????????
                     ?
                     ?
        ??????????????????????????????
        ? RecalcularTotais()         ?
        ? - Soma todos os ValorTotal ?
        ? - Notifica ItemCount       ?
        ? - Notifica ValorTotal      ?
        ??????????????????????????????
                     ?
                     ?
        ??????????????????????????????
        ? Painel Direito Atualizado  ?
        ? Resumo Atualizado          ?
        ??????????????????????????????
```

---

## Binding e Data Binding

### Bindings Utilizados:

| Propriedade | Binding | Tipo |
|------------|---------|------|
| `ValorTotal` | `{Binding ValorTotal, StringFormat='R$ {0:F2}'}` | TwoWay |
| `ItemCount` | `{Binding ItemCount}` | OneWay |
| `Itens` | `{Binding Itens}` | OneWay |
| `Produto.Nome` | `{Binding Produto.Nome}` | OneWay |
| `Produto.Codigo` | `{Binding Produto.Codigo, StringFormat='Código: {0}'}` | OneWay |
| `Produto.Categoria` | `{Binding Produto.Categoria, StringFormat='Categoria: {0}'}` | OneWay |
| `Produto.ImageUrl` | `{Binding Produto.ImageUrl}` | OneWay |
| `Valor` | `{Binding Valor, StringFormat='Preço Unit.: R$ {0:F2}'}` | OneWay |
| `Quantidade` | `{Binding Quantidade}` | OneWay |
| `ValorTotal` (Item) | `{Binding ValorTotal, StringFormat='R$ {0:F2}'}` | OneWay |

---

## Padrões de Design Utilizados

### 1. **MVVM (Model-View-ViewModel)**
- **Model**: `ItemVenda`, `Produto`, `Venda`, `Cliente`
- **View**: `VendaDetailPage.xaml`
- **ViewModel/Lógica**: `VendaDetailPage.xaml.cs`

### 2. **Observer Pattern**
- Uso de `OnPropertyChanged()` para notificar a UI sobre mudanças
- `ObservableCollection<ItemVenda>` para sincronização automática

### 3. **Builder Pattern**
- Construção dinâmica de componentes na UI (`ExibirInformacoesProduto()`)

### 4. **Dependency Injection**
- `DatabaseService` injetado para acesso a dados

---

## Recursos Utilizados

### Componentes MAUI:
- `Grid` - Layout em colunas
- `VerticalStackLayout` - Organização vertical
- `CollectionView` - Exibição de listas
- `Image` - Exibição de imagens
- `Label` - Textos
- `Entry` - Entrada de dados
- `Picker` - Seleção em lista
- `Button` - Ações
- `Frame` - Containers com borda
- `DataTemplate` - Template para itens

### Conceitos MAUI:
- Data Binding
- INotifyPropertyChanged
- ObservableCollection
- EventHandler
- Command Parameter

---

## Performance e Otimizações

### Otimizações Implementadas:

1. **Uso de `Clear()` em vez de recriação**
   ```csharp
   ProdutoInfoPanel.Clear();  // Limpa e reutiliza
   ```

2. **Lazy Loading de Imagens**
   - Imagens carregadas via URL (não embarcadas)

3. **CollectionView em vez de ListView**
   - Melhor performance com virtualization

4. **Validação Antecipada**
   ```csharp
   if (!decimal.TryParse(...)) { /* erro */ }
   ```

### Sugestões de Melhorias:

1. **Caching de Produtos**
   ```csharp
   private static Dictionary<int, Produto> _produtoCache;
   ```

2. **Paginação no Carrinho** (se houver muitos itens)

3. **Busca/Filtro de Produtos**
   ```csharp
   var filtrados = _produtos.Where(p => p.Nome.Contains(termo));
   ```

4. **Quantidade Máxima Validada**
   ```csharp
   if (quantidade > produto.EstoqueDisponivel) { /* erro */ }
   ```

---

## Tratamento de Erros

### Validações Implementadas:

1. **Quantidade Inválida**
   ```csharp
   if (!decimal.TryParse(QuantidadeEntry.Text, out var quantidade) 
       || quantidade <= 0)
   {
       await DisplayAlert("Validação", "Informe uma quantidade válida", "OK");
       return;
   }
   ```

2. **Produto Não Selecionado**
   ```csharp
   if (ProdutoPicker.SelectedItem is Produto produto) { /* ... */ }
   else { await DisplayAlert("Validação", "Selecione um produto", "OK"); }
   ```

3. **Cliente Não Selecionado**
   ```csharp
   if (_venda.IdCliente <= 0)
   {
       await DisplayAlert("Validação", "Selecione um cliente", "OK");
       return;
   }
   ```

4. **Carrinho Vazio**
   ```csharp
   if (!_venda.Itens.Any())
   {
       await DisplayAlert("Validação", "Adicione pelo menos um produto", "OK");
       return;
   }
   ```

5. **Erro ao Salvar**
   ```csharp
   catch (Exception ex)
   {
       await DisplayAlert("Erro", $"Erro ao salvar venda: {ex.Message}", "OK");
   }
   ```

---

## Testes Recomendados

### Testes Unitários:
```csharp
[TestMethod]
public void AdicionarItem_DeveCalcularSubtotalCorreto()
{
    // Arrange
    var item = new ItemVenda { Quantidade = 2, Valor = 50m };

    // Act
    item.ValorTotal = item.Quantidade * item.Valor;

    // Assert
    Assert.AreEqual(100m, item.ValorTotal);
}

[TestMethod]
public void RecalcularTotais_DeveCalcularSomaCorreta()
{
    // Arrange
    var venda = new Venda();
    venda.Itens.Add(new ItemVenda { ValorTotal = 100m });
    venda.Itens.Add(new ItemVenda { ValorTotal = 50m });

    // Act
    venda.ValorTotal = venda.Itens.Sum(i => i.ValorTotal);

    // Assert
    Assert.AreEqual(150m, venda.ValorTotal);
}
```

### Testes de Interface:
1. Seleção de produto exibe informações
2. Adição de item atualiza carrinho
3. Remoção de item atualiza total
4. Validações funcionam corretamente
5. Confirmação de venda salva dados

---

## Compatibilidade

- ? .NET MAUI 9
- ? Windows, macOS, iOS, Android
- ? Responde bem a diferentes resoluções de tela
- ? Suporte a temas claro/escuro (com ajustes adicionais)

---

## Próximas Versões

### v2.0:
- [ ] Edição de quantidade no carrinho
- [ ] Desconto por item
- [ ] Cupom de desconto
- [ ] Histórico de vendas rápidas

### v3.0:
- [ ] Sincronização em tempo real
- [ ] Suporte offline
- [ ] Múltiplos métodos de pagamento
- [ ] Integração com sistemas de estoque
