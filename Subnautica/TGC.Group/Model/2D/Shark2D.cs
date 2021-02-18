using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Group.Model.Status;
using TGC.Group.Utils;

namespace TGC.Group.Model._2D
{
    class Shark2D
    {
        private struct Constants
        {
            public static int SCREEN_WIDTH = D3DDevice.Instance.Device.Viewport.Width;
            public static int SCREEN_HEIGHT = D3DDevice.Instance.Device.Viewport.Height;
            public static TGCVector2 LIFE_SHARK_SCALE = new TGCVector2(0.4f, 0.5f);
            public static TGCVector2 LIFE_SHARK_SIZE = new TGCVector2(1000 * LIFE_SHARK_SCALE.X, 100 * LIFE_SHARK_SCALE.Y);
            public static TGCVector2 LIFE_SHARK_POSITION = new TGCVector2(SCREEN_WIDTH - LIFE_SHARK_SIZE.X - 20, 0);
            public static TGCVector2 LIFE_SHARK_TEXT_SIZE = new TGCVector2(130, 100);
            public static TGCVector2 LIFE_SHARK_TEXT_POSITION = new TGCVector2(LIFE_SHARK_POSITION.X - LIFE_SHARK_TEXT_SIZE.X, 20f);
        }

        private readonly SharkStatus Status;
        private readonly DrawSprite LifeShark;
        private readonly DrawText LifeSharkText;

        public Shark2D(string MediaDir, SharkStatus status)
        {
            Status = status;
            LifeShark = new DrawSprite(MediaDir);
            LifeSharkText = new DrawText();
            InitializerLifeShark();
        }

        public void Dispose()
        {
            LifeShark.Dispose();
            LifeSharkText.Dispose();
        }

        private void InitializerLifeShark()
        {
            LifeShark.SetImage("LifeBar.png");
            LifeShark.SetInitialScallingAndPosition(Constants.LIFE_SHARK_SCALE, Constants.LIFE_SHARK_POSITION);
            LifeSharkText.SetTextSizeAndPosition(text: "SHARK LIFE", size: Constants.LIFE_SHARK_TEXT_SIZE, position: Constants.LIFE_SHARK_TEXT_POSITION);
            LifeSharkText.Color = Color.MediumVioletRed;
        }

        public void Render()
        {
            LifeShark.Render();
            LifeSharkText.Render();
        }

        public void Update() => LifeShark.Scaling = new TGCVector2((Status.Life / Status.GetLifeMax()) * LifeShark.ScalingInitial.X, LifeShark.ScalingInitial.Y);
    }
}
