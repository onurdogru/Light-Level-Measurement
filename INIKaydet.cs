using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LedBarKirmaMakinesi
{
    public class INIKaydet
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public INIKaydet(string dosyaYolu)
        {
            DOSYAYOLU = dosyaYolu;
        }
        private string DOSYAYOLU = String.Empty;
        public string Varsayilan { get; set; }
        public string Oku(string bolum, string ayaradi)
        {
            Varsayilan = Varsayilan ?? string.Empty;
            StringBuilder StrBuild = new StringBuilder(256);
            GetPrivateProfileString(bolum, ayaradi, Varsayilan, StrBuild, 255, DOSYAYOLU);
            return StrBuild.ToString();
        }
        public long Yaz(string bolum, string ayaradi, string deger)
        {
            return WritePrivateProfileString(bolum, ayaradi, deger, DOSYAYOLU);
        }
        public long Sil(string bolum, string ayaradi, string deger)
        {
            return WritePrivateProfileString(bolum, ayaradi, deger, DOSYAYOLU);
        }
    }
}
