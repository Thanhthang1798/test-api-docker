using Microsoft.AspNetCore.Mvc;
using DemoDockerAPI2.Services;

namespace DemoDockerAPI2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_service.GetMessage());
    }
}