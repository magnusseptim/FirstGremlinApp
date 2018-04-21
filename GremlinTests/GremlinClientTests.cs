using FirstGremlinApp;
using FirstGremlinApp.Model;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace GremlinTests
{
    public class GremlinClientTests : IDisposable
    {
        private MockRepository mockRepository;



        public GremlinClientTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async void CheckIfDatabaseCreatedCorrect()
        {
            // Arrange
            GremlinClient gremlinClient = this.CreateGremlinClient();
            // Act
            var response = await gremlinClient.CreateDatabase("StoneGargoyle", "Gargoyle's Factory ");

            // Assert
            response.Should().NotBeNull();
        }

        [Fact]
        public async void CheckIfCollectionCreatedCorrect()
        {
            // Arrange
            GremlinClient gremlinClient = this.CreateGremlinClient();

            // Act
            var response = await gremlinClient.CreateGraphCollection("StoneGargoyle", "MarableGargoyles");

            // Assert
            response.Should().NotBeNull();
        }

        [Fact]
        public async void CheckIfVertexCreatedCorrect()
        {
            // Arrange
            GremlinClient gremlinClient = this.CreateGremlinClient();

            // Act
            var responseCollection = await gremlinClient.CreateGraphCollection("StoneGargoyle","MarableGargoyles");
            var response = await gremlinClient.CreateEmptyVertex(responseCollection.Resource,"SmallGargoyle");

            // Assert
            response.Should().NotBeNull();
        }

        [Fact]
        public void CheckIfBuildGremlinQueryWorksCorrect()
        {
            // Arrange
            GremlinClient gremlinClient = this.CreateGremlinClient();
            JToken jToken = JToken.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "appsettings.json")));
            var list = new List<KeyValuePair<string, string[]>>();
            string query = ((List<Setting>)jToken["configuration"]["appSettings"].ToObject(typeof(List<Setting>)))
                                                                                  .Where(x => x.Name == "CreateVertex")
                                                                                  .First()
                                                                                  .Value;
            list.Add(new KeyValuePair<string, string[]>(query, new string[] { "WoodenGargoyle" }));


            query = ((List<Setting>)jToken["configuration"]["appSettings"].ToObject(typeof(List<Setting>)))
                                                                                  .Where(x => x.Name == "AddVertexProperty")
                                                                                  .First()
                                                                                  .Value;
            list.Add(new KeyValuePair<string, string[]>(query, new string[] { "material","wood"}));
            list.Add(new KeyValuePair<string, string[]>(query, new string[] { "purpose", "art" }));
            // Act
            var fullQuery = gremlinClient.BuildGremlinQuery(list);

            // Assert
            fullQuery.Should().NotBeNull();
            fullQuery.Should().Be("g.AddV('WoodenGargoyle').property('material', 'wood').property('purpose', 'art')");
        }


        private GremlinClient CreateGremlinClient()
        {
            return new GremlinClient(
                "", "");
        }
    }
}
