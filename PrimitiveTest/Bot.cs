﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrimitiveTest
{
    public class Bot
    {
        //private Vector2 position;

        private Circle circle;
        private Vector2 velocity;
        public Vector2 acceleration;
        private double rotation;

        private bool isAiming;
        private bool isFiring;

        private int teamNumber;

        private int health;

        private Vector2 target;

        private static Random random;

        private StaticMap staticMap;

        //private int radius = 5;

        private Color colour = Color.Green;

        public Bot(int team, Vector2 position, StaticMap map)
        {
            circle = new Circle(position, 5f, Color.White);
            random = new Random();
            teamNumber = team;

            target = new Vector2(random.Next(200, 1000), random.Next(200, 1000));

            staticMap = map;
        }

        private void Stop()
        {
            Vector2 temp = new Vector2(velocity.X, velocity.Y);

            if (temp != Vector2.Zero)
            {
                temp.Normalize();
            }
            else
            {
                temp = Vector2.Zero;
            }

            temp.X *= -1;
            temp.Y *= -1;

            temp.X *= 60;
            temp.Y *= 60;

            acceleration = temp;
        }

        public void Update(float gameTime)
        {
            float distance = circle.Position.Subtract(target).Length();

            if (distance < 50)
            {
                colour = Color.Blue;
                Stop();
            }
            else
            {
                colour = Color.Green;
                float speed = distance / 1.0f;

                Vector2 desiredVelocity = circle.Position.Subtract(target);
                desiredVelocity = desiredVelocity.Scale(speed);

                acceleration = velocity.Subtract(desiredVelocity);
            }

            if (acceleration.Length() > 60)
            {
                acceleration.Scale(60);
            }

            velocity.X += acceleration.X * gameTime;
            velocity.Y += acceleration.Y * gameTime;

            WallAvoid();

            if (velocity.Length() > 100)
            {
                velocity = velocity.Scale(100);
            }

            circle.Position.X += velocity.X * gameTime;
            circle.Position.Y += velocity.Y * gameTime;

            if (IsInsideBlock())
            {
                circle.Position.X -= velocity.X * gameTime;
                circle.Position.Y -= velocity.Y * gameTime;
                velocity = Vector2.Zero;
            }
        }

        private void WallAvoid()
        {
            //Circle circle;

            Vector2 temp = new Vector2(velocity.X, velocity.Y);
            Vector2 newPos;

            if (velocity.Length() > 5.0)
            {
                newPos = new Vector2(circle.Position.X, circle.Position.Y);
                newPos.X += velocity.X * 40;
                newPos.Y += velocity.Y * 40;
            }
            else
            {
                newPos = new Vector2(circle.Position.X, circle.Position.Y);
            }


        }

        public bool IsInsideBlock()
        {
            foreach (Rectangle rect in staticMap.rectangles)
            {
                //get centre of rectangle

                float centerX = rect.Left + rect.Width / 2f;
                float centerY = rect.Top + rect.Height / 2f;

                float dx = Math.Abs(circle.Position.X - centerX);
                float dy = Math.Abs(circle.Position.Y - centerY);

                if (dx > (rect.Width / 2f + circle.Radius)) continue;
                if (dy > (rect.Height / 2f + circle.Radius )) continue;

                if (dx <= (rect.Width / 2f))
                {
                    return true;
                }

                if (dy <= (rect.Height / 2f))
                {
                    return true;
                }

                float cornerDistSq = (dx - rect.Width / 2f) * (dx - rect.Width / 2f) + (dy - rect.Height / 2f) * (dy - rect.Height / 2f);

                if (cornerDistSq <= (circle.Radius * circle.Radius))
                {
                    return true;
                }

            }

            return false;
        }

        public void SetTarget(float x, float y)
        {
            target.X = x;
            target.Y = y;
        }

        public void SetTarget(Vector2 vec)
        {
            target = vec;
        }

        public void Draw(SpriteBatch batch)
        {
            circle.Draw(batch);

            double angle = Math.Atan2(velocity.Y, velocity.X);

            Vector2 lineEnd = new Vector2((float)Math.Cos(angle) * circle.Radius * 2, (float)Math.Sin(angle) * circle.Radius * 2);

            //DebugDraw.DrawLine(batch, centerPos.X, centerPos.Y, centerPos.X + lineEnd.X, centerPos.Y + lineEnd.Y);
            //DebugDraw.DrawLine(batch, circle.Position, circle.Radius * 2, (float) angle, Color.Red);
            DebugDraw.DrawLine(batch, circle.Position, circle.Position + lineEnd);

            Vector2 centerTarget = new Vector2(target.X - circle.Radius * 2, target.Y - circle.Radius * 2);

            DebugDraw.DrawCircle(batch, centerTarget, Color.Red);

            DebugDraw.DrawText(batch, 0, 0, string.Format("Acceleration : {0}", acceleration.Length()));
            DebugDraw.DrawText(batch, 0, 50, string.Format("Distance To Target : {0}", circle.Position.Subtract(target).Length()));

        }
    }
}