using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VeManagerApp
{
    class ReadText //読み込みクラス
    {
        public string Read(string FileName)
        {
            string filename = GetCurrentAppDir() + "\\ConfigDB\\" + FileName + ".txt";

            if (FileName == ".txt")
            {
                MessageBox.Show("ファイル名が空白です", "ファイル保存", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }

            try
            {
                //読み込みは StreamReader クラスを用いる
                //引数には読み込むファイル名とエンコードが必要
                StreamReader streamReader = new StreamReader(filename, Encoding.UTF8);

                var mainWindow = (VeManager)App.Current.MainWindow;

                //読み込む前に初期化しておく
                String FileContent = "";
                try
                {
                    while (streamReader.EndOfStream == false)
                    {
                        string text = streamReader.ReadLine();
                        FileContent += text + "\n";
                    }

                    MessageBox.Show(filename + " の読み込みが完了しました。", "ファイル読み込み", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show("他のアプリがファイルを開いています", "ファイルエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }

                streamReader.Close();
                return FileContent;
            }
            catch (Exception)
            {
                MessageBox.Show("ファイルが見つかりませんでした", "ファイルエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }

        public static string GetCurrentAppDir()
        {
            return System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }
}
