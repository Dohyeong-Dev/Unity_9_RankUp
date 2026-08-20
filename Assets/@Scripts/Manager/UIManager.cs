using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    #region SortingOrder

    private const int HudSortingOrder = 0;
    private const int ScreenSortingOrder = 1;
    private int _nextPopupSortingOrder = 2; // HUD(0), Screen(1) 이후부터 시작
    private const int LoadingSortingOrder = 999;

    #endregion

    #region CurrentSceneUI

    public BaseHUD CurrentHUD { get; private set; }
    public BaseScreen CurrentScreen { get; private set; }
    public BasePopup CurrentPopup => _popupStack.Count > 0 ? _popupStack.Peek() : null;

    #endregion


    #region PopupUI

    private readonly Stack<BasePopup> _popupStack = new();
    public int PopupCount => _popupStack.Count;

    #endregion

    #region LoadingUI

    private GameObject _loadingObject;
    public bool IsLoading => _loadingObject != null;

    #endregion

    #region Root
    
    private GameObject _root;

    public GameObject Root
    {
        get
        {
            if (_root == null)
            {
                _root = new GameObject("UI_Root");
            }

            return _root;
        }
    }
    
    private GameObject _persistentRoot;
    private GameObject PersistentRoot
    {
        get
        {
            if (_persistentRoot == null)
            {
                _persistentRoot = new GameObject("UI_PersistentRoot");
                Object.DontDestroyOnLoad(_persistentRoot);
            }

            return _persistentRoot;
        }
    }
    
    #endregion

    public void OnUpdate()
    {
        if (IsLoading)
        {
            return;
        }

        if (CurrentPopup != null)
        {
            CurrentHUD?.SetInputEnabled(false);
            CurrentPopup.OnInputKey();
        }
        else if (CurrentScreen != null)
        {
            CurrentHUD?.SetInputEnabled(false);
            CurrentScreen.OnInputKey();
        }
        else if (CurrentHUD != null)
        {
            CurrentHUD.SetInputEnabled(true);
            CurrentHUD.OnInputKey();
        }
    }

    // UI Canvas 초기 설정
    public void SetupCanvas(GameObject uiObject, BaseUI baseUI)
    {
        Canvas canvas = uiObject.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        switch (baseUI)
        {
            case BaseHUD hud:
                CurrentHUD = hud;
                canvas.sortingOrder = HudSortingOrder;
                break;

            case BaseScreen:
                canvas.sortingOrder = ScreenSortingOrder;
                break;

            case BasePopup:
                canvas.sortingOrder = _nextPopupSortingOrder++;
                break;

            case LoadingUI:
                canvas.sortingOrder = LoadingSortingOrder;
                break;
        }
    }

    // Screen UI 열기
    public T OpenScreen<T>() where T : BaseScreen
    {
        if (CurrentScreen is T)
        {
            CPrint.Error($"이미 동일한 Screen UI가 열려있습니다. [{typeof(T).Name}]");
            return null;
        }

        if (CurrentScreen != null)
        {
            CloseAll();
        }

        string resourceName = typeof(T).Name;

        GameObject uiObject = Managers.Resource.Spawn(ResourceKey.Path.ScreenUI + resourceName);
        if (uiObject == null)
        {
            CPrint.Error($"Screen UI를 찾을 수 없습니다. [{resourceName}]");
            return null;
        }

        uiObject.transform.SetParent(Root.transform, worldPositionStays: false);

        T screen = uiObject.GetOrAddComponent<T>();
        CurrentScreen = screen;

        return screen;
    }

    // 현재 Screen과 모든 Popup 닫기
    public void CloseAll()
    {
        CloseAllPopupUI();

        if (CurrentScreen != null)
        {
            CurrentScreen.gameObject.DestroyGO();
            CurrentScreen = null;
        }
    }

    // 모든 Popup 닫기
    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
        {
            ClosePopupUI();
        }
    }

    // Popup UI 열기
    public T OpenPopup<T>() where T : BasePopup
    {
        string resourceName = typeof(T).Name;

        GameObject uiObject = Managers.Resource.Spawn(ResourceKey.Path.PopupUI + resourceName);
        if (uiObject == null)
        {
            CPrint.Error($"Popup UI를 찾을 수 없습니다. [{resourceName}]");
            return null;
        }

        if (CurrentScreen != null)
        {
            uiObject.transform.SetParent(CurrentScreen.transform, worldPositionStays: false);
        }
        else
        {
            uiObject.transform.SetParent(Root.transform, worldPositionStays: false);
        }
        uiObject.transform.localScale = Vector3.one;

        T popup = uiObject.GetOrAddComponent<T>();
        _popupStack.Push(popup);

        return popup;
    }

    // 가장 최근에 열린 Popup 닫기
    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
        {
            return;
        }

        BasePopup popup = _popupStack.Pop();
        if (popup != null)
        {
            popup.gameObject.DestroyGO();
        }

        _nextPopupSortingOrder--;
    }

    // 지정된 Popup이 가장 최근 Popup일 경우 닫기
    public void ClosePopupUI(BasePopup popup)
    {
        if (_popupStack.Count == 0)
        {
            return;
        }

        if (_popupStack.Peek() != popup)
        {
            CPrint.Error("가장 최근에 열린 Popup이 아닙니다.");
            return;
        }

        ClosePopupUI();
    }

    // Loading UI 열기
    public void OpenLoadingUI(float fadeTime = 0f)
    {
        if (_loadingObject != null)
        {
            return;
        }

        string resourcePath = ResourceKey.Path.UI + ResourceKey.Name.LoaindgUI;

        GameObject uiObject = Managers.Resource.Spawn(resourcePath);
        if (uiObject == null)
        {
            CPrint.Error($"Loading UI를 찾을 수 없습니다. [{resourcePath}]");
            return;
        }

        uiObject.transform.SetParent(PersistentRoot.transform, worldPositionStays: false);

        _loadingObject = uiObject;

        LoadingUI loadingUI = uiObject.GetOrAddComponent<LoadingUI>();
        loadingUI.FadeIn(fadeTime);
    }

    // Loading UI 닫기
    public void CloseLoadingUI(float fadeTime = 0f)
    {
        if (_loadingObject == null)
        {
            return;
        }

        if (fadeTime <= 0f)
        {
            _loadingObject.DestroyGO();
            _loadingObject = null;
            return;
        }

        LoadingUI loadingUI = _loadingObject.GetComponent<LoadingUI>();
        if (loadingUI == null)
        {
            CPrint.Error("LoadingUI 컴포넌트를 찾을 수 없습니다.");
            _loadingObject.DestroyGO();
            _loadingObject = null;
            return;
        }
        
        loadingUI.FadeOut(fadeTime);
    }

    public void Clear()
    {
        CloseAll();

        CurrentHUD = null;
        _nextPopupSortingOrder = 2;
    }
}