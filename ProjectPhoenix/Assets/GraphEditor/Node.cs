using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Graph
{
    public class Node<T>
    {
        public T value;
        
        public int m_iId;
        
        public List<Neighbour> neighbours;

        public Node()
        {

        }

        public Node(T value, int id)
        {
            this.value = value;
            this.m_iId = id;
            neighbours = new List<Neighbour>();
        }

        public void Link(Node<T> next, float distance)
        {
            Neighbour n = new Neighbour();
            n.Node = next;
            n.Distance = distance;

            neighbours.Add(n);
        }
        
        public class Neighbour
        {
            public Node<T> Node;
            
            public float Distance;

            public Neighbour()
            {

            }
        }

        //public override string ToString()
        //{
        //    string s = string.Empty;
        //    s += this.name + " => ";
        //    neighbours.ForEach(n => s += n.Node.name + " " + n.Distance + ", ");
        //    return s;
        //}
    }

}
