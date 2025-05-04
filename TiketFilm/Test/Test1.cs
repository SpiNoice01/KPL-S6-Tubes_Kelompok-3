using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace TiketFilmCore.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void TestMainDisplaysMenu()
        {
            using (var sw = new StringWriter())
            using (var sr = new StringReader("4\n")) // Simulasi input: pilih menu keluar
            {
                Console.SetOut(sw);
                Console.SetIn(sr);

                // Aktifkan mode testing agar tidak error saat Console.Clear() dan keluar otomatis
                TiketFilmCore.Program.IsTesting = true;

                // Jalankan menu
                TiketFilmCore.Program.RunMenuLoop();

                var output = sw.ToString();

                // Validasi bahwa output mengandung menu utama
                Assert.IsTrue(output.Contains("Sistem Pemesanan Tiket Bioskop"));
            }
        }
    }
}
