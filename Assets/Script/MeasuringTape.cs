using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeasuringTape 
{
    public GameObject TapeLine;
    public TextMeshPro TapeInfo;
    public static double MetersToInches(double meters) => meters * 39.3701;
    public static double MetersTocentemeters(double meters) => meters * 100;

}
