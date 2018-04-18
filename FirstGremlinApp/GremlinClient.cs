using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using FirstGremlinApp.Tools;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;

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
        private ResourceManager resManager;

        public GremlinClient(string endpoint = "", string authKey = "")
        {
            this.endpoint = endpoint;
            this.authKey = authKey;
            this.client = new DocumentClient(new Uri(endpoint), authKey);
            this.resManager = new ResourceManager("PrefabQueries", Assembly.GetExecutingAssembly());
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

        public async void CreateGraphElement<T>(DocumentCollection graphCollection, string query)
        {
            IDocumentQuery<T> createdVertexQuery;
            try
            {
                createdVertexQuery = client.CreateGremlinQuery<T>(graphCollection, query);
                while (createdVertexQuery.HasMoreResults)
                {

                    this.graphElementsStorages.AddElementToStorage((await createdVertexQuery.ExecuteNextAsync<T>()).First());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Gremlin says that this: {0} was happend", ex.Message));
            }
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

        public async Task<IList<Vertex>> CreateVertex(DocumentCollection graphCollection, string vertexName)
        {
            IList<Vertex> vertexes = new List<Vertex>();
            try
            {
                var response = client.CreateGremlinQuery<Vertex>(graphCollection, string.Format(resManager.GetString("CreateVertex"), vertexName));
                while (response.HasMoreResults)
                {
                     vertexes.Add((await response.ExecuteNextAsync<Vertex>()).First());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Gremlin says that this: {0} was happend", ex.Message));
            }

            return vertexes;
        }
    }
}
