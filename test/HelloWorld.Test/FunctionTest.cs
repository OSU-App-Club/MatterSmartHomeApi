using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

/*
 * https://stackoverflow.com/a/67873341/16762230
 * Note: Run this test command to see the output in the console
 * dotnet test -l "console;verbosity=detailed" test/HelloWorld.Test
 */

namespace HelloWorld.Tests
{
    public class FunctionTest
    {
        private readonly ITestOutputHelper _output;
        
        public FunctionTest(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public async Task TestGetDevice()
        {
            var request = new APIGatewayProxyRequest
            {
                Path = "/devices",
                HttpMethod = "GET",
                PathParameters = new Dictionary<string, string>
                {
                    {"id", "mydeviceid"}
                }
            };
            var logger = Logger.CreateLogger(_output);
            var context = new TestLambdaContext();
            var function = new Function(logger);
            var response = await function.FunctionHandler(request, context);
            _output.WriteLine("Lambda response status code: " + response.StatusCode);
            _output.WriteLine("Lambda response body: " + response.Body);
            
            // do asserts
            var expectedResponse = new APIGatewayProxyResponse
            {
                // Body = JsonSerializer.Serialize(new {}),
                StatusCode = (int) HttpStatusCode.OK,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };

            // Assert.Equal(expectedResponse.Body, response.Body);
            Assert.Equal(expectedResponse.Headers, response.Headers);
            Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
        }
        
        [Fact]
        public async Task TestGetDevice2()
        {
            var request = new APIGatewayProxyRequest
            {
                Path = "/devices",
                HttpMethod = "GET",
                PathParameters = new Dictionary<string, string>
                {
                    {"id", "123"}
                }
            };
            var logger = Logger.CreateLogger(_output);
            var context = new TestLambdaContext();
            var function = new Function(logger);
            var response = await function.FunctionHandler(request, context);
            _output.WriteLine("Lambda response status code: " + response.StatusCode);
            _output.WriteLine("Lambda response body: " + response.Body);
            
            // do asserts
            var expectedResponse = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.NotAcceptable,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };

            Assert.Equal(expectedResponse.Headers, response.Headers);
            Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
        }
        
        [Fact]
        public async Task TestCreateDevice()
        {
            var request = new APIGatewayProxyRequest
            {
                Path = "/devices",
                HttpMethod = "POST",
                Body = JsonSerializer.Serialize(new
                {
                    name = "mydevicename",
                    active = true,
                    wifi = "mywifi",
                    deviceType = "dimmer",
                    groups = new List<string>
                    {
                        "group1",
                        "group2"
                    }
                })
            };
            var logger = Logger.CreateLogger(_output);
            var context = new TestLambdaContext();
            var function = new Function(logger);
            var response = await function.FunctionHandler(request, context);
            _output.WriteLine("Lambda response status code: " + response.StatusCode);
            _output.WriteLine("Lambda response body: " + response.Body);
            
            // do asserts
            var expectedResponse = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.Created,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };

            Assert.Equal(expectedResponse.Headers, response.Headers);
            Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
        }
        
        [Fact]
        public async Task TestUpdateDevice()
        {
            const string deviceId = "mydeviceid";
            
            var request = new APIGatewayProxyRequest
            {
                Path = $"/devices/{deviceId}",
                HttpMethod = "PUT",
                PathParameters = new Dictionary<string, string>
                {
                    {"id", deviceId}
                },
                Body = JsonSerializer.Serialize(new
                {
                    name = "mynewname"
                })
            };
            var logger = Logger.CreateLogger(_output);
            var context = new TestLambdaContext();
            var function = new Function(logger);
            var response = await function.FunctionHandler(request, context);
            _output.WriteLine("Lambda response status code: " + response.StatusCode);
            _output.WriteLine("Lambda response body: " + response.Body);
            
            // do asserts
            var expectedResponse = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.NoContent,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };

            Assert.Equal(expectedResponse.Headers, response.Headers);
            Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
        }
        
        [Fact]
        public async Task TestDeleteDevice()
        {
            const string deviceId = "a2e2a5e1-c0e1-4715-84cf-11b2f570ac4e";
            
            var request = new APIGatewayProxyRequest
            {
                Path = $"/devices/{deviceId}",
                HttpMethod = "DELETE",
                PathParameters = new Dictionary<string, string>
                {
                    {"id", deviceId}
                }
            };
            var logger = Logger.CreateLogger(_output);
            var context = new TestLambdaContext();
            var function = new Function(logger);
            var response = await function.FunctionHandler(request, context);
            _output.WriteLine("Lambda response status code: " + response.StatusCode);
            _output.WriteLine("Lambda response body: " + response.Body);
            
            // do asserts
            var expectedResponse = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.NoContent,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };

            Assert.Equal(expectedResponse.Headers, response.Headers);
            Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
        }
    }
}