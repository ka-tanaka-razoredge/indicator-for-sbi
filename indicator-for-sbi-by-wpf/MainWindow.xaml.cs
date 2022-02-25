using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Image = System.Windows.Controls.Image;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using System.Runtime.InteropServices;
using System.IO;
using Windows.Storage;
using System.Drawing.Imaging;
using Windows.Storage.Streams;
using BitmapDecoder = Windows.Graphics.Imaging.BitmapDecoder;

namespace indicator_for_sbi_by_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Mp3Player> mp3Players;
        private SoftwareBitmap softwareBitmap;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            mp3Players = new List<Mp3Player>();
            TextBox textBox = FindName("textBox") as TextBox;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (e, s) => { DoRoutine(); };
            timer.Start();
        }

        private async 
        Task
procureSoftwareBitmap(Bitmap bitmap)
        {
            var folder = Directory.GetCurrentDirectory();
            var imageName = "ScreenCapture.bmp";
            StorageFolder appFolder = await StorageFolder.GetFolderFromPathAsync(@folder);
            bitmap.Save(folder + "\\" + imageName, ImageFormat.Bmp);
            var storageFile = await appFolder.GetFileAsync(imageName);

            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            File.Delete(folder + "\\" + imageName);
        }

        private async void DoRoutine()
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan(0, 0, 5);

            for (int i = mp3Players.Count - 1; i >= 0; i--)
            {
                if (mp3Players[i].getCreatedAt() < currentTime - timeSpan)
                {
                    mp3Players.RemoveAt(i);
                }
            }

            System.Diagnostics.Debug.WriteLine("DoRoutine: {0}", mp3Players.Count);


            Rectangle rectangle = new Rectangle(-1190, 265, 80, 40);
//            Rectangle rectangle = new Rectangle(-1190, 240, 100, 50);
            Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
            Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(new Point(rectangle.X, rectangle.Y), new Point(0, 0), bitmap.Size);
            g.Dispose();
            Image image = FindName("image") as Image;
            image.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            OcrEngine ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            await procureSoftwareBitmap(bitmap);
            var memento = await ocrEngine.RecognizeAsync(softwareBitmap);
//            var memento = await ocrEngine.RecognizeAsync(MainWindow.MakeSoftwareBitmap(bitmap));
            TextBox textBox = FindName("textBox") as TextBox;
            if (memento.Text != textBox.Text)
            {
                textBox.Text = memento.Text;
            }
        }

        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        static SoftwareBitmap MakeSoftwareBitmap(System.Drawing.Bitmap bmp)
        {
            unsafe
            {
                var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, bmp.Width, bmp.Height, BitmapAlphaMode.Premultiplied);


                System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                byte* pSrc = (byte*)bd.Scan0;

                using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
                {
                    using (var reference = buffer.CreateReference())
                    {
                        byte* pDest;
                        uint capacity;
                        ((IMemoryBufferByteAccess)reference).GetBuffer(out pDest, out capacity);

                        // Fill-in the BGRA plane
                        BitmapPlaneDescription bl = buffer.GetPlaneDescription(0);
                        for (int y = 0; y < bl.Height; y++)
                        {
                            int blOffset = bl.StartIndex + y * bl.Stride;
                            int yb = y * bd.Stride;
                            for (int x = 0; x < bl.Width; x++)
                            {
                                pDest[blOffset + 4 * x] = pSrc[yb + 4 * x]; // blue
                                pDest[blOffset + 4 * x + 1] = pSrc[yb + 4 * x + 1]; // green
                                pDest[blOffset + 4 * x + 2] = pSrc[yb + 4 * x + 2]; // red
                                pDest[blOffset + 4 * x + 3] = (byte)255; // alpha
                            }
                        }
                    }
                }

                bmp.UnlockBits(bd);

                return softwareBitmap;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mp3Players == null)
            {
                return;
            }
            DateTime currentTime = DateTime.Now;
            string alias = "mp3_" + String.Format("{0:yyyy-MM-dd-HH:mm:ss-fff}", currentTime);
            mp3Players.Add(new Mp3Player("c:\\se_00000100.mp3", alias, currentTime));
            mp3Players[mp3Players.FindIndex(player => player.getAlias() == alias)].Play();
        }
    }
}
