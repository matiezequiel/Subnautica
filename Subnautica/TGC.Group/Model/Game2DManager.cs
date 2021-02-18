using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Group.Model._2D;
using TGC.Group.Model.Status;
using TGC.Group.Utils;

namespace TGC.Group.Model
{
    internal class Game2DManager
    {
        private struct Constants
        {
            public static float TIME_HISTORY_TEXT = 10f;
            public static int SCREEN_WIDTH = D3DDevice.Instance.Device.Viewport.Width;
            public static int SCREEN_HEIGHT = D3DDevice.Instance.Device.Viewport.Height;
            public static TGCVector2 LIFE_SHARK_SCALE = new TGCVector2(0.4f, 0.5f);
            public static TGCVector2 LIFE_SHARK_SIZE = new TGCVector2(1000 * LIFE_SHARK_SCALE.X, 100 * LIFE_SHARK_SCALE.Y);
            public static TGCVector2 LIFE_SHARK_POSITION = new TGCVector2(SCREEN_WIDTH - LIFE_SHARK_SIZE.X - 20, 0);
            public static TGCVector2 LIFE_SHARK_TEXT_SIZE = new TGCVector2(130, 100);
            public static TGCVector2 LIFE_SHARK_TEXT_POSITION = new TGCVector2(LIFE_SHARK_POSITION.X - LIFE_SHARK_TEXT_SIZE.X, 20f);
            public static TGCVector2 POINTER_SCALE = new TGCVector2(1, 1);
            public static TGCVector2 POINTER_SIZE = new TGCVector2(64 * POINTER_SCALE.X, 64 * POINTER_SCALE.Y);
            public static TGCVector2 POINTER_POSITION = new TGCVector2((SCREEN_WIDTH - POINTER_SIZE.X) / 2, (SCREEN_HEIGHT - POINTER_SIZE.Y) / 2);
            public static TGCVector2 MOUSE_POINTER_SCALE = new TGCVector2(1, 1);
            public static TGCVector2 MOUSE_POINTER_SIZE = new TGCVector2(32 * POINTER_SCALE.X, 32 * POINTER_SCALE.Y);
            public static TGCVector2 MOUSE_POINTER_POSITION = new TGCVector2((SCREEN_WIDTH - MOUSE_POINTER_SIZE.X) / 2, (SCREEN_HEIGHT - MOUSE_POINTER_SIZE.Y) / 2);
            public static string HELP_TEXT = "FOR HELP, PRESS F1 KEY";
            public static string SHIP_EXIT_TEXT = "PRESS E TO EXIT";
            public static string SHIP_ENTER_TEXT = "PRESS E TO ENTER";
            public static string COLLECT_TEXT = "PRESS E TO PICK UP";
            public static TGCVector2 COMMON_TEXT_SIZE = new TGCVector2(300, 50);
            public static TGCVector2 HELP_TEXT_POSITION = new TGCVector2(SCREEN_WIDTH - COMMON_TEXT_SIZE.X + 30, SCREEN_HEIGHT - COMMON_TEXT_SIZE.Y + 20);
            public static TGCVector2 INSTRUCTION_TEXT_SIZE = new TGCVector2(860, 450);
            public static TGCVector2 INSTRUCTION_TEXT_POSITION = new TGCVector2((SCREEN_WIDTH - INSTRUCTION_TEXT_SIZE.X) / 2, (SCREEN_HEIGHT - INSTRUCTION_TEXT_SIZE.Y) / 2 + 20);
            public static string INSTRUCTION_TEXT = "Movement: W(↑) | A(←) | S(↓) | D(→) " +
                                                    "\nInstructions for leaving and entering the ship: " +
                                                    "\n\t- To exit the ship look towards the hatch and press the E key." +
                                                    "\n\t- To enter the ship, come closer and press the E key." +
                                                    "\nCollect and attack: " +
                                                    "\n\t- To collect the objects, press E key on them." +
                                                    "\n\t- To attack the shark, left click when you have the weapon." +
                                                    "\n\t- Once the weapon is crafted, you can equip it opening the inventory." +
                                                    "\nInventory: " +
                                                    "\n\t- To open and close, press I key." +
                                                    "\nCrafting inside the ship: " +
                                                    "\nTo open and close help, press F1 key.";
            public static TGCVector2 PRESS_TEXT_POSITION = new TGCVector2((SCREEN_WIDTH - COMMON_TEXT_SIZE.X + 145) / 2, (SCREEN_HEIGHT - COMMON_TEXT_SIZE.Y - 30) / 2);
            public static TGCVector2 COLLECT_TEXT_SIZE = new TGCVector2(520, 50);
            public static TGCVector2 COLLECT_TEXT_POSITION = new TGCVector2(SCREEN_WIDTH - COLLECT_TEXT_SIZE.X, SCREEN_HEIGHT - COLLECT_TEXT_SIZE.Y - 100);
            public static TGCVector2 SHIP_INDICATOR_SCALE = new TGCVector2(1, 1);
            public static TGCVector2 SHIP_INDICATOR_POSITION = new TGCVector2((SCREEN_WIDTH - 128) / 2, 20);
            public static TGCVector2 SHIP_INDICATOR_TEXT_POSITION = new TGCVector2(SHIP_INDICATOR_POSITION.X + 23, SHIP_INDICATOR_POSITION.Y + 84);
        }

        public float ScreenWitdh => Constants.SCREEN_WIDTH;
        public float ScreenHeight => Constants.SCREEN_HEIGHT;

        private readonly string MediaDir;
        private readonly DrawSprite MousePointer;
        private readonly DrawSprite Pointer;

        private readonly DrawText InstructionText;
        private readonly DrawText HelpText;
        private readonly DrawText ShipText;
        private readonly DrawText CollectText;
        private readonly DrawText ItemsHistoryText;
        private readonly DrawSprite ShipLocationIndicator;
        private readonly DrawText DistanceShipLocation;

        public Inventory2D Inventory { get; set; }
        public Crafting2D Crafting { get; set; }
        public Character2D Character { get; set; }
        public Shark2D Shark { get; set; }
        public float ItemHistoryTime => Constants.TIME_HISTORY_TEXT;
        public float DistanceWithShip { get; set; }
        public bool ActiveInventory { get; set; }
        public bool ActiveWeapon { get; set; }
        public bool CanCraft { get; set; }
        public bool ShowHelp { get; set; }
        public bool ShowInfoExitShip { get; set; }
        public bool ShowInfoEnterShip { get; set; }
        public bool NearObjectForSelect { get; set; }
        public bool ShowInfoItemCollect { get; set; }
        public bool ShowIndicatorShip { get; set; }
        public bool ShowSharkLife { get; set; }
        public bool GodMode { get; set; }

        public List<string> ItemHistory { get; set; }

        public Game2DManager(string mediaDir, CharacterStatus character, SharkStatus shark, TgcD3dInput input)
        {
            MediaDir = mediaDir;
            Character = new Character2D(mediaDir, character);
            Shark = new Shark2D(mediaDir, shark);

            Pointer = new DrawSprite(MediaDir);
            MousePointer = new DrawSprite(MediaDir);
            Inventory = new Inventory2D(MediaDir);
            Crafting = new Crafting2D(MediaDir, input);
            InstructionText = new DrawText();
            HelpText = new DrawText();
            ShipText = new DrawText();
            CollectText = new DrawText();
            ItemsHistoryText = new DrawText();
            ShipLocationIndicator = new DrawSprite(MediaDir);
            DistanceShipLocation = new DrawText();
            Init();
        }

        public Game2DManager(string mediaDir)
        {
            MousePointer = new DrawSprite(mediaDir);
            InstructionText = new DrawText();
            InitializerMousePointer();
            InitializerInstructionText();
        }

        public void Dispose()
        {
            InstructionText.Dispose();
            HelpText.Dispose();
            Character.Dispose();
            Shark.Dispose();
            MousePointer.Dispose();
            Pointer.Dispose();
            ShipLocationIndicator.Dispose();
            DistanceShipLocation.Dispose();
        }

        public void DisposePointerAndInstruction()
        {
            InstructionText.Dispose();
            MousePointer.Dispose();
        }

        private void Init()
        {
            InitializerPointer();
            InitializerMousePointer();
            InitializerInstructionText();
            InitializerSimpleText();
            InitializerIndicatorShip();
        }

        private void InitializerIndicatorShip()
        {
            ShipLocationIndicator.SetImage("fund_ship.png");
            ShipLocationIndicator.SetInitialScallingAndPosition(Constants.SHIP_INDICATOR_SCALE, Constants.SHIP_INDICATOR_POSITION);
            DistanceShipLocation.SetTextAndPosition(text: DistanceWithShip.ToString(), position: Constants.SHIP_INDICATOR_TEXT_POSITION);
        }

        private void InitializerPointer()
        {
            Pointer.SetImage("Pointer.png");
            Pointer.SetInitialScallingAndPosition(Constants.POINTER_SCALE, Constants.POINTER_POSITION);
        }

        private void InitializerMousePointer()
        {
            MousePointer.SetImage("MousePointer.png");
            MousePointer.SetInitialScallingAndPosition(Constants.MOUSE_POINTER_SCALE, Constants.MOUSE_POINTER_POSITION);
        }

        private void InitializerInstructionText()
        {
            InstructionText.SetTextSizeAndPosition(text: Constants.INSTRUCTION_TEXT, Constants.INSTRUCTION_TEXT_SIZE, Constants.INSTRUCTION_TEXT_POSITION);
            InstructionText.Color = Color.Red;
        }

        private void InitializerSimpleText()
        {
            HelpText.SetTextSizeAndPosition(text: Constants.HELP_TEXT, Constants.COMMON_TEXT_SIZE, Constants.HELP_TEXT_POSITION);
            HelpText.Color = Color.Red;
            ShipText.SetTextSizeAndPosition(text: Constants.SHIP_EXIT_TEXT, Constants.COMMON_TEXT_SIZE, Constants.PRESS_TEXT_POSITION);
            CollectText.SetTextSizeAndPosition(text: Constants.COLLECT_TEXT, Constants.COMMON_TEXT_SIZE, Constants.PRESS_TEXT_POSITION);
            ItemsHistoryText.Size = Constants.COLLECT_TEXT_SIZE;
            ItemsHistoryText.Color = Color.Blue;
        }

        public void Render()
        {
            if (ShowHelp)
            {
                InstructionText.Render();
            }
            else
            {
                HelpText.Render();
            }

            if (!ActiveInventory)
            {
                if (ShowSharkLife)
                {
                    Shark.Render();
                }

                Character.Render();
                Pointer.Render();

                if (ShowInfoExitShip)
                {
                    ShipText.Text = Constants.SHIP_EXIT_TEXT;
                    ShipText.Render();
                }
                if (ShowInfoEnterShip)
                {
                    ShipText.Text = Constants.SHIP_ENTER_TEXT;
                    ShipText.Render();
                }
                if (NearObjectForSelect)
                {
                    CollectText.Render();
                }

                if (ShowInfoItemCollect)
                {
                    var index = 0;
                    ItemHistory.ForEach(item =>
                    {
                        index++;
                        ItemsHistoryText.Text = "COLLECTED " + item + " + 1";
                        ItemsHistoryText.Position = new TGCVector2(Constants.COLLECT_TEXT_POSITION.X, Constants.COLLECT_TEXT_POSITION.Y + index * 20);
                        ItemsHistoryText.Render();
                    });
                }

                if (ShowIndicatorShip)
                {
                    ShipLocationIndicator.Render();
                    DistanceShipLocation.Text = DistanceWithShip.ToString();
                    DistanceShipLocation.Render();
                }
            }
            else
            {
                if (CanCraft)
                {
                    Crafting.Render();
                }
                else
                {
                    Inventory.Render();
                    if (ActiveWeapon)
                    {
                        Crafting.RenderItemWeapon();
                    }
                }
                RenderMousePointer();
            }
        }

        public void RenderMousePointer()
        {
            if (ShowHelp)
            {
                InstructionText.Render();
            }

            MousePointer.Position = new TGCVector2(Cursor.Position.X - 16, Cursor.Position.Y - 16);
            MousePointer.Render();
        }

        public void Update()
        {
            Shark.Update();
            if (!GodMode)
            {
                Character.Update();
            }
            else
            {
                Character.UpdateForGodMode();
            }
        }

        public void UpdateItemWeapon()
        {
            if (ActiveWeapon)
            {
                Crafting.UpdateItemWeapon();
            }
        }

        public void UpdateItems(Dictionary<string, List<string>> items)
        {
            Crafting.UpdateItems(items);
            Inventory.UpdateItems(items);
        }

        public void Reset()
        {
            ActiveInventory =
            ActiveWeapon =
            CanCraft =
            ShowHelp =
            ShowInfoExitShip =
            ShowInfoEnterShip =
            NearObjectForSelect =
            ShowInfoItemCollect =
            ShowIndicatorShip =
            ShowSharkLife = false;

        }
    }
}