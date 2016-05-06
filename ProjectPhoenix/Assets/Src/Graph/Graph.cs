using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public class Graph<T>
    {
        public List<Node<T>> m_hNodes { get; private set; }

        public Graph()
        {
            m_hNodes = new List<Node<T>>();
        }

        public void Add(params Node<T>[] nodes)
        {
            foreach (Node<T> node in nodes)
            {
                m_hNodes.Add(node);
            }
        }

        public void RemoveNode(Node<T> node)
        {
            m_hNodes.Remove(node);
        }

        public override string ToString()
        {
            string s = string.Empty;
            foreach (var n in m_hNodes)
            {
                s += n.ToString() + "\n";
            }
            return s;
        }

        public List<Node<T>> Dijkstra(Node<T> from, Node<T> to)
        {
            List<Node<T>> resNodes = new List<Node<T>>();

            if (from == to)
            {
                resNodes.Add(to);
                return resNodes;
            }

            Dictionary<Node<T>, float> dict = new Dictionary<Node<T>, float>();
            Dictionary<Node<T>, Node<T>> preceedings = new Dictionary<Node<T>, Node<T>>();

            this.m_hNodes.ForEach(hNode => dict.Add(hNode, float.PositiveInfinity));
            dict[from] = 0f;

            while (dict.Count > 0)
            {
                Node<T> node = dict.OrderBy(hN => hN.Value).First().Key;

                if (node == to)
                    break;

                foreach (Node<T>.Neighbour neighbour in node.neighbours)
                {
                    float weight;
                    if (dict.TryGetValue(neighbour.Node, out weight))
                    {
                        float alt = dict[node] + neighbour.Distance;

                        if (alt < weight)
                        {
                            dict[neighbour.Node] = alt;
                            Node<T> v;
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
            Node<T> prec = preceedings[to];

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

        public List<Node<T>> Dijkstra(Node<T> from, Node<T> to, List<Node<T>> customNodeList)
        {
            List<Node<T>> resNodes = new List<Node<T>>();

            if (from == to)
            {
                resNodes.Add(to);
                return resNodes;
            }

            Dictionary<Node<T>, float> dict = new Dictionary<Node<T>, float>();
            Dictionary<Node<T>, Node<T>> preceedings = new Dictionary<Node<T>, Node<T>>();

            customNodeList.ForEach(hNode => dict.Add(hNode, float.PositiveInfinity));
            dict[from] = 0f;

            while (dict.Count > 0)
            {
                Node<T> node = dict.OrderBy(hN => hN.Value).First().Key;

                if (node == to)
                    break;

                foreach (Node<T>.Neighbour neighbour in node.neighbours)
                {
                    float weight;
                    if (dict.TryGetValue(neighbour.Node, out weight))
                    {
                        float alt = dict[node] + neighbour.Distance;

                        if (alt < weight)
                        {
                            dict[neighbour.Node] = alt;
                            Node<T> v;
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
            Node<T> prec = preceedings[to];

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
    }
}