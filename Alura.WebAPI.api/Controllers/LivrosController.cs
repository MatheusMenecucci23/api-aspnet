using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alura.ListaLeitura.Api.Controllers
{
    [Authorize]

    //api para o usuário final programador
    //Com essa API é possível que o programador faça alterações no banco de dados
    [ApiController]
    [Route("api/[controller]")]
    //ControllerBase: classe que nosso controlador deve derivar para seguir as especificações de uma API com ASP.NET
    public class LivrosController : ControllerBase
    {
        private readonly IRepository<Livro> _repo;

        public LivrosController(IRepository<Livro> repository)
        {
            _repo = repository;
        }

        [HttpGet]
        public IActionResult ListaDeLivros()
        {
            var lista = _repo.All.Select(l => l.ToApi()).ToList();
            return Ok(lista);
        }

        //Ação Recuperar para recuperar o livro
        //Método Get para pegar o livro
        [HttpGet("{id}")]

        public IActionResult Recuperar(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            //retorna o código 200 mas o objeto(model.ToModel) como json
            return Ok(model.ToApi());
        }

        //Trazendo a imagem da capa do livro
        //na URL ficará http://localhost:64466/api/Livros/capa/{id}
        [HttpGet("{id}/capa")]
        public IActionResult ImagemCapa(int id)
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

        //Ação Inlcuir para incluir o livro
        //Método POST para incluir o livro
        [HttpPost]
        public IActionResult Incluir([FromBody] LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                _repo.Incluir(livro);
                //criando a URL de uma action(Recuperar,terceiro segmento da rota)
                var url = Url.Action("Recuperar", new { id = livro.Id });
                //Esse código indica que está tudo ok e o recurso foi criado corretamente
                return Created(url, livro);//201

            }
            //Enviando um código de erro 400
            return BadRequest();

        }


        //Ação de alterar as informações do livro
        //Método Put para alterar as informações do livro
        [HttpPut]
        //[FromBody] quer dizer que as informações vem do corpo
        public IActionResult Alterar([FromBody] LivroUpload model)
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
                return Ok();//200
            }
            return BadRequest();
        }

        //Ação de remover o livro
        //Método Delete para remover o livro
        [HttpDelete("{id}")]
        public IActionResult Remover(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            _repo.Excluir(model);
            //203 tudo ok, mas não tem mais conteúdo usando esse Id
            return NoContent();//204
        }
    }
}
