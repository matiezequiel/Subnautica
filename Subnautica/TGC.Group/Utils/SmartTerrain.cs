using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Group.Utils
{
    public class SmartTerrain
    {
        public float Length { get; protected set; }
        public float Width { get; protected set; }
        public float[,] HeightmapData { get; private set; }
        public TGCVector3 Center { get; private set; }
        public TGCVector3 CameraPos { get; private set; }
        private float MaxIntensity = 0;
        private float MinIntensity = -1;
        private TGCVector3 Traslation;
        private VertexBuffer VertexTerrain;
        private CustomVertex.PositionTextured[] Vertex;
        private Texture Texture;
        public TgcBoundingAxisAlignBox BoundingBox { get; private set; }
        private int VertexTotal { get; set; }
        public Effect Effect { get; set; }
        private float TimeForWaves = 0;
        public float ScaleXZ { get; set; }
        public float ScaleY { get; set; }

        public SmartTerrain()
        {
            BoundingBox = new TgcBoundingAxisAlignBox();
        }
        public void LoadTexture(string path)
        {
            if (Texture != null && !Texture.Disposed)
            {
                Texture.Dispose();
            }

            var bitMap = (Bitmap)Image.FromFile(path);
            bitMap.RotateFlip(RotateFlipType.Rotate90FlipX);
            Texture = Texture.FromBitmap(D3DDevice.Instance.Device, bitMap, Usage.AutoGenerateMipMap, Pool.Managed);
            bitMap.Dispose();
        }

        protected float[,] LoadHeightMap(string path)
        {
            var bitmap = (Bitmap)Image.FromFile(path);
            var width = bitmap.Size.Width;
            var length = bitmap.Size.Height;
            var heightmap = new float[length, width];

            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var pixel = bitmap.GetPixel(j, i);
                    var intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[i, j] = intensity;
                }
            }

            bitmap.Dispose();
            return heightmap;
        }

        internal void SetCameraPosition(TGCVector3 cameraPos)
        {
            CameraPos = cameraPos;
        }

        public void SetHeightmapData(float[,] heightmapData)
        {
            if (heightmapData.GetLength(0) == HeightmapData.GetLength(0) && HeightmapData.GetLength(1) == heightmapData.GetLength(1))
            {
                HeightmapData = heightmapData;
            }
        }

        public void LoadHeightmap(string heightmapPath, float scaleXZ, float scaleY, TGCVector3 center)
        {
            Center = center;
            ScaleXZ = scaleXZ;
            ScaleY = scaleY;

            if (VertexTerrain != null && !VertexTerrain.Disposed)
            {
                VertexTerrain.Dispose();
            }

            HeightmapData = LoadHeightMap(heightmapPath);
            Width = HeightmapData.GetLength(0);
            Length = HeightmapData.GetLength(1);
            var totalvertices = 2 * 3 * (Width - 1) * (Length - 1);
            VertexTotal = (int)totalvertices;

            VertexTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), VertexTotal,
                                         D3DDevice.Instance.Device,
                                         Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            LoadVertices();
        }

        private void LoadVertices()
        {
            Traslation.X = Center.X - Width / 2;
            Traslation.Y = Center.Y;
            Traslation.Z = Center.Z - Length / 2;

            var dataIdx = 0;

            Vertex = new CustomVertex.PositionTextured[VertexTotal];

            for (var i = 0; i < Width - 1; i++)
            {
                for (var j = 0; j < Length - 1; j++)
                {
                    if (HeightmapData[i, j] > MaxIntensity)
                    {
                        MaxIntensity = HeightmapData[i, j];
                    }

                    if (MinIntensity == -1 || HeightmapData[i, j] < MinIntensity)
                    {
                        MinIntensity = HeightmapData[i, j];
                    }

                    //Vertices
                    var v1 = new TGCVector3((Traslation.X + i) * ScaleXZ, (Traslation.Y + HeightmapData[i, j]) * ScaleY, (Traslation.Z + j) * ScaleXZ);
                    var v2 = new TGCVector3((Traslation.X + i) * ScaleXZ, (Traslation.Y + HeightmapData[i, j + 1]) * ScaleY, (Traslation.Z + (j + 1)) * ScaleXZ);
                    var v3 = new TGCVector3((Traslation.X + i + 1) * ScaleXZ, (Traslation.Y + HeightmapData[i + 1, j]) * ScaleY, (Traslation.Z + j) * ScaleXZ);
                    var v4 = new TGCVector3((Traslation.X + i + 1) * ScaleXZ, (Traslation.Y + HeightmapData[i + 1, j + 1]) * ScaleY, (Traslation.Z + j + 1) * ScaleXZ);

                    //Coordendas de textura
                    var t1 = new TGCVector2(i / Width, j / Length);
                    var t2 = new TGCVector2(i / Width, (j + 1) / Length);
                    var t3 = new TGCVector2((i + 1) / Width, j / Length);
                    var t4 = new TGCVector2((i + 1) / Width, (j + 1) / Length);

                    //Cargar triangulo 1
                    Vertex[dataIdx + 0] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                    Vertex[dataIdx + 1] = new CustomVertex.PositionTextured(v2, t2.X, t2.Y);
                    Vertex[dataIdx + 2] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);

                    //Cargar triangulo 2
                    Vertex[dataIdx + 3] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                    Vertex[dataIdx + 4] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);
                    Vertex[dataIdx + 5] = new CustomVertex.PositionTextured(v3, t3.X, t3.Y);

                    dataIdx += 6;
                }
                VertexTerrain.SetData(Vertex, 0, LockFlags.None);

                var size = HeightmapData.GetLength(0) / 2;
                var pMin = new TGCVector3(size * -ScaleXZ,
                                            MinIntensity * ScaleY,
                                            size * -ScaleXZ);
                var pMax = new TGCVector3(size * ScaleXZ,
                                            MaxIntensity * ScaleY,
                                            size * ScaleXZ);

                BoundingBox.setExtremes(pMin, pMax);
            }
        }

        public void UpdateVertices()
        {
            for (var i = 0; i < Vertex.Length; i++)
            {
                var intensity = HeightmapData[(int)Vertex[i].X, (int)Vertex[i].Z];
                Vertex[i].Y = intensity;
                if (intensity > MaxIntensity)
                {
                    MaxIntensity = intensity;
                }

                if (MinIntensity == -1 || intensity < MinIntensity)
                {
                    MinIntensity = intensity;
                }
            }

            VertexTerrain.SetData(Vertex, 0, LockFlags.None);
        }

        public void Render()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            var texturesManager = TexturesManager.Instance;
            var shader = TGCShaders.Instance;

            Effect.SetValue("CameraPos", TGCVector3.TGCVector3ToFloat4Array(CameraPos));
            Effect.SetValue("time", TimeForWaves);

            texturesManager.clear(1);
            shader.SetShaderMatrix(Effect, TGCMatrix.Identity);
            d3dDevice.VertexDeclaration = shader.VdecPositionTextured;
            d3dDevice.SetStreamSource(0, VertexTerrain, 0);

            var p = Effect.Begin(0);
            for (var i = 0; i < p; i++)
            {
                Effect.BeginPass(i);
                d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, VertexTotal / 3);
                Effect.EndPass();
            }
            Effect.End();
        }

        public void Dispose()
        {
            if (VertexTerrain != null)
            {
                VertexTerrain.Dispose();
            }

            if (Texture != null)
            {
                Texture.Dispose();
            }
        }

        public void LoadEffect(string effectPath, string technique)
        {
            Effect = TGCShaders.Instance.LoadEffect(effectPath);
            Effect.Technique = technique;
            Effect.SetValue("texDiffuseMap", Texture);
            Effect.SetValue("ColorFog", Color.SteelBlue.ToArgb());
            Effect.SetValue("StartFogDistance", 2000);
            Effect.SetValue("EndFogDistance", 10000);
        }

        public void SetTimeForWaves(float elapsedTime) => TimeForWaves += elapsedTime;

        public CustomVertex.PositionTextured[] GetVertices() => Vertex;

        private bool XZToHeightmapCoords(float x, float z, out TGCVector2 coords)
        {
            float i = x / ScaleXZ - Traslation.X;
            float j = z / ScaleXZ - Traslation.Z;

            coords = new TGCVector2(i, j);

            if (coords.X >= HeightmapData.GetLength(0) || coords.Y >= HeightmapData.GetLength(1) || coords.Y < 0 || coords.X < 0)
            {
                return false;
            }

            return true;
        }

        public bool InterpoledHeight(float x, float z, out float y)
        {
            y = 0;
            if (!XZToHeightmapCoords(x, z, out TGCVector2 coords))
            {
                return false;
            }

            InterpoledIntensity(coords.X, coords.Y, out float i);
            y = (i + Traslation.Y) * ScaleY;
            return true;
        }

        private bool InterpoledIntensity(float u, float v, out float i)
        {
            i = 0;
            float maxX = HeightmapData.GetLength(0);
            float maxZ = HeightmapData.GetLength(1);
            if (u >= maxX || v >= maxZ || v < 0 || u < 0)
            {
                return false;
            }

            int x1, x2, z1, z2;
            float s, t;

            x1 = (int)FastMath.Floor(u);
            x2 = x1 + 1;
            s = u - x1;

            z1 = (int)FastMath.Floor(v);
            z2 = z1 + 1;
            t = v - z1;

            if (z2 >= maxZ)
            {
                z2--;
            }

            if (x2 >= maxX)
            {
                x2--;
            }

            var i1 = HeightmapData[x1, z1] + s * (HeightmapData[x2, z1] - HeightmapData[x1, z1]);
            var i2 = HeightmapData[x1, z2] + s * (HeightmapData[x2, z2] - HeightmapData[x1, z2]);

            i = i1 + t * (i2 - i1);
            return true;
        }

        public TGCVector3 NormalVectorGivenXZ(float X, float Z)
        {
            float delta = 0.3f;

            InterpoledHeight(X, Z + delta, out float alturaN);
            InterpoledHeight(X, Z - delta, out float alturaS);
            InterpoledHeight(X + delta, Z, out float alturaE);
            InterpoledHeight(X - delta, Z, out float alturaO);

            TGCVector3 vectorEO = new TGCVector3(delta * 2, alturaE - alturaO, 0);
            TGCVector3 vectorNS = new TGCVector3(0, alturaN - alturaS, delta * 2);

            return TGCVector3.Cross(vectorNS, vectorEO);
        }
    }
}