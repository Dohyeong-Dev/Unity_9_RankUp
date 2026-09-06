/// <summary> 게임 씬 진입시 오픈하는 스타트 팝업으로 스페이스 누를 시 진입  </summary>
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
    }
}