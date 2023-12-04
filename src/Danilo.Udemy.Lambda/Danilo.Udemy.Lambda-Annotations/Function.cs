using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Danilo.Udemy.Lambda_Annotations;

public class Function
{

    private readonly IDynamoDBContext _dynamoDBContext;

    public Function()
    {
        // Instância do DynamoDBContext para realizar a consulta no DynamoDB.
        _dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "usuarios/{usuarioId}")]
    public async Task<Usuarios> FunctionHandler(string usuarioId, ILambdaContext context)
    {
        Guid.TryParse(usuarioId, out var id);
        return await _dynamoDBContext.LoadAsync<Usuarios>(id);
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