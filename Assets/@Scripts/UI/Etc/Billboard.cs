using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _camTr;
    
    private void Start()
    {
        if (Managers.Scene.CurrentScene is GameScene gameScene)
        {
            _camTr = gameScene.Cam.transform;
        }
    }

    private void LateUpdate()
    {
        if (_camTr)
        {
            transform.forward = _camTr.transform.forward;
        }
    }
}
