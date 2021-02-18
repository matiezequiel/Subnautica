using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Group.Utils;

namespace TGC.Group.Model._2D
{
    class Inventory2D
    {
        private struct Constants
        {
            public static int SCREEN_WIDTH = D3DDevice.Instance.Device.Viewport.Width;
            public static int SCREEN_HEIGHT = D3DDevice.Instance.Device.Viewport.Height;
            public static TGCVector2 INVENTORY_TEXT_SIZE = new TGCVector2(300, 300);
            public static TGCVector2 INVENTORY_TEXT_POSITION = new TGCVector2((SCREEN_WIDTH - INVENTORY_TEXT_SIZE.X) / 2, (SCREEN_HEIGHT - INVENTORY_TEXT_SIZE.Y) / 2);
            public static string INVENTORY_TEXT_GENERIC = "Inventory without items!";
        }

        private readonly string MediaDir;
        private bool HasItems;
        private readonly DrawText TitleInventory;
        private readonly List<(DrawSprite sprite, DrawText text)> InventoryItems;

        private TGCVector2 Size;

        public Inventory2D(string mediaDir)
        {
            MediaDir = mediaDir;
            TitleInventory = new DrawText();
            InventoryItems = new List<(DrawSprite, DrawText)>();
            Init();
        }

        public void Dispose()
        {
            TitleInventory.Dispose();
            InventoryItems.ForEach(item => { item.sprite.Dispose(); item.text.Dispose(); });
        }

        public void Init()
        {
            TitleInventory.Font = new Font("Arial Black", 15, FontStyle.Bold);
            TitleInventory.Color = Color.Crimson;
            TitleInventory.SetTextSizeAndPosition(text: "", Constants.INVENTORY_TEXT_SIZE, Constants.INVENTORY_TEXT_POSITION);
            InventoryItems.Add(InitializerItems("NORMALCORAL"));
            InventoryItems.Add(InitializerItems("SPIRALCORAL"));
            InventoryItems.Add(InitializerItems("TREECORAL"));
            InventoryItems.Add(InitializerItems("GOLD"));
            InventoryItems.Add(InitializerItems("SILVER"));
            InventoryItems.Add(InitializerItems("IRON"));
            InventoryItems.Add(InitializerItems("NORMALFISH"));
            InventoryItems.Add(InitializerItems("YELLOWFISH"));
            CalculateItemPosition();
        }

        private (DrawSprite, DrawText) InitializerItems(string sprite)
        {
            var item = new DrawSprite(MediaDir);
            item.SetImage(sprite + ".png");
            var text = new DrawText();
            return (item, text);
        }

        private void CalculateItemPosition()
        {
            TGCVector2 scale;
            if (Constants.SCREEN_WIDTH < 1366)
            {
                scale = new TGCVector2(0.732f, 0.783f);
            }
            else if (FastUtils.IsNumberBetweenInterval(Constants.SCREEN_WIDTH, (1366, 1700)))
            {
                scale = new TGCVector2(0.9f, 0.9f);
            }
            else
            {
                scale = new TGCVector2(1.2f, 1.2f);
            }

            Size = new TGCVector2(100 * scale.X, 100 * scale.Y);
            TGCVector2 initialPosition = new TGCVector2(Constants.SCREEN_WIDTH * 0.39f, Constants.SCREEN_HEIGHT * 0.35f);

            var columns = 4;
            var count = 1;
            var position = initialPosition;
            InventoryItems[0].sprite.SetInitialScallingAndPosition(scale, position);

            for (int index = 1; index < InventoryItems.Count; index++)
            {
                if (count < columns)
                {
                    position.X = InventoryItems[index - 1].sprite.Position.X + Size.X + 80;
                    position.Y = InventoryItems[index - 1].sprite.Position.Y;
                }
                else
                {
                    position.X = initialPosition.X;
                    position.Y = initialPosition.Y + Size.Y + 80;
                    count = 0;
                }

                count++;
                InventoryItems[index].sprite.SetInitialScallingAndPosition(scale, position);
            }
        }

        public void Render()
        {
            TitleInventory.Render();
            if (HasItems)
            {
                InventoryItems.ForEach(item => { item.sprite.Render(); item.text.Render(); });
            }
        }

        public void UpdateItems(Dictionary<string, List<string>> items)
        {
            HasItems = items.Values.ToList().Any(listItems => listItems.Count > 0);

            if (HasItems)
            {
                TitleInventory.SetTextAndPosition("Inventory:", position: new TGCVector2(InventoryItems[0].sprite.Position.X,
                                                  InventoryItems[0].sprite.Position.Y - 60));
                InventoryItems.ForEach(item =>
                {
                    item.text.SetTextAndPosition("x" + items[item.sprite.Name].Count,
                        position: new TGCVector2(item.sprite.Position.X + Size.X,
                                                  item.sprite.Position.Y + Size.Y + 10));
                });
            }
            else
            {
                TitleInventory.Text = Constants.INVENTORY_TEXT_GENERIC;
            }
        }
    }
}