using UnityEngine;

// ReSharper disable once CheckNamespace
namespace EzySlice
{
    public readonly struct Line
    {
        public Line(in Vector3 pta, in Vector3 ptb)
        {
            PositionA = pta;
            PositionB = ptb;
        }

        public float Dist => Vector3.Distance(PositionA, PositionB);

        public float DistSq => (PositionA - PositionB).sqrMagnitude;

        public Vector3 PositionA { get; }

        public Vector3 PositionB { get; }
    }
}