using System;

namespace AuthServer
{
    public class Vector3
    {
        public float x {get;set;}
        public float y {get; set;}
        public float z {get; set;}

        public Vector3(float x, float y, float z){
            this.x = x;
            this.y = y; 
            this.z = z;
        }

        public float magnitude(){
            return (float) Math.Sqrt((x*x) + (y*y) + (z*z));
        }

        public Vector3 normalized(){
            float mag = magnitude();
            return new Vector3(x / mag, y/mag, z / mag);
        }

        public override string ToString(){
            return x + "/" + y + "/" + z;
        }

        public static Vector3 Deserialise(string pos){
            string[] temp = pos.Split('/');
            return new Vector3(float.Parse(temp[0]),float.Parse(temp[1]),float.Parse(temp[2]));
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2){
            return new Vector3(v2.x - v1.x, v2.y - v1.y, v2.z - v1.z);
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2){
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3 operator *(Vector3 v, float f){
            return new Vector3(v.x*f, v.y*f, v.z*f);
        }
    }
}