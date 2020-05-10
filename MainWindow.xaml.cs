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
        public const short HUE_IMAGE_TASK = 4;

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
                    /* 処理コストのために画素数を1/4にする */
                    Cv2.Resize(mat, mat, OpenCvSharp.Size.Zero, 0.5, 0.5);
                    //Mat matGray = mat.CvtColor(ColorConversionCodes.BGR2GRAY);

                    // gamma補正値 2.2

                    double LtoGamma = 1 / 2.2;
                    unsafe
                    {
                        MatForeachFunctionVec3b del_grayscale_func;
                        del_grayscale_func = delegate (Vec3b* px, int* position)
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
                        mat.ForEachAsVec3b(del_grayscale_func);

                    };

                    //byte[] image_bytes = new byte[mat.Total()];
                    //Marshal.Copy(mat.Data, image_bytes, 0, image_bytes.Length);
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
        private void hue_image(object sender, RoutedEventArgs e)
        {
            HueButton.IsEnabled = false;
            this.task_flag = Constants.HUE_IMAGE_TASK;
            try
            {

                String base_image_path = "C:\\Users\\owner\\source\\repos\\test_wpf\\Resources\\base_image.jpeg";
                String origin_image_path = "C:\\Users\\owner\\Pictures\\grass.jpg";
                String origin2_image_path = "C:\\Users\\owner\\Pictures\\grass2.jpg";
                File.Copy(origin_image_path, base_image_path, true);
                System.Threading.Thread.Sleep(1);

                while (this.task_flag == Constants.HUE_IMAGE_TASK)
                {


                    DateTime s_dt = DateTime.Now;
                    String image = s_dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String str_date = s_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");
                    String hue_image_path = "C:\\Users\\owner\\source\\repos\\test_wpf\\Resources\\" + image + ".jpeg";

                    listView.Items.Add(new string[] { str_date, "Hue" });
                    Mat current_mat = Cv2.ImRead(origin2_image_path);
                    Mat base_mat = Cv2.ImRead(base_image_path);

                    Cv2.Resize(current_mat, current_mat, OpenCvSharp.Size.Zero, 0.5, 0.5);
                    Cv2.Resize(base_mat, base_mat, OpenCvSharp.Size.Zero, 0.5, 0.5);
                    Cv2.CvtColor(current_mat, current_mat, ColorConversionCodes.BGR2HSV_FULL);
                    Cv2.CvtColor(base_mat, base_mat, ColorConversionCodes.BGR2HSV_FULL); // H: 0-180 S: 0-255 V: 0-255

                    double ave_hue = 0;
                    double ave_saturation = 0;
                    int sum_px = current_mat.Width * current_mat.Height;

                    for (int x = 0; x < current_mat.Width ; x++)
                    {
                        for (int y = 0; y < current_mat.Height; y++)
                        {

                            Vec3b current_px = current_mat.At<Vec3b>(y, x);
                            Vec3b base_px = base_mat.At<Vec3b>(y, x);

                            double current_sin = Math.Sin(current_px.Item0 * Math.PI / 180);
                            double current_cos = Math.Cos(current_px.Item0 * Math.PI / 180);
                            double base_sin = Math.Sin(base_px.Item0 * Math.PI / 180);
                            double base_cos = Math.Cos(base_px.Item0 * Math.PI / 180);

                            double dif_y = (current_px.Item1 * current_sin) - (base_px.Item1 * base_sin);
                            double dif_x = (current_px.Item1 * current_cos) - (base_px.Item1 * base_cos);

                            double dif_hue = 0.0;
                            if (dif_x != 0.0)
                            {
                                dif_hue = Math.Atan(dif_y / dif_x);

                            }
                            double dif_saturation = dif_x / Math.Cos(dif_hue);

                            ave_hue = ave_hue + (dif_hue * 180 / Math.PI);
                            ave_saturation = ave_saturation + dif_saturation;

                        }

                    }
                    ave_hue = ave_hue / sum_px;
                    ave_saturation = ave_saturation / sum_px;

                    DateTime e_dt = DateTime.Now;
                    String end_date = e_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");
                    listView.Items.Add(new string[] { end_date, "Hue Fin" });

                    Console.WriteLine("HUE");
                    Console.WriteLine(ave_hue);
                    Console.WriteLine("SATU");
                    Console.WriteLine((byte)ave_saturation);

                    DoEvents();
                    System.Threading.Thread.Sleep(1000);

                }

            }
            catch
            {
                HueButton.IsEnabled = true;

            }
            finally
            {
                HueButton.IsEnabled = true;

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

