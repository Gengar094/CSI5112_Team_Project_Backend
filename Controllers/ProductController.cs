using CSI5112BackEndApi.Models;
using CSI5112BackEndApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSI5112BackEndApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductsService _ProductsService;

    public ProductsController(ProductsService ProductsService) =>
        _ProductsService = ProductsService;
    
    [HttpGet("all")]
    public async Task<ActionResult<List<Product>>> Get() {
        var product = await _ProductsService.GetAllProducts();
        return product;
    }

    [HttpGet("get_filter_option")]
    public async Task<FilterOption> GetFilterOption() {
        List<Product> res = await _ProductsService.GetAllProducts();
        HashSet<String> filter_category = new HashSet<string>();
        HashSet<String> filter_manufacturer = new HashSet<string>();
        foreach (Product p in res) {
            filter_category.Add(p.category);
            filter_manufacturer.Add(p.manufacturer);
        }
        String categories = "";
        String manufacturers = "";
        foreach (String s in filter_category) {
            categories += s;
            categories += "_";
        }
        foreach (String s in filter_manufacturer) {
            manufacturers += s;
            manufacturers += "_";
        }
        return new FilterOption("ascending", categories.Remove(categories.Length - 1, 1), manufacturers.Remove(manufacturers.Length - 1, 1));
    }

    [HttpGet("get_filter_option_merchant")]
    public async Task<FilterOption> GetFilterOptionMerchant([FromQuery] string merchant_id) {
        List<Product> res = await _ProductsService.GetProductsByMerchant(merchant_id);
        HashSet<String> filter_category = new HashSet<string>();
        HashSet<String> filter_manufacturer = new HashSet<string>();
        foreach (Product p in res) {
            filter_category.Add(p.category);
            filter_manufacturer.Add(p.manufacturer);
        }
        String categories = "";
        String manufacturers = "";
        foreach (String s in filter_category) {
            categories += s;
            categories += "_";
        }
        foreach (String s in filter_manufacturer) {
            manufacturers += s;
            manufacturers += "_";
        }
        return new FilterOption("ascending", categories.Remove(categories.Length - 1, 1), manufacturers.Remove(manufacturers.Length - 1, 1));
    }

    [HttpGet]
    public async Task<List<Product>> Get([FromQuery]string product_id) =>
        await _ProductsService.GetProductsByID(product_id);

    [HttpGet("filter/owner")]
    public async Task<List<Product>> Get([FromQuery] string owner_id, [FromQuery] string input, [FromQuery] string priceSort, [FromQuery] string location, [FromQuery] string category)
    {
        String[] locations = location.Split('_');
        String[] categories = category.Split('_');
        
        var product = await _ProductsService.SortProductsByMerchant(owner_id, input == "#" ? "" : input, priceSort, locations, categories);

        return product;
    }

    [HttpGet("filter")]
    public async Task<List<Product>> Gets([FromQuery] string input, [FromQuery] string priceSort, [FromQuery] string location, [FromQuery] string category)
    {
        String[] locations = location.Split('_');
        String[] categories = category.Split('_');
        
        var product = await _ProductsService.SortProductsBySearch(input == "#" ? "" : input, priceSort, locations, categories);

        return product;
    }

    [HttpPost("create")]
    public async Task<List<Product>> Post(Product newProduct)
    {
        await _ProductsService.CreateProduct(newProduct);

        return await _ProductsService.GetProductsByMerchant(newProduct.owner_id);
    }

    [HttpPut("update")]
    public async Task<List<Product>> Update([FromQuery]string id, Product updatedProduct)
    {
        var Product = await _ProductsService.GetProductsByID(id);

        updatedProduct.product_id = Product[0].product_id;

        await _ProductsService.UpdateProduct(id, updatedProduct);

        return await _ProductsService.GetProductsByMerchant(updatedProduct.owner_id);
    }

    [HttpPut("update_category")]
    public async Task<List<Product>> UpdateCategory([FromQuery] string owner_id, [FromQuery] string category, [FromQuery] string origin)
    {
        List<Product> products = await _ProductsService.GetProductByCategory(origin, owner_id);

        foreach (Product product in products) {
            product.category = category;
            if (product.product_id == null) continue;
            await _ProductsService.UpdateProduct(product.product_id, product);
        }

        return await _ProductsService.GetProductsByMerchant(owner_id);
    }

    [HttpDelete("delete")]
    public async Task<List<Product>> Delete([FromBody] string id)
    {
        await _ProductsService.RemoveProduct(id.Split('_'));

        return await _ProductsService.GetAllProducts();
    }

    [HttpDelete("delete_category")]
    public async Task<List<Product>> DeleteCategory([FromQuery] string category, [FromQuery] string owner_id)
    {
        List<Product> products = await _ProductsService.GetProductByCategory(category, owner_id);

        foreach (Product product in products) {
            product.category = "";
            if (product.product_id == null) continue;
            await _ProductsService.UpdateProduct(product.product_id, product);
        }

        return await _ProductsService.GetProductsByMerchant(owner_id);
    }
}