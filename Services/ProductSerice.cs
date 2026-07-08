namespace DemoDockerAPI2.Services;

public class ProductService : IProductService
{
    public string GetMessage()
    {
        return "Hello Service";
    }
}