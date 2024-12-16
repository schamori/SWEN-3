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
using System.Net.Http.Json;
using SharedResources.DTO;
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

            // WebApplicationFactory konfigurieren
            _factory = new WebApplicationFactory<Program>();

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
        public async Task UploadDocument_Search_Download_VerifyResults()
        {
            // Arrange
            var testFilePath = "testfile.pdf";
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(File.OpenRead(testFilePath)), "file", "testfile.pdf");

            // Act - Upload
            var uploadResponse = await _client.PostAsync("/api/document", content);
            Assert.That(uploadResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            // Get document ID from response
            var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<DocumentDTO>();
            Assert.That(uploadResult, Is.Not.Null);
            Assert.That(uploadResult.Id, Is.Not.Empty);

            // Act - Search
            var searchResponse = await _client.GetAsync("/api/document/search?query=Cambridge");
            Assert.That(searchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Act - Download
            var downloadResponse = await _client.GetAsync($"/api/document/download/{uploadResult.Id}");
            Assert.That(downloadResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Verify download content
            var downloadedContent = await downloadResponse.Content.ReadAsByteArrayAsync();
            Assert.That(downloadedContent.Length, Is.GreaterThan(0));
        }
    }





}


