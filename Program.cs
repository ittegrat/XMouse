using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace XMouse
{

  internal static class Program
  {

    [Flags]
    enum Command
    {
      NONE = 0x00,
      TRACK = 0x01,
      ZORDER = 0x02,
      DELAY = 0x04,
      ALL = TRACK | ZORDER | DELAY,
      HELP = 0x80,
    }

    const uint defDelay = 200;

    static void Main(string[] args) {

      try {

        bool track = false;
        bool zorder = false;
        uint delay = 0;

        var cmd = Command.NONE;

        for (var i = 0; i < args.Length; ++i) {

          switch (args[i].Substring(0, 2)) {

            case "--":
              switch (args[i].Substring(2)) {
                case "help":
                  cmd = Command.HELP;
                  break;
                default:
                  throw new ArgumentException($"Invalid option: {args[i]}");
              }
              break;

            case "-d":
              ++i; delay = UInt32.Parse(args[i]);
              cmd |= Command.DELAY;
              break;

            case "-e":
              if (args[i].Length < 3) {
                track = true;
                zorder = false;
                delay = defDelay;
              } else {
                track = false;
                zorder = false;
                delay = 0;
              }
              cmd |= Command.ALL;
              break;

            case "-h":
              cmd = Command.HELP;
              break;

            case "-t":
              track = args[i].Length < 3;
              cmd |= Command.TRACK;
              break;

            case "-z":
              zorder = args[i].Length < 3;
              cmd |= Command.ZORDER;
              break;

            default:
              throw new ArgumentException($"Invalid option: {args[i]}");

          }

        }

        if (cmd.HasFlag(Command.HELP)) {
          Usage();
          return;
        }

        if (cmd.HasFlag(Command.TRACK))
          NativeWrappers.SetActiveWindowTracking(track);
        if (cmd.HasFlag(Command.ZORDER))
          NativeWrappers.SetActiveWndtrkZorder(zorder);
        if (cmd.HasFlag(Command.DELAY))
          NativeWrappers.SetActiveWndtrkTimeout(delay);

        Console.WriteLine($"Active Window Tracking: {NativeWrappers.GetActiveWindowTracking()}");
        Console.WriteLine($"Active Wndtrk Zorder:   {NativeWrappers.GetActiveWndtrkZorder()}");
        Console.WriteLine($"Active Wndtrk Timeout:  {NativeWrappers.GetActiveWndtrkTimeout()} ms");

      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
    }
    static void Usage() {
      var name = System.IO.Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
      Console.Write(@"
Usage: {0} [options...]
Configure active window tracking.
If no options are specified, displays the current configuration.

Options (add 'x' to disable):
  -e[x]: enable xmouse behavior (tracking on, no raise, {1} msec delay)
  -t[x]: enable active window tracking
  -z[x]: enable active window tracking zorder
  -d MSEC: set active window tracking delay
  -h, --help: print this help

", name, defDelay);

    }

  }

  internal static class NativeWrappers
  {

    enum SPI : uint
    {
      GetActiveWindowTracking = 0x1000,
      SetActiveWindowTracking = 0x1001,
      GetActiveWndtrkZorder = 0x100C,
      SetActiveWndtrkZorder = 0x100D,
      GetActiveWndtrkTimeout = 0x2002,
      SetActiveWndtrkTimeout = 0x2003,
    }

    [Flags]
    enum SPIF : uint
    {
      None = 0x00,
      UpdateIniFile = 0x01,
      SendChange = 0x02,
      UpdateAndSend = UpdateIniFile | SendChange,
    }

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, IntPtr pvParam, SPIF fWinIni);

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref IntPtr pvParam, SPIF fWinIni);

    public static bool GetActiveWindowTracking() {
      var pvp = IntPtr.Zero;
      var ans = SystemParametersInfo(SPI.GetActiveWindowTracking, 0, ref pvp, SPIF.None);
      if (!ans)
        throw new Win32Exception();
      return pvp != IntPtr.Zero;
    }
    public static bool GetActiveWndtrkZorder() {
      var pvp = IntPtr.Zero;
      var ans = SystemParametersInfo(SPI.GetActiveWndtrkZorder, 0, ref pvp, SPIF.None);
      if (!ans)
        throw new Win32Exception();
      return pvp != IntPtr.Zero;
    }
    public static uint GetActiveWndtrkTimeout() {
      var pvp = IntPtr.Zero;
      var ans = SystemParametersInfo(SPI.GetActiveWndtrkTimeout, 0, ref pvp, SPIF.None);
      if (!ans)
        throw new Win32Exception();
      return (uint)pvp;
    }

    public static void SetActiveWindowTracking(bool enabled) {
      var pvp = enabled ? new IntPtr(1) : IntPtr.Zero;
      var ans = SystemParametersInfo(SPI.SetActiveWindowTracking, 0, pvp, SPIF.UpdateAndSend);
      if (!ans)
        throw new Win32Exception();
    }
    public static void SetActiveWndtrkZorder(bool enabled) {
      var pvp = enabled ? new IntPtr(1) : IntPtr.Zero;
      var ans = SystemParametersInfo(SPI.SetActiveWndtrkZorder, 0, pvp, SPIF.UpdateAndSend);
      if (!ans)
        throw new Win32Exception();
    }
    public static void SetActiveWndtrkTimeout(uint delay) {
      var pvp = new IntPtr(delay);
      var ans = SystemParametersInfo(SPI.SetActiveWndtrkTimeout, 0, pvp, SPIF.UpdateAndSend);
      if (!ans)
        throw new Win32Exception();
    }

  }


}
