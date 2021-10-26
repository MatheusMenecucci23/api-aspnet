using Alura.ListaLeitura.Seguranca;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alura.ListaLeitura.HttpClients
{
    public class LoginResult
    {
        public bool Succeeded { get; set; }
        public string Token { get; set; }
    }


    //API de autenticação
    public class AuthApiClient
    {

        //consumindo o serviço de autenticação e obtendo o token JWT

        private HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<LoginResult> PostLoginAsync(LoginModel model)
        {
            //fazendo uma requisição para obter o token
            var resposta = await _httpClient.PostAsJsonAsync("login", model);
            return new LoginResult
            {
                Succeeded = resposta.IsSuccessStatusCode,
                Token = await resposta.Content.ReadAsStringAsync()
            };

        }
    }
}
