using ProductsEntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace Products
{
    // WCF service that implements the service contract
    // This implementation performs minimal error checking and exception handling
    public class ProductsServiceImpl : IProductsService
    {
        public List<string> ListProducts()
        {
            // Create a list for holding product numbers
            List<string> productsList = new List<string>();

            try
            {
                // Connect to the AdventureWorks database by using the Entity Framework
                using (AdventureWorksEntities database = new AdventureWorksEntities())
                {
                    // Fetch the product number of every product in the database
                    var products = from product in database.Products
                                   select product.ProductNumber;

                    productsList = products.ToList();
                }
            }
            catch
            {
                // Ignore exceptions in this implementation
            }

            // Return the list of product numbers
            return productsList;
        }

        public ProductData GetProduct(string productNumber)
        {
            // Create a reference to a ProductData object
            // This object will be instantiated if a matching product is found
            ProductData productData = null;

            try
            {
                // Connect to the AdventureWorks database by using the Entity Framework
                using (AdventureWorksEntities database = new AdventureWorksEntities())
                {
                    // Find the first product that matches the specified product number
                    Product matchingProduct = database.Products.First(
                        p => String.Compare(p.ProductNumber, productNumber) == 0);

                    productData = new ProductData()
                    {
                        Name = matchingProduct.Name,
                        ProductNumber = matchingProduct.ProductNumber,
                        Color = matchingProduct.Color,
                        ListPrice = matchingProduct.ListPrice
                    };
                }
            }
            catch
            {
                // Ignore exceptions in this implementation
            }

            // Return the product
            return productData;
        }

        public int CurrentStockLevel(string productNumber)
        {
            // Obtain the total stock level for the specified product.
            // The stock level is calculated by summing the quantity of the product
            // available in all the bins in the ProductInventory table.

            // The Product and ProductInventory tables are joined over the 
            // ProductID column.

            int stockLevel = 0;

            try
            {
                // Connect to the AdventureWorks database by using the Entity Framework
                using (AdventureWorksEntities database = new AdventureWorksEntities())
                {
                    // Calculate the sum of all quantities for the specified product
                    stockLevel = (from pi in database.ProductInventories
                                  join p in database.Products
                                  on pi.ProductID equals p.ProductID
                                  where String.Compare(p.ProductNumber, productNumber) == 0
                                  select (int)pi.Quantity).Sum();
                }
            }
            catch
            {
                // Ignore exceptions in this implementation
            }

            // Return the stock level
            return stockLevel;
        }

        public bool ChangeStockLevel(string productNumber, short newStockLevel,
                             string shelf, int bin)
        {
            // Modify the current stock level of the selected product 
            // in the ProductInventory table.
            // If the update is successful then return true, otherwise return false.

            // The Product and ProductInventory tables are joined over the 
            // ProductID column.

            try
            {
                // Connect to the AdventureWorks database by using the Entity Framework
                using (AdventureWorksEntities database = new AdventureWorksEntities())
                {
                    // Find the ProductID for the specified product
                    int productID =
                        (from p in database.Products
                         where String.Compare(p.ProductNumber, productNumber) == 0
                         select p.ProductID).First();

                    // Find the ProductInventory object that matches the parameters passed
                    // in to the operation
                    ProductInventory productInventory = database.ProductInventories.First(
                        pi => String.Compare(pi.Shelf, shelf) == 0 &&
                              pi.Bin == bin &&
                              pi.ProductID == productID);

                    // Update the stock level for the ProductInventory object
                    productInventory.Quantity += newStockLevel;

                    // Save the change back to the database
                    database.SaveChanges();
                }
            }
            catch
            {
                // If an exception occurs, return false to indicate failure
                return false;
            }

            // Return true to indicate success
            return true; 
        }
    }
}
