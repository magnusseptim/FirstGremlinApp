using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using FirstGremlinApp.Model;
using FirstGremlinApp.Tools;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace FirstGremlinApp
{
    public class GremlinClient
    {
        private string endpoint;
        private string authKey;
        private GraphElementsStorage graphElementsStorages;
        public Database ActiveDatabase { get; private set; }
        public DocumentCollection ActiveCollection { get; private set; }
        private DocumentClient client;
        private IConfigurationRoot configuration;
        private JToken jToken = JToken.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "appsettings.json")));


        public GremlinClient(string endpoint = "", string authKey = "")
        {
            this.endpoint = endpoint;
            this.authKey = authKey;
            this.client = new DocumentClient(new Uri(endpoint), authKey);
            this.configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                           .Build();
        }
        

        public async Task<ResourceResponse<Database>> CreateDatabase(string dbName = "defaultDb", string resourceName = "defaultResource")
        {
            ResourceResponse<Database> db;
            try
            {
                db = await client.CreateDatabaseIfNotExistsAsync(new Database() { Id = dbName, ResourceId = resourceName });
                ActiveDatabase = db;
            }
            catch (Exception)
            {
                db = new ResourceResponse<Database>(); 
            }
            return db;
        }
        public async Task<ResourceResponse<DocumentCollection>> CreateGraphCollection(string dbName = "graphdb", string collectionName = "graphcollz", int defaultOfferThroughput = 1000)
        {
            ResourceResponse<DocumentCollection> collection;
            try
            {
                collection = await client.CreateDocumentCollectionIfNotExistsAsync(
                                            UriFactory.CreateDatabaseUri(dbName),
                                            new DocumentCollection { Id = collectionName },
                                            new RequestOptions { OfferThroughput = defaultOfferThroughput });
            }
            catch (Exception)
            {
                collection = new ResourceResponse<DocumentCollection>();
            }
            return collection;
        }
        public async Task<IList<T>> CreateGraphElement<T>(DocumentCollection graphCollection, string query)
        {
            IList<T> list = new List<T>();
            try
            {
                IDocumentQuery<T> createdVertexQuery = client.CreateGremlinQuery<T>(graphCollection, query);
                while (createdVertexQuery.HasMoreResults)
                {
                    var result = (await createdVertexQuery.ExecuteNextAsync<T>()).First();
                    list.Add(result);
                    this.graphElementsStorages.AddElementToStorage(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Gremlin says that this: {0} was happend", ex.Message));
            }

            return list; 
        }
        public IDocumentQuery<T> RunGremlinQueryManualy<T>(DocumentCollection collection, string query)
        {
            IDocumentQuery<T> queryResult;
            try
            {
                queryResult = client.CreateGremlinQuery<T>(collection, query);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Gremlin says that something wrong had place here: {0}", ex.Message));
            }

            return queryResult;
        }
        public async Task<IList<Vertex>> CreateEmptyVertex(DocumentCollection graphCollection, string vertexName)
        {
            return await RunGremlinQuery<Vertex>(graphCollection, new string[] { vertexName }, "CreateVertex");
        }
        public async Task<IList<Edge>> CreateEdge(DocumentCollection graphCollection, string edgeName, string fromVName, string toVName)
        {
            return await RunGremlinQuery<Edge>(graphCollection, new string[] { fromVName, edgeName, toVName }, "AddEdgeBetween");
        }
        public async Task<IList<Vertex>> CreateVertex(DocumentCollection graphCollection, string vertexName, List<KeyValuePair<string,List<string>>> properties)
        {
            string completeQuery = "";
            foreach (var pair in properties)
            {
                completeQuery = completeQuery + string.Format(pair.Key, pair.Value);
            }

            return await CreateGraphElement<Vertex>(graphCollection, completeQuery);
        }
        public async Task<IList<Vertex>> DropVertex(DocumentCollection graphCollection, string vertexName)
        {
            return await RunGremlinQuery<Vertex>(graphCollection, new string[] { vertexName }, "DropVertex");
        }
        public async Task<IList<Edge>> DropEdge(DocumentCollection graphCollection, string edgeName)
        {
            return await RunGremlinQuery<Edge>(graphCollection, new string[] { edgeName }, "DropEdge");
        }
        public string BuildGremlinQuery(List<KeyValuePair<string, string[]>> commands)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var command in commands)
            {
                sb.AppendJoin("", string.Format(command.Key, command.Value));
            }
            return sb.ToString();
        }
        private async Task<IList<T>> RunGremlinQuery<T>(DocumentCollection graphCollection, string[] queryValues, string queryName)
        {

            IList<T> list = new List<T>();
            try
            {
                var response = client.CreateGremlinQuery<T>(graphCollection, string.Format(GetSpecificQuery(queryName), queryValues));
                while (response.HasMoreResults)
                {
                    list.Add((await response.ExecuteNextAsync<T>()).First());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Gremlin says that this: {0} was happend", ex.Message));
            }

            return list;
        }

        private string GetSpecificQuery(string queryName, string confSecName = "configuration", string appSettName = "appSettings")
        {
            return ((List<Setting>)jToken[confSecName][appSettName].ToObject(typeof(List<Setting>)))
                                                                   .Where(x => x.Name == queryName)
                                                                   .First()
                                                                   .Value;
        }
    }
}
