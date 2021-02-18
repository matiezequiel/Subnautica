using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Textures;

namespace TGC.Group.Utils
{
    internal class DrawSprite
    {
        private readonly string MediaDir;
        private Sprite Sprite { get; set; }
        private Rectangle SrcRect { get; set; }
        private TgcTexture Texture { get; set; }
        private TGCMatrix TransformationMatrix { get; set; }

        private TGCVector2 position;
        private float rotation;
        private TGCVector2 rotationCenter;
        private TGCVector2 scaling;
        private TGCVector2 scalingCenter;

        public Color Color { get; set; }
        public string Name { get; set; }

        public TGCVector2 Position { get { return position; } set { position = value; UpdateTransformationMatrix(); } }
        public float Rotation { get { return rotation; } set { rotation = value; UpdateTransformationMatrix(); } }
        public TGCVector2 RotationCenter { get { return rotationCenter; } set { rotationCenter = value; UpdateTransformationMatrix(); } }
        public TGCVector2 Scaling { get { return scaling; } set { scaling = value; UpdateTransformationMatrix(); } }
        public TGCVector2 ScalingCenter { get { return scalingCenter; } set { scalingCenter = value; UpdateTransformationMatrix(); } }
        public TGCVector2 ScalingInitial { get; private set; }
        public TGCVector2 Size { get { return new TGCVector2(Texture.Size.Width * Scaling.X, Texture.Size.Height * Scaling.Y); } private set { } }

        public DrawSprite(string mediaDir)
        {
            MediaDir = mediaDir;
            Initialize();
        }

        public void Dispose()
        {
            if (Texture != null)
            {
                Texture.dispose();
            }

            if (Sprite != null && !Sprite.Disposed)
            {
                Sprite.Dispose();
            }
        }

        private void Initialize()
        {
            TransformationMatrix = TGCMatrix.Identity;
            SrcRect = Rectangle.Empty;
            Position = TGCVector2.Zero;
            ScalingInitial = Scaling = TGCVector2.One;
            ScalingCenter = TGCVector2.Zero;
            Rotation = 0;
            RotationCenter = TGCVector2.Zero;
            Color = Color.White;
            Sprite = new Sprite(D3DDevice.Instance.Device);
        }

        public void Render()
        {
            Sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);
            Sprite.Transform = TransformationMatrix.ToMatrix();
            Sprite.Draw(Texture.D3dTexture, SrcRect, TGCVector3.Empty, TGCVector3.Empty, Color);
            Sprite.End();
        }

        public void SetImage(string imageNameAndExtension)
        {
            try { Texture = TgcTexture.createTexture(MediaDir + @"Images\" + imageNameAndExtension); }
            catch { throw new Exception("Sprite image file, not found!"); }
            Name = imageNameAndExtension.Split('.')[0].ToUpper();
        }

        public void SetInitialScallingAndPosition(TGCVector2 scale, TGCVector2 position)
        {
            Scaling = ScalingInitial = scale;
            Position = position;
            Size = new TGCVector2(Texture.Size.Width * scale.X, Texture.Size.Height * scale.Y);
        }

        private void UpdateTransformationMatrix() => TransformationMatrix = TGCMatrix.Transformation2D(scalingCenter, 0, scaling, rotationCenter, rotation, position);
    }
}