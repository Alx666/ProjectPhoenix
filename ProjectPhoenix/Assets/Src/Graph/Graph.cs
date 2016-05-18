using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public class Graph<T> : IEnumerable<T> where T : Graph<T>.Node
    {
        private List<T> m_hNodes;

        public Graph()
        {
            m_hNodes = new List<T>();
        }

        public void Add(params T[] nodes)
        {
            foreach (T node in nodes)
            {
                m_hNodes.Add(node);
            }
        }

        public void Remove(T hNode)
        {
            try
            {
                m_hNodes.ForEach(hN => hN.Remove(hNode));
                m_hNodes.Remove(hNode);
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
                T hNode = m_hNodes.Where(hN => hN.Id == iId).First();
                this.Remove(hNode);
            }
            catch (InvalidOperationException hEx)
            {
                throw new KeyNotFoundException("Node Not Found", hEx);
            }
            
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

            Dictionary<T, float> dict = new Dictionary<T, float>();
            Dictionary<T, T> preceedings = new Dictionary<T, T>();

            this.m_hNodes.ForEach(hNode => dict.Add(hNode, float.PositiveInfinity));
            dict[from] = 0f;

            while (dict.Count > 0)
            {
                T node = dict.OrderBy(hN => hN.Value).First().Key;

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
                            T v;
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
            T prec = preceedings[to];

            do
            {
                resNodes.Add(prec);
                if (prec != from)
                    prec = preceedings[prec];
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

            Dictionary<T, float> dict = new Dictionary<T, float>();
            Dictionary<T, T> preceedings = new Dictionary<T, T>();

            customNodeList.ForEach(hNode => dict.Add(hNode, float.PositiveInfinity));
            dict[from] = 0f;

            while (dict.Count > 0)
            {
                T node = dict.OrderBy(hN => hN.Value).First().Key;

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
                            T v;
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
            T prec = preceedings[to];

            do
            {
                resNodes.Add(prec);
                if (prec != from)
                    prec = preceedings[prec];
            }
            while (prec != from);

            resNodes.Reverse();
            return resNodes;
        }


        #endregion


        public IEnumerator<T> GetEnumerator()
        {
            return m_hNodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_hNodes.GetEnumerator();
        }

        public T this[int iIndex]
        {
            get { return m_hNodes[iIndex]; }
        }



        #region Nested Types

        public class Node
        {
            public int Id { get; private set; }
            public List<Neighbour> Neighbours;

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

            public class Neighbour
            {
                public T        Node;
                public float    Distance;
            }
        }

        #endregion


    }
}