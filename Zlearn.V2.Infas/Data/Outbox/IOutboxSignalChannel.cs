using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Zlearn.V2.Infas.Data.Outbox
{
    public interface IOutboxSignalChannel
    {
        void Notify();
        ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);
    }

    public class OutboxSignalChannel : IOutboxSignalChannel
    {
        private readonly Channel<bool> _channel = Channel.CreateUnbounded<bool>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        public void Notify()
        {
            _channel.Writer.TryWrite(true);
        }

        public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.WaitToReadAsync(cancellationToken);
        }
    }
}
