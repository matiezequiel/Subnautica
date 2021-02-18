using BulletSharp;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.BulletPhysics;
using TGC.Core.Textures;

namespace TGC.Group.Model.Objects
{
    internal class Terrain : World
    {
        private readonly BulletRigidBodyFactory RigidBodyFactory = BulletRigidBodyFactory.Instance;
        public RigidBody Body { get; set; }

        public Terrain(string mediaDir, string shadersDir) : base(mediaDir, shadersDir) => Init();

        private void Init()
        {
            FILE_HEIGHTMAPS = @"Heightmaps\suelo.jpg";
            FILE_TEXTURES = @"Textures\sand.jpg";
            FILE_EFFECT = "Shaders.fx";
            Technique = "DiffuseMap";
            LoadWorld();
            Body = RigidBodyFactory.CreateSurfaceFromHeighMap(world.GetVertices());
            var texture = TgcTexture.createTexture(MediaDir + @"Textures\reflex.jpg");
            world.Effect.SetValue("texReflex", texture.D3dTexture);
            world.Effect.SetValue("ColorFog", Color.SteelBlue.ToArgb());
            world.Effect.SetValue("StartFogDistance", 2000);
            world.Effect.SetValue("EndFogDistance", 10000);
        }

        public override void Dispose()
        {
            Body.Dispose();
            base.Dispose();
        }

        public void SetShader(Effect fogShader, string technique)
        {
            world.Effect = fogShader;
            world.Effect.Technique = technique;
        }
    }
}