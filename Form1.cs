using FeasaLib;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace LedBarKirmaMakinesi
{
    public partial class Main : Form
    {
        public bool boolReadStatus;

        int readLoopCounter = 0, sayac1 = 0, sayac2 = 0, sayac3 = 0, sayac4 = 0, sayac5 = 0, sayac6 = 0, sayac7 = 0, sayac8 = 0, sayacFlag = 0;

        Thread threadWriteBool, threadWriteString, threadPushButton, threadReadLoop;

        static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string modelFolderPath = desktopPath + @"\Debug";
        string modelFilePath = desktopPath + @"\Modeller.ini";

        INIKaydet ini;

        byte[] array = new byte[4096];

        int counterByte = 0;

        byte[] byteArrayAS = new byte[10];
        byte[] byteArrayVS = new byte[10];

        Color[,] ButtonIndex = new Color[13, 15];

        public Main()
        {
            InitializeComponent();
            InitializeDataGridView();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

            threadWriteBool = new Thread(() => nxCompoletBoolWrite("connectionstart", true));
            threadWriteBool.Start();
            Thread.Sleep(100);

            threadWriteBool = new Thread(() => nxCompoletStringWrite("calismaMod", "0"));
            threadWriteBool.Start();
            Thread.Sleep(100);

            pctrBxPlcState.BackColor = nxCompoletBoolRead("connectionOk") && boolReadStatus == false ? Color.Green : Color.Red;

            threadWriteString = new Thread(() => nxCompoletStringWrite("jogHiz", "10"));
            threadWriteString.Start();

            threadWriteString = new Thread(() => nxCompoletStringWrite("jogAralik", "0"));
            threadWriteString.Start();

            txtUretilenAdet.Text = nxCompoletStringRead("uretilenAdet");

            txtCevrimSure.Text = nxCompoletStringRead("cevrimSure");


            for (int i = 1; i <= 100; i++)
            {
                if (FeasaCom.IsPortAvailable(i) == 1)
                {
                    cmbPortsName.Items.Add(i.ToString());
                    cmbPortsName2.Items.Add(i.ToString());
                }
            }

            timer1.Start();

            //cmbPortsName.SelectedIndex = 7;
            //cmbPortsName2.SelectedIndex = 8;
            //cmbJogAralik.SelectedIndex = 0;
        }

        //*****İsteklerin haricindeki fonksiyonlar ve methodlar
        #region İşlemler
        private void timer1_Tick(object sender, EventArgs e)
        {
            threadReadLoop = new Thread(() => readPlcData());
            threadReadLoop.Start();
        }
        private void btnHomeYap_Click(object sender, EventArgs e)
        {
            cmbCalismaSecim.SelectedIndex = 0;
            threadWriteString = new Thread(() => nxCompoletBoolWrite("homeButon", true));
            threadWriteString.Start();
        }

        private void txtJogHiz_TextChanged(object sender, EventArgs e)
        {
            threadWriteString = new Thread(() => nxCompoletStringWrite("jogHiz", txtJogHiz.Text.ToString()));
            threadWriteString.Start();
        }

        private void cmbJogAralik_SelectedIndexChanged(object sender, EventArgs e)
        {
            threadWriteString = new Thread(() => nxCompoletStringWrite("jogAralik", cmbJogAralik.SelectedIndex.ToString()));
            threadWriteString.Start();
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (e.Page == xtraTabPage1)
            {
                lblLEDBARKIRMAMAKİNESİ.Text = "LEDBAR KIRMA VE TEST MAKİNASI";

                CheckR();
                CheckG();
                CheckB();
                CheckX();
                CheckY();
                CheckU();
                CheckV();
                CheckSat();
                CheckHue();
                CheckInt();
                CheckCct();
                CheckWave();
                CheckAkimVoltaj();

                if ((cmbLedOlcum.Text == "VAR") || (cmbAkimVoltaj.Text == "VAR"))
                    CheckTest();
            }
            else if (e.Page == xtraTabPage2)
            {
                lblLEDBARKIRMAMAKİNESİ.Text = "KART AYARLAR";

                InitializeResetButtons();
            }
            else if (e.Page == xtraTabPage3)
            {
                lblLEDBARKIRMAMAKİNESİ.Text = "FEASA AYARLAR";
                InitializeResetButtons();
            }

        }

        private void InitializeDataGridView()
        {
            dgwFeasaSettings.AllowUserToAddRows = false; // Kullanıcıdan yeni satır eklemesini engeller
            dgwFeasaSettings.AllowUserToDeleteRows = false; // Kullanıcıdan satır silmesini engeller
            dgwFeasaSettings.AllowUserToResizeColumns = false;
            dgwFeasaSettings.AllowUserToResizeRows = false;
            dgwFeasaSettings.AllowUserToOrderColumns = false;
            dgwFeasaSettings.RowHeadersVisible = false; // Satır başlıklarını gizler
            dgwFeasaSettings.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgwFeasaSettings.RowCount = 12;
            dgwFeasaSettings.Rows[0].Cells[0].Value = "R";
            dgwFeasaSettings.Rows[1].Cells[0].Value = "G";
            dgwFeasaSettings.Rows[2].Cells[0].Value = "B";
            dgwFeasaSettings.Rows[3].Cells[0].Value = "CIE x";
            dgwFeasaSettings.Rows[4].Cells[0].Value = "CIE y";
            dgwFeasaSettings.Rows[5].Cells[0].Value = "CIE u";
            dgwFeasaSettings.Rows[6].Cells[0].Value = "CIE v";
            dgwFeasaSettings.Rows[7].Cells[0].Value = "SATURATION";
            dgwFeasaSettings.Rows[8].Cells[0].Value = "HUE";
            dgwFeasaSettings.Rows[9].Cells[0].Value = "INTENSITY";
            dgwFeasaSettings.Rows[10].Cells[0].Value = "CCT";
            dgwFeasaSettings.Rows[11].Cells[0].Value = "WAVELENGTH";

            dgwFeasaSettings.ReadOnly = true;
        }

        private void InitializeButtons()
        {
            for (int i = 0; i < 1; i++)
            {
                for (int k = 0; k < 1; k++)
                {

                    ButtonIndex[k, i] = btnRLed1.Appearance.BackColor;
                    ButtonIndex[k + 1, i] = btnGLed1.Appearance.BackColor;
                    ButtonIndex[k + 2, i] = btnBLed1.Appearance.BackColor;
                    ButtonIndex[k + 3, i] = btnCieXLed1.Appearance.BackColor;
                    ButtonIndex[k + 4, i] = btnCieYLed1.Appearance.BackColor;
                    ButtonIndex[k + 5, i] = btnCieULed1.Appearance.BackColor;
                    ButtonIndex[k + 6, i] = btnCieVLed1.Appearance.BackColor;
                    ButtonIndex[k + 7, i] = btnSAT1.Appearance.BackColor;
                    ButtonIndex[k + 8, i] = btnHUE1.Appearance.BackColor;
                    ButtonIndex[k + 9, i] = btnINT1.Appearance.BackColor;
                    ButtonIndex[k + 10, i] = btnCCT1.Appearance.BackColor;
                    ButtonIndex[k + 11, i] = btnWAVE1.Appearance.BackColor;

                    ButtonIndex[k, i + 1] = btnRLed2.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 1] = btnGLed2.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 1] = btnBLed2.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 1] = btnCieXLed2.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 1] = btnCieYLed2.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 1] = btnCieULed2.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 1] = btnCieVLed2.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 1] = btnSAT2.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 1] = btnHUE2.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 1] = btnINT2.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 1] = btnCCT2.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 1] = btnWAVE2.Appearance.BackColor;

                    ButtonIndex[k, i + 2] = btnRLed3.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 2] = btnGLed3.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 2] = btnBLed3.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 2] = btnCieXLed3.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 2] = btnCieYLed3.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 2] = btnCieULed3.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 2] = btnCieVLed3.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 2] = btnSAT3.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 2] = btnHUE3.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 2] = btnINT3.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 2] = btnCCT3.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 2] = btnWAVE3.Appearance.BackColor;

                    ButtonIndex[k, i + 3] = btnRLed4.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 3] = btnGLed4.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 3] = btnBLed4.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 3] = btnCieXLed4.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 3] = btnCieYLed4.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 3] = btnCieULed4.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 3] = btnCieVLed4.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 3] = btnSAT4.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 3] = btnHUE4.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 3] = btnINT4.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 3] = btnCCT4.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 3] = btnWAVE4.Appearance.BackColor;

                    ButtonIndex[k, i + 4] = btnRLed5.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 4] = btnGLed5.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 4] = btnBLed5.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 4] = btnCieXLed5.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 4] = btnCieYLed5.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 4] = btnCieULed5.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 4] = btnCieVLed5.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 4] = btnSAT5.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 4] = btnHUE5.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 4] = btnINT5.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 4] = btnCCT5.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 4] = btnWAVE5.Appearance.BackColor;

                    ButtonIndex[k, i + 5] = btnRLed6.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 5] = btnGLed6.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 5] = btnBLed6.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 5] = btnCieXLed6.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 5] = btnCieYLed6.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 5] = btnCieULed6.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 5] = btnCieVLed6.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 5] = btnSAT6.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 5] = btnHUE6.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 5] = btnINT6.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 5] = btnCCT6.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 5] = btnWAVE6.Appearance.BackColor;

                    ButtonIndex[k, i + 6] = btnRLed7.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 6] = btnGLed7.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 6] = btnBLed7.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 6] = btnCieXLed7.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 6] = btnCieYLed7.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 6] = btnCieULed7.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 6] = btnCieVLed7.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 6] = btnSAT7.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 6] = btnHUE7.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 6] = btnINT7.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 6] = btnCCT7.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 6] = btnWAVE7.Appearance.BackColor;

                    ButtonIndex[k, i + 7] = btnRLed8.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 7] = btnGLed8.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 7] = btnBLed8.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 7] = btnCieXLed8.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 7] = btnCieYLed8.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 7] = btnCieULed8.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 7] = btnCieVLed8.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 7] = btnSAT8.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 7] = btnHUE8.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 7] = btnINT8.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 7] = btnCCT8.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 7] = btnWAVE8.Appearance.BackColor;

                    ButtonIndex[k, i + 8] = btnRLed9.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 8] = btnGLed9.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 8] = btnBLed9.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 8] = btnCieXLed9.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 8] = btnCieYLed9.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 8] = btnCieULed9.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 8] = btnCieVLed9.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 8] = btnSAT9.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 8] = btnHUE9.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 8] = btnINT9.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 8] = btnCCT9.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 8] = btnWAVE9.Appearance.BackColor;

                    ButtonIndex[k, i + 9] = btnRLed10.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 9] = btnGLed10.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 9] = btnBLed10.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 9] = btnCieXLed10.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 9] = btnCieYLed10.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 9] = btnCieULed10.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 9] = btnCieVLed10.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 9] = btnSAT10.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 9] = btnHUE10.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 9] = btnINT10.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 9] = btnCCT10.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 9] = btnWAVE10.Appearance.BackColor;

                    ButtonIndex[k, i + 10] = btnRLed11.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 10] = btnGLed11.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 10] = btnBLed11.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 10] = btnCieXLed11.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 10] = btnCieYLed11.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 10] = btnCieULed11.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 10] = btnCieVLed11.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 10] = btnSAT11.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 10] = btnHUE11.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 10] = btnINT11.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 10] = btnCCT11.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 10] = btnWAVE11.Appearance.BackColor;

                    ButtonIndex[k, i + 11] = btnRLed12.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 11] = btnGLed12.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 11] = btnBLed12.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 11] = btnCieXLed12.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 11] = btnCieYLed12.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 11] = btnCieULed12.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 11] = btnCieVLed12.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 11] = btnSAT12.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 11] = btnHUE12.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 11] = btnINT12.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 11] = btnCCT12.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 11] = btnWAVE12.Appearance.BackColor;

                    ButtonIndex[k, i + 12] = btnRLed13.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 12] = btnGLed13.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 12] = btnBLed13.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 12] = btnCieXLed13.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 12] = btnCieYLed13.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 12] = btnCieULed13.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 12] = btnCieVLed13.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 12] = btnSAT13.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 12] = btnHUE13.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 12] = btnINT13.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 12] = btnCCT13.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 12] = btnWAVE13.Appearance.BackColor;

                    ButtonIndex[k, i + 13] = btnRLed14.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 13] = btnGLed14.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 13] = btnBLed14.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 13] = btnCieXLed14.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 13] = btnCieYLed14.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 13] = btnCieULed14.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 13] = btnCieVLed14.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 13] = btnSAT14.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 13] = btnHUE14.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 13] = btnINT14.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 13] = btnCCT14.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 13] = btnWAVE14.Appearance.BackColor;

                    ButtonIndex[k, i + 14] = btnRLed15.Appearance.BackColor;
                    ButtonIndex[k + 1, i + 14] = btnGLed15.Appearance.BackColor;
                    ButtonIndex[k + 2, i + 14] = btnBLed15.Appearance.BackColor;
                    ButtonIndex[k + 3, i + 14] = btnCieXLed15.Appearance.BackColor;
                    ButtonIndex[k + 4, i + 14] = btnCieYLed15.Appearance.BackColor;
                    ButtonIndex[k + 5, i + 14] = btnCieULed15.Appearance.BackColor;
                    ButtonIndex[k + 6, i + 14] = btnCieVLed15.Appearance.BackColor;
                    ButtonIndex[k + 7, i + 14] = btnSAT15.Appearance.BackColor;
                    ButtonIndex[k + 8, i + 14] = btnHUE15.Appearance.BackColor;
                    ButtonIndex[k + 9, i + 14] = btnINT15.Appearance.BackColor;
                    ButtonIndex[k + 10, i + 14] = btnCCT15.Appearance.BackColor;
                    ButtonIndex[k + 11, i + 14] = btnWAVE15.Appearance.BackColor;

                    if (cmbLedOlcum.Text == "VAR")
                        ButtonIndex[12, i] = btnKartAkimVoltaj.Appearance.BackColor;

                }
            }
        }

        private void InitializeResetButtons()
        {
            for (int i = 0; i < 1; i++)
            {
                for (int k = 0; k < 1; k++)
                {
                    btnRLed1.Appearance.BackColor = Color.White;
                    btnRLed2.Appearance.BackColor = Color.White;
                    btnRLed3.Appearance.BackColor = Color.White;
                    btnRLed4.Appearance.BackColor = Color.White;
                    btnRLed5.Appearance.BackColor = Color.White;
                    btnRLed6.Appearance.BackColor = Color.White;
                    btnRLed7.Appearance.BackColor = Color.White;
                    btnRLed8.Appearance.BackColor = Color.White;
                    btnRLed9.Appearance.BackColor = Color.White;
                    btnRLed10.Appearance.BackColor = Color.White;
                    btnRLed11.Appearance.BackColor = Color.White;
                    btnRLed12.Appearance.BackColor = Color.White;
                    btnRLed13.Appearance.BackColor = Color.White;
                    btnRLed14.Appearance.BackColor = Color.White;
                    btnRLed15.Appearance.BackColor = Color.White;

                    btnGLed1.Appearance.BackColor = Color.White;
                    btnGLed2.Appearance.BackColor = Color.White;
                    btnGLed3.Appearance.BackColor = Color.White;
                    btnGLed4.Appearance.BackColor = Color.White;
                    btnGLed5.Appearance.BackColor = Color.White;
                    btnGLed6.Appearance.BackColor = Color.White;
                    btnGLed7.Appearance.BackColor = Color.White;
                    btnGLed8.Appearance.BackColor = Color.White;
                    btnGLed9.Appearance.BackColor = Color.White;
                    btnGLed10.Appearance.BackColor = Color.White;
                    btnGLed11.Appearance.BackColor = Color.White;
                    btnGLed12.Appearance.BackColor = Color.White;
                    btnGLed13.Appearance.BackColor = Color.White;
                    btnGLed14.Appearance.BackColor = Color.White;
                    btnGLed15.Appearance.BackColor = Color.White;

                    btnBLed1.Appearance.BackColor = Color.White;
                    btnBLed2.Appearance.BackColor = Color.White;
                    btnBLed3.Appearance.BackColor = Color.White;
                    btnBLed4.Appearance.BackColor = Color.White;
                    btnBLed5.Appearance.BackColor = Color.White;
                    btnBLed6.Appearance.BackColor = Color.White;
                    btnBLed7.Appearance.BackColor = Color.White;
                    btnBLed8.Appearance.BackColor = Color.White;
                    btnBLed9.Appearance.BackColor = Color.White;
                    btnBLed10.Appearance.BackColor = Color.White;
                    btnBLed11.Appearance.BackColor = Color.White;
                    btnBLed12.Appearance.BackColor = Color.White;
                    btnBLed13.Appearance.BackColor = Color.White;
                    btnBLed14.Appearance.BackColor = Color.White;
                    btnBLed15.Appearance.BackColor = Color.White;

                    btnCieXLed1.Appearance.BackColor = Color.White;
                    btnCieXLed2.Appearance.BackColor = Color.White;
                    btnCieXLed3.Appearance.BackColor = Color.White;
                    btnCieXLed4.Appearance.BackColor = Color.White;
                    btnCieXLed5.Appearance.BackColor = Color.White;
                    btnCieXLed6.Appearance.BackColor = Color.White;
                    btnCieXLed7.Appearance.BackColor = Color.White;
                    btnCieXLed8.Appearance.BackColor = Color.White;
                    btnCieXLed9.Appearance.BackColor = Color.White;
                    btnCieXLed10.Appearance.BackColor = Color.White;
                    btnCieXLed11.Appearance.BackColor = Color.White;
                    btnCieXLed12.Appearance.BackColor = Color.White;
                    btnCieXLed13.Appearance.BackColor = Color.White;
                    btnCieXLed14.Appearance.BackColor = Color.White;
                    btnCieXLed15.Appearance.BackColor = Color.White;

                    btnCieYLed1.Appearance.BackColor = Color.White;
                    btnCieYLed2.Appearance.BackColor = Color.White;
                    btnCieYLed3.Appearance.BackColor = Color.White;
                    btnCieYLed4.Appearance.BackColor = Color.White;
                    btnCieYLed5.Appearance.BackColor = Color.White;
                    btnCieYLed6.Appearance.BackColor = Color.White;
                    btnCieYLed7.Appearance.BackColor = Color.White;
                    btnCieYLed8.Appearance.BackColor = Color.White;
                    btnCieYLed9.Appearance.BackColor = Color.White;
                    btnCieYLed10.Appearance.BackColor = Color.White;
                    btnCieYLed11.Appearance.BackColor = Color.White;
                    btnCieYLed12.Appearance.BackColor = Color.White;
                    btnCieYLed13.Appearance.BackColor = Color.White;
                    btnCieYLed14.Appearance.BackColor = Color.White;
                    btnCieYLed15.Appearance.BackColor = Color.White;

                    btnCieULed1.Appearance.BackColor = Color.White;
                    btnCieULed2.Appearance.BackColor = Color.White;
                    btnCieULed3.Appearance.BackColor = Color.White;
                    btnCieULed4.Appearance.BackColor = Color.White;
                    btnCieULed5.Appearance.BackColor = Color.White;
                    btnCieULed6.Appearance.BackColor = Color.White;
                    btnCieULed7.Appearance.BackColor = Color.White;
                    btnCieULed8.Appearance.BackColor = Color.White;
                    btnCieULed9.Appearance.BackColor = Color.White;
                    btnCieULed10.Appearance.BackColor = Color.White;
                    btnCieULed11.Appearance.BackColor = Color.White;
                    btnCieULed12.Appearance.BackColor = Color.White;
                    btnCieULed13.Appearance.BackColor = Color.White;
                    btnCieULed14.Appearance.BackColor = Color.White;
                    btnCieULed15.Appearance.BackColor = Color.White;

                    btnCieVLed1.Appearance.BackColor = Color.White;
                    btnCieVLed2.Appearance.BackColor = Color.White;
                    btnCieVLed3.Appearance.BackColor = Color.White;
                    btnCieVLed4.Appearance.BackColor = Color.White;
                    btnCieVLed5.Appearance.BackColor = Color.White;
                    btnCieVLed6.Appearance.BackColor = Color.White;
                    btnCieVLed7.Appearance.BackColor = Color.White;
                    btnCieVLed8.Appearance.BackColor = Color.White;
                    btnCieVLed9.Appearance.BackColor = Color.White;
                    btnCieVLed10.Appearance.BackColor = Color.White;
                    btnCieVLed11.Appearance.BackColor = Color.White;
                    btnCieVLed12.Appearance.BackColor = Color.White;
                    btnCieVLed13.Appearance.BackColor = Color.White;
                    btnCieVLed14.Appearance.BackColor = Color.White;
                    btnCieVLed15.Appearance.BackColor = Color.White;

                    btnSAT1.Appearance.BackColor = Color.White;
                    btnSAT2.Appearance.BackColor = Color.White;
                    btnSAT3.Appearance.BackColor = Color.White;
                    btnSAT4.Appearance.BackColor = Color.White;
                    btnSAT5.Appearance.BackColor = Color.White;
                    btnSAT6.Appearance.BackColor = Color.White;
                    btnSAT7.Appearance.BackColor = Color.White;
                    btnSAT8.Appearance.BackColor = Color.White;
                    btnSAT9.Appearance.BackColor = Color.White;
                    btnSAT10.Appearance.BackColor = Color.White;
                    btnSAT11.Appearance.BackColor = Color.White;
                    btnSAT12.Appearance.BackColor = Color.White;
                    btnSAT13.Appearance.BackColor = Color.White;
                    btnSAT14.Appearance.BackColor = Color.White;
                    btnSAT15.Appearance.BackColor = Color.White;

                    btnHUE1.Appearance.BackColor = Color.White;
                    btnHUE2.Appearance.BackColor = Color.White;
                    btnHUE3.Appearance.BackColor = Color.White;
                    btnHUE4.Appearance.BackColor = Color.White;
                    btnHUE5.Appearance.BackColor = Color.White;
                    btnHUE6.Appearance.BackColor = Color.White;
                    btnHUE7.Appearance.BackColor = Color.White;
                    btnHUE8.Appearance.BackColor = Color.White;
                    btnHUE9.Appearance.BackColor = Color.White;
                    btnHUE10.Appearance.BackColor = Color.White;
                    btnHUE11.Appearance.BackColor = Color.White;
                    btnHUE12.Appearance.BackColor = Color.White;
                    btnHUE13.Appearance.BackColor = Color.White;
                    btnHUE14.Appearance.BackColor = Color.White;
                    btnHUE15.Appearance.BackColor = Color.White;

                    btnINT1.Appearance.BackColor = Color.White;
                    btnINT2.Appearance.BackColor = Color.White;
                    btnINT3.Appearance.BackColor = Color.White;
                    btnINT4.Appearance.BackColor = Color.White;
                    btnINT5.Appearance.BackColor = Color.White;
                    btnINT6.Appearance.BackColor = Color.White;
                    btnINT7.Appearance.BackColor = Color.White;
                    btnINT8.Appearance.BackColor = Color.White;
                    btnINT9.Appearance.BackColor = Color.White;
                    btnINT10.Appearance.BackColor = Color.White;
                    btnINT11.Appearance.BackColor = Color.White;
                    btnINT12.Appearance.BackColor = Color.White;
                    btnINT13.Appearance.BackColor = Color.White;
                    btnINT14.Appearance.BackColor = Color.White;
                    btnINT15.Appearance.BackColor = Color.White;

                    btnCCT1.Appearance.BackColor = Color.White;
                    btnCCT2.Appearance.BackColor = Color.White;
                    btnCCT3.Appearance.BackColor = Color.White;
                    btnCCT4.Appearance.BackColor = Color.White;
                    btnCCT5.Appearance.BackColor = Color.White;
                    btnCCT6.Appearance.BackColor = Color.White;
                    btnCCT7.Appearance.BackColor = Color.White;
                    btnCCT8.Appearance.BackColor = Color.White;
                    btnCCT9.Appearance.BackColor = Color.White;
                    btnCCT10.Appearance.BackColor = Color.White;
                    btnCCT11.Appearance.BackColor = Color.White;
                    btnCCT12.Appearance.BackColor = Color.White;
                    btnCCT13.Appearance.BackColor = Color.White;
                    btnCCT14.Appearance.BackColor = Color.White;
                    btnCCT15.Appearance.BackColor = Color.White;

                    btnWAVE1.Appearance.BackColor = Color.White;
                    btnWAVE2.Appearance.BackColor = Color.White;
                    btnWAVE3.Appearance.BackColor = Color.White;
                    btnWAVE4.Appearance.BackColor = Color.White;
                    btnWAVE5.Appearance.BackColor = Color.White;
                    btnWAVE6.Appearance.BackColor = Color.White;
                    btnWAVE7.Appearance.BackColor = Color.White;
                    btnWAVE8.Appearance.BackColor = Color.White;
                    btnWAVE9.Appearance.BackColor = Color.White;
                    btnWAVE10.Appearance.BackColor = Color.White;
                    btnWAVE11.Appearance.BackColor = Color.White;
                    btnWAVE12.Appearance.BackColor = Color.White;
                    btnWAVE13.Appearance.BackColor = Color.White;
                    btnWAVE14.Appearance.BackColor = Color.White;
                    btnWAVE15.Appearance.BackColor = Color.White;

                    btnKartAkimVoltaj.Appearance.BackColor = Color.White;

                    _ = ButtonIndex[i, k] == Color.White;
                }
            }

            txtTestSon.Text = null;
        }

        public void CheckTest()
        {
            InitializeButtons();
            Thread.Sleep(10);

            for (int i = 0; i < 15; i++)
            {
                for (int k = 0; k < 13; k++)
                {
                    if (ButtonIndex[k, i] == Color.Red)
                    {
                        sayacFlag++;
                    }
                }
            }

            if (sayacFlag >= 1)
            {
                txtTestSon.Text = "NOT OK";
                txtTestSon.ForeColor = Color.Red;
            }
            else
            {
                txtTestSon.Text = "OK";
                txtTestSon.ForeColor = Color.Green;
            }
        }

        public void CheckAkimVoltaj()
        {
            if ((txtVoltajMIN.Text != "") && (txtVoltajMAX.Text != "") && (txtAkimMIN.Text != "") && (txtAkimMAX.Text != "") && (txtVoltajOkunan.Text != "") && (txtAkimOkunan.Text != ""))
            {
                double.TryParse(txtVoltajOkunan.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double a);
                double.TryParse(txtVoltajMIN.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double b);
                double.TryParse(txtVoltajMAX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double c);
                double.TryParse(txtAkimOkunan.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double d);
                double.TryParse(txtAkimMIN.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double e);
                double.TryParse(txtAkimMAX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double f);

                if ((a > b) && (a < c) && (d > e) && (d < f))
                {
                    btnKartAkimVoltaj.Appearance.BackColor = Color.Green;
                }
                else
                {
                    btnKartAkimVoltaj.Appearance.BackColor = Color.Red;
                }
            }
        }

        public void CheckR()
        {
            //****R
            try
            {

                if (dgwFeasaSettings.Rows[0].Cells[1].Value != null)
                {
                    btnRLed1.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[1].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[1].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[2].Value != null)
                {
                    btnRLed2.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[2].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[2].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[3].Value != null)
                {
                    btnRLed3.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[3].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[3].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[4].Value != null)
                {
                    btnRLed4.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[4].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[4].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[5].Value != null)
                {
                    btnRLed5.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[5].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[5].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[6].Value != null)
                {
                    btnRLed6.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[6].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[6].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[7].Value != null)
                {
                    btnRLed7.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[7].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[7].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[8].Value != null)
                {
                    btnRLed8.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[8].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[8].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[9].Value != null)
                {
                    btnRLed9.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[9].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[9].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[10].Value != null)
                {
                    btnRLed10.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[10].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[10].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[11].Value != null)
                {
                    btnRLed11.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[11].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[11].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[12].Value != null)
                {
                    btnRLed12.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[12].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[12].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[13].Value != null)
                {
                    btnRLed13.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[13].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[13].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[14].Value != null)
                {
                    btnRLed14.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[14].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[14].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[0].Cells[15].Value != null)
                {
                    btnRLed15.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[0].Cells[15].Value.ToString()) < Convert.ToInt32(txtRMax.Text) && int.Parse(dgwFeasaSettings.Rows[0].Cells[15].Value.ToString()) > Convert.ToInt32(txtRMin.Text)
                        ? Color.Green
                        : Color.Red;
                }


            }
            catch (Exception)
            {
                MessageBox.Show("R Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");
                return;

            }
        }

        public void CheckG()
        {
            //****G
            try
            {

                if (dgwFeasaSettings.Rows[1].Cells[1].Value != null)
                {
                    btnGLed1.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[1].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[1].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[2].Value != null)
                {
                    btnGLed2.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[2].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[2].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[3].Value != null)
                {
                    btnGLed3.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[3].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[3].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[4].Value != null)
                {
                    btnGLed4.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[4].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[4].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[5].Value != null)
                {
                    btnGLed5.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[5].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[5].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[6].Value != null)
                {
                    btnGLed6.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[6].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[6].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[7].Value != null)
                {
                    btnGLed7.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[7].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[7].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[8].Value != null)
                {
                    btnGLed8.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[8].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[8].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[9].Value != null)
                {
                    btnGLed9.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[9].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[9].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[10].Value != null)
                {
                    btnGLed10.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[10].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[10].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[11].Value != null)
                {
                    btnGLed11.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[11].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[11].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[12].Value != null)
                {
                    btnGLed12.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[12].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[12].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[13].Value != null)
                {
                    btnGLed13.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[13].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[13].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[14].Value != null)
                {
                    btnGLed14.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[14].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[14].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[1].Cells[15].Value != null)
                {
                    btnGLed15.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[1].Cells[15].Value.ToString()) < Convert.ToInt32(txtGMax.Text) && int.Parse(dgwFeasaSettings.Rows[1].Cells[15].Value.ToString()) > Convert.ToInt32(txtGMin.Text)
                        ? Color.Green
                        : Color.Red;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("G Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckB()
        {
            //****B
            try
            {

                if (dgwFeasaSettings.Rows[2].Cells[1].Value != null)
                {
                    btnBLed1.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[1].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[1].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[2].Value != null)
                {
                    btnBLed2.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[2].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[2].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[3].Value != null)
                {
                    btnBLed3.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[3].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[3].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[4].Value != null)
                {
                    btnBLed4.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[4].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[4].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[5].Value != null)
                {
                    btnBLed5.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[5].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[5].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[6].Value != null)
                {
                    btnBLed6.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[6].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[6].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[7].Value != null)
                {
                    btnBLed7.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[7].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[7].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[8].Value != null)
                {
                    btnBLed8.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[8].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[8].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[9].Value != null)
                {
                    btnBLed9.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[9].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[9].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[10].Value != null)
                {
                    btnBLed10.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[10].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[10].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[11].Value != null)
                {
                    btnBLed11.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[11].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[11].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[12].Value != null)
                {
                    btnBLed12.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[12].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[12].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[13].Value != null)
                {
                    btnBLed13.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[13].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[13].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[14].Value != null)
                {
                    btnBLed14.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[14].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[14].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[2].Cells[15].Value != null)
                {
                    btnBLed15.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[2].Cells[15].Value.ToString()) < Convert.ToInt32(txtBMax.Text) && int.Parse(dgwFeasaSettings.Rows[2].Cells[15].Value.ToString()) > Convert.ToInt32(txtBMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("B Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckX()
        {
            //****X
            try
            {
                if (dgwFeasaSettings.Rows[3].Cells[1].Value != null)
                {
                    btnCieXLed1.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[1].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[1].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[2].Value != null)
                {
                    btnCieXLed2.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[2].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[2].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[3].Value != null)
                {
                    btnCieXLed3.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[3].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[3].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[4].Value != null)
                {
                    btnCieXLed4.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[4].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[4].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[5].Value != null)
                {
                    btnCieXLed5.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[5].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[5].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[6].Value != null)
                {
                    btnCieXLed6.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[6].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[6].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[7].Value != null)
                {
                    btnCieXLed7.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[7].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[7].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[8].Value != null)
                {
                    btnCieXLed8.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[8].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[8].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[9].Value != null)
                {
                    btnCieXLed9.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[9].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[9].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[10].Value != null)
                {
                    btnCieXLed10.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[10].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[10].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[11].Value != null)
                {
                    btnCieXLed11.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[11].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[11].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[12].Value != null)
                {
                    btnCieXLed12.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[12].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[12].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[13].Value != null)
                {
                    btnCieXLed13.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[13].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[13].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[14].Value != null)
                {
                    btnCieXLed14.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[14].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[14].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[3].Cells[15].Value != null)
                {
                    btnCieXLed15.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[3].Cells[15].Value.ToString()) < Convert.ToDouble(txtCIeXMax.Text) && double.Parse(dgwFeasaSettings.Rows[3].Cells[15].Value.ToString()) > Convert.ToDouble(txtCIeXMin.Text)
                        ? Color.Green
                        : Color.Red;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("CIE X Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckY()
        {
            //****Y
            try
            {

                if (dgwFeasaSettings.Rows[4].Cells[1].Value != null)
                {
                    btnCieYLed1.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[1].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[1].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[2].Value != null)
                {
                    btnCieYLed2.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[2].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[2].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[3].Value != null)
                {
                    btnCieYLed3.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[3].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[3].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[4].Value != null)
                {
                    btnCieYLed4.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[4].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[4].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[5].Value != null)
                {
                    btnCieYLed5.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[5].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[5].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[6].Value != null)
                {
                    btnCieYLed6.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[6].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[6].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[7].Value != null)
                {
                    btnCieYLed7.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[7].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[7].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[8].Value != null)
                {
                    btnCieYLed8.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[8].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[8].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[9].Value != null)
                {
                    btnCieYLed9.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[9].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[9].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[10].Value != null)
                {
                    btnCieYLed10.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[10].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[10].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[11].Value != null)
                {
                    btnCieYLed11.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[11].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[11].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[12].Value != null)
                {
                    btnCieYLed12.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[12].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[12].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[13].Value != null)
                {
                    btnCieYLed13.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[13].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[13].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[14].Value != null)
                {
                    btnCieYLed14.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[14].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[14].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[4].Cells[15].Value != null)
                {
                    btnCieYLed15.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[4].Cells[15].Value.ToString()) < Convert.ToDouble(txtCIeYMax.Text) && double.Parse(dgwFeasaSettings.Rows[4].Cells[15].Value.ToString()) > Convert.ToDouble(txtCIeYMin.Text)
                        ? Color.Green
                        : Color.Red;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("CIE Y Min ve Max değerlerini kontrol ediniz!!", "Hata");

            }
        }

        public void CheckU()
        {
            //****U
            try
            {

                if (dgwFeasaSettings.Rows[5].Cells[1].Value != null)
                {
                    btnCieULed1.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[1].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[1].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[2].Value != null)
                {
                    btnCieULed2.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[2].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[2].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[3].Value != null)
                {
                    btnCieULed3.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[3].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[3].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[4].Value != null)
                {
                    btnCieULed4.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[4].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[4].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[5].Value != null)
                {
                    btnCieULed5.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[5].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[5].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[6].Value != null)
                {
                    btnCieULed6.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[6].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[6].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[7].Value != null)
                {
                    btnCieULed7.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[7].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[7].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[8].Value != null)
                {
                    btnCieULed8.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[8].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[8].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[9].Value != null)
                {
                    btnCieULed9.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[9].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[9].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[10].Value != null)
                {
                    btnCieULed10.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[10].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[10].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[11].Value != null)
                {
                    btnCieULed11.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[11].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[11].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[12].Value != null)
                {
                    btnCieULed12.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[12].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[12].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[13].Value != null)
                {
                    btnCieULed13.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[13].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[13].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[14].Value != null)
                {
                    btnCieULed14.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[14].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[14].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[5].Cells[15].Value != null)
                {
                    btnCieULed15.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[5].Cells[15].Value.ToString()) < Convert.ToDouble(txtCIeUMax.Text) && double.Parse(dgwFeasaSettings.Rows[5].Cells[15].Value.ToString()) > Convert.ToDouble(txtCIeUMin.Text)
                        ? Color.Green
                        : Color.Red;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("CIE U Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckV()
        {
            //****V
            try
            {

                if (dgwFeasaSettings.Rows[6].Cells[1].Value != null)
                {
                    btnCieVLed1.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[1].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[1].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[2].Value != null)
                {
                    btnCieVLed2.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[2].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[2].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[3].Value != null)
                {
                    btnCieVLed3.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[3].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[3].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[4].Value != null)
                {
                    btnCieVLed4.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[4].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[4].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[5].Value != null)
                {
                    btnCieVLed5.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[5].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[5].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[6].Value != null)
                {
                    btnCieVLed6.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[6].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[6].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[7].Value != null)
                {
                    btnCieVLed7.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[7].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[7].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[8].Value != null)
                {
                    btnCieVLed8.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[8].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[8].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[9].Value != null)
                {
                    btnCieVLed9.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[9].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[9].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[10].Value != null)
                {
                    btnCieVLed10.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[10].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[10].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[11].Value != null)
                {
                    btnCieVLed11.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[11].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[11].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[12].Value != null)
                {
                    btnCieVLed12.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[12].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[12].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[13].Value != null)
                {
                    btnCieVLed13.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[13].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[13].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[14].Value != null)
                {
                    btnCieVLed14.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[14].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[14].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[6].Cells[15].Value != null)
                {
                    btnCieVLed15.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[6].Cells[15].Value.ToString()) < Convert.ToDouble(txtCIeVMax.Text) && double.Parse(dgwFeasaSettings.Rows[6].Cells[15].Value.ToString()) > Convert.ToDouble(txtCIeVMin.Text)
                        ? Color.Green
                        : Color.Red;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("CIE V Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckSat()
        {
            //****SAT
            try
            {
                if (dgwFeasaSettings.Rows[7].Cells[1].Value != null)
                {
                    btnSAT1.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[1].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[1].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[2].Value != null)
                {
                    btnSAT2.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[2].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[2].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[3].Value != null)
                {
                    btnSAT3.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[3].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[3].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[4].Value != null)
                {
                    btnSAT4.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[4].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[4].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[5].Value != null)
                {
                    btnSAT5.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[5].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[5].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[6].Value != null)
                {
                    btnSAT6.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[6].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[6].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[7].Value != null)
                {
                    btnSAT7.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[7].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[7].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[8].Value != null)
                {
                    btnSAT8.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[8].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[8].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[9].Value != null)
                {
                    btnSAT9.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[9].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[9].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[10].Value != null)
                {
                    btnSAT10.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[10].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[10].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[11].Value != null)
                {
                    btnSAT11.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[11].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[11].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[12].Value != null)
                {
                    btnSAT12.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[12].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[12].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[13].Value != null)
                {
                    btnSAT13.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[13].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[13].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[14].Value != null)
                {
                    btnSAT14.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[14].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[14].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[7].Cells[15].Value != null)
                {
                    btnSAT15.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[7].Cells[15].Value.ToString()) < Convert.ToInt32(txtSatMax.Text) && int.Parse(dgwFeasaSettings.Rows[7].Cells[15].Value.ToString()) > Convert.ToInt32(txtSatMin.Text)
                        ? Color.Green
                        : Color.Red;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("SATURATION Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckHue()
        {
            //****Hue
            try
            {
                if (dgwFeasaSettings.Rows[8].Cells[1].Value != null)
                {
                    btnHUE1.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[1].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[1].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[2].Value != null)
                {
                    btnHUE2.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[2].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[2].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[3].Value != null)
                {
                    btnHUE3.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[3].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[3].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[4].Value != null)
                {
                    btnHUE4.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[4].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[4].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[5].Value != null)
                {
                    btnHUE5.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[5].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[5].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[6].Value != null)
                {
                    btnHUE6.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[6].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[6].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[7].Value != null)
                {
                    btnHUE7.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[7].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[7].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[8].Value != null)
                {
                    btnHUE8.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[8].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[8].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[9].Value != null)
                {
                    btnHUE9.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[9].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[9].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[10].Value != null)
                {
                    btnHUE10.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[10].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[10].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[11].Value != null)
                {
                    btnHUE11.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[11].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[11].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[12].Value != null)
                {
                    btnHUE12.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[12].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[12].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[13].Value != null)
                {
                    btnHUE13.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[13].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[13].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[14].Value != null)
                {
                    btnHUE14.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[14].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[14].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[8].Cells[15].Value != null)
                {
                    btnHUE15.Appearance.BackColor = double.Parse(dgwFeasaSettings.Rows[8].Cells[15].Value.ToString()) < Convert.ToDouble(txtHueMax.Text) && double.Parse(dgwFeasaSettings.Rows[8].Cells[15].Value.ToString()) > Convert.ToDouble(txtHueMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("HUE Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckInt()
        {
            //****INT
            try
            {
                if (dgwFeasaSettings.Rows[9].Cells[1].Value != null)
                {
                    btnINT1.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[1].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[1].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[2].Value != null)
                {
                    btnINT2.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[2].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[2].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[3].Value != null)
                {
                    btnINT3.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[3].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[3].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[4].Value != null)
                {
                    btnINT4.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[4].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[4].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[5].Value != null)
                {
                    btnINT5.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[5].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[5].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[6].Value != null)
                {
                    btnINT6.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[6].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[6].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[7].Value != null)
                {
                    btnINT7.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[7].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[7].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[8].Value != null)
                {
                    btnINT8.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[8].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[8].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[9].Value != null)
                {
                    btnINT9.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[9].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[9].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[10].Value != null)
                {
                    btnINT10.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[10].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[10].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[11].Value != null)
                {
                    btnINT11.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[11].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[11].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[12].Value != null)
                {
                    btnINT12.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[12].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[12].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[13].Value != null)
                {
                    btnINT13.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[13].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[13].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[14].Value != null)
                {
                    btnINT14.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[14].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[14].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[9].Cells[15].Value != null)
                {
                    btnINT15.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[9].Cells[15].Value.ToString()) < Convert.ToInt32(txtIntMax.Text) && int.Parse(dgwFeasaSettings.Rows[9].Cells[15].Value.ToString()) > Convert.ToInt32(txtIntMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("INTENSITY Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckCct()
        {
            //****CCT
            try
            {
                if (dgwFeasaSettings.Rows[10].Cells[1].Value != null)
                {
                    btnCCT1.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[1].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[1].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[2].Value != null)
                {
                    btnCCT2.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[2].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[2].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[3].Value != null)
                {
                    btnCCT3.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[3].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[3].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[4].Value != null)
                {
                    btnCCT4.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[4].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[4].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[5].Value != null)
                {
                    btnCCT5.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[5].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[5].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[6].Value != null)
                {
                    btnCCT6.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[6].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[6].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[7].Value != null)
                {
                    btnCCT7.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[7].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[7].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[8].Value != null)
                {
                    btnCCT8.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[8].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[8].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[9].Value != null)
                {
                    btnCCT9.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[9].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[9].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[10].Value != null)
                {
                    btnCCT10.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[10].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[10].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[11].Value != null)
                {
                    btnCCT11.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[11].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[11].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[12].Value != null)
                {
                    btnCCT12.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[12].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[12].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[13].Value != null)
                {
                    btnCCT13.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[13].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[13].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[14].Value != null)
                {
                    btnCCT14.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[14].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[14].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[10].Cells[15].Value != null)
                {
                    btnCCT15.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[10].Cells[15].Value.ToString()) < Convert.ToInt32(txtCctMax.Text) && int.Parse(dgwFeasaSettings.Rows[10].Cells[15].Value.ToString()) > Convert.ToInt32(txtCctMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("CCT Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }

        public void CheckWave()
        {
            //****WAVE
            try
            {
                if (dgwFeasaSettings.Rows[11].Cells[1].Value != null)
                {
                    btnWAVE1.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[1].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[1].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[2].Value != null)
                {
                    btnWAVE2.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[2].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[2].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[3].Value != null)
                {
                    btnWAVE3.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[3].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[3].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[4].Value != null)
                {
                    btnWAVE4.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[4].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[4].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[5].Value != null)
                {
                    btnWAVE5.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[5].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[5].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[6].Value != null)
                {
                    btnWAVE6.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[6].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[6].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[7].Value != null)
                {
                    btnWAVE7.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[7].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[7].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[8].Value != null)
                {
                    btnWAVE8.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[8].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[8].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[9].Value != null)
                {
                    btnWAVE9.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[9].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[9].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[10].Value != null)
                {
                    btnWAVE10.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[10].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[10].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[11].Value != null)
                {
                    btnWAVE11.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[11].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[11].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[12].Value != null)
                {
                    btnWAVE12.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[12].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[12].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[13].Value != null)
                {
                    btnWAVE13.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[13].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[13].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[14].Value != null)
                {
                    btnWAVE14.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[14].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[14].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }
                if (dgwFeasaSettings.Rows[11].Cells[15].Value != null)
                {
                    btnWAVE15.Appearance.BackColor = int.Parse(dgwFeasaSettings.Rows[11].Cells[15].Value.ToString()) < Convert.ToInt32(txtWaveMax.Text) && int.Parse(dgwFeasaSettings.Rows[11].Cells[15].Value.ToString()) > Convert.ToInt32(txtWaveMin.Text)
                        ? Color.Green
                        : Color.Red;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Wavelength Min ve Max değerlerini girmeyi unuttunuz!!", "Hata");

            }
        }


        #endregion

        //*****
        #region Min-Normal-Exit
        private void btnMin_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnNormal_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;

            }
            else
            {
                WindowState = FormWindowState.Normal;
                this.Height = 650;
                this.Width = 1000;
                StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            threadWriteBool = new Thread(() => nxCompoletBoolWrite("connectionStart", false));
            threadWriteBool.Start();
            FeasaCom.Close(Convert.ToInt32(cmbPortsName.SelectedItem));
            if (portGwInstek.IsOpen)
                portGwInstek.Close();

            threadReadLoop.Abort();
            timer1.Stop();
            Thread.Sleep(1000);
            GwInstekPortClose();
            Application.Exit();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadWriteBool = new Thread(() => nxCompoletBoolWrite("connectionStart", false));
            threadWriteBool.Start();
            FeasaCom.Close(Convert.ToInt32(cmbPortsName.SelectedItem));
            if (portGwInstek.IsOpen)
                portGwInstek.Close();

            threadReadLoop.Abort();
            timer1.Stop();
            Thread.Sleep(1000);
            GwInstekPortClose();
            Application.Exit();

        }
        #endregion

        //*****Radio Buton Check kısımları*****
        #region Check
        //*****Led Sayısı 
        private void txtTestBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtTestBox.Text, out int enteredValue))
            {
                //Convert.ToInt32(txtTestBox.Text) > 0 &&
                if (Convert.ToInt32(txtTestBox.Text) < 16)
                {
                    txtLedPort1.Enabled = false;
                    txtLedPort2.Enabled = false;
                    txtLedPort3.Enabled = false;
                    txtLedPort4.Enabled = false;
                    txtLedPort5.Enabled = false;
                    txtLedPort6.Enabled = false;
                    txtLedPort7.Enabled = false;
                    txtLedPort8.Enabled = false;
                    txtLedPort9.Enabled = false;
                    txtLedPort10.Enabled = false;
                    txtLedPort11.Enabled = false;
                    txtLedPort12.Enabled = false;
                    txtLedPort13.Enabled = false;
                    txtLedPort14.Enabled = false;
                    txtLedPort15.Enabled = false;

                    txtLed1.Enabled = false;
                    txtLed2.Enabled = false;
                    txtLed3.Enabled = false;
                    txtLed4.Enabled = false;
                    txtLed5.Enabled = false;
                    txtLed6.Enabled = false;
                    txtLed7.Enabled = false;
                    txtLed8.Enabled = false;
                    txtLed9.Enabled = false;
                    txtLed10.Enabled = false;
                    txtLed11.Enabled = false;
                    txtLed12.Enabled = false;
                    txtLed13.Enabled = false;
                    txtLed14.Enabled = false;
                    txtLed15.Enabled = false;

                    if (Convert.ToInt32(txtTestBox.Text) > 0)
                    {
                        txtLedPort1.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 1)
                    {
                        txtLedPort2.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 2)
                    {
                        txtLedPort3.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 3)
                    {
                        txtLedPort4.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 4)
                    {
                        txtLedPort5.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 5)
                    {
                        txtLedPort6.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 6)
                    {
                        txtLedPort7.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 7)
                    {
                        txtLedPort8.Enabled = true;
                        txtLed8.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 8)
                    {
                        txtLedPort9.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 9)
                    {
                        txtLedPort10.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 10)
                    {
                        txtLedPort11.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 11)
                    {
                        txtLedPort12.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 12)
                    {
                        txtLedPort13.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 13)
                    {
                        txtLedPort14.Enabled = true;
                        txtLed14.Enabled = true;
                    }
                    if (Convert.ToInt32(txtTestBox.Text) > 14)
                    {
                        txtLedPort15.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("Uzunluk 1 ile 15 aralığında olmalıdır.", "Hata");
                    txtTestBox.Text = "0";
                }

                if (Convert.ToInt32(txtTestBox.Text) < 16)
                {
                    if (txtLedPort1.Enabled == false)
                    {
                        txtLedPort1.Text = "";
                    }
                    if (txtLedPort2.Enabled == false)
                    {
                        txtLedPort2.Text = "";
                    }
                    if (txtLedPort3.Enabled == false)
                    {
                        txtLedPort3.Text = "";
                    }
                    if (txtLedPort4.Enabled == false)
                    {
                        txtLedPort4.Text = "";
                    }
                    if (txtLedPort5.Enabled == false)
                    {
                        txtLedPort5.Text = "";
                    }
                    if (txtLedPort6.Enabled == false)
                    {
                        txtLedPort6.Text = "";
                    }
                    if (txtLedPort7.Enabled == false)
                    {
                        txtLedPort7.Text = "";
                    }
                    if (txtLedPort8.Enabled == false)
                    {
                        txtLedPort8.Text = "";
                    }
                    if (txtLedPort9.Enabled == false)
                    {
                        txtLedPort9.Text = "";
                    }
                    if (txtLedPort10.Enabled == false)
                    {
                        txtLedPort10.Text = "";
                    }
                    if (txtLedPort11.Enabled == false)
                    {
                        txtLedPort11.Text = "";
                    }
                    if (txtLedPort12.Enabled == false)
                    {
                        txtLedPort12.Text = "";
                    }
                    if (txtLedPort13.Enabled == false)
                    {
                        txtLedPort13.Text = "";
                    }
                    if (txtLedPort14.Enabled == false)
                    {
                        txtLedPort14.Text = "";
                    }
                    if (txtLedPort15.Enabled == false)
                    {
                        txtLedPort15.Text = "";
                    }

                }
            }
        }

        //*****Test edilecek Led sayısını girebilmek için Led ölçümün olup olmadığı kontrolü
        private void cmbLedOlcum_SelectedIndexChanged(object sender, EventArgs e)
        {
            _ = (cmbLedOlcum.Text == "VAR") ? txtTestBox.Enabled = true : txtTestBox.Enabled = false;
        }

        //*****RadioButonların işaretlenmesinde gerçekleşen olaylar
        private void rbtnRGB_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnRGB.Checked == false)
            {
                txtRMin.Enabled = false;
                txtRMax.Enabled = false;
                txtGMin.Enabled = false;
                txtGMax.Enabled = false;
                txtBMin.Enabled = false;
                txtBMax.Enabled = false;

                txtRMin.Text = "";
                txtRMax.Text = "";
                txtGMin.Text = "";
                txtGMax.Text = "";
                txtBMin.Text = "";
                txtBMax.Text = "";

                for (int i = 1; i <= 15; i++)
                {
                    dgwFeasaSettings.Rows[0].Cells[i].Value = null;
                    dgwFeasaSettings.Rows[1].Cells[i].Value = null;
                    dgwFeasaSettings.Rows[2].Cells[i].Value = null;
                }

                InitializeResetButtons();

            }
            else if (rbtnRGB.Checked == true)
            {
                try
                {
                    txtRMin.Enabled = true;
                    txtRMax.Enabled = true;
                    txtGMin.Enabled = true;
                    txtGMax.Enabled = true;
                    txtBMin.Enabled = true;
                    txtBMax.Enabled = true;


                    txtRMin.Text = "1";
                    txtRMax.Text = "255";
                    txtGMin.Text = "1";
                    txtGMax.Text = "255";
                    txtBMin.Text = "1";
                    txtBMax.Text = "255";


                    if (txtLedPort1.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort1.Text));
                    }
                    if (txtLedPort2.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort2.Text));
                    }
                    if (txtLedPort3.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort3.Text));
                    }
                    if (txtLedPort4.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort4.Text));
                    }
                    if (txtLedPort5.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort5.Text));
                    }
                    if (txtLedPort6.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort6.Text));
                    }
                    if (txtLedPort7.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort7.Text));
                    }
                    if (txtLedPort8.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort8.Text));
                    }
                    if (txtLedPort9.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort9.Text));
                    }
                    if (txtLedPort10.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort10.Text));
                    }
                    if (txtLedPort11.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort11.Text));
                    }
                    if (txtLedPort12.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort12.Text));
                    }
                    if (txtLedPort13.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort13.Text));
                    }
                    if (txtLedPort14.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort14.Text));
                    }
                    if (txtLedPort15.Text != "")
                    {
                        FeasaReadRGB(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort15.Text));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Önce Led Bilgisinin Girişini sağlayınız!!! " + ex);
                    rbtnRGB.Checked = false;
                }

            }
        }

        private void rbtnCIExy_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnCIExy.Checked == false)
            {
                txtCIeXMin.Enabled = false;
                txtCIeXMax.Enabled = false;
                txtCIeYMin.Enabled = false;
                txtCIeYMax.Enabled = false;

                txtCIeXMin.Text = "";
                txtCIeXMax.Text = "";
                txtCIeYMin.Text = "";
                txtCIeYMax.Text = "";
                for (int i = 1; i <= 15; i++)
                {
                    dgwFeasaSettings.Rows[3].Cells[i].Value = null;
                    dgwFeasaSettings.Rows[4].Cells[i].Value = null;
                }
                InitializeResetButtons();
            }
            else if (rbtnCIExy.Checked == true)
            {
                try
                {
                    txtCIeXMin.Enabled = true;
                    txtCIeXMax.Enabled = true;
                    txtCIeYMin.Enabled = true;
                    txtCIeYMax.Enabled = true;

                    txtCIeXMin.Text = "0.0001";
                    txtCIeXMax.Text = "0.9999";
                    txtCIeYMin.Text = "0.0001";
                    txtCIeYMax.Text = "0.9999";

                    if (txtLedPort1.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort1.Text));
                    }
                    if (txtLedPort2.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort2.Text));
                    }
                    if (txtLedPort3.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort3.Text));
                    }
                    if (txtLedPort4.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort4.Text));
                    }
                    if (txtLedPort5.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort5.Text));
                    }
                    if (txtLedPort6.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort6.Text));
                    }
                    if (txtLedPort7.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort7.Text));
                    }
                    if (txtLedPort8.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort8.Text));
                    }
                    if (txtLedPort9.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort9.Text));
                    }
                    if (txtLedPort10.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort10.Text));
                    }
                    if (txtLedPort11.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort11.Text));
                    }
                    if (txtLedPort12.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort12.Text));
                    }
                    if (txtLedPort13.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort13.Text));
                    }
                    if (txtLedPort14.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort14.Text));
                    }
                    if (txtLedPort15.Text != "")
                    {
                        FeasaReadCIExy(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort15.Text));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Önce Led Bilgisinin Girişini sağlayınız!!! " + ex);
                    rbtnRGB.Checked = false;
                }

            }


        }

        private void rbtnCIEuv_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnCIEuv.Checked == false)
            {
                txtCIeUMin.Enabled = false;
                txtCIeUMax.Enabled = false;
                txtCIeVMin.Enabled = false;
                txtCIeVMax.Enabled = false;

                txtCIeUMin.Text = "";
                txtCIeUMax.Text = "";
                txtCIeVMin.Text = "";
                txtCIeVMax.Text = "";

                for (int i = 1; i <= 15; i++)
                {
                    dgwFeasaSettings.Rows[5].Cells[i].Value = null;
                    dgwFeasaSettings.Rows[6].Cells[i].Value = null;
                }

                InitializeResetButtons();
            }
            else if (rbtnCIEuv.Checked == true)
            {
                try
                {
                    txtCIeUMin.Enabled = true;
                    txtCIeUMax.Enabled = true;
                    txtCIeVMin.Enabled = true;
                    txtCIeVMax.Enabled = true;

                    txtCIeUMin.Text = "0.0001";
                    txtCIeUMax.Text = "0.9999";
                    txtCIeVMin.Text = "0.0001";
                    txtCIeVMax.Text = "0.9999";

                    if (txtLedPort1.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort1.Text));
                    }
                    if (txtLedPort2.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort2.Text));
                    }
                    if (txtLedPort3.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort3.Text));
                    }
                    if (txtLedPort4.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort4.Text));
                    }
                    if (txtLedPort5.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort5.Text));
                    }
                    if (txtLedPort6.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort6.Text));
                    }
                    if (txtLedPort7.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort7.Text));
                    }
                    if (txtLedPort8.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort8.Text));
                    }
                    if (txtLedPort9.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort9.Text));
                    }
                    if (txtLedPort10.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort10.Text));
                    }
                    if (txtLedPort11.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort11.Text));
                    }
                    if (txtLedPort12.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort12.Text));
                    }
                    if (txtLedPort13.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort13.Text));
                    }
                    if (txtLedPort14.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort14.Text));
                    }
                    if (txtLedPort15.Text != "")
                    {
                        FeasaReadCIEuv(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort15.Text));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Önce Led Bilgisinin Girişini sağlayınız!!! " + ex);
                    rbtnRGB.Checked = false;
                }


            }
        }

        private void rbtnSaturation_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnSaturation.Checked == false)
            {
                txtSatMin.Enabled = false;
                txtSatMax.Enabled = false;

                txtSatMin.Text = "";
                txtSatMax.Text = "";

                for (int i = 1; i <= 15; i++)
                {
                    dgwFeasaSettings.Rows[7].Cells[i].Value = null;
                }

                InitializeResetButtons();

            }
            else if (rbtnSaturation.Checked == true)
            {
                txtSatMin.Enabled = true;
                txtSatMax.Enabled = true;

                txtSatMin.Text = "0";
                txtSatMax.Text = "100";

                if (txtLedPort1.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort1.Text));
                if (txtLedPort2.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort2.Text));
                if (txtLedPort3.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort3.Text));
                if (txtLedPort4.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort4.Text));
                if (txtLedPort5.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort5.Text));
                if (txtLedPort6.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort6.Text));
                if (txtLedPort7.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort7.Text));
                if (txtLedPort8.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort8.Text));
                if (txtLedPort9.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort9.Text));
                if (txtLedPort10.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort10.Text));
                if (txtLedPort11.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort11.Text));
                if (txtLedPort12.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort12.Text));
                if (txtLedPort13.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort13.Text));
                if (txtLedPort14.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort14.Text));
                if (txtLedPort15.Text != "")
                    FeaseReadSat(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort15.Text));
            }
        }

        private void rbtnHue_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnHue.Checked == false)
            {
                txtHueMin.Enabled = false;
                txtHueMax.Enabled = false;

                txtHueMin.Text = "";
                txtHueMax.Text = "";

                for (int i = 1; i <= 15; i++)
                {
                    dgwFeasaSettings.Rows[8].Cells[i].Value = null;
                }

                InitializeResetButtons();
            }
            else if (rbtnHue.Checked == true)
            {
                txtHueMin.Enabled = true;
                txtHueMax.Enabled = true;

                txtHueMin.Text = "0.00";
                txtHueMax.Text = "360.00";

                if (txtLedPort1.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort1.Text));
                if (txtLedPort2.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort2.Text));
                if (txtLedPort3.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort3.Text));
                if (txtLedPort4.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort4.Text));
                if (txtLedPort5.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort5.Text));
                if (txtLedPort6.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort6.Text));
                if (txtLedPort7.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort7.Text));
                if (txtLedPort8.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort8.Text));
                if (txtLedPort9.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort9.Text));
                if (txtLedPort10.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort10.Text));
                if (txtLedPort11.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort11.Text));
                if (txtLedPort12.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort12.Text));
                if (txtLedPort13.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort13.Text));
                if (txtLedPort14.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort14.Text));
                if (txtLedPort15.Text != "")
                    FeaseReadHue(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort15.Text));

            }

        }

        private void rbtnIntensty_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnIntensty.Checked == false)
            {
                txtIntMin.Enabled = false;
                txtIntMax.Enabled = false;

                txtIntMin.Text = "";
                txtIntMax.Text = "";

                for (int i = 1; i <= 15; i++)
                {
                    dgwFeasaSettings.Rows[9].Cells[i].Value = null;
                }

                InitializeResetButtons();
            }
            else if (rbtnIntensty.Checked == true)
            {
                txtIntMin.Enabled = true;
                txtIntMax.Enabled = true;

                txtIntMin.Text = "0";
                txtIntMax.Text = "99999";

                if (txtLedPort1.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort1.Text));
                }
                if (txtLedPort2.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort2.Text));
                }
                if (txtLedPort3.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort3.Text));
                }
                if (txtLedPort4.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort4.Text));
                }
                if (txtLedPort5.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort5.Text));
                }
                if (txtLedPort6.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort6.Text));
                }
                if (txtLedPort7.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort7.Text));
                }
                if (txtLedPort8.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort8.Text));
                }
                if (txtLedPort9.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort9.Text));
                }
                if (txtLedPort10.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort10.Text));
                }
                if (txtLedPort11.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort11.Text));
                }
                if (txtLedPort12.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort12.Text));
                }
                if (txtLedPort13.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort13.Text));
                }
                if (txtLedPort14.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort14.Text));
                }
                if (txtLedPort15.Text != "")
                {
                    FeasaReadINT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort15.Text));
                }
            }
        }

        private void rbtnCCT_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnCCT.Checked == false)
            {
                txtCctMin.Enabled = false;
                txtCctMax.Enabled = false;

                txtCctMin.Text = "";
                txtCctMax.Text = "";

                for (int i = 1; i <= 15; i++)
                {
                    dgwFeasaSettings.Rows[10].Cells[i].Value = null;
                }
                InitializeResetButtons();
            }
            else if (rbtnCCT.Checked == true)
            {
                txtCctMin.Enabled = true;
                txtCctMax.Enabled = true;

                txtCctMin.Text = "0";
                txtCctMax.Text = "99999";

                if (txtLedPort1.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort1.Text));
                }
                if (txtLedPort2.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort2.Text));
                }
                if (txtLedPort3.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort3.Text));
                }
                if (txtLedPort4.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort4.Text));
                }
                if (txtLedPort5.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort5.Text));
                }
                if (txtLedPort6.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort6.Text));
                }
                if (txtLedPort7.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort7.Text));
                }
                if (txtLedPort8.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort8.Text));
                }
                if (txtLedPort9.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort9.Text));
                }
                if (txtLedPort10.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort10.Text));
                }
                if (txtLedPort11.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort11.Text));
                }
                if (txtLedPort12.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort12.Text));
                }
                if (txtLedPort13.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort13.Text));
                }
                if (txtLedPort14.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort14.Text));
                }
                if (txtLedPort15.Text != "")
                {
                    FeasaReadCCT(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort15.Text));
                }
            }
        }

        private void rbtnWavelength_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnWavelength.Checked == false)
            {
                txtWaveMin.Enabled = false;
                txtWaveMax.Enabled = false;

                txtWaveMin.Text = "";
                txtWaveMax.Text = "";

                for (int i = 1; i <= 15; i++)
                {
                    dgwFeasaSettings.Rows[11].Cells[i].Value = null;
                }

                InitializeResetButtons();
            }
            else if (rbtnWavelength.Checked == true)
            {
                txtWaveMin.Enabled = true;
                txtWaveMax.Enabled = true;

                txtWaveMin.Text = "0";
                txtWaveMax.Text = "999";

                if (txtLedPort1.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort1.Text));
                }
                if (txtLedPort2.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort2.Text));
                }
                if (txtLedPort3.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort3.Text));
                }
                if (txtLedPort4.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort4.Text));
                }
                if (txtLedPort5.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort5.Text));
                }
                if (txtLedPort6.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort6.Text));
                }
                if (txtLedPort7.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort7.Text));
                }
                if (txtLedPort8.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort8.Text));
                }
                if (txtLedPort9.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort9.Text));
                }
                if (txtLedPort10.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName2.SelectedItem.ToString()), Convert.ToInt32(txtLedPort10.Text));
                }
                if (txtLedPort11.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort11.Text));
                }
                if (txtLedPort12.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort12.Text));
                }
                if (txtLedPort13.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort13.Text));
                }
                if (txtLedPort14.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort14.Text));
                }
                if (txtLedPort15.Text != "")
                {
                    FeasaReadWaveLength(Convert.ToInt32(cmbPortsName.SelectedItem.ToString()), Convert.ToInt32(txtLedPort15.Text));
                }
            }
        }

        //*****RadioButonun işaretlenmesi ve işaretinin kaldırılması için yardımcı olaylar
        private void rbtnRGB_Click(object sender, EventArgs e)
        {
            if ((sayac1 % 2) == 0)
            {
                rbtnRGB.Checked = true;
            }
            else
                rbtnRGB.Checked = false;

            sayac1++;

            if (sayac1 == 10)
                sayac1 = 0;

        }

        private void rbtnCIExy_Click(object sender, EventArgs e)
        {
            if ((sayac2 % 2) == 0)
            {
                rbtnCIExy.Checked = true;
            }
            else
                rbtnCIExy.Checked = false;

            sayac2++;

            if (sayac2 == 10)
                sayac2 = 0;
        }

        private void rbtnCIEuv_Click(object sender, EventArgs e)
        {
            if ((sayac3 % 2) == 0)
            {
                rbtnCIEuv.Checked = true;
            }
            else
                rbtnCIEuv.Checked = false;

            sayac3++;

            if (sayac3 == 10)
                sayac3 = 0;
        }

        private void rbtnSaturation_Click(object sender, EventArgs e)
        {

            if ((sayac4 % 2) == 0)
            {
                rbtnSaturation.Checked = true;
            }
            else
                rbtnSaturation.Checked = false;

            sayac4++;

            if (sayac4 == 10)
                sayac4 = 0;
        }

        private void rbtnHue_Click(object sender, EventArgs e)
        {
            if ((sayac5 % 2) == 0)
            {
                rbtnHue.Checked = true;
            }
            else
                rbtnHue.Checked = false;

            sayac5++;

            if (sayac5 == 10)
                sayac5 = 0;
        }

        private void rbtnIntensty_Click(object sender, EventArgs e)
        {
            if ((sayac6 % 2) == 0)
            {
                rbtnIntensty.Checked = true;
            }
            else
                rbtnIntensty.Checked = false;

            sayac6++;

            if (sayac6 == 10)
                sayac6 = 0;
        }

        private void rbtnCCT_Click(object sender, EventArgs e)
        {
            if ((sayac7 % 2) == 0)
            {
                rbtnCCT.Checked = true;
            }
            else
                rbtnCCT.Checked = false;

            sayac7++;

            if (sayac7 == 10)
                sayac7 = 0;
        }

        private void rbtnWavelength_Click(object sender, EventArgs e)
        {
            if ((sayac8 % 2) == 0)
            {
                rbtnWavelength.Checked = true;
            }
            else
                rbtnWavelength.Checked = false;

            sayac8++;

            if (sayac8 == 10)
                sayac8 = 0;
        }

        private void CalismaSecim_SelectedIndexChanged(object sender, EventArgs e)
        {
            threadWriteString = new Thread(() => nxCompoletStringWrite("calismaMod", cmbCalismaSecim.SelectedIndex.ToString()));
            threadWriteString.Start();
        }

        #endregion

        //*****(-) Eksi değer girilmemesi için ayarlamaları*****
        #region KEYPRESS
        private void btnManuelTestStart_Click(object sender, EventArgs e)
        {
            if (nxCompoletBoolRead("manuelTestStart"))
            {
                threadPushButton = new Thread(() => nxCompoletBoolWrite("manuelTestStart", false));
                threadPushButton.Start();
            }
            else
            {
                threadPushButton = new Thread(() => nxCompoletBoolWrite("manuelTestStart", true));
                threadPushButton.Start();
            }
        }
        private void txtRMin_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtRMax_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtGMin_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtGMax_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtBMin_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtBMax_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCIeXMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            char[] allowedChars = "0123456789.\b\t".ToCharArray();

            // Basılan tuş ASCII değerini alın
            char keyPressed = e.KeyChar;


            if (Array.IndexOf(allowedChars, keyPressed) == -1)
            {
                e.Handled = true;
            }

            // Eğer nokta (.) zaten yazılmışsa ve tekrar bir nokta yazılmak isteniyorsa işlemeyi engelle
            if (keyPressed == '.' && txtCIeXMin.Text.Contains("."))
            {
                e.Handled = true;
            }
            //e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCIeYMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            char[] allowedChars = "0123456789.\b\t".ToCharArray();

            // Basılan tuş ASCII değerini alın
            char keyPressed = e.KeyChar;


            if (Array.IndexOf(allowedChars, keyPressed) == -1)
            {
                e.Handled = true;
            }

            // Eğer nokta (.) zaten yazılmışsa ve tekrar bir nokta yazılmak isteniyorsa işlemeyi engelle
            if (keyPressed == '.' && txtCIeYMin.Text.Contains("."))
            {
                e.Handled = true;
            }
            //e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCIeXMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            char[] allowedChars = "0123456789.\b\t".ToCharArray();

            // Basılan tuş ASCII değerini alın
            char keyPressed = e.KeyChar;


            if (Array.IndexOf(allowedChars, keyPressed) == -1)
            {
                e.Handled = true;
            }

            // Eğer nokta (.) zaten yazılmışsa ve tekrar bir nokta yazılmak isteniyorsa işlemeyi engelle
            if (keyPressed == '.' && txtCIeXMax.Text.Contains("."))
            {
                e.Handled = true;
            }
            //e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCIeYMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            char[] allowedChars = "0123456789.\b\t".ToCharArray();

            // Basılan tuş ASCII değerini alın
            char keyPressed = e.KeyChar;


            if (Array.IndexOf(allowedChars, keyPressed) == -1)
            {
                e.Handled = true;
            }

            // Eğer nokta (.) zaten yazılmışsa ve tekrar bir nokta yazılmak isteniyorsa işlemeyi engelle
            if (keyPressed == '.' && txtCIeYMax.Text.Contains("."))
            {
                e.Handled = true;
            }
            //e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCIeUMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCIeUMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCIeVMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCIeVMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtSatMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtSatMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtHueMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtHueMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtIntMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtIntMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCctMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtCctMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtWaveMIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtWaveMAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        #endregion

        //*****Girilen değerlerin aralık bilgileri*****
        #region DEĞERLER
        private void txtRMin_TextChanged(object sender, EventArgs e)
        {
            if (txtRMin.Text != "")
            {
                int a = Convert.ToInt32(txtRMin.Text);
                if (a > 0 && a > 255)
                {
                    MessageBox.Show("0 ile 255 arasında bir değer giriniz!!", "Hata");
                    txtRMin.Text = "";
                }
            }
        }

        private void txtRMax_TextChanged(object sender, EventArgs e)
        {
            if (txtRMax.Text != "")
            {
                int a = Convert.ToInt32(txtRMax.Text);
                if (a > 0 && a > 255)
                {
                    MessageBox.Show("0 ile 255 arasında bir değer giriniz!!", "Hata");
                    txtRMax.Text = "";
                }
            }
        }

        private void txtGMin_TextChanged(object sender, EventArgs e)
        {
            if (txtGMin.Text != "")
            {
                int a = Convert.ToInt32(txtGMin.Text);
                if (a > 0 && a > 255)
                {
                    MessageBox.Show("0 ile 255 arasında bir değer giriniz!!", "Hata");
                    txtGMin.Text = "";
                }
            }
        }

        private void txtGMax_TextChanged(object sender, EventArgs e)
        {
            if (txtGMax.Text != "")
            {
                int a = Convert.ToInt32(txtGMax.Text);
                if (a > 0 && a > 255)
                {
                    MessageBox.Show("0 ile 255 arasında bir değer giriniz!!", "Hata");
                    txtGMax.Text = "";
                }
            }
        }

        private void txtBMin_TextChanged(object sender, EventArgs e)
        {
            if (txtBMin.Text != "")
            {
                int a = Convert.ToInt32(txtBMin.Text);
                if (a > 0 && a > 255)
                {
                    MessageBox.Show("0 ile 255 arasında bir değer giriniz!!", "Hata");
                    txtBMin.Text = "";
                }
            }
        }

        private void txtBMax_TextChanged(object sender, EventArgs e)
        {
            if (txtBMax.Text != "")
            {
                int a = Convert.ToInt32(txtBMax.Text);
                if (a > 0 && a > 255)
                {
                    MessageBox.Show("0 ile 255 arasında bir değer giriniz!!", "Hata");
                    txtBMax.Text = "";
                }
            }
        }


        private void txtCIeXMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtCIeXMin.Text != "")
            {
                string a = txtCIeXMin.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.0001 && f > 0.9999)
                {
                    MessageBox.Show("0.0001 ile 0.9999 arasında bir değer giriniz!!", "Hata");
                    txtCIeXMin.Text = "";
                }
            }
        }

        private void txtCIeXMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtCIeXMax.Text != "")
            {
                string a = txtCIeXMax.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.0001 && f > 0.9999)
                {
                    MessageBox.Show("0.0001 ile 0.9999 arasında bir değer giriniz!!", "Hata");
                    txtCIeXMax.Text = "";
                }
            }
        }

        private void txtCIeYMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtCIeYMin.Text != "")
            {
                string a = txtCIeYMin.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.0001 && f > 0.9999)
                {
                    MessageBox.Show("0.0001 ile 0.9999 arasında bir değer giriniz!!", "Hata");
                    txtCIeYMin.Text = "";
                }
            }
        }

        private void txtCIeYMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtCIeYMax.Text != "")
            {
                string a = txtCIeYMax.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.0001 && f > 0.9999)
                {
                    MessageBox.Show("0.0001 ile 0.9999 arasında bir değer giriniz!!", "Hata");
                    txtCIeXMax.Text = "";
                }
            }
        }

        private void txtCIeUMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtCIeUMin.Text != "")
            {
                string a = txtCIeUMin.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.0001 && f > 0.9999)
                {
                    MessageBox.Show("0.0001 ile 0.9999 arasında bir değer giriniz!!", "Hata");
                    txtCIeUMin.Text = "";
                }
            }
        }

        private void txtCIeUMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtCIeUMax.Text != "")
            {
                string a = txtCIeUMax.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.0001 && f > 0.9999)
                {
                    MessageBox.Show("0.0001 ile 0.9999 arasında bir değer giriniz!!", "Hata");
                    txtCIeUMax.Text = "";
                }
            }
        }

        private void txtCIeVMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtCIeVMin.Text != "")
            {
                string a = txtCIeVMin.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.0001 && f > 0.9999)
                {
                    MessageBox.Show("0.0001 ile 0.9999 arasında bir değer giriniz!!", "Hata");
                    txtCIeVMin.Text = "";
                }
            }
        }

        private void txtCIeVMAX_TextChanged(object sender, EventArgs e)
        {

            if (txtCIeVMax.Text != "")
            {
                string a = txtCIeYMax.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.0001 && f > 0.9999)
                {
                    MessageBox.Show("0.0001 ile 0.9999 arasında bir değer giriniz!!", "Hata");
                    txtCIeVMax.Text = "";
                }
            }
        }

        private void txtSatMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtSatMin.Text != "")
            {
                int a = Convert.ToInt32(txtSatMin.Text);
                if (a > 1 && a > 100)
                {
                    MessageBox.Show("1 ile 100 arasında bir değer giriniz!!", "Hata");
                    txtSatMin.Text = "";
                }
            }
        }

        private void txtSatMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtSatMax.Text != "")
            {
                int a = Convert.ToInt32(txtSatMax.Text);
                if (a > 1 && a > 100)
                {
                    MessageBox.Show("1 ile 100 arasında bir değer giriniz!!", "Hata");
                    txtSatMax.Text = "";
                }
            }
        }

        private void txtHueMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtHueMin.Text != "")
            {
                string a = txtHueMin.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.00 && f > 360.00)
                {
                    MessageBox.Show("0.00 ile 360.00 arasında bir değer giriniz!!", "Hata");
                    txtHueMin.Text = "";
                }
            }
        }

        private void txtHueMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtHueMax.Text != "")
            {
                string a = txtHueMax.Text.Replace('.', ',');
                double f = Convert.ToDouble(a);
                if (f > 0.00 && f > 360.00)
                {
                    MessageBox.Show("0.00 ile 360.00 arasında bir değer giriniz!!", "Hata");
                    txtHueMax.Text = "";
                }
            }
        }

        private void txtIntMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtIntMin.Text != "")
            {
                int a = Convert.ToInt32(txtIntMin.Text);
                if (a > 0 && a > 99999)
                {
                    MessageBox.Show("0 ile 99999 arasında bir değer giriniz!!", "Hata");
                    txtIntMin.Text = "";
                }
            }
        }

        private void txtIntMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtIntMax.Text != "")
            {
                int a = Convert.ToInt32(txtIntMax.Text);
                if (a > 0 && a > 99999)
                {
                    MessageBox.Show("0 ile 99999 arasında bir değer giriniz!!", "Hata");
                    txtIntMax.Text = "";
                }
            }
        }

        private void txtCctMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtCctMin.Text != "")
            {
                int a = Convert.ToInt32(txtCctMin.Text);
                if (a > 0 && a > 99999)
                {
                    MessageBox.Show("0 ile 99999 arasında bir değer giriniz!!", "Hata");
                    txtCctMin.Text = "";
                }
            }
        }

        private void txtCctMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtCctMax.Text != "")
            {
                int a = Convert.ToInt32(txtCctMax.Text);
                if (a > 0 && a > 99999)
                {
                    MessageBox.Show("0 ile 99999 arasında bir değer giriniz!!", "Hata");
                    txtCctMax.Text = "";
                }
            }
        }

        private void txtWaveMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtWaveMin.Text != "")
            {
                int a = Convert.ToInt32(txtWaveMin.Text);
                if (a > 0 && a > 999)
                {
                    MessageBox.Show("0 ile 999 arasında bir değer giriniz!!", "Hata");
                    txtWaveMin.Text = "";
                }
            }
        }

        private void txtWaveMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtWaveMax.Text != "")
            {
                int a = Convert.ToInt32(txtWaveMax.Text);
                if (a > 0 && a > 999999)
                {
                    MessageBox.Show("0 ile 999 arasında bir değer giriniz!!", "Hata");
                    txtWaveMax.Text = "";
                }
            }
        }


        private void txtTestPcbBoy_TextChanged(object sender, EventArgs e)
        {
            if (txtTestPcbBoy.Text != "")
            {
                double a = Convert.ToDouble(txtTestPcbBoy.Text);
                if (a > 0.00 && a > 150.00)
                {
                    MessageBox.Show("0.00 ile 150.00 arasında bir değer giriniz!!", "Hata");
                    txtTestPcbBoy.Text = "";
                }
            }

        }

        private void txtTestPcbaAdet_TextChanged(object sender, EventArgs e)
        {
            if (txtTestPcbaAdet.Text != "")
            {
                int a = Convert.ToInt32(txtTestPcbaAdet.Text);
                if (a > 0 && a > 50)
                {
                    MessageBox.Show("0 ile 50 arasında bir değer giriniz!!", "Hata");
                    txtTestPcbaAdet.Text = "";
                }
            }
        }

        private void txtTestKartPos_TextChanged(object sender, EventArgs e)
        {
            if (txtTestKartPos.Text != "")
            {
                double a = Convert.ToDouble(txtTestKartPos.Text);
                if (a > 0.00 && a > 75.00)
                {
                    MessageBox.Show("0.00 ile 75.00 arasında bir değer giriniz!!", "Hata");
                    txtTestKartPos.Text = "";
                }
            }
        }

        private void txtKartTestPos_TextChanged(object sender, EventArgs e)
        {
            if (txtKartTestPos.Text != "")
            {
                double a = Convert.ToDouble(txtKartTestPos.Text);
                if (a > 0.00 && a > 75.00)
                {
                    MessageBox.Show("0.00 ile 75.00 arasında bir değer giriniz!!", "Hata");
                    txtKartTestPos.Text = "";
                }
            }
        }

        private void txtBeslemeVoltaj_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtBeslemeVoltaj.Text != "")
                {
                    double a = Convert.ToDouble(txtBeslemeVoltaj.Text);
                    if (a > 0.01 && a > 24.00)
                    {
                        MessageBox.Show("0.01 ile 24.00 arasında bir değer giriniz!!", "Hata");
                        txtBeslemeVoltaj.Text = "";
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void txtBeslemeAkim_TextChanged(object sender, EventArgs e)
        {
            if (txtBeslemeAkim.Text != "")
            {
                try
                {
                    double a = Convert.ToDouble(txtBeslemeAkim.Text);
                    if (a > 0.00 && a > 3.00)
                    {
                        MessageBox.Show("0.00 ile 3.00 arasında bir değer giriniz!!", "Hata");
                        txtBeslemeAkim.Text = "";
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        private void txtVoltajMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtVoltajMIN.Text != "")
            {
                double a = Convert.ToDouble(txtVoltajMIN.Text);
                if (a > 0.00 && a > 3.00)
                {
                    MessageBox.Show("0.00 ile 3.00 arasında bir değer giriniz!!", "Hata");
                    txtVoltajMIN.Text = "";
                }
            }
        }

        private void txtVoltajMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtVoltajMAX.Text != "")
            {
                double a = Convert.ToDouble(txtVoltajMAX.Text);
                double b = Convert.ToDouble(txtVoltajMIN.Text);
                if (a > 0.00 && a > 3.00)
                {
                    MessageBox.Show("0.00 ile 3.00 arasında bir değer giriniz!!", "Hata");
                    txtVoltajMAX.Text = "";
                }
                else if (b > a)
                {
                    MessageBox.Show("MAX değer MIN değerden küçük olamaz!!", "Hata");
                    txtVoltajMAX.Text = "";
                }
            }
        }

        private void txtAkimMIN_TextChanged(object sender, EventArgs e)
        {
            if (txtAkimMIN.Text != "")
            {
                double a = Convert.ToDouble(txtAkimMIN.Text);
                if (a > 0.00 && a > 3.00)
                {
                    MessageBox.Show("0.00 ile 3.00 arasında bir değer giriniz!!", "Hata");
                    txtAkimMIN.Text = "";
                }
            }
        }

        private void txtAkimMAX_TextChanged(object sender, EventArgs e)
        {
            if (txtAkimMAX.Text != "")
            {
                double a = Convert.ToDouble(txtAkimMAX.Text);
                double b = Convert.ToDouble(txtAkimMIN.Text);
                if (a > 0.00 && a > 3.00)
                {
                    MessageBox.Show("0.00 ile 3.00 arasında bir değer giriniz!!", "Hata");
                    txtAkimMAX.Text = "";
                }
                else if (b > a)
                {
                    MessageBox.Show("MAX değer MIN değerden küçük olamaz!!", "Hata");
                    txtAkimMAX.Text = "";
                }
            }
        }

        #endregion

        //*****Text Bilgilerine girilen port bilgilerinin kontrolü*****
        #region Port Bilgisi kontrolü

        private void txtLedPort15_Leave(object sender, EventArgs e)
        {
            if (txtLedPort15.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
            else if (txtLedPort15.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort15.Text = "";
            }
        }

        private void txtLedPort14_Leave(object sender, EventArgs e)
        {
            if (txtLedPort14.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
            else if (txtLedPort14.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort14.Text = "";
            }
        }

        private void txtLedPort13_Leave(object sender, EventArgs e)
        {
            if (txtLedPort13.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
            else if (txtLedPort13.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort13.Text = "";
            }
        }

        private void txtLedPort12_Leave(object sender, EventArgs e)
        {
            if (txtLedPort12.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
            else if (txtLedPort12.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort12.Text = "";
            }
        }

        private void txtLedPort11_Leave(object sender, EventArgs e)
        {
            if (txtLedPort11.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
            else if (txtLedPort11.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort11.Text = "";
            }
        }

        private void txtLedPort10_Leave(object sender, EventArgs e)
        {
            if ((txtLedPort10.Text == txtLedPort1.Text))
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
            else if (txtLedPort10.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort10.Text = "";
            }
        }

        private void txtLedPort9_Leave(object sender, EventArgs e)
        {
            if (txtLedPort9.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
            else if (txtLedPort9.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort9.Text = "";
            }
        }

        private void txtLedPort8_Leave(object sender, EventArgs e)
        {
            if (txtLedPort8.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
            else if (txtLedPort8.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort8.Text = "";
            }
        }

        private void txtLedPort7_Leave(object sender, EventArgs e)
        {
            if (txtLedPort7.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
            else if (txtLedPort7.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort7.Text = "";
            }
        }

        private void txtLedPort6_Leave(object sender, EventArgs e)
        {
            if (txtLedPort6.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
            else if (txtLedPort6.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort6.Text = "";
            }
        }

        private void txtLedPort5_Leave(object sender, EventArgs e)
        {
            if (txtLedPort5.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
            else if (txtLedPort5.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort5.Text = "";
            }
        }

        private void txtLedPort4_Leave(object sender, EventArgs e)
        {
            if (txtLedPort4.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
            else if (txtLedPort4.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort4.Text = "";
            }
        }

        private void txtLedPort3_Leave(object sender, EventArgs e)
        {
            if (txtLedPort3.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort3.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
        }

        private void txtLedPort2_Leave(object sender, EventArgs e)
        {
            if (txtLedPort2.Text == txtLedPort1.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
            else if (txtLedPort2.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort2.Text = "";
            }
        }

        private void txtLedPort1_Leave(object sender, EventArgs e)
        {

            if (txtLedPort1.Text == txtLedPort2.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort3.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort4.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort5.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort6.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort7.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort8.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort9.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort10.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort11.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort3.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort12.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort13.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort14.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }
            else if (txtLedPort1.Text == txtLedPort15.Text)
            {
                MessageBox.Show("Aynı port bilgisi girişi yapılmıştır. Port Bilgisini değiştiriniz!!", "Hata");
                txtLedPort1.Text = "";
            }

        }


        #endregion

        //*****Tıklanılan butonların göndereceği bilgiler*****
        #region PUSH-DOWN

        private void btnYurutmeMotorUP_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("yurutmeAxisUp", true));
            threadPushButton.Start();
        }

        private void btnYurutmeMotorDOWN_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("yurutmeAxisDown", true));
            threadPushButton.Start();
        }
        private void btnTestButonDown_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("testAxisDown", true));
            threadPushButton.Start();
        }

        private void btnTestButonUp_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("testAxisUp", true));
            threadPushButton.Start();
        }

        private void btnKonveyorUP_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("konveyorUp", true));
            threadPushButton.Start();
        }

        private void btnKonveyorDOWN_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("konveyorDown", true));
            threadPushButton.Start();
        }

        private void btnDonmePistonUP_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("donmeUp", true));
            threadPushButton.Start();
        }

        private void btnDonmePistonDOWN_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("donmeDown", true));
            threadPushButton.Start();
        }
        private void btnKirmaPistonUP_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("kirmaUp", true));
            threadPushButton.Start();
        }
        private void btnKirmaPistonDOWN_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("kirmaDown", true));
            threadPushButton.Start();
        }

        private void btnTestPistonUP_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("testUp", true));
            threadPushButton.Start();
        }
        private void btnGripperDownUP_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("yurutmeGripperDown", true));
            threadPushButton.Start();
        }

        private void btnGripperUpDOWN_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("yurutmeGripperUp", true));
            threadPushButton.Start();
        }

        private void btnTestGripperDownUP_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("testGripperDown", true));
            threadPushButton.Start();
        }

        private void btnTestGripperUpDOWN_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("testGripperUp", true));
            threadPushButton.Start();
        }

        private void btnBaskiPistonUP_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("baskiUp", true));
            threadPushButton.Start();
        }

        private void btnBaskiPistonDOWN_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("baskiDown", true));
            threadPushButton.Start();
        }

        private void btnTestPistonDOWN_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("testDown", true));
            threadPushButton.Start();
        }

        private void btnTestKartPosGit_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("kartAlmaPosGit", true));
            threadPushButton.Start();
        }

        private void btnTestKartAlmaPosGit_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("kartAlmaPosGit", true));
            threadPushButton.Start();
        }

        private void btnTestKartBirakmaPosGit_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("kartBirakmaPosGit", true));
            threadPushButton.Start();
        }

        private void btnUretilenAdetSifirla_MouseDown(object sender, MouseEventArgs e)
        {
            threadPushButton = new Thread(() => nxCompoletBoolWrite("adetSifirla", true));
            threadPushButton.Start();
        }

        #endregion

        //*****PLC bilgilerini sürekli okuyan kod parçası****
        #region ReadPlcData

        private void readPlcData()
        {
            while (true)
            {
                Thread.Sleep(50);
                if (readLoopCounter == 2)
                {
                    readLoopCounter = 0;


                    /************************************************HARDWARE*****************************************************************************/
                    /********************************DONANIM BUTONLARI İLE İLGİLİ AKSİYONLAR**************************************************************/
                    try
                    {
                        txtYurutmeMotor.Text = nxCompoletDoubleRead("yurutmePos").ToString();

                        txtTestMotor.Text = nxCompoletDoubleRead("testPos").ToString();

                        //47
                        if ((nxCompoletStringRead("homeDurum") == "0") && !boolReadStatus)
                        {
                            btnHome.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("homeDurum") == "1") && !boolReadStatus)
                        {
                            btnHome.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("homeDurum") == "2") && !boolReadStatus)
                        {
                            btnHome.Appearance.BackColor = Color.Green;
                        }

                        //48
                        if ((nxCompoletStringRead("kirmaDurum") == "0") && !boolReadStatus)
                        {
                            btnKirmaPistonYukarida.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("kirmaDurum") == "1") && !boolReadStatus)
                        {
                            btnKirmaPistonYukarida.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("kirmaDurum") == "2") && !boolReadStatus)
                        {
                            btnKirmaPistonYukarida.Appearance.BackColor = Color.Green;
                        }

                        //49
                        if ((nxCompoletStringRead("donmeDurum") == "0") && !boolReadStatus)
                        {
                            btnDonmePistonYukarida.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("donmeDurum") == "1") && !boolReadStatus)
                        {
                            btnDonmePistonYukarida.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("donmeDurum") == "2") && !boolReadStatus)
                        {
                            btnDonmePistonYukarida.Appearance.BackColor = Color.Green;
                        }

                        //50
                        if ((nxCompoletStringRead("kirmaUrunSensor") == "0") && !boolReadStatus)
                        {
                            btnKirmaUrunSensor.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("kirmaUrunSensor") == "1") && !boolReadStatus)
                        {
                            btnKirmaUrunSensor.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("kirmaUrunSensor") == "2") && !boolReadStatus)
                        {
                            btnKirmaUrunSensor.Appearance.BackColor = Color.Green;
                        }

                        //51
                        if ((nxCompoletStringRead("kartVarYokSensor") == "0") && !boolReadStatus)
                        {
                            btnKartVarYokSensor.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("kartVarYokSensor") == "1") && !boolReadStatus)
                        {
                            btnKartVarYokSensor.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("kartVarYokSensor") == "2") && !boolReadStatus)
                        {
                            btnKartVarYokSensor.Appearance.BackColor = Color.Green;
                        }

                        //52
                        if ((nxCompoletStringRead("yurutmeGripperDurum") == "0") && !boolReadStatus)
                        {
                            btnYurutmeGripperYukarida.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("yurutmeGripperDurum") == "1") && !boolReadStatus)
                        {
                            btnYurutmeGripperYukarida.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("yurutmeGripperDurum") == "2") && !boolReadStatus)
                        {
                            btnYurutmeGripperYukarida.Appearance.BackColor = Color.Green;
                        }

                        //53
                        if ((nxCompoletStringRead("testGripperDurum") == "0") && !boolReadStatus)
                        {
                            btnTestGripperYukarida.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("testGripperDurum") == "1") && !boolReadStatus)
                        {
                            btnTestGripperYukarida.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("testGripperDurum") == "2") && !boolReadStatus)
                        {
                            btnTestGripperYukarida.Appearance.BackColor = Color.Green;
                        }

                        //54
                        if ((nxCompoletStringRead("testPistonDurum") == "0") && !boolReadStatus)
                        {
                            btnTestPistonYukarida.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("testPistonDurum") == "1") && !boolReadStatus)
                        {
                            btnTestPistonYukarida.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("testPistonDurum") == "2") && !boolReadStatus)
                        {
                            btnTestPistonYukarida.Appearance.BackColor = Color.Green;
                        }

                        //55
                        if ((nxCompoletStringRead("bariyerSensor") == "0") && !boolReadStatus)
                        {
                            btnBariyerSensor.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("bariyerSensor") == "1") && !boolReadStatus)
                        {
                            btnBariyerSensor.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("bariyerSensor") == "2") && !boolReadStatus)
                        {
                            btnBariyerSensor.Appearance.BackColor = Color.Green;
                        }

                        //56
                        if ((nxCompoletStringRead("solKapiSensor") == "0") && !boolReadStatus)
                        {
                            btnSolaKapi.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("solKapiSensor") == "1") && !boolReadStatus)
                        {
                            btnSolaKapi.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("solKapiSensor") == "2") && !boolReadStatus)
                        {
                            btnSolaKapi.Appearance.BackColor = Color.Green;
                        }

                        //57
                        if ((nxCompoletStringRead("sagKapiSensor") == "0") && !boolReadStatus)
                        {
                            btnArkaKapiSensor.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("sagKapiSensor") == "1") && !boolReadStatus)
                        {
                            btnArkaKapiSensor.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("sagKapiSensor") == "2") && !boolReadStatus)
                        {
                            btnArkaKapiSensor.Appearance.BackColor = Color.Green;
                        }

                        //58
                        if ((nxCompoletStringRead("arkaKapıSensor") == "0") && !boolReadStatus)
                        {
                            btnSagKapiSensor.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("arkaKapıSensor") == "1") && !boolReadStatus)
                        {
                            btnSagKapiSensor.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("arkaKapıSensor") == "2") && !boolReadStatus)
                        {
                            btnSagKapiSensor.Appearance.BackColor = Color.Green;
                        }

                        //59
                        if ((nxCompoletStringRead("onKapiSensor") == "0") && !boolReadStatus)
                        {
                            btnOnKapiSensor.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("onKapiSensor") == "1") && !boolReadStatus)
                        {
                            btnOnKapiSensor.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("onKapiSensor") == "2") && !boolReadStatus)
                        {
                            btnOnKapiSensor.Appearance.BackColor = Color.Green;
                        }

                        //60
                        if ((nxCompoletStringRead("yurutmeMotorDurum") == "0") && !boolReadStatus)
                        {
                            btnYurutmeMotorAriza.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("yurutmeMotorDurum") == "1") && !boolReadStatus)
                        {
                            btnYurutmeMotorAriza.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("yurutmeMotorDurum") == "2") && !boolReadStatus)
                        {
                            btnYurutmeMotorAriza.Appearance.BackColor = Color.Green;
                        }

                        //61
                        if ((nxCompoletStringRead("testMotorDurum") == "0") && !boolReadStatus)
                        {
                            btnTestMotorAriza.Appearance.BackColor = Color.Maroon;
                        }
                        else if ((nxCompoletStringRead("testMotorDurum") == "1") && !boolReadStatus)
                        {
                            btnTestMotorAriza.Appearance.BackColor = Color.Yellow;
                        }
                        else if ((nxCompoletStringRead("testMotorDurum") == "2") && !boolReadStatus)
                        {
                            btnTestMotorAriza.Appearance.BackColor = Color.Green;
                        }

                        /**************************************HARDWARE**********************************/


                        if (nxCompoletStringRead("makinaDurum") == "0")
                        {
                            txtMakinaDurumu.Text = "MAKİNA HAZIR DEĞİL!";
                        }
                        else if (nxCompoletStringRead("makinaDurum") == "1")
                        {
                            txtMakinaDurumu.Text = "KART BEKLENİYOR!";
                        }
                        else if (nxCompoletStringRead("makinaDurum") == "2")
                        {
                            txtMakinaDurumu.Text = "ÜRETİM YAPILIYOR!";
                        }
                        else if (nxCompoletStringRead("makinaDurum") == "3")
                        {
                            txtMakinaDurumu.Text = "ÜRETİM DURDURULDU!";
                        }
                        else if (nxCompoletStringRead("makinaDurum") == "4")
                        {
                            txtMakinaDurumu.Text = "SİSTEM HAZIR!";
                        }

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }


                readLoopCounter++;
            }

        }

        #endregion

        //*****Kaydedilen/Kayıtlı olan INI dosyalarının bilgileri*****
        #region GET BÖLÜMLERİ (GETMODELNAME, GETMODELDATA, GETMODELSETTİNG, CONTROLMODELDATA INI vb.)

        private void createModel(string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                File.Delete(iniFilePath);
            }

            if ((txtModelName.Text != "") && (txtTestPcbBoy.Text != "") && (txtTestPcbaAdet.Text != "") && (txtKartTestPos.Text != "") && (txtTestKartPos.Text != ""))
            {
                try
                {

                INIKaydet kaydet = new INIKaydet(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                kaydet.Yaz("TestPcbBot", "Metin Kutusu", Convert.ToString(txtTestPcbBoy.Text));
                kaydet.Yaz("TestPcbaAdet", "Metin Kutusu", Convert.ToString(txtTestPcbaAdet.Text));
                kaydet.Yaz("TestKartPos", "Metin Kutusu", Convert.ToString(txtTestKartPos.Text));
                kaydet.Yaz("KartTestPos", "Metin Kutusu", Convert.ToString(txtKartTestPos.Text));
                kaydet.Yaz("BeslemeVoltaj", "Metin Kutusu", Convert.ToString(txtBeslemeVoltaj.Text));
                kaydet.Yaz("BeslemeAkim", "Metin Kutusu", Convert.ToString(txtBeslemeAkim.Text));


                kaydet.Yaz("TestBox", "Metin Kutusu", Convert.ToString(txtTestBox.Text));
                kaydet.Yaz("txtLedPort1", "Metin Kutusu", Convert.ToString(txtLedPort1.Text));
                kaydet.Yaz("txtLedPort2", "Metin Kutusu", Convert.ToString(txtLedPort2.Text));
                kaydet.Yaz("txtLedPort3", "Metin Kutusu", Convert.ToString(txtLedPort3.Text));
                kaydet.Yaz("txtLedPort4", "Metin Kutusu", Convert.ToString(txtLedPort4.Text));
                kaydet.Yaz("txtLedPort5", "Metin Kutusu", Convert.ToString(txtLedPort5.Text));
                kaydet.Yaz("txtLedPort6", "Metin Kutusu", Convert.ToString(txtLedPort6.Text));
                kaydet.Yaz("txtLedPort7", "Metin Kutusu", Convert.ToString(txtLedPort7.Text));
                kaydet.Yaz("txtLedPort8", "Metin Kutusu", Convert.ToString(txtLedPort8.Text));
                kaydet.Yaz("txtLedPort9", "Metin Kutusu", Convert.ToString(txtLedPort9.Text));
                kaydet.Yaz("txtLedPort10", "Metin Kutusu", Convert.ToString(txtLedPort10.Text));
                kaydet.Yaz("txtLedPort11", "Metin Kutusu", Convert.ToString(txtLedPort11.Text));
                kaydet.Yaz("txtLedPort12", "Metin Kutusu", Convert.ToString(txtLedPort12.Text));
                kaydet.Yaz("txtLedPort13", "Metin Kutusu", Convert.ToString(txtLedPort13.Text));
                kaydet.Yaz("txtLedPort14", "Metin Kutusu", Convert.ToString(txtLedPort14.Text));
                kaydet.Yaz("txtLedPort15", "Metin Kutusu", Convert.ToString(txtLedPort15.Text));

                kaydet.Yaz("rbtnRGBChecked", "Metin Kutusu", Convert.ToString(rbtnRGB.Checked));
                kaydet.Yaz("rbtnCIExyChecked", "Metin Kutusu", Convert.ToString(rbtnCIExy.Checked));
                kaydet.Yaz("rbtnCIEuvChecked", "Metin Kutusu", Convert.ToString(rbtnCIExy.Checked));
                kaydet.Yaz("rbtnSaturationChecked", "Metin Kutusu", Convert.ToString(rbtnSaturation.Checked));
                kaydet.Yaz("rbtnHueChecked", "Metin Kutusu", Convert.ToString(rbtnHue.Checked));
                kaydet.Yaz("rbtnIntenstyChecked", "Metin Kutusu", Convert.ToString(rbtnIntensty.Checked));
                kaydet.Yaz("rbtnCCTChecked", "Metin Kutusu", Convert.ToString(rbtnCCT.Checked));
                kaydet.Yaz("rbtnWavelengthChecked", "Metin Kutusu", Convert.ToString(rbtnWavelength.Checked));



                MessageBox.Show("Bütün Ayarlar Dosyaya Başarıyla Kaydedildi.");

                }
                catch (Exception ex)
                {
                    MessageBox.Show("" + ex);
                    return;
                    
                }
                threadWriteString = new Thread(() => nxCompoletStringWrite("pcbBoy", txtTestPcbBoy.Text));
                threadWriteString.Start();

                threadWriteString = new Thread(() => nxCompoletStringWrite("pcbAdet", txtTestPcbaAdet.Text));
                threadWriteString.Start();

                threadWriteString = new Thread(() => nxCompoletStringWrite("kartAlmaPos", txtTestKartPos.Text));
                threadWriteString.Start();

                threadWriteString = new Thread(() => nxCompoletStringWrite("kartTestPos", txtKartTestPos.Text));
                threadWriteString.Start();

                if (cmbAkimVoltaj.Text == "YOK" && cmbLedOlcum.Text == "YOK")
                {
                    threadWriteString = new Thread(() => nxCompoletStringWrite("testVarYok", "0"));
                    threadWriteString.Start();
                }
                else
                {
                    threadWriteString = new Thread(() => nxCompoletStringWrite("testVarYok", "1"));
                    threadWriteString.Start();
                }
            }
            else
            {
                MessageBox.Show("Model Adı veya Test Edilecek Led Sayısı Boş Kalamaz");
            }
        }


        private void selectPath(System.Windows.Forms.TextBox textBox)
        {
            if (txtModelName.Text != "")
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Bat Files (BAT)|*.BAT;";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox.Text = openFileDialog.FileName;
                    }

                    if (File.Exists(txtModelName.Text))
                    {
                        ini = new INIKaydet(txtModelName.Text);
                        if (ini.Oku("cbError", "Metin Kutusu") == "True")
                        {
                            txtTestPcbBoy.Text = ini.Oku("txtTestPcbBoy", "Metin Kutusu");
                            txtTestPcbaAdet.Text = ini.Oku("projectName", "Metin Kutusu");
                            txtTestKartPos.Text = ini.Oku("txtTestKartPos", "Metin Kutusu");
                            txtTestBox.Text = ini.Oku("txtTestBox", "Metin Kutusu");
                            txtLedPort1.Text = ini.Oku("txtLedPort1", "Metin Kutusu");
                            txtLedPort2.Text = ini.Oku("txtLedPort2", "Metin Kutusu");
                            txtLedPort3.Text = ini.Oku("txtLedPort3", "Metin Kutusu");
                            txtLedPort4.Text = ini.Oku("txtLedPort4", "Metin Kutusu");
                            txtLedPort5.Text = ini.Oku("txtLedPort5", "Metin Kutusu");
                            txtLedPort6.Text = ini.Oku("txtLedPort6", "Metin Kutusu");
                            txtLedPort7.Text = ini.Oku("txtLedPort7", "Metin Kutusu");
                            txtLedPort8.Text = ini.Oku("txtLedPort8", "Metin Kutusu");
                            txtLedPort9.Text = ini.Oku("txtLedPort9", "Metin Kutusu");
                            txtLedPort10.Text = ini.Oku("txtLedPort10", "Metin Kutusu");
                            txtLedPort11.Text = ini.Oku("txtLedPort11", "Metin Kutusu");
                            txtLedPort12.Text = ini.Oku("txtLedPort12", "Metin Kutusu");
                            txtLedPort13.Text = ini.Oku("txtLedPort13", "Metin Kutusu");
                            txtLedPort14.Text = ini.Oku("txtLedPort14", "Metin Kutusu");
                            txtLedPort15.Text = ini.Oku("txtLedPort15", "Metin Kutusu");

                            MessageBox.Show("Bütün Ayarlar Dosyadan Başarıyla Okundu.");
                        }
                        else if (ini.Oku("cbError", "Metin Kutusu") == "False")
                            MessageBox.Show("Bütün Ayarlar Dosyadan Başarıyla Okunmayan Bilgiler Var. Dosyayı Kontrol Ediniz!!");
                    }
                }
                catch (Exception hata)
                {
                    MessageBox.Show("ini Dosyası Hasarlı" + hata);
                }
            }
            else
            {
                MessageBox.Show("Dosya Yolu Boş Kalamaz");
            }

        }

        #endregion

        //*****PLC ile bağlantı sağlayan metotlar*****
        #region NXCompolet

        public bool nxCompoletBoolWrite(string variable, bool value)  //NX BOOL WRITE
        {
            try
            {
                nxCompolet2.WriteVariable(variable, value);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public bool nxCompoletBoolRead(string variable)  //NX BOOL READ
        {
            try
            {
                boolReadStatus = false;
                bool staticValue = Convert.ToBoolean(nxCompolet2.ReadVariable(variable));
                return staticValue;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                boolReadStatus = true;
                return false;
            }
        }


        public bool nxCompoletStringWrite(string variable, string value)  //NX STRING WRİTE
        {
            try
            {
                nxCompolet2.WriteVariable(variable, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string nxCompoletStringRead(string variable)  //NX STRING READ
        {
            try
            {
                string staticStringValue = Convert.ToString(nxCompolet2.ReadVariable(variable));
                return staticStringValue;
            }
            catch (Exception e)
            {

                return "PLC Okunmadı Hatası:" + e;
            }

        }

        public string nxCompoletDoubleRead(string variable)  //NX STRING
        {
            try
            {
                string s = Convert.ToString(nxCompolet2.ReadVariable(variable));
                return s;
            }
            catch (Exception e)
            {
                return "PLC Okunmadı Hatası:" + e;
            }
        }


        #endregion

        //****GwInstek Port bağlantılarından gelen bilgilerin çekilen kısımlar*****
        #region Ports Data_Received

        private void portGwInstek_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (portGwInstek.BytesToRead > 0)
            {
                array[counterByte] = Convert.ToByte(portGwInstek.ReadByte());
                counterByte++;

                Thread.Sleep(100);
            }
        }

        #endregion

        //*****Programlanabilir Power Supply için gerekli kod blokları*****
        #region GwInstek

        private void cmbAkimVoltaj_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (portGwInstek.IsOpen)
                portGwInstek.Close();

            portGwInstek.Open();
            _ = (cmbAkimVoltaj.Text == "VAR") ? txtVoltajMIN.Enabled = true : txtVoltajMIN.Enabled = false;
            _ = (cmbAkimVoltaj.Text == "VAR") ? txtVoltajMIN.Text = "0.01" : txtVoltajMIN.Text = "24.00";
            _ = (cmbAkimVoltaj.Text == "VAR") ? txtVoltajMAX.Enabled = true : txtVoltajMAX.Enabled = false;
            _ = (cmbAkimVoltaj.Text == "VAR") ? txtVoltajMAX.Text = "24.00" : txtVoltajMAX.Text = "0.01";

            _ = (cmbAkimVoltaj.Text == "VAR") ? txtVoltajOkunan.Enabled = true : txtVoltajOkunan.Enabled = false;


            _ = (cmbAkimVoltaj.Text == "VAR") ? txtAkimMIN.Enabled = true : txtAkimMIN.Enabled = false;
            _ = (cmbAkimVoltaj.Text == "VAR") ? txtAkimMIN.Text = "0.01" : txtAkimMIN.Text = "3.00";
            _ = (cmbAkimVoltaj.Text == "VAR") ? txtAkimMAX.Enabled = true : txtAkimMAX.Enabled = false;
            _ = (cmbAkimVoltaj.Text == "VAR") ? txtAkimMAX.Text = "3.00" : txtAkimMAX.Text = "0.01";

            _ = (cmbAkimVoltaj.Text == "VAR") ? txtAkimOkunan.Enabled = true : txtAkimOkunan.Enabled = false;

            if (cmbAkimVoltaj.Text == "VAR")
            {
                GwInstekPortOpen();
                Thread.Sleep(1250);
                sendData(txtVoltajMIN.Text, txtVoltajMAX.Text, txtAkimMIN.Text, txtAkimMAX.Text);

                GwInstekSendDataVoltaj(txtBeslemeVoltaj.Text);
                

            }
            else if (cmbAkimVoltaj.Text == "YOK")
            {
                btnKartAkimVoltaj.BackColor = Color.White;
                txtAkimMIN.Text = "";
                txtAkimMAX.Text = "";
                txtAkimOkunan.Text = "";
                txtVoltajMIN.Text = "";
                txtVoltajMAX.Text = "";
                txtVoltajOkunan.Text = "";
                Thread.Sleep(1000);
                GwInstekPortClose();

            }
            portGwInstek.Close();
        }
        public void sendData(string voltajMin, string voltajMax, string akimMin, string akimMax)
        {
            if ((akimMin != "") && (akimMax != ""))
            {
                txtAkimOkunanHex.Text = null;
                byte[] byteArray = new byte[2];
                string ifade1 = "0x41";
                string ifade2 = "0x0D";
                int hex = hextointConvert(ifade1.Substring(2, 1)) * 16 + hextointConvert(ifade1.Substring(3, 1));
                byteArray[0] = Convert.ToByte(hex);

                int hex2 = hextointConvert(ifade2.Substring(2, 1)) * 16 + hextointConvert(ifade2.Substring(3, 1));
                byteArray[1] = Convert.ToByte(hex2);

                portGwInstek.Write(byteArray, 0, 2);
                Thread.Sleep(750);
                try
                {
                    for (int j = 0; j < counterByte; j++)
                    {
                        txtAkimOkunanHex.Text += string.Format("{0:X}", array[j]);
                    }

                    counterByte = 0;
                    string hexValue = txtAkimOkunanHex.Text; // HEX değeri
                    string charValue = HexToChar(hexValue.Substring(2, 10)); // CHAR değeri

                    if (charValue != null)
                    {
                        txtAkimOkunan.Text = charValue;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Power Supply okumasında belirlenmeyen hata oluştu!!" + ex);
                }
            }


            if ((voltajMin != "") && (voltajMax != ""))
            {
                txtVoltajOkunanHex.Text = null;

                byte[] byteArray = new byte[2];
                string ifade1 = "0x56";
                string ifade2 = "0x0D";
                int hex = hextointConvert(ifade1.Substring(2, 1)) * 16 + hextointConvert(ifade1.Substring(3, 1));
                byteArray[0] = Convert.ToByte(hex);

                int hex2 = hextointConvert(ifade2.Substring(2, 1)) * 16 + hextointConvert(ifade2.Substring(3, 1));
                byteArray[1] = Convert.ToByte(hex2);

                portGwInstek.Write(byteArray, 0, 2);
                Thread.Sleep(750);

                try
                {
                    for (int j = 0; j < counterByte; j++)
                    {
                        txtVoltajOkunanHex.Text += string.Format("{0:X}", array[j]);
                    }

                    counterByte = 0;
                    string hexValue = txtVoltajOkunanHex.Text; // HEX değeri
                    string charValue = HexToChar(hexValue.Substring(2, 10)); // CHAR değeri

                    if (charValue != null)
                    {
                        txtVoltajOkunan.Text = charValue;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Power Supply okumasında belirlenmeyen hata oluştu!!" + ex);
                }
            }
        }

        private void GwInstekSendDataVoltaj(string voltaj)
        {
            double a = Convert.ToDouble(txtBeslemeVoltaj.Text);
            int b = (int)a;

            if ((txtBeslemeVoltaj.Text != ""))
            {
                Thread.Sleep(100);
                for (int i = 0; i < b; i++)
                {
                    string byteString0 = "0x53";
                    string byteString1 = "0x56";
                    string byteString2 = "0x2B";
                    string byteString3 = "0x0D";

                    int hex = hextointConvert(byteString0.Substring(2, 1)) * 16 + hextointConvert(byteString0.Substring(3, 1));
                    byteArrayVS[0] = Convert.ToByte(hex);
                    int hex1 = hextointConvert(byteString1.Substring(2, 1)) * 16 + hextointConvert(byteString1.Substring(3, 1));
                    byteArrayVS[1] = Convert.ToByte(hex1);
                    int hex2 = hextointConvert(byteString2.Substring(2, 1)) * 16 + hextointConvert(byteString2.Substring(3, 1));
                    byteArrayVS[2] = Convert.ToByte(hex2);
                    int hex3 = hextointConvert(byteString3.Substring(2, 1)) * 16 + hextointConvert(byteString3.Substring(3, 1));
                    byteArrayVS[3] = Convert.ToByte(hex3);

                    portGwInstek.Write(byteArrayVS, 0, 4);
                    Thread.Sleep(750);

                }
            }
            else
            {
                MessageBox.Show("Voltaj değeri boş kalmıştır!! ", "Hata");
            }
        }
        private void GwInstekClearDataVoltaj()
        {
            Thread.Sleep(100);

            string byteString0 = "0x53";
            string byteString1 = "0x56";
            string byteString2 = "0x20";
            string byteString3 = "0x31";
            string byteString4 = "0x30";
            string byteString5 = "0x2E";
            string byteString6 = "0x30";
            string byteString7 = "0x30";
            string byteString8 = "0x0D"; //31 30 2E 30 30

            int hex0 = hextointConvert(byteString0.Substring(2, 1)) * 16 + hextointConvert(byteString0.Substring(3, 1));
            byteArrayVS[0] = Convert.ToByte(hex0);
            int hex1 = hextointConvert(byteString1.Substring(2, 1)) * 16 + hextointConvert(byteString1.Substring(3, 1));
            byteArrayVS[1] = Convert.ToByte(hex1);
            int hex2 = hextointConvert(byteString2.Substring(2, 1)) * 16 + hextointConvert(byteString2.Substring(3, 1));
            byteArrayVS[2] = Convert.ToByte(hex2);
            int hex3 = hextointConvert(byteString3.Substring(2, 1)) * 16 + hextointConvert(byteString3.Substring(3, 1));
            byteArrayVS[3] = Convert.ToByte(hex3);
            int hex4 = hextointConvert(byteString4.Substring(2, 1)) * 16 + hextointConvert(byteString4.Substring(3, 1));
            byteArrayVS[4] = Convert.ToByte(hex4);
            int hex5 = hextointConvert(byteString5.Substring(2, 1)) * 16 + hextointConvert(byteString5.Substring(3, 1));
            byteArrayVS[5] = Convert.ToByte(hex5);
            int hex6 = hextointConvert(byteString6.Substring(2, 1)) * 16 + hextointConvert(byteString6.Substring(3, 1));
            byteArrayVS[6] = Convert.ToByte(hex6);
            int hex7 = hextointConvert(byteString7.Substring(2, 1)) * 16 + hextointConvert(byteString7.Substring(3, 1));
            byteArrayVS[7] = Convert.ToByte(hex7);
            int hex8 = hextointConvert(byteString8.Substring(2, 1)) * 16 + hextointConvert(byteString8.Substring(3, 1));
            byteArrayVS[8] = Convert.ToByte(hex8);

            portGwInstek.Write(byteArrayVS, 0, 9);
            Thread.Sleep(1000);
        }
        private void GwInstekSendDataAkim(string akim)
        {

            double b = Convert.ToDouble(txtBeslemeAkim.Text);
            int a = (int)b;

            if ((txtBeslemeAkim.Text != ""))
            {
                Thread.Sleep(100);

                for (int i = 0; i < a; i++)
                {

                    string byteString0 = "0x53";
                    string byteString1 = "0x49";
                    string byteString2 = "0x2B";
                    string byteString3 = "0x0D";

                    int hex = hextointConvert(byteString0.Substring(2, 1)) * 16 + hextointConvert(byteString0.Substring(3, 1));
                    byteArrayAS[0] = Convert.ToByte(hex);
                    int hex1 = hextointConvert(byteString1.Substring(2, 1)) * 16 + hextointConvert(byteString1.Substring(3, 1));
                    byteArrayAS[1] = Convert.ToByte(hex1);
                    int hex2 = hextointConvert(byteString2.Substring(2, 1)) * 16 + hextointConvert(byteString2.Substring(3, 1));
                    byteArrayAS[2] = Convert.ToByte(hex2);
                    int hex3 = hextointConvert(byteString3.Substring(2, 1)) * 16 + hextointConvert(byteString3.Substring(3, 1));
                    byteArrayAS[3] = Convert.ToByte(hex3);

                    portGwInstek.Write(byteArrayAS, 0, 4);
                    Thread.Sleep(750);

                }
            }
            else
            {
                MessageBox.Show("Akım değeri boş kalmıştır!! ", "Hata");
            }
        }
        private void GwInstekClearDataAkim()
        {
            Thread.Sleep(100);

            string byteString0 = "0x53";
            string byteString1 = "0x49";
            string byteString2 = "0x20";
            string byteString3 = "0x30";
            string byteString4 = "0x2E";
            string byteString5 = "0x30";
            string byteString6 = "0x30";
            string byteString7 = "0x0D"; //30 30 2E 30 30

            int hex0 = hextointConvert(byteString0.Substring(2, 1)) * 16 + hextointConvert(byteString0.Substring(3, 1));
            byteArrayAS[0] = Convert.ToByte(hex0);
            int hex1 = hextointConvert(byteString1.Substring(2, 1)) * 16 + hextointConvert(byteString1.Substring(3, 1));
            byteArrayAS[1] = Convert.ToByte(hex1);
            int hex2 = hextointConvert(byteString2.Substring(2, 1)) * 16 + hextointConvert(byteString2.Substring(3, 1));
            byteArrayAS[2] = Convert.ToByte(hex2);
            int hex3 = hextointConvert(byteString3.Substring(2, 1)) * 16 + hextointConvert(byteString3.Substring(3, 1));
            byteArrayAS[3] = Convert.ToByte(hex3);
            int hex4 = hextointConvert(byteString4.Substring(2, 1)) * 16 + hextointConvert(byteString4.Substring(3, 1));
            byteArrayAS[4] = Convert.ToByte(hex4);
            int hex5 = hextointConvert(byteString5.Substring(2, 1)) * 16 + hextointConvert(byteString5.Substring(3, 1));
            byteArrayAS[5] = Convert.ToByte(hex5);
            int hex6 = hextointConvert(byteString6.Substring(2, 1)) * 16 + hextointConvert(byteString6.Substring(3, 1));
            byteArrayAS[6] = Convert.ToByte(hex6);
            int hex7 = hextointConvert(byteString7.Substring(2, 1)) * 16 + hextointConvert(byteString7.Substring(3, 1));
            byteArrayAS[7] = Convert.ToByte(hex7);

            portGwInstek.Write(byteArrayAS, 0, 8);
            Thread.Sleep(750);
        }
        public static string HexToChar(string hex)
        {
            try
            {
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                string result = Encoding.ASCII.GetString(bytes);
                return result;
            }
            catch (Exception ex)
            {
                // Hata durumunda burada işleyebilirsiniz.
                MessageBox.Show("Hata: " + ex.Message);
                return null;
            }
        }
        private int hextointConvert(string hex)
        {
            if (hex == "1")
            {
                return 1;
            }
            else if (hex == "2")
            {
                return 2;
            }
            else if (hex == "3")
            {
                return 3;
            }
            else if (hex == "4")
            {
                return 4;
            }
            else if (hex == "5")
            {
                return 5;
            }
            else if (hex == "6")
            {
                return 6;
            }
            else if (hex == "7")
            {
                return 7;
            }
            else if (hex == "8")
            {
                return 8;
            }
            else if (hex == "9")
            {
                return 9;
            }
            else if (hex == "A")
            {
                return 10;
            }
            else if (hex == "B")
            {
                return 11;
            }
            else if (hex == "C")
            {
                return 12;
            }
            else if (hex == "D")
            {
                return 13;
            }
            else if (hex == "E")
            {
                return 14;
            }
            else if (hex == "F")
            {
                return 15;
            }
            return 0;
        }
        private void txtVoltajOkunan_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(txtVoltajOkunan.Text) > Convert.ToInt32(txtVoltajMIN.Text) && Convert.ToInt32(txtVoltajOkunan.Text) < Convert.ToInt32(txtVoltajMAX.Text))
                {
                    if (Convert.ToInt32(txtAkimOkunan.Text) > Convert.ToInt32(txtAkimMIN.Text) && Convert.ToInt32(txtAkimOkunan.Text) < Convert.ToInt32(txtAkimMAX.Text))
                    {
                        btnKartAkimVoltaj.Appearance.BackColor = Color.Green;
                    }
                    else
                        btnKartAkimVoltaj.Appearance.BackColor = Color.Red;
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        private void txtAkimOkunan_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(txtAkimOkunan.Text) > Convert.ToInt32(txtAkimMIN.Text) && Convert.ToInt32(txtAkimOkunan.Text) < Convert.ToInt32(txtAkimMAX.Text))
                {
                    if (Convert.ToInt32(txtVoltajOkunan.Text) > Convert.ToInt32(txtVoltajMIN.Text) && Convert.ToInt32(txtVoltajOkunan.Text) < Convert.ToInt32(txtVoltajMAX.Text))
                    {
                        btnKartAkimVoltaj.Appearance.BackColor = Color.Green;
                    }
                    else
                        btnKartAkimVoltaj.Appearance.BackColor = Color.Red;
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        private void GwInstekPortOpen()
        {
            byte[] byteArray = new byte[4];

            string byteString0 = "0x4B";
            string byteString1 = "0x4F";
            string byteString2 = "0x45";
            string byteString3 = "0x0D";

            int hex = hextointConvert(byteString0.Substring(2, 1)) * 16 + hextointConvert(byteString0.Substring(3, 1));
            byteArray[0] = Convert.ToByte(hex);

            int hex1 = hextointConvert(byteString1.Substring(2, 1)) * 16 + hextointConvert(byteString1.Substring(3, 1));
            byteArray[1] = Convert.ToByte(hex1);

            int hex2 = hextointConvert(byteString2.Substring(2, 1)) * 16 + hextointConvert(byteString2.Substring(3, 1));
            byteArray[2] = Convert.ToByte(hex2);

            int hex3 = hextointConvert(byteString3.Substring(2, 1)) * 16 + hextointConvert(byteString3.Substring(3, 1));
            byteArray[3] = Convert.ToByte(hex3);

            portGwInstek.Write(byteArray, 0, 4);
            Thread.Sleep(750);

        }
        private void GwInstekPortClose()
        {
            byte[] byteArray = new byte[4];

            string byteString0 = "0x4B";
            string byteString1 = "0x4F";
            string byteString2 = "0x44";
            string byteString3 = "0x0D";

            int hex = hextointConvert(byteString0.Substring(2, 1)) * 16 + hextointConvert(byteString0.Substring(3, 1));
            byteArray[0] = Convert.ToByte(hex);

            int hex1 = hextointConvert(byteString1.Substring(2, 1)) * 16 + hextointConvert(byteString1.Substring(3, 1));
            byteArray[1] = Convert.ToByte(hex1);

            int hex2 = hextointConvert(byteString2.Substring(2, 1)) * 16 + hextointConvert(byteString2.Substring(3, 1));
            byteArray[2] = Convert.ToByte(hex2);

            int hex3 = hextointConvert(byteString3.Substring(2, 1)) * 16 + hextointConvert(byteString3.Substring(3, 1));
            byteArray[3] = Convert.ToByte(hex3);

            portGwInstek.Close();
            portGwInstek.Open();
            portGwInstek.Write(byteArray, 0, 4);
            Thread.Sleep(750);
            portGwInstek.Close();
        }

        private void txtBeslemeVoltaj_Leave(object sender, EventArgs e)
        {
            try
            {
                if (txtBeslemeVoltaj.Text != "")
                {
                    GwInstekClearDataVoltaj();
                    GwInstekSendDataVoltaj(txtBeslemeVoltaj.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Power Supply tarafına Voltaj bilgisi iletilemedi!! " + ex, "Hata");
            }
        }

        private void txtTestBox_EnabledChanged(object sender, EventArgs e)
        {
            if (txtTestBox.Enabled == false)
            {
                txtTestBox.Text = "0";
                txtLedPort1.Text = null;
                txtLedPort2.Text = null;
                txtLedPort3.Text = null;
                txtLedPort4.Text = null;
                txtLedPort5.Text = null;
                txtLedPort6.Text = null;
                txtLedPort7.Text = null;
                txtLedPort8.Text = null;
                txtLedPort9.Text = null;
                txtLedPort10.Text = null;
                txtLedPort11.Text = null;
                txtLedPort12.Text = null;
                txtLedPort13.Text = null;
                txtLedPort14.Text = null;
                txtLedPort15.Text = null;

                for (int i = 1; i <= 15; i++)
                {
                    for (int j = 1; j <= 12; j++)
                    {
                        dgwFeasaSettings.Rows[j].Cells[i].Value = null;
                    }
                }
            }
        }

        private void txtBeslemeAkim_Leave(object sender, EventArgs e)
        {
            try
            {
                if (txtBeslemeAkim.Text != "")
                {
                    GwInstekClearDataAkim();
                    GwInstekSendDataAkim(txtBeslemeAkim.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Power Supply tarafına Akım bilgisi iletilemedi!! " + ex, "Hata");
            }
        }


        #endregion

        //*****Yeni model, Kayıtlı modellerin seçilmesi ve Verilerin hem PLC'ye hem de INI dosyası olarak kaydedilmesi*****
        #region Model Seç, Yeni Kayıt, Veri Gönder
        private void btnYeniModel_Click(object sender, EventArgs e)
        {
            txtModelName.Text = "";
            txtTestPcbBoy.Text = "";
            txtTestPcbaAdet.Text = "";
            txtTestKartPos.Text = "";
            txtBeslemeVoltaj.Text = "";
            txtBeslemeAkim.Text = "";
            cmbAkimVoltaj.SelectedIndex = 0;
            txtVoltajMIN.Text = "";
            txtVoltajMAX.Text = "";
            txtAkimMIN.Text = "";
            txtAkimMAX.Text = "";
            cmbLedOlcum.SelectedIndex = 0;
        }


        private void btnModelSec_Click(object sender, EventArgs e)
        {
            selectPath(txtModelName);
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            createModel(modelFilePath);

        }

        #endregion

        //*****Feasa Analyser bilgilerinin erişimi*****
        #region FEASA 

        public void FeasaReadRGB(int portNumber, int NumFibers)
        {
            int resp;
            string aux;
            int Sensor;
            StringBuilder buffer = new StringBuilder(100);

            //Open port
            //if (FeasaCom_Open(portNumber, "57600") == 1)
            if (FeasaCom.Open(portNumber, "AUTO") == 1)
            {

                resp = FeasaCom.Send(portNumber, "CAPTURE", buffer);
                if (resp == -1)
                {
                    MessageBox.Show("Feasa tarafına bilgiler iletetilemedi!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }
                else if (resp == 0)
                {
                    MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }

                for (Sensor = 1; Sensor <= NumFibers; Sensor++)
                {
                    //Send command to the LED Analyser
                    resp = FeasaCom.Send(portNumber, "GETRGBI" + Sensor.ToString("00"), buffer);
                    if (resp == -1)
                    {
                        MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }
                    else if (resp == 0)
                    {
                        MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }

                    aux = buffer.ToString();
                    string[] auxlist = aux.Split(' ');
                    if (rbtnRGB.Checked == true)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            dgwFeasaSettings.Rows[i].Cells[NumFibers].Value = Convert.ToString(int.Parse(auxlist[0])); //red
                        }
                        for (int i = 1; i < 2; i++)
                        {
                            dgwFeasaSettings.Rows[i].Cells[NumFibers].Value = Convert.ToString(int.Parse(auxlist[1])); //green
                        }
                        for (int i = 2; i < 3; i++)
                        {
                            dgwFeasaSettings.Rows[i].Cells[NumFibers].Value = Convert.ToString(int.Parse(auxlist[2])); //blue
                        }
                    }

                } //for

                //Close the port
                FeasaCom.Close(portNumber);
            }
            else
            {
                //Error: unable to open the selected port
                MessageBox.Show("Port Bağlantısı açılmadı!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                rbtnRGB.Checked = false;
            }
        }

        public void FeasaReadINT(int portNumber, int NumFibers)
        {
            int resp;
            string aux;
            int Sensor;
            StringBuilder buffer = new StringBuilder(100);

            //Open port
            //if (FeasaCom_Open(portNumber, "57600") == 1)
            if (FeasaCom.Open(portNumber, "AUTO") == 1)
            {

                resp = FeasaCom.Send(portNumber, "CAPTURE", buffer);
                if (resp == -1)
                {
                    MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }
                else if (resp == 0)
                {
                    MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }

                for (Sensor = 1; Sensor <= NumFibers; Sensor++)
                {
                    //Send command to the LED Analyser
                    resp = FeasaCom.Send(portNumber, "GETHSI" + Sensor.ToString("00"), buffer);
                    if (resp == -1)
                    {
                        MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }
                    else if (resp == 0)
                    {
                        MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }

                    aux = buffer.ToString();
                    string[] auxlist = aux.Split(' ');

                    for (int i = 1; i < 2; i++)
                    {
                        dgwFeasaSettings.Rows[9].Cells[NumFibers].Value = Convert.ToString(int.Parse(auxlist[2])); //intensity
                    }

                } //for

                //Close the port
                FeasaCom.Close(portNumber);
            }
            else
            {
                //Error: unable to open the selected port
                MessageBox.Show("Port Bağlantısı açılmadı!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                rbtnRGB.Checked = false;
            }
        }

        public void FeaseReadHue(int portNumber, int NumFibers)
        {
            int resp;
            string aux;
            int Sensor;
            StringBuilder buffer = new StringBuilder(100);

            //Open port
            //if (FeasaCom_Open(portNumber, "57600") == 1)
            if (FeasaCom.Open(portNumber, "AUTO") == 1)
            {

                resp = FeasaCom.Send(portNumber, "CAPTURE", buffer);
                if (resp == -1)
                {
                    MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }
                else if (resp == 0)
                {
                    MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }

                for (Sensor = 1; Sensor <= NumFibers; Sensor++)
                {
                    //Send command to the LED Analyser
                    resp = FeasaCom.Send(portNumber, "GEThsi" + Sensor.ToString("00"), buffer);
                    if (resp == -1)
                    {
                        MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }
                    else if (resp == 0)
                    {
                        MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }

                    aux = buffer.ToString();
                    string[] auxlist = aux.Split(' ');

                    for (int i = 0; i < 1; i++)
                    {
                        dgwFeasaSettings.Rows[8].Cells[NumFibers].Value = Convert.ToString((auxlist[0]));
                    }


                } //for

                //Close the port
                FeasaCom.Close(portNumber);
            }
            else
            {
                //Error: unable to open the selected port
                MessageBox.Show("Port Bağlantısı açılmadı!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                rbtnRGB.Checked = false;
            }
        }

        public void FeaseReadSat(int portNumber, int NumFibers)
        {

            int resp;
            string aux;
            int Sensor;
            StringBuilder buffer = new StringBuilder(100);

            //Open port
            //if (FeasaCom_Open(portNumber, "57600") == 1)
            if (FeasaCom.Open(portNumber, "AUTO") == 1)
            {

                resp = FeasaCom.Send(portNumber, "CAPTURE", buffer);
                if (resp == -1)
                {
                    MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }
                else if (resp == 0)
                {
                    MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }

                for (Sensor = 1; Sensor <= NumFibers; Sensor++)
                {
                    //Send command to the LED Analyser
                    resp = FeasaCom.Send(portNumber, "GETHSI" + Sensor.ToString("00"), buffer);
                    if (resp == -1)
                    {
                        MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }
                    else if (resp == 0)
                    {
                        MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }

                    aux = buffer.ToString();
                    string[] auxlist = aux.Split(' ');

                    for (int i = 0; i < 1; i++)
                    {
                        dgwFeasaSettings.Rows[7].Cells[NumFibers].Value = Convert.ToString(int.Parse(auxlist[1]));
                    }


                } //for

                //Close the port
                FeasaCom.Close(portNumber);
            }
            else
            {
                //Error: unable to open the selected port
                MessageBox.Show("Port Bağlantısı açılmadı!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                rbtnRGB.Checked = false;
            }
        }

        public void FeasaReadCIExy(int portNumber, int NumFibers)
        {
            int resp;
            string aux;
            int Sensor;
            StringBuilder buffer = new StringBuilder(100);

            //Open port
            if (FeasaCom.Open(portNumber, "AUTO") == 1)
            {
                resp = FeasaCom.Send(portNumber, "CAPTURE", buffer);
                if (resp == -1)
                {
                    MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }
                else if (resp == 0)
                {
                    MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }

                for (Sensor = 1; Sensor <= NumFibers; Sensor++)
                {
                    //Send command to the LED Analyser
                    resp = FeasaCom.Send(portNumber, "GETxy" + Sensor.ToString("00"), buffer);
                    if (resp == -1)
                    {
                        MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }
                    else if (resp == 0)
                    {
                        MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }

                    aux = buffer.ToString();
                    string[] auxlist = aux.Split(' ');

                    for (int i = 3; i < 4; i++)
                    {
                        dgwFeasaSettings.Rows[i].Cells[NumFibers].Value = Convert.ToString((auxlist[0])); //x
                    }
                    for (int i = 4; i < 5; i++)
                    {
                        dgwFeasaSettings.Rows[i].Cells[NumFibers].Value = Convert.ToString((auxlist[1])); //y
                    }

                } //for

                //Close the port
                FeasaCom.Close(portNumber);
            }
            else
            {
                //Error: unable to open the selected port
                MessageBox.Show("Port Bağlantısı açılmadı!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                rbtnRGB.Checked = false;
            }
        }

        public void FeasaReadCIEuv(int portNumber, int NumFibers)
        {
            int resp;
            string aux;
            int Sensor;
            StringBuilder buffer = new StringBuilder(100);

            //Open port
            //if (FeasaCom_Open(portNumber, "57600") == 1)
            if (FeasaCom.Open(portNumber, "AUTO") == 1)
            {
                resp = FeasaCom.Send(portNumber, "CAPTURE", buffer);
                if (resp == -1)
                {
                    MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }
                else if (resp == 0)
                {
                    MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }

                for (Sensor = 1; Sensor <= NumFibers; Sensor++)
                {
                    //Send command to the LED Analyser
                    resp = FeasaCom.Send(portNumber, "GETuv" + Sensor.ToString("00"), buffer);
                    if (resp == -1)
                    {
                        MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }
                    else if (resp == 0)
                    {
                        MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }

                    aux = buffer.ToString();
                    string[] auxlist = aux.Split(' ');

                    for (int i = 5; i < 6; i++)
                    {
                        dgwFeasaSettings.Rows[i].Cells[NumFibers].Value = Convert.ToString((auxlist[0])); //u
                    }
                    for (int i = 6; i < 7; i++)
                    {
                        dgwFeasaSettings.Rows[i].Cells[NumFibers].Value = Convert.ToString((auxlist[1])); //v
                    }

                } //for

                //Close the port
                FeasaCom.Close(portNumber);
            }
            else
            {
                //Error: unable to open the selected port
                MessageBox.Show("Port Bağlantısı açılmadı!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                rbtnRGB.Checked = false;
            }
        }

        public void FeasaReadCCT(int portNumber, int NumFibers)
        {
            int resp;
            string aux;
            int Sensor;
            StringBuilder buffer = new StringBuilder(100);

            //Open port
            if (FeasaCom.Open(portNumber, "AUTO") == 1)
            {
                resp = FeasaCom.Send(portNumber, "CAPTURE", buffer);
                if (resp == -1)
                {
                    MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }
                else if (resp == 0)
                {
                    MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }

                for (Sensor = 1; Sensor <= NumFibers; Sensor++)
                {
                    //Send command to the LED Analyser
                    resp = FeasaCom.Send(portNumber, "GETCCT" + Sensor.ToString("00"), buffer);
                    if (resp == -1)
                    {
                        MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }
                    else if (resp == 0)
                    {
                        MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }

                    aux = buffer.ToString();
                    string[] auxlist = aux.Split(' ');

                    for (int i = 0; i < 1; i++)
                    {
                        dgwFeasaSettings.Rows[10].Cells[NumFibers].Value = Convert.ToString((auxlist[0]));
                    }
                } //for

                //Close the port
                FeasaCom.Close(portNumber);
            }
            else
            {
                //Error: unable to open the selected port
                MessageBox.Show("Port Bağlantısı açılmadı!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                rbtnRGB.Checked = false;
            }
        }

        public void FeasaReadWaveLength(int portNumber, int NumFibers)
        {
            int resp;
            string aux;
            int Sensor;
            StringBuilder buffer = new StringBuilder(100);

            //Open port
            //if (FeasaCom_Open(portNumber, "57600") == 1)
            if (FeasaCom.Open(portNumber, "AUTO") == 1)
            {
                resp = FeasaCom.Send(portNumber, "CAPTURE", buffer);
                if (resp == -1)
                {
                    MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }
                else if (resp == 0)
                {
                    MessageBox.Show("easa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FeasaCom.Close(portNumber);
                    return;
                }

                for (Sensor = 1; Sensor <= NumFibers; Sensor++)
                {
                    //Send command to the LED Analyser
                    resp = FeasaCom.Send(portNumber, "GETWAVELENGTH" + Sensor.ToString("00"), buffer);
                    if (resp == -1)
                    {
                        MessageBox.Show("Hata! Feasa tarafına bilgiler iletetilemedi!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }
                    else if (resp == 0)
                    {
                        MessageBox.Show("Feasa tarafına bilgi gönderilemediğinden zaman aşımına uğradı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FeasaCom.Close(portNumber);
                        rbtnRGB.Checked = false;
                        return;
                    }

                    aux = buffer.ToString();
                    string[] auxlist = aux.Split(' ');

                    for (int i = 0; i < 1; i++)
                    {
                        dgwFeasaSettings.Rows[11].Cells[NumFibers].Value = Convert.ToString(int.Parse(auxlist[0]));
                    }

                } //for

                //Close the port
                FeasaCom.Close(portNumber);
            }
            else
            {
                //Error: unable to open the selected port
                MessageBox.Show("Port Bağlantısı açılmadı!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                rbtnRGB.Checked = false;
            }
        }

        public static string FormatDecimal(string Number)
        {
            bool DecimalFormatDot = false;

            //Set the decimal format
            float auxfloat = 0;
            if (float.TryParse("3.21", out auxfloat))
            {
                if (auxfloat == 3.21f)
                    DecimalFormatDot = true;
                else
                    DecimalFormatDot = false;
            }
            else
            {
                DecimalFormatDot = false;
            }

            if (DecimalFormatDot)
                return Number.Replace(',', '.');
            else
                return Number.Replace('.', ',');
        }

        #endregion



        
    }
}