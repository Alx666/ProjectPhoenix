using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Graph
{
    [Serializable]
    public class Graph<T> : IEnumerable<T> where T : Graph<T>.Node
    {
        public List<T> Nodes { get; private set; }

        public Graph()
        {
            Nodes = new List<T>();
        }

        public Graph(List<T> hNodes)
        {
            Nodes = hNodes;
        }

        public void Add(params T[] nodes)
        {
            foreach (T node in nodes)
            {
                Nodes.Add(node);
            }
        }

        public void Remove(T hNode)
        {
            try
            {
                Nodes.ForEach(hN => hN.Remove(hNode));
                Nodes.Remove(hNode);
            }
            catch(InvalidOperationException hEx)
            {
                throw new KeyNotFoundException("Node Not Found", hEx);
            }
        }

        public void Remove(int iId)
        {
            try
            {
                T hNode = Nodes.Where(hN => hN.Id == iId).First();
                this.Remove(hNode);
            }
            catch (InvalidOperationException hEx)
            {
                throw new KeyNotFoundException("Node Not Found", hEx);
            }
            
        }

        public void Clear()
        {
            Nodes.Clear();
        }

        #region Dijkstra

        public List<T> Dijkstra(T from, T to)
        {
            List<T> resNodes = new List<T>();

            if (from == to)
            {
                resNodes.Add(to);
                return resNodes;
            }

            Dictionary<Node, float> dict = new Dictionary<Node, float>();
            Dictionary<Node, Node> preceedings = new Dictionary<Node, Node>();

            this.Nodes.ForEach(hNode => dict.Add(hNode, float.PositiveInfinity));
            dict[from] = 0f;

            while (dict.Count > 0)
            {
                T node = dict.OrderBy(hN => hN.Value).First().Key as T;

                if (node == to)
                    break;

                foreach (Node.Neighbour neighbour in node.Neighbours)
                {
                    float weight;
                    if (dict.TryGetValue(neighbour.Node, out weight))
                    {
                        float alt = dict[node] + neighbour.Distance;

                        if (alt < weight)
                        {
                            dict[neighbour.Node] = alt;
                            Node v;
                            if (!preceedings.TryGetValue(neighbour.Node, out v))
                                preceedings.Add(neighbour.Node, node);
                            else
                                preceedings[neighbour.Node] = node;
                        }
                    }
                }

                dict.Remove(node);
            }

            resNodes.Add(to);
            T prec = preceedings[to] as T;

            do
            {
                resNodes.Add(prec);
                if (prec != from)
                    prec = preceedings[prec] as T;
            }
            while (prec != from);

            resNodes.Reverse();
            return resNodes;
        }

        public List<T> Dijkstra(T from, T to, List<T> customNodeList)
        {
            List<T> resNodes = new List<T>();

            if (from == to)
            {
                resNodes.Add(to);
                return resNodes;
            }

            Dictionary<Node, float> dict = new Dictionary<Node, float>();
            Dictionary<Node, Node> preceedings = new Dictionary<Node, Node>();

            customNodeList.ForEach(hNode => dict.Add(hNode, float.PositiveInfinity));
            dict[from] = 0f;

            while (dict.Count > 0)
            {
                T node = dict.OrderBy(hN => hN.Value).First().Key as T;

                if (node == to)
                    break;

                foreach (Node.Neighbour neighbour in node.Neighbours)
                {
                    float weight;
                    if (dict.TryGetValue(neighbour.Node, out weight))
                    {
                        float alt = dict[node] + neighbour.Distance;

                        if (alt < weight)
                        {
                            dict[neighbour.Node] = alt;
                            Node v;
                            if (!preceedings.TryGetValue(neighbour.Node, out v))
                                preceedings.Add(neighbour.Node, node);
                            else
                                preceedings[neighbour.Node] = node;
                        }
                    }
                }

                dict.Remove(node);
            }

            resNodes.Add(to);
            T prec = preceedings[to] as T;

            do
            {
                resNodes.Add(prec);
                if (prec != from)
                    prec = preceedings[prec] as T;
            }
            while (prec != from);

            resNodes.Reverse();
            return resNodes;
        }


        public static byte[] ToBinary<K>(Graph<K> hObj) where K : Graph<K>.Node
        {
            BinaryFormatter hSerializer = new BinaryFormatter();
            SurrogateSelector hSelector = new SurrogateSelector();
            hSelector.AddSurrogate(typeof(Vector3), new StreamingContext(), new Vector3Surrogate());
            hSerializer.SurrogateSelector = hSelector;

            using (MemoryStream hStream = new MemoryStream())
            {
                hSerializer.Serialize(hStream, hObj);
                hStream.Flush();
                return hStream.GetBuffer();
            }
        }

        public static Graph<K> FromBinary<K>(byte[] hBinary) where K : Graph<K>.Node
        {
            BinaryFormatter hSerializer = new BinaryFormatter();
            SurrogateSelector hSelector = new SurrogateSelector();
            hSelector.AddSurrogate(typeof(Vector3), new StreamingContext(), new Vector3Surrogate());
            hSerializer.SurrogateSelector = hSelector;

            using (MemoryStream hStream = new MemoryStream(hBinary))
            {
                return hSerializer.Deserialize(hStream) as Graph<K>;
            }
        }


        #endregion


        public IEnumerator<T> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        public T this[int iIndex]
        {
            get { return Nodes[iIndex]; }
        }


        #region Nested Types

        [Serializable]
        public class Node
        {
            public int Id { get; set; }
            public List<Neighbour> Neighbours;

            public Node()
            {

            }

            public Node(int iId)
            {
                this.Id = iId;
                Neighbours = new List<Neighbour>();
            }

            public void Link(T hNext, float distance)
            {
                Neighbour n = new Neighbour();
                n.Node      = hNext;
                n.Distance  = distance;

                Neighbours.Add(n);
            }

            public void Remove(T hNode)
            {
                Neighbours.Remove(Neighbours.Where(hN => hN.Node == hNode).First());
            }
            
            [Serializable]
            public class Neighbour
            {
                public Node     Node;
                public float    Distance;
            }
        }

        public class Vector3Surrogate : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var vector = (Vector3)obj;
                info.AddValue("x", vector.x);
                info.AddValue("y", vector.y);
                info.AddValue("z", vector.z);
            }
            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                Func<string, float> get = name => (float)info.GetValue(name, typeof(float));
                return new Vector3(get("x"), get("y"), get("z"));
            }
        }

        #endregion


    }
}