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
        private int tileSize; 

        public Pathfinder(Tile[,] layout, int tileSize)
        {
            this.layout = layout;
            this.tileSize = tileSize;
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
                if (neighbor.X >= 0 && neighbor.X < layout.GetLength(1) &&
             neighbor.Y >= 0 && neighbor.Y < layout.GetLength(0) &&
             IsWalkable(neighbor))
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


        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            
            Point startGrid = WorldToGrid(start);
            Point endGrid = WorldToGrid(end);

            var openList = new List<Node>();
            var closedList = new HashSet<Point>();

            openList.Add(new Node(startGrid, null, 0, GetHeuristic(startGrid, endGrid)));

            while (openList.Any())
            {
                var currentNode = openList.OrderBy(n => n.TotalCost).First();

                if (currentNode.Position == endGrid)
                {
                    List<Vector2> finalpath = ReconstructPathWorld(currentNode, start);

                    return finalpath; 
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
                        neighborNode = new Node(neighbor, currentNode, tentativeGCost, GetHeuristic(neighbor, endGrid));
                        openList.Add(neighborNode);
                    }
                    else if (tentativeGCost < neighborNode.Cost)
                    {
                        neighborNode.Parent = currentNode;
                        neighborNode.Cost = tentativeGCost;
                    }
                }
            }

            return new List<Vector2>(); 
        }

       


        private List<Vector2> ReconstructPathWorld(Node currentNode, Vector2 currentWorldPosition)
        {
            var path = new List<Vector2>();
            while (currentNode != null)
            {
                Vector2 worldPosition = GridToWorld(new Point(currentNode.Position.X, currentNode.Position.Y));
                path.Add(worldPosition);
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            float maxStepDistance = 80.0f;
            List<Vector2> smoothPath = SmoothPath(path, maxStepDistance);

            if (smoothPath.Count > 1)
            {
                
                if (currentWorldPosition.X < smoothPath[smoothPath.Count - 1].X)
                 {
                     for (int i = 0; i < smoothPath.Count; i++)
                     {
                         if (smoothPath[i].X < currentWorldPosition.X)
                         {
                             smoothPath[i] = new Vector2(currentWorldPosition.X, smoothPath[i].Y);
                         }
                     }
                 }

                   if (currentWorldPosition.X > smoothPath[smoothPath.Count - 1].X)
                   {
                       for (int i = 0; i < smoothPath.Count; i++)
                       {
                           if (smoothPath[i].X > currentWorldPosition.X)
                           {
                               smoothPath[i] = new Vector2(currentWorldPosition.X, smoothPath[i].Y);
                           }
                       }
                   }
                 

              

                     if (currentWorldPosition.Y > smoothPath[smoothPath.Count - 1].Y)
                     {
                         for (int i = 0; i < smoothPath.Count; i++)
                         {
                             if (smoothPath[i].Y > currentWorldPosition.Y)
                             {
                                 smoothPath[i] = new Vector2(smoothPath[i].X, currentWorldPosition.Y);
                             }
                         }
                     }
                  
             
                    for (int i = 0; i < smoothPath.Count; i++)
                    {
                        if (smoothPath[i].Y < 200)
                        {
                            smoothPath[i] = new Vector2(smoothPath[i].X, 200);
                        }
                    }
                

                for (int i = 0;i < smoothPath.Count-1; i++)
                {
                    if (smoothPath[i].Equals(smoothPath[i+1]))
                    {
                        smoothPath.RemoveAt(i + 1);
                    }
                }

            }

            return smoothPath;
        }


        public List<Vector2> SmoothPath(List<Vector2> roughPath, float maxStepDistance)
        {
            if (roughPath == null || !roughPath.Any())
                return new List<Vector2>();

            List<Vector2> smoothPath = new List<Vector2>();
            smoothPath.Add(roughPath[0]);

            for (int i = 0; i < roughPath.Count - 1; i++)
            {
                Vector2 segmentStart = roughPath[i];
                Vector2 segmentEnd = roughPath[i + 1];


                Vector2 direction = segmentEnd - segmentStart;
                float segmentLength = direction.Length();
                direction.Normalize();


                while (segmentLength > maxStepDistance)
                {
                    segmentStart += direction * maxStepDistance;
                    smoothPath.Add(segmentStart);
                    segmentLength -= maxStepDistance;
                }


                if (segmentLength > 0)
                    smoothPath.Add(segmentEnd);
            }

            if (smoothPath.Last() != roughPath.Last())
                smoothPath.Add(roughPath.Last());

            return smoothPath;
        }


        private Point WorldToGrid(Vector2 worldPosition)
        {
            return new Point((int)worldPosition.X / tileSize, (int)worldPosition.Y / tileSize);
        }

        private Vector2 GridToWorld(Point gridPosition)
        {
            return new Vector2(gridPosition.X * tileSize, gridPosition.Y * tileSize);
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
