public class StartPopup : BasePopup
{
    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Space)
        {
            Close();
        }
    }

    protected override void DestroyOverride()
    {
        if (Managers.Scene.CurrentScene?.Type == SceneType.GameScene)
        {
            GameScene gameScene = Managers.Scene.CurrentScene as GameScene;
            if (gameScene != null)
            {
                gameScene.SetStart(true);
            }
        }
    }
}