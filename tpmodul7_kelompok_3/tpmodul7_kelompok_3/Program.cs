using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TPModul7
{
    public class ProgramBacaJSON
    {
        public static void Main()
        {
            DataMahasiswa2211104006.ReadJSON();
            DataKelas.ReadJSON();
        }
    }

    // Kelas untuk menyimpan nama mahasiswa
    public class Nama
    {
        public string Depan { get; set; }
        public string Belakang { get; set; }
    }

    // Kelas untuk menyimpan data mahasiswa
    public class DataMahasiswa2211104006
    {
        public Nama Nama { get; set; }
        public long NIM { get; set; }
        public string Fakultas { get; set; }

        public static void ReadJSON()
        {
            string filePath = "tp7_1_2211104006.json"; // File mahasiswa

            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                var mahasiswa = JsonConvert.DeserializeObject<DataMahasiswa2211104006>(jsonContent);

                Console.WriteLine("=== Data Mahasiswa ===");
                Console.WriteLine($"Nama: {mahasiswa.Nama.Depan} {mahasiswa.Nama.Belakang}");
                Console.WriteLine($"NIM: {mahasiswa.NIM}");
                Console.WriteLine($"Fakultas: {mahasiswa.Fakultas}");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("File JSON mahasiswa tidak ditemukan!");
            }
        }
    }

    // Kelas untuk menyimpan informasi mata kuliah
    public class Course
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    // Kelas untuk menyimpan daftar mata kuliah
    public class DataKelas
    {
        public List<Course> Courses { get; set; }

        public static void ReadJSON()
        {
            string filePath = "tp7_2_2211104006.json"; // F

            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                var dataKelas = JsonConvert.DeserializeObject<DataKelas>(jsonContent);

                Console.WriteLine("=== Daftar Mata Kuliah ===");
                foreach (var course in dataKelas.Courses)
                {
                    Console.WriteLine($"Kode: {course.Code}, Nama: {course.Name}");
                }
            }
            else
            {
                Console.WriteLine("File JSON mata kuliah tidak ditemukan!");
            }
        }
    }
}
