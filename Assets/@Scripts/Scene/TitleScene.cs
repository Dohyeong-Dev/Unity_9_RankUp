public class TitleScene : BaseScene
{
    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
        Managers.Scene.LoadSceneWithLoading(SceneType.GameScene);
    }

    protected override void OnUpdate()
    {
    }
}
