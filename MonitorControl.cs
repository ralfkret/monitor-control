using System;
using System.Runtime.InteropServices;

public class MonitorControl
{
  private static Action<string> Log = Console.WriteLine;
  private static Action<string> Debug = (_) => { };
  private static Action<string> Info = Console.WriteLine;
  private const int BVCPCodeSource = 0x60;
  private static string sourceSetting;

  [StructLayout(LayoutKind.Sequential)]
  public struct RECT
  {
    public int left;
    public int top;
    public int right;
    public int bottom;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct PHYSICAL_MONITOR
  {
    public IntPtr hPhysicalMonitor;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = 128)]
    public char[] szPhysicalMonitorDescription;

    public string MonitorDescription
    {
      get { return new string(szPhysicalMonitorDescription).TrimEnd('\0'); }
    }
  }


  [DllImport("user32.dll")]
  static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

  [DllImport("dxva2.dll", SetLastError = true)]
  static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

  [DllImport("dxva2.dll", SetLastError = true)]
  static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

  [DllImport("dxva2.dll", SetLastError = true)]
  static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

  [DllImport("dxva2.dll", SetLastError = true)]
  static extern bool SetVCPFeature(IntPtr hMonitor, byte bVCPCode, uint dwNewValue);

  [DllImport("dxva2.dll", SetLastError = true)]
  static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr hMonitor, byte bVCPCode, IntPtr pvct, out uint pdwCurrentValue, out uint pdwMaximumValue);

  delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

  static bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
  {
    uint physicalMonitorCount;
    if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out physicalMonitorCount))
    {
      Info("Failed to get monitor count");
      return true;
    }

    var monitors = new PHYSICAL_MONITOR[physicalMonitorCount];
    if (GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitorCount, monitors))
    {
      foreach (var monitor in monitors)
      {
        Debug($"Monitor: {monitor.MonitorDescription}");
        if (!string.IsNullOrEmpty(sourceSetting))
        {
          Debug($"NewSetting: {sourceSetting}");
        }
        else
        {
          Debug("No new setting");
        }
        uint currentValue, maximumValue;
        if (GetVCPFeatureAndVCPFeatureReply(monitor.hPhysicalMonitor, BVCPCodeSource, IntPtr.Zero, out currentValue, out maximumValue))
        {
          Log($"Current source is {currentValue:x4} on monitor: {monitor.MonitorDescription}");
          if (!string.IsNullOrEmpty(sourceSetting))
          {
            Log($"Setting source to {sourceSetting:x04}");
            var sourceValue = uint.Parse(sourceSetting, System.Globalization.NumberStyles.HexNumber);
            if (!SetVCPFeature(monitor.hPhysicalMonitor, BVCPCodeSource, sourceValue))
            {
              Info("Failed to set VCP feature");
            }
          }
        }
        else
        {
          Debug("Failed to read VCP feature");
        }

        if (!DestroyPhysicalMonitors(1, new[] { monitor }))
        {
          Debug("Failed to close monitor handle");
        }
      }
    }
    else
    {
      Debug("Failed to get physical monitors from HMONITOR");
    }

    return true;
  }

  public static void Main(string args)
  {
    sourceSetting = args;
    if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnum, IntPtr.Zero))
    {
      Debug("Failed to enumerate monitors");
    }
  }
}