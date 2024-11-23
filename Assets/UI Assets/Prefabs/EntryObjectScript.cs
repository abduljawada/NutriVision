using TMPro;
using UnityEngine;

public class EntryObjectScript : MonoBehaviour
{
    public TMP_Text dateLabel;
    public TMP_Text totCaloriesLabel;

    public void SetEntryData(string date, string calories)
    {
        dateLabel.text = date;
        totCaloriesLabel.text = $"{calories} kcal";
    }
}
