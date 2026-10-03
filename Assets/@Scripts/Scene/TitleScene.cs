public class TitleScene : BaseScene
{
    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
        Managers.Table.Load();
        
        Managers.Data.Load();
        CPrint.Log($"[현재골드] : {Managers.Data.Gold}");
        
        Managers.Scene.LoadSceneWithLoading(SceneType.GameScene);
    }

    protected override void OnUpdate()
    {
    }
}
