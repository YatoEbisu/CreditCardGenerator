GitHub repository: [https://github.com/YatoEbisu/CreditCardGenerator](https://github.com/YatoEbisu/CreditCardGenerator)

- Crie o projeto do tipo webapi, se estiver pelo vscode utilize o seguinte comando no terminal `dotnet new webapi -o CreditCardGenerator`
- Instale o pacote nuget **Microsoft.EntityFrameworkCore.InMemory**, para podermos utilizar um banco em memória
- Apague os arquivos **WeatherForecast.cs** e o **WeatherForecastController.cs** gerados junto com o projeto
- Dentro da pasta **Models** crie uma classe com o nome **CreditCard** com os seguintes atributos:

    ```csharp
    public Guid? Id { get; set; }
    public string Email { get; set; }
    public long? CreditCardNumber { get; set; }
    public DateTime? CreatedAt { get; set; }
    ```

- Adicione as annotations para validações nos atributos no model, adicione `[Key]` no Id para indicar que ele é a chave primária, adicione `[JsonPropertyName("XXXX")]` em todos campos para alterar o nome que será apresentado no JSON da chamada e da resposta, no **Email** adicione uma `[RegularExpression("XXXX", ErrorMessage = "XXXXXXXXXXXXXX")]` passando o regex para validar se o formato do email está correto no primeiro parametro e no segundo passe a mensagem caso a validação do regex não seja atendida e adicione também `[Required(ErrorMessage = "XXXXXXXXX")]` para que o campo seja obrigatório passando também a mensagem de erro caso essa validação não seja atendida

    ```csharp
    [Key]
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Incorrect email format")]
    [Required(ErrorMessage = "required field")]
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("credit_card_number")]
    public long? CreditCardNumber { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    ```

- Na raiz do projeto adicione uma pasta com o nome **Data** e dentro desta crie uma classe **DataContext.cs**, essa classe ira herdar de **DbContext** do **Entity Framework Core** , como irei utilizar um banco interno, nao vou colocar nenhuma option ou string de conexão, e abaixo eu crio um **DbSet** passando meu model **CreditCard**

```csharp
public DataContext(DbContextOptions<DataContext> opt) : base(opt)
{
}

public DbSet<CreditCard> CreditCards { get; set; }
```

- Faça a DI do DbContext, no arquivo **Startup.cs**, adicione ao service o **DbContext** passando nosso **DataContext** e dentro dos parênteses passe as **options**                                                       `services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database"));`
- Adicione também um escopo do **DataContext** para que ele possa salvar na memória e não destruir os nossos dados quando terminar uma requisição        `services.AddScoped<DataContext, DataContext>();`
- Na pasta **Controllers** adicione um controller com o nome **CreditCardController.cs**
- Exclue as ações que são criadas automaticamente
- Crie um metodo **NonAction** do tipo **bool** para validar se o email que nosso metodo **FindAll** receber está no formato correto

```csharp
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
```

- Crie um metodo asincrono **Task<ActionResult>** passando uma lista do nosso model **CreditCard,** coloque o nome do metodo como FindAll e como parametro passe nosso **DataContext** do **Services** e uma string **email** da **Rota**... Dentro desse metodo faça a lógica para buscar os email de acordo com o email passado na rota... Acima desse metodo criado adicione o atributo `[HttpGet]` para indicar que esse é um metodo http do tipo **GET**, e adicione também o atributo `[Route]` passando o nome da rota e o parametro email que irá receber

```csharp
[HttpGet] //tipo da acao http
[Route("FindAll/{email}")]
public async Task<ActionResult<List<CreditCard>>> FindAll([FromServices] DataContext context, [FromRoute] string email)
{
    //try captura erros caso houver
    try
    {
        //validamos com o nosso metodo criado se o email que recebemos na rota esta no formato correto
        if (!IsValid(email))
            return BadRequest("Incorrect email format"); //caso nao esteja retornara o codigo 400(BadRequest) com a mensagem passada

        //buscamos do nosso banco todos os CreditCards que o email seja igual ao email que recebemos na rota
        List<CreditCard> creditCards = await context.CreditCards.Where(p => p.Email == email).ToListAsync();

        //virificamos se a lista tem algum dado
        if (creditCards.Count() < 1)
            return BadRequest("no credit cards found for this email");//caso nao tenha retornara o codigo 400(BadRequest) com a mensagem passada, aqui poderia ser usado o code 404(NotFound)

        //no final retornamos todos os CreditCards ordernando em ordem DESC por data de criacao
        return Ok(creditCards.OrderByDescending(p => p.CreatedAt));
    }
    //catch trata erro capturados, no caso aqui so retornamos um code 400 passando a exception
    catch (Exception ex)
    {
        return BadRequest(ex);
    }

}
```

- Crie outro metodo asincrono **Task<ActionResult>** passando nosso model **CreditCard,** passando como parametro o **DataContext** do **Services** e o model do **Body,** dentro desse metodo faça a logica para salvar os dados recebidos do **Body** da requisição.... Acima desse metodo adicione `[HttpPost]` para indicar que esse é um metodo http do tipo **POST,**  e  adicione também o atributo `[Route]` passando o nome da rota

```csharp
[HttpPost] //tipo da acao http
[Route("Create")]
public async Task<ActionResult<CreditCard>> Create([FromServices] DataContext context, [FromBody] CreditCard creditCard)
{
    //try captura erros caso houver
    try
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
    //catch trata erro capturados, no caso aqui so retornamos um code 400 passando a exception
    catch (Exception ex)
    {
        return BadRequest(ex);
    }

}
```

**Exemplo Create**

*Request:*

`POST: /api/CreditCard/Create`

```json
{
  "email": "teste01@email.com"
}
```

*Response:*

```json
2045243698538788
```

**Exemplo FindAll**

*Request:*

`GET: /api/CreditCard/FindAll/teste01@email.com`

*Response*

```json
[
  {
    "id": "c9efffd4-7dba-487c-b55b-fdc3db1d6bfb",
    "email": "teste01@email.com",
    "credit_card_number": 2045243698538788,
    "created_at": "2021-05-27T21:08:58.1712134-03:00"
  },
  {
    "id": "a6320a8c-3e44-442d-9e8c-dfd23b83b145",
    "email": "teste01@email.com",
    "credit_card_number": 9303582051603562,
    "created_at": "2021-05-27T21:08:56.6935808-03:00"
  }
]
```
