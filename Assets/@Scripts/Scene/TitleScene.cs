public class TitleScene : BaseScene
{
    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
        Managers.Table.Load();

        CPrint.Log(Managers.Table.Enemy.GetMonsterInfo(1).Name);
        
        Managers.Scene.LoadSceneWithLoading(SceneType.GameScene);
    }

    protected override void OnUpdate()
    {
    }
}
