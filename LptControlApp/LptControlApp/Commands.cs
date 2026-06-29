using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Commands
{
    public static byte[] SOF = { 0xAA };
    public static byte[] CMD = { 0xC1 };
    public static byte[] DATA = { 0x55, 0x66, 0x77 };
    public static byte[] EOF = { 0xFF };
}
