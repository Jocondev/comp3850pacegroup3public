using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NumberSequencePuzzle : UdonSharpBehaviour
{
    [SerializeField] private TurnDial1 dial1;
    [SerializeField] private TurnDial1 dial2;
    [SerializeField] private TurnDial1 dial3;
    [SerializeField] private TurnDial1 dial4;

    [SerializeField] private int code1;
    [SerializeField] private int code2;
    [SerializeField] private int code3;
    [SerializeField] private int code4;


    void Start()
    {
        
    }

    public void CheckCode()
    {
        int number1 = dial1.GetCurrentNumber();
        int number2 = dial2.GetCurrentNumber();
        int number3 = dial3.GetCurrentNumber();
        int number4 = dial4.GetCurrentNumber();

        if (number1 == code1 && number2 == code2 && number3 == code3 && number4 == code4)
        {
            Debug.Log("Correct code!");
        }
    }
}
