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
    public class OrderServiceTests
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
        public async Task GetOrdersAsync_ApiReturnsOrders_SyncsToLocalDbAndReturns()
        {
            var orders = new List<Order>
            {
                new() { Id = 1, Status = "PENDING",     ClinicId = 1, Name = "Amoksycylina" },
                new() { Id = 2, Status = "IN_PROGRESS", ClinicId = 2, Name = "Bandaż" },
            };
            var db = CreateDb();
            var service = new OrderService(MakeClient(orders), db);

            var result = await service.GetOrdersAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, await db.Orders.CountAsync());
        }

        [Fact]
        public async Task GetOrdersAsync_ApiFails_ReturnsLocalFallback()
        {
            var db = CreateDb();
            db.Orders.Add(new Order { Id = 1, Status = "PENDING", Name = "Cached" });
            await db.SaveChangesAsync();

            var handler = new MessageHandler(_ => throw new HttpRequestException("offline"));
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new OrderService(client, db);

            var result = await service.GetOrdersAsync();

            Assert.Single(result);
            Assert.Equal("Cached", result[0].Name);
        }

        [Fact]
        public async Task GetOrdersAsync_WithClinicIdFilter_SendsCorrectQueryParam()
        {
            HttpRequestMessage? captured = null;
            var handler = new MessageHandler(req =>
            {
                captured = req;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", Encoding.UTF8, "application/json")
                };
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new OrderService(client, CreateDb());

            await service.GetOrdersAsync(clinicId: 5);

            Assert.Contains("clinicId=5", captured?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetOrdersAsync_WithStatusFilter_SendsCorrectQueryParam()
        {
            HttpRequestMessage? captured = null;
            var handler = new MessageHandler(req =>
            {
                captured = req;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", Encoding.UTF8, "application/json")
                };
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new OrderService(client, CreateDb());

            await service.GetOrdersAsync(status: "PENDING");

            Assert.Contains("status=PENDING", captured?.RequestUri?.ToString());
        }

        [Fact]
        public async Task UpdateStatusAsync_SendsPutToCorrectEndpointWithStatus()
        {
            var updated = new Order { Id = 3, Status = "IN_PROGRESS" };
            HttpRequestMessage? captured = null;
            string? capturedBody = null;
            var handler = new MessageHandler(req =>
            {
                captured = req;
                capturedBody = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(updated), Encoding.UTF8, "application/json")
                };
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new OrderService(client, CreateDb());

            var result = await service.UpdateStatusAsync(3, "IN_PROGRESS");

            Assert.Equal(HttpMethod.Put, captured?.Method);
            Assert.Contains("/api/orders/3/status", captured?.RequestUri?.ToString());
            Assert.Contains("IN_PROGRESS", capturedBody);
            Assert.Equal("IN_PROGRESS", result?.Status);
        }
    }
}