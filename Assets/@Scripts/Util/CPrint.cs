using System;
using UnityEngine;

public static class CPrint
{
    // 로그 출력 스위치
    public static bool EnableLog = true;

    // 들여쓰기 레벨
    private static int _indentLevel;

    // 공백 개수
    private const int IndentSize = 10;


    #region ===== Log =====

    public static void Log(object message)
    {
        Emit(PrintType.Log, message);
    }

    public static void Success(object message)
    {
        Emit(PrintType.Success, message);
    }

    public static void Warning(object message)
    {
        Emit(PrintType.Warning, message);
    }

    public static void Error(object message)
    {
        Emit(PrintType.Error, message);
    }

    #endregion


    /// <summary> 에디터 또는 Development Build에서만 로그를 출력한다. Release Build에서는 호출 코드 자체가 제거된다. </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void Emit(PrintType type, object message)
    {
        if (!EnableLog)
        {
            return;
        }

        string indent = new string(' ', _indentLevel * IndentSize);

        switch (type)
        {
            case PrintType.Log:
            {
                Debug.Log($"{indent}<color=#4FC3F7>[LOG]</color> {message}");
                break;
            }

            case PrintType.Success:
            {
                Debug.Log($"{indent}<color=#66BB6A>[SUCCESS]</color> {message}");
                break;
            }

            case PrintType.Warning:
            {
                Debug.Log($"{indent}<color=#FFD54F>[WARNING]</color> {message}");
                break;
            }

            case PrintType.Error:
            {
                Debug.Log($"{indent}<color=#EF5350>[ERROR]</color> {message}");
                break;
            }
        }
    }
}