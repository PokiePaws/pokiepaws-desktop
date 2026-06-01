using Microsoft.EntityFrameworkCore;
using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using PokiePawsDesk.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokiePawsDesk.Tests
{
    public class ProductServiceTests
    {
        private static AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static HttpClient MakeClient(object body, HttpStatusCode status = HttpStatusCode.OK)
        {
            var json = JsonSerializer.Serialize(body);
            var handler = new MessageHandler(_ =>
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            return new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        }

        [Fact]
        public async Task GetProductsAsync_ApiReturnsProducts_SyncsToLocalDbAndReturns()
        {
            var products = new List<Product>
            {
                new() { Id = 1, Name = "Amoksycylina", Category = "Antybiotyki", Amount = 150 },
                new() { Id = 2, Name = "Karprogen",    Category = "Leki",        Amount = 200 },
            };
            var db = CreateDb();
            var service = new ProductService(MakeClient(products), db);

            var result = await service.GetProductsAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, await db.Products.CountAsync());
        }

        [Fact]
        public async Task GetProductsAsync_ApiFails_ReturnsLocalFallback()
        {
            var db = CreateDb();
            db.Products.Add(new Product { Id = 1, Name = "Cached", Category = "Leki", Amount = 10 });
            await db.SaveChangesAsync();

            var handler = new MessageHandler(_ => throw new HttpRequestException("offline"));
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ProductService(client, db);

            var result = await service.GetProductsAsync();

            Assert.Single(result);
            Assert.Equal("Cached", result[0].Name);
        }

        [Fact]
        public async Task CreateAsync_SendsPostToCorrectEndpoint_ReturnsCreatedProduct()
        {
            var created = new Product { Id = 99, Name = "Nowy produkt", Category = "Inne", Amount = 0 };
            HttpRequestMessage? captured = null;
            var handler = new MessageHandler(req =>
            {
                captured = req;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(created), Encoding.UTF8, "application/json")
                };
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ProductService(client, CreateDb());

            var result = await service.CreateAsync(new Product { Name = "Nowy produkt" });

            Assert.Equal(99, result?.Id);
            Assert.Equal(HttpMethod.Post, captured?.Method);
            Assert.Contains("/api/warehouse/stock", captured?.RequestUri?.ToString());
        }

        [Fact]
        public async Task UpdateAsync_SendsPutToCorrectEndpoint_ReturnsUpdatedProduct()
        {
            var updated = new Product { Id = 1, Name = "Zaktualizowany", Amount = 55 };
            HttpRequestMessage? captured = null;
            var handler = new MessageHandler(req =>
            {
                captured = req;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(updated), Encoding.UTF8, "application/json")
                };
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ProductService(client, CreateDb());

            var result = await service.UpdateAsync(new Product { Id = 1, Name = "Zaktualizowany" });

            Assert.Equal("Zaktualizowany", result?.Name);
            Assert.Equal(HttpMethod.Put, captured?.Method);
            Assert.Contains("/api/warehouse/stock/1", captured?.RequestUri?.ToString());
        }

        [Fact]
        public async Task DeleteAsync_SendsDeleteToCorrectEndpoint()
        {
            HttpRequestMessage? captured = null;
            var handler = new MessageHandler(req =>
            {
                captured = req;
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ProductService(client, CreateDb());

            await service.DeleteAsync(42);

            Assert.Equal(HttpMethod.Delete, captured?.Method);
            Assert.Contains("/api/warehouse/stock/42", captured?.RequestUri?.ToString());
        }
    }
}