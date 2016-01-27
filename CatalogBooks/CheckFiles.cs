using System;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Threading.Tasks;
using System.Reactive.Subjects;

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
        private string Filter { get; }
        /// <summary>
        /// Наблюдатель 
        /// </summary>
        private readonly Subject<Book> _subject;
        /// <summary>
        /// Обеспечивает доступ к наблюдателю
        /// </summary>
        public IObservable<Book> NewBook { get { return _subject; } } 

        /// <summary>
        /// Констуктор класса
        /// </summary>
        /// <param name="filter">Расширения искомых файлов</param>
        public CheckFiles(string filter)
        {
            Filter = filter;
            _subject = new Subject<Book>();
        }

        /// <summary>
        /// Получение MD5 суммы файла
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Путь к файлу</returns>
        private Task<string> GetMd5Sum(string path)
        {
            return Task.Run(() => BitConverter.ToString(MD5.Create().ComputeHash(File.ReadAllBytes(path))).Replace("-", ""));            
        }

        /// <summary>
        /// Поиск файлов
        /// </summary>
        /// <param name="path">Путь к папке</param>
        private async Task CheckFolderAsync(string path)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                string pathFile = Path.Combine(path, file);                
                string ext = Path.GetExtension(pathFile);                
                if (Filter.Split('|').Any(x => x == ext))
                {
                    string md5 = await GetMd5Sum(pathFile);
                    _subject.OnNext(new Book() { Path = pathFile, MD5 = md5 });                    
                }
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                string pathDirectory = Path.Combine(path, directory);
                if (Path.GetExtension(pathDirectory) == "")
                    await CheckFolderAsync(pathDirectory);
            }
        }

        /// <summary>
        /// Сканирование директорий
        /// </summary>
        /// <param name="folders">Список директорий</param>
        public async Task CheckForldersAsync(string[] folders)
        {
            foreach (string folder in folders)
                await CheckFolderAsync(folder);
            _subject.OnCompleted();
        }
    }
}
