using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public class LptManager
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool ReadFile(
    SafeFileHandle hFile,
    byte[] lpBuffer,
    uint nNumberOfBytesToRead,
    out uint lpNumberOfBytesRead,
    IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToWrite,
        out uint lpNumberOfBytesWritten,
        IntPtr lpOverlapped);

    public SafeFileHandle Handle { get; private set; }
    public byte[] ReadResponse(int length)
    {
        if (Handle == null || Handle.IsInvalid)
            return null;

        byte[] buffer = new byte[length];
        uint read;

        bool ok = ReadFile(Handle, buffer, (uint)length, out read, IntPtr.Zero);
        return ok ? buffer : null;
    }

    public bool Connect(string portName)
    {
        Handle = CreateFile(
            portName,
            0x40000000, // GENERIC_WRITE
            0,
            IntPtr.Zero,
            3,          // OPEN_EXISTING
            0,
            IntPtr.Zero);

        return !Handle.IsInvalid;
    }

    public void Disconnect()
    {
        if (Handle != null && !Handle.IsInvalid)
            Handle.Close();
    }

    public bool Send(byte[] data)
    {
        if (Handle == null || Handle.IsInvalid)
            return false;

        uint written;
        return WriteFile(Handle, data, (uint)data.Length, out written, IntPtr.Zero);
    }
}
