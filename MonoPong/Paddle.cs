using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoPong
{
    public class Paddle
    {
        public Rectangle Box
        {
            get;
            private set;
        }

        private readonly bool _side;

        public Paddle(bool side)
        {
            _side = side;
            var x = side ? 600 : 32;
            Box = new Rectangle(new Point(x, 224), new Point(8, 32));
        }

        public bool CollisionCheck(Ball ball)
        {
            if (!BallIsAbleToBeHit(ball)) return false;

            (float delta, bool wayPastPaddle) = FindDeltaInBallMovement(ball);

            if (wayPastPaddle) return false;

            float deltaTime = delta / ball.Velocity.X;
            int collY = (int) (ball.Box.Y - ball.Velocity.Y * deltaTime);
            int collX = (int) (ball.Box.X - ball.Velocity.X * deltaTime);

            if (PaddleCheck(collX, collY))
            {
                // make the ball linger on collision, makes it look more like it really hit.
                ball.SetPosition(new Point(collX, collY));

                var diffY = (collY + ball.Box.Height / 2) - (Box.Y + Box.Height / 2);
                diffY /= Box.Height / 8;
                diffY -= Math.Sign(diffY);

                ball.IncreaseVelocity(Math.Sign(ball.Velocity.X), diffY);
                ball.ReverseVelocity(true);

                return true;
            }

            return false;
        }
        private bool PaddleCheck(int x, int y)
        {
            return x <= Box.X + Box.Width &&
                   x + 8 >= Box.X &&
                   y <= Box.Y + Box.Height &&
                   y + 8 >= Box.Y;
        }

        public bool BallIsAbleToBeHit(Ball ball)
        {
            bool directionCheck;
            bool distanceCheck;
            if (_side)
            {
                directionCheck = ball.Velocity.X > 0;
                distanceCheck = ball.Box.X + ball.Box.Width > Box.X;
                return directionCheck & distanceCheck;
            }

            directionCheck = ball.Velocity.X < 0;
            distanceCheck = ball.Box.X < Box.X + Box.Width;
            return directionCheck & distanceCheck;
        }

        public (float, bool) FindDeltaInBallMovement(Ball ball)
        {
            float delta;
            bool wayPastPaddle;
            if (_side)
            {
                delta = ball.Box.X + ball.Box.Width - Box.X;
                wayPastPaddle = delta > ball.Velocity.X + ball.Box.Width;
                return (delta, wayPastPaddle);
            }

            delta = ball.Box.X - (Box.X + Box.Width);
            wayPastPaddle = delta < ball.Velocity.X;
            return (delta, wayPastPaddle);
        }

        private void FixBounds(Point pos)
        {
            if (pos.Y < Box.Height) pos.Y = Box.Height;
            if (pos.Y + Box.Height > 480) pos.Y = 480 - Box.Height;

            Box = new Rectangle(pos, Box.Size);
        }

        public static int AiPaddleSpeed = 4;
        public void AIMove(Ball ball)
        {
            var delta = ball.Box.Y + ball.Box.Height / 2 - (Box.Y + Box.Height / 2);
            var pos = Box.Location;

            if (Math.Abs(delta) > AiPaddleSpeed) delta = Math.Sign(delta) * AiPaddleSpeed;
            pos.Y += delta;

            FixBounds(pos);
        }

        public void PlayerMove(int diff)
        {
            var pos = Box.Location;
            pos.Y += diff;
            FixBounds(pos);
        }
    }
}
