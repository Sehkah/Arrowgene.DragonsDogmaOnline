using System;
using Arrowgene.Logging;
using Xunit.Abstractions;

namespace Arrowgene.Ddon.Test
{
    public class TestBase : IDisposable
    {
        private ITestOutputHelper _output;
        private EventHandler<LogWriteEventArgs> _onLogWrite;

        public TestBase(ITestOutputHelper output)
        {
            _output = output;
#if (DEBUG)
            LogProvider.Start();
            _onLogWrite = (sender, args) =>
            {
                try
                {
                    _output.WriteLine(args.Log.ToString());
                }
                catch (InvalidOperationException)
                {
                    // Logged while no test of this class runs: a finalizer, or another test's thread.
                }
            };
            LogProvider.OnLogWrite += _onLogWrite;
#endif
        }

        public void Output(string text)
        {
            _output.WriteLine(text);
        }

        public void Dispose()
        {
#if (DEBUG)
            LogProvider.OnLogWrite -= _onLogWrite;
            LogProvider.Stop();
#endif
        }
    }
}
