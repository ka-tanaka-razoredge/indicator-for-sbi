using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace indicator_for_sbi_by_wpf
{
    class Mp3Player : IDisposable
    {
        private string alias;
        public string getAlias()
        {
            return alias;
        }

        private DateTime createdAt;
        public DateTime getCreatedAt()
        {
            return createdAt;
        }

        public Mp3Player(string fileName, string alias, DateTime createdAt)
        {
            this.alias = alias;
            this.createdAt = createdAt;
            //            const string FORMAT = "open \"c:\\se_00000100.mp3\" type mpegvideo alias MediaFile";
            string FORMAT = @"open {0} type mpegvideo alias " + this.alias;
            //            const string FORMAT = @"open ""{0}"" type mpegvideo alias MediaFile";
            string command = String.Format(FORMAT, fileName);
            System.Diagnostics.Debug.WriteLine(mciSendString(command, null, 0, IntPtr.Zero));
            System.Diagnostics.Debug.WriteLine(command);
        }

        public void Play()
        {
            string command = "play " + this.alias;
            mciSendString(command, null, 0, IntPtr.Zero);
            System.Diagnostics.Debug.WriteLine("Mp3Player.Play()");
        }

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand,
            StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        public void Dispose()
        {
            string command = "close " + this.alias;
            mciSendString(command, null, 0, IntPtr.Zero);
        }
    }
}
