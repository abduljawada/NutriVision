// using UnityEngine;
// using OpenFoodFactsCSharp.Services;
// using System.Threading.Tasks;
// using System.Text.RegularExpressions;

// public class OpenFoodFactsTest : MonoBehaviour
// {
//     private static System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
//     private OpenFoodFactsWrapperImpl service;

//     // Make Start method async with Task
//     public async void FetchProduct(string value)
//     {
//         FetchProductTask(value);
//     }
//     private async Task FetchProductTask(string value)
//     {
//         Debug.Log("Initializing Service");
//         // Initialize the service
//         service = new OpenFoodFactsWrapperImpl(new OpenFoodFactsCSharp.Clients.OpenFoodFactsApiLowLevelClient(client));
//         Debug.Log("Initialized Service");

//         var match = Regex.Match(value, @"^\d+");

//         var productCode = match.Value;  // Extract the matched portion

//         try
//         {
//             Debug.Log("Fetching Product: " + productCode);
//             // Fetch product details asynchronously
//             var productResponse = await service.FetchProductByCodeAsync(productCode);
//             Debug.Log("Fetched Product");

//             // Log the product response (customize based on the structure of productResponse)
//             if (productResponse != null && productResponse.Product != null)
//             {
//                 Debug.Log($"Product Name: {productResponse.Product.ProductName}");
//                 Debug.Log($"Product Kcal per serving: {productResponse.Product.Nutriments.EnergyKcalServing}");
//                 Debug.Log($"Product Protein per serving: {productResponse.Product.Nutriments.ProteinsServing}");
//                 Debug.Log($"Product Fats per serving: {productResponse.Product.Nutriments.FatServing}");
//                 Debug.Log($"Product Carbs per serving: {productResponse.Product.Nutriments.CarbohydratesServing}");
//             }
//             else
//             {
//                 Debug.LogError("Product not found or invalid response.");
//             }
//         }
//         catch (System.Exception ex)
//         {
//             Debug.LogError($"Error fetching product: {ex.Message}");
//         }
//     }
// }
