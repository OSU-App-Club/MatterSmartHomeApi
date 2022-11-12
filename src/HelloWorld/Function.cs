using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using HelloWorld.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld
{
    public class Function
    {
        private readonly ILogger _logger;

        public Function(ILogger logger)
        {
            _logger = logger;
        }
        
        public Function()
        {
        }
        
        private ILambdaContext _context;
        
        private void Log(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Information:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    _logger?.LogInformation(message);
                    _context?.Logger.LogInformation(message);
                    break;
                case LogLevel.Warning:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    _logger?.LogWarning(message);
                    _context?.Logger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    _logger?.LogError(message);
                    _context?.Logger.LogError(message);
                    break;
                case LogLevel.Critical:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    _logger?.LogCritical(message);
                    _context?.Logger.LogCritical(message);
                    break;
                case LogLevel.Debug:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    _logger?.LogDebug(message);
                    _context?.Logger.LogDebug(message);
                    break;
                case LogLevel.Trace:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    _logger?.LogTrace(message);
                    _context?.Logger.LogTrace(message);
                    break;
                case LogLevel.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
        
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            _context = context;
            
            // print the apiProxyEvent to the log
            Log(LogLevel.Information, JsonConvert.SerializeObject(apiProxyEvent));
            
            object ret;
            HttpStatusCode statusCode;
            
            try
            {
                // get the important details from request
                var path = apiProxyEvent.Path;
                var method = apiProxyEvent.HttpMethod;
                var pathParameters = apiProxyEvent.PathParameters;
                var queryStringParameters = apiProxyEvent.QueryStringParameters;
                var headers = apiProxyEvent.Headers;
                var rawBody = apiProxyEvent.Body;
                if (apiProxyEvent.IsBase64Encoded)
                {
                    // base 64 decode
                    rawBody = Encoding.UTF8.GetString(Convert.FromBase64String(rawBody));
                }
                // convert body to json
                var body = new JObject();
                if(rawBody != null)
                {
                    body = JObject.Parse(rawBody);
                }
                
                ret = new
                {
                    path,
                    method,
                    body
                };
                statusCode = HttpStatusCode.NotAcceptable;

                // handle paths and methods
                if (path == "/devices" && method == "GET" && pathParameters == null)
                {
                    ret = GetDevices("myuserid");
                    statusCode = HttpStatusCode.OK;
                }
                else if (path == "/devices" && method == "POST" && pathParameters == null)
                {
                    ret = CreateDevice(body);
                    statusCode = HttpStatusCode.Created;
                }
                else if (path.StartsWith("/devices") && method == "GET" && pathParameters["id"] != null)
                {
                    ret = GetDevice(pathParameters["id"]);
                    statusCode = HttpStatusCode.OK;
                }
                else if (path.StartsWith("/devices") && method == "POST" && pathParameters["id"] != null)
                {
                    
                }
                else if (path.StartsWith("/devices") && method == "PUT" && pathParameters["id"] != null)
                {
                    ret = PutDevice(pathParameters["id"], body);
                    statusCode = HttpStatusCode.NoContent;
                }
                else if (path.StartsWith("/devices") && method == "DELETE" && pathParameters["id"] != null)
                {
                    ret = DeleteDevice(pathParameters["id"]);
                    statusCode = HttpStatusCode.NoContent;
                }
            } catch (Exception e)
            {
                Log(LogLevel.Error, e.Message);
                Log(LogLevel.Error, e.StackTrace);
                
                ret = new
                {
                    error = e.Message,
                    stackTrace = e.StackTrace
                };
                statusCode = HttpStatusCode.NotAcceptable;
            }

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(ret),
                StatusCode = (int) statusCode,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        
        private object DeleteDevice(string id)
        {
            var data = new JObject
            {
                ["query"] = "mutation MyMutation { deleteDevice(input: {id: \"" + id + "\"}) { id } }"
            };
            var response = PostRequest(data);
            
            var json = JObject.Parse(response)["data"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }
            json = json["deleteDevice"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }

            return null;
        }
        
        private object PutDevice(string id, JToken body)
        {
            var device = body.ToObject<Device>();
            
            var query = "id: \"" + id + "\",";
            if (device.name != null)
            {
                query += "name: \"" + device.name + "\",";
            }
            if (device.active != null)
            {
                query += "active: " + device.active.ToString().ToLower() + ",";
            }
            if (device.wifi != null)
            {
                query += "wifi: \"" + device.wifi + "\",";
            }
            if (device.deviceType != null)
            {
                query += "deviceType: \"" + device.deviceType + "\",";
            }
            if (device.groups != null)
            {
                var groupsQuery = "[";
                foreach (var group in device.groups)
                {
                    groupsQuery += "\"" + group + "\",";
                }
                // remove last comma
                groupsQuery = groupsQuery.Substring(0, groupsQuery.Length - 1);
                groupsQuery += "]";
                
                query += "groups: " + groupsQuery + ",";
            }
            // remove last comma
            query = query.Substring(0, query.Length - 1);
            
            var data = new JObject
            {
                ["query"] = "mutation MyMutation { updateDevice(input: {" + query + "}) { id } }"
            };
            var response = PostRequest(data);
            
            var json = JObject.Parse(response)["data"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }
            json = json["updateDevice"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }

            return null;
        }

        private object GetDevice(string id)
        {
            var data = new JObject
            {
                ["query"] = "query MyQuery { getDevice(id: \"" + id + "\") { id name active deviceType groups wifi } }"
            };
            var response = PostRequest(data);

            var json = JObject.Parse(response)["data"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }
            json = json["getDevice"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }
            
            var device = json.ToObject<Device>();

            return new
            {
                device
            };
        }
        
        private object CreateDevice(JToken body)
        {
            var device = body.ToObject<Device>();
            const string userId = "myuserid";
            
            // Log(LogLevel.Information, JsonConvert.SerializeObject(device));
            
            var groupsQuery = "[";
            foreach (var group in device.groups)
            {
                groupsQuery += "\"" + group + "\",";
            }
            // remove last comma
            groupsQuery = groupsQuery.Substring(0, groupsQuery.Length - 1);
            groupsQuery += "]";
            
            var data = new JObject
            {
                ["query"] = $"mutation MyMutation {{ createDevice(input: {{active: {device.active.ToString().ToLower()}, deviceType: \"{device.deviceType}\", groups: {groupsQuery}, name: \"{device.name}\", userID: \"{userId}\", wifi: \"{device.wifi}\"}}) {{ id }} }}"
            };
            Log(LogLevel.Information, data.ToString());
            var response = PostRequest(data);

            var json = JObject.Parse(response)["data"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }
            json = json["createDevice"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }
            
            // get id from json
            var id = json["id"].ToString();

            return new
            {
                id
            };
        }

        private object GetDevices(string userId)
        {
            var data = new JObject
            {
                ["query"] = "query MyQuery { listDevices(filter: {userID: {eq: \"" + userId + "\"}}) { items { active deviceType groups id name wifi } } }"
            };
            var response = PostRequest(data);
            
            var json = JObject.Parse(response)["data"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }
            json = json["listDevices"]["items"];
            if (!json.HasValues)
            {
                throw new Exception(response);
            }
            
            var devices = json.ToObject<List<Device>>();
            
            return new
            {
                devices
            };
        }

        private string PostRequest(JObject data)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", "da2-eled6vli55hb3fjojnajabtyxe");
            var endpoint = new Uri("https://ql4yart2w5gbdh4tqv2q5glrau.appsync-api.us-west-2.amazonaws.com/graphql");
    
            // create json
            var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
    
            var result = client.PostAsync(endpoint, content).Result;
            var rawJson = result.Content.ReadAsStringAsync().Result;
            
            // print status code
            Log(LogLevel.Information, $"PostRequest status code: {result.StatusCode}");
            Log(LogLevel.Information, rawJson);

            return rawJson;
        }
    }
}
