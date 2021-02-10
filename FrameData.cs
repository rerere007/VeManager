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
    class FrameData
    {
        private Mat frame_mat;
        private Mat gray_frame_mat = new Mat();
        private BFMatcher bfMatcher = new BFMatcher(NormTypes.Hamming, false);
        private AKAZE akaze = AKAZE.Create(); //AKAZEのセットアップ
        private KeyPoint[] frame_keypoint;        //比較元画像の特徴点
        private Mat frame_desc = new Mat();  //比較元画像の特徴量


        public FrameData(string read_image_path)
        {
            try
            {
                frame_mat = Cv2.ImRead(read_image_path);

            }
            catch (Exception read_image_error)
            {
                Console.WriteLine(read_image_error);
                throw;

            }

        }

        public FrameData(Mat mat)
        {
            try
            {
                frame_mat = mat;

            }
            catch (Exception copy_mat_error)
            {
                Console.WriteLine(copy_mat_error);
                throw;

            }
        }

        public void ReadFrame(string read_image_path)
        {
            try
            {
                frame_mat = Cv2.ImRead(read_image_path);

            }
            catch (Exception read_image_error)
            {
                Console.WriteLine(read_image_error);
                throw;

            }
        }

        public void ResizeFrame(double comp_rate)
        {
            try
            {
                /* image resize */
                Cv2.Resize(frame_mat, frame_mat, OpenCvSharp.Size.Zero, comp_rate, comp_rate);

            }
            catch (Exception opencv_resize_error)
            {
                Console.WriteLine(opencv_resize_error);
                throw;

            }

        }
        public void GrayFrame()
        {
            try
            {
                Cv2.CvtColor(frame_mat, gray_frame_mat, ColorConversionCodes.BGR2GRAY);
                //BGR2XYZ, Y = 709 Ysig, 2channel
                //BGR2GRAY, Y = 0.299 * R + 0.587 * G + 0.114 * B

            }
            catch (Exception opencv_gray_error)
            {
                Console.WriteLine(opencv_gray_error);
                throw;

            }

        }

        public void GaussianBlurToGray()
        {
            Cv2.GaussianBlur(gray_frame_mat, gray_frame_mat, new OpenCvSharp.Size(5, 5), 0);
            
        }

        public void GaussianBlurToBGR()
        {
            Cv2.GaussianBlur(frame_mat, frame_mat, new OpenCvSharp.Size(5, 5), 0);

        }


        public Mat getFrameMat()
        {
            return this.frame_mat;

        }

        public Mat getGrayFrameMat()
        {
            return this.gray_frame_mat;

        }

        public void FrameDetectAndCompute()
        {
            this.akaze.DetectAndCompute(this.gray_frame_mat, null, out this.frame_keypoint, this.frame_desc);

        }

        public double CalcDistance(FrameData target_frame)
        {
            double avg_partial_distance = 0;
            DMatch[] matches;

            matches =  this.bfMatcher.Match(this.frame_desc, target_frame.frame_desc);
            foreach(var m in matches)
            {
                avg_partial_distance = m.Distance;
            }
            avg_partial_distance = avg_partial_distance / matches.Length;
            return avg_partial_distance;

        }

        public void WriteFrame(string write_image_path)
        {
            try
            {
                Cv2.ImWrite(write_image_path, frame_mat);

            }
            catch (Exception opencv_write_error)
            {
                Console.WriteLine(opencv_write_error);
                throw;

            }

        }


        public WriteableBitmap ReadWriteableBitmap(string target_image_path)
        {
            //lock less bitmap
            WriteableBitmap wbmp;
            using (MemoryStream data = new MemoryStream(File.ReadAllBytes(target_image_path)))
            {
                wbmp = new WriteableBitmap(BitmapFrame.Create(data));
            }
            File.Delete(target_image_path);
            return wbmp;

        }

        public void cine_convert(double LeftBlue, double RightBlue, double LeftRed, double RightRed, double LeftGreen, double RightGreen)
        {
            // gamma補正値 2.2
            //double LtoGamma = 1 / 2.2;
            try
            {
                unsafe
                {
                    MatForeachFunctionVec3b del_cinelite_func;
                    del_cinelite_func = delegate (Vec3b* px, int* position)
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
                    this.frame_mat.ForEachAsVec3b(del_cinelite_func);

                };
            }
            catch (Exception cine_convert_error)
            {
                Console.WriteLine(cine_convert_error);
                throw;

            }
        }

        public void hue_detect_convert(double saturation_border_percent, double LowerAngle, double UpperAngle)
        {
            double saturation_border = saturation_border_percent * 0.59567675798 * 255 / 100; //G & Magentaの時
            try
            {
                unsafe
                {

                    MatForeachFunctionVec3b del_hue_detect_func;
                    del_hue_detect_func = delegate (Vec3b* px, int* position)
                    {

                        // Gamma RGB
                        byte b_px = px->Item0;
                        byte g_px = px->Item1;
                        byte r_px = px->Item2;

                        // YCrCb
                        // Color matrix: ARIB HDTV
                        double linear_y_sig = 0.2126 * r_px + 0.7152 * g_px + 0.0722 * b_px;
                        double cr = 0.6350 * (r_px - linear_y_sig); //0.6350 * R_Y
                        double cb = 0.5389 * (b_px - linear_y_sig); //0.5389 * B_Y

                        double saturation = Math.Sqrt(cr * cr + cb * cb); // rを計算
                        double radian = Math.Atan2(cr, cb); //radianを計算
                        double angle = radian * (180 / Math.PI);
                        if (angle >= 0)
                        {
                            ;
                        } else
                        {
                            angle = angle + 360;
                        }

                        bool color_space_detect_flag = (LowerAngle <= angle) && (angle <= UpperAngle);
                        bool saturation_detect_flag = (saturation > saturation_border);

                        if (color_space_detect_flag && saturation_detect_flag)
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
                    this.frame_mat.ForEachAsVec3b(del_hue_detect_func);

                };
            }
            catch (Exception hue_detect_error)
            {
                Console.WriteLine(hue_detect_error);
                throw;

            }

        }

        /* ２次元データの重心計算 */
        public Vec3d cal_ave_point()
        {
            Vec3d result_point;
            int sum_px = 0;
            double ave_Y_point = 0;
            double ave_X_point = 0;
            double ave_Ysig = 0;

            for (int y = 0; y < frame_mat.Rows; y++)
            {
                for (int x = 0; x < frame_mat.Cols; x++)
                {
                    Vec3b point = frame_mat.At<Vec3b>(y, x);
                    byte r_px = point.Item2;
                    byte g_px = point.Item1;
                    byte b_px = point.Item0;

                    //Y, R-Y, B-Yを計算. Color matrixはARIB HDTV
                    double y_sig = 0.2126 * r_px + 0.7152 * g_px + 0.0722 * b_px;
                    double cr = 0.6350 * ((double)r_px - y_sig); // R_Y * 0.6350
                    double cb = 0.5389 * ((double)b_px - y_sig); // B_Y * 0.5389

                    //黒部分はスルー. その他はインクリメントし、平均値を計算
                    if ((r_px == 0) && (g_px == 0) && (b_px == 0))
                    {
                        continue;

                    }else
                    {
                        sum_px++;
                        ave_Y_point = ave_Y_point + cr;
                        ave_X_point = ave_X_point + cb;
                        ave_Ysig = ave_Ysig + y_sig;

                    }
                }
            }

            if (sum_px == 0)
            {
                throw new DivideByZeroException(); // 入力に該当映像がなかった場合のError 処理

            }
            else
            {

                ave_Ysig = ave_Ysig / sum_px;
                ave_Y_point = ave_Y_point / sum_px;
                ave_X_point = ave_X_point / sum_px;

                result_point.Item0 = ave_Y_point;
                result_point.Item1 = ave_X_point;
                result_point.Item2 = ave_Ysig;
            }

            return result_point;

        }

        public void gamma_correction(double lambda)
        {
            try
            {
                unsafe
                {

                    MatForeachFunctionVec3b del_gamma_correction_func;
                    del_gamma_correction_func = delegate (Vec3b* px, int* position)
                    {
                        // 各ピクセルに対してgamma correction
                        byte b_px = px->Item0;
                        byte g_px = px->Item1;
                        byte r_px = px->Item2;

                        px->Item0 = Convert.ToByte(255 * Math.Pow((double)b_px / 255, lambda));
                        px->Item1 = Convert.ToByte(255 * Math.Pow((double)g_px / 255, lambda));
                        px->Item2 = Convert.ToByte(255 * Math.Pow((double)r_px / 255, lambda));

                    };
                    this.frame_mat.ForEachAsVec3b(del_gamma_correction_func);

                };
            }
            catch (Exception gamma_correction_error)
            {
                Console.WriteLine(gamma_correction_error);
                throw;

            }
        }

        public static implicit operator Mat(FrameData v)
        {
            throw new NotImplementedException();

        }
    }
}
