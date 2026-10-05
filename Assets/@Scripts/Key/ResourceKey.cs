public abstract class ResourceKey
{
    public static class Path
    {
        public const string Prefab = "Prefab/";
        public const string Table = "Table/";
        public const string Sprite = "Sprite/";
            
        public const string Misc = Prefab + "Misc/";
        public const string Item = Prefab + "Item/";
        public const string UI = Prefab + "UI/";
        
        public const string ScreenUI = UI + "ScreenUI/";
        public const string PopupUI = UI + "PopupUI/";
        public const string OverlayUI = UI + "OverlayUI/";
        public const string SlotUI = UI + "SlotUI/";
        
        public const string ItemSprite = Sprite + "UI/Item/";
        
        #region ===== 사운드 =====
        
        public const string Sound = "Sound/";
        public const string SoundMixer = Sound + "SoundMixer";

        public const string Bgm = Sound + "Bgm/";
        public const string Sfx = Sound + "Sfx/";
        
        #endregion ===== 사운드 =====
    }

    public static class Name
    {
        public const string Event = "EventSystem";
        
        #region ===== 사운드 =====
        
        public enum BgmType
        {
            GlobalResonance,
            Boss
        }

        public enum SfxType
        {
            FootStep,
            KatanaSwing01,
            KatanaSwing02,
            KatanaSwing03,
            KatanaSheathe,
            PlayerDash,
            ElectronicShield,
            PlayerHit,
            PlayerDieVoice,
            PlayerIdle2,
            EnemyDissolve,
            EnemyHit,
            EnemyAppearVoice,
            ChimePopup,
            ChimeConfirm,
            ChimeCancel,
            FireProjectile,
            IceProjectile,
            SparkProjectile,
            GameOver,
            GameClear,
            PickUpItem,
            Button
        }
        
        #endregion ===== 사운드 =====
    }
}