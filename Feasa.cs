/********************************************************
*  Copyright 2020 Feasa Enterprises Ltd
*  Feasa Communications Library
********************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FeasaLib
{
    class FeasaCom 
    {
        private const string DLL_FEASACOM = "feasacom.dll"; // Note: feasacom64.dll file has to be used for 64bit targets and feasacom.dll for 32bit targets

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetDLLVersion")]
        public static extern void GetDLLVersion(StringBuilder Version);

        // Basic Comm functions
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Open")]
        public static extern int Open(int CommPort, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Send")]
        public static extern int Send(int CommPort, string Command, StringBuilder ResponseText);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Close")]
        public static extern int Close(int CommPort);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_OpenSN")]
        public static extern int OpenSN(string SerialNumber, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SendSN")]
        public static extern int SendSN(string SerialNumber, string Command, StringBuilder ResponseText);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_CloseSN")]
        public static extern int CloseSN(string SerialNumber);


        // Comm helper functions
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SendToAll")]
        public static extern int SendToAll(int[] ReturnValues, string Command, [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] Responses);
	
		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SendToAll_NR")]
		public static extern int SendToAll_NR(int[] ReturnValues, string Command);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Open_Multi")]
        public static extern int Open_Multi(int[] ReturnValues, int[] CommPorts, int nPorts, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Close_Multi")]
        public static extern int Close_Multi(int[] ReturnValues, int[] CommPorts, int nPorts);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Send_Multi")]
        public static extern int Send_Multi(int[] ReturnValues, int[] CommPorts, int nPorts, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] Commands, [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] Responses);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Send_Multi_NR")]
		public static extern int Send_Multi_NR(int[] ReturnValues, int[] CommPorts, int nPorts, string Commands, char CommandSeparator);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_OpenSN_Multi")]
        public static extern int OpenSN_Multi(int[] ReturnValues, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] SerialNumbers, int nSerials, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_CloseSN_Multi")]
        public static extern int CloseSN_Multi(int[] ReturnValues, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] SerialNumbers, int nSerials);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SendSN_Multi")]
        public static extern int SendSN_Multi(int[] ReturnValues, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] SerialNumbers, int nSerials, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] Commands, [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] Responses);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_CloseAll")]
        public static extern int CloseAll();

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetResponseByPort")]
		public static extern int GetResponseByPort(int CommPort, StringBuilder ResponseText);
		
		
		// Test functions
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Capture")]
        public static extern int Capture(int CommPort, int isPWM, int CaptureRange, int CapturePWMFrames);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_CaptureFromAll")]
        public static extern int CaptureFromAll(int[] ReturnValues, int isPWM, int CaptureRange, int CapturePWMFrames);
	
		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SpectrometerCapture")]
		public static extern int SpectrometerCapture(int CommPort, int isPWM, int UseCustomExposure, float ExposureTime);
	
		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SpectrometerDark")]
		public static extern int SpectrometerDark(int CommPort, int isPWM, int UseCustomExposure, float ExposureTime);
		
		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_CaptureFromAllSpectrometers")]
		public static extern int CaptureFromAllSpectrometers(int[] ReturnValues, int isPWM, int UseCustomExposure, float ExposureTime);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_Setup")]
		public static extern int Sequence_Setup(int CommPort, int StartDelay, int CaptureTime, int TimeBetweenCaptures, int SampleCount, int toFlash);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_Capture")]
		public static extern int Sequence_Capture(int CommPort, int Fiber);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_ReadIntensity")]
		public static extern int Sequence_ReadIntensity(int CommPort, int Fiber, int[] IntensityValues);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_ReadxyI")]
		public static extern int Sequence_ReadxyI(int CommPort, int Fiber, float[] xValues, float[] yValues, int[] IntensityValues);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_ReadHSI")]
		public static extern int Sequence_ReadHSI(int CommPort, int Fiber, float[] HueValues, int[] SaturationValues, int[] IntensityValues);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_ReadRGBI")]
		public static extern int Sequence_ReadRGBI(int CommPort, int Fiber, byte[] RedValues, byte[] GreenValues, byte[] BlueValues, int[] IntensityValues);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_ReadCCT")]
		public static extern int Sequence_ReadCCT(int CommPort, int Fiber, int[] CCTValues, float[] deltauvValues);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_ReadWavelength")]
		public static extern int Sequence_ReadWavelength(int CommPort, int Fiber, int[] WavelengthValues);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_GetPattern")]
		public static extern int Sequence_GetPattern(int CommPort, int[] IntensityValues, ref int StatusCount, int[] PatternTimes, int[] PatternIntensities);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_GetSweepingPattern")]
		public static extern int Sequence_GetSweepingPattern(int CommPort, int LEDCount, int isOffToOn, int[] LowTimes, int[] HighTimes, int[] IntensityValues);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_GetFrequency")]
		public static extern int Sequence_GetFrequency(int CommPort, int[] IntensityValues, ref float Frequency, ref float DC, ref int CycleCount);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_FindTestSettings")]
		public static extern int Sequence_FindTestSettings(int CommPort, int TotalLEDCount, int FiberToTest, int SignalSpeed, int BlinkingSpeed, int MinCycleCount, int TimeResolutionIsImportant, ref int CaptureTime, ref int WaitTime, ref int SampleCount);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_SetPatternThresholdHigh")]
		public static extern int Sequence_SetPatternThresholdHigh(int CommPort, int Intensity);
		
		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Sequence_SetPatternThresholdLow")]
		public static extern int Sequence_SetPatternThresholdLow(int CommPort, int Intensity);


        // Daisy-chain functions
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_DaisyChain_Add")]
        public static extern int DaisyChain_Add(int CommPort, string SerialNumber);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_DaisyChain_Del")]
        public static extern int DaisyChain_Del(int CommPort, string SerialNumber);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_DaisyChain_Clear")]
        public static extern int DaisyChain_Clear(int CommPort);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_DaisyChain_Send")]
		public static extern int DaisyChain_Send(int CommPort, string SerialNumber, string Command, StringBuilder ResponseText);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_DaisyChain_Capture")]
        public static extern int DaisyChain_Capture(int CommPort, int isPWM, int CaptureRange, int CapturePWMFrames);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_DaisyChain_SpectrometerCapture")]
		public static extern int DaisyChain_SpectrometerCapture(int CommPort, int isPWM, int UsePresetExposure, float ExposureTime);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_DaisyChain_SpectrometerDark")]
		public static extern int DaisyChain_SpectrometerDark(int CommPort, int isPWM, int UsePresetExposure, float ExposureTime);


        // External Trigger functions
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_ExternalTrigger_Listen")]
        public static extern int ExternalTrigger_Listen(int CommPort);
		
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_ExternalTrigger_Abort")]
        public static extern int ExternalTrigger_Abort(int CommPort);
		
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_ExternalTrigger_isFinished")]
        public static extern int ExternalTrigger_isFinished(int CommPort);


        // Comm handling functions
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_EnumPorts")]
        public static extern int EnumPorts();
	
		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_EnumPorts_Filter")]
		public static extern void EnumPorts_Filter(int USB, int RS232, int Bluetooth);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_IsConnected")]
        public static extern int IsConnected(string SerialNumber, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_AreConnected")]
        public static extern int AreConnected(int[] PortNumbers, [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] SerialNumbers, int nSerials, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_AreConnectedS")]
        public static extern int AreConnectedS(int[] PortNumbers, string SerialNumbers, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Detect")]
        public static extern int Detect(int[] CommPorts, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_DetectSN")]
        public static extern int DetectSN([In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] SerialNumbers, string Baudrate);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_AddDetectionFilter")]
        public static extern void AddDetectionFilter(string Filter);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_ClearDetectionFilters")]
        public static extern void ClearDetectionFilters();

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_IsPortAvailable")]
        public static extern int IsPortAvailable(int CommPort);

        //FeasaCom.IsPortAvailable(i)

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_ListPortsDetected")]
        public static extern int ListPortsDetected(int[] ListOfPortsDetected);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_ListPortsDetectedTxt")]
        public static extern int ListPortsDetectedTxt(StringBuilder ListOfPortsDetected, string Delimiter);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SetResponseTimeout")]
        public static extern int SetResponseTimeout(uint Timeout);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SetResponseTimeoutAuto")]
		public static extern int SetResponseTimeoutAuto(int CommPort, int Status);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetBaudrate")]
        public static extern int GetBaudrate(int CommPort);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetDeviceType")]
		public static extern int GetDeviceType(int CommPort, StringBuilder DeviceType);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetError_Description")]
        public static extern void GetError_Description(StringBuilder ErrorDescription);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetError_DescriptionByPort")]
        public static extern void GetError_DescriptionByPort(int CommPort, StringBuilder ErrorDescription);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetError_DescriptionBySN")]
        public static extern void GetError_DescriptionBySN(string SerialNumber, StringBuilder ErrorDescription);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetPortBySN")]
        public static extern int GetPortBySN(string SerialNumber);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetSNByPort")]
        public static extern int GetSNByPort(StringBuilder SerialNumber, int CommPort);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetPortByID")]
        public static extern int GetPortByID(string DeviceID);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetOpenedPorts")]
		public static extern int GetOpenedPorts(int[] CommPorts);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_GetOpenedPortsS")]
		public static extern int GetOpenedPortsS(StringBuilder CommPortsTxt, char Delimiter);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_OpenProject")]
		public static extern int OpenProject(string Path);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_CloseProject")]
		public static extern int CloseProject();

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_SendByID")]
		public static extern int SendByID(string DeviceID, string Command, StringBuilder ResponseText);


        // Binning
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_Binning_GetBinFromVECFile")]
        public static extern int Binning_GetBinFromVECFile(string Path, float x, float y, StringBuilder ResultBinName);


        // UserCal functions
        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_ResetIntensity")]
        public static extern int UserCal_ResetIntensity(int CommPort, int Fiber, int toFlash);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_GetIntensityGain")]
        public static extern int UserCal_GetIntensityGain(int CommPort, int Fiber, ref int Gain);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_SetIntensityGain")]
        public static extern int UserCal_SetIntensityGain(int CommPort, int Fiber, int Gain, int toFlash);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_AdjustIntensity")]
        public static extern int UserCal_AdjustIntensity(int CommPort, int Fiber, int IntensityRef, int isPWM, int CaptureRange, int toFlash);


        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_ResetxyOffsets")]
        public static extern int UserCal_ResetxyOffsets(int CommPort, int Fiber, int toFlash);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_GetxyOffsets")]
        public static extern int UserCal_GetxyOffsets(int CommPort, int Fiber, ref float xOffset, ref float yOffset);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_SetxyOffsets")]
        public static extern int UserCal_SetxyOffsets(int CommPort, int Fiber, float xOffset, float yOffset, int toFlash);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_AdjustxyOffsets")]
        public static extern int UserCal_AdjustxyOffsets(int CommPort, int Fiber, float xRef, float yRef, int ToFlash);


        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_ResetWavelengthOffset")]
        public static extern int UserCal_ResetWavelengthOffset(int CommPort, int Fiber, int toFlash);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_GetWavelengthOffset")]
        public static extern int UserCal_GetWavelengthOffset(int CommPort, int Fiber, ref int WavelengthOffset);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_SetWavelengthOffset")]
        public static extern int UserCal_SetWavelengthOffset(int CommPort, int Fiber, int WavelengthOffset, int toFlash);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_AdjustWavelengthOffset")]
        public static extern int UserCal_AdjustWavelengthOffset(int CommPort, int Fiber, int WavelengthRef, int toFlash);


        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_ResetAbsInt")]
        public static extern int UserCal_ResetAbsInt(int CommPort, int Fiber, int toFlash);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_GetAbsIntFactor")]
        public static extern int UserCal_GetAbsIntFactor(int CommPort, int Fiber, ref double Factor);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_SetAbsIntFactor")]
        public static extern int UserCal_SetAbsIntFactor(int CommPort, int Fiber, double Factor, int toFlash);

        [DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_AdjustAbsInt")]
        public static extern int UserCal_AdjustAbsInt(int CommPort, int Fiber, double AbsIntRef, int toFlash);


		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_ResetRGBAdj")]
		public static extern int UserCal_ResetRGBAdj(int CommPort, int Fiber);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_TakeRGBCurrentValues")]
		public static extern int UserCal_TakeRGBCurrentValues(int CommPort, int Fiber, char Color);

		[DllImport(DLL_FEASACOM, EntryPoint = "FeasaCom_UserCal_AdjustRGB")]
		public static extern int UserCal_AdjustRGB(int CommPort, int Fiber, float xRefRed, float yRefRed, double AbsIntRefRed, float xRefGreen, float yRefGreen, double AbsIntRefGreen, float xRefBlue, float yRefBlue, double AbsIntRefBlue);
    }

    static class FeasaTools
    {
        public const string DLL_FEASATOOLS = "feasa_tools.dll"; // Note: feasa_tools64.dll file has to be used for 64bit targets and feasa_tools.dll for 32bit targets


        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_Int16")]
        public static extern Int16 Parse_Int16(string StringToParse, byte Parameter);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_Int32")]
        public static extern Int32 Parse_Int32(string StringToParse, byte Parameter);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_Float")]
        public static extern float Parse_Float(string StringToParse, byte Parameter);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_RGBI")]
        public static extern int Parse_RGBI(string Response, ref byte Red, ref byte Green, ref byte Blue, ref int Intensity);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_RGBI_All")]
        public static extern int Parse_RGBI_All(string Response, byte[] RedValues, byte[] GreenValues, byte[] BlueValues, int[] IntensityValues);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_HSI")]
        public static extern int Parse_HSI(string Response, ref float Hue, ref int Saturation, ref int Intensity);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_HSI_All")]
        public static extern int Parse_HSI_All(string Response, float[] HueValues, int[] SaturationValues, int[] IntensityValues);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_xy")]
        public static extern int Parse_xy(string Response, ref float x, ref float y);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_xy_All")]
        public static extern int Parse_xy_All(string Response, float[] xValues, float[] yValues);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_uv")]
        public static extern int Parse_uv(string Response, ref float u, ref float v);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_uv_All")]
        public static extern int Parse_uv_All(string Response, float[] uValues, float[] vValues);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_CCT")]
        public static extern int Parse_CCT(string Response, ref int CCT, ref float delta);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_CCT_All")]
        public static extern int Parse_CCT_All(string Response, int[] CCTValues, float[] deltaValues);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_Wavelength")]
        public static extern int Parse_Wavelength(string Response, ref float Wavelength);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_Wavelength_All")]
        public static extern int Parse_Wavelength_All(string Response, float[] WavelengthValues);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_WI")]
        public static extern int Parse_WI(string Response, ref float Wavelength, ref int Intensity);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_WI_All")]
        public static extern int Parse_WI_All(string Response, float[] WavelengthValues, int[] IntensityValues);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_Intensity")]
        public static extern int Parse_Intensity(string Response, ref int Intensity);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_Intensity_All")]
        public static extern int Parse_Intensity_All(string Response, int[] IntensityValues);

        [DllImport(DLL_FEASATOOLS, EntryPoint = "Feasa_Parse_Spectrum")]
        public static extern int Parse_Spectrum(string Response, float[] WavelengthValues, double[] IntensityValues);

        public static void InitializeArrayOfStrings(ref string[] mArray, int StringSize)
        {
            for (int i = 0; i < mArray.Length; i++)
                mArray[i] = new string('\0', StringSize);
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
    }
}
