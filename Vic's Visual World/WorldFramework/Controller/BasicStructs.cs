using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;

namespace WorldFramework.Controller
{
    class BasicStructs
    {
    }

    public struct VertexC4ubV3f
    {
        //Save order as C4ubV3f (first color4ub, then Vector3)
        public uint Color;
        public Vector3 Position;


        public VertexC4ubV3f(float x, float y, float z, Color color)
        {
            Position = new Vector3(x, y, z);
            Color = ToRgba(color);
        }

        static uint ToRgba(Color color)
        {
            return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
        }
    }

    public struct VertexT2C4ubV3f
    {
        //Save order as C4ubV3f (first color4ub, then Vector3)
        public Vector2 Texture;
        public uint Color;
        public Vector3 Position;


        public VertexT2C4ubV3f(float u,float v, float x, float y, float z, Color color)
        {
            Texture = new Vector2(u, v);
            Position = new Vector3(x, y, z);
            Color = ToRgba(color);
        }

        static uint ToRgba(Color color)
        {
            return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
        }
    }

    public struct VertexT2fC3fV3f
    {
       
        public Vector2 Texture;
        public uint Color;
        public Vector3 Position;

        public VertexT2fC3fV3f(float u, float v,float x, float y, float z,Color color)
        {
            Texture=new Vector2(u,v);
            Position = new Vector3(x, y, z);
            Color = ToRgba(color);
        }

        static uint ToRgba(Color color)
        {
            return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
        }

    }

    public struct Orientation
    {
        public float longitude, latitude;
       
        public override string ToString()
        {   
            StringBuilder stringBuilder=new StringBuilder();
            //longitude >180 is W <=180 is E
            stringBuilder.Append(
                longitude <= 180
                    ? string.Format("({0:f4}°E", longitude)
                    : string.Format("({0:f4}°W", 360 - longitude));
            //latitude >=0 is N <0 is S
            stringBuilder.Append(latitude >= 0 ? string.Format(" : {0:f4}°N) ", latitude) : string.Format(" : {0:f4}°S) ", -latitude));
            return stringBuilder.ToString();
        }
        public Orientation(float longi,float lati)
        {
            longitude = longi;
            latitude = lati;
        }
    }

    //public class Location
    //{
    //    public int Degree, Minute, Second;
    //    public Location(int d, int m, int s)
    //    {
    //        Degree = d;
    //        Minute = m;
    //        Second = s;
    //    }
    //    //Normal:3° 27′ 30" E; Decimal:23.45833°
    //    public string ConvertToDecimal(string normalStr)
    //    {
    //        return 
    //    }
    //    public string ConvertToNormal(float decimalStr)
    //    {
    //        Degree
    //    }
    //}
    public struct Particle
    {
        public float X, Y, Z, Radias;
        public Color Color;
        public Particle(float x, float y, float z, float radias, Color color)
        {
            X = x;
            Y = y;
            Z = z;
            Radias = radias;
            Color = color;
        }

    }

    public struct VertexT2dN3dV3d
    {
        public Vector2d TexCoord;
        public Vector3d Normal;
        public Vector3d Position;

        public VertexT2dN3dV3d(Vector2d texcoord, Vector3d normal, Vector3d position)
        {
            TexCoord = texcoord;
            Normal = normal;
            Position = position;
        }
    }

    public struct VertexT2fN3fV3f
    {
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector3 Position;
    }

    public struct VertexT2hN3hV3h
    {
        public Vector2h TexCoord;
        public Vector3h Normal;
        public Vector3h Position;
    }

    public struct VertexN3fV3f
    {
        public Vector3 Normal;
        public Vector3 Position;
        public VertexN3fV3f(Vector3 normal, Vector3 position)
        {
            Normal = normal;
            Position = position;
        }
        public VertexN3fV3f(Vector3 normal, float x,float y,float z)
        {
            Normal = normal;
            Position = new Vector3(x, y, z);
        }
    }
}
