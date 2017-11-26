﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrimitiveTest
{
    public class StaticMap
    {
        public List<Rectangle> rectangles;

        private Texture2D texture;

        public List<Node> NodeMap;

        public List<Line> AllLines;

        public StaticMap()
        {
            rectangles = new List<Rectangle>();

            texture = new Texture2D(Game.graphics.GraphicsDevice, 100, 100);

            Color[] data = new Color[100 * 100];

            for (int i = 0; i < 100 * 100; ++i)
            {
                data[i] = Color.White;
            }

            texture.SetData(data);
        }

        public void AddRectangle(Rectangle rect)
        {
            rectangles.Add(rect);
        }

        public void AddRectangle(int x, int y, int width, int height)
        {
            Rectangle rect = new Rectangle(x, y, width, height);
            
            AddRectangle(rect);
        }

        public void DrawMap(SpriteBatch batch)
        {
            foreach (Rectangle rect in rectangles)
            {
                DrawRect(batch, rect);
            }
        }

        private void DrawRect(SpriteBatch batch, Rectangle rect)
        {
            batch.Draw(texture, rect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
        }

        public void GetNodes()
        {
            NodeMap = new List<Node>();
            Rectangle rect = new Rectangle(-500, -500, 2000, 2000);

            GeneratePath(rect);

            CleanPath(); //removes any outside field of view

            GenerateLinesOfSight();
        }

        private void GenerateLinesOfSight()
        {
            //loop through all nodes
            //try make a line between all other nodes
            //if intersects with static map, don't add

            AllLines = new List<Line>();

            for (int i = 0; i < NodeMap.Count - 1; ++i)
            {
                for (int j = 1; j < NodeMap.Count; ++j)
                {
                    var n1 = NodeMap[i];
                    var n2 = NodeMap[j];
                    Line newLine = new Line(n1.Position, n2.Position);

                    if (newLine.Distance() < 400 && newLine.Distance() > 150)
                    {
                        if (IsLineOfSight(newLine))
                        {
                            AllLines.Add(newLine);

                            n1.Neighbours.Add(n2);
                        }
                    }
                }
            }
        }

        private float GetGScore(List<Node> path)
        {
            float total = 0;
            for (int i = 0; i < path.Count - 1; ++i)
            {
                total += Vector2.Distance(path[i].Position, path[i + 1].Position);
            }

            return total;
        }

        public List<Node> GetShortestPath(Vector2 start, Vector2 end)
        {
            //find closest node to start

            //run a* algorithm to end node

            //add end vector to list

            //return list

            Node startNode = GetClosestNodeToPoint(start);
            Node endNode = GetClosestNodeToPoint(end);

            List<Node> openPath = new List<Node>();
            List<Node> closedPath = new List<Node>();

            openPath.Add(startNode);

            startNode.gScore = 0;
            startNode.fScore = startNode.gScore + Vector2.Distance(startNode.Position, endNode.Position);
            
            do
            {
                //F - G + H
                //G - movement cost from start to current
                //H - estimated cost from current square to end

                    //OPEN = priority queue containing START
                    //CLOSED = empty set
                    //while lowest rank in OPEN is not the GOAL:
                    //                current = remove lowest rank item from OPEN
                    //                add current to CLOSED
                    //  for neighbors of current:
                    //    cost = g(current) + movementcost(current, neighbor)
                    //    if neighbor in OPEN and cost less than g(neighbor):
                    //      remove neighbor from OPEN, because new path is better
                    //    if neighbor in CLOSED and cost less than g(neighbor): ⁽²⁾
                    //      remove neighbor from CLOSED
                    //    if neighbor not in OPEN and neighbor not in CLOSED:
                    //                set g(neighbor) to cost
                            //      add neighbor to OPEN
                            //      set priority queue rank to g(neighbor) +h(neighbor)
                            //      set neighbor's parent to current

                    //reconstruct reverse path from goal to start
//by following parent pointers

                var currentNode = GetLowestFScore(openPath);

                closedPath.Add(currentNode);
                openPath.Remove(currentNode);

                if (closedPath.Contains(endNode)) break; //got to the end

                foreach (var neighbour in currentNode.Neighbours)
                {
                    //neighbour.Parent = currentNode;
                    //if (closedPath.Contains(neighbour)) continue;

                    float cost = currentNode.gScore + Vector2.Distance(currentNode.Position, neighbour.Position);

                    if (openPath.Contains(neighbour) && cost < neighbour.gScore)
                    {
                        openPath.Remove(neighbour);
                    }
                    if (closedPath.Contains(neighbour) && cost < neighbour.gScore)
                    {
                        closedPath.Remove(neighbour);
                    }
                    if (!closedPath.Contains(neighbour) && !openPath.Contains(neighbour))
                    {
                        neighbour.gScore = cost;
                        openPath.Add(neighbour);
                        neighbour.Parent = currentNode;

                        neighbour.fScore = neighbour.gScore + Vector2.Distance(neighbour.Position, endNode.Position);
                    }

                    

                }
                
                    //if (closedList.contains(neighbour) continue;

                    //if (!openList.Contains(neighbour)
                        //work out score
                        //add to list
                    //else
                        //test if current G score will make F score better, if yes update parent becase its better path

            } while (openPath.Any());


            List<Node> shortestPath = new List<Node>();

            shortestPath.Add(endNode);

            Node n = endNode;

            do
            {
                shortestPath.Add(n.Parent);
                n = n.Parent;
            } while (n.Parent != null);


            return shortestPath;

            //foreach node neighbour
            //find one closest to end goal
            //add to path

            //path.Add(startNode.Position);

            //var minDistance = 10000f;
            //Node nearestNode = null;


            //foreach (var neighbour in startNode.Neighbours)
            //{
            //    var thisDistance = Vector2.Distance(neighbour.Position, endNode.Position);

            //    if (thisDistance < minDistance)
            //    {
            //        minDistance = thisDistance;
            //        nearestNode = neighbour;
            //    }

            //    path.Add(nearestNode.Position);
            //}

            //return path;
        }

        private Node GetLowestFScore(List<Node> nodes)
        {
            float lowestScore = 10000000;
            Node n = null;

            foreach (Node node in nodes)
            {
                if (node.fScore < lowestScore)
                {
                    lowestScore = node.fScore;
                    n = node;
                }
            }

            return n;
        }

        private Node GetClosestNodeToPoint(Vector2 p)
        {
            float minDistance = 10000;
            Node shortestNode = null;

            foreach (Node n in NodeMap)
            {
                var distance = Vector2.Distance(p, n.Position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    shortestNode = n;
                }
            }

            return shortestNode;
        }

        private bool IsLineOfSight(Line line)
        {
            foreach (Rectangle rect in rectangles)
            {
                Line l1, l2, l3, l4;

                l1 = new Line(new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Top));
                l2 = new Line(new Vector2(rect.Right, rect.Top), new Vector2(rect.Right, rect.Bottom));
                l3 = new Line(new Vector2(rect.Right, rect.Bottom), new Vector2(rect.Left, rect.Bottom));
                l4 = new Line(new Vector2(rect.Left, rect.Bottom), new Vector2(rect.Left, rect.Top));

                if (Line.Intersects(line, l1).HasValue)
                {
                    return false;
                }

                if (Line.Intersects(line, l2).HasValue)
                {
                    return false;
                }

                if (Line.Intersects(line, l3).HasValue)
                {
                    return false;
                }

                if (Line.Intersects(line, l4).HasValue)
                {
                    return false;
                }
            }

            return true;
        }

        private void CleanPath()
        {
            var listToRemove = new List<Node>();
            for (int i = 0; i < NodeMap.Count; ++i)
            {
                var vec = NodeMap[i].Position;
                if (vec.X < 0 || vec.X > 1200 || vec.Y < 0 || vec.Y > 1200)
                {
                    listToRemove.Add(NodeMap[i]);
                }
            }

            for (int i = 0; i < listToRemove.Count; ++i)
            {
                NodeMap.Remove(listToRemove[i]);
            }
        }

        private void GeneratePath(Rectangle rect)
        {
            double width = rect.Width;
            double height = rect.Height;

            if (width * height > 500)
            {
                if (IsInsideBlock(rect))
                {
                    Rectangle[] rects = new Rectangle[4];

                    rects[0] = new Rectangle(rect.Left, rect.Top, rect.Width / 2, rect.Height / 2);
                    rects[1] = new Rectangle(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 2);
                    rects[2] = new Rectangle(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
                    rects[3] = new Rectangle(rect.Left, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);

                    for (int i = 0; i < 4; ++i)
                    {
                        GeneratePath(rects[i]);
                    }
                }
                else
                {
                    Node newNode = new Node(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
                    NodeMap.Add(newNode);
                }
            }
        }

        private bool IsInsideBlock(Rectangle rect)
        {
            foreach (Rectangle r in rectangles)
            {
                if (r.Intersects(rect))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
