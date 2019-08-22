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

        public double magnitude(){
            return Math.Sqrt((x*x) + (y*y) + (z*z));
        }
    }
}