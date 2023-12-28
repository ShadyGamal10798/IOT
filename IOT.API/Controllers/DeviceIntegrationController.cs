using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IOT.TCPListner;
namespace IOT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceIntegrationController : ControllerBase
    {
        private readonly ITcpClientService _tcpClientService;
        public DeviceIntegrationController(ITcpClientService tcpClientService)
        {
             _tcpClientService = tcpClientService;
        }

        [HttpPost]
        public async Task<IActionResult> SendCommand([FromBody] SendCommandModel model)
        {
            
            await _tcpClientService.SendCommandToClient(model.IEMI,model.Command);
            return Ok();
        }
    }
}
