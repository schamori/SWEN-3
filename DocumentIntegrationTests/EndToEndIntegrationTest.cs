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
        public async Task UploadDocument_ProcessOCR_VerifyResults()
        {
            // Arrange
            var testFilePath = "testfile.pdf";
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(File.OpenRead(testFilePath)), "file", "testfile.pdf");

            var uploadResponse = await _client.PostAsync("/api/document", content);
            Assert.AreEqual(HttpStatusCode.Created, uploadResponse.StatusCode);

         
        }

        

        
    }

}
