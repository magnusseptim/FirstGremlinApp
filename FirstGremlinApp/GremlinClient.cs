﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private IList<Vertex> Vertexes;
        public Database ActiveDatabase { get; private set; }
        public DocumentCollection ActiveCollection { get; private set; }
        private DocumentClient client;

        public GremlinClient(string endpoint = "", string authKey = "")
        {
            this.endpoint = endpoint;
            this.authKey = authKey;
            this.client = new DocumentClient(new Uri(endpoint), authKey);
            this.Vertexes = new List<Vertex>();
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

        public async void CreateVertex(DocumentCollection graphCollection, string query)
        {
            IDocumentQuery<Vertex> createdVertexQuery;
            try
            {
                createdVertexQuery = client.CreateGremlinQuery<Vertex>(graphCollection, query);
                while (createdVertexQuery.HasMoreResults)
                {
                    Vertexes.Add((await createdVertexQuery.ExecuteNextAsync<Vertex>()).First());
                }
            }
            catch (Exception)
            {
                throw new Exception("No vertex was retrieved");
            }
        }
    }
}
