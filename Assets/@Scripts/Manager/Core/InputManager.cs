using UnityEngine;

/// <summary> 게임에서 사용하는 마우스와 키보드 입력을 관리한다. </summary>
public class InputManager
{
    #region ===== 마우스 =====

    public float MouseAxisX { get; private set; }
    public float MouseAxisY { get; private set; }
    public float MouseWheel { get; private set; }

    public bool MouseDown_Left => Input.GetMouseButtonDown(0);
    public bool MouseDown_Right => Input.GetMouseButtonDown(1);

    #endregion ===== 마우스 =====

    #region ===== 키보드 =====

    private const float AxisSensitivity = 1f;
    private const float AxisReturnSpeedMultiplier = 2f;

    public float KeyAxisX { get; private set; }
    public float KeyAxisY { get; private set; }

    /// <summary> 입력 자체의 세기 </summary>
    public float KeyInputStrength => Mathf.Max(Mathf.Abs(KeyAxisX), Mathf.Abs(KeyAxisY));

    public bool Key_LeftShift => Input.GetKey(KeyCode.LeftShift);
    public bool KeyDown_Space => Input.GetKeyDown(KeyCode.Space);
    public bool KeyDown_Esc => Input.GetKeyDown(KeyCode.Escape);
    public bool KeyDown_Enter => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

    public bool KeyDown_O => Input.GetKeyDown(KeyCode.O);
    public bool KeyDown_P => Input.GetKeyDown(KeyCode.P);
    public bool KeyDown_I => Input.GetKeyDown(KeyCode.I);
    public bool KeyDown_F => Input.GetKeyDown(KeyCode.F);
    
    #endregion ===== 키보드 =====

    #region ===== 상태 =====

    /// <summary> 플레이어가 입력을 받을 수 있는 상태인지 나타낸다. </summary>
    public bool CanReceivePlayer { get; private set; }

    #endregion ===== 상태 =====

    /// <summary> 현재 프레임의 입력 값을 업데이트한다. </summary>
    public void OnUpdate()
    {
        UpdateMouseInput();
        UpdateKeyboardInput();
    }

    #region ===== 입력 업데이트 =====

    /// <summary> 마우스 입력 값을 업데이트한다. </summary>
    private void UpdateMouseInput()
    {
        // TODO: 마우스 민감도 설정
        MouseAxisX = Input.GetAxis("Mouse X") * 0.5f;
        MouseAxisY = Input.GetAxis("Mouse Y") * 0.5f;
        MouseWheel = Input.GetAxis("Mouse ScrollWheel");
    }

    /// <summary> 키보드 이동 축 값을 업데이트한다. </summary>
    private void UpdateKeyboardInput()
    {
        KeyAxisX = UpdateAxis(KeyAxisX, KeyCode.A, KeyCode.D);
        KeyAxisY = UpdateAxis(KeyAxisY, KeyCode.S, KeyCode.W);
    }

    /// <summary> 현재 입력 축 값을 초기화하여 이전 이동 입력이 유지되지 않도록 한다. </summary>
    public void ClearKeyAxisValue()
    {
        KeyAxisX = 0f;
        KeyAxisY = 0f;
    }
    
    /// <summary> 지정한 두 키의 입력에 따라 축 값을 부드럽게 변경한다. </summary>
    private float UpdateAxis(float currentValue, KeyCode negativeKey, KeyCode positiveKey)
    {
        bool isNegativePressed = Input.GetKey(negativeKey);
        bool isPositivePressed = Input.GetKey(positiveKey);

        if (isNegativePressed && !isPositivePressed)
        {
            if (currentValue > 0f)
            {
                currentValue = 0f;
            }

            return Mathf.MoveTowards(currentValue, -1f, Time.deltaTime * AxisSensitivity);
        }

        if (isPositivePressed && !isNegativePressed)
        {
            if (currentValue < 0f)
            {
                currentValue = 0f;
            }

            return Mathf.MoveTowards(currentValue, 1f, Time.deltaTime * AxisSensitivity);
        }

        return Mathf.MoveTowards(currentValue, 0f,
            Time.deltaTime * AxisSensitivity * AxisReturnSpeedMultiplier);
    }

    #endregion ===== 입력 업데이트 =====

    #region ===== 입력 제어 =====

    /// <summary> 입력 수신 가능 여부를 설정한다. </summary>
    public void SetInputEnabled(bool enabled)
    {
        CanReceivePlayer = enabled;
    }

    /// <summary> 마우스 커서의 잠금 상태를 설정한다. </summary>
    public void SetCursorLock(bool isLock)
    {
        Cursor.visible = !isLock;
        Cursor.lockState = isLock ? CursorLockMode.Locked : CursorLockMode.None;
    }

    #endregion ===== 입력 제어 =====
}