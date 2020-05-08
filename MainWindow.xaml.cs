using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using Reactive.Bindings;
using OpenCvSharp;


namespace test_wpf
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    static class Constants
    {
        public const short SHOW_IMAGE_TASK= 1;
        public const short DELETE_IMAGE_TASK = 2;
        public const short GRAY_IMAGE_TASK = 3;

    }

    public partial class MainWindow : System.Windows.Window
    {
        //Reactive Propertyは結局使い方がよくわからなかった. 描写できていない
        public ReactiveProperty<WriteableBitmap> rp_image { set; get; } = new ReactiveProperty<WriteableBitmap>();
        //Task Flag
        public int task_flag = 0;


        public MainWindow()
        {
            InitializeComponent();

        }

        private void show_image(object sender, RoutedEventArgs e)
        {
            ShowButton.IsEnabled = false;
            this.task_flag = Constants.SHOW_IMAGE_TASK;
            try
            {

                while (this.task_flag == Constants.SHOW_IMAGE_TASK)
                {
                    DateTime dt = DateTime.Now;
                    String image = dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String str_date = dt.ToString("yyyy/MM/dd-HH:mm:ss");

                    listView.Items.Add(new string[] { str_date, "Show" });
                    String show_image_path = "C:\\Users\\owner\\source\\repos\\test_wpf\\Resources\\" + image + ".jpeg";

                    File.Copy("C:\\Users\\owner\\Pictures\\shizuoka_outlet.jpg", show_image_path, true);

                    //lock less bitmap
                    MemoryStream data = new MemoryStream(File.ReadAllBytes(show_image_path));
                    WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                    data.Close();
                    this.MainImage.Source = wbmp;
                    File.Delete(show_image_path);
                    Console.WriteLine(this.task_flag);
                    DoEvents();
                    System.Threading.Thread.Sleep(1);

                }
            }
            catch
            {
                ShowButton.IsEnabled = true;

            }
            finally
            {
                ShowButton.IsEnabled = true;

            }

        }

        private void delete_image(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            listView.Items.Add(new string[] { dt.ToString("yyyy/MM/dd HH:mm:ss"), "Delete" });

            this.task_flag = Constants.DELETE_IMAGE_TASK;
            var source = new BitmapImage();
            MainImage.Source = source;
            Console.WriteLine(this.task_flag);

        }

        private void grayscale_image(object sender, RoutedEventArgs e)
        {
            GrayButton.IsEnabled = false;
            this.task_flag = Constants.GRAY_IMAGE_TASK;
            try
            {
                while (this.task_flag == Constants.GRAY_IMAGE_TASK)
                {

                    DateTime s_dt = DateTime.Now;
                    String image = s_dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String str_date = s_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");

                    String grayscale_image_path = "C:\\Users\\owner\\source\\repos\\test_wpf\\Resources\\" + image + ".jpeg";
                    String origin_image_path = "C:\\Users\\owner\\Pictures\\shizuoka_outlet.jpg";

                    Mat mat = Cv2.ImRead(origin_image_path);
                    //Mat matGray = mat.CvtColor(ColorConversionCodes.BGR2GRAY);

                    // gamma補正値 2.2

                    double LtoGamma = 1 / 2.2;
                    unsafe
                    {
                        MatForeachFunctionVec3b del_func;
                        del_func = delegate (Vec3b* px, int* position)
                        {
                            // BGRを逆ガンマ補正（低輝度部分の差は捨てる）
                            double lpx_b = Math.Pow((double)px->Item0 / 255.0, 2.2) * 255;
                            double lpx_g = Math.Pow((double)px->Item1 / 255.0, 2.2) * 255;
                            double lpx_r = Math.Pow((double)px->Item2 / 255.0, 2.2) * 255;

                            //BT.601 V = 0.183R + 0.614G + 0.062B + 16
                            //BT.709 V = 0.2126 * R + 0.7152 * G + 0.0722 * B

                            // Y信号化 => BT.709
                            double linear_y_sig = 0.2126 * lpx_r + 0.7152 * lpx_g + 0.0722 * lpx_b;

                            // Gamma補正
                            double gamma_y_sig = Math.Pow((double)linear_y_sig / 255.0, LtoGamma) * 255;
                            // 放送規格のHD-SDIならば 1bit per 0.45662100456%のY信号レベル, 70%は16+153.3=169.3
                            //画像規格sRGBならば255階調なので、
                            if (gamma_y_sig > 169.3)
                            {
                                    px->Item2 = 240;

                            }
                            else if (gamma_y_sig < 26)
                            {
                                px->Item0 = 200;

                            }
                            else
                            {
                                px->Item0 = (byte)gamma_y_sig;
                                px->Item1 = (byte)gamma_y_sig;
                                px->Item2 = (byte)gamma_y_sig;

                            }
                        };
                        mat.ForEachAsVec3b(del_func);

                    };

                    byte[] image_bytes = new byte[mat.Total()];
                    Marshal.Copy(mat.Data, image_bytes, 0, image_bytes.Length);
                    Cv2.ImWrite(grayscale_image_path, mat);

                    //lock less bitmap
                    MemoryStream data = new MemoryStream(File.ReadAllBytes(grayscale_image_path));
                    WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                    data.Close();
                    this.MainImage.Source = wbmp;
                    //rp_image.Value = wbmp;

                    DateTime e_dt = DateTime.Now;
                    String end_date = e_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");
                    listView.Items.Add(new string[] { str_date, "Gray" });
                    listView.Items.Add(new string[] { end_date, "Gray Fin" });

                    File.Delete(grayscale_image_path);
                    Console.WriteLine(this.task_flag);
                    DoEvents();
                    System.Threading.Thread.Sleep(1);

                }
            }
            catch
            {
                GrayButton.IsEnabled = true;

            }
            finally
            {
                GrayButton.IsEnabled = true;

            }

        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // ↓ ここから独自開発のDoEvents()
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrames(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }

    }
}

