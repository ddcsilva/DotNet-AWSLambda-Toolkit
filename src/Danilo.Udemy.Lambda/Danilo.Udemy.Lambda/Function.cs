using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Net;
using System.Text.Json;

// Atributo Assembly para habilitar a entrada JSON da função Lambda para ser convertida em uma classe .NET.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Danilo.Udemy.Lambda;

public class Function
{
    private readonly IDynamoDBContext _dynamoDBContext;

    public Function()
    {
        // Instância do DynamoDBContext para realizar a consulta no DynamoDB.
        _dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    /// <summary>
    /// Função Lambda para retornar um usuário do DynamoDB.
    /// </summary>
    /// <param name="input">Id do usuário</param>
    /// <param name="context">Contexto da função Lambda</param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        request.RequestContext.Http.Method.ToUpper();

        // Verifica se o método da requisição é GET.
        var httpMethod = request.RequestContext.Http.Method.ToUpper();

        switch (httpMethod)
        {
            case "GET":
                return await HandleGetRequest(request);
            case "POST":
                return await HandlePostRequest(request);
            case "PUT":
                return await HandlePutRequest(request);
            case "DELETE":
                return await HandleDeleteRequest(request);
            default:
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "Método não suportado.",
                    StatusCode = (int)HttpStatusCode.MethodNotAllowed
                };
        }
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        // Verifica se o Id do usuário foi informado na requisição.
        request.PathParameters.TryGetValue("usuarioId", out var usuarioIdString);
        // Converte o Id do usuário para Guid.
        if (Guid.TryParse(usuarioIdString, out var usuarioId))
        {
            // Consulta no DynamoDB, passando o Id do usuário como parâmetro.
            var usuario = await _dynamoDBContext.LoadAsync<Usuarios>(usuarioId);

            // Verifica se o usuário foi encontrado.
            if (usuario == null)
            {
                // Se o usuário não for encontrado, retorna um erro 404.
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "Usuário não encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
            else
            {
                // Se o usuário foi encontrado, retorna o usuário.
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = JsonSerializer.Serialize(usuario),
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
        }

        // Se o Id do usuário for inválido, retorna um erro 400.
        return BadResponse("Id do usuário inválido");
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        // Converte o corpo da requisição para a classe Usuarios.
        var usuario = JsonSerializer.Deserialize<Usuarios>(request.Body);

        if (usuario == null)
        {
            return BadResponse("Usuário inválido");
        }

        // Salva o usuário no DynamoDB.
        await _dynamoDBContext.SaveAsync(usuario);

        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePutRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("usuarioId", out var usuarioIdString);

        if (Guid.TryParse(usuarioIdString, out var usuarioId))
        {
            var usuario = await _dynamoDBContext.LoadAsync<Usuarios>(usuarioId);

            if (usuario == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "Usuário não encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            var usuarioAtualizado = JsonSerializer.Deserialize<Usuarios>(request.Body);

            if (usuarioAtualizado == null)
            {
                return BadResponse("Usuário inválido");
            }

            usuarioAtualizado.Id = usuario.Id;

            await _dynamoDBContext.SaveAsync(usuarioAtualizado);

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        return BadResponse("Id do usuário inválido");
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleDeleteRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("usuarioId", out var usuarioIdString);
        if (Guid.TryParse(usuarioIdString, out var usuarioId))
        {
            var usuario = await _dynamoDBContext.LoadAsync<Usuarios>(usuarioId);

            if (usuario == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "Usuário não encontrado.",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            await _dynamoDBContext.DeleteAsync(usuario);

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        return BadResponse("Id do usuário inválido");
    }

    private static APIGatewayHttpApiV2ProxyResponse BadResponse(string mensagem)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = mensagem,
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    }
}

public class Usuarios
{
    // Atributo para definir a chave primária da tabela.
    // DynamoDBHashKey: Chave primária do tipo hash.
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    public string Nome { get; set; }
}