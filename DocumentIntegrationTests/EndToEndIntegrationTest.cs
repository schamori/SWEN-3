using DAL.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using SharedResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using WebApplicationSWEN3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DotNet.Testcontainers.Builders;
using Npgsql;
using Docker.DotNet.Models;
using Microsoft.Extensions.Hosting;

namespace DocumentIntegrationTests
{
    [TestFixture]
    public class EndToEndIntegrationTest
    {
        private HttpClient _client;
        private PostgreSqlContainer _postgresContainer;
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public async Task Setup()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithDatabase("documentsearch")
                .WithUsername("mamo")
                .WithPassword("T1P3m!hvQ9")
                .WithHostname("paperless-postgres")
                .WithNetwork("paperless-network")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();

            await _postgresContainer.StartAsync();

            var connectionString = "Host=paperless-postgres;Port=5432;Database=documentsearch;Username=mamo;Password=T1P3m!hvQ9";
            Console.WriteLine($"Postgres Connection String: {connectionString}");

            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                Console.WriteLine("Connection to PostgreSQL successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PostgreSQL connection failed: {ex.Message}");
                throw;
            }

            // WebApplicationFactory konfigurieren
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new[]
                        {
                    new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", connectionString)
                        });
                    });
                });

            _client = _factory.CreateClient();
        }


        [TearDown]
        public async Task TearDown()
        {
            if (_postgresContainer != null)
            {
                await _postgresContainer.DisposeAsync();
            }

            _factory?.Dispose();
            _client?.Dispose();
        }

        [Test]
        public async Task UploadDocument_ProcessOCR_VerifyResults()
        {
            // Arrange
            var testFilePath = "testfile.pdf";
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(File.OpenRead(testFilePath)), "file", "testfile.pdf");

            var uploadResponse = await _client.PostAsync("/api/document", content);
            Assert.AreEqual(HttpStatusCode.Created, uploadResponse.StatusCode);

            var uploadResponseBody = await uploadResponse.Content.ReadAsStringAsync();
            var documentId = ExtractDocumentId(uploadResponseBody);

            bool ocrIndexed = await WaitForElasticSearch(documentId);
            Assert.IsTrue(ocrIndexed, "OCR result not found in ElasticSearch");

            var fileInMinIO = await GetFileFromMinIO(documentId);
            Assert.IsNotNull(fileInMinIO, "File not found in MinIO storage");

            var dbDocument = GetDocumentFromDatabase(documentId);
            Assert.IsNotNull(dbDocument, "Document metadata not found in PostgreSQL");
            Assert.AreEqual("testfile.pdf", dbDocument.Title);
        }

        private async Task<bool> WaitForElasticSearch(Guid documentId)
        {
            var retries = 10;
            while (retries > 0)
            {
                var response = await new HttpClient().GetAsync($"http://elasticsearch:9200/documents/_doc/{documentId}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                await Task.Delay(1000);
                retries--;
            }
            return false;
        }

        private async Task<byte[]> GetFileFromMinIO(Guid documentId)
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://minio:9000") };
            var response = await client.GetAsync($"/documents/{documentId}");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
        }

        private DocumentDAL GetDocumentFromDatabase(Guid documentId)
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return context.Documents.FirstOrDefault(d => d.Id == documentId);
        }

        private Guid ExtractDocumentId(string responseBody)
        {
            var match = Regex.Match(responseBody, @"""id"":""(?<id>[^""]+)""");
            return Guid.Parse(match.Groups["id"].Value);
        }
    }

}
