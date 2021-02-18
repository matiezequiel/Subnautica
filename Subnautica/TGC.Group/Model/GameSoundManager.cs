using TGC.Core.Sound;

namespace TGC.Group.Model
{
    class GameSoundManager
    {
        public TgcMp3Player Menu { get; private set; }
        public TgcMp3Player Ambient { get; private set; }
        public TgcStaticSound SharkStalking { get; private set; }
        public TgcStaticSound Crafting { get; private set; }
        public TgcStaticSound SharkDead { get; private set; }
        public TgcStaticSound SharkAppear { get; private set; }
        public TgcStaticSound SharkAttack { get; private set; }
        public TgcStaticSound Collect { get; private set; }
        public TgcStaticSound EquipWeapon { get; private set; }
        public TgcStaticSound WeaponHit { get; private set; }
        public TgcStaticSound HitToShark { get; private set; }
        public TgcStaticSound ToSurface { get; private set; }
        public TgcStaticSound Submerge { get; private set; }

        private string AmbientFileName;
        private string UnderWaterFileName;
        private bool JustSubmerge;

        public GameSoundManager(string mediaDir, TgcDirectSound sound)
        {
            Menu = new TgcMp3Player();
            Ambient = new TgcMp3Player();
            SharkStalking = new TgcStaticSound();
            Crafting = new TgcStaticSound();
            SharkDead = new TgcStaticSound();
            SharkAppear = new TgcStaticSound();
            SharkAttack = new TgcStaticSound();
            Collect = new TgcStaticSound();
            EquipWeapon = new TgcStaticSound();
            WeaponHit = new TgcStaticSound();
            HitToShark = new TgcStaticSound();
            ToSurface = new TgcStaticSound();
            Submerge = new TgcStaticSound();
            Init(mediaDir, sound);
        }

        private void Init(string mediaDir, TgcDirectSound sound)
        {
            Menu.FileName = mediaDir + @"\Sounds\Menu.mp3";
            AmbientFileName = mediaDir + @"\Sounds\Ambient.mp3";
            UnderWaterFileName = mediaDir + @"\Sounds\UnderWater.mp3";
            SharkStalking.loadSound(mediaDir + @"\Sounds\SharkNear.wav", sound.DsDevice);
            Crafting.loadSound(mediaDir + @"\Sounds\Crafting.wav", sound.DsDevice);
            SharkDead.loadSound(mediaDir + @"\Sounds\SharkDead.wav", sound.DsDevice);
            SharkAppear.loadSound(mediaDir + @"\Sounds\SharkAppear.wav", sound.DsDevice);
            SharkAttack.loadSound(mediaDir + @"\Sounds\SharkAttack.wav", sound.DsDevice);
            Collect.loadSound(mediaDir + @"\Sounds\gather_resource.wav", sound.DsDevice);
            EquipWeapon.loadSound(mediaDir + @"\Sounds\WeaponEquip.wav", sound.DsDevice);
            WeaponHit.loadSound(mediaDir + @"\Sounds\WeaponHit.wav", sound.DsDevice);
            HitToShark.loadSound(mediaDir + @"\Sounds\HitToShark.wav", sound.DsDevice);
            ToSurface.loadSound(mediaDir + @"\Sounds\ToSurface.wav", sound.DsDevice);
            Submerge.loadSound(mediaDir + @"\Sounds\Submerge.wav", sound.DsDevice);
        }

        public void Dispose()
        {
            SharkStalking.dispose();
            Crafting.dispose();
            SharkDead.dispose();
            SharkAppear.dispose();
            SharkAttack.dispose();
            Collect.dispose();
            EquipWeapon.dispose();
            WeaponHit.dispose();
            HitToShark.dispose();
            ToSurface.dispose();
            Submerge.dispose();
            Dispose(Menu);
            Dispose(Ambient);
        }

        public void PlayMusicAmbient(bool submerge)
        {
            if (submerge)
            {
                if (JustSubmerge)
                {
                    JustSubmerge = false;
                    Ambient.stop();
                    Dispose(Ambient);
                    Ambient.FileName = UnderWaterFileName;
                    Ambient.play(true);
                    Submerge.play();
                }
            }
            else
            {
                if (!JustSubmerge)
                {
                    JustSubmerge = true;
                    Ambient.stop();
                    Dispose(Ambient);
                    Ambient.FileName = AmbientFileName;
                    Ambient.play(true);
                    ToSurface.play();
                }
            }
        }

        public void Dispose(TgcMp3Player music)
        {
            if (music.FileName != null)
            {
                music.closeFile();
            }
        }
    }
}
