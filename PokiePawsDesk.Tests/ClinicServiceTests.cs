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
    public class ClinicServiceTests
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
        public async Task GetClinicsAsync_ApiReturnsClinics_SyncsToLocalDbAndReturns()
        {
            var clinics = new List<Clinic>
            {
                new() { Id = 1, ClinicName = "PokiePaws Warszawa",  Active = true },
                new() { Id = 2, ClinicName = "PokiePaws Kraków",    Active = true },
                new() { Id = 3, ClinicName = "PokiePaws Wrocław",   Active = false },
            };
            var db = CreateDb();
            var service = new ClinicService(MakeClient(clinics), db);

            var result = await service.GetClinicsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal(3, await db.Clinics.CountAsync());
        }

        [Fact]
        public async Task GetClinicsAsync_ApiFails_ReturnsLocalFallback()
        {
            var db = CreateDb();
            db.Clinics.Add(new Clinic { Id = 1, ClinicName = "Cached Clinic", Active = true });
            await db.SaveChangesAsync();

            var handler = new MessageHandler(_ => throw new HttpRequestException("offline"));
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ClinicService(client, db);

            var result = await service.GetClinicsAsync();

            Assert.Single(result);
            Assert.Equal("Cached Clinic", result[0].ClinicName);
        }

        [Fact]
        public async Task GetClinicsAsync_ReplacesLocalDbWithFreshData()
        {
            var db = CreateDb();
            db.Clinics.Add(new Clinic { Id = 1, ClinicName = "Stara klinika", Active = true });
            await db.SaveChangesAsync();

            var freshClinics = new List<Clinic>
            {
                new() { Id = 10, ClinicName = "Nowa klinika A", Active = true },
                new() { Id = 11, ClinicName = "Nowa klinika B", Active = true },
            };
            var service = new ClinicService(MakeClient(freshClinics), db);

            await service.GetClinicsAsync();

            var local = db.Clinics.ToList();
            Assert.Equal(2, local.Count);
            Assert.DoesNotContain(local, c => c.ClinicName == "Stara klinika");
        }

        [Fact]
        public async Task GetClinicsAsync_SendsGetToCorrectEndpoint()
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
            var service = new ClinicService(client, CreateDb());

            await service.GetClinicsAsync();

            Assert.Equal(HttpMethod.Get, captured?.Method);
            Assert.Contains("/api/clinics", captured?.RequestUri?.ToString());
        }

        [Fact]
        public void GetLocalClinics_ReturnsOnlyLocalDbData()
        {
            var db = CreateDb();
            db.Clinics.AddRange(
                new Clinic { Id = 1, ClinicName = "Klinika A", Active = true },
                new Clinic { Id = 2, ClinicName = "Klinika B", Active = false }
            );
            db.SaveChanges();

            var handler = new MessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ClinicService(client, db);

            var result = service.GetLocalClinics();

            Assert.Equal(2, result.Count);
        }
    }
}