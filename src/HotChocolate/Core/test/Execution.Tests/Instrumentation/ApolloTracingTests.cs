using System;
using System.Threading.Tasks;
using HotChocolate.Execution.Options;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit;
using static HotChocolate.Tests.TestHelper;

namespace HotChocolate.Execution.Instrumentation;

public class DiagnosticsEventsTests
{
    [Fact]
    public async Task ApolloTracing_Always()
    {
        // arrange
        var executor = await CreateExecutorAsync(c => c
            .AddDocumentFromString(
                @"
                    type Query {
                        a: String
                    }")
            .AddResolver("Query", "a", () => "hello world a")
            .AddApolloTracing(TracingPreference.Always, new TestTimestampProvider()));

        // act
        var result = await executor.ExecuteAsync("{ a }");

        // assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task ApolloTracing_Always_Parsing_And_Validation_Is_Cached()
    {
        // arrange
        var executor = await CreateExecutorAsync(c => c
            .AddDocumentFromString(
                @"
                    type Query {
                        a: String
                    }")
            .AddResolver("Query", "a", () => "hello world a")
            .AddApolloTracing(TracingPreference.Always, new TestTimestampProvider()));

        // act
        await executor.ExecuteAsync("{ a }");

        // the second execution will not parse or validate since these steps are cached.
        var result = await executor.ExecuteAsync("{ a }");

        // assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task ApolloTracing_OnDemand_NoHeader()
    {
        // arrange
        var executor = await CreateExecutorAsync(c => c
            .AddDocumentFromString(
                @"
                    type Query {
                        a: String
                    }")
            .AddResolver("Query", "a", () => "hello world a")
            .AddApolloTracing(TracingPreference.OnDemand, new TestTimestampProvider()));

        // act
        var result = await executor.ExecuteAsync("{ a }");

        // assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task ApolloTracing_OnDemand_WithHeader()
    {
        // arrange
        var executor = await CreateExecutorAsync(c => c
            .AddDocumentFromString(
                @"
                    type Query {
                        a: String
                    }")
            .AddResolver("Query", "a", () => "hello world a")
            .AddApolloTracing(TracingPreference.OnDemand, new TestTimestampProvider()));

        // act
        var result = await executor.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ a }")
                .SetGlobalState(WellKnownContextData.EnableTracing, true)
                .Create());

        // assert
        result.ToJson().MatchSnapshot();
    }

    private sealed class TestTimestampProvider : ITimestampProvider
    {
        private DateTime _utcNow = new DateTime(2010, 10, 10, 12, 00, 00);
        private long _nowInNanoseconds = 10;

        public DateTime UtcNow()
        {
            var time = _utcNow;
            _utcNow = _utcNow.AddMilliseconds(50);
            return time;
        }

        public long NowInNanoseconds()
        {
            return _nowInNanoseconds += 20;
        }
    }
}
