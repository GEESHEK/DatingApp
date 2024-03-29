using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ServiceFilter(typeof(LogUserActivity))]
[ApiController]
[Route("api/[controller]")] // /api/users (class name minus the word controller)
public class BaseApiController : ControllerBase
{
    
}