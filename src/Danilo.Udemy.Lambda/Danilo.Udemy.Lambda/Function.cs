using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;

// Atributo Assembly para habilitar a entrada JSON da função Lambda para ser convertida em uma classe .NET.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Danilo.Udemy.Lambda;

public class Function
{

    /// <summary>
    /// Função Lambda para retornar um usuário do DynamoDB.
    /// </summary>
    /// <param name="input">Id do usuário</param>
    /// <param name="context">Contexto da função Lambda</param>
    /// <returns></returns>
    public async Task<Usuarios> FunctionHandler(Guid input, ILambdaContext context)
    {
        // Instância do DynamoDBContext para realizar a consulta no DynamoDB.
        var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
        // Consulta no DynamoDB, passando o Id do usuário como parâmetro.
        var usuario = await dynamoDBContext.LoadAsync<Usuarios>(input);

        return usuario;
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