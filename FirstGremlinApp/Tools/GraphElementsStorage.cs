using Microsoft.Azure.Graphs.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirstGremlinApp.Tools
{
    public class GraphElementsStorage
    {
        private static string NoErrorsMess = "No Errors in queue";
        private static string NoIndexErrorMessage = "Any error with specific index was recorded";

        private IList<Vertex> Vertexes;
        private IList<Edge> Edges;

        private Queue<string> ErrorQueue;
        public GraphElementsStorage()
        {
            Vertexes = new List<Vertex>();
            Edges = new List<Edge>();
            ErrorQueue = new Queue<string>();
        }

        public IList<Vertex> GetVertexesList
        {
            get { return Vertexes; }
        }

        public IList<Edge> GetEdgesList
        {
            get { return Edges; }
        }

        public string GetLastError()
        {
            return ErrorQueue.Last();
        }

        public string GetErrorByIndex(int index)
        {
            return ErrorQueue.ElementAt(index);
        }

     
        public bool AddElementToStorage<T>(T element)
        {
            bool response = true;
            try
            {
                GetListOfDesiredType(element).Add(element);
            }
            catch (Exception ex)
            {
                ErrorQueue.Enqueue(ex.Message);
                response = false;
            }

            return response;
        }

        private IList<T> GetListOfDesiredType<T>(T type)
        {
            var properties = this.GetType().GetProperties();
            IList<T> value = new List<T>();
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(T))
                {
                    value = (IList<T>)prop.GetValue(this);
                }
            }
            return value;
        }
    }
}
