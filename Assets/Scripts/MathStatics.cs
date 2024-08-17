using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathStatics
{
    public static float Map(float x, float inMin, float inMax, float outMin, float outMax)
    {
        return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }


}
