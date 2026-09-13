public abstract class ResourceKey
{
    public static class Path
    {
        public const string Misc = "Prefab/Misc/";

        #region ===== UI =====
        
        public const string UI = "Prefab/UI/";
        public const string ScreenUI = UI + "ScreenUI/";
        public const string PopupUI = UI + "PopupUI/";
        public const string OverlayUI = UI + "OverlayUI/";
        
        #endregion ===== UI =====
        
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

        #region ===== UI =====
        
        public const string LoaindgUI = "LoadingUI";
        
        #endregion ===== UI =====
        
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
            ChimeAlert,
            ChimePopup,
            ChimeConfirm,
            ChimeCancel,
            FireProjectile,
            IceProjectile,
            SparkProjectile,
            GameOver,
            GameClear
        }
        
        #endregion ===== 사운드 =====
    }
}