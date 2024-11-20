using TMPro;
using UnityEngine;

public class ItemObjectScript : MonoBehaviour
{   public TMP_Text countLabel;
    public TMP_Text nameLabel;
    public TMP_Text caloriesLabel;

    public void SetItemData(FoodData foodData)
    {
        nameLabel.text = foodData.Name;
        countLabel.text = $"x{foodData.Quantity}";
        caloriesLabel.text = $"{foodData.Calories * foodData.Quantity} kcal";
    }
}
