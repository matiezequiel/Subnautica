using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Group.Model.Status;
using TGC.Group.Utils;
using Font = System.Drawing.Font;

namespace TGC.Group.Model
{
    public class GameModel : TGCExample
    {
        public struct Perimeter
        {
            public float xMin, xMax, zMin, zMax;
            public Perimeter(float xMin, float xMax, float zMin, float zMax)
            {
                this.xMin = xMin;
                this.xMax = xMax;
                this.zMin = zMin;
                this.zMax = zMax;
            }
        }

        private CameraFPS camera;
        private FullQuad FullQuad;

        private GameState StateGame;
        private GameState StateMenu;
        private GameState StateHelp;
        private GameState StateExit;
        private GameState StateGodMode;
        private GameState CurrentState;

        private DrawSprite Title;
        private DrawButton Play;
        private DrawButton Help;
        private DrawButton Exit;
        private DrawButton ModeGod;

        private DrawText CraftingStatus;

        private GameObjectManager ObjectManager;
        private GameInventoryManager InventoryManager;
        private GameEventsManager EventsManager;
        private Game2DManager Draw2DManager;
        private Game2DManager PointerAndInstruction;
        private CharacterStatus CharacterStatus;
        private SharkStatus SharkStatus;
        private GameSoundManager SoundManager;

        private bool ActiveInventory { get; set; }
        private bool ExitGame { get; set; }
        private bool CanCraftObjects => ObjectManager.Character.IsInsideShip;
        private bool RenderCraftingStatus { get; set; }
        private bool GodMode { get; set; }

        public float TimeToRevive { get; set; }
        public float TimeToAlarm { get; set; }
        public float ItemHistoryTime { get; set; }
        private float TimeToRenderCraftingStatus = 1;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir) => FixedTickEnable = true;

        public override void Dispose()
        {
            FullQuad.Dispose();
            ObjectManager.Dispose();
            Draw2DManager.Dispose();
            Title.Dispose();
            SoundManager.Dispose();
            CraftingStatus.Dispose();
        }

        public override void Update() => CurrentState.Update();
        public override void Render()
        {
            CurrentState.Render();
            if (ExitGame)
            {
                Application.Exit();
            }
        }

        public override void Init()
        {
            Camera = camera = new CameraFPS(Input);
            FullQuad = new FullQuad(MediaDir, ShadersDir, ElapsedTime);
            PointerAndInstruction = new Game2DManager(MediaDir);
            SoundManager = new GameSoundManager(MediaDir, DirectSound);
            InitializerState();

            InitializerMenu();
            InitializerGame();
        }

        private void InitializerMenu()
        {
            Title = new DrawSprite(MediaDir);
            Title.SetImage("subnautica.png");
            Title.SetInitialScallingAndPosition(new TGCVector2(0.8f, 0.8f), new TGCVector2(50, 50));

            Play = new DrawButton(MediaDir, Input);
            Play.InitializerButton(text: "Play", scale: new TGCVector2(0.4f, 0.4f), position: new TGCVector2(50, 500),
                           action: () => CurrentState = StateGame);
            Help = new DrawButton(MediaDir, Input);
            Help.InitializerButton(text: "Help", scale: new TGCVector2(0.4f, 0.4f), position: new TGCVector2(50, 580),
                           action: () => CurrentState = StateHelp);
            Exit = new DrawButton(MediaDir, Input);
            Exit.InitializerButton(text: "Exit", scale: new TGCVector2(0.4f, 0.4f),
                                   position: new TGCVector2(PointerAndInstruction.ScreenWitdh - Help.Size.X - 50, PointerAndInstruction.ScreenHeight - Help.Size.Y - 50),
                                   action: () => CurrentState = StateExit);
            ModeGod = new DrawButton(MediaDir, Input);
            ModeGod.InitializerButton(text: "God Mode", scale: new TGCVector2(0.4f, 0.4f),
                                      position: new TGCVector2(50, 660),
                                      action: () => CurrentState = StateGodMode);
            ModeGod.ButtonText.Position = new TGCVector2(ModeGod.ButtonText.Position.X - 30, ModeGod.ButtonText.Position.Y);

            camera.Position = new TGCVector3(1030, 3900, 2500);
            camera.Lock = true;
        }

        private void InitializerGame()
        {
            ObjectManager = new GameObjectManager(MediaDir, ShadersDir, camera, Input, SoundManager);
            CharacterStatus = new CharacterStatus(ObjectManager.Character);
            SharkStatus = new SharkStatus();
            EventsManager = new GameEventsManager(ObjectManager.Shark, ObjectManager.Character, SoundManager);
            Draw2DManager = new Game2DManager(MediaDir, CharacterStatus, SharkStatus, Input);
            InventoryManager = new GameInventoryManager();
            SoundManager.Menu.play(true);
            Draw2DManager.Crafting.CraftingItems[0].button.Action = CraftSkillFish;
            Draw2DManager.Crafting.CraftingItems[1].button.Action = CraftWeapon;
            Draw2DManager.Crafting.CraftingItems[2].button.Action = CraftDivingHelmet;
            CraftingStatus = new DrawText();
            CraftingStatus.SetTextSizeAndPosition("Insufficient materials", size: new TGCVector2(400, 100),
                           position: new TGCVector2(Draw2DManager.ScreenWitdh / 2, Draw2DManager.ScreenHeight / 2));
            CraftingStatus.Font = new Font("Arial Black", 15, FontStyle.Bold);
            CraftingStatus.Color = Color.Crimson;
        }

        private void InitializerState()
        {
            StateGame = new GameState()
            {
                Update = UpdateGame,
                Render = RenderGame
            };

            StateMenu = new GameState()
            {
                Update = UpdateMenu,
                Render = RenderMenu
            };

            StateHelp = new GameState()
            {
                Update = UpdateInstructionHelp,
                Render = RenderMenu
            };

            StateExit = new GameState()
            {
                Update = UpdateExit,
                Render = RenderMenu
            };

            StateGodMode = new GameState()
            {
                Update = UpdateGame,
                Render = RenderGame
            };

            CurrentState = StateMenu;
        }

        #region Help
        private void UpdateInstructionHelp()
        {
            Play.Invisible = true;
            Help.Invisible = true;
            ModeGod.Invisible = true;
            PointerAndInstruction.ShowHelp = true;
            UpdateMenu();
        }

        private void UpdateExit()
        {
            Play.Invisible = false;
            Help.Invisible = false;
            ModeGod.Invisible = false;
            PointerAndInstruction.ShowHelp = false;
            CurrentState = StateMenu;
            UpdateMenu();
        }
        #endregion

        #region Menu
        private void RenderMenu()
        {
            PreRender();
            if (ObjectManager.ShowScene)
            {
                ObjectManager.Skybox.Render();
                ObjectManager.Ship.OutdoorMesh.Render();
                ObjectManager.Water.Render();
            }
            Title.Render();
            Play.Render();
            Help.Render();
            Exit.Render();
            ModeGod.Render();

            PointerAndInstruction.RenderMousePointer();
            PostRender();
        }

        private void UpdateMenu()
        {
            Play.Update();
            Help.Update();
            ModeGod.Update();
            if (CurrentState == StateMenu)
            {
                Exit.Update();
                if (CurrentState == StateExit)
                {
                    ExitGame = true;
                }
            }
            else
            {
                Exit.Update();
            }

            if (ObjectManager.ShowScene)
            {
                ObjectManager.Skybox.Update();
                ObjectManager.Water.Update(ElapsedTime, Camera.Position);
            }

            if (CurrentState == StateGodMode)
            {
                GodMode = true;
                InventoryManager.Cheat();
                Draw2DManager.UpdateItems(InventoryManager.Items);
            }

            if (CurrentState == StateGame || CurrentState == StateGodMode)
            {
                Play.Dispose();
                Help.Dispose();
                Exit.Dispose();
                ModeGod.Dispose();
                camera.Lock = false;
                SoundManager.Dispose(SoundManager.Menu);
            }
        }
        #endregion

        #region Game
        private void RenderGame()
        {
            FullQuad.PreRenderMeshes();
            ObjectManager.Render(Frustum);
            FullQuad.Render();
            if (RenderCraftingStatus)
            {
                CraftingStatus.Render();
            }

            Draw2DManager.Render();
            PostRender();
        }

        private void UpdateGame()
        {
            if (Input.keyPressed(Key.F2))
            {
                GodMode = !GodMode;
                if (GodMode)
                {
                    InventoryManager.Cheat();
                    Draw2DManager.UpdateItems(InventoryManager.Items);
                }
            }
            Draw2DManager.GodMode = GodMode;
            if (Input.keyPressed(Key.F1))
            {
                Draw2DManager.ShowHelp = !Draw2DManager.ShowHelp;
            }

            ObjectManager.CreateBulletCallbacks(CharacterStatus);
            if (CharacterStatus.IsDead && !GodMode && !ActiveInventory)
            {
                TimeToRevive += ElapsedTime;
                if (TimeToRevive < 5)
                {
                    FullQuad.SetTime(ElapsedTime);
                    FullQuad.RenderTeleportEffect = true;
                    Draw2DManager.Reset();
                    InventoryManager.Reset();
                }
                else
                {
                    CharacterStatus.Respawn();
                    FullQuad.RenderTeleportEffect = FullQuad.RenderAlarmEffect = false;
                }
                return;
            }
            TimeToRevive = 0;
            if (Input.keyPressed(Key.I))
            {
                Draw2DManager.ActiveInventory = camera.Lock =
                    FullQuad.RenderPDA = ActiveInventory = !ActiveInventory;
            }

            if (!ActiveInventory)
            {
                UpdateEvents();
            }
            else
            {
                Draw2DManager.UpdateItemWeapon();
            }

            CharacterStatus.Update(ElapsedTime, GodMode);
            ObjectManager.Character.RestartBodySpeed();
            if (Input.keyPressed(Key.E))
            {
                ObjectManager.Character.Teleport();
            }

            UpdateFlags();
            UpdateInfoItemCollect();

            if (RenderCraftingStatus)
            {
                TimeToRenderCraftingStatus -= ElapsedTime;
                if (TimeToRenderCraftingStatus < 0)
                {
                    RenderCraftingStatus = false;
                    TimeToRenderCraftingStatus = 1;
                }
            }
        }

        private void UpdateEvents()
        {
            ObjectManager.Update(ElapsedTime, TimeBetweenUpdates);
            EventsManager.Update(ElapsedTime, ObjectManager.Fishes, SharkStatus);
            InventoryManager.AddItem(ObjectManager.ItemSelected);
            Draw2DManager.ItemHistory = InventoryManager.ItemHistory;
            ObjectManager.ItemSelected = null;
            FullQuad.RenderAlarmEffect = CharacterStatus.ActiveRenderAlarm;
            Draw2DManager.DistanceWithShip = FastUtils.DistanceBetweenVectors(camera.Position, ObjectManager.Ship.PositionShip);
            Draw2DManager.ShowIndicatorShip = Draw2DManager.DistanceWithShip > 15000 && !ObjectManager.Character.IsInsideShip;
            Draw2DManager.ShowSharkLife = EventsManager.SharkIsAttacking && !SharkStatus.IsDead;
            if (CharacterStatus.ActiveAlarmForDamageReceived)
            {
                TimeToAlarm += ElapsedTime;
                if (TimeToAlarm > 2)
                {
                    FullQuad.RenderAlarmEffect = CharacterStatus.ActiveAlarmForDamageReceived = false;
                    TimeToAlarm = 0;
                }
            }
            SharkStatus.DamageReceived = ObjectManager.Character.AttackedShark;
            SharkStatus.Update();
            ObjectManager.Shark.DeathMove = SharkStatus.IsDead;
            ObjectManager.Character.AttackedShark = SharkStatus.DamageReceived;
            Draw2DManager.Update();
            Draw2DManager.UpdateItems(InventoryManager.Items);
        }

        private void UpdateInfoItemCollect()
        {
            if (!Draw2DManager.ShowInfoItemCollect)
            {
                return;
            }

            ItemHistoryTime += ElapsedTime;
            if (ItemHistoryTime > Draw2DManager.ItemHistoryTime)
            {
                Draw2DManager.ShowInfoItemCollect = false;
                InventoryManager.ItemHistory.RemoveRange(0, InventoryManager.ItemHistory.Count);
                ItemHistoryTime = 0;
            }
        }

        private void UpdateFlags()
        {
            Draw2DManager.CanCraft = CanCraftObjects;
            if (CanCraftObjects && Draw2DManager.ActiveInventory)
            {
                Draw2DManager.Crafting.UpdateItemsCrafting();
            }

            Draw2DManager.ShowInfoExitShip = ObjectManager.Character.LooksAtTheHatch;
            Draw2DManager.ShowInfoEnterShip = ObjectManager.Character.NearShip;
            Draw2DManager.NearObjectForSelect = ObjectManager.NearObjectForSelect;
            Draw2DManager.ShowInfoItemCollect = ObjectManager.ShowInfoItemCollect;
        }

        private void CraftWeapon()
        {
            ObjectManager.Character.HasWeapon = GameCraftingManager.CanCraftWeapon(InventoryManager.Items);
            if (!ObjectManager.Character.HasWeapon)
            {
                RenderCraftingStatus = true;
            }
            else
            {
                SoundManager.Crafting.play();
                var textPosition = Draw2DManager.Crafting.CraftingItems[1].button.ButtonText.Position;
                Draw2DManager.Crafting.CraftingItems[1].button.ButtonText.Position = new TGCVector2(textPosition.X - 15, textPosition.Y);
                Draw2DManager.Crafting.CraftingItems[1].button.ButtonText.Text = " Equip";
                Draw2DManager.Crafting.CraftingItems[1].button.Action = Draw2DManager.Crafting.Weapon.button.Action = UseWeapon;
                Draw2DManager.ActiveWeapon = true;
            }
            Draw2DManager.Crafting.UpdateItems(InventoryManager.Items);
        }

        private void CraftDivingHelmet()
        {
            CharacterStatus.HasDivingHelmet = ObjectManager.Character.HasDivingHelmet = GameCraftingManager.CanCraftDivingHelmet(InventoryManager.Items);
            if (CharacterStatus.HasDivingHelmet)
            {
                SoundManager.Crafting.play();
                CharacterStatus.UpdateOxygenMax();
            }
            else
            {
                RenderCraftingStatus = true;
            }

            Draw2DManager.Crafting.UpdateItems(InventoryManager.Items);
        }

        private void CraftSkillFish()
        {
            Draw2DManager.Crafting.Learned = ObjectManager.Character.CanFish = GameCraftingManager.CanCatchFish(InventoryManager.Items);
            if (!ObjectManager.Character.CanFish)
            {
                RenderCraftingStatus = true;
            }
            else
            {
                SoundManager.Crafting.play();
                Draw2DManager.Crafting.CraftingItems[0].button.ButtonText.Text = "Learned";
                Draw2DManager.Crafting.CraftingItems[0].button.ButtonText.Color = Color.Orange;
                var position = Draw2DManager.Crafting.CraftingItems[0].button.ButtonText.Position;
                Draw2DManager.Crafting.CraftingItems[0].button.ButtonText.Position = new TGCVector2(position.X - 15, position.Y);
            }
            Draw2DManager.Crafting.UpdateItems(InventoryManager.Items);
        }

        private void UseWeapon()
        {
            string text;
            if (ObjectManager.Character.InHand)
            {
                text = " Equip";
            }
            else
            {
                text = "Unequip";
            }

            ObjectManager.Character.InHand = !ObjectManager.Character.InHand;
            Draw2DManager.Crafting.CraftingItems[1].button.ButtonText.Text = text;
            Draw2DManager.Crafting.Weapon.button.ButtonText.Text = text;
            if (ObjectManager.Character.InHand)
            {
                SoundManager.EquipWeapon.play();
            }

            ObjectManager.Character.Render();
        }
        #endregion
    }
}