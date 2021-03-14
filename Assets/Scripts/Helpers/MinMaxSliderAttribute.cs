﻿using System;
using UnityEngine;

public class MinMaxSliderAttribute : PropertyAttribute
{

    public readonly float Min;
    public readonly float Max;

    public MinMaxSliderAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }

}