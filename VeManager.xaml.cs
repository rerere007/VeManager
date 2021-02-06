using OpenCvSharp; //OpenCvSharp4.windows
using Reactive.Bindings;
using System;
using System.IO;
using System.Text;
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

    public partial class VeManager : System.Windows.Window
    {
        //Task Flag
        private Constants cont = new Constants();

        //FrameDataの取り込み先(上は開発, 下はPC)
        //private string resource_dir = "C:\\Users\\dev\\source\\repos\\VeManager\\Resources\\";
        private string resource_dir = "C:\\Users\\0000526030\\source\\repos\\VeManager\\Image\\";

        //CaptureStillsのOutput(上は開発PC, 下はPC)
        //private string captured_image_dir = "R:\\Temp\\";
        private string captured_image_dir = "C:\\Users\\0000526030\\source\\repos\\VeManager\\Resources\\";

        private string config_filename;
        private string config_texts;
        private String current_app_dir = GetCurrentAppDir();

        public VeManager()
        {
            string config_db_path = current_app_dir + "\\" + "ConfigDB";
            string log_db_path = current_app_dir + "\\" + "SystemLog";

            SafeCreateDirectory(config_db_path);
            SafeCreateDirectory(log_db_path);
            InitializeComponent();

        }

        private void show_image(object sender, RoutedEventArgs e)
        {
            ShowButton.IsEnabled = false;
            double show_comp_rate = 0.5;
            cont.change_task(Constants.SHOW_IMAGE_TASK);

            try
            {

                while (cont.getCurrentTask() == Constants.SHOW_IMAGE_TASK)
                {
                    DateTime dt = DateTime.Now;
                    String current_image_name = dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String show_image_path = resource_dir + current_image_name + ".png";
                    FrameData fd;

                    String current_image_path;

                    try
                    {
                        current_image_path = captured_image_dir + GetNewestFileName(captured_image_dir);
                    }
                    catch (Exception get_name_error)
                    {
                        Console.WriteLine(get_name_error);
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        fd = new FrameData(current_image_path);

                    }
                    catch (Exception opencv_read_error)
                    {
                        Console.WriteLine(opencv_read_error);
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        fd.ResizeFrame(show_comp_rate);

                    }
                    catch (Exception opencv_resize_error)
                    {
                        Console.WriteLine(opencv_resize_error);
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        fd.WriteFrame(show_image_path);

                    }
                    catch (Exception opencv_write_error)
                    {
                        Console.WriteLine(opencv_write_error);
                        DoEvents();
                        continue;

                    }

                    //lock less bitmap
                    this.MainImage.Source = fd.ReadWriteableBitmap(show_image_path);
                    DoEvents();

                }
            }
            catch
            {
                throw;

            }
            finally
            {
                ShowButton.IsEnabled = true;
                cont.init();
                DoEvents();

            }

        }

        private void delete_image(object sender, RoutedEventArgs e)
        {
            cont.init();
            var source = new BitmapImage();
            MainImage.Source = source;
            BaseImage.Source = source;
            HueTextYsig.Inlines.Clear();
            HueTextRed.Inlines.Clear();
            HueTextBlue.Inlines.Clear();
            HueText.Inlines.Clear();

        }
        private void train_image(object sender, RoutedEventArgs e)
        {
            TrainButton.IsEnabled = false;
            double train_comp_rate = 0.5;
            cont.change_task(Constants.TRAIN_IMAGE_TASK);

            try
            {

                while (cont.getCurrentTask() == Constants.TRAIN_IMAGE_TASK)
                {
                    DateTime dt = DateTime.Now;
                    String current_image_name = dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String train_image_path = resource_dir + current_image_name + ".png";
                    FrameData fd;

                    String current_image_path;

                    try
                    {
                        current_image_path = captured_image_dir + GetNewestFileName(captured_image_dir);
                    }
                    catch (Exception get_name_error)
                    {
                        Console.WriteLine(get_name_error);
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        fd = new FrameData(current_image_path);

                    }
                    catch (Exception opencv_read_error)
                    {
                        Console.WriteLine(opencv_read_error);
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        fd.ResizeFrame(train_comp_rate);

                    }
                    catch (Exception opencv_resize_error)
                    {
                        Console.WriteLine(opencv_resize_error);
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        fd.WriteFrame(train_image_path);

                    }
                    catch (Exception opencv_write_error)
                    {
                        Console.WriteLine(opencv_write_error);
                        DoEvents();
                        continue;

                    }




                    //lock less bitmap
                    this.MainImage.Source = fd.ReadWriteableBitmap(train_image_path);
                    DoEvents();

                }
            }
            catch
            {
                throw;

            }
            finally
            {
                TrainButton.IsEnabled = true;
                cont.init();
                DoEvents();

            }


        }


        /* HUE Detectで彩度と色相範囲を用いて抽出し、Gamma補正をかけつつ色を比較 */
        private void hue_image(object sender, RoutedEventArgs e)
        {
            HueButton.IsEnabled = false;
            double hue_comp_rate = 0.5;
            double Ysig_range = 20.0; //Y信号%ずれを許容する範囲
            double similar_border = 70; //類似度を許容する範囲
            double conv_percent_to_sig = 0.39215686274;

            DateTime dt = DateTime.Now;
            String exe_time = GetCurrentAppDir() + "\\SystemLog\\" + dt.ToString("yyyy_MM_dd-HH_mm_ss_fff") + ".txt";
            FileStream exe_time_stream = File.Open(exe_time, FileMode.OpenOrCreate);
            StreamWriter exe_stream_writer = new StreamWriter(exe_time_stream, System.Text.Encoding.UTF8);

            double LeftBlue = 0;
            double RightBlue = 0;
            double LeftRed = 0;
            double RightRed = 0;
            double LeftGreen = 0;
            double RightGreen = 0;
            double BlueSatur = 0;
            double RedSatur = 0;
            double GreenSatur = 0;

            /*
            double ret_b = 0;
            double ret_g = 0;
            double ret_r = 0;
            Mat current_hist_b = new Mat();
            Mat current_hist_g = new Mat();
            Mat current_hist_r = new Mat();
            Mat base_hist_r = new Mat();
            Mat base_hist_g = new Mat();
            Mat base_hist_b = new Mat();
            */


            cont.change_task(Constants.HUE_IMAGE_TASK);

            try
            {

                String base_image_path = resource_dir + "base_image.png";
                String current_image_path = captured_image_dir + GetNewestFileName(captured_image_dir);
                FrameData CaptureFrame, BaseFrame;

                try
                {
                    LeftBlue = double.Parse(textLeftBlue.Text);
                    RightBlue = double.Parse(textRightBlue.Text);
                    LeftRed = double.Parse(textLeftRed.Text);
                    RightRed = double.Parse(textRightRed.Text);
                    LeftGreen = double.Parse(textLeftGreen.Text);
                    RightGreen = double.Parse(textRightGreen.Text);
                    BlueSatur = double.Parse(textBlueSatur.Text);
                    RedSatur = double.Parse(textRedSatur.Text);
                    GreenSatur = double.Parse(textGreenSatur.Text);

                }
                catch (Exception catch_color_border_data)
                {
                    Console.WriteLine(catch_color_border_data);

                }

                try
                {
                    CaptureFrame = new FrameData(current_image_path);

                }
                catch (Exception opencv_read_error)
                {
                    Console.WriteLine(opencv_read_error);
                    DoEvents();
                    return;

                }

                try
                {
                    CaptureFrame.WriteFrame(base_image_path);

                }
                catch (Exception opencv_write_error)
                {
                    Console.WriteLine(opencv_write_error);
                    DoEvents();

                }

                BaseFrame = new FrameData(CaptureFrame.getFrameMat());

                try
                {
                    /* resize image */
                    BaseFrame.ResizeFrame(hue_comp_rate);

                }
                catch (Exception opencv_resize_error)
                {
                    Console.WriteLine(opencv_resize_error);
                    DoEvents();

                }

                try
                {
                    /* save grayimage */
                    BaseFrame.GrayFrame();

                }
                catch (Exception opencv_gray_error)
                {
                    Console.WriteLine(opencv_gray_error);
                    DoEvents();

                }

                //Detect key point
                BaseFrame.FrameDetectAndCompute();

                //hist類似度計算
                /*
                Cv2.CalcHist(new Mat[] { BaseFrame.getFrameMat() }, new int[] { 0 }, new Mat(), base_hist_b, 1, new int[] { 256 }, new OpenCvSharp.Rangef[] { new Rangef(0, 256) }, true, false);
                Cv2.CalcHist(new Mat[] { BaseFrame.getFrameMat() }, new int[] { 1 }, new Mat(), base_hist_g, 1, new int[] { 256 }, new OpenCvSharp.Rangef[] { new Rangef(0, 256) }, true, false);
                Cv2.CalcHist(new Mat[] { BaseFrame.getFrameMat() }, new int[] { 2 }, new Mat(), base_hist_r, 1, new int[] { 256 }, new OpenCvSharp.Rangef[] { new Rangef(0, 256) }, true, false);
                */
                //類似度計算終了

                //Border Saturation Percent
                BaseFrame.hue_detect_convert(RedSatur, LeftRed, RightRed); 

                try
                {
                    BaseFrame.WriteFrame(base_image_path);

                }
                catch (Exception opencv_write_error)
                {
                    Console.WriteLine(opencv_write_error);
                    DoEvents();

                }

                this.BaseImage.Source = BaseFrame.ReadWriteableBitmap(base_image_path);
                Vec3d base_point;
                try
                {
                    base_point = BaseFrame.cal_ave_point();
                }
                catch (DivideByZeroException no_target_pixel_error)
                {
                    Console.WriteLine(no_target_pixel_error);
                    DoEvents();
                    throw; // ⇒catch句

                }

                //以下, HUEの比較計算処理
                HueText.Inlines.Clear();
                HueText.FontSize = 24;
                HueText.Inlines.Add(new Bold(new Run("基準画像と現在画像の色ズレ")));
                HueText.TextAlignment = TextAlignment.Center;

                while (cont.getCurrentTask() == Constants.HUE_IMAGE_TASK)
                {

                    DateTime s_dt = DateTime.Now;
                    String image = s_dt.ToString("yyyy_MM_dd-HH_mm_ss_fff");
                    String hue_image_path = resource_dir + image + ".png";
                    current_image_path = captured_image_dir + GetNewestFileName(captured_image_dir);
                    FrameData CurrentFrame;
                    double dif_Y_point = 0, dif_X_point = 0, dif_Ysig_point = 0;
                    double dif_R_axis = 0, dif_B_axis = 0;

                    try
                    {
                        LeftBlue = double.Parse(textLeftBlue.Text);
                        RightBlue = double.Parse(textRightBlue.Text);
                        LeftRed = double.Parse(textLeftRed.Text);
                        RightRed = double.Parse(textRightRed.Text);
                        LeftGreen = double.Parse(textLeftGreen.Text);
                        RightGreen = double.Parse(textRightGreen.Text);
                        BlueSatur = double.Parse(textBlueSatur.Text);
                        RedSatur = double.Parse(textRedSatur.Text);
                        GreenSatur = double.Parse(textGreenSatur.Text);

                    }
                    catch (Exception catch_color_border_data)
                    {
                        Console.WriteLine(catch_color_border_data);
                        continue;

                    }


                    try
                    {
                        CurrentFrame = new FrameData(current_image_path);

                    }
                    catch(Exception opencv_read_error)
                    {
                        Console.WriteLine(opencv_read_error);
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        CurrentFrame.ResizeFrame(hue_comp_rate);

                    }
                    catch (Exception current_mat_resize_error)
                    {
                        Console.WriteLine(current_mat_resize_error);
                        DoEvents();
                        continue;

                    }

                    try
                    {
                        /* save grayimage */
                        CurrentFrame.GrayFrame();

                    }
                    catch (Exception opencv_gray_error)
                    {
                        Console.WriteLine(opencv_gray_error);
                        DoEvents();

                    }

                    //Detect key point
                    CurrentFrame.FrameDetectAndCompute();
                    double avg_partial_distance = BaseFrame.CalcDistance(CurrentFrame);

                    //類似度計算 
                    /*
                    Cv2.CalcHist(new Mat[] { CurrentFrame.getFrameMat() }, new int[] { 0 }, new Mat(), current_hist_b, 1, new int[] { 256 }, new OpenCvSharp.Rangef[] { new Rangef(0, 256) }, true, false);
                    Cv2.CalcHist(new Mat[] { CurrentFrame.getFrameMat() }, new int[] { 1 }, new Mat(), current_hist_g, 1, new int[] { 256 }, new OpenCvSharp.Rangef[] { new Rangef(0, 256) }, true, false);
                    Cv2.CalcHist(new Mat[] { CurrentFrame.getFrameMat() }, new int[] { 2 }, new Mat(), current_hist_r, 1, new int[] { 256 }, new OpenCvSharp.Rangef[] { new Rangef(0, 256) }, true, false);
                    ret_b = Math.Round(Cv2.CompareHist(current_hist_b, base_hist_b, 0), 1, MidpointRounding.AwayFromZero) * 100;
                    ret_g = Math.Round(Cv2.CompareHist(current_hist_g, base_hist_g, 0), 1, MidpointRounding.AwayFromZero) * 100;
                    ret_r = Math.Round(Cv2.CompareHist(current_hist_r, base_hist_r, 0), 1, MidpointRounding.AwayFromZero) * 100;
                    */
                    //類似度計算 終了


                    //hue_detect_convert(saturation percent, hue start angle, hue end angle)
                    CurrentFrame.hue_detect_convert(RedSatur, LeftRed, RightRed);
                    Vec3d current_point;
                    try
                    {
                        current_point = CurrentFrame.cal_ave_point();//Gamma補正前

                    }
                    catch(DivideByZeroException no_target_pixel_error)
                    {
                        Console.WriteLine(no_target_pixel_error);
                        DoEvents();
                        continue;

                    }
                    double gamma_lambda = Math.Log(base_point.Item2 / 255) / Math.Log(current_point.Item2 / 255); //補正指数値計算
                    CurrentFrame.gamma_correction(gamma_lambda); //gamma補正

                    Vec3d gamma_current_point;
                    try
                    {
                        gamma_current_point = CurrentFrame.cal_ave_point();//gammga補正後（輝度平均がそろう）
                    }
                    catch (DivideByZeroException no_target_pixel_error)
                    {
                        Console.WriteLine(no_target_pixel_error);
                        DoEvents();
                        continue;

                    }

                    /* R/Bのベクトル方程式を解いてR/Bの二軸で表現する */
                    dif_X_point = (gamma_current_point.Item1 - base_point.Item1); // X軸値 εX
                    dif_Y_point = (gamma_current_point.Item0 - base_point.Item0); // Y軸値 εY
                    dif_Ysig_point = (gamma_current_point.Item2 - base_point.Item2); // Ysig値 εY

                    dif_R_axis = (10.9170305677 * dif_Y_point - dif_X_point) / 1421.13725738 * 100; //Red軸 %表記
                    dif_B_axis = (4.36406800964 * dif_X_point + dif_Y_point) / 568.097671229 * 100; //Blue軸 %表記
                    dif_Ysig_point = dif_Ysig_point * conv_percent_to_sig; //Ysig dif % 映像信号レベル

                    dif_R_axis = Math.Round(dif_R_axis, 1, MidpointRounding.AwayFromZero);
                    dif_B_axis = Math.Round(dif_B_axis, 1, MidpointRounding.AwayFromZero);
                    dif_Ysig_point = Math.Round(dif_Ysig_point, 1, MidpointRounding.AwayFromZero);

                    DoEvents();

                    try
                    {
                        if(ViewResult(CurrentFrame, hue_image_path, Ysig_range, similar_border, dif_Y_point, dif_R_axis, dif_B_axis, avg_partial_distance))
                        {
                            exe_stream_writer.WriteLine("---------------------------------------------");
                            exe_stream_writer.WriteLine(current_point.Item2 * conv_percent_to_sig);
                            exe_stream_writer.WriteLine(dif_Ysig_point);
                            exe_stream_writer.WriteLine(dif_R_axis);
                            exe_stream_writer.WriteLine(dif_B_axis);

                        }
                        else
                        {
                            continue;

                        }

                    }
                    catch(Exception opencv_write_error)
                    {
                        Console.WriteLine(opencv_write_error);
                        DoEvents();
                        continue;

                    }

                    Thread.Sleep(10);

                }

            }
            catch
            {

            }
            finally
            {
                var source = new BitmapImage();
                this.MainImage.Source = source;
                exe_stream_writer.Close();
                HueTextYsig.Inlines.Clear();
                HueTextRed.Inlines.Clear();
                HueTextBlue.Inlines.Clear();
                cont.init();
                HueButton.IsEnabled = true;
                DoEvents();

            }
        }

        private bool ViewResult(FrameData CurrentFrame, String hue_image_path, double Ysig_range, double similar_border, double dif_Ysig_point, double dif_R_axis, double dif_B_axis, double avg_partial_distance)
        {
            // ここからは表示
            try
            {
                CurrentFrame.WriteFrame(hue_image_path);

            }
            catch (Exception opencv_write_error)
            {
                throw opencv_write_error;

            }
            this.MainImage.Source = CurrentFrame.ReadWriteableBitmap(hue_image_path);

            HueTextYsig.Inlines.Clear();
            HueTextRed.Inlines.Clear();
            HueTextBlue.Inlines.Clear();

            HueTextYsig.Inlines.Add(new Run("Y信号差分"));
            HueTextYsig.Inlines.Add(dif_Ysig_point.ToString());
            HueTextYsig.Inlines.Add("%\n");
            HueTextYsig.Inlines.Add(avg_partial_distance.ToString());
            HueTextYsig.Inlines.Add("\n");

            HueTextYsig.FontSize = 30;
            HueTextYsig.TextAlignment = TextAlignment.Center;
            HueTextYsig.Foreground = Brushes.White;

            //特徴量距離がボーダー値より離れているかどうかを比較. ボーダー値は事前に３００フレーム程度キャリブレーションする必要がある。
            if (avg_partial_distance >= similar_border)
            {
                HueTextYsig.Inlines.Add("画角不一致\n");
                return false;

            } else
            {
                HueTextYsig.Inlines.Add("画角一致\n");

            }

            if ((dif_Ysig_point < -1 * Ysig_range) | (dif_Ysig_point > Ysig_range))
            {
                return false;

            }
            else
            {

                HueTextRed.Inlines.Add(new Run("赤色ずれ "));
                HueTextRed.Inlines.Add(dif_R_axis.ToString());
                HueTextRed.Inlines.Add("%");
                HueTextRed.FontSize = 30;
                HueTextRed.TextAlignment = TextAlignment.Center;
                HueTextRed.Foreground = Brushes.Red;

                HueTextBlue.Inlines.Add(new Run("青色ずれ "));
                HueTextBlue.Inlines.Add(dif_B_axis.ToString());
                HueTextBlue.Inlines.Add("%");
                HueTextBlue.FontSize = 30;
                HueTextBlue.TextAlignment = TextAlignment.Center;
                HueTextBlue.Foreground = Brushes.Navy;

                return true;

            }
        }


        // ↓ ここから独自開発のDoEvents()
        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);

        }


        private object ExitFrames(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;

        }


        private string GetNewestFileName(string folderName)
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
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
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

            config_filename = WriteFileName.Text.ToString();
            config_texts = textLeftRed.Text + "," + textRightRed.Text + "," + textLeftGreen.Text + "," + textRightGreen.Text + "," + textLeftBlue.Text + "," + textRightBlue.Text + "," + textRedSatur.Text + "," + textGreenSatur.Text + "," + textBlueSatur.Text;
            textWrite.Write(config_filename, config_texts);
        }

        //読み込み
        private void Read_Button(object sender, RoutedEventArgs e)
        {
            //読み込みクラスのインスタンス化
            ReadText readText = new ReadText();

            config_filename = ReadFileName.Text.ToString();

            String FileContent = readText.Read(config_filename);
            string[] ParameterArray = FileContent.Split(',');
            if (ParameterArray.Length != 9)
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
                textRedSatur.Text = ParameterArray[6];
                textGreenSatur.Text = ParameterArray[7];
                textBlueSatur.Text = ParameterArray[8];

            }
        }


        private static string GetCurrentAppDir()
        {
            return System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
        }


        private static DirectoryInfo SafeCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
        }

    }
}

