using System;
using System.Drawing;
using OpenTK;

namespace WorldFramework.Controller
{
    public class ArcBall
    {
        private const float Epsilon = 1.0e-5f;
        private float _scale = 0.1f;
        private Vector3 _startVector; //Saved click vector
        private Vector3 _endVector; //Saved drag vector
        private float _adjustWidth; //Mouse bounds width
        private float _adjustHeight; //Mouse bounds height
        public Matrix4 CurrentRotateMatrix = Matrix4.Identity;
        public ArcBall(float width, float height)
        {
            _startVector = new Vector3();
            _endVector = new Vector3();
            SetBounds(width, height);
        }

        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        private void MapToSphere(Point point, ref Vector3 vector)
        {
            var tempPoint = new Vector2(point.X, point.Y);

            //Adjust point coords and scale down to range of [-1 ... 1]
            tempPoint.X = (tempPoint.X * _adjustWidth) - 1.0f;
            tempPoint.Y = (tempPoint.Y * _adjustHeight) - 1.0f;

            //Compute square of the length of the vector from this point to the center
            float length = (tempPoint.X * tempPoint.X) + (tempPoint.Y * tempPoint.Y);

            //If the point is mapped outside the sphere... (length > radius squared)
            if (length > 1.0f)
            {
                //Compute a normalizing factor (radius / sqrt(length))
                var norm = (float)(1.0 / Math.Sqrt(length));

                //Return the "normalized" vector, a point on the sphere
                vector.X = tempPoint.X * norm;
                vector.Y = tempPoint.Y * norm;
                vector.Z = 0.0f;
            }
            //Else it's inside
            else
            {
                //Return a vector to a point mapped inside the sphere sqrt(radius squared - length)
                vector.X = tempPoint.X;
                vector.Y = tempPoint.Y;
                vector.Z = (float)Math.Sqrt(1.0f - length);
            }
        }

        public void SetBounds(float width, float height)
        {
            //Set adjustment factor for width/height
            _adjustWidth =  1.0f / ((width - 1.0f) * 0.5f);
            _adjustHeight =  1.0f / ((height - 1.0f) * 0.5f);
        }

        //Mouse down
        public virtual void MouseDown(Point newPosition)
        {
            MapToSphere(newPosition, ref _startVector);
        }

        public void MouseMoveRotation(Point position)
        {
            CurrentRotateMatrix = Matrix4.Mult(CurrentRotateMatrix, Drag(position));
        }

        //Mouse drag, calculate rotation
        private Matrix4 Drag(Point newPosition)
        {
            //Only allows rotation around x and y, z is not allowed
            //Map the point to the sphere
            MapToSphere(newPosition, ref _endVector);
            //Calculate x-axis rotate angle
            Vector3 xStartVector = new Vector3(0, _startVector.Y, _startVector.Z);
            Vector3 xEndVector = new Vector3(0, _endVector.Y, _endVector.Z);

            Vector3 xPerp;
            Vector3.Cross(ref xStartVector, ref xEndVector, out xPerp);
            Quaternion xRotation = new Quaternion();
            //Compute the length of the perpendicular vector
            if (xPerp.Length > Epsilon)
            //if its non-zero
            {
                //We're ok, so return the perpendicular vector as the transform after all
                xRotation.X = xPerp.X;
                xRotation.Y = xPerp.Y;
                xRotation.Z = xPerp.Z;
                //In the quaternion values, w is cosine (theta / 2), where theta is the rotation angle
                xRotation.W = Vector3.Dot(xStartVector, xEndVector);
            }
            //if it is zero
            else
            {
                //The begin and end vectors coincide, so return an identity transform
                xRotation.X = xRotation.Y = xRotation.Z = 0.0f;
                xRotation.W = 1.0f;
            }
            float xAngle;
            Vector3 xAxis;
            xRotation.ToAxisAngle(out xAxis, out xAngle);
            float xDegree = MathHelper.RadiansToDegrees(xAngle);
            xDegree = xDegree *0.01f;
            xAngle = MathHelper.DegreesToRadians(xDegree);
            var xMatrix = Matrix4.CreateFromAxisAngle(xAxis, xAngle); //Matrix4.Rotate(rotation);

            //float xAngle = (float)Math.Acos(Vector3.Dot(xStartVector, xEndVector));
            //float xdegree = MathHelper.RadiansToDegrees(xAngle);
            //xdegree = xdegree / 10;
            //xAngle = MathHelper.DegreesToRadians(xdegree);
            //Matrix4 xMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, xAngle);
            //Calculate y_axis rotate angle
            Vector3 yStartVector = new Vector3(_startVector.X, 0, _startVector.Z);
            Vector3 yEndVector = new Vector3(_endVector.X, 0, _endVector.Z);

            Vector3 yPerp;
            Vector3.Cross(ref yStartVector, ref yEndVector, out yPerp);
            Quaternion yRotation = new Quaternion();
            //Compute the length of the perpendicular vector
            if (yPerp.Length > Epsilon)
            //if its non-zero
            {
                //We're ok, so return the perpendicular vector as the transform after all
                yRotation.X = yPerp.X;
                yRotation.Y = yPerp.Y;
                yRotation.Z = yPerp.Z;
                //In the quaternion values, w is cosine (theta / 2), where theta is the rotation angle
                yRotation.W = Vector3.Dot(yStartVector, yEndVector);
            }
            //if it is zero
            else
            {
                //The begin and end vectors coincide, so return an identity transform
                yRotation.X = yRotation.Y = yRotation.Z = 0.0f;
                yRotation.W = 1.0f;
            }
            float yAngle;
            Vector3 yAxis;
            yRotation.ToAxisAngle(out yAxis, out yAngle);
            float yDegree = MathHelper.RadiansToDegrees(yAngle);
            yDegree = yDegree *0.01f ;
            yAngle = MathHelper.DegreesToRadians(yDegree);
            var yMatrix = Matrix4.CreateFromAxisAngle(yAxis, yAngle); //Matrix4.Rotate(rotation);


            //float yAngle = (float)Math.Acos(Vector3.Dot(yStartVector, yEndVector));
            //float ydegree = MathHelper.RadiansToDegrees(yAngle);
            //ydegree = ydegree / 10;
            //yAngle = MathHelper.DegreesToRadians(ydegree);
            //Matrix4 yMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitY, yAngle);
            //z_axis is not allowed for rotation
            ////////////////////////////////
            return yMatrix;
            //return rotateMatrix;
            //return Matrix4.Mult(yMatrix, xMatrix);


            ////Return the quaternion equivalent to the rotation

            //Vector3 perp;

            ////Compute the vector perpendicular to the begin and end vectors
            //Vector3.Cross(ref _startVector, ref _endVector, out perp);
            //Quaternion rotation = new Quaternion();
            ////Compute the length of the perpendicular vector
            //if (perp.Length > Epsilon)
            ////if its non-zero
            //{
            //    //We're ok, so return the perpendicular vector as the transform after all
            //    rotation.X = perp.X;
            //    rotation.Y = perp.Y;
            //    rotation.Z = perp.Z;
            //    //In the quaternion values, w is cosine (theta / 2), where theta is the rotation angle
            //    rotation.W = Vector3.Dot(_startVector, _endVector);
            //}
            ////if it is zero
            //else
            //{
            //    //The begin and end vectors coincide, so return an identity transform
            //    rotation.X = rotation.Y = rotation.Z = rotation.W = 0.0f;
            //}
            //return rotation;
        }
    }
}