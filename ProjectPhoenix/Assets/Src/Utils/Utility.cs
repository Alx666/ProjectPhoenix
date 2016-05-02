using UnityEngine;
using System.Collections;
using System;

public class Utility 
{
  
  static public float ClampAngle(float angle, float max, float min)
    {
        if (angle < 90 || angle > 270)
        {       // if angle in the critic region...
            if (angle > 180) angle -= 360;  // convert all angles to -180..+180
            if (max > 180) max -= 360;
            if (min > 180) min -= 360;
        }
        angle = Mathf.Clamp(angle, min, max);
        if (angle < 0) angle += 360;  // if angle negative, convert to 0..360
        return angle;
    }
    static public bool aimAngle(double v, double g, double x, double y, out float angle)
    {
        angle = 0;

        double v2 = Math.Pow(v, 2);
        double v4 = Math.Pow(v, 4);
        double gpart = g * (g * Math.Pow(x, 2) + (2 * y * v2));
        double sqrt = Math.Sqrt(v4 - gpart);
        //    sqrt = traj ? sqrt : -sqrt;
        if (double.IsNaN(sqrt))
            return false;

        double numerator = v2 - sqrt;
        double argument = numerator / (g * x);
        angle = -(float)(Mathf.Rad2Deg * Math.Atan(argument));

        return true;
    }
}
