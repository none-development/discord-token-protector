using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ModernMessageBoxLib;
using DiscordTokenProtector.INISystem;
using Path = System.IO.Path;

namespace DiscordTokenProtector
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string password { get; set; }
        private string final_path { get; set; }
        private string down_path { get; set; }

        public MainWindow()
        {
            var MyIni = new INIFile(PATHFILES.CONFIGFILE);
            InitializeComponent();
            StartUpChecker.main();
            this.MouseLeftButtonDown += delegate { DragMove(); };
            bool savedpath = Boolean.Parse(MyIni.Read("SAFEPATH", "SETTINGS").ToLower().ToString());
            change(savedpath);
            saveinconfig.IsChecked = savedpath;

        }

        //Close Programm

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }
        //Minimize Programm
        private void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.ResetUserSelections();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\";
            dialog.IsFolderPicker = true;
            dialog.Title = @"Select Discord leveldb Folder | by default: Discord\Local Storage\leveldb";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PATH_DISCORD_DB.Content = dialog.FileName;
                final_path = dialog.FileName;
            }
        }




        private void Decrypter(string inputFile, string outputFile)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[65];
            FileStream cryptfiles = new FileStream(inputFile, FileMode.Open);
            cryptfiles.Read(salt, 0, salt.Length);
            RijndaelManaged ert = new RijndaelManaged();
            ert.KeySize = 256;
            ert.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            ert.Key = key.GetBytes(ert.KeySize / 8);
            ert.IV = key.GetBytes(ert.BlockSize / 8);
            ert.Padding = PaddingMode.PKCS7;
            ert.Mode = CipherMode.CBC;

            CryptoStream cryptoStream = new CryptoStream(cryptfiles, ert.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fileStreamOutput = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                {

                    fileStreamOutput.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                QModernMessageBox.Show($"CryptographicException error:\n{ex_CryptographicException.Message}", "Error Appeard!", QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Warning);

            }
            catch (Exception ex)
            {

                QModernMessageBox.Show($"Error:\n{ex.Message}", "Error Appeard!", QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Warning);
            }

            try
            {
                cryptoStream.Close();
            }
            catch (Exception ex)
            {
                QModernMessageBox.Show($"Error by closing CryptoStream::\n{ex.Message}", "Error Appeard!", QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Warning);
            }
            finally
            {
                fileStreamOutput.Close();
                cryptfiles.Close();
            }
        }

        public void Crypto(string inputFile)
        {
            byte[] salt = GenerateRandomSalt2();
            FileStream fsCrypt = new FileStream(inputFile + ".DTP", FileMode.Create);
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CBC;
            fsCrypt.Write(salt, 0, salt.Length);
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
            FileStream fsIn = new FileStream(inputFile, FileMode.Open);
            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cs.Write(buffer, 0, read);
                }

                fsIn.Close();
            }
            catch
            {
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
            }
        }
        public byte[] GenerateRandomSalt2()
        {

            byte[] data = new byte[65];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(data);
                }
            }

            return data;
        }
        private void Crypt_Click(object sender, RoutedEventArgs e)
        {
            listbox1.Items.Clear();
            password = cryptpw.Password;
            if (password != "" && final_path != "")
            {
                encryptStuff(final_path + @"\");
            }
        }
        private void encryptStuff(string sDir)
        {

            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if (!f.Contains(".DTP"))
                    {
                        if (f.Contains(".ldb"))
                        {
                            string g = Path.GetFileName(f);
                            string h = g + " Crypted";
                            listbox1.Items.Add(h);
                            Crypto(f);
                            File.Delete(f);
                        }
                    }

                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    encryptStuff(d);
                }
            }
            catch (System.Exception excpt)
            {
                QModernMessageBox.Show($"Error:\n{excpt.Message}", "Application Information", QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Error);
            }
            
        }

        private void deleteLogs(string sDir, string endstuff)
        {

            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if (f.Contains(endstuff))
                    {
                        string g = Path.GetFileName(f);
                        string h = "Deleted: " + g;
                        listbox1.Items.Add(h);
                        File.Delete(f);
                    }

                }
            }
            catch (System.Exception excpt)
            {
                QModernMessageBox.Show($"Error:\n{excpt.Message}", "Application Information", QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Error);
            }

        }
        private void deleteSession(string sDir)
        {

            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    string g = Path.GetFileName(f);
                    string h = "Deleted: " + g;
                    listbox1.Items.Add(h);
                  File.Delete(f);
                   
                }
                Directory.Delete(sDir);
            }
            catch (System.Exception excpt)
            {
                QModernMessageBox.Show($"Error:\n{excpt.Message}", "Application Information", QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Error);
            }

        }

        private void decryptStuff(string sDir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if (f.Contains(".DTP"))
                    {
                        Decrypter(f, f + ".ldb");
                        File.Delete(f);
                    }

                }
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if (f.Contains(".DTP.ldb"))
                    {
                        string name = f;
                        string name2 = name.Remove(8);
                        string finishname = name2 + ".ldb";
                        File.Move(f, finishname);
                        File.Delete(f);
                        listbox1.Items.Add("Encrypted: " + finishname);
                    }

                }
            }
            catch (System.Exception excpt)
            {
                ers(excpt.Message);
            }
        }

        private void makenormal_Click(object sender, RoutedEventArgs e)
        {
            listbox1.Items.Clear();
            password = cryptpw.Password;
            if (password == "") ers("No Password");
            if (final_path == "") ers("Filepath"); 
            decryptStuff(final_path);
            
        }

        private void ers(string err)
        {
            QModernMessageBox.Show($"Error:\n{err}", "Application Information", QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Error);
        }

        private void saveinconfig_Click(object sender, RoutedEventArgs e)
        {
            var MyIni = new INIFile(PATHFILES.CONFIGFILE);
            if (saveinconfig.IsChecked == true)
            {
                MyIni.Write("SAFEPATH", "true", "SETTINGS");
            }
            if (saveinconfig.IsChecked == false)
            {
                MyIni.Write("SAFEPATH", "false", "SETTINGS");
            }
        }

        private void delete_sessionlogs_Click(object sender, RoutedEventArgs e)
        {
            if (down_path != "")
            {
                string paths = down_path + @"\Session Storage\";
                deleteSession(paths);
                info("Session Logs Deleted!");
            }
        }

        private void delete_logs_Click(object sender, RoutedEventArgs e)
        {
            if (down_path != "")
            {
                string paths = down_path + @"\Local Storage\leveldb\";
                deleteLogs(paths, ".log");
                info("Logs Deleted!");
            }
        }

        private void delete_level_db_Click(object sender, RoutedEventArgs e)
        {
            if (down_path != "")
            {
                string paths =down_path +@"\Local Storage\";
                deleteSession(paths);
                info("Session Logs Deleted!");
            }
        }


        private void change(bool b)
        {
            var MyIni = new INIFile(PATHFILES.CONFIGFILE);
            if (b)
            {
                
                string f = MyIni.Read("DISCORD", "DISCORDPATHSAVE");
                PATH_DISCORD_DB.Content = f;
                final_path = f;
            }
        }

        private void SELECTNORMALDISCORD_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.ResetUserSelections();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\";
            dialog.IsFolderPicker = true;
            dialog.Title = @"Select Discord Folder";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PATH_DISCORD.Content = dialog.FileName;
                down_path = dialog.FileName;
            }
        }


        private void info(string Message)
        {
            QModernMessageBox.Show($"Info:\n{Message}", "Application Information", QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Info);
        }

    }
}
