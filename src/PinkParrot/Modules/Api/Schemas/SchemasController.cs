using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PinkParrot.Modules.Api.Schemas
{
    public class SchemasController : Controller
    {
        [HttpPost]
        [Route("schemas/")]
        public async Task Create()
        {
        }

        [HttpPut]
        [Route("schemas/{name}/")]
        public async Task Update()
        {
        }

        [HttpDelete]
        [Route("schemas/{name}/")]
        public async Task Delete()
        {
        }
    }
}