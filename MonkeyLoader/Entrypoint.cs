using System;
using System.IO;

namespace Doorstop
{
    internal static class Entrypoint
    {
        public static void Start()
        {
            File.WriteAllText("doorstop_hello.log", "Hello from Unity!");
        }
    }
}