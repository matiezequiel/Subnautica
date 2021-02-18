using System;
using System.Collections.Generic;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Utils;
using static TGC.Group.Model.GameModel;
using static TGC.Group.Model.Objects.Common;
using static TGC.Group.Model.Objects.Vegetation;

namespace TGC.Group.Model.Objects
{
    internal class MeshBuilder
    {
        private struct Constants
        {
            public static int MESH_TERRAIN_OFFSET = 300;
            public static int MAX_POSITION_Y = 500;
        }

        private readonly Random random;
        private readonly Terrain Terrain;
        private readonly Water Water;

        public MeshBuilder(Terrain terrain, Water water)
        {
            random = new Random();
            Terrain = terrain;
            Water = water;
        }

        private TGCVector3 CalculateRotation(TGCVector3 normalObjeto)
        {
            var objectInclinationX = FastMath.Atan2(normalObjeto.X, normalObjeto.Y);
            var objectInclinationZ = FastMath.Atan2(normalObjeto.X, normalObjeto.Y);
            var rotation = new TGCVector3(-objectInclinationX, 0, -objectInclinationZ);
            return rotation;
        }

        private (int XPosition, int ZPosition) GetXZPositionByPerimeter(Perimeter perimeter)
        {
            var XMin = (int)perimeter.xMin;
            var XMax = (int)perimeter.xMax;
            var ZMin = (int)perimeter.zMin;
            var ZMax = (int)perimeter.zMax;

            var xPosition = random.Next(XMin, XMax);
            var zPosition = random.Next(ZMin, ZMax);

            return (XPosition: xPosition, ZPosition: zPosition);
        }

        private bool IsFish(string name) => FastUtils.Contains(name, "fish");

        private void LocateFish(ref TgcMesh mesh, (int XPosition, int ZPosition) pairXZ, float YPosition)
        {
            YPosition = random.Next((int)YPosition + Constants.MESH_TERRAIN_OFFSET, (int)Water.world.Center.Y - Constants.MAX_POSITION_Y);
            var position = new TGCVector3(pairXZ.XPosition, YPosition, pairXZ.ZPosition);
            mesh.Transform *= TGCMatrix.Translation(pairXZ.XPosition, YPosition, pairXZ.ZPosition);
            mesh.Position = position;
        }

        private void LocateBubble(ref TGCSphere mesh, (int XPosition, int ZPosition) pairXZ, float YPosition)
        {
            YPosition = random.Next((int)YPosition + Constants.MESH_TERRAIN_OFFSET, (int)Water.world.Center.Y - Constants.MAX_POSITION_Y);
            var position = new TGCVector3(pairXZ.XPosition, YPosition, pairXZ.ZPosition);
            mesh.Transform *= TGCMatrix.Translation(pairXZ.XPosition, YPosition, pairXZ.ZPosition);
            mesh.Position = position;
        }

        public void LocateMeshInWorld(ref TgcMesh mesh, Perimeter area)
        {
            var pairXZ = GetXZPositionByPerimeter(area);
            Terrain.world.InterpoledHeight(pairXZ.XPosition, pairXZ.ZPosition, out float YPosition);

            if (IsFish(mesh.Name))
            {
                LocateFish(ref mesh, pairXZ, YPosition);
            }
            else
            {
                LocateMeshesTypeTerrain(ref mesh, pairXZ, YPosition);
            }
        }

        public void LocateMeshInWorld(ref TGCSphere mesh, Perimeter area)
        {
            var pairXZ = GetXZPositionByPerimeter(area);
            Terrain.world.InterpoledHeight(pairXZ.XPosition, pairXZ.ZPosition, out float YPosition);

            LocateBubble(ref mesh, pairXZ, YPosition);
        }

        public void LocateMeshesInWorld(ref List<TGCSphere> meshes, Perimeter area) => meshes.ForEach(mesh => LocateMeshInWorld(ref mesh, area));
        public void LocateMeshesInWorld(ref List<TypeCommon> meshes, Perimeter area) => meshes.ForEach(common => LocateMeshInWorld(ref common.Mesh, area));
        public void LocateMeshesInWorld(ref List<TypeVegetation> meshes, Perimeter area) => meshes.ForEach(vegetation => LocateMeshInWorld(ref vegetation.Mesh, area));

        private void LocateMeshesTypeTerrain(ref TgcMesh mesh, (int XPosition, int ZPosition) pairXZ, float YPosition)
        {
            var position = new TGCVector3(pairXZ.XPosition, YPosition, pairXZ.ZPosition);
            var rotation = CalculateRotation(Terrain.world.NormalVectorGivenXZ(position.X, position.Z));
            mesh.Transform *= TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * TGCMatrix.Translation(position);
            mesh.Position = position;
        }
    }
}