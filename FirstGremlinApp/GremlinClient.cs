using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;

namespace FirstGremlinApp
{
    public class GremlinClient
    {
        private string endpoint;
        private string authKey;
        public Database ActiveDatabase { get; set; }
        private DocumentClient client;

        public GremlinClient(string endpoint = "", string authKey = "")
        {
            this.endpoint = endpoint;
            this.authKey = authKey;
            this.client = new DocumentClient(new Uri(endpoint), authKey);
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
    }
}
