using UnityEngine;
using OpenFoodFactsCSharp.Services;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class OpenFoodFactsTest2 : MonoBehaviour
{
    private static System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
    private OpenFoodFactsWrapperImpl service;

    private DebugToScreen debugToScreen => GetComponent<DebugToScreen>();

    // Make Start method async with Task
    public async void FetchProduct(string value)
    {
        FetchProductTask(value);
    }
    private async Task FetchProductTask(string value)
    {
        debugToScreen.Log("Initializing Service");
        // Initialize the service
        service = new OpenFoodFactsWrapperImpl(new OpenFoodFactsCSharp.Clients.OpenFoodFactsApiLowLevelClient(client));
        debugToScreen.Log("Initialized Service");

        var match = Regex.Match(value, @"^\d+");

        var productCode = match.Value;  // Extract the matched portion

        try
        {
            debugToScreen.Log("Fetching Product: " + productCode);
            // Fetch product details asynchronously
            var productResponse = await service.FetchProductByCodeAsync(productCode);
            debugToScreen.Log("Fetched Product");

            // Log the product response (customize based on the structure of productResponse)
            if (productResponse != null && productResponse.Product != null)
            {
                debugToScreen.Log($"Product Name: {productResponse.Product.ProductName}");
                debugToScreen.Log($"Product Kcal per serving: {productResponse.Product.Nutriments.EnergyKcalServing}");
                debugToScreen.Log($"Product Protein per serving: {productResponse.Product.Nutriments.ProteinsServing}");
                debugToScreen.Log($"Product Fats per serving: {productResponse.Product.Nutriments.FatServing}");
                debugToScreen.Log($"Product Carbs per serving: {productResponse.Product.Nutriments.CarbohydratesServing}");
            }
            else
            {
                debugToScreen.LogError("Product not found or invalid response.");
            }
        }
        catch (System.Exception ex)
        {
            debugToScreen.LogError($"Error fetching product: {ex.Message}");
        }
    }
}
