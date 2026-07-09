# Importação de Produtos da Fake Store API

## Visão Geral

Agora sua aplicação SisVenda suporta importação automática de produtos da Fake Store API (https://fakestoreapi.com/). A implementação evita duplicação de produtos e mapeia os campos da API para o modelo local.

## Mapeamento de Campos

| Campo da API | Campo Local | Tipo |
|---|---|---|
| id | Codigo | string |
| title | Nome | string |
| description | Descricao | string |
| price | Preco | decimal |
| category | Categoria | string |
| image | ImageUrl | string |

## Estrutura Implementada

### 1. **Modelo Produto Atualizado** (`Models/Produto.cs`)
- Adicionado campo `ImageUrl` para armazenar a URL da imagem
- Adicionado campo `Categoria` para armazenar a categoria do produto

### 2. **DTO da API** (`Models/FakeStoreApiProduct.cs`)
- Classe para mapear a resposta JSON da API
- Inclui classes para desserialização com System.Text.Json

### 3. **Serviço de API** (`Services/FakeStoreApiService.cs`)
- `GetAllProductsAsync()` - Busca todos os produtos da API
- `MapApiProductToProduto()` - Converte produto da API para modelo local

### 4. **Serviço de Banco de Dados** (`Database/DatabaseService.cs`)
- `GetProdutoByCodigoAsync()` - Busca produto por código (evita duplicatas)
- `ImportarProdutosFakeStoreAsync()` - Importa produtos sem duplicação

### 5. **Serviço de Seed** (`Services/SeedDataService.cs`)
- `ImportarProdutosFakeStoreAsync()` - Método público para importação

## Como Usar

### Importar Produtos da API

```csharp
// Injetar o SeedDataService (já está registrado em MauiProgram.cs)
var seedService = ServiceHelper.Current.GetService<SeedDataService>();

// Importar produtos
var (adicionados, duplicados) = await seedService.ImportarProdutosFakeStoreAsync();

Console.WriteLine($"? {adicionados} produtos adicionados");
Console.WriteLine($"?? {duplicados} produtos duplicados");
```

### Acessar Produtos com Imagens

```csharp
var dbService = ServiceHelper.Current.GetService<DatabaseService>();
var produtos = await dbService.GetProdutosAsync();

// Cada produto agora tem ImageUrl e Categoria
foreach (var produto in produtos)
{
    Console.WriteLine($"{produto.Nome} - {produto.Categoria}");
    Console.WriteLine($"Imagem: {produto.ImageUrl}");
}
```

## Características

? **Sem Duplicação**: Verifica pelo código antes de adicionar  
? **Mapeamento Automático**: Converte automaticamente os campos  
? **Tratamento de Erros**: Logging de operações  
? **Injeção de Dependência**: Totalmente integrado ao IoC  
? **Imagens Incluídas**: URLs de imagens armazenadas localmente  
? **Categorias**: Todas as categorias da API são preservadas  

## Exemplos de Categorias Disponíveis

- electronics
- jewelery
- men's clothing
- women's clothing

## Notas

- O campo `PrecoCusto` é definido como 0m durante a importação
- Os IDs dos produtos da API são usados como código (1-20)
- As imagens não são baixadas, apenas as URLs são armazenadas
- A importação respeita os produtos existentes no banco

## Próximos Passos

Para exibir as imagens na UI, use controles como:
- `Image` (MAUI) com a propriedade `Source` apontando para `produto.ImageUrl`
- Considere adicionar cache de imagens para melhor performance
