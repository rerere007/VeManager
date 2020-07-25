using OpenCvSharp; //OpenCvSharp4.windows
using Reactive.Bindings;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;




namespace VeManagerApp
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    static class Constants
    {
        public const short NO_TASK = 0;
        public const short SHOW_IMAGE_TASK = 1;
        public const short DELETE_IMAGE_TASK = 2;
        public const short GRAY_IMAGE_TASK = 3;
        public const short HUE_IMAGE_TASK = 4;
        public const short FACE_IMAGE_TASK = 5;

    }

    public partial class MainWindow : System.Windows.Window
    {
        //Reactive Propertyは結局使い方がよくわからなかった. 描写できていない
        public ReactiveProperty<WriteableBitmap> rp_image { set; get; } = new ReactiveProperty<WriteableBitmap>();
        //Task Flag
        public int task_flag = Constants.NO_TASK;
        public string resource_dir = "C:\\Users\\owner\\source\\repos\\VeManager\\Resources\\";
        //public string captured_image_dir = "R:\\Temp\\";
        public string captured_image_dir = "C:\\Users\\owner\\source\\repos\\VeManager\\Image\\";
        string filename;
        string texts;
        String current_app_dir = GetCurrentAppDir();
        CascadeClassifier cascade = new CascadeClassifier();
        private object task_flag_key = new object();

        public MainWindow()
        {
            string config_db_path = current_app_dir + "\\" + "ConfigDB";
            string cascade_dir = current_app_dir + "\\" + "cascade";

            SafeCreateDirectory(config_db_path);
            SafeCreateDirectory(cascade_dir);

            try
            {

                String cascade_path = cascade_dir + "\\" + "haarcascade_frontalface_alt2.xml";
                cascade.Load(cascade_path);

            }
            catch (Exception cascade_error)
            {
                Console.WriteLine(cascade_error);
                Console.WriteLine("Cascade分類機が開けません");
                System.Threading.Thread.Sleep(10000);

            }
            InitializeComponent();


        }

        private void show_image(object sender, RoutedEventArgs e)
        {
            ShowButton.IsEnabled = false;
            lock (task_flag_key)
            {
                this.task_flag = Constants.SHOW_IMAGE_TASK;
            }

            try
            {

                while (this.task_flag == Constants.SHOW_IMAGE_TASK)
                {
                    DateTime dt = DateTime.Now;
                    String image = dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    Mat mat;

                    String show_image_path = resource_dir + image + ".png";
                    String current_image_path = captured_image_dir + getNewestFileName(captured_image_dir);

                    try
                    {
                        Console.WriteLine("break point A");
                        mat = Cv2.ImRead(current_image_path);
                        Console.WriteLine("break point B");

                    }
                    catch (Exception opencv_read_error)
                    {
                        Console.WriteLine(opencv_read_error);
                        Console.WriteLine("Catched the exception in line 83");
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        /* 処理コストのために画素数を1/16にする */
                        Cv2.Resize(mat, mat, OpenCvSharp.Size.Zero, 0.334, 0.334);
                    }
                    catch (Exception opencv_resize_error)
                    {
                        Console.WriteLine(opencv_resize_error);
                        Console.WriteLine("Catched the exception in line 97");
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        Cv2.ImWrite(show_image_path, mat);
                    }
                    catch (Exception opencv_write_error)
                    {
                        Console.WriteLine(opencv_write_error);
                        Console.WriteLine("Catched the exception in line 205");
                        DoEvents();
                        continue;

                    }


                    //lock less bitmap
                    MemoryStream data = new MemoryStream(File.ReadAllBytes(show_image_path));
                    WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                    data.Close();
                    this.MainImage.Source = wbmp;
                    File.Delete(show_image_path);
                    Console.WriteLine(this.task_flag);
                    DoEvents();


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
            lock (task_flag_key)
            {
                this.task_flag = Constants.DELETE_IMAGE_TASK;
            }

            var source = new BitmapImage();
            MainImage.Source = source;
            BaseImage.Source = source;
            HueTextRed.Inlines.Clear();
            HueTextBlue.Inlines.Clear();
            HueText.Inlines.Clear();

        }

        private void grayscale_image(object sender, RoutedEventArgs e)
        {
            GrayButton.IsEnabled = false;
            double conv_percent_to_sig = 0.39215686274;
            lock (task_flag_key)
            {
                this.task_flag = Constants.GRAY_IMAGE_TASK;
            }

            try
            {
                while (this.task_flag == Constants.GRAY_IMAGE_TASK)
                {

                    double LeftBlue = 0;
                    double RightBlue = 0;
                    double LeftRed = 0;
                    double RightRed = 0;
                    double LeftGreen = 0;
                    double RightGreen = 0;

                    try
                    {
                        LeftBlue = int.Parse(textLeftBlue.Text) / conv_percent_to_sig;
                        RightBlue = int.Parse(textRightBlue.Text) / conv_percent_to_sig;
                        LeftRed = int.Parse(textLeftRed.Text) / conv_percent_to_sig;
                        RightRed = int.Parse(textRightRed.Text) / conv_percent_to_sig;
                        LeftGreen = int.Parse(textLeftGreen.Text) / conv_percent_to_sig;
                        RightGreen = int.Parse(textRightGreen.Text) / conv_percent_to_sig;

                    }
                    catch (Exception catch_color_border_data)
                    {
                        Console.WriteLine(catch_color_border_data);

                    }

                    DateTime s_dt = DateTime.Now;
                    String image = s_dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String str_date = s_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");
                    Console.WriteLine("break point -2");

                    Mat mat;
                    String grayscale_image_path = resource_dir + image + ".png";
                    String current_image_path;
                    Console.WriteLine("break point -1");
                    

                    try
                    {
                        current_image_path = captured_image_dir + getNewestFileName(captured_image_dir);
                    }
                    catch (Exception get_name_error)
                    {
                        Console.WriteLine(get_name_error);
                        Console.WriteLine("Catched the exception in line 140");
                        DoEvents();
                        continue;

                    }
                    Console.WriteLine("break point 0");

                    try
                    {
                        Console.WriteLine("break point A");
                        mat = Cv2.ImRead(current_image_path);
                        Console.WriteLine("break point B");

                    }
                    catch (Exception opencv_read_error)
                    {
                        Console.WriteLine(opencv_read_error);
                        Console.WriteLine("Catched the exception in line 188");
                        DoEvents();
                        continue;

                    }
                    Console.WriteLine("break point 1");
                    try
                    {
                        /* 処理コストのために画素数を1/16にする */
                        Cv2.Resize(mat, mat, OpenCvSharp.Size.Zero, 0.334, 0.334);
                        //Mat matGray = mat.CvtColor(ColorConversionCodes.BGR2GRAY);

                    }
                    catch (Exception opencv_resize_error)
                    {
                        Console.WriteLine(opencv_resize_error);
                        Console.WriteLine("Catched the exception in line 205");
                        DoEvents();
                        continue;

                    }
                    Console.WriteLine("break point 2");


                    // gamma補正値 2.2
                    //double LtoGamma = 1 / 2.2;

                    Console.WriteLine("break point 3");
                    Console.WriteLine("画素値[0, 0]の出力(B)" + mat.At<Vec3b>(0,0)[0]);
                    Console.WriteLine("画素値[0, 0]の出力(G)" + mat.At<Vec3b>(0,0)[1]);
                    Console.WriteLine("画素値[0, 0]の出力(R)" + mat.At<Vec3b>(0,0)[2]);

                    unsafe
                    {
                        MatForeachFunctionVec3b del_grayscale_func;
                        del_grayscale_func = delegate (Vec3b* px, int* position)
                        {
                            /*ガンマ返還を考慮した式*/
                            // BGRを逆ガンマ補正（低輝度部分の差は捨てる）
                            //double lpx_b = Math.Pow((double)px->Item0 / 255.0, 2.2) * 255;
                            //double lpx_g = Math.Pow((double)px->Item1 / 255.0, 2.2) * 255;
                            //double lpx_r = Math.Pow((double)px->Item2 / 255.0, 2.2) * 255;

                            //BT.601 V = 0.183R + 0.614G + 0.062B + 16
                            //BT.709 V = 0.2126 * R + 0.7152 * G + 0.0722 * B

                            // Y信号化 => BT.709
                            //double linear_y_sig = 0.2126 * lpx_r + 0.7152 * lpx_g + 0.0722 * lpx_b;

                            // Gamma補正
                            //double gamma_y_sig = Math.Pow((double)linear_y_sig / 255.0, LtoGamma) * 255;
                            // 放送規格のHD-SDIならば 1bit per 0.45662100456%のY信号レベル, 70%は16+153.3=169.3
                            //画像規格sRGBならば255階調なので、100% / 255 = 0.39215686%

                            /* Gamma処理を行わずに計算した場合(DeckLinkがPNGの際にやってくれている?) */
                            double linear_y_sig = 0.2126 * (double)px->Item2 + 0.7152 * (double)px->Item1 + 0.0722 * (double)px->Item0;

                            if (LeftRed >= linear_y_sig && linear_y_sig >= RightRed)
                            {
                                px->Item0 = 0;
                                px->Item1 = 0;
                                px->Item2 = 240;

                            }
                            else if (LeftBlue >= linear_y_sig && linear_y_sig >= RightBlue)
                            {
                                px->Item0 = 200;
                                px->Item1 = 0;
                                px->Item2 = 0;

                            }
                            else if (LeftGreen >= linear_y_sig && linear_y_sig >= RightGreen)
                            {
                                px->Item0 = 0;
                                px->Item1 = 180;
                                px->Item2 = 0;
                            }
                            else //GrayScale
                            {
                                px->Item0 = (byte)linear_y_sig;
                                px->Item1 = (byte)linear_y_sig;
                                px->Item2 = (byte)linear_y_sig;

                            }
                        };
                        mat.ForEachAsVec3b(del_grayscale_func);

                    };
                    Console.WriteLine("break point 4");


                    //byte[] image_bytes = new byte[mat.Total()];
                    //Marshal.Copy(mat.Data, image_bytes, 0, image_bytes.Length);
                    try
                    {
                        Cv2.ImWrite(grayscale_image_path, mat);
                    }
                    catch (Exception opencv_write_error)
                    {
                        Console.WriteLine(opencv_write_error);
                        Console.WriteLine("Catched the exception in line 205");
                        DoEvents();
                        continue;

                    }
                    Console.WriteLine("break point 5");

                    //lock less bitmap
                    MemoryStream data = new MemoryStream(File.ReadAllBytes(grayscale_image_path));
                    WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                    data.Close();
                    this.MainImage.Source = wbmp;
                    //rp_image.Value = wbmp;
                    Console.WriteLine("break point 6");


                    DateTime e_dt = DateTime.Now;
                    String end_date = e_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");
                    Console.WriteLine("break point 7");

                    File.Delete(grayscale_image_path);
                    Console.WriteLine(this.task_flag);
                    DoEvents();
                    System.Threading.Thread.Sleep(100);
                    Console.WriteLine("break point 8");


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

        private void face_image(object sender, RoutedEventArgs e)
        {
            FaceButton.IsEnabled = false;
            lock (task_flag_key)
            {
                this.task_flag = Constants.FACE_IMAGE_TASK;
            }

            try
            {
                while (this.task_flag == Constants.FACE_IMAGE_TASK)
                {
                    DateTime dt = DateTime.Now;
                    String image = dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String str_date = dt.ToString("yyyy/MM/dd-HH:mm:ss");
                    Mat OriginMat;
                    Mat GrayMat;
                    Mat FaceMat;
                    OpenCvSharp.Rect[] FaceRects;
                    //bool detect_flag = false;

                    String face_image_path = resource_dir + image + ".png";
                    String current_image_path = captured_image_dir + getNewestFileName(captured_image_dir);

                    try
                    {
                        OriginMat = Cv2.ImRead(current_image_path);

                    }
                    catch (Exception opencv_read_error)
                    {
                        Console.WriteLine(opencv_read_error);
                        Console.WriteLine("Catched the exception in line 83");
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        /* 処理コストのために画素数を1/16にする */
                        Cv2.Resize(OriginMat, OriginMat, OpenCvSharp.Size.Zero, 0.334, 0.334);
                    }
                    catch (Exception opencv_resize_error)
                    {
                        Console.WriteLine(opencv_resize_error);
                        Console.WriteLine("Catched the exception in line 97");
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        GrayMat = OriginMat.CvtColor(ColorConversionCodes.BGR2GRAY);

                    }
                    catch (Exception opencv_cvt_error)
                    {
                        Console.WriteLine(opencv_cvt_error);
                        Console.WriteLine("Catched the exception in line 434");
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        FaceRects = cascade.DetectMultiScale(GrayMat);

                    }
                    catch (Exception face_detect_error)
                    {
                        Console.WriteLine(face_detect_error);
                        Console.WriteLine("Catched the exception in line aa");
                        DoEvents();
                        continue;

                    }

                    Console.WriteLine("Break Point");

                    if (FaceRects.Length > 0)
                    {
                        //detect_flag = true;
                        Console.WriteLine("Face Detection Done.");
                        OpenCvSharp.Rect FaceRect = FaceRects[0];
                        FaceMat = OriginMat.Clone(FaceRect);

                        try
                        {
                            Cv2.ImWrite(face_image_path, FaceMat);

                        }
                        catch (Exception opencv_write_error)
                        {
                            Console.WriteLine(opencv_write_error);
                            Console.WriteLine("Catched the exception in line 205");
                            DoEvents();
                            continue;

                        }

                    }
                    else
                    {
                        Console.WriteLine("No Detect");
                        DoEvents();
                        System.Threading.Thread.Sleep(100);
                        continue;

                    }

                    

                    //face image
                    MemoryStream data = new MemoryStream(File.ReadAllBytes(face_image_path));
                    WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                    data.Close();
                    this.BaseImage.Source = wbmp;

                    File.Delete(face_image_path);
                    Console.WriteLine(this.task_flag);
                    DoEvents();


                }
            }
            catch
            {
                FaceButton.IsEnabled = true;

            }
            finally
            {
                FaceButton.IsEnabled = true;

            }
        }

        
        private void hue_image(object sender, RoutedEventArgs e)
        {
            HueButton.IsEnabled = false;
            double hue_comp_rate = 0.5;
            lock (task_flag_key)
            {
                this.task_flag = Constants.HUE_IMAGE_TASK;
            }

            try
            {

                String base_image_path = resource_dir + "base_image.png";
                String current_image_path = captured_image_dir + getNewestFileName(captured_image_dir);

                Mat cap_mat = Cv2.ImRead(current_image_path);
                Cv2.Resize(cap_mat, cap_mat, OpenCvSharp.Size.Zero, hue_comp_rate, hue_comp_rate);
                Cv2.ImWrite(base_image_path, cap_mat);

                //lock less bitmap
                MemoryStream base_data = new MemoryStream(File.ReadAllBytes(base_image_path));
                WriteableBitmap base_wbmp = new WriteableBitmap(BitmapFrame.Create(base_data));
                base_data.Close();
                this.BaseImage.Source = base_wbmp;
                System.Threading.Thread.Sleep(1);

                Mat base_mat = new Mat();
                try
                {
                    base_mat = Cv2.ImRead(base_image_path);

                }
                catch (Exception opencv_read_error)
                {
                    Console.WriteLine(opencv_read_error);
                    Console.WriteLine("Catched the exception in line 280");
                    DoEvents();

                }

                HueText.Inlines.Clear();
                HueText.FontSize = 24;
                HueText.Inlines.Add(new Bold(new Run("基準画像と現在画像の色ズレ")));
                HueText.TextAlignment = TextAlignment.Center;

                while (this.task_flag == Constants.HUE_IMAGE_TASK)
                {

                    DateTime s_dt = DateTime.Now;
                    String image = s_dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String str_date = s_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");
                    String hue_image_path = resource_dir + image + ".png";
                    current_image_path = captured_image_dir + getNewestFileName(captured_image_dir);
                    Mat current_mat;
                    Mat differ_mat = new Mat(0, 0, OpenCvSharp.MatType.CV_64FC3);

                    int sum_px = 0;
                    double ave_X_point = 0;
                    double ave_Y_point = 0;

                    try
                    {
                        current_mat = Cv2.ImRead(current_image_path);

                    }
                    catch(Exception opencv_read_error)
                    {
                        Console.WriteLine(opencv_read_error);
                        Console.WriteLine("Catched the exception in line 280");
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        Cv2.Resize(current_mat, current_mat, OpenCvSharp.Size.Zero, hue_comp_rate, hue_comp_rate);
                    }
                    catch (Exception current_mat_resize_error)
                    {
                        Console.WriteLine(current_mat_resize_error);
                        Console.WriteLine("Catched the exception in line 396");
                        DoEvents();
                        continue;

                    }
                    
                    try
                    {
                        unsafe
                        {
                            
                            MatForeachFunctionVec3b del_hue_detect_func;
                            del_hue_detect_func = delegate (Vec3b* px, int* position)
                            {

                                // Gamma処理を行わずに計算した場合(DeckLinkがPNGの際にやってくれている?) .. BGRに注意
                                
                                byte b_px = px->Item0;
                                byte g_px = px->Item1;
                                byte r_px = px->Item2;

                                //Y, R-Y, B-Yを計算
                                double linear_y_sig = 0.2126 * r_px + 0.7152 * g_px + 0.0722 * b_px;
                                double R_Y = r_px - linear_y_sig;
                                double B_Y = b_px - linear_y_sig;

                                if ((R_Y < 0) && (B_Y > 0))
                                {
                                    ;
                                }
                                else
                                {
                                    px->Item0 = 0;
                                    px->Item1 = 0;
                                    px->Item2 = 0;

                                }

                            };
                            current_mat.ForEachAsVec3b(del_hue_detect_func);
                            base_mat.ForEachAsVec3b(del_hue_detect_func);

                        };
                    }
                    catch(Exception cal_error)
                    {
                        Console.WriteLine(cal_error);
                        Console.WriteLine("line 671");
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        Cv2.ImWrite(hue_image_path, current_mat);

                    }
                    catch (Exception opencv_write_error)
                    {
                        Console.WriteLine(opencv_write_error);
                        Console.WriteLine("Catched the exception in line 295");
                        DoEvents();
                        continue;

                    }

                    //lock less bitmap
                    MemoryStream data = new MemoryStream(File.ReadAllBytes(hue_image_path));
                    WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                    data.Close();
                    this.MainImage.Source = wbmp;
                    File.Delete(hue_image_path);


                    //差分計算
                    try
                    {
                        differ_mat = current_mat - base_mat;

                    }
                    catch (Exception dif_error)
                    {
                        Console.WriteLine(dif_error);
                        Console.WriteLine("line 684");
                        DoEvents();
                        continue;

                    }

                    for (int y = 0; y < differ_mat.Rows; y++)
                    {
                        for (int x = 0; x < differ_mat.Cols; x++)
                        {
                            Vec3d point = differ_mat.At<Vec3d>(y, x);
                            Vec3b current_point = current_mat.At<Vec3b>(y, x);
                            double r_px = point.Item2;
                            double g_px = point.Item1;
                            double b_px = point.Item0;

                            //Y, R-Y, B-Yを計算
                            double linear_y_sig = 0.2126 * r_px + 0.7152 * g_px + 0.0722 * b_px;
                            double R_Y = r_px - linear_y_sig;
                            double B_Y = b_px - linear_y_sig;

                            //該当のPXの場合にはインクリメントし、計算
                            if ((current_point.Item1 == 0) && (current_point.Item2 == 0))
                            {
                                sum_px++;
                                ave_Y_point = ave_Y_point + R_Y;
                                ave_X_point = ave_X_point + B_Y;

                            }
                        }
                    }


                    //differ_matに対して平均を計算
                    /*
                    Cv2.Reduce(differ_mat, differ_mat, OpenCvSharp.ReduceDimension.Column, OpenCvSharp.ReduceTypes.Avg, -1);
                    Cv2.Reduce(differ_mat, differ_mat, OpenCvSharp.ReduceDimension.Row, OpenCvSharp.ReduceTypes.Avg, -1);

                    ave_X_point = differ_mat.At<Vec3d>(0, 0).Item2;
                    ave_Y_point = differ_mat.At<Vec3d>(0, 0).Item1;
                    */

                    ave_Y_point = ave_Y_point / sum_px;
                    ave_X_point = ave_X_point / sum_px;
                    ave_X_point = Math.Round(ave_X_point, 1, MidpointRounding.AwayFromZero);
                    ave_Y_point = Math.Round(ave_Y_point, 1, MidpointRounding.AwayFromZero);
                    

                    DateTime e_dt = DateTime.Now;
                    String end_date = e_dt.ToString("yyyy/MM/dd-HH:mm:ss:fff");

                    DoEvents();

                    // 結果表示
                    HueTextRed.Inlines.Clear();
                    HueTextRed.Inlines.Add(new Run("赤色ずれ(Y軸) "));
                    HueTextRed.Inlines.Add(ave_Y_point.ToString());
                    HueTextRed.Inlines.Add("%");
                    HueTextRed.FontSize = 30;
                    HueTextRed.TextAlignment = TextAlignment.Center;
                    HueTextRed.Foreground = Brushes.Red;

                    HueTextBlue.Inlines.Clear();
                    HueTextBlue.Inlines.Add(new Run("青色ずれ(X軸) "));
                    HueTextBlue.Inlines.Add(ave_X_point.ToString());
                    HueTextBlue.Inlines.Add("%");
                    HueTextBlue.FontSize = 30;
                    HueTextBlue.TextAlignment = TextAlignment.Center;
                    HueTextBlue.Foreground = Brushes.Navy;

                    Thread.Sleep(500);

                }

            }
            catch
            {
                HueButton.IsEnabled = true;
                HueText.Inlines.Clear();
                HueTextRed.Inlines.Clear();
                HueTextBlue.Inlines.Clear();

            }
            finally
            {
                HueButton.IsEnabled = true;
                HueText.Inlines.Clear();
                HueTextRed.Inlines.Clear();
                HueTextBlue.Inlines.Clear();

            }

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

        public string getNewestFileName(string folderName)
        {
            // 指定されたフォルダ内のdatファイル名をすべて取得する
            string[] files = System.IO.Directory.GetFiles(folderName, "*.png", System.IO.SearchOption.TopDirectoryOnly);

            string newestFileName = string.Empty;

            System.DateTime updateTime = System.DateTime.MinValue;
            foreach (string file in files)
            {
                // それぞれのファイルの更新日付を取得する
                System.IO.FileInfo fi = new System.IO.FileInfo(file);
                // 更新日付が最新なら更新日付とファイル名を保存する
                if (fi.LastWriteTime > updateTime)
                {
                    updateTime = fi.LastWriteTime;
                    newestFileName = file;
                }
            }
            // ファイル名を返す
            return System.IO.Path.GetFileName(newestFileName);
        }

        //新規作成
        private void textBoxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void textBoxPrice_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼り付けを許可しない
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void Write_Button(object sender, RoutedEventArgs e)
        {
            //新規書き込みクラスのインスタンス化
            TextWrite textWrite = new TextWrite();

            filename = WriteFileName.Text.ToString();
            texts = textLeftRed.Text + "," + textRightRed.Text + "," + textLeftGreen.Text + "," + textRightGreen.Text + "," + textLeftBlue.Text + "," + textRightBlue.Text;

            textWrite.Write(filename, texts);
        }

        //読み込み
        private void Read_Button(object sender, RoutedEventArgs e)
        {
            //読み込みクラスのインスタンス化
            ReadText readText = new ReadText();

            filename = ReadFileName.Text.ToString();

            String FileContent = readText.Read(filename);
            string[] ParameterArray = FileContent.Split(',');
            if (ParameterArray.Length != 6)
            {
                MessageBox.Show("設定ファイルの形式に問題があります", "ファイルエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (FileContent != "")
            {
                textLeftRed.Text = ParameterArray[0];
                textRightRed.Text = ParameterArray[1];
                textLeftGreen.Text = ParameterArray[2];
                textRightGreen.Text = ParameterArray[3];
                textLeftBlue.Text = ParameterArray[4];
                textRightBlue.Text = ParameterArray[5];

            }


        }

        public static string GetCurrentAppDir()
        {
            return System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        public static DirectoryInfo SafeCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
        }

    }
}

