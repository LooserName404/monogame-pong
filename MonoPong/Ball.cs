using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoPong
{
    public class Ball
    {
        public Rectangle Box
        {
            get;
            private set;
        }

        public Point Velocity
        {
            get;
            private set;
        }

        public Ball(Random rand, bool direction)
        {
            Box = new Rectangle(640 / 2, 480 / 2, 8, 8);
            Velocity = new Point(direction ? rand.Next(3, 7) : -rand.Next(3, 7),
                rand.Next() > int.MaxValue / 2 ? rand.Next(3, 7) : -rand.Next(3, 7));
        }

        public (int, bool) Move(bool bounceOffSides)
        {
            bool bounced = false;

            var pos = Box.Location;

            pos.X += Velocity.X;
            pos.Y += Velocity.Y;

            if (pos.Y < 0)
            {
                bounced = true;
                pos.Y = -pos.Y;
                ReverseVelocity(y: true);
            }

            if (pos.Y + Box.Height > 480)
            {
                bounced = true;
                pos.Y = 480 - (pos.Y + Box.Height - 480);
                ReverseVelocity(y: true);
            }

            int score = 0;

            if (pos.X < 0)
            {
                if (bounceOffSides)
                {
                    bounced = true;
                    pos.X = 0;
                    ReverseVelocity(x: true);
                }
                else
                {
                    score = -1;
                }
            }

            if (pos.X + Box.Width > 640)
            {
                if (bounceOffSides)
                {
                    bounced = true;
                    pos.X = 640 - Box.Width;
                    ReverseVelocity(x: true);
                }
                else
                {
                    score = 1;
                }
            }

            SetPosition(pos);

            return (score, bounced);
        }

        private const int MaxVelocity = 64;
        public void IncreaseVelocity(int? x = null, int? y = null)
        {
            Point vel = Velocity;
            if (x != null) vel.X += (int)x;
            if (y != null) vel.Y += (int)y;

            // cap ball speed
            if (Math.Abs(Velocity.X) > MaxVelocity) vel.X = Math.Sign(vel.X) * MaxVelocity;
            if (Math.Abs(Velocity.Y) > MaxVelocity) vel.Y = Math.Sign(vel.Y) * MaxVelocity;

            Velocity = vel;
        }

        public void SetPosition(Point point)
        {
            Box = new Rectangle(point, Box.Size);
        }

        public void ReverseVelocity(bool x = false, bool y = false)
        {
            var vel = Velocity;

            if (x) vel.X = -vel.X;
            if (y) vel.Y = -vel.Y;

            Velocity = vel;
        }
    }
}
