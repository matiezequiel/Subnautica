using System.Drawing;
using TGC.Core.Mathematica;
using TGC.Core.Text;
using Font = System.Drawing.Font;

namespace TGC.Group.Utils
{
    internal class DrawText
    {
        private TgcText2D Text2D { get; set; }
        private string text;
        private Color color;
        private TGCVector2 position;
        private TGCVector2 size;
        private TgcText2D.TextAlign align;
        private Font font;

        public string Text { get { return text; } set { Text2D.Text = text = value; } }
        public TGCVector2 Position { get { return position; } set { position = value; Text2D.Position = new Point((int)position.X, (int)position.Y); } }
        public TGCVector2 Size { get { return size; } set { size = value; Text2D.Size = new Size((int)size.X, (int)size.Y); } }
        public Color Color { get { return color; } set { Text2D.Color = color = value; } }
        public TgcText2D.TextAlign Align { get { return align; } set { Text2D.Align = align = value; } }
        public Font Font { get { return font; } set { font = value; Text2D.changeFont(font); } }

        public DrawText()
        {
            Text2D = new TgcText2D();
            Initializer();
        }

        public void Dispose()
        {
            if (Text2D != null)
            {
                Text2D.Dispose();
            }
        }

        private void Initializer()
        {
            Text = "";
            Position = TGCVector2.Zero;
            Size = new TGCVector2(100, 100);
            Color = Color.White;
            Align = TgcText2D.TextAlign.LEFT;
            Font = new Font("Arial Black", 14, FontStyle.Bold);
            UpdateTextSettings();
        }

        public void Render() => Text2D.render();

        public void SetTextSizeAndPosition(string text, TGCVector2 size, TGCVector2 position)
        {
            Text = text;
            Position = position;
            Size = size;
        }

        public void SetTextAndPosition(string text, TGCVector2 position)
        {
            Text = text;
            Position = position;
        }

        private void UpdateTextSettings()
        {
            Text2D.Text = Text;
            Text2D.Color = Color;
            Text2D.Align = Align;
            Text2D.Position = new Point((int)Position.X, (int)Position.Y);
            Text2D.Size = new Size((int)Size.X, (int)Size.Y);
            Text2D.changeFont(Font);
        }
    }
}
