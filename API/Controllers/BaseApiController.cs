using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")] // /api/users (class name minus the word controller)
public class BaseApiController : ControllerBase
{
    
}