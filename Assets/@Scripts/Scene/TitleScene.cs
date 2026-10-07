public class TitleScene : BaseScene
{
    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
        Managers.Table.Load();
        
        Managers.Data.Load();
        
        Managers.Scene.LoadSceneWithLoading(SceneType.GameScene);
    }

    protected override void OnUpdate()
    {
    }
}
