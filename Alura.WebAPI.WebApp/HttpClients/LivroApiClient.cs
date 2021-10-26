using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Seguranca;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
//criando um apelido
using Lista = Alura.ListaLeitura.Modelos.ListaLeitura;

namespace Alura.ListaLeitura.HttpClients
{
    public class LivroApiClient
    {
        //injeção de dependência
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _accessor;

        //Criando uma api com recursos seperado dos recursos da web

        public LivroApiClient(HttpClient httpClient, AuthApiClient auth, IHttpContextAccessor accessor)
        {
            //Montando a requisição
            _httpClient = httpClient;

            _accessor = accessor;
        }

        private void AddBearerToken()
        {

            //pegando o token
            var token = _accessor.HttpContext.User.Claims.First(c => c.Type == "Token").Value;
            
            // injetando o token como cabeçalho de autenticação
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",token);
        }

        //método de pegar a lista de leitura
        public async Task<Lista> GetListaLeituraAsync(TipoListaLeitura tipo)
        {

            AddBearerToken();

            //var token = await _auth.PostLoginAsync(new LoginModel { Login = "admin", Password = "123" });
            var resposta = await _httpClient.GetAsync($"listasleitura/{tipo}");
            resposta.EnsureSuccessStatusCode();
            return await resposta.Content.ReadAsAsync<Lista>();
        }

        //método de deletar um livro
        public async Task DeleteLivroAsync(int id)
        {
            AddBearerToken();
            var resposta = await _httpClient.DeleteAsync($"livros/{id}");
            resposta.EnsureSuccessStatusCode();
        }

        //método que busca a capa do livro
        public async Task<byte[]> GetCapaLivroAsync(int id)
        {
            AddBearerToken();
            //Fazendo uma requisição HTTP do tipo Get
            HttpResponseMessage resposta = await _httpClient.GetAsync($"livros/{id}/capa");

            //EnsureSuccessStatusCode() se o código da reuqisição for da família do 200 nada acontece, caso contrário é lançado uma exceção
            resposta.EnsureSuccessStatusCode();

            //Retornando a repostas da requisição
            return await resposta.Content.ReadAsByteArrayAsync();
        }

        //métod que busca o livro
        public async Task<LivroApi> GetLivroAsync(int id)
        {
            AddBearerToken();

            //Fazendo uma requisição HTTP do tipo get
            HttpResponseMessage resposta = await _httpClient.GetAsync($"livros/{id}");

            //EnsureSuccessStatusCode() se o código da reuqisição for da família do 200 nada acontece, caso contrário é lançado uma exceção
            resposta.EnsureSuccessStatusCode();

            //retornando a resposta da requisição
            return await resposta.Content.ReadAsAsync<LivroApi>();
        }

        private string EnvolveComAspasDuplas(string valor)
        {
            return $"\"{valor}\"";
        }

        private HttpContent CreateMultipartFormDataContent(LivroUpload model)
        {
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.Titulo), EnvolveComAspasDuplas("titulo"));
            content.Add(new StringContent(model.Lista.ParaString()), EnvolveComAspasDuplas("lista"));

            if (!string.IsNullOrEmpty(model.Subtitulo))
            {
                content.Add(new StringContent(model.Subtitulo), EnvolveComAspasDuplas("subtitulo"));
            }

            if (!string.IsNullOrEmpty(model.Resumo))
            {
                content.Add(new StringContent(model.Resumo), EnvolveComAspasDuplas("resumo"));
            }

            if (!string.IsNullOrEmpty(model.Autor))
            {
                content.Add(new StringContent(model.Autor), EnvolveComAspasDuplas("autor"));
            }



            if (model.Id > 0)
            {
                content.Add(new StringContent(model.Id.ToString()), EnvolveComAspasDuplas("id"));
            }

            if (model.Capa != null)
            {
                //criando método de upload da capa do livro
                var imagemContent = new ByteArrayContent(model.Capa.ConvertToBytes());
                imagemContent.Headers.Add("content-type", "image/png");
                content.Add(imagemContent, EnvolveComAspasDuplas("capa"), EnvolveComAspasDuplas("capa.png"));


            }

            return content;
        }

        //criando o método post
        public async Task PostLivroAsync(LivroUpload model)
        {
            AddBearerToken();
            HttpContent content = CreateMultipartFormDataContent(model);
            var resposta = await _httpClient.PostAsync("livros", content);
            resposta.EnsureSuccessStatusCode(); //se não for cód 200, lança exception.
        }
        
        //criando o método put
        public async Task PutLivrosAsync(LivroUpload model)
        {
            AddBearerToken();
            HttpContent content = CreateMultipartFormDataContent(model);
            var resposta = await _httpClient.PutAsync("livros", content);
            resposta.EnsureSuccessStatusCode();
        }
    }
}
