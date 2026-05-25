using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Models;
namespace WebBanHang.Services
{
	public class ElasticSearchService
	{
		private readonly IElasticClient _elasticClient;

		public ElasticSearchService()
		{
			var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
							.DefaultIndex("products");

			_elasticClient = new ElasticClient(settings);
		}

		public async Task IndexProductAsync(Product product)
		{
			await _elasticClient.IndexDocumentAsync(product);
		}

		public async Task BulkIndexProductsAsync(IEnumerable<Product> products)
		{
			var bulkIndexResponse = await _elasticClient.BulkAsync(b => b
				.Index("products")
				.IndexMany(products));

			if (bulkIndexResponse.Errors)
			{
				// Handle errors (log them, etc.)
			}
		}

		public async Task<List<Product>> SearchProductsAsync(string keyword)
		{
			var searchResponse = await _elasticClient.SearchAsync<Product>(s => s
				.Query(q => q
					.MultiMatch(m => m
						.Fields(f => f
							.Field(p => p.Name)
							.Field(p => p.Description))
						.Query(keyword)
						.Fuzziness(Fuzziness.Auto)
					)
				)
			);

			return searchResponse.Documents.ToList();
		}
	}

}
