using UnityEngine;
using TMPro;

public class PostRaceTimeEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text lapDisplay;
    [SerializeField] private TMP_Text timeDisplay;

    public void Display(string lapTitle, int lapTime) {
        lapDisplay.text = lapTitle;
        timeDisplay.text = CarLapTimer.GetFormattedTime(lapTime);
    }
}