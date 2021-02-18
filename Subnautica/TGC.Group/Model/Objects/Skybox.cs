using BulletSharp;
using Microsoft.DirectX.Direct3D;
using System.Linq;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Group.Utils;
using static TGC.Group.Model.GameModel;

namespace TGC.Group.Model.Objects
{
    internal class Skybox
    {
        private struct Constants
        {
            public static TGCVector3 SKYBOX_SIZE = new TGCVector3(30000, 30000, 30000);
            public static TGCVector3 SKYBOX_CENTER = new TGCVector3(0, 1750, 0);
            public static string SKYBOX_TEXTURE_FOLDER = @"SkyBox\";
            public static string SKYBOX_FACE_TEXTURE_UP = "Daylight Box_Top.bmp";
            public static string SKYBOX_FACE_TEXTURE_DOWN = "Daylight Box_Bottom.bmp";
            public static string SKYBOX_FACE_TEXTURE_LEFT = "Daylight Box_Left.bmp";
            public static string SKYBOX_FACE_TEXTURE_RIGHT = "Daylight Box_Right.bmp";
            public static string SKYBOX_FACE_TEXTURE_FRONT = "Daylight Box_Front.bmp";
            public static string SKYBOX_FACE_TEXTURE_BACK = "Daylight Box_Back.bmp";
        }

        private readonly string MediaDir;
        private readonly TgcSkyBox skybox;
        private readonly CameraFPS Camera;
        public Perimeter CurrentPerimeter;
        public float Radius => Constants.SKYBOX_SIZE.X / 2;
        public bool IsNearSkybox => !InPerimeterSkyBox(Camera.Position.X, Camera.Position.Z);

        public Skybox(string mediaDir, CameraFPS camera)
        {
            skybox = new TgcSkyBox
            {
                Size = Constants.SKYBOX_SIZE,
                Center = Constants.SKYBOX_CENTER
            };

            MediaDir = mediaDir;
            Camera = camera;
            LoadSkyBox();
        }

        private void LoadSkyBox()
        {
            var texturesPath = MediaDir + Constants.SKYBOX_TEXTURE_FOLDER;
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + Constants.SKYBOX_FACE_TEXTURE_UP);
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + Constants.SKYBOX_FACE_TEXTURE_DOWN);
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + Constants.SKYBOX_FACE_TEXTURE_LEFT);
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + Constants.SKYBOX_FACE_TEXTURE_RIGHT);
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + Constants.SKYBOX_FACE_TEXTURE_FRONT);
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + Constants.SKYBOX_FACE_TEXTURE_BACK);
            skybox.SkyEpsilon = 60f;

            skybox.Init();
            CalculatePerimeter();
        }

        public void SetShader(Effect effect, string technique)
        {
            skybox.Faces.ToList().ForEach(face =>
            {
                face.Effect = effect;
                face.Technique = technique;
            });
            skybox.Faces[0].Technique = "Sun";
        }

        public void Update() => CalculatePerimeter();
        public void Render() => skybox.Render();

        public void Render(Perimeter worldSize)
        {
            skybox.Center = new TGCVector3(FastMath.Clamp(Camera.Position.X, worldSize.xMin + Radius, worldSize.xMax - Radius),
                                        skybox.Center.Y,
                                        FastMath.Clamp(Camera.Position.Z, worldSize.zMin + Radius, worldSize.zMax - Radius));
            skybox.Render();
        }

        public void Dispose() => skybox.Dispose();

        public bool Contains(RigidBody rigidBody)
        {
            var posX = rigidBody.CenterOfMassPosition.X;
            var posZ = rigidBody.CenterOfMassPosition.Z;
            return InPerimeterSkyBox(posX, posZ);
        }

        public bool Contains(TgcMesh mesh)
        {
            var posX = mesh.Position.X;
            var posZ = mesh.Position.Z;
            mesh.AlphaBlendEnable = true;
            return InPerimeterSkyBox(posX, posZ);
        }

        public TGCVector3 GetSkyboxCenter() => skybox.Center;

        public bool InPerimeterSkyBox(float posX, float posZ) =>
            FastUtils.IsNumberBetweenInterval(posX, (CurrentPerimeter.xMin, CurrentPerimeter.xMax)) &&
            FastUtils.IsNumberBetweenInterval(posZ, (CurrentPerimeter.zMin, CurrentPerimeter.zMax));

        private void CalculatePerimeter()
        {
            var size = skybox.Size.X / 2;

            CurrentPerimeter.xMin = skybox.Center.X - size;
            CurrentPerimeter.xMax = skybox.Center.X + size;
            CurrentPerimeter.zMin = skybox.Center.Z - size;
            CurrentPerimeter.zMax = skybox.Center.Z + size;
        }
    }
}
