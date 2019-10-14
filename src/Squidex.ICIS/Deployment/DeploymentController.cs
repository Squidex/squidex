using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.ICIS.Deployment
{
    public sealed class DeploymentController : ApiController
    {
        private readonly DeploymentService deploymentService;

        public DeploymentController(ICommandBus commandBus, DeploymentService deploymentService)
            : base(commandBus)
        {
            this.deploymentService = deploymentService;
        }

        [HttpGet]
        [Route("/deploy")]
        public IActionResult Deploy([FromQuery] int kafka = 1, [FromQuery] int deploy = 1)
        {
            var started = deploymentService.Start(kafka == 1, deploy == 1);

            return Ok(new { started });
        }
    }
}
