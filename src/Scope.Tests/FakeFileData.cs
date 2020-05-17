using System;
using System.IO;
using System.Text;
using Scope.Zip.Zip;

namespace Scope.Tests
{
    public class FakeFileData : IStaticDataSource, IDisposable
    {
        private readonly string _content;
        private Stream _stream;

        public FakeFileData(string content)
        {
            _content = content;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public Stream GetSource()
        {
            _stream = new MemoryStream(Encoding.Default.GetBytes(_content));

            return _stream;
        }
    }
}
