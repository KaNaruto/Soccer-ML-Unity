using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private KeyCode reset;
    public static event Action OnReset;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(reset))
        {
            OnReset?.Invoke();
        }
    }
}
