using Microsoft.AspNetCore.Mvc;
using DemoDockerAPI2.Services;

namespace DemoDockerAPI2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotifyController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IEmailService _service;

    public NotifyController(IConfiguration configuration, IEmailService service)
    {
        _configuration = configuration;
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _service.SendMessage("thanhthang1798@gmail.com", "Test Subject", "Test Body");
        return Ok("Email sent successfully!");
    }
}   