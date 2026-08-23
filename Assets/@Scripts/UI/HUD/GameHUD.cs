using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : BaseHUD
{
    private enum Sliders
    {
    }
    
    private enum Texts
    {
    }

    
    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Managers.Input.SetCursorLock(true);
    }

    protected override void OnUpdate()
    {
    }
}
