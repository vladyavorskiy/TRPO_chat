using Microsoft.AspNetCore.Mvc;

namespace TRPO_Web_start.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private IProductService _productService;
    private readonly IServiceProvider _serviceProvider;

    public ProductController(IProductService productService, IServiceProvider serviceProvider)
    {
        _productService = productService;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var t = _serviceProvider.GetService<IProductService>();
        return Ok(_productService.GetProducts());
    }
}
