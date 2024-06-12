// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.EndToEndTests;

public class SkillTests(ITestOutputHelper outputHelper)
{
    [SkippableTheory]
    [InlineData("Alexa, ask London Travel if there is any disruption today.")]
    [InlineData("Alexa, ask London Travel about the DLR.")]
    [InlineData("Alexa, ask London Travel about the Elizabeth line.")]
    [InlineData("Alexa, ask London Travel about London Overground.")]
    [InlineData("Alexa, ask London Travel about the Victoria line.")]
    [InlineData("Alexa, ask London Travel about the Windrush line.")]
    public async Task Can_Invoke_Skill_And_Get_Valid_Response(string content)
    {
        // Arrange
        string? functionName = TestConfiguration.FunctionName;
        string? regionName = TestConfiguration.RegionName;
        string? skillId = TestConfiguration.SkillId;
        string? stage = TestConfiguration.SkillStage;

        Skip.If(string.IsNullOrEmpty(functionName), "No Lambda function name is configured.");
        Skip.If(string.IsNullOrEmpty(regionName), "No AWS region name is configured.");
        Skip.If(string.IsNullOrEmpty(skillId), "No skill ID is configured.");
        Skip.If(string.IsNullOrEmpty(stage), "No skill stage is configured.");

        using var client = await CreateHttpClientAsync();

        await EnableSkillAsync(client, skillId, stage);

        // Act
        var simulation = await SimulateSkillAsync(client, skillId, stage, content);
        simulation = await GetSimulationSkillAsync(client, skillId, stage, simulation);

        // Assert
        simulation.ShouldNotBeNull();
        simulation.Result.ShouldNotBeNull();
        simulation.Result.Error.ShouldBeNull(simulation.Result.Error?.Message);
        simulation.Result.SkillExecutionInfo.ShouldNotBeNull();
        simulation.Result.SkillExecutionInfo.Invocations.ShouldNotBeNull();
        simulation.Result.SkillExecutionInfo.Invocations.ShouldNotBeEmpty();
        simulation.Result.SkillExecutionInfo.Invocations.Count.ShouldBeGreaterThanOrEqualTo(1);

        var invocation = simulation.Result.SkillExecutionInfo.Invocations[0];

        invocation.InvocationRequest.ShouldNotBeNull();
        invocation.InvocationRequest.RootElement.TryGetProperty("body", out _).ShouldBeTrue();
        invocation.InvocationRequest.RootElement.TryGetProperty("endpoint", out var endpoint).ShouldBeTrue();

        string? endpointValue = endpoint.GetString();
        endpointValue.ShouldNotBeNull();
        endpointValue.ShouldStartWith($"arn:aws:lambda:{regionName}:");
        endpointValue.ShouldEndWith($":function:{functionName}");

        invocation.InvocationResponse.ShouldNotBeNull();
        invocation.InvocationResponse.RootElement.TryGetProperty("body", out var body).ShouldBeTrue();
        body.TryGetProperty("response", out var skillResponse).ShouldBeTrue();
        skillResponse.TryGetProperty("outputSpeech", out var outputSpeech).ShouldBeTrue();

        outputHelper.WriteLine($"Output speech: {outputSpeech}");

        outputSpeech.TryGetProperty("type", out var speechType).ShouldBeTrue();
        speechType.GetString().ShouldBe("SSML");

        outputSpeech.TryGetProperty("ssml", out var ssml).ShouldBeTrue();
        string? speech = ssml.GetString();

        speech.ShouldNotBeNullOrWhiteSpace();
        speech.ShouldStartWith("<speak>");
        speech.ShouldNotContain("Sorry, something went wrong.");
    }

    private static async Task<string> GenerateAccessTokenAsync()
    {
        // To generate a new refresh token, run the following command:
        // npm install -g ask-cli && ask util generate-lwa-tokens --client-id $CLIENT_ID --client-confirmation $CLIENT_SECRET --scopes "alexa::ask:skills:readwrite alexa::ask:skills:test"
        string? clientId = TestConfiguration.AlexaClientId;
        string? clientSecret = TestConfiguration.AlexaClientSecret;
        string? refreshToken = TestConfiguration.AlexaRefreshToken;

        Skip.If(string.IsNullOrEmpty(clientId), "No client ID is configured.");
        Skip.If(string.IsNullOrEmpty(clientSecret), "No client secret is configured.");
        Skip.If(string.IsNullOrEmpty(refreshToken), "No refresh token is configured.");

        // See https://developer.amazon.com/docs/login-with-amazon/authorization-code-grant.html#using-refresh-tokens
        var parameters = new Dictionary<string, string>()
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
        };

        using var client = new HttpClient()
        {
            DefaultRequestHeaders =
            {
                UserAgent = { TestConfiguration.UserAgent },
            },
        };

        using var content = new FormUrlEncodedContent(parameters);
        using var response = await client.PostAsync(new Uri("https://api.amazon.com/auth/o2/token"), content);

        response.EnsureSuccessStatusCode();

        using var tokens = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return tokens!.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
    }

    private static async Task<HttpClient> CreateHttpClientAsync()
    {
        string token = await GenerateAccessTokenAsync();

        var client = new HttpClient()
        {
            BaseAddress = new Uri("https://api.amazonalexa.com", UriKind.Absolute),
            DefaultRequestHeaders =
            {
                Authorization = new("Bearer", token),
                UserAgent = { TestConfiguration.UserAgent },
            },
        };

        return client;
    }

    private static async Task EnableSkillAsync(HttpClient client, string skillId, string stage)
    {
        // See https://developer.amazon.com/en-US/docs/alexa/smapi/skill-enablement.html#enable-skill
        using var response = await client.PutAsJsonAsync($"v1/skills/{skillId}/stages/{stage}/enablement", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, $"Failed to enable skill for stage {stage}.");
        response.EnsureSuccessStatusCode();
    }

    private static async Task<SimulationResponse> GetSimulationSkillAsync(
        HttpClient client,
        string skillId,
        string stage,
        SimulationResponse simulation)
    {
        string simulationId = simulation.Id!;

        const string InProgress = "IN_PROGRESS";

        if (simulation.Status is InProgress)
        {
            // Poll for a response to be available
            var delay = TimeSpan.FromSeconds(2);

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(delay);

                // See https://developer.amazon.com/en-US/docs/alexa/smapi/skill-simulation-api.html#get-simulation-result
                var result = await client.GetFromJsonAsync<SimulationResponse>($"v2/skills/{skillId}/stages/{stage}/simulations/{simulationId}");
                result.ShouldNotBeNull();

                simulation = result;

                if (simulation.Status is not InProgress)
                {
                    break;
                }
            }
        }

        simulation.ShouldNotBeNull();
        simulation.Id.ShouldBe(simulationId);
        simulation.Status.ShouldBe("SUCCESSFUL", $"Code: {simulation.Result?.Code}; Result: {simulation.Result?.Message}");

        return simulation;
    }

    private async Task<SimulationResponse> SimulateSkillAsync(
        HttpClient client,
        string skillId,
        string stage,
        string content)
    {
        // See https://developer.amazon.com/en-US/docs/alexa/smapi/skill-simulation-api.html#simulate-skill
        var request = new
        {
            session = new { mode = "DEFAULT" },
            input = new { content },
            device = new { locale = "en-GB" },
        };

        using var response = await client.PostAsJsonAsync($"v2/skills/{skillId}/stages/{stage}/simulations", request);

        string? requestId = response.Headers.GetValues("x-amzn-requestid").FirstOrDefault();
        outputHelper.WriteLine($"Request Id: {requestId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var simulation = await response.Content.ReadFromJsonAsync<SimulationResponse>();

        simulation.ShouldNotBeNull();

        outputHelper.WriteLine($"Simulation ID: {simulation.Id}");

        return simulation;
    }

    private sealed class SimulationRequest
    {
        public string? Content { get; set; }

        public string? Locale { get; set; }

        public string? SkillId { get; set; }

        public string? Stage { get; set; }
    }

    private sealed class SimulationResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("result")]
        public SimulationResult? Result { get; set; }
    }

    private sealed class SimulationResult
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("skillExecutionInfo")]
        public SkillExecutionInfo? SkillExecutionInfo { get; set; }

        [JsonPropertyName("error")]
        public SimulationError? Error { get; set; }
    }

    private sealed class SimulationError
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }

    private sealed class SkillExecutionInfo
    {
        [JsonPropertyName("invocations")]
        public IList<SkillInvocation>? Invocations { get; set; }
    }

    private sealed class SkillInvocation
    {
        [JsonPropertyName("invocationRequest")]
        public JsonDocument? InvocationRequest { get; set; }

        [JsonPropertyName("invocationResponse")]
        public JsonDocument? InvocationResponse { get; set; }
    }
}
