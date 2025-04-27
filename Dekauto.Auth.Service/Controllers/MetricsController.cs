using Dekauto.Auth.Service.Domain.Interfaces;
using Dekauto.Auth.Service.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.Auth.Service.Controllers
{
    [Route("api/metrics")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly DekautoContext context;
        private readonly IRequestMetricsService requestMetricsService;

        public MetricsController(DekautoContext context, IRequestMetricsService requestMetricsService)
        {
            this.context = context;
            this.requestMetricsService = requestMetricsService;
        }
        [Authorize(Policy = "OnlyAdmin")]
        [Route("healthcheck")]
        [HttpGet]
        public async Task<IActionResult> HealthCheckAsync() 
        {
            try
            {
                await context.Roles.FirstAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return Ok(false);
            }
        }

        [Route("counter")]
        [Authorize(Policy = "OnlyAdmin")]
        [HttpGet]
        public async Task<IActionResult> RequestsPerPeriod()
        {
            try
            {
                return Ok(requestMetricsService.GetRecentCounters());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Не удалось получить количество запросов.");
            }
        }

    }
}
