using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 게임의 HUD, Screen, Popup, Overlay, Loading UI를 생성하고 관리한다. </summary>
public class UIManager
{
    #region ===== 현재 UI =====

    public BaseHUD CurrentHUD { get; private set; }
    public BaseScreen CurrentScreen { get; private set; }
    public BasePopup CurrentPopup => _popupStack.Count > 0 ? _popupStack.Peek() : null;

    #endregion ===== 현재 UI =====

    #region ===== Popup =====

    private readonly Stack<BasePopup> _popupStack = new();
    private int _nextPopupSortingOrder = 2;

    #endregion ===== Popup =====

    #region ===== Overlay =====

    private HitEffectUI _hitEffectUI;

    #endregion ===== Overlay =====

    #region ===== Loading =====

    private GameObject _loadingObject;
    public bool IsLoading => _loadingObject != null;

    #endregion ===== Loading =====

    #region ===== Root =====

    private GameObject _root;

    private GameObject Root
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
                UnityEngine.Object.DontDestroyOnLoad(_persistentRoot);
            }

            return _persistentRoot;
        }
    }

    #endregion ===== Root =====

    /// <summary> 현재 활성화된 UI에 따라 입력 처리를 수행한다. </summary>
    public void OnUpdate()
    {
        if (IsLoading)
        {
            return;
        }

        if (CurrentPopup != null)
        {
            Managers.Input.SetInputEnabled(false);
            CurrentPopup.OnInputKey();
            return;
        }

        if (CurrentScreen != null)
        {
            Managers.Input.SetInputEnabled(false);
            CurrentScreen.OnInputKey();
            return;
        }

        if (CurrentHUD == null)
        {
            return;
        }

        if (!CurrentHUD.IsVisible)
        {
            Managers.Input.SetInputEnabled(false);
            return;
        }

        Managers.Input.SetInputEnabled(true);
        CurrentHUD.OnInputKey();
    }

    #region ===== Canvas =====

    /// <summary> UI Canvas를 설정하고 UI 종류에 맞는 정렬 순서를 적용한다. </summary>
    public void SetupCanvas(BaseUI baseUI)
    {
        if (baseUI == null)
        {
            return;
        }

        Canvas canvas = baseUI.gameObject.GetOrAddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (baseUI is BaseHUD baseHUD)
        {
            CurrentHUD = baseHUD;
        }

        if (baseUI is BasePopup)
        {
            canvas.sortingOrder = _nextPopupSortingOrder++;
            return;
        }

        canvas.sortingOrder = baseUI.SortingOrder;
    }

    #endregion ===== Canvas =====

    #region ===== Screen =====

    /// <summary> 지정된 타입의 Screen UI를 연다. </summary>
    public T OpenScreen<T>() where T : BaseScreen
    {
        if (CurrentScreen is T)
        {
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

        uiObject.transform.SetParent(Root.transform, false);

        T screen = uiObject.GetOrAddComponent<T>();
        CurrentScreen = screen;

        return screen;
    }

    /// <summary> 현재 열려 있는 Screen UI를 닫는다. </summary>
    private void CloseCurrentScreen()
    {
        if (CurrentScreen == null)
        {
            return;
        }

        CurrentScreen.gameObject.DestroyGO();
        CurrentScreen = null;
    }

    #endregion ===== Screen =====

    #region ===== Popup =====

    /// <summary> 지정된 타입의 Popup UI를 연다. </summary>
    public T OpenPopup<T>() where T : BasePopup
    {
        string resourceName = typeof(T).Name;
        GameObject uiObject = Managers.Resource.Spawn(ResourceKey.Path.PopupUI + resourceName);

        if (uiObject == null)
        {
            CPrint.Error($"Popup UI를 찾을 수 없습니다. [{resourceName}]");
            return null;
        }

        Transform parent = CurrentScreen != null ? CurrentScreen.transform : Root.transform;

        uiObject.transform.SetParent(parent, false);
        uiObject.transform.localScale = Vector3.one;

        T popup = uiObject.GetOrAddComponent<T>();
        _popupStack.Push(popup);

        Managers.Input.SetCursorLock(false);

        return popup;
    }

    /// <summary> 가장 최근에 열린 Popup을 닫는다. </summary>
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

        if (_popupStack.Count == 0)
        {
            Managers.Input.SetCursorLock(true);
        }
    }

    /// <summary> 지정된 Popup이 가장 최근에 열린 Popup인 경우 닫는다. </summary>
    public void ClosePopupUI(BasePopup popup)
    {
        if (_popupStack.Count == 0)
        {
            return;
        }

        if (CurrentPopup != popup)
        {
            CPrint.Error("가장 최근에 열린 Popup이 아닙니다.");
            return;
        }

        ClosePopupUI();
    }

    /// <summary> 현재 열려 있는 모든 Popup을 닫는다. </summary>
    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
        {
            ClosePopupUI();
        }
    }

    #endregion ===== Popup =====

    #region ===== Overlay =====

    /// <summary> 플레이어 피격 효과를 재생한다. </summary>
    public void OpenHitEffect()
    {
        if (!TryCreateHitEffect())
        {
            return;
        }

        _hitEffectUI.Play();
    }

    /// <summary> 플레이어 피격 효과를 중지한다. </summary>
    public void CloseHitEffect()
    {
        _hitEffectUI?.Stop();
    }
    
    /// <summary> 피격 효과 UI가 없으면 생성하고 참조를 저장한다. </summary>
    private bool TryCreateHitEffect()
    {
        if (_hitEffectUI != null)
        {
            return true;
        }

        GameObject uiObject = Managers.Resource.Spawn(ResourceKey.Path.OverlayUI + nameof(HitEffectUI));

        if (uiObject == null)
        {
            CPrint.Error("HitEffectUI를 생성하지 못했습니다.");
            return false;
        }

        uiObject.transform.SetParent(PersistentRoot.transform, false);

        _hitEffectUI = uiObject.GetOrAddComponent<HitEffectUI>();

        SetupCanvas(_hitEffectUI);

        return true;
    }

    #endregion ===== Overlay =====

    #region ===== Loading =====

    /// <summary> Loading UI를 열고 지정된 페이드 연출을 재생한다. </summary>
    public void OpenLoadingUI(float fadeTime = 0f, Action openAction = null)
    {
        if (IsLoading)
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

        uiObject.transform.SetParent(PersistentRoot.transform, false);

        _loadingObject = uiObject;

        LoadingUI loadingUI = uiObject.GetOrAddComponent<LoadingUI>();
        loadingUI.FadeIn(fadeTime, openAction);
    }

    /// <summary> 현재 Loading UI를 닫고 지정된 페이드 연출을 재생한다. </summary>
    public void CloseLoadingUI(float fadeTime = 0f, Action closeAction = null)
    {
        if (!IsLoading)
        {
            return;
        }

        if (fadeTime <= 0f)
        {
            DestroyLoadingUI();
            return;
        }

        LoadingUI loadingUI = _loadingObject.GetComponent<LoadingUI>();

        if (loadingUI == null)
        {
            CPrint.Error("LoadingUI 컴포넌트를 찾을 수 없습니다.");
            DestroyLoadingUI();
            return;
        }

        loadingUI.FadeOut(fadeTime, closeAction);
    }

    /// <summary> 현재 Loading UI를 제거하고 참조를 초기화한다. </summary>
    private void DestroyLoadingUI()
    {
        if (_loadingObject == null)
        {
            return;
        }

        _loadingObject.DestroyGO();
        _loadingObject = null;
    }

    #endregion ===== Loading =====

    #region ===== 정리 =====

    /// <summary> 현재 Screen과 모든 Popup을 닫는다. </summary>
    public void CloseAll()
    {
        CloseAllPopupUI();
        CloseCurrentScreen();
    }

    /// <summary> 현재 씬에서 사용한 UI 상태를 초기화한다. </summary>
    public void Clear()
    {
        CloseAll();

        CurrentHUD = null;
        _nextPopupSortingOrder = 2;
    }

    #endregion ===== 정리 =====
}