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