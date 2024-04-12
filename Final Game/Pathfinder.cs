using Final_Game.LevelGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Final_Game
{
    public class Pathfinder
    {
        private Tile[,] layout;

        public Pathfinder(Tile[,] layout)
        {
            this.layout = layout;
        }

        private bool IsWalkable(Point point)
        {
            
            return point.X >= 0 && point.X < layout.GetLength(1) &&
                   point.Y >= 0 && point.Y < layout.GetLength(0) &&
                   (layout[point.Y, point.X].Type != TileType.Wall &&
                    layout[point.Y, point.X].Type != TileType.Spike);
        }

        private List<Point> GetNeighbors(Point point)
        {
            var neighbors = new List<Point>();

            
            var potentialNeighbors = new Point[]
            {
            new Point(point.X + 1, point.Y),
            new Point(point.X - 1, point.Y),
            new Point(point.X, point.Y + 1),
            new Point(point.X, point.Y - 1)
            };

            foreach (var neighbor in potentialNeighbors)
            {
                if (IsWalkable(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        private int GetHeuristic(Point from, Point to)
        {
            
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }

        public List<Point> FindPath(Point start, Point end)
        {
            var openList = new List<Node>();
            var closedList = new HashSet<Point>();
            openList.Add(new Node(start, null, 0, GetHeuristic(start, end)));

            while (openList.Any())
            {
                var currentNode = openList.OrderBy(n => n.TotalCost).First();

                if (currentNode.Position == end)
                {
                    return ReconstructPath(currentNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode.Position);

                foreach (var neighbor in GetNeighbors(currentNode.Position))
                {
                    if (closedList.Contains(neighbor))
                    {
                        continue;
                    }

                    int tentativeGCost = currentNode.Cost + 1;
                    var neighborNode = openList.FirstOrDefault(n => n.Position == neighbor);

                    if (neighborNode == null)
                    {
                        neighborNode = new Node(neighbor, currentNode, tentativeGCost, GetHeuristic(neighbor, end));
                        openList.Add(neighborNode);
                    }
                    else if (tentativeGCost < neighborNode.Cost)
                    {
                        neighborNode.Parent = currentNode;
                        neighborNode.Cost = tentativeGCost;
                    }
                }
            }

            return new List<Point>();
        }

        private List<Point> ReconstructPath(Node currentNode)
        {
            var path = new List<Point>();
            while (currentNode != null)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }
            path.Reverse();
            return path;
        }
    }

    class Node
    {
        public Point Position;
        public Node Parent;
        public int Cost; 
        public int Heuristic; 
        public int TotalCost => Cost + Heuristic; 

        public Node(Point position, Node parent, int cost, int heuristic)
        {
            Position = position;
            Parent = parent;
            Cost = cost;
            Heuristic = heuristic;
        }
    }

}
