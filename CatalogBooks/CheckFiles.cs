using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Threading.Tasks;

namespace CatalogBooks
{
    /// <summary>
    /// Класс для поиска файлов
    /// </summary>
    public class CheckFiles
    {
        /// <summary>
        /// Расширения искомых файлов
        /// </summary>
        private string Filter { get; set; }

        /// <summary>
        /// Список найденных файлов
        /// </summary>
        private List<Book> DetectedFiles { get; set; }

        /// <summary>
        /// Объект для работы с базой данных
        /// </summary>
        private BaseContext db;

        /// <summary>
        /// Констуктор класса
        /// </summary>
        /// <param name="filter">Расширения искомых файлов</param>
        public CheckFiles(string filter)
        {
            Filter = filter;
            db = new BaseContext();
            db.Books.Load();
        }

        /// <summary>
        /// Получение MD5 суммы файла
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Путь к файлу</returns>
        private Task<string> GetMd5Sum(string path)
        {
            return Task.Run(() =>
            {
                return BitConverter.ToString(MD5.Create().ComputeHash(File.ReadAllBytes(path))).Replace("-", "");
            });            
        }

        /// <summary>
        /// Поиск файлов
        /// </summary>
        /// <param name="path">Путь к папке</param>
        private async Task CheckDirectory(string path)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                string pathFile = Path.Combine(path, file);
                string md5 = await GetMd5Sum(pathFile);
                string ext = Path.GetExtension(pathFile);

                if (!db.Books.Any(book => book.MD5 == md5) &&
                    Filter.Split('|').Any(x => x == ext))
                    DetectedFiles.Add(new Book() {Path = pathFile, MD5 = md5});
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                string pathDirectory = Path.Combine(path, directory);
                if (Path.GetExtension(pathDirectory) == "")
                    await CheckDirectory(pathDirectory);
            }
        }

        /// <summary>
        /// Запуск поиска файлов
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Путь к папке</returns>
        public async Task<List<Book>> Check(string path)
        {
            DetectedFiles = new List<Book>();  
            await CheckDirectory(path);
            return DetectedFiles;
        }
    }
}
