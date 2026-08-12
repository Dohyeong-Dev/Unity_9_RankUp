using UnityEngine;

public class InputManager
{
    #region MOUSE
    public float MouseAxisX { get; private set; }
    public float MouseAxisY { get; private set; }
    #endregion

    #region KEY
    // 키 민감도
    private const float AxisRecogSpd = 2f;
    
    public float KeyAxisX { get; private set; }
    public float KeyAxisY { get; private set; }
    
    public bool Key_LeftShift => Input.GetKey(KeyCode.LeftShift);
    
    public bool KeyDown_Space => Input.GetKeyDown(KeyCode.Space);
    #endregion
    
    public void OnUpdate()
    {
        SetMouseAxis();
        SetKeyAxis();
    }
    
    /// <summary> 마우스의 축 값을 업데이트 </summary>
    private void SetMouseAxis()
    {
        // TODO 민감도
        MouseAxisX = Input.GetAxis("Mouse X") * 0.5f;
        MouseAxisY = Input.GetAxis("Mouse Y") * 0.5f;
    }

    /// <summary> 키보드의 축 값을 업데이트 </summary>
    private void SetKeyAxis()
    {
        bool isPress = false;

        // X축
        if (Input.GetKey(KeyCode.A))
        {
            if (KeyAxisX > 0f)
            {
                KeyAxisX = 0f;
            }

            KeyAxisX = Mathf.MoveTowards(KeyAxisX, -1, Time.deltaTime * AxisRecogSpd);
            isPress = true;

        }
        if (Input.GetKey(KeyCode.D))
        {
            if (KeyAxisX < 0f)
            {
                KeyAxisX = 0f;
            }

            KeyAxisX = Mathf.MoveTowards(KeyAxisX, 1, Time.deltaTime * AxisRecogSpd);
            isPress = true;
        }

        if (!isPress)
        {
            KeyAxisX = Mathf.MoveTowards(KeyAxisX, 0, Time.deltaTime * AxisRecogSpd * 2);
        }

        // Y축
        isPress = false;
        if (Input.GetKey(KeyCode.S))
        {
            if (KeyAxisY > 0f)
            {
                KeyAxisY = 0f;
            }

            KeyAxisY = Mathf.MoveTowards(KeyAxisY, -1, Time.deltaTime * AxisRecogSpd);
            isPress = true;

        }
        if (Input.GetKey(KeyCode.W))
        {
            if (KeyAxisY < 0f)
            {
                KeyAxisY = 0f;
            }

            KeyAxisY = Mathf.MoveTowards(KeyAxisY, 1, Time.deltaTime * AxisRecogSpd);
            isPress = true;
        }
        
        if (!isPress)
        {
            KeyAxisY = Mathf.MoveTowards(KeyAxisY, 0, Time.deltaTime * AxisRecogSpd * 2f);
        }
    }
    
    public void SetCursorLock(bool isLock)
    {
        if (isLock)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}