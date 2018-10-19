using System;

namespace BananaHomie.ZCopy.IO
{
    public class MD5VerificationFinishedEventArgs : EventArgs
    {
        public MD5VerificationFinishedEventArgs(bool successful, string checksum)
        {
            Successful = successful;
            Checksum = checksum;
        }

        public bool Successful { get; set; }
        public string Checksum { get; set; }
    }
}