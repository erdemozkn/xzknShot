using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Controls.Primitives;

namespace xzknshot
{
    public partial class MainWindow : Window
    {
        private System.Drawing.Point startPoint;
        private bool isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowState = WindowState.Normal;
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.Topmost = true;

            BitmapSource screenshot = CaptureScreen();
            screenshotImage.Source = screenshot;

            mainGrid.Focus();
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource CaptureScreen()
        {
            int virtualLeft = (int)SystemParameters.VirtualScreenLeft;
            int virtualTop = (int)SystemParameters.VirtualScreenTop;
            int virtualWidth = (int)SystemParameters.VirtualScreenWidth;
            int virtualHeight = (int)SystemParameters.VirtualScreenHeight;

            using (var bmp = new Bitmap(virtualWidth, virtualHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(virtualLeft, virtualTop, 0, 0, bmp.Size);
                }

                IntPtr hBitmap = bmp.GetHbitmap();
                try
                {
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    return bitmapSource;
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        private void UpdateOverlayCutout(double x, double y, double w, double h)
        {
            var fullRect = new RectangleGeometry(new Rect(0, 0, darkOverlay.ActualWidth, darkOverlay.ActualHeight));
            var holeRect = new RectangleGeometry(new Rect(x, y, w, h));
            var combined = new CombinedGeometry(GeometryCombineMode.Exclude, fullRect, holeRect);
            var drawing = new GeometryDrawing(System.Windows.Media.Brushes.White, null, combined);
            darkOverlay.OpacityMask = new DrawingBrush(drawing);
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point startPointWpf = e.GetPosition(selectionCanvas);
            startPoint = new System.Drawing.Point((int)startPointWpf.X, (int)startPointWpf.Y);

            Canvas.SetLeft(selectionRectangle, startPointWpf.X);
            Canvas.SetTop(selectionRectangle, startPointWpf.Y);
            selectionRectangle.Width = 0;
            selectionRectangle.Height = 0;
            selectionRectangle.Visibility = Visibility.Visible;
            menuPanel.Visibility = Visibility.Collapsed;
            resizeThumb.Visibility = Visibility.Collapsed;

            isDragging = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            System.Windows.Point currentPoint = e.GetPosition(selectionCanvas);
            double x = Math.Min(currentPoint.X, startPoint.X);
            double y = Math.Min(currentPoint.Y, startPoint.Y);
            double w = Math.Abs(currentPoint.X - startPoint.X);
            double h = Math.Abs(currentPoint.Y - startPoint.Y);

            Canvas.SetLeft(selectionRectangle, x);
            Canvas.SetTop(selectionRectangle, y);
            selectionRectangle.Width = w;
            selectionRectangle.Height = h;

            UpdateOverlayCutout(x, y, w, h);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isDragging) return;

            isDragging = false;

            double left = Canvas.GetLeft(selectionRectangle);
            double top = Canvas.GetTop(selectionRectangle);
            double width = selectionRectangle.Width;
            double height = selectionRectangle.Height;

            if (width > 0 && height > 0)
            {
                Canvas.SetLeft(resizeThumb, left + width - (resizeThumb.Width / 2));
                Canvas.SetTop(resizeThumb, top + height - (resizeThumb.Height / 2));
                resizeThumb.Visibility = Visibility.Visible;
            }
            else
            {
                resizeThumb.Visibility = Visibility.Collapsed;
            }

            Canvas.SetLeft(menuPanel, left + width - menuPanel.ActualWidth + 10);
            Canvas.SetTop(menuPanel, top + height - menuPanel.ActualHeight - 100);
            menuPanel.Visibility = Visibility.Visible;
            menuPanel.UpdateLayout();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                SS_Click(this, new RoutedEventArgs());
            }
        }

        private void SS_Click(object sender, RoutedEventArgs e)
        {
            int left = (int)Canvas.GetLeft(selectionRectangle);
            int top = (int)Canvas.GetTop(selectionRectangle);
            int width = (int)selectionRectangle.Width;
            int height = (int)selectionRectangle.Height;

            if (width > 0 && height > 0 && screenshotImage.Source is BitmapSource source)
            {
                // Seçilen bölgeyi kırp
                CroppedBitmap croppedImage = new CroppedBitmap(source, new Int32Rect(left, top, width, height));
                Clipboard.SetImage(croppedImage);

                Application.Current.Shutdown();
            }
            else
            {
                MessageBox.Show("Invalid selection area. Please select a region first.");
            }
        }

        private void Math_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Math not implemented yet.");
            Application.Current.Shutdown();
        }

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            int left = (int)Canvas.GetLeft(selectionRectangle);
            int top = (int)Canvas.GetTop(selectionRectangle);
            int width = (int)selectionRectangle.Width;
            int height = (int)selectionRectangle.Height;

            if (width > 0 && height > 0 && screenshotImage.Source is BitmapSource source)
            {
                CroppedBitmap croppedImage = new CroppedBitmap(source, new Int32Rect(left, top, width, height));
                using (var bitmap = ConvertCroppedBitmapToBitmap(croppedImage))
                {
                    string recognizedText = PerformOcr(bitmap);

                    if (!string.IsNullOrWhiteSpace(recognizedText))
                    {
                        Clipboard.SetText(recognizedText);
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        MessageBox.Show("No text recognized in the selected region.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid selection area. Please select a region first.");
            }
        }

        private Bitmap ConvertCroppedBitmapToBitmap(BitmapSource bitmapSource)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);

                stream.Position = 0;
                return new Bitmap(stream);
            }
        }

        private string PerformOcr(Bitmap bitmap)
        {
            string tessDataPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            string textResult = string.Empty;

            using (var engine = new Tesseract.TesseractEngine(tessDataPath, "eng", Tesseract.EngineMode.Default))
            {
                using (var pix = Tesseract.PixConverter.ToPix(bitmap))
                {
                    using (var page = engine.Process(pix))
                    {
                        textResult = page.GetText();
                    }
                }
            }
            return textResult.Trim();
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double currentWidth = selectionRectangle.Width;
            double currentHeight = selectionRectangle.Height;
            double left = Canvas.GetLeft(selectionRectangle);
            double top = Canvas.GetTop(selectionRectangle);

            double newWidth = currentWidth + e.HorizontalChange;
            double newHeight = currentHeight + e.VerticalChange;

            if (newWidth < 20) newWidth = 20;
            if (newHeight < 20) newHeight = 20;

            selectionRectangle.Width = newWidth;
            selectionRectangle.Height = newHeight;

            Canvas.SetLeft(resizeThumb, left + newWidth - (resizeThumb.Width / 2));
            Canvas.SetTop(resizeThumb, top + newHeight - (resizeThumb.Height / 2));

            Canvas.SetLeft(menuPanel, left + newWidth - menuPanel.ActualWidth + 10);
            Canvas.SetTop(menuPanel, top + newHeight - menuPanel.ActualHeight - 100);

            UpdateOverlayCutout(left, top, newWidth, newHeight);
        }
    }
}
