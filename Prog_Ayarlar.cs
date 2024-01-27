// Decompiled with JetBrains decompiler
// Type: EsdTurnikesi.Ayarlar
// Assembly: EsdTurnikesi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C8099926-BBEB-495E-ADF6-36B4F5F75BE8
// Assembly location: C:\Users\serkan.baki\Desktop\esd-rar\ESD\Release\EsdTurnikesi.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace Vektor_Paketleme
{
    [CompilerGenerated]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed class Prog_Ayarlar : ApplicationSettingsBase
    {
        private static Prog_Ayarlar defaultInstance = (Prog_Ayarlar)SettingsBase.Synchronized((SettingsBase)new Prog_Ayarlar());

        private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
        {
        }

        private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
        {
        }

        public static Prog_Ayarlar Default
        {
            get
            {
                return Prog_Ayarlar.defaultInstance;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("91")]
        public string companyNo
        {
            get
            {
                return (string)this[nameof(companyNo)];
            }
            set
            {
                this[nameof(companyNo)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("2500001000")]
        public string SAPNo
        {
            get
            {
                return (string)this[nameof(SAPNo)];
            }
            set
            {
                this[nameof(SAPNo)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("10000000000000")]
        public string productionNo
        {
            get
            {
                return (string)this[nameof(productionNo)];
            }
            set
            {
                this[nameof(productionNo)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("05")]
        public string cardNo
        {
            get
            {
                return (string)this[nameof(cardNo)];
            }
            set
            {
                this[nameof(cardNo)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("06")]
        public string gerberVer
        {
            get
            {
                return (string)this[nameof(gerberVer)];
            }
            set
            {
                this[nameof(gerberVer)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("03")]
        public string BOMVer
        {
            get
            {
                return (string)this[nameof(BOMVer)];
            }
            set
            {
                this[nameof(BOMVer)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("01")]
        public string ICTRev
        {
            get
            {
                return (string)this[nameof(ICTRev)];
            }
            set
            {
                this[nameof(ICTRev)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("01")]
        public string FCTRev
        {
            get
            {
                return (string)this[nameof(FCTRev)];
            }
            set
            {
                this[nameof(FCTRev)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("01")]
        public string softwareVer
        {
            get
            {
                return (string)this[nameof(softwareVer)];
            }
            set
            {
                this[nameof(softwareVer)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("04")]
        public string softwareRev
        {
            get
            {
                return (string)this[nameof(softwareRev)];
            }
            set
            {
                this[nameof(softwareRev)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("2SPZZZ05001329")]
        public string specialCode
        {
            get
            {
                return (string)this[nameof(specialCode)];
            }
            set
            {
                this[nameof(specialCode)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool cbError
        {
            get
            {
                return (bool)this[nameof(cbError)];
            }
            set
            {
                this[nameof(cbError)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string iniDosyaYolu
        {
            get
            {
                return (string)this[nameof(iniDosyaYolu)];
            }
            set
            {
                this[nameof(iniDosyaYolu)] = (object)value;
            }
        }


        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string printerName
        {
            get
            {
                return (string)this[nameof(printerName)];
            }
            set
            {
                this[nameof(printerName)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string printerPos
        {
            get
            {
                return (string)this[nameof(printerPos)];
            }
            set
            {
                this[nameof(printerPos)] = (object)value;
            }
        }


        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("1903")]
        public string adminSifre
        {
            get
            {
                return (string)this[nameof(adminSifre)];
            }
            set
            {
                this[nameof(adminSifre)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("1903")]
        public string kaliteSifre
        {
            get
            {
                return (string)this[nameof(kaliteSifre)];
            }
            set
            {
                this[nameof(kaliteSifre)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string projectName
        {
            get
            {
                return (string)this[nameof(projectName)];
            }
            set
            {
                this[nameof(projectName)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("Kitaplıklar\\Belgeler\\logpersonel.accdb")]
        public string PNGdosyayolu
        {
            get
            {
                return (string)this[nameof(PNGdosyayolu)];
            }
            set
            {
                this[nameof(PNGdosyayolu)] = (object)value;
            }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("1000")]
        public int timerAdmin
        {
            get
            {
                return (int)this[nameof(timerAdmin)];
            }
            set
            {
                this[nameof(timerAdmin)] = (object)value;
            }
        }
    }
}
