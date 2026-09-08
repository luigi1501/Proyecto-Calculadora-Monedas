
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TasaPlus.Shared.Services;

namespace DolarMonitorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasasController : ControllerBase
    {
        private readonly TasasDataService _tasasDataService;

        public TasasController(TasasDataService tasasDataService)
        {
            _tasasDataService = tasasDataService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasas()
        {
            var result = await _tasasDataService.RefreshTasasAsync();
            return Ok(result);
        }
    }
}
