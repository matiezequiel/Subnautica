using TGC.Core.Mathematica;

namespace TGC.Group.Model.Objects
{
    internal class Water : World
    {
        public TGCVector3 WaterPosition { get { return Position; } set { Position = value; } }

        public Water(string mediaDir, string shadersDir, TGCVector3 position) : base(mediaDir, shadersDir)
        {
            WaterPosition = position;
            FILE_HEIGHTMAPS = @"Heightmaps\oceano.jpg";
            FILE_TEXTURES = @"Textures\water.png";
            FILE_EFFECT = "Shaders.fx";
            Technique = "Waves";
            SCALEY = 1;
            LoadWorld();
        }
    }
}
