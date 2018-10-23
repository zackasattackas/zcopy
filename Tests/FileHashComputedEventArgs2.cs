using System;
using System.Text;

namespace Tests
{
    public class FileHashComputedEventArgs2 : EventArgs
    {
        private readonly byte[] bytes;

        public FileHashComputedEventArgs2(byte[] hash)
        {
            bytes = hash;
        }

        public string Hash
        {
            get
            {
                var bldr = new StringBuilder();
                foreach (var b in bytes)
                {
                    bldr.Append(b.ToString("x2"));
                }

                return bldr.ToString();
            }
        }
    }
}