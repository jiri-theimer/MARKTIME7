using System.Text.Json;
using System.Text.Json.Nodes;
using BL.Mcp;
using Microsoft.AspNetCore.Authentication;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace UI.Code.Mcp
{
    /// <summary>
    /// Napojení na knihovnu MCP (NuGet ModelContextProtocol.AspNetCore) – jediný soubor v aplikaci, který její typy používá.
    /// Knihovna obstarává protokol (JSON-RPC, Streamable HTTP), MARKTIME dodává seznam nástrojů a jejich provedení (BL.Mcp.McpToolRunner).
    /// Volá se ze Startup.cs: McpSdk.AddServices(services, Configuration) a McpSdk.Map(endpoints).
    /// </summary>
    public static class McpSdk
    {
        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            var opt = McpOptions.Load(configuration);
            services.AddSingleton(opt);
            services.AddSingleton<McpClientCodec>();
            services.AddScoped<McpCallInfo>();

            // přihlášení tokenem jen pro /mcp; výchozí schéma aplikace (cookie) se nemění
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, McpBearerHandler>(McpBearerHandler.SchemeName, null);
            services.AddAuthorization(o => o.AddPolicy(McpBearerHandler.PolicyName, p => p.AddAuthenticationSchemes(McpBearerHandler.SchemeName).RequireAuthenticatedUser()));

            if (!opt.Enabled) return;

            services.AddMcpServer(o =>
                {
                    o.ServerInfo = new Implementation { Name = "marktime", Title = McpToolRunner.ServerName, Version = BL.Singleton.RunningApp.DefaultVersion };
                    o.ServerInstructions = McpToolRunner.Instructions;
                })
                .WithHttpTransport(o => o.Stateless = true)     // bez relací: každý požadavek nese token a obslouží se samostatně
                .WithListToolsHandler(ListTools)
                .WithCallToolHandler(CallTool);
        }

        public static void Map(IEndpointRouteBuilder endpoints)
        {
            var opt = endpoints.ServiceProvider.GetRequiredService<McpOptions>();
            if (!opt.Enabled) return;
            endpoints.MapMcp(McpUrls.McpPath).RequireAuthorization(McpBearerHandler.PolicyName);
        }

        private static McpToolRunner Runner(IServiceProvider sp) =>
            new McpToolRunner(sp.GetRequiredService<BL.Factory>(), sp.GetRequiredService<BL.TheColumnsProvider>(), sp.GetService<McpCallInfo>()?.CanWrite ?? false);

        private static ValueTask<ListToolsResult> ListTools(RequestContext<ListToolsRequestParams> request, CancellationToken ct)
        {
            var res = new ListToolsResult();
            foreach (var t in Runner(request.Services).List())
            {
                res.Tools.Add(new Tool
                {
                    Name = t.Name,
                    Title = t.Title,
                    Description = t.Description,
                    InputSchema = JsonSerializer.SerializeToElement(t.InputSchema),
                    Annotations = new ToolAnnotations { Title = t.Title, ReadOnlyHint = t.ReadOnly, DestructiveHint = false, IdempotentHint = t.ReadOnly, OpenWorldHint = false }
                });
            }
            return ValueTask.FromResult(res);
        }

        private static ValueTask<CallToolResult> CallTool(RequestContext<CallToolRequestParams> request, CancellationToken ct)
        {
            var input = new JsonObject();
            if (request.Params?.Arguments != null)
            {
                foreach (var kv in request.Params.Arguments) input[kv.Key] = JsonNode.Parse(kv.Value.GetRawText());
            }

            string client = request.Services.GetService<McpCallInfo>()?.ClientName;
            var o = Runner(request.Services).Call(request.Params?.Name, input, client);

            var res = new CallToolResult { IsError = o.IsError };
            res.Content.Add(new TextContentBlock { Text = o.Content ?? "" });
            return ValueTask.FromResult(res);
        }
    }
}
