using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Group.Model.Status;
using TGC.Group.Utils;

namespace TGC.Group.Model._2D
{
    class Character2D
    {
        private struct Constants
        {
            public static int SCREEN_WIDTH = D3DDevice.Instance.Device.Viewport.Width;
            public static int SCREEN_HEIGHT = D3DDevice.Instance.Device.Viewport.Height;
            public static TGCVector2 LIFE_CHARACTER_SCALE = new TGCVector2(0.3f, 0.45f);
            public static TGCVector2 LIFE_CHARACTER_POSITION = new TGCVector2(20, SCREEN_HEIGHT - 80);
            public static TGCVector2 LIFE_CHARACTER_TEXT_SIZE = new TGCVector2(150, 50);
            public static TGCVector2 LIFE_CHARACTER_TEXT_POSITION = new TGCVector2(((1000 * LIFE_CHARACTER_SCALE.X) - LIFE_CHARACTER_TEXT_SIZE.X + 20) / 2, LIFE_CHARACTER_POSITION.Y + 15);
            public static TGCVector2 OXYGEN_CHARACTER_POSITION = new TGCVector2(20, LIFE_CHARACTER_POSITION.Y + 25);
            public static TGCVector2 OXYGEN_CHARACTER_SCALE = new TGCVector2(0.3f, 0.45f);
            public static TGCVector2 OXYGEN_CHARACTER_TEXT_SIZE = new TGCVector2(150, 50);
            public static TGCVector2 OXYGEN_CHARACTER_TEXT_POSITION = new TGCVector2(((1000 * OXYGEN_CHARACTER_SCALE.X) - OXYGEN_CHARACTER_TEXT_SIZE.X + 20) / 2, OXYGEN_CHARACTER_POSITION.Y + 15);
        }

        private readonly DrawSprite Life;
        private readonly DrawSprite Oxygen;
        private readonly DrawText LifeText;
        private readonly DrawText OxygenText;
        private CharacterStatus Status { get; set; }

        public Character2D(string MediaDir, CharacterStatus status)
        {
            Status = status;
            Life = new DrawSprite(MediaDir);
            Oxygen = new DrawSprite(MediaDir);
            LifeText = new DrawText();
            OxygenText = new DrawText();
            Init();
        }

        public void Dispose()
        {
            Life.Dispose();
            LifeText.Dispose();
            Oxygen.Dispose();
            OxygenText.Dispose();
        }

        public void Init()
        {
            InitializerLifeCharacter();
            InitializerOxygenCharacter();
        }

        private void InitializerLifeCharacter()
        {
            Life.SetImage("LifeBar.png");
            Life.SetInitialScallingAndPosition(Constants.LIFE_CHARACTER_SCALE, Constants.LIFE_CHARACTER_POSITION);
            LifeText.Size = Constants.LIFE_CHARACTER_TEXT_SIZE;
        }

        private void InitializerOxygenCharacter()
        {
            Oxygen.SetImage("OxygenBar.png");
            Oxygen.SetInitialScallingAndPosition(Constants.OXYGEN_CHARACTER_SCALE, Constants.OXYGEN_CHARACTER_POSITION);
            OxygenText.Size = new TGCVector2(Constants.OXYGEN_CHARACTER_TEXT_SIZE.X + 300, Constants.OXYGEN_CHARACTER_TEXT_SIZE.Y);
        }

        public void Render()
        {
            Life.Render();
            Oxygen.Render();
            LifeText.SetTextAndPosition(text: " Life   " + Status.ShowLife + @" / " + Status.GetLifeMax(),
                                                 position: Constants.LIFE_CHARACTER_TEXT_POSITION);
            OxygenText.SetTextAndPosition(text: "    O₂    " + Status.ShowOxygen + @" / " + Status.GetOxygenMax(),
                                                   position: Constants.OXYGEN_CHARACTER_TEXT_POSITION);
            LifeText.Render();
            OxygenText.Render();
        }

        public void Update()
        {
            UpdateSprite(Life, Status.Life, Status.GetLifeMax());
            UpdateSprite(Oxygen, Status.Oxygen, Status.GetOxygenMax());
        }

        public void UpdateForGodMode()
        {
            ResetSprite(Oxygen);
            ResetSprite(Life);
        }

        private void UpdateSprite(DrawSprite sprite, float percentage, float max) => sprite.Scaling = new TGCVector2((percentage / max) * sprite.ScalingInitial.X, sprite.ScalingInitial.Y);

        private void ResetSprite(DrawSprite sprite) => sprite.Scaling = sprite.ScalingInitial;
    }
}
