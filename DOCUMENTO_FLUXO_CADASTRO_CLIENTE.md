# FLUXO DO CÓDIGO — CADASTRO DE CLIENTE
## Sistema SisVenda — Projeto Final de Programação Visual

---

## SUMÁRIO

1. [Visão Geral do Fluxo](#1-visão-geral-do-fluxo)
2. [Arquitetura de Camadas](#2-arquitetura-de-camadas)
3. [Etapa 1: Interface de Lista (ClienteListPage)](#3-etapa-1-interface-de-lista-clientelistpage)
4. [Etapa 2: Interface de Detalhe (ClienteDetailPage)](#4-etapa-2-interface-de-detalhe-clientedetailpage)
5. [Etapa 3: Modelo de Dados (Cliente)](#5-etapa-3-modelo-de-dados-cliente)
6. [Etapa 4: ViewModel (ClienteViewModel)](#6-etapa-4-viewmodel-clienteviewmodel)
7. [Etapa 5: Persistência (DatabaseService)](#7-etapa-5-persistência-databaseservice)
8. [Fluxo Completo Passo a Passo](#8-fluxo-completo-passo-a-passo)
9. [Diagrama de Sequência](#9-diagrama-de-sequência)
10. [Conclusão](#10-conclusão)

---

## 1. VISÃO GERAL DO FLUXO

O cadastro de um novo cliente no sistema **SisVenda** é iniciado pelo acionamento do botão **(+)** localizado na tela de lista de clientes. O fluxo segue a arquitetura **MVVM (Model-View-ViewModel)** e culmina com a inserção dos dados no banco de dados **SQLite**.

### Resumo simplificado:

```
Clique no (+) → Abre formulário → Preenche dados → Clica Salvar → 
Valida → Persiste no SQLite → Atualiza lista → Exibe confirmação
```

---

## 2. ARQUITETURA DE CAMADAS

O sistema é organizado em camadas:

1. **Camada de Apresentação (View)**: XAML (ClienteListPage, ClienteDetailPage)
2. **Camada de Lógica (ViewModel)**: ClienteViewModel
3. **Camada de Modelo (Model)**: Cliente
4. **Camada de Persistência (Data)**: DatabaseService

Cada camada tem uma responsabilidade específica, facilitando manutenção e testes.

---

## 3. ETAPA 1: INTERFACE DE LISTA (ClienteListPage)

### Arquivo: `ClienteListPage.xaml`

A tela de lista é o **ponto de entrada** do fluxo de cadastro.

#### 3.1 Estrutura principal

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="Sisvenda.Views.ClienteListPage"
             Title="Clientes"
             BackgroundColor="#1a1a1a">

    <Grid RowDefinitions="auto,*" Padding="0">
        <!-- Linha 0: Barra superior (busca + botão +) -->
        <!-- Linha 1: Lista de clientes -->
    </Grid>
</ContentPage>
```

**Explicação linha a linha:**

- `<ContentPage>`: Container principal de uma página em MAUI.
- `Title="Clientes"`: Define o título da página exibido no header.
- `BackgroundColor="#1a1a1a"`: Cor de fundo escura (tema dark mode).
- `<Grid RowDefinitions="auto,*">`: Define 2 linhas:
  - Linha 0 (`auto`): altura automática conforme conteúdo.
  - Linha 1 (`*`): altura relativa, ocupa espaço restante.

#### 3.2 Barra superior com busca e botão `+`

```xml
<Grid ColumnDefinitions="*,auto" Padding="10" BackgroundColor="#2a2a2a" ColumnSpacing="10">
    <SearchBar Grid.Column="0"
              x:Name="SearchBar" 
              Placeholder="Buscar clientes..." 
              PlaceholderColor="#808080"
              TextChanged="OnSearchTextChanged"
              BackgroundColor="#404040"
              TextColor="White"/>
    
    <Button Grid.Column="1" 
           Text="+" 
           FontSize="24"
           Clicked="OnAddClienteClicked"
           CornerRadius="8"
           WidthRequest="50"
           HeightRequest="50"
           BackgroundColor="#007AFF"
           TextColor="White"
           Padding="0"/>
</Grid>
```

**Análise linha por linha:**

- `<Grid ColumnDefinitions="*,auto" ... ColumnSpacing="10">`:
  - Define 2 colunas: coluna 0 (`*`) ocupa espaço disponível, coluna 1 (`auto`) com largura fixa.
  - `ColumnSpacing="10"`: espaço entre colunas = 10 unidades.

- `<SearchBar Grid.Column="0" x:Name="SearchBar" ... >`:
  - Posicionada na coluna 0.
  - `x:Name="SearchBar"`: identificador para acesso no code-behind.
  - `TextChanged="OnSearchTextChanged"`: dispara evento conforme o usuário digita.
  - Responsável por **filtrar a lista** de clientes durante a busca.

- **BOTÃO (+) — PONTO DE ENTRADA DO CADASTRO:**

```xml
<Button Grid.Column="1" 
       Text="+" 
       FontSize="24"
       Clicked="OnAddClienteClicked"
       CornerRadius="8"
       WidthRequest="50"
       HeightRequest="50"
       BackgroundColor="#007AFF"
       TextColor="White"
       Padding="0"/>
```

  - `Grid.Column="1"`: posicionado na coluna 1 (lado direito).
  - `Text="+"`: exibe o símbolo `+`.
  - `FontSize="24"`: tamanho grande do texto, bem visível.
  - **`Clicked="OnAddClienteClicked"`**: quando o usuário toca este botão, dispara o evento `OnAddClienteClicked` no code-behind.
  - `CornerRadius="8"`: cantos arredondados para aparência moderna.
  - `WidthRequest="50" HeightRequest="50"`: dimensões quadradas 50x50.
  - `BackgroundColor="#007AFF"`: azul padrão iOS.
  - `Padding="0"`: sem preenchimento interno (símbolo centralizado).

#### 3.3 Lista de clientes

```xml
<CollectionView Grid.Row="1" 
               x:Name="ClienteCollectionView"
               ItemsSource="{Binding Clientes}"
               SelectionMode="Single"
               BackgroundColor="#1a1a1a">
    <CollectionView.ItemTemplate>
       <DataTemplate>
           <!-- Template para cada cliente na lista -->
       </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

**Explicação:**

- `Grid.Row="1"`: posicionada na linha 1 (abaixo da barra superior).
- `x:Name="ClienteCollectionView"`: identificador para acesso no code-behind.
- `ItemsSource="{Binding Clientes}"`: vinculada à propriedade `Clientes` do ViewModel.
  - A propriedade é uma `ObservableCollection<Cliente>`.
  - Qualquer mudança na coleção atualiza automaticamente a UI.
- `SelectionMode="Single"`: permite seleção de um item de cada vez.
- `DataTemplate`: define o layout de cada item (cliente) exibido na lista.

---

## 4. ETAPA 2: INTERFACE DE DETALHE (ClienteDetailPage)

### Arquivo: `ClienteDetailPage.xaml`

Quando o usuário clica no botão `+`, a navegação abre esta tela de formulário.

#### 4.1 Estrutura geral

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="Sisvenda.Views.ClienteDetailPage"
             Title="Detalhes do Cliente"
             BackgroundColor="#1a1a1a">

    <Grid RowDefinitions="*,auto" Padding="0">
        <ScrollView Grid.Row="0" BackgroundColor="#1a1a1a">
            <!-- Formulário com campos -->
        </ScrollView>

        <Grid Grid.Row="1" ColumnDefinitions="*,*" Padding="10" ColumnSpacing="10" BackgroundColor="#2a2a2a">
            <!-- Botões Cancelar e Salvar -->
        </Grid>
    </Grid>
</ContentPage>
```

**Explicação:**

- `Grid RowDefinitions="*,auto"`:
  - Linha 0 (`*`): contém o formulário, ocupa espaço disponível.
  - Linha 1 (`auto`): barra de botões na base.

#### 4.2 Formulário de entrada de dados

Cada campo segue o padrão:

```xml
<VerticalStackLayout Spacing="8">
    <Label Text="Nome:" FontSize="12" FontAttributes="Bold" TextColor="#A0A0A0"/>
    <Frame Padding="0" CornerRadius="8" BorderColor="#404040" HasShadow="False" BackgroundColor="#2a2a2a">
        <Entry Placeholder="Digite o nome do cliente"
              Text="{Binding Cliente.Nome}"
              PlaceholderColor="#606060"
              TextColor="White"
              BackgroundColor="#2a2a2a"/>
    </Frame>
</VerticalStackLayout>
```

**Linha por linha:**

- `<VerticalStackLayout Spacing="8">`:
  - Organiza label + campo verticalmente.
  - `Spacing="8"`: espaço entre rótulo e entrada.

- `<Label Text="Nome:" ... >`:
  - Rótulo descritivo do campo.
  - `FontAttributes="Bold"`: texto em negrito.
  - `TextColor="#A0A0A0"`: cor cinza claro.

- `<Frame ... >`:
  - Container visual ao redor do `Entry`.
  - `CornerRadius="8"`: cantos arredondados.
  - `BorderColor="#404040"`: borda cinza.
  - `BackgroundColor="#2a2a2a"`: fundo escuro.

- **`<Entry ... Text="{Binding Cliente.Nome}" ... >`**:
  - Campo de texto para entrada de dados.
  - **`Text="{Binding Cliente.Nome}"`**: data binding em tempo real com a propriedade `Cliente.Nome` do ViewModel.
    - Quando o usuário digita, o texto é automaticamente atualizado no objeto `Cliente`.
    - Quando o código atualiza `Cliente.Nome`, o campo exibe o novo valor.
  - `Keyboard="Email"` (em campo de email): exibe teclado otimizado para email.
  - `Keyboard="Telephone"` (em campo de telefone): exibe teclado numérico.

#### 4.3 Campos do formulário

- **Nome**: `Text="{Binding Cliente.Nome}"`
- **Email**: `Text="{Binding Cliente.Email}"` (nota: campo não existe no model, pode causar erro)
- **Telefone**: `Text="{Binding Cliente.Telefone}"`
- **Endereço**: `Text="{Binding Cliente.Endereco}"`
- **CPF/CNPJ**: `Text="{Binding Cliente.CpfCnpj}"`

#### 4.4 Botões de ação

```xml
<Grid Grid.Row="1" ColumnDefinitions="*,*" Padding="10" ColumnSpacing="10" BackgroundColor="#2a2a2a">
    <Button Grid.Column="0"
           Text="Cancelar"
           Clicked="OnCancelClicked"
           BackgroundColor="#FF3B30"
           TextColor="White"
           CornerRadius="8"
           Padding="15"/>

    <Button Grid.Column="1"
           Text="Salvar"
           Clicked="OnSaveClicked"
           BackgroundColor="#34C759"
           TextColor="White"
           CornerRadius="8"
           Padding="15"/>
</Grid>
```

**Análise:**

- `Grid ColumnDefinitions="*,*"`: 2 colunas de largura igual.
- Botão **Cancelar** (coluna 0):
  - `Clicked="OnCancelClicked"`: descarta mudanças e volta à lista.
  - `BackgroundColor="#FF3B30"`: vermelho (iOS red).
  
- Botão **Salvar** (coluna 1):
  - **`Clicked="OnSaveClicked"`**: dispara o evento de salvamento.
  - `BackgroundColor="#34C759"`: verde (iOS green).
  - **Este é o gatilho para a persistência do cliente.**

---

## 5. ETAPA 3: MODELO DE DADOS (Cliente)

### Arquivo: `Models/Cliente.cs`

```csharp
using SQLite;

namespace Sisvenda.Models
{
    public class Cliente
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CpfCnpj { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Telefone { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Cep { get; set; }
    }
}
```

**Explicação linha por linha:**

- `using SQLite;`:
  - Importa biblioteca SQLite-net para mapeamento objeto-relacional (ORM).
  - Fornece atributos como `[PrimaryKey]`, `[AutoIncrement]`.

- `namespace Sisvenda.Models`:
  - Organiza a classe no namespace de modelos de domínio.

- `public class Cliente`:
  - Define a entidade que representa um cliente no sistema.
  - Corresponde a uma **tabela `Clientes` no banco SQLite**.

- `[PrimaryKey, AutoIncrement]` em `Id`:
  - **`[PrimaryKey]`**: marca como chave primária (identificador único).
  - **`[AutoIncrement]`**: valor é gerado automaticamente pelo banco (1, 2, 3, ...).
  - Assim, não é necessário informar `Id` ao inserir um novo cliente.

- Propriedades públicas de string:
  - `CpfCnpj`: identificação fiscal do cliente.
  - `Nome`: nome da pessoa/empresa.
  - `Endereco`: logradouro (rua, avenida, etc.).
  - `Numero`: número do endereço.
  - `Complemento`: apartamento, sala, etc.
  - `Telefone`: número para contato.
  - `Bairro`: bairro/distrito.
  - `Cidade`: município.
  - `Cep`: código postal.

**Importante:** O XAML da tela de detalhe usa `Cliente.Email`, mas não existe propriedade `Email` nesta classe. Isso causará erro de binding em tempo de execução.

---

## 6. ETAPA 4: VIEWMODEL (ClienteViewModel)

### Arquivo: `ViewModels/ClienteViewModel.cs`

O ViewModel contém a lógica de apresentação e coordena a comunicação entre View e Model/Database.

#### 6.1 Campos e inicialização

```csharp
public class ClienteViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<Cliente> _clientes;
    private Cliente _clienteSelecionado;
    private bool _isLoading;

    public ClienteViewModel()
    {
        _databaseService = new DatabaseService();
        _clientes = new ObservableCollection<Cliente>();

        LoadClientesCommand = new Command(async () => await LoadClientesAsync());
        AddClienteCommand = new Command<Cliente>(async (c) => await AddClienteAsync(c));
        UpdateClienteCommand = new Command<Cliente>(async (c) => await UpdateClienteAsync(c));
        DeleteClienteCommand = new Command<Cliente>(async (c) => await DeleteClienteAsync(c));
    }
}
```

**Linha por linha:**

- `public class ClienteViewModel : INotifyPropertyChanged`:
  - Herda de `INotifyPropertyChanged` para notificar UI quando propriedades mudam (padrão MVVM).

- `private readonly DatabaseService _databaseService;`:
  - Campo privado que armazena instância do serviço de banco.
  - `readonly`: não pode ser reatribuído após inicialização.

- `private ObservableCollection<Cliente> _clientes;`:
  - Coleção observável de clientes.
  - Quando um cliente é adicionado/removido, a UI é atualizada automaticamente.

- `private Cliente _clienteSelecionado;`:
  - Armazena o cliente atualmente selecionado (opcional).

- `private bool _isLoading;`:
  - Flag para indicar se há operação em andamento (carregamento, salvamento).

- `_databaseService = new DatabaseService();`:
  - Cria instância do serviço de banco.

- `_clientes = new ObservableCollection<Cliente>();`:
  - Inicializa coleção vazia.

- Comandos (`LoadClientesCommand`, `AddClienteCommand`, etc.):
  - Definem ações que podem ser disparadas pela UI.
  - Usam `async` para operações não-bloqueantes.

#### 6.2 Propriedades com notificação de mudança

```csharp
public ObservableCollection<Cliente> Clientes
{
    get => _clientes;
    set
    {
        if (_clientes != value)
        {
            _clientes = value;
            OnPropertyChanged();
        }
    }
}

public bool IsLoading
{
    get => _isLoading;
    set
    {
        if (_isLoading != value)
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }
}
```

**Explicação:**

- `get => _clientes;`:
  - Retorna o valor do campo privado.
  - Sintaxe `=>` (expression-bodied property) é equivalente a `return _clientes;`.

- `set { if (_clientes != value) { ... } }`:
  - Valida se o novo valor é diferente do antigo (evita notificações desnecessárias).
  - Se diferente, atualiza `_clientes` e notifica listeners com `OnPropertyChanged()`.

- `OnPropertyChanged();`:
  - Dispara evento `PropertyChanged`, informando à UI que a propriedade mudou.
  - A UI atualiza o binding automaticamente.

#### 6.3 Método principal: AddClienteAsync (CADASTRO)

Este é o **método-chave** que persiste um novo cliente no banco.

```csharp
public async Task AddClienteAsync(Cliente cliente)
{
    try
    {
        if (cliente == null)
            return;

        IsLoading = true;
        await _databaseService.AddClienteAsync(cliente);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Clientes.Add(cliente);
        });

        await Application.Current.MainPage.DisplayAlert("Sucesso", "Cliente adicionado com sucesso!", "OK");
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao adicionar cliente: {ex.Message}", "OK");
    }
    finally
    {
        IsLoading = false;
    }
}
```

**Análise linha por linha detalhada:**

1. **`public async Task AddClienteAsync(Cliente cliente)`**
   - Define método assíncrono que não retorna valor (apenas `Task`).
   - Parâmetro: objeto `Cliente` a ser adicionado.
   - Ao ser chamado, o método não bloqueia a thread principal (permite responsividade da UI).

2. **`try { ... }`**
   - Inicia bloco protegido contra exceções.

3. **`if (cliente == null) return;`**
   - Validação defensiva: se o objeto é nulo, sai da função.
   - Evita tentar persistir um cliente vazio/inválido.

4. **`IsLoading = true;`**
   - Define estado como "carregando".
   - Notifica UI para mostrar indicador de progresso (spinner, etc.).
   - Propriedade `IsLoading` dispara `OnPropertyChanged()` internamente.

5. **`await _databaseService.AddClienteAsync(cliente);`**
   - **LINHA CRÍTICA**: chama o serviço de banco para inserir o cliente no SQLite.
   - `await`: aguarda a conclusão da operação assíncrona.
   - Este é o momento em que o cliente é efetivamente gravado no banco.
   - Se bem-sucedido, o `Id` do cliente é preenchido pelo banco (auto-increment).
   - Se houver erro (validação, banco indisponível, etc.), lança exceção → `catch`.

6. **`MainThread.BeginInvokeOnMainThread(() => { ... })`**
   - Garante que o código interno execute na **thread principal de UI**.
   - Necessário em MAUI porque a operação de banco foi assíncrona (pode estar em thread diferente).
   - Protege contra erro "acesso à UI de thread não-UI".

7. **`Clientes.Add(cliente);`**
   - Adiciona o cliente à coleção observável `_clientes`.
   - Como `Clientes` é observável, a UI (CollectionView) é automaticamente atualizada.
   - O novo cliente aparece imediatamente na lista sem precisar recarregar.

8. **`await Application.Current.MainPage.DisplayAlert("Sucesso", ...);`**
   - Exibe diálogo de confirmação ao usuário.
   - Mensagem: "Cliente adicionado com sucesso!".
   - Aguarda clique do usuário no botão "OK".

9. **`catch (Exception ex) { ... }`**
   - Se ocorrer **qualquer exceção** nas linhas anteriores (banco, validação, etc.).
   - Captura a exceção e evita travamento da app.

10. **`DisplayAlert("Erro", $"Erro ao adicionar cliente: {ex.Message}", ...);`**
    - Informa o usuário sobre o erro.
    - `ex.Message` contém detalhes técnicos (ex: "database locked", "constraint violation").

11. **`finally { IsLoading = false; }`**
    - **Sempre executado**, independente de sucesso ou erro.
    - Define `IsLoading = false` para remover indicador de progresso.
    - Garante que a UI volte ao estado responsivo.

---

## 7. ETAPA 5: PERSISTÊNCIA (DatabaseService)

### Arquivo: `Database/DatabaseService.cs`

Esta classe gerencia todas as operações no banco SQLite.

#### 7.1 Inicialização do banco

```csharp
public class DatabaseService
{
    private SQLiteAsyncConnection _db;
    private const string DbFileName = "sisvenda.db";
    private string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
    }

    public async Task InitializeAsync()
    {
        if (_db != null)
            return;

        _db = new SQLiteAsyncConnection(_dbPath);
        await _db.CreateTableAsync<Produto>();
        await _db.CreateTableAsync<Cliente>();
        await _db.CreateTableAsync<Venda>();
        await _db.CreateTableAsync<ItemVenda>();
    }
}
```

**Explicação:**

- `private SQLiteAsyncConnection _db;`:
  - Conexão com banco SQLite (lazy-loaded, criado sob demanda).

- `private const string DbFileName = "sisvenda.db";`:
  - Nome do arquivo do banco.

- `_dbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);`:
  - Caminho completo do banco no diretório de dados da app.
  - Exemplo: `/data/user/0/Sisvenda/files/sisvenda.db` (Android).

- `InitializeAsync()`:
  - Garante que a conexão está aberta e tabelas existem.
  - Chamado no início de cada operação (por segurança).
  - `CreateTableAsync<Produto>()` cria tabela `Produtos` se não existir (baseado na classe `Produto`).
  - Similar para `Cliente`, `Venda`, `ItemVenda`.

#### 7.2 Método de inserção de cliente: AddClienteAsync

```csharp
public async Task<int> AddClienteAsync(Cliente cliente)
{
    await InitializeAsync();
    return await _db.InsertAsync(cliente);
}
```

**Linha por linha:**

1. **`public async Task<int> AddClienteAsync(Cliente cliente)`**
   - Método assíncrono que retorna um inteiro (ID do cliente inserido).
   - Parâmetro: objeto `Cliente` a ser persistido.

2. **`await InitializeAsync();`**
   - Prepara a conexão com o banco (abre se necessário, cria tabelas).
   - Assíncrono, não bloqueia app.

3. **`return await _db.InsertAsync(cliente);`**
   - **LINHA FINAL DO CADASTRO**: executa comando SQL `INSERT` no banco.
   - `InsertAsync(cliente)` do SQLite-net:
     - Mapeia propriedades do objeto `Cliente` para colunas da tabela `Clientes`.
     - Gera SQL: `INSERT INTO Clientes (CpfCnpj, Nome, Endereco, ...) VALUES (?, ?, ?, ...);`
     - Executa no banco.
     - Retorna o `Id` auto-increment atribuído ao novo registro.
   - Se bem-sucedido, cliente está salvo no SQLite.
   - Se erro (violação de constraint, disco cheio, etc.), lança exceção.

**O que acontece no SQLite exato:**

Quando `_db.InsertAsync(cliente)` é executado com um objeto como:

```
Cliente:
  Id = 0 (ou null, pois é auto-increment)
  Nome = "João Silva"
  CpfCnpj = "123.456.789-00"
  Telefone = "(11) 99999-9999"
  Endereco = "Rua A, 123"
  ...
```

O SQLite-net gera e executa algo como:

```sql
INSERT INTO Clientes 
  (CpfCnpj, Nome, Telefone, Endereco, Numero, Complemento, Bairro, Cidade, Cep) 
VALUES 
  ('123.456.789-00', 'João Silva', '(11) 99999-9999', 'Rua A', '123', NULL, NULL, NULL, NULL);

-- Banco retorna Id = 1 (primeiro cliente)
```

O registro fica permanentemente no arquivo `sisvenda.db`.

---

## 8. FLUXO COMPLETO PASSO A PASSO

Aqui está a sequência **exata** desde o clique até a gravação:

### Passo 1: Clique no botão (+)
**Local:** `ClienteListPage.xaml`  
**XML:**
```xml
<Button Text="+" Clicked="OnAddClienteClicked" ... />
```
**Ação:** O evento `OnAddClienteClicked` é disparado no code-behind `ClienteListPage.xaml.cs`.

### Passo 2: Abertura da tela de detalhe (presumido em OnAddClienteClicked)
**Local:** `ClienteListPage.xaml.cs` (não fornecido, mas presumido)  
**Ação:**
```csharp
// Abre tela de cadastro com novo Cliente
await Shell.Current.GoToAsync("cliente-detail");
// ou
await Navigation.PushAsync(new ClienteDetailPage());
```

### Passo 3: Usuário preenche o formulário
**Local:** `ClienteDetailPage.xaml`  
**Dados preenchidos:**
- `Entry Text="{Binding Cliente.Nome}"` recebe "João Silva"
- `Entry Text="{Binding Cliente.Telefone}"` recebe "(11) 99999-9999"
- `Entry Text="{Binding Cliente.CpfCnpj}"` recebe "123.456.789-00"
- etc.

**Binding:** Cada campo está ligado via `{Binding Cliente.*}`, então o objeto `Cliente` é atualizado em tempo real conforme o usuário digita.

### Passo 4: Clique no botão "Salvar"
**Local:** `ClienteDetailPage.xaml`  
**XML:**
```xml
<Button Text="Salvar" Clicked="OnSaveClicked" BackgroundColor="#34C759" ... />
```
**Ação:** O evento `OnSaveClicked` é disparado no code-behind `ClienteDetailPage.xaml.cs` (não fornecido, mas presumido).

### Passo 5: Envio para ViewModel
**Local:** `ClienteDetailPage.xaml.cs` (presumido)  
**Ação:**
```csharp
private void OnSaveClicked(object sender, EventArgs e)
{
    var viewModel = this.BindingContext as ClienteViewModel;
    viewModel.AddClienteCommand.Execute(this.BindingContext); // ou chama direto
    // await viewModel.AddClienteAsync(cliente);
}
```

### Passo 6: Execução de AddClienteAsync no ViewModel
**Local:** `ClienteViewModel.cs`  
**Trecho executado:**
```csharp
public async Task AddClienteAsync(Cliente cliente)
{
    try
    {
        if (cliente == null)
            return;  // Passo 6a: Validação

        IsLoading = true;  // Passo 6b: Marca como carregando

        await _databaseService.AddClienteAsync(cliente);  // Passo 6c: Chama persistência

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Clientes.Add(cliente);  // Passo 6d: Atualiza coleção da UI
        });

        await Application.Current.MainPage.DisplayAlert("Sucesso", "Cliente adicionado com sucesso!", "OK");  // Passo 6e: Confirma
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao adicionar cliente: {ex.Message}", "OK");
    }
    finally
    {
        IsLoading = false;  // Passo 6f: Remove carregamento
    }
}
```

### Passo 7: Persistência no banco
**Local:** `DatabaseService.cs`  
**Trecho executado:**
```csharp
public async Task<int> AddClienteAsync(Cliente cliente)
{
    await InitializeAsync();  // Passo 7a: Abre/prepara conexão

    return await _db.InsertAsync(cliente);  // Passo 7b: INSERE no SQLite
}
```

**O que o SQLite faz:**

1. Valida constraints (tipo, comprimento, etc.).
2. Gera `INSERT INTO Clientes (...)`.
3. Executa no arquivo `sisvenda.db`.
4. Retorna novo `Id` (auto-increment).
5. **Cliente está permanentemente gravado.**

### Passo 8: Atualização da UI
**Volta ao ViewModel em `Clientes.Add(cliente);`:**
- `Clientes` é `ObservableCollection<Cliente>`.
- Adicionar um item dispara notificação.
- `CollectionView ItemsSource="{Binding Clientes}"` recebe notificação.
- A UI renderiza o novo cliente na lista instantaneamente.

### Passo 9: Confirmação ao usuário
**Exibe `DisplayAlert("Sucesso", ...)`:**
- Diálogo visual informando êxito.
- Usuário clica "OK" e volta à lista.

---

## 9. DIAGRAMA DE SEQUÊNCIA

```
[Usuário] ──→ Toca (+)
                    │
                    ├──→ ClienteListPage → OnAddClienteClicked
                    │    │
                    │    └──→ Abre ClienteDetailPage
                    │
[Usuário] ──→ Preenche campos
                    │
                    ├──→ Binding: Cliente.Nome, Cliente.Telefone, etc.
                    │    Objeto Cliente é atualizado em tempo real
                    │
[Usuário] ──→ Toca "Salvar"
                    │
                    ├──→ ClienteDetailPage → OnSaveClicked
                    │    │
                    │    └──→ ClienteViewModel.AddClienteAsync(cliente)
                    │         │
                    │         ├──1. IsLoading = true
                    │         ├──2. DatabaseService.AddClienteAsync(cliente)
                    │         │    │
                    │         │    ├──A. InitializeAsync()
                    │         │    │    └──Abre conexão SQLite
                    │         │    │
                    │         │    └──B. _db.InsertAsync(cliente)
                    │         │         └──Executa: INSERT INTO Clientes (...)
                    │         │            └──[SQLite gravar no arquivo]
                    │         │
                    │         ├──3. Clientes.Add(cliente)
                    │         │    └──CollectionView atualiza (novo cliente aparece)
                    │         │
                    │         ├──4. DisplayAlert("Sucesso", ...)
                    │         │
                    │         └──5. IsLoading = false
                    │
[Usuário] ← Vê novo cliente na lista + confirmação
```

---

## 10. CONCLUSÃO

O fluxo de cadastro de cliente no **SisVenda** é um exemplo completo de arquitetura em camadas com MVVM:

1. **UI (View)**: Oferece interface intuitiva com botão `+` e formulário.
2. **ViewModel**: Coordena lógica, valida dados, gerencia estado de loading.
3. **Model**: Define estrutura de dados (classe `Cliente`).
4. **Persistência (DatabaseService)**: Executa operações SQL no SQLite.

### Linha crítica de persistência:
```csharp
await _db.InsertAsync(cliente);  // Em DatabaseService.AddClienteAsync
```

### Sequência de chamadas:
```
OnAddClienteClicked → ClienteDetailPage → OnSaveClicked → 
AddClienteAsync (ViewModel) → DatabaseService.AddClienteAsync → 
_db.InsertAsync(cliente) → [SQLite INSERT] → 
Clientes.Add(cliente) → CollectionView atualiza → DisplayAlert
```

### Responsabilidades de cada camada:

| Camada | Arquivo | Responsabilidade |
|--------|---------|------------------|
| View | ClienteListPage.xaml | Exibir lista e botão `+` |
| View | ClienteDetailPage.xaml | Formulário de entrada |
| ViewModel | ClienteViewModel.cs | Lógica de negócio, CRUD, notificações |
| Model | Cliente.cs | Estrutura de dados |
| Data | DatabaseService.cs | Comunicação com SQLite |
| Banco | sisvenda.db (SQLite) | Persistência permanente |

---

**Documento preparado para SisVenda — Projeto Final de Programação Visual**  
**Data de geração:** 09/07/2026  
**Arquitetura:** MVVM (Model-View-ViewModel)  
**Banco de dados:** SQLite (sqlite-net)
