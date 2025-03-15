using System;
using System.IO;
using Newtonsoft.Json;

public class DataMahasiswa<NIM_PRAKTIKAN>
{
    public string Nama { get; set; }
    public string NIM { get; set; }
    public string Fakultas { get; set; }

    public static void ReadJSON()
    {
        string filePath = "tp7_1_<NIM_PRAKTIKAN>.json"; // Ganti dengan nama file yang sudah disimpan
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            var mahasiswa = JsonConvert.DeserializeObject<DataMahasiswa<NIM_PRAKTIKAN>>(jsonContent);

            Console.WriteLine($"Nama {mahasiswa.Nama} dengan NIM {mahasiswa.NIM} dari fakultas {mahasiswa.Fakultas}");
        }
        else
        {
            Console.WriteLine("File JSON tidak ditemukan!");
        }
    }
}
