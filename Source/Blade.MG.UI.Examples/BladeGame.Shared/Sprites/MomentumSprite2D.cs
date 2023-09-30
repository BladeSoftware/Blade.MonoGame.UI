using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;

namespace BladeGame.Sprites
{
    public abstract class MomentumSprite2D : Sprite2D
    {

        ///// <summary>
        ///// Transformations affect inertia of sprite
        ///// </summary>
        protected Matrix inertiaMatrix = Matrix.Identity;
        protected Matrix InertiaMatrix { get { return inertiaMatrix; } set { inertiaMatrix = value; } }


        /// <summary>
        /// Velocity of sprite in direction defined by inertiaMatrix
        /// </summary>
        public float Velocity = 0f;

        /// <summary>
        /// Maximum velocity. Set to double.NaN to not limit velocity
        /// </summary>
        public float MaxVelocity = 1000f;

        /// <summary>
        /// ctor
        /// </summary>
        public MomentumSprite2D(GameBase game) : base(game)
        {

        }

        /////// <summary>
        /////// Rotates the sprite but does not change the direction of movement
        /////// </summary>
        /////// <param name="angleDeg"></param>
        //public override void Rotate(double degrees)
        //{
        //    imageMatrix.MulRotate(degrees);
        //    //inertiaMatrix.MulRotate(degrees);
        //    spriteMatrix.MulRotate(degrees);
        //}

        //public void RotateIgnoreInertia(float degrees)
        //{
        //    base.Rotate(degrees);
        //    //inertiaMatrix.MulRotate(degrees);
        //    Matrix3x2.Multiply(inertiaMatrix, Matrix3x2.CreateRotation((float)MathHelper.ToRad(degrees)));
        //}

        public override void MoveForward(float dist)
        {
            Vector3 forward = Vector3.Transform(axisY, inertiaMatrix);

            //if (forward.X is float.NaN)
            //{
            //    Debug.WriteLine("Forward (" + forward.X + ", " + forward.Y + ") : Dist = " + dist.ToString() + " : pointUp (" + pointUp.X + ", " + pointUp.Y + ")");
            //    float[] val = InertiaMatrix.Values;
            //    Debug.WriteLine("InertiaMatrix : Scale (" + val[0] + ", " + val[4] + ") : Skew (" + val[1] + ", " + val[3] + ") : Trans (" + val[2] + ", " + val[5] + ") : Perspective : (" + val[6] + ", " + val[7] + ", " + val[8] + ")");
            //    return;
            //}

            Translate(forward.X * dist, forward.Y * dist);
        }

        //public void MoveForwardIgnoreInertia(float dist)
        //{
        //  // Call base.MoveForward ???
        //    Vector2 up = Vector2.Transform(pointUp, inertiaMatrix);
        //    Translate(up.X * dist, up.Y * dist);
        //}

        /// <summary>
        /// Thrust in the specified direction, producing an acceleration in the opposite direction.
        /// </summary>
        /// <param name="thrustDirection"></param>
        /// <param name="acceleration"></param>
        public void Thrust(Vector3 thrustDirection, float acceleration)
        {
            Vector3 momentumUp = Vector3.Transform(axisY, inertiaMatrix);

            // Calculate the forward momentum direction
            Vector3 forward = new Vector3(thrustDirection.X * acceleration + momentumUp.X * Velocity, thrustDirection.Y * acceleration + momentumUp.Y * Velocity, thrustDirection.Z * acceleration + momentumUp.Z * Velocity);

            // Get the Magnitude / Length of the Forward Vector
            float forwardMagnitude = forward.LengthSquared(); //forward.X * forward.X + forward.Y * forward.Y;

            // Normalize the Forward Vector
            if (forwardMagnitude != 0f)
            {
                forwardMagnitude = (float)Math.Sqrt(forwardMagnitude);

                forward.X /= forwardMagnitude;
                forward.Y /= forwardMagnitude;
                forward.Z /= forwardMagnitude;
            }

            Velocity = forwardMagnitude;

            // Should we limit the maximum velocity
            if (!float.IsNaN(MaxVelocity))
            {
                Velocity = Math.Min(Velocity, MaxVelocity);
            }

            //TODO: Calculate a new (Right) vector perpendicular to the Forward Vector
            //Vector3 right = SKMathHelper.CalcPerpendicular(forward);
            Vector3 right = Vector3.Cross(forward, axisZ);


            //float momentumDotForward = momentumUp.X * forward.X + momentumUp.Y * forward.Y; // Dot product of normalized vectors
            //float momentumDotRight = momentumUp.X * right.X + momentumUp.Y * right.Y; // Dot product of normalized vectors
            float momentumDotForward = Vector3.Dot(momentumUp, forward);
            float momentumDotRight = Vector3.Dot(momentumUp, right);

            // Clamp momentumDotForward to the range [-1 ... +1]
            if (momentumDotForward < -1f) momentumDotForward = -1f;
            if (momentumDotForward > 1f) momentumDotForward = 1f;

            // Calculate the Angle between the Momentum Up Vector and Forward Vector 
            float angleRad = (float)Math.Acos(momentumDotForward);
            //float angleDeg = MathHelper.ToDegrees(angleRad); //float angleDeg = angleRad / (SKMathHelper.PI / 180f);

            if (momentumDotRight < 0f)
            {
                //angleDeg = 360f - angleDeg;
                angleRad = MathHelper.TwoPi - angleRad;
            }


            //float[] valB = InertiaMatrix.Values;

            inertiaMatrix = Matrix.Multiply(inertiaMatrix, Matrix.CreateRotationZ(angleRad));

            //float[] valA = InertiaMatrix.Values;

            //if (float.IsNaN(valB[0]) || float.IsNaN(valA[0]))
            //{
            //    Debug.WriteLine("B:InertiaMatrix : Scale (" + valB[0] + ", " + valB[4] + ") : Skew (" + valB[1] + ", " + valB[3] + ") : Trans (" + valB[2] + ", " + valB[5] + ") : Perspective : (" + valB[6] + ", " + valB[7] + ", " + valB[8] + ")");

            //    Debug.WriteLine("inertiaMatrix.Values = new float[] {" + valB[0] + ", " + valB[1] + ", " + valB[2] + ", " + valB[3] + ", " + valB[4] + ", " + valB[5] + ", " + valB[6] + ", " + valB[7] + ", " + valB[8] + ");");
            //    Debug.WriteLine("thrustDirection = new SKPoint(" + thrustDirection.X + ", " + thrustDirection.Y + ");");
            //    Debug.WriteLine("acceleration = " + acceleration.ToString() + "; ");
            //    Debug.WriteLine("Velocity = " + Velocity.ToString() + "; ");


            //    Debug.WriteLine("AngleRad = " + angleRad.ToString() + " : AngleDeg = " + angleDeg.ToString() + " : Mf = " + momentumDotForward.ToString() + " : " + " Mr = " + momentumDotRight + " : ");

            //    Debug.WriteLine("A:InertiaMatrix : Scale (" + valA[0] + ", " + valA[4] + ") : Skew (" + valA[1] + ", " + valA[3] + ") : Trans (" + valA[2] + ", " + valA[5] + ") : Perspective : (" + valA[6] + ", " + valA[7] + ", " + valA[8] + ")");
            //    Debug.WriteLine("inertiaMatrix.Values = new float[] {" + valA[0] + ", " + valA[1] + ", " + valA[2] + ", " + valA[3] + ", " + valA[4] + ", " + valA[5] + ", " + valA[6] + ", " + valA[7] + ", " + valA[8] + ")");
            //}

        }

        /// <summary>
        /// Thrust Backwards, producing an acceleration in the direction the sprite is facing.
        /// </summary>
        public void ThrustBackwards(float acceleration)
        {
            Vector3 thrustDirection = Vector3.Transform(axisY, spriteMatrix);
            Thrust(thrustDirection, acceleration);
        }

        /// <summary>
        /// Thrust Right, producing an acceleration to the left
        /// </summary>
        public void ThrustRight(float acceleration)
        {
            Vector3 thrustDirection = Vector3.Transform(axisX, spriteMatrix);
            Thrust(thrustDirection, acceleration);
        }

    }
}
