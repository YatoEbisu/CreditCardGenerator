using CreditCardGenerator.Data;
using CreditCardGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;


namespace CreditCardGenerator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreditCardController : ControllerBase
    {
        [HttpGet] //tipo da acao http
        [Route("FindAll/{email}")]
        public async Task<ActionResult<List<CreditCard>>> FindAll([FromServices] DataContext context, [FromRoute] string email)
        {
            //validamos com o nosso metodo criado se o email que recebemos na rota esta no formato correto
            if (!IsValid(email)) 
                return BadRequest("Incorrect email format"); //caso nao esteja retornara o codigo 400(BadRequest) com a mensagem passada

            //buscamos do nosso banco todos os CreditCards que o email seja igual ao email que recebemos na rota
            List<CreditCard> creditCards = await context.CreditCards.Where(p => p.Email == email).ToListAsync();

            //virificamos se a lista tem algum dado
            if(creditCards.Count() < 1)
               return BadRequest("no credit cards found for this email");//caso nao tenha retornara o codigo 400(BadRequest) com a mensagem passada, aqui poderia ser usado o code 404(NotFound)

            //no final retornamos todos os CreditCards ordernando em ordem DESC por data de criacao
            return Ok(creditCards.OrderByDescending(p => p.CreatedAt));
        }

        [HttpPost] //tipo da acao http
        [Route("Create")]
        public async Task<ActionResult<CreditCard>> Create([FromServices] DataContext context, [FromBody] CreditCard creditCard) 
        {
            //verifico se os dados recebidos estao de acordo com as nossas validacoes feita no model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //cartoes de credito geralmente tem 16 numeros, irei gerar numeros aleatorios como foi pedido no exercicio
            //aqui instancio a classe random para usar seus metodos
            Random random = new Random();

            //crio uma string vazia para adicionar os numeros aleatorios
            string creditCardRandom = "";

            //gero um numero de 1000 a 9999 e vou concatenando na variavel creditCardRandom
            //rodo 4 vezes para dar os 16 caracteres que precisamos
            for (int i = 0; i < 4; i++)
            {
                var randomNumber = random.Next(1000, 9999).ToString();
                creditCardRandom += randomNumber;
            }

            //converto a string com os 16 numeros para o tipo long para que passo ser atribuido esse valor no  CreditCardNumber que e do tipo long
            creditCard.CreditCardNumber = long.Parse(creditCardRandom);

            //gero um novo UUID para a chave primaria
            creditCard.Id = Guid.NewGuid();

            //atribuo a data e hora que este codigo esta sendo rodado 
            creditCard.CreatedAt = DateTime.Now;

            //adiciono o model no nosso context
            context.CreditCards.Add(creditCard);

            //salvo as alteracoes feita no context
            await context.SaveChangesAsync();

            //no final retorno o codigo 201(Created) passando a rota e o novo cartao de credito gerado
            return Created("api/CreditCard/Create", creditCard.CreditCardNumber);
        }

        [NonAction]
        public bool IsValid(string email)
        {
            try
            {
                MailAddress m = new MailAddress(email);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


    }
}
