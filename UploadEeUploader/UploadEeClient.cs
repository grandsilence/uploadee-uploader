using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace UploadEeUploader
{
    public class UploadEeClient : IDisposable
    {
        private readonly HttpClient _http;

        // Время используется как один из параметров запроса.
        public static ulong MillisecondsFrom1970 => (ulong) (DateTime.UtcNow - FirstJanuary1970).TotalMilliseconds;
        public static DateTime FirstJanuary1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        public UploadEeClient()
        {
            var handler = new HttpClientHandler {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                // Отладка запросов через Charles (платный) или Fiddler (бесплатный)
                #if CHARLES
                Proxy = new WebProxy("http://127.0.0.1:8888")
                #endif
            };

            _http = new HttpClient(handler) {
                // Все запросы будут относительно этого адреса
                BaseAddress = new Uri("https://www.upload.ee/"),
                // Максимальное время запроса 10 секунд - после будет исключение что нет соединения.
                Timeout = TimeSpan.FromSeconds(10)
            };

            
            // Пытаемся выдать себя за реальный браузер
            _http.DefaultRequestHeaders.ExpectContinue = false;
            _http.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3583.0 Safari/537.36");
            _http.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, br");
            _http.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("en-US,en;q=0.5");
            _http.DefaultRequestHeaders.Referrer = new Uri("https://www.upload.ee");
            // _http.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.upload.ee");
        }

        /// <summary>
        /// Загружает файл на Upload.ee.
        /// </summary>
        /// <param name="filePath">Путь к файлу который следует загрузить.</param>
        /// <exception cref="ArgumentException">Когда указан неверный путь к файлу.</exception>
        /// <exception cref="FileNotFoundException">Когда файл не существует.</exception>
        /// <exception cref="UploadEeException">Когда не удалось загрузить файл из-за ошибки алгоритма загрузки</exception>
        /// <returns>Вернет URL на загруженный файл</returns>
        public string UploadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Следует указать путь к загружаемому файлу", filePath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл для загрузки не найден", filePath);

            // Резервируем ID загрузки
            // Иными словами если результат == null, т.е. ID не был получен - бросаем исключение.
            string uploadId = GetNewUploadId()
                ?? throw new UploadEeException("Не был получен ID для новой загрузки");

            // Загружаем сам файл на сервер
            return UploadAndGetUrl(filePath, uploadId);
        }

        private string GetNewUploadId()
        {
            string resp = Get("/ubr_link_upload.php?rnd_id=" + MillisecondsFrom1970);
            // Получаем ID между двумя строками. Этот метод находится в StringExtensions.
            // Т.е. берем все что между startUpload(" и ",
            // Этот вариант быстрее примерно раз 5-20 чем RegEx (зависит от регулярки).
            return resp.Between("startUpload(\"", "\",");
        }

        private string UploadAndGetUrl(string filePath, string uploadId)
        {
            const string finishUrl = "/?page=finished&upload_id=";
            string uploadUrl = string.Format("/cgi-bin/ubr_upload.pl?X-Progress-ID={0}&upload_id={0}", uploadId);
            FileStream file = null;
            MultipartFormDataContent content = null;

            try
            {
                file = File.OpenRead(filePath);
                var fileContent = new StreamContent(file);

                // Тоже ХАК для upload.ee (привет индусам-программистам) - без него сервер выдаст исключение
                fileContent.Headers.Add("Content-Disposition", "form-data; name=\"upfile_0\"; filename=\"" + Path.GetFileName(filePath) + "\"");
                // fileContent.Headers.Add("Context-Type", "text/plain");

                content = new MultipartFormDataContent("----WebKitFormBoundaryRcbpCXRhM2idX4Yq") {
                    { fileContent, "upfile_0", Path.GetFileName(filePath)},
                    // Внимание: имя поля с кавычками не просто так. Без них сервер почему-то кидает ошибку XML парсинга (кто-то не умеет писать код).
                    { new StringContent(""), "\"link\"" },
                    { new StringContent(""), "\"email\"" },
                    { new StringContent("cat_file"), "\"category\"" },
                    { new StringContent("none"), "\"big_resize\"" },
                    { new StringContent("120x90"), "\"small_resize\"" }
                };


                string html = Post(uploadUrl, content);
                if (!html.Contains(finishUrl))
                    throw new UploadEeException("Не удалось загрузить файл по неизвестной причине");

                        // _http.DefaultRequestHeaders.Referrer = new Uri("https://www.upload.ee" + uploadUrl); 
                html = Get(finishUrl + uploadId);


                // Получаем результирующую ссылку
                return html.Between("Файл можно увидеть здесь:<br /><a href=\"", "\">")
                    ?? throw new UploadEeException("Не найдена ссылка на загруженный файл");
            }
            finally
            {
                // Закрываем файл если он был открыт, освобождаем память.
                file?.Dispose();
                content?.Dispose();
            }
        }

        /// <summary>
        /// Делает GET запрос по указанному относительному URL пути ресурса.
        /// </summary>
        /// <param name="relativeUrl">Относительный URL ресурса</param>
        /// <returns>Вернет ответ от сервера в качестве строки</returns>
        private string Get(string relativeUrl)
        {
            return Result(_http.GetAsync(relativeUrl));
        }

        /// <summary>
        /// Делает POST запрос по указанному относительному URL пути ресурса.
        /// </summary>
        /// <param name="relativeUrl">Относительный URL ресурса</param>
        /// <param name="content">Дан</param>
        /// <returns>Вернет ответ от сервера в качестве строки</returns>
        private string Post(string relativeUrl, HttpContent content)
        {
            return Result(_http.PostAsync(relativeUrl, content));
        }

        /// <summary>
        /// Синхронное чтение ответа.
        /// </summary>
        /// <param name="task">Задача с HTTP ответом содержимое которой нужно прочитать.</param>
        /// <returns>Ответ от сервера в виде строки</returns>
        private static string Result(Task<HttpResponseMessage> task)
        {
            // оптимизация, т.к. не используем async await
            task.ConfigureAwait(false);

            var taskResult = task.Result;
            taskResult.EnsureSuccessStatusCode();

            var resp = taskResult.Content.ReadAsStringAsync();
            resp.ConfigureAwait(false);
            return resp.Result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _http?.Dispose();
        }
    }
}
