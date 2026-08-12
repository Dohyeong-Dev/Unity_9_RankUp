using System;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    private static Managers Instance
    {
        get
        {
            if (_instance == null)
            {
                Init();
            }

            return _instance;
        }
    }

    private InputManager input = new InputManager();
    public static InputManager Input => Instance?.input;

    private void Update()
    {
        Input.OnUpdate();
    }

    private static void Init()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("@Managers");
            _instance = go.AddComponent<Managers>();
            DontDestroyOnLoad(go);
        }
    }
}