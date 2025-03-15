using System;
using System.IO;
using Newtonsoft.Json;

namespace TPModul7
{
    public class programBacaJSON
    {
        public static void Main()
        {
            DataMahasiswa2211104006.ReadJSON();
        }
    }

    public class Nama
    {
        public string Depan { get; set; }
        public string Belakang { get; set; }
    }

    public class DataMahasiswa2211104006
    {
        public Nama Nama { get; set; }
        public long NIM { get; set; }
        public string Fakultas { get; set; }

        public static void ReadJSON()
        {
            string filePath = "tp7_1_2211104006.json"; // Sesuaikan dengan nama file JSON
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                var mahasiswa = JsonConvert.DeserializeObject<DataMahasiswa2211104006>(jsonContent);

                Console.WriteLine($"Nama {mahasiswa.Nama.Depan} {mahasiswa.Nama.Belakang} dengan NIM {mahasiswa.NIM} dari fakultas {mahasiswa.Fakultas}");
            }
            else
            {
                Console.WriteLine("File JSON tidak ditemukan!");
            }
        }
    }
}
