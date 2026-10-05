using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary> 최대 3개의 토스트를 표시하며 동일한 메시지의 중복 표시를 방지한다. </summary>
public class ToastMessage : BaseOverlay
{
    public override int SortingOrder => 997;

    public enum CanvasGroups
    {
        ToastMessageItem,
        ToastMessageItem1,
        ToastMessageItem2,
    }

    public enum Texts
    {
        ToastMessageText,
        ToastMessageText1,
        ToastMessageText2,
    }

    #region ===== 설정 =====

    [Header("Position")]
    [SerializeField] private float _moveDistance = 55f;

    [Header("Animation")]
    [SerializeField] private float _showDuration = 0.2f;
    [SerializeField] private float _showScale = 0.96f;
    [SerializeField] private float _showOffset = 15f;
    
    [SerializeField] private float _displayDuration = 1f;
    
    [SerializeField] private float _hideDuration = 0.2f;
    [SerializeField] private float _hideScale = 0.98f;
    
    [SerializeField] private float _moveDuration = 0.2f;

    #endregion ===== 설정 =====

    #region ===== 토스트 아이템 =====

    private const int MaxMessageCount = 3;
    /// <summary> 토스트 메시지 아이템이 없는 상태를 나타내는 인덱스 </summary>
    private const int EmptyItemIndex = -1;

    // tm = toastMesseage
    private CanvasGroup[] tmCanvasGroups = new CanvasGroup[MaxMessageCount];
    private RectTransform[] tmRectTransforms = new RectTransform[MaxMessageCount];
    private TMP_Text[] tmTexts = new TMP_Text[MaxMessageCount];

    private Sequence[] _lifeSequences = new Sequence[MaxMessageCount];
    private Tweener[] _moveTweens = new Tweener[MaxMessageCount];
    
    // _인덱스 : 화면 표시 순서 (SlotIndex), 값 : ToastMessageItem(숫자) (ItemIndex)
    // ex) [2, 0, 1]이면 ToastMessageItem2 → ToastMessageItem → ToastMessageItem1
    private int[] _slotItems = new int[MaxMessageCount];

    // 현재 표시하지 못한 토스트 메시지 대기열
    private readonly Queue<string> _messageQueue = new();

    // 프리팹에 배치된 첫 번째 토스트의 위치
    private Vector2 _basePosition;

    #endregion ===== 토스트 아이템 =====

    protected override void OnAwake()
    {
        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<TMP_Text>(typeof(Texts));

        InitializeToastMessage();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < MaxMessageCount; i++)
        {
            KillAnimation(i);
        }

        _messageQueue.Clear();
    }

    #region ===== 초기화 =====

    /// <summary> 토스트 메시지에 필요한 참조를 초기화 한다. </summary>
    private void InitializeToastMessage()
    {
        tmCanvasGroups[0] = Get<CanvasGroup>(CanvasGroups.ToastMessageItem);
        tmCanvasGroups[1] = Get<CanvasGroup>(CanvasGroups.ToastMessageItem1);
        tmCanvasGroups[2] = Get<CanvasGroup>(CanvasGroups.ToastMessageItem2);

        tmRectTransforms[0] = tmCanvasGroups[0].transform as RectTransform;
        tmRectTransforms[1] = tmCanvasGroups[1].transform as RectTransform;
        tmRectTransforms[2] = tmCanvasGroups[2].transform as RectTransform;

        tmTexts[0] = Get<TMP_Text>(Texts.ToastMessageText);
        tmTexts[1] = Get<TMP_Text>(Texts.ToastMessageText1);
        tmTexts[2] = Get<TMP_Text>(Texts.ToastMessageText2);

        // 프리팹에 배치한 첫 번째 토스트의 위치를 기준으로 사용한다.
        _basePosition = tmRectTransforms[0].anchoredPosition;

        ResetToastMessage();
    }

    /// <summary> _slotItems를 EmptyItemIndex의 값으로 모두 초기화한다. </summary>
    private void SetSlotItemsEmptyIndex()
    {
        for (int i = 0; i < MaxMessageCount; i++)
        {
            _slotItems[i] = EmptyItemIndex;
        }
    }

    /// <summary> 토스트 메시지 아이템을 초기 상태로 되돌린다. </summary>
    private void ResetToastMessageItem(int itemIndex)
    {
        KillAnimation(itemIndex);

        tmCanvasGroups[itemIndex].alpha = 1f;

        tmRectTransforms[itemIndex].localScale = Vector3.one;
        tmRectTransforms[itemIndex].gameObject.SetActive(false);
    }

    /// <summary> 토스트 메세지 관련된 것들을 모두 초기화한다. </summary>
    public void ResetToastMessage()
    {
        SetSlotItemsEmptyIndex();

        for (int i = 0; i < MaxMessageCount; i++)
        {
            ResetToastMessageItem(i);
        }

        _messageQueue.Clear();
    }

    #endregion ===== 초기화 =====

    #region ===== 상태 체크 =====

    /// <summary> _slotItems에서 해당 위치가 비어있는 상태인지 체크한다. </summary>
    private bool IsEmptySlot(int slotIndex)
    {
        return _slotItems[slotIndex] == EmptyItemIndex;
    }

    /// <summary> 해당 Item이 현재 활성화되어 있는지 확인한다. </summary>
    private bool IsActiveItem(int itemIndex)
    {
        for (int i = 0; i < MaxMessageCount; i++)
        {
            if (_slotItems[i] == itemIndex)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary> 현재 표시 중인 메시지와 동일한 메시지가 있는지 확인한다. </summary>
    private bool IsSameMessageAppeared(string message)
    {
        for (int i = 0; i < MaxMessageCount; i++)
        {
            if (IsEmptySlot(i))
            {
                continue;
            }

            int itemIndex = _slotItems[i];

            if (tmTexts[itemIndex].text.Equals(message))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary> Queue에 대기 중인 메시지와 동일한 메시지가 있는지 확인한다. </summary>
    private bool IsSameMessageQueued(string message)
    {
        foreach (string queuedMessage in _messageQueue)
        {
            if (queuedMessage.Equals(message))
            {
                return true;
            }
        }

        return false;
    }

    #endregion ===== 상태 체크 =====

    #region ===== 메시지 =====

    /// <summary> 토스트 메시지를 표시한다. </summary>
    public void ActiveMessage(string message)
    {
        // 같은 메시지가 이미 표시 중이면 무시
        if (IsSameMessageAppeared(message))
        {
            return;
        }

        // 대기 중인 동일한 메시지가 있으면 무시
        if (IsSameMessageQueued(message))
        {
            return;
        }

        int itemIndex = GetUsableItemIndex();

        // 사용 가능한 Item이 있으면 바로 표시
        if (itemIndex >= 0)
        {
            ShowMessage(message, itemIndex);
            return;
        }

        // 모든 Item이 사용 중이면 Queue에 저장
        _messageQueue.Enqueue(message);
    }

    /// <summary> 새로운 토스트 메시지를 표시한다. </summary>
    private void ShowMessage(string message, int itemIndex)
    {
        ShiftSlotItemsBack(itemIndex);

        PlayItemActivated(itemIndex, message, GetSlotPosition(0));

        // 변경된 슬롯 좌표를 기준으로 좌표를 업데이트 한다.
        for (int i = 1; i < MaxMessageCount; i++)
        {
            if (IsEmptySlot(i))
            {
                continue;
            }

            PlayItemMoved(_slotItems[i], GetSlotPosition(i));
        }
    }

    /// <summary> Queue에서 다음 메시지를 꺼내 표시한다. </summary>
    private void ShowNextMessage()
    {
        if (_messageQueue.Count == 0)
        {
            return;
        }

        int itemIndex = GetUsableItemIndex();

        if (itemIndex < 0)
        {
            return;
        }

        string message = _messageQueue.Dequeue();

        ShowMessage(message, itemIndex);
    }

    /// <summary> 기존 SlotItem을 한 칸씩 뒤로 이동하고 첫 번째 Slot에 새로운 Item을 배치한다. </summary>
    private void ShiftSlotItemsBack(int itemIndex)
    {
        for (int i = MaxMessageCount - 1; i > 0; i--)
        {
            _slotItems[i] = _slotItems[i - 1];
        }
        
        _slotItems[0] = itemIndex;
    }

    /// <summary> 해당 Item의 SlotIndex를 찾고 그 인덱스를 기준으로 한 칸씩 앞으로 이동한다. </summary>
    private void RemoveItemAndShiftForward(int itemIndex)
    {
        int removeSlotIndex = -1;

        for (int i = 0; i < MaxMessageCount; i++)
        {
            if (_slotItems[i] == itemIndex)
            {
                removeSlotIndex = i;
                break;
            }
        }

        if (removeSlotIndex < 0)
        {
            return;
        }

        // 제거된 Item의 뒤에 있는 Item을 한 칸씩 앞으로 이동시킨다.
        for (int i = removeSlotIndex; i < MaxMessageCount - 1; i++)
        {
            _slotItems[i] = _slotItems[i + 1];
        }

        // 마지막 슬롯은 비어있는 상태로 설정한다.
        _slotItems[MaxMessageCount - 1] = EmptyItemIndex;
    }

    #endregion ===== 메시지 =====

    #region ===== 애니메이션 =====

    /// <summary> 토스트 메시지를 활성화하고 등장 애니메이션을 재생한다. </summary>
    private void PlayItemActivated(int itemIndex, string message, Vector2 position)
    {
        KillAnimation(itemIndex);

        // 활성화 전 세팅
        tmTexts[itemIndex].text = message;
        tmCanvasGroups[itemIndex].alpha = 0f;
        tmRectTransforms[itemIndex].anchoredPosition = position + Vector2.up * _showOffset;
        tmRectTransforms[itemIndex].localScale = Vector3.one * _showScale;

        // 활성화
        tmRectTransforms[itemIndex].gameObject.SetActive(true);

        // 해당 위치로 이동 애니메이션을 재생
        _moveTweens[itemIndex] = tmRectTransforms[itemIndex].DOAnchorPos(position, _showDuration).SetEase(Ease.OutCubic);

        // 생명주기 애니메이션
        Sequence sequence = DOTween.Sequence();

        // Fade In + Scale
        sequence.Append(tmCanvasGroups[itemIndex].DOFade(1f, _showDuration).SetEase(Ease.OutQuad));
        sequence.Join(tmRectTransforms[itemIndex].DOScale(1f, _showDuration).SetEase(Ease.OutCubic));

        // 화면 표시 시간
        sequence.AppendInterval(_displayDuration);

        // Fade Out + Scale
        sequence.Append(tmCanvasGroups[itemIndex].DOFade(0f, _hideDuration).SetEase(Ease.InQuad));
        sequence.Join(tmRectTransforms[itemIndex].DOScale(_hideScale, _hideDuration).SetEase(Ease.InQuad));

        _lifeSequences[itemIndex] = sequence;

        sequence.OnComplete(() => HandleItemDisplayFinished(itemIndex));
    }

    /// <summary> 토스트 메시지를 지정된 위치로 이동하는 애니메이션을 재생한다. </summary>
    private void PlayItemMoved(int itemIndex, Vector2 position)
    {
        // 토스트의 생명주기 Sequence에는 영향을 주지 않으며 위치를 이동시킨다.
        _moveTweens[itemIndex]?.Kill();
        _moveTweens[itemIndex] = tmRectTransforms[itemIndex].DOAnchorPos(position, _moveDuration).SetEase(Ease.OutCubic);
    }

    /// <summary> 토스트 메시지의 표시 시간이 끝났을 때 호출된다. </summary>
    private void HandleItemDisplayFinished(int itemIndex)
    {
        KillAnimation(itemIndex);
        RemoveItemAndShiftForward(itemIndex);
        ResetToastMessageItem(itemIndex);

        // 남아있는 메시지들의 좌표를 업데이트 한다.
        for (int i = 0; i < MaxMessageCount; i++)
        {
            if (IsEmptySlot(i))
            {
                continue;
            }

            PlayItemMoved(_slotItems[i], GetSlotPosition(i));
        }

        // 대기 중인 메시지가 있으면 다음 메시지를 표시
        ShowNextMessage();
    }

    /// <summary> 현재 실행 중인 애니메이션을 종료한다. </summary>
    private void KillAnimation(int itemIndex)
    {
        _lifeSequences[itemIndex]?.Kill();
        _lifeSequences[itemIndex] = null;

        _moveTweens[itemIndex]?.Kill();
        _moveTweens[itemIndex] = null;
    }

    #endregion ===== 애니메이션 =====

    #region ===== Get =====

    /// <summary> 지정된 슬롯의 위치를 반환한다. 첫 번째 토스트의 프리팹 위치를 기준으로 아래쪽에 배치한다. </summary>
    private Vector2 GetSlotPosition(int slotIndex)
    {
        return _basePosition + Vector2.down * (_moveDistance * slotIndex);
    }

    /// <summary> 사용 가능한 토스트 Item의 인덱스를 반환한다. </summary>
    private int GetUsableItemIndex()
    {
        for (int i = 0; i < MaxMessageCount; i++)
        {
            if (!IsActiveItem(i))
            {
                return i;
            }
        }

        return EmptyItemIndex;
    }

    #endregion ===== Get =====
}