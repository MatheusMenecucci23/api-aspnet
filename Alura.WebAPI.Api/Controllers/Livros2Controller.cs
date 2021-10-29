using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Alura.WebAPI.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;

namespace Alura.ListaLeitura.Api.Controllers
{
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName ="v2")]//monstrando para o swagger que essa é a versão 2
    [ApiVersion("2.0")]//Versionamento está suportando a versão 2.0
    [Route("api/v{version:apiVersion}/livros")]
    public class Livros2Controller : ControllerBase
    {
        private readonly IRepository<Livro> _repo;

        public Livros2Controller(IRepository<Livro> repository)
        {
            _repo = repository;
        }

        //aplicando filtro na coleções de livros pela Query com AplicaFiltro
        //aplicando uma ordem nos livros com o AplicaOrdem
        //pegando os parametros da Query, ou seja, da Url com o [FromQuery]
        [HttpGet]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroPaginado))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 404)]
        public IActionResult ListaDeLivros([FromQuery] LivroFiltro filtro,
            [FromQuery] LivroOrdem ordem, [FromQuery] LivroPaginacao paginacao)
        {
            var livroPaginado = _repo.All.AplicaFiltro(filtro).AplicaOrdem(ordem).Select(l => l.ToApi()).ToLivroPaginado(paginacao);
            return Ok(livroPaginado);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Recupera o livro identificado por seu {id}.",
            Tags = new[] { "Livros" },
            Produces = new[] { "application/json", "application/xml" }
        )]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        [ProducesResponseType(404)]
        public IActionResult Recuperar([SwaggerParameter("Id do livro.", Required = true)] int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }
        [SwaggerOperation(
            Summary = "Recupera a capa do livro identificado por seu {id}.",
            Tags = new[] { "Livros" },
            Produces = new[] { "image/png" }
        )]
        [HttpGet("{id}/capa")]
        public IActionResult ImagemCapa([SwaggerParameter("Id do livro.", Required = true)] int id)
        {
            byte[] img = _repo.All
                .Where(l => l.Id == id)
                .Select(l => l.ImagemCapa)
                .FirstOrDefault();
            if (img != null)
            {
                return File(img, "image/png");
            }
            return File("~/images/capas/capa-vazia.png", "image/png");
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Registra novo livro na base.",
            Tags = new[] { "Livros" }
            )]
        [ProducesResponseType(statusCode: 201, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 400, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        public IActionResult Incluir([FromForm] LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                
                _repo.Incluir(livro);
                var uri = Url.Action("Recuperar", new { id = livro.Id });
                return Created(uri, livro); //201
            }
            return BadRequest(ErrorResponse.FromModelState(ModelState));
        }

        [HttpPut]
        [SwaggerOperation(
            Summary = "Modifica o livro na base.",
            Tags = new[] { "Livros" })]
        [ProducesResponseType(statusCode: 200)]
        [ProducesResponseType(400, Type = typeof(ErrorResponse))]
        [ProducesResponseType(500, Type = typeof(ErrorResponse))]
        public IActionResult Alterar([FromForm] LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                if (model.Capa == null)
                {
                    livro.ImagemCapa = _repo.All
                        .Where(l => l.Id == livro.Id)
                        .Select(l => l.ImagemCapa)
                        .FirstOrDefault();
                }
                _repo.Alterar(livro);
                return Ok(); //200
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Exclui o livro da base.",
            Tags = new[] { "Livros" }
        )]
        [ProducesResponseType(404)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500, Type = typeof(ErrorResponse))]
        public IActionResult Remover(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            _repo.Excluir(model);
            return NoContent(); //203
        }
    }
}
