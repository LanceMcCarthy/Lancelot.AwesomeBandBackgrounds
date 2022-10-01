using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace BandCentral.Models.Helpers
{
    public static class IapHelper
    {
        public static async Task<bool> PurchaseProductAsync(this string productId)
        {
            try
            {
                var result = await CurrentApp.RequestProductPurchaseAsync(productId);
                Debug.WriteLine($"---------{productId} Purchase Result Status: {result.Status}------------");

                switch (result.Status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        Debug.WriteLine($"{productId} Purchase Successful");
                        return true;
                    case ProductPurchaseStatus.AlreadyPurchased:
                        Debug.WriteLine($"{productId} Purchase AlreadyPurchased");
                        return true;
                    case ProductPurchaseStatus.NotFulfilled:
                        Debug.WriteLine($"{productId} Purchase NotFulfilled");
                        return false;
                    case ProductPurchaseStatus.NotPurchased:
                        Debug.WriteLine($"{productId} Purchase NotPurchased");
                        return false;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{productId} Purchase Error: {ex.Message}");
                return false;
            }
        }
    }
}
