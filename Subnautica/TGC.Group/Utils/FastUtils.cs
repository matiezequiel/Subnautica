using System;
using TGC.Core.Mathematica;

namespace TGC.Group.Utils
{
    internal static class FastUtils
    {
        public static float AngleBetweenVectors(TGCVector3 vectorA, TGCVector3 vectorB)
        {
            var dotProduct = TGCVector3.Dot(vectorA, vectorB) / (vectorA.Length() * vectorB.Length());
            return dotProduct < 1 ? FastMath.Acos(dotProduct) : 0;
        }
        public static bool IsNumberBetweenInterval(float number, TGCVector2 interval) => number > interval.X && number < interval.Y;
        public static bool IsNumberBetweenInterval(float number, (float min, float max) interval) => number > interval.min && number < interval.max;
        public static TGCVector3 ObtainNormalVector(TGCVector3 vectorA, TGCVector3 vectorB) => TGCVector3.Normalize(TGCVector3.Cross(vectorA, vectorB));
        public static bool LessThan(float numberA, float numberB) => numberA < numberB;
        public static bool GreaterThan(float numberA, float numberB) => numberA > numberB;
        public static bool Contains(string expression, string searchExpression) => expression.ToLower().Contains(searchExpression);
        public static bool IsDistanceBetweenVectorsLessThan(float distance, TGCVector3 vectorA, TGCVector3 vectorB) => DistanceBetweenVectors(vectorA, vectorB) < distance;
        public static float Distance(float numberA, float numberB) => FastMath.Abs(numberB - numberA);
        public static float DistanceBetweenVectors(TGCVector3 vectorA, TGCVector3 vectorB) =>
            (float)Math.Round((vectorA - vectorB).Length(), 2);
    }
}