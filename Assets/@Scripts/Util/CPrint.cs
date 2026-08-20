using System;
using UnityEngine;
using System.Collections.Generic;

/* 그룹 사용방법
CPrint.Group("Player", () =>
{
      CPrint.Log("Move");
      CPrint.Log("Jump");

      CPrint.Group("Attack", () =>
      {
          CPrint.Success("Critical");
          CPrint.Warning("HP Low");
      });

      CPrint.Error("Dead");
});

[LOG] ▼ Player
       [LOG] Move
       [LOG] Jump
       [LOG] ▼ Attack
           [SUCCESS] Critical
           [WARNING] HP Low
       [LOG] ----------------------------------------
       [ERROR] Dead
   [LOG] ----------------------------------------
*/

public static class CPrint
{
    // 로그 출력 스위치
    public static bool EnableLog = true;

    // 들여쓰기 레벨
    private static int _indentLevel = 0;

    // 공백 개수
    private const int IndentSize = 10;

    public static void Log(object message) => Emit(PrintType.Log, message);
    
    public static void Success(object message) => Emit(PrintType.Success, message);

    public static void Warning(object message) => Emit(PrintType.Warning, message);

    public static void Error(object message) => Emit(PrintType.Error, message);


    /*
       🔵 [LOG]      플레이어 생성
       🟢 [SUCCESS]  아이템 획득
       🟡 [WARNING]  HP가 부족합니다.
       🔴 [ERROR]    Player가 연결되지 않았습니다.
     */
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
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
                Debug.Log($"{indent}<color=#4FC3F7>[LOG]</color> {message}");
                break;

            case PrintType.Success:
                Debug.Log($"{indent}<color=#66BB6A>[SUCCESS]</color> {message}");
                break;

            case PrintType.Warning:
                Debug.LogWarning($"{indent}<color=#FFD54F>[WARNING]</color> {message}");
                break;

            case PrintType.Error:
                Debug.LogError($"{indent}<color=#EF5350>[ERROR]</color> {message}");
                break;
        }
    }

    public static void KV(string key, object value)
    {
        Log($"{key} : {value}");
    }

    // 그룹 로그 출력
    public static void Group(string title, Action body)
    {
        Log($"▼ {title}");

        Push();

        body?.Invoke();

        Pop();

        Log("----------------------------------------");
    }

    private static void Push()
    {
        _indentLevel++;
    }

    private static void Pop()
    {
        if (_indentLevel > 0)
        {
            _indentLevel--;
        }
    }
}