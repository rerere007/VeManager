using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
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
using System.Runtime.InteropServices;
using OpenCvSharp;


namespace test_wpf
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>

   
public partial class MainWindow : System.Windows.Window
    {

        public MainWindow()
        {
            InitializeComponent();

        }

        private void show_image(object sender, RoutedEventArgs e)
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

        }

        private void delete_image(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            listView.Items.Add(new string[] { dt.ToString("yyyy/MM/dd HH:mm:ss"), "Delete" });

            var source = new BitmapImage();
            MainImage.Source = source;

        }

        private async void grayscale_image(object sender, RoutedEventArgs e)
        {
            GrayButton.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    while (true)
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


                        DateTime e_dt = DateTime.Now;
                        String end_date = e_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            listView.Items.Add(new string[] { str_date, "Gray" });
                            listView.Items.Add(new string[] { end_date, "Gray Fin" });
                            this.MainImage.Source = wbmp;

                        }));

                        //File.Delete(grayscale_image_path);
                        System.Threading.Thread.Sleep(1000 / 60);

                    }
                });
            }
            catch
            {

            }
            finally
            {
                GrayButton.IsEnabled = true;

            }


        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

