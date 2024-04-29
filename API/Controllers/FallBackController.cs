using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class FallBackController : Controller
{
    public ActionResult Index()
    {
        //going into current directory > API folder
        return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
            //go to wwwroot, get index.html which is a type of text/HTML
            "wwwroot", "index.html"), "text/HTML");
    }
}