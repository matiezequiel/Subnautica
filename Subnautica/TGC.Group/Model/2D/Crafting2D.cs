using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Group.Utils;

namespace TGC.Group.Model._2D
{
    class Crafting2D
    {
        private struct Constants
        {
            public static int SCREEN_WIDTH = D3DDevice.Instance.Device.Viewport.Width;
            public static int SCREEN_HEIGHT = D3DDevice.Instance.Device.Viewport.Height;
            public static TGCVector2 TEXT_SIZE = new TGCVector2(300, 300);
            public static TGCVector2 TEXT_POSITION = new TGCVector2((SCREEN_WIDTH - TEXT_SIZE.X) / 2, (SCREEN_HEIGHT - TEXT_SIZE.Y) / 2);
        }

        private readonly string MediaDir;
        private readonly DrawText TitleInventory;
        private readonly DrawText TitleCrafting;
        private readonly List<(DrawSprite sprite, DrawText text)> InventoryItems;
        public List<(DrawSprite sprite, DrawButton button)> CraftingItems;
        public (DrawSprite sprite, DrawButton button) Weapon;
        private readonly DrawSprite CountCraftingItems;

        private TGCVector2 Size;
        public TgcD3dInput Input { get; set; }
        public bool Learned { get; set; }

        public Crafting2D(string mediaDir, TgcD3dInput input)
        {
            Input = input;
            MediaDir = mediaDir;
            TitleInventory = new DrawText();
            TitleCrafting = new DrawText();
            InventoryItems = new List<(DrawSprite, DrawText)>();
            CraftingItems = new List<(DrawSprite, DrawButton)>();
            CountCraftingItems = new DrawSprite(MediaDir);
            Init();
        }

        public void Dispose()
        {
            TitleInventory.Dispose();
            TitleCrafting.Dispose();
            InventoryItems.ForEach(item => { item.sprite.Dispose(); item.text.Dispose(); });
            CraftingItems.ForEach(item => { item.sprite.Dispose(); item.button.Dispose(); });
            CountCraftingItems.Dispose();
        }

        public void Init()
        {
            TitleInventory.Font = new Font("Arial Black", 15, FontStyle.Bold);
            TitleInventory.Color = Color.Crimson;
            TitleInventory.SetTextSizeAndPosition(text: "Inventory:", Constants.TEXT_SIZE, Constants.TEXT_POSITION);
            InventoryItems.Add(InitializerItems("NORMALCORAL"));
            InventoryItems.Add(InitializerItems("SPIRALCORAL"));
            InventoryItems.Add(InitializerItems("TREECORAL"));
            InventoryItems.Add(InitializerItems("GOLD"));
            InventoryItems.Add(InitializerItems("SILVER"));
            InventoryItems.Add(InitializerItems("IRON"));
            InventoryItems.Add(InitializerItems("NORMALFISH"));
            InventoryItems.Add(InitializerItems("YELLOWFISH"));
            CalculateItemPosition();
            TitleCrafting.Font = new Font("Arial Black", 15, FontStyle.Bold);
            TitleCrafting.Color = Color.Crimson;
            TitleCrafting.Text = "Crafting:";
            CraftingItems.Add(InitializerCraftItem("CATCHFISH"));
            CraftingItems.Add(InitializerCraftItem("WEAPON"));
            CraftingItems.Add(InitializerCraftItem("OXYGEN"));
            CalculateCraftItemPosition();
            CountCraftingItems.SetImage("textCrafting.png");
            CountCraftingItems.Scaling = new TGCVector2(0.36f, 0.6f);
            InitializerWeapon();
        }

        private (DrawSprite, DrawText) InitializerItems(string sprite)
        {
            var item = new DrawSprite(MediaDir);
            item.SetImage(sprite + ".png");
            var text = new DrawText();
            return (item, text);
        }

        private void InitializerWeapon()
        {
            Weapon = InitializerCraftItem("WEAPON");

            TGCVector2 scale;
            if (Constants.SCREEN_WIDTH < 1366)
            {
                scale = new TGCVector2(0.8f / 1.5f, 0.8f / 1.5f);
            }
            else if (FastUtils.IsNumberBetweenInterval(Constants.SCREEN_WIDTH, (1366, 1700)))
            {
                scale = new TGCVector2(1f / 1.5f, 1f / 1.5f);
            }
            else
            {
                scale = new TGCVector2(1.2f / 2, 1.2f / 2);
            }

            var posY = TitleInventory.Position.Y;
            var posX = InventoryItems[3].sprite.Position.X;
            var position = new TGCVector2(posX, posY);
            Weapon.sprite.SetInitialScallingAndPosition(scale, position);
            position = new TGCVector2(position.X + 100, posY + 10);
            Weapon.button.InitializerButton("Equip", new TGCVector2(0.4f, 0.4f), position);
            var textPosition = Weapon.button.ButtonText.Position;
            Weapon.button.ButtonText.Position = new TGCVector2(textPosition.X - 15, textPosition.Y);
        }

        private (DrawSprite, DrawButton) InitializerCraftItem(string sprite)
        {
            var item = new DrawSprite(MediaDir);
            item.SetImage(sprite + ".png");
            var button = new DrawButton(MediaDir, Input);
            button.InitializerButton(text: "Craft", scale: new TGCVector2(0.4f, 0.4f),
                position: Constants.TEXT_POSITION);
            return (item, button);
        }

        private void CalculateItemPosition()
        {
            TGCVector2 scale;
            if (Constants.SCREEN_WIDTH < 1366)
            {
                scale = new TGCVector2(0.8f / 1.85f, 0.8f / 1.85f);
            }
            else if (FastUtils.IsNumberBetweenInterval(Constants.SCREEN_WIDTH, (1366, 1700)))
            {
                scale = new TGCVector2(0.9f / 1.85f, 0.9f / 1.85f);
            }
            else
            {
                scale = new TGCVector2(1.2f / 1.85f, 1.2f / 1.85f);
            }

            Size = new TGCVector2(100 * scale.X, 100 * scale.Y);
            TGCVector2 initialPosition = new TGCVector2(Constants.SCREEN_WIDTH * 0.39f, Constants.SCREEN_HEIGHT * 0.30f);

            var columns = 8;
            var count = 1;
            var position = initialPosition;
            InventoryItems[0].sprite.SetInitialScallingAndPosition(scale, position);
            InventoryItems[0].text.Position = new TGCVector2(InventoryItems[0].sprite.Position.X + Size.X, InventoryItems[0].sprite.Position.Y + Size.Y + 10);

            for (int index = 1; index < InventoryItems.Count; index++)
            {
                if (count < columns)
                {
                    position.X = InventoryItems[index - 1].sprite.Position.X + Size.X + 25;
                    position.Y = InventoryItems[index - 1].sprite.Position.Y;
                }
                else
                {
                    position.X = initialPosition.X;
                    position.Y = initialPosition.Y + Size.Y + 25;
                    count = 0;
                }

                count++;
                InventoryItems[index].sprite.SetInitialScallingAndPosition(scale, position);
                InventoryItems[index].text.Position = new TGCVector2(InventoryItems[index].sprite.Position.X + Size.X, InventoryItems[index].sprite.Position.Y + Size.Y + 10);
            }

            TitleInventory.Position = new TGCVector2(InventoryItems[0].sprite.Position.X, InventoryItems[0].sprite.Position.Y - 40);
            TitleCrafting.Position = new TGCVector2(InventoryItems[0].sprite.Position.X, InventoryItems[0].text.Position.Y + 40);
        }

        private void CalculateCraftItemPosition()
        {
            TGCVector2 scale;
            if (Constants.SCREEN_WIDTH < 1366)
            {
                scale = new TGCVector2(0.8f / 1.5f, 0.8f / 1.5f);
            }
            else if (FastUtils.IsNumberBetweenInterval(Constants.SCREEN_WIDTH, (1366, 1700)))
            {
                scale = new TGCVector2(1f / 1.5f, 1f / 1.5f);
            }
            else
            {
                scale = new TGCVector2(1.2f / 2, 1.2f / 2);
            }

            var Size = new TGCVector2(100 * scale.X, 100 * scale.Y);
            TGCVector2 position = new TGCVector2(InventoryItems[1].sprite.Position.X, TitleCrafting.Position.Y + 30);

            CraftingItems[0].sprite.SetInitialScallingAndPosition(scale, position);
            CraftingItems[0].button.ChangePosition(new TGCVector2(InventoryItems[5].sprite.Position.X, position.Y - 10 + (Size.Y - CraftingItems[0].button.SizeText.Y) / 2));

            for (int index = 1; index < CraftingItems.Count; index++)
            {
                position.X = CraftingItems[index - 1].sprite.Position.X;
                position.Y = CraftingItems[index - 1].sprite.Position.Y + Size.Y + 30;

                CraftingItems[index].sprite.SetInitialScallingAndPosition(scale, position);
                CraftingItems[index].button.ChangePosition(new TGCVector2(InventoryItems[5].sprite.Position.X, position.Y - 10 + (Size.Y - CraftingItems[0].button.SizeText.Y) / 2));
            }
        }

        public void UpdateItemsCrafting()
        {
            CraftingItems.ForEach(item =>
            {
                if (item.sprite.Name != "CATCHFISH")
                {
                    item.button.Update();
                }
                else if (!Learned)
                {
                    item.button.Update();
                }
            });
        }

        public void Render()
        {
            TitleInventory.Render();
            TitleCrafting.Render();
            InventoryItems.ForEach(item => { item.sprite.Render(); item.text.Render(); });
            CraftingItems.ForEach(item =>
            {
                item.sprite.Render();
                if (item.sprite.Name != "CATCHFISH")
                {
                    item.button.Render();
                }
                else if (!Learned)
                {
                    item.button.Render();
                }
                else
                {
                    item.button.ButtonText.Render();
                }
            });

            CraftingItems.ForEach(item =>
            {
                if (FastUtils.IsNumberBetweenInterval(Input.Xpos, (item.sprite.Position.X, item.sprite.Position.X + item.sprite.Size.X)) &&
                    FastUtils.IsNumberBetweenInterval(Input.Ypos, (item.sprite.Position.Y, item.sprite.Position.Y + item.sprite.Size.Y)))
                {
                    CountCraftingItems.Position = new TGCVector2(Cursor.Position.X + 40, Cursor.Position.Y - 12);
                    var text = GameCraftingManager.GetTextCraftingItems()[item.sprite.Name];
                    text.Position = new TGCVector2(CountCraftingItems.Position.X + 10, CountCraftingItems.Position.Y);
                    CountCraftingItems.Render();
                    text.Render();
                }
            });
        }

        public void UpdateItems(Dictionary<string, List<string>> items) =>
            InventoryItems.ForEach(item => item.text.Text = "x" + items[item.sprite.Name].Count);

        public void RenderItemWeapon()
        {
            Weapon.sprite.Render();
            Weapon.button.Render();
        }

        public void UpdateItemWeapon() => Weapon.button.Update();
    }
}