using UnityEngine;
using OpenFoodFactsCSharp.Services;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class OpenFoodFactsTest : MonoBehaviour
{
    private static System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
    private OpenFoodFactsWrapperImpl service;

    // Make Start method async with Task
    public async Task<FoodData> FetchProduct(string value)
    {
        return await FetchProductTask(value);
    }
    private async Task<FoodData> FetchProductTask(string value)
    {
        Debug.Log("Initializing Service");
        // Initialize the service
        service = new OpenFoodFactsWrapperImpl(new OpenFoodFactsCSharp.Clients.OpenFoodFactsApiLowLevelClient(client));
        Debug.Log("Initialized Service");

        var match = Regex.Match(value, @"^\d+");

        var productCode = match.Value;  // Extract the matched portion

        try
        {
            Debug.Log("Fetching Product: " + productCode);
            // Fetch product details asynchronously
            var productResponse = await service.FetchProductByCodeAsync(productCode);
            Debug.Log("Fetched Product");

            // Log the product response (customize based on the structure of productResponse)
            if (productResponse != null && productResponse.Product != null)
            {
                string productName = productResponse.Product.ProductName;
                float calories = productResponse.Product.Nutriments.EnergyKcalServing != null? (float) productResponse.Product.Nutriments.EnergyKcalServing : 0;
                float protein = productResponse.Product.Nutriments.ProteinsServing != null? (float) productResponse.Product.Nutriments.ProteinsServing : 0;
                float fats = productResponse.Product.Nutriments.FatServing != null? (float) productResponse.Product.Nutriments.FatServing : 0;
                float carbs = productResponse.Product.Nutriments.CarbohydratesServing != null? (float) productResponse.Product.Nutriments.CarbohydratesServing : 0;
                
                Debug.Log($"Product Name: {productName}");
                Debug.Log($"Product Kcal per serving: {calories}");
                Debug.Log($"Product Protein per serving: {protein}");
                Debug.Log($"Product Fats per serving: {fats}");
                Debug.Log($"Product Carbs per serving: {carbs}");
                
                return new FoodData
                {
                    Name = productName,
                    Calories = calories,
                    Protein = protein,
                    Fats = fats,
                    Carbs = carbs
                };
            }
            else
            {
                Debug.LogError(productCode + " not found or invalid response.");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error fetching product: {ex.Message}");
            return null;
        }
    }
}
