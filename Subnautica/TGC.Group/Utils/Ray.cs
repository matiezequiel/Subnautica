using System;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Input;
using TGC.Core.Mathematica;

namespace TGC.Group.Utils
{
    class Ray
    {
        private readonly TgcD3dInput Input;
        private readonly TgcPickingRay pickingRay;

        public Ray(TgcD3dInput input)
        {
            Input = input;
            pickingRay = new TgcPickingRay(Input);
        }

        public bool IntersectsWithObject(TgcBoundingAxisAlignBox objectAABB, float distance)
        {
            pickingRay.updateRay();

            bool intersected = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, objectAABB, out TGCVector3 collisionPoint);
            bool inSight = Math.Sqrt(TGCVector3.LengthSq(pickingRay.Ray.Origin, collisionPoint)) < distance;

            return intersected && inSight;
        }

        public bool IntersectsWithObject(TGCPlane objectPlane, float distance)
        {
            pickingRay.updateRay();

            bool intersected = TgcCollisionUtils.intersectRayPlane(pickingRay.Ray, objectPlane, out _, out TGCVector3 collisionPoint);
            bool inSight = Math.Sqrt(TGCVector3.LengthSq(pickingRay.Ray.Origin, collisionPoint)) < distance;

            return intersected && inSight;
        }

        public bool GetDistanceWithObject(TgcBoundingAxisAlignBox objectAABB, out float distance)
        {
            pickingRay.updateRay();
            distance = TgcCollisionUtils.sqDistPointAABB(pickingRay.Ray.Origin, objectAABB);
            return distance > 0;
        }
    }
}
