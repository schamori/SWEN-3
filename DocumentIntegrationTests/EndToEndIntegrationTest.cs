using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using System.IO;
using System.Net;
using Testcontainers.RabbitMq;
using WebApplicationSWEN3; // Ersetze dies mit dem tatsächlichen Namespace deiner Anwendung
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Testcontainers.PostgreSql;

namespace DocumentIntegrationTests
{
    [TestFixture]
    public class DocumentControllerIntegrationTests
    {
        private HttpClient _client;
        private PostgreSqlContainer _postgresContainer;
        private RabbitMqContainer _rabbitMqContainer;
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public async Task Setup()
        {
            // Starte PostgreSQL Testcontainer
            _postgresContainer = new PostgreSqlBuilder()
                .WithDatabase("documentsearch")
                .WithUsername("mamo")
                .WithPassword("T1P3m!hvQ9")
                .Build();

            await _postgresContainer.StartAsync();
            Console.WriteLine(await _postgresContainer.GetLogsAsync());

            // Starte RabbitMQ Testcontainer
            _rabbitMqContainer = new RabbitMqBuilder()
                .WithUsername("user")
                .WithPassword("password")
                .Build();

            await _rabbitMqContainer.StartAsync();
            Console.WriteLine(await _rabbitMqContainer.GetLogsAsync());

            // Manuelles Erstellen der Verbindungszeichenfolgen
            var postgresHost = "localhost";
            var postgresPort = "5432";
            var postgresDatabase = "documentsearch";
            var postgresUsername = "mamo";
            var postgresPassword = "T1P3m!hvQ9";

            var postgresConnectionString = $"Host={postgresHost};Port={postgresPort};Database={postgresDatabase};Username={postgresUsername};Password={postgresPassword}";

            var rabbitMqHost = "localhost";
            var rabbitMqPort = "5672";
            var rabbitMqUsername = "user";
            var rabbitMqPassword = "password";

            var rabbitMqConnectionString1 = $"amqp://{rabbitMqUsername}:{rabbitMqPassword}@{rabbitMqHost}:{rabbitMqPort}";
            var rabbitMqConnectionString2 = $"amqp://{rabbitMqUsername}:{rabbitMqPassword}@{rabbitMqHost}:{rabbitMqPort}";

            Console.WriteLine($"Postgres Connection String: {postgresConnectionString}");
            Console.WriteLine($"RabbitMQ Connection String: {rabbitMqConnectionString1}");

            // Erstelle WebApplicationFactory mit überschriebenen Konfigurationen
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("postgresConnectionString", postgresConnectionString),
                            new KeyValuePair<string, string>("rabbitMqConnectionString1", rabbitMqConnectionString1),
                            new KeyValuePair<string, string>("rabbitMqConnectionString2", rabbitMqConnectionString2)
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

            if (_rabbitMqContainer != null)
            {
                await _rabbitMqContainer.DisposeAsync();
            }

            _factory?.Dispose();
            _client?.Dispose();
        }

        [Test]
        public async Task PostDocument_UploadsFile_ReturnsCreated()
        {
            try
            {
                // Arrange
                var filePath = "testfile.pdf";

                // Stelle sicher, dass die Datei existiert
                if (!File.Exists(filePath))
                {
                    // Erstelle eine temporäre PDF-Datei für den Test
                    using (var fs = File.Create(filePath))
                    {
                        byte[] info = new System.Text.UTF8Encoding(true).GetBytes("This is a test PDF file.");
                        fs.Write(info, 0, info.Length);
                    }
                }

                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath));

                // Act
                var response = await _client.PostAsync("/api/document", content);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Expected status code 201 Created.");

                var responseBody = await response.Content.ReadAsStringAsync();
                Assert.IsTrue(responseBody.Contains("id"), "Response body does not contain 'id'.");
            }
            catch (Exception ex)
            {
                // Ausgabe der Logs zur Fehlerbehebung
                Console.WriteLine("PostgreSQL Logs:");
                Console.WriteLine(await _postgresContainer.GetLogsAsync());

                Console.WriteLine("RabbitMQ Logs:");
                Console.WriteLine(await _rabbitMqContainer.GetLogsAsync());

                throw; // Re-throw the exception nach dem Loggen
            }
        }
    }
}