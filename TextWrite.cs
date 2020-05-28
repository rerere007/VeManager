using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VeManagerApp
{
    class TextWrite //新規書き込みクラス
    {
        public void Write(string FileName, string gettext)
        {
            string filename = GetCurrentAppDir() + "\\ConfigDB\\" + FileName + ".txt";
            string text = gettext;

            if (FileName == ".txt")
            {
                MessageBox.Show("ファイル名が空白です", "ファイル保存", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (File.Exists(filename))
            {
                MessageBox.Show(filename + " は既に存在します。\nファイル名を変更してください。", "ファイルエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                //書き込みには StreamWriter クラスを使う
                //引数には書き込むファイル名、追記の有無、エンコードを入力
                //false にしておくと上書きモードになる
                StreamWriter streamWriter = new StreamWriter(filename, false, Encoding.UTF8);

                //Write メソッドはそのまま書き込む
                streamWriter.Write(text);

                //ファイルを使ったら閉じましょう
                streamWriter.Close();

                MessageBox.Show(filename + " を作成しました", "ファイル作成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("他のアプリがファイルを開いています", "ファイルエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static string GetCurrentAppDir()
        {
            return System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }
}
