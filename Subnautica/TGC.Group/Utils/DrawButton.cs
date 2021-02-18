using System;
using TGC.Core.Input;
using TGC.Core.Mathematica;

namespace TGC.Group.Utils
{
    class DrawButton
    {
        public Action Action { get; set; }
        public DrawText ButtonText { get; set; }
        public DrawSprite MarkedButton { get; set; }
        public DrawSprite UnmarkedButton { get; set; }
        public TGCVector2 Position { get; set; }
        public TGCVector2 Scale { get; set; }
        public TGCVector2 SizeText { get; set; }
        public TGCVector2 Size { get; set; }
        public bool Invisible { get; set; }

        private bool IsMarked;
        private readonly TgcD3dInput Input;

        public DrawButton(string mediaDir, TgcD3dInput input)
        {
            Input = input;
            ButtonText = new DrawText();
            MarkedButton = new DrawSprite(mediaDir);
            UnmarkedButton = new DrawSprite(mediaDir);
        }

        public void InitializerButton(string text, TGCVector2 scale, TGCVector2 position, Action action)
        {
            Scale = scale;
            Position = position;
            Action = action;
            MarkedButton.SetImage("marked.png");
            MarkedButton.SetInitialScallingAndPosition(scale, position);
            UnmarkedButton.SetImage("unmarked.png");
            UnmarkedButton.SetInitialScallingAndPosition(scale, position);
            Size = MarkedButton.Size;
            SizeText = new TGCVector2(335 * scale.X * 0.6f, 66 * scale.Y * 0.5f);
            ButtonText.SetTextAndPosition(text, position: Position + SizeText);
        }

        public void ChangePosition(TGCVector2 position)
        {
            MarkedButton.Position = UnmarkedButton.Position = Position = position;
            ButtonText.Position = position + SizeText;
        }

        public void InitializerButton(string text, TGCVector2 scale, TGCVector2 position)
        {
            Scale = scale;
            Position = position;
            MarkedButton.SetImage("marked.png");
            MarkedButton.SetInitialScallingAndPosition(scale, position);
            UnmarkedButton.SetImage("unmarked.png");
            UnmarkedButton.SetInitialScallingAndPosition(scale, position);
            Size = MarkedButton.Size;
            SizeText = new TGCVector2(335 * scale.X * 0.6f, 66 * scale.Y * 0.5f);
            ButtonText.SetTextAndPosition(text, position: Position + SizeText);
        }

        public void Dispose()
        {
            ButtonText.Dispose();
            MarkedButton.Dispose();
            UnmarkedButton.Dispose();
        }

        public void Render()
        {
            if (Invisible)
            {
                return;
            }

            if (IsMarked)
            {
                MarkedButton.Render();
            }
            else
            {
                UnmarkedButton.Render();
            }

            ButtonText.Render();
        }

        public void Update()
        {
            if (Invisible)
            {
                return;
            }

            if (FastUtils.IsNumberBetweenInterval(Input.Xpos, (Position.X, Position.X + Size.X)) &&
                FastUtils.IsNumberBetweenInterval(Input.Ypos, (Position.Y, Position.Y + Size.Y)))
            {
                IsMarked = true;
                if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    Action();
                }
            }
            else
            {
                IsMarked = false;
            }
        }
    }
}
