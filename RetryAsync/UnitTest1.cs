using FluentAssertions;
using LanguageExt;
using NSubstitute;
using Polly;
using Polly.Retry;

namespace RetryAsync;

public class UnitTest1
{
    private ICanFail canFail = Substitute.For<ICanFail>();

    public UnitTest1()
    {
        canFail.ReturnEither().Returns(0, 0, "Ok");
    }

    [Fact]
    public void ShouldRetry()
    {
        RetryStrategyOptions<EitherAsync<int, string>> asyncRetryStrategyOptions = new()
        {
            ShouldHandle = async args => await args.Outcome.Result.IsLeft,
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(1)
        };

        ResiliencePipeline<EitherAsync<int, string>> asyncRetryPipeline =
            new ResiliencePipelineBuilder<EitherAsync<int, string>>()
            .AddRetry<EitherAsync<int, string>>(asyncRetryStrategyOptions).Build();

        EitherAsync<int, string> actual = 
            asyncRetryPipeline.Execute<EitherAsync<int, string>>(_ => canFail.ReturnEither());
        actual.Match(
            s => s.Should().Be("Ok"),
            _ => false.Should().BeTrue()
            );
    }

}

public interface ICanFail
{
    EitherAsync<int, string> ReturnEither();
}

