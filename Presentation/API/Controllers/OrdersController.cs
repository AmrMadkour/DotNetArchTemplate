using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OrdersController : ControllerBase
    {

        [HttpPost(Name = "quote")]
        public Quote Quote(Order order)
        {
           
        }
    }
}
