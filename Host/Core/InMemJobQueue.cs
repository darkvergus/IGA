using System.Threading.Channels;
using Domain.Jobs;

namespace Host.Core;

public sealed class InMemJobQueue
{
    private readonly Channel<JobEnvelope> channel = Channel.CreateUnbounded<JobEnvelope>();

    public ValueTask EnqueueAsync(JobEnvelope env, CancellationToken cancellationToken = default) => channel.Writer.WriteAsync(env, cancellationToken);

    public IAsyncEnumerable<JobEnvelope> ReadAllAsync(CancellationToken cancellationToken = default) => channel.Reader.ReadAllAsync(cancellationToken);
}