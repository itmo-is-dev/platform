using FluentAssertions;
using Itmo.Dev.Platform.Grpc.Clients;
using Itmo.Dev.Platform.Grpc.Services;
using Itmo.Dev.Platform.Observability.Tests.Startup;
using Itmo.Dev.Platform.Observability.Tests.Startup.Controllers;
using Itmo.Dev.Platform.Observability.Tests.Startup.Models;
using Itmo.Dev.Platform.Observability.Tests.Startup.Tools;
using Itmo.Dev.Platform.Testing.ApplicationFactories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Observability.Tests;

#pragma warning disable CA1506

public class GrpcHeaderPropagationTests
{
    private readonly ITestOutputHelper _output;

    public GrpcHeaderPropagationTests(ITestOutputHelper output)
    {
        _output = output;

        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Fact]
    public async Task HelloAsync_ShouldHaveSameTraceParent_WhenCalledFromHttpService()
    {
        // Arrange
        var assemblyLocation = typeof(TestingConstants).Assembly.Location;

        var grpcServiceSpanFileName = Path.GetTempFileName();
        var httpServiceSpanFileName = Path.GetTempFileName();

        using var grpcServiceProcess = new Process();
        using var httpServiceProcess = new Process();

        grpcServiceProcess.StartInfo = new ProcessStartInfo("dotnet", [assemblyLocation])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            EnvironmentVariables =
            {
                [TestingConstants.IsGrpcServiceEnvVariableName] = "true",
                [TestingConstants.SpanIdFileVariableName] = grpcServiceSpanFileName,
            },
        };

        httpServiceProcess.StartInfo = new ProcessStartInfo("dotnet", [assemblyLocation])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            EnvironmentVariables =
            {
                [TestingConstants.IsGrpcServiceEnvVariableName] = "false",
                [TestingConstants.SpanIdFileVariableName] = httpServiceSpanFileName,
            },
        };

        httpServiceProcess.Start();
        grpcServiceProcess.Start();

        using var grpcServiceLogs = new ProcessLogCollector("GRPC", grpcServiceProcess, _output);
        using var httpServiceLogs = new ProcessLogCollector("HTTP", httpServiceProcess, _output);

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        using var client = new HttpClient();

        // Act
        await client.GetAsync($"{TestingConstants.HttpServiceUrl}/hello?name=ronimizy");
        grpcServiceProcess.Kill();
        httpServiceProcess.Kill();

        // Assert
        var grpcServiceSpans = await File.ReadAllLinesAsync(grpcServiceSpanFileName);
        var httpServiceSpansText = await File.ReadAllLinesAsync(httpServiceSpanFileName);

        var httpServiceSpanInfos = httpServiceSpansText
            .Select(JsonConvert.DeserializeObject<SpanInfo>)
            .ToArray();

        var httpServiceSpans = httpServiceSpanInfos.Select(x => x?.SpanId).ToArray();

        File.Delete(grpcServiceSpanFileName);
        File.Delete(httpServiceSpanFileName);

        Log.Information("Grpc span ids: {SpanIds}", string.Join(", ", grpcServiceSpans));
        Log.Information("Http span ids: {SpanIds}", string.Join(", ", httpServiceSpansText));

        grpcServiceSpans.Should().AllSatisfy(grpcSpan => httpServiceSpans.Should().Contain(grpcSpan));
    }

    private class ProcessLogCollector : IDisposable
    {
        private readonly StringBuilder _builder;
        private readonly ITestOutputHelper _output;
        private readonly string _serviceName;

        private readonly Lock _lock = new();

        public ProcessLogCollector(string serviceName, Process process, ITestOutputHelper output)
        {
            _serviceName = serviceName;
            _output = output;
            _builder = new StringBuilder();

            _ = WriteStream(process.StandardOutput);
            _ = WriteStream(process.StandardError);
        }

        private async Task WriteStream(StreamReader reader)
        {
            try
            {
                while (true)
                {
                    string? line = await reader.ReadLineAsync();

                    if (line is null)
                        break;

                    lock (_lock)
                    {
                        _builder.AppendLine(line);
                    }
                }
            }
            catch
            {
                // Ignore error
            }
        }

        public void Dispose()
        {
            const int paddingSize = 10;
            var startPadding = string.Join("", Enumerable.Repeat('=', paddingSize));
            var endPadding = string.Join("", Enumerable.Repeat('=', (paddingSize * 2) + 4 + _serviceName.Length));

            _output.WriteLine($"{startPadding}  {_serviceName}  {startPadding}");
            _output.WriteLine(_builder.ToString());
            _output.WriteLine(endPadding);
        }
    }
}
