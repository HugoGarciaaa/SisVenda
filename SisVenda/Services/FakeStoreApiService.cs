using System.Diagnostics;
using System.Net.Http.Json;
using Sisvenda.Database;
using Sisvenda.Models;

namespace Sisvenda.Services
{
    public class FakeStoreApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://fakestoreapi.com";

        public FakeStoreApiService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Obtém todos os produtos da Fake Store API
        /// </summary>
        public async Task<List<FakeStoreApiProduct>> GetAllProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<FakeStoreApiProduct>>($"{BaseUrl}/products");
                return response ?? new List<FakeStoreApiProduct>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Erro ao obter produtos da API: {ex.Message}");
                return new List<FakeStoreApiProduct>();
            }
        }

        /// <summary>
        /// Obtém produtos de uma categoria específica
        /// </summary>
        public async Task<List<FakeStoreApiProduct>> GetProductsByCategoryAsync(string category)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<FakeStoreApiProduct>>($"{BaseUrl}/products/category/{category}");
                return response ?? new List<FakeStoreApiProduct>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Erro ao obter produtos por categoria: {ex.Message}");
                return new List<FakeStoreApiProduct>();
            }
        }

        /// <summary>
        /// Obtém todas as categorias disponíveis
        /// </summary>
        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<string>>($"{BaseUrl}/products/categories");
                return response ?? new List<string>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Erro ao obter categorias: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Mapeia um produto da API para o modelo local Produto
        /// </summary>
        public Produto MapApiProductToProduto(FakeStoreApiProduct apiProduct)
        {
            // Calcula o custo como 70% do preço (margem de lucro de 30%)
            decimal precoCusto = apiProduct.Price * 0.70m;

            return new Produto
            {
                Codigo = apiProduct.Id.ToString(),
                Nome = apiProduct.Title,
                Descricao = apiProduct.Description,
                Preco = apiProduct.Price,
                PrecoCusto = Math.Round(precoCusto, 2),
                ImagemUrl = apiProduct.Image,
                Categoria = apiProduct.Category
            };
        }

        /// <summary>
        /// Importa produtos da API para o banco de dados local, evitando duplicatas
        /// </summary>
        public async Task<int> ImportProductsAsync(DatabaseService databaseService)
        {
            try
            {
                var apiProducts = await GetAllProductsAsync();

                if (apiProducts == null || apiProducts.Count == 0)
                {
                    Debug.WriteLine("Nenhum produto retornado pela API");
                    return 0;
                }

                int importedCount = 0;

                foreach (var apiProduct in apiProducts)
                {
                    // Verifica se o produto já existe (baseado no Codigo que é mapeado do ID da API)
                    var existingProduct = await databaseService.GetProdutoByCodigoAsync(apiProduct.Id.ToString());

                    if (existingProduct == null)
                    {
                        var produto = MapApiProductToProduto(apiProduct);
                        await databaseService.AddProdutoAsync(produto);
                        importedCount++;
                    }
                }

                Debug.WriteLine($"Produtos importados com sucesso: {importedCount}");
                return importedCount;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro durante importação de produtos: {ex.Message}");
                return 0;
            }
        }
    }
}
