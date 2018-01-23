using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Policy;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.ComponentModel;
using Microsoft.VisualBasic;
using System.Linq;

namespace sw_EnligateWeb.Engine
{
    public class Global_Functions
    {
        public string strError = "";
        public static string applicationPath;
        private static Random random = new Random((int)DateTime.Now.Ticks);
        public static DateTime startPointDate = new DateTime(2010, 1, 1, 0, 0, 00);

        static Global_Functions()
        {
            Global_Functions.applicationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/");
        }

        # region Methods

        /// <summary>
        /// Generate and return random code.
        /// </summary>
        /// <returns></returns>
        public static string generateCode()
        {
            return getRandomString(13) + TimeSpan.FromTicks((int)DateTime.Now.Ticks).ToString();
        }

        /// <summary>
        /// Return the body of a html file
        /// </summary>
        /// <param name="filePath"> ej: "~/emails/PassengerNamesTemplate.htm"</param>
        /// <returns></returns>
        public static string getBodyHTML(string filePath)
        {
            string body = "";
            try
            {
                StreamReader fil = File.OpenText(HttpContext.Current.Server.MapPath(filePath));
                body = fil.ReadToEnd();
                fil.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return body;
        }

        /// <summary>
        /// Method that save the error massage from the user context
        ///</summary>
        /// <param name="exception">Error massage</param>
        /// <param name="redirect">True if the page is to be redirected</param>
        public static void saveErrors(String exception, Boolean redirect)
        {
            String filename = HttpContext.Current.Server.MapPath("~/_Logs/errors.txt");
            setError(exception, filename, redirect);
        }
        public static void saveReport(String exception)
        {           
            String filename = HttpContext.Current.Server.MapPath("~/_Logs/bugReports.txt");
            setError(exception, filename);

        }
        public static void saveLoginHistory(String exception)
        {
            String filename = HttpContext.Current.Server.MapPath("~/_Logs/login_History.txt");
            setError(exception, filename);
        }

        public static void setError(String exception, string filename, Boolean? redirect=false)
        {
            StreamWriter sw = File.AppendText(filename);
            sw.WriteLine("****** " + DateTime.Now.ToString() + " " + exception.ToString());
            sw.Close();
            if ((bool)redirect)
                HttpContext.Current.Response.Redirect("~/error.aspx");
        }

        /// <summary>
        /// Get the userId that is logged, for the relationships aspnet_User and other tables.
        /// </summary>        
        public static string GetUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        /// <summary>
        /// Get the applicationName
        /// </summary>        
        public static string GetApplicationName()
        {
            ApplicationSecurityInfo asi = new ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
            string appName = asi.ApplicationId.Name;
            return appName;
        }

        /// <summary>
        /// Validate if the string is mail
        /// </summary>
        /// <param name="p_email"></param>
        /// <returns></returns>
        public static bool IsMail(string p_email)
        {

            System.Text.RegularExpressions.Regex l_reg = new System.Text.RegularExpressions.Regex("^(([^<;>;()[\\]\\\\.,;:\\s@\\\"]+" + "(\\.[^<;>;()[\\]\\\\.,;:\\s@\\\"]+)*)|(\\\".+\\\"))@" + "((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}" + "\\.[0-9]{1,3}\\])|(([a-zA-Z\\-0-9]+\\.)+" + "[a-zA-Z]{2,}))$");
            return (l_reg.IsMatch(p_email));
        }

        /// <summary>
        ///  Convert the string into SHA1
        /// </summary>
        /// <param name="NoRepeticiones"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static string getSha1(int NoRepeticiones, string valor)
        {
            SHA1 algorithm = SHA1.Create();
            int i = 0;
            for (i = 0; i <= NoRepeticiones; i += 1)
            {
                byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(valor));
                string sh1 = "";
                for (int y = 0; y < data.Length; y++)
                {
                    sh1 += data[y].ToString("x2").ToUpperInvariant();
                }
                valor = sh1;
            }
            return valor;
        }

        public static string getEncriptPrivateKey(string inputString, string privateKey, CipherMode CyphMode = CipherMode.ECB)
        {

            try
            {
                TripleDESCryptoServiceProvider Des = new TripleDESCryptoServiceProvider();
                //Put the string into a byte array
                byte[] InputbyteArray = Encoding.UTF8.GetBytes(inputString);
                //Create the crypto objects, with the key, as passed in
                MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider();
                Des.Key = hashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(privateKey));
                Des.Mode = CyphMode;
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, Des.CreateEncryptor(), CryptoStreamMode.Write);
                //Write the byte array into the crypto stream
                //(It will end up in the memory stream)
                cs.Write(InputbyteArray, 0, InputbyteArray.Length);
                cs.FlushFinalBlock();
                //Get the data back from the memory stream, and into a string
                StringBuilder ret = new StringBuilder();
                byte[] b = ms.ToArray();
                ms.Close();
                for (int i = 0; i <= Information.UBound(b); i++)
                {
                    //Format as hex
                    ret.AppendFormat("{0:X2}", b[i]);
                }

                return ret.ToString();
            }
            catch (CryptographicException ex)
            {
                saveErrors(ex.ToString(), false);
            }
            return "";
        }

        public static string getDecryptPrivateKey(string inputString, string privateKey, CipherMode CyphMode = CipherMode.ECB)
        {
            if (inputString == string.Empty)
            {
                return "";
            }
            else
            {
                TripleDESCryptoServiceProvider Des = new TripleDESCryptoServiceProvider();
                //Put the string into a byte array
                byte[] InputbyteArray = new byte[Convert.ToInt32(inputString.Length / 2 - 1) + 1];
                //= Encoding.UTF8.GetBytes(InputString)
                //Create the crypto objects, with the key, as passed in
                MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider();

                Des.Key = hashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(privateKey));
                Des.Mode = CyphMode;
                //Put the input string into the byte array

                int X = 0;

                for (X = 0; X <= InputbyteArray.Length - 1; X++)
                {
                    Int32 IJ = (Convert.ToInt32(inputString.Substring(X * 2, 2), 16));
                    ByteConverter BT = new ByteConverter();
                    InputbyteArray[X] = new byte();
                    InputbyteArray[X] = Convert.ToByte(BT.ConvertTo(IJ, typeof(byte)));
                }

                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, Des.CreateDecryptor(), CryptoStreamMode.Write);

                //Flush the data through the crypto stream into the memory stream
                cs.Write(InputbyteArray, 0, InputbyteArray.Length);
                cs.FlushFinalBlock();

                ////Get the decrypted data back from the memory stream
                StringBuilder ret = new StringBuilder();
                byte[] B = ms.ToArray();

                ms.Close();

                int I = 0;

                for (I = 0; I <= Information.UBound(B); I++)
                {
                    ret.Append(Convert.ToChar(B[I]));
                }

                return ret.ToString();
            }
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="emailTo"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool sendMail(string emailTo, string displayEmailName, string subject, string body,
                                    string senderEmail, string senderEmailPassword, string senderSMTPServer, string senderPort,
                                    Stream pdfDocument = null, string pdfDocumentFileName = "",
                                    string cssStyle = "", bool bodyIsHtml = false, string CcoTo = "")
        {

            using (MailMessage mail = new MailMessage())
            {
                string htmlBody;

                if (bodyIsHtml)
                    htmlBody = body;
                else
                {
                    htmlBody = "" + "<html xmlns='http://www.w3.org/1999/xhtml'>" +
                                            "<head>" +
                                                "<meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                                "<title>" + subject + "</title>" + cssStyle +
                                            "</head>" +
                                            "<body>" + body + "</body> </html>";
                }

                string[] mailToArr = emailTo.Split(',');

                mail.From = new MailAddress(senderEmail, displayEmailName);
                int countEmails = 0;
                foreach (string mailTo in mailToArr)
                {
                    if (countEmails == 0)
                        mail.To.Add(new MailAddress(mailTo.Trim()));
                    else
                        mail.CC.Add(new MailAddress(mailTo.Trim()));
                    countEmails++;
                }
                mail.Subject = subject;
                mail.Body = htmlBody;

                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.Normal;
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                //correo.HeadersEncoding = System.Text.Encoding.UTF8
                mail.SubjectEncoding = System.Text.Encoding.UTF8;

                if (pdfDocument != null)
                {
                    mail.Attachments.Add(new Attachment(pdfDocument, pdfDocumentFileName));
                }

                using (SmtpClient smtp = new SmtpClient(senderSMTPServer, int.Parse(senderPort)))
                {                 
                    // 464 si la cuenta es de Gmail
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = new System.Net.NetworkCredential(senderEmail, senderEmailPassword);
                    smtp.EnableSsl = true;

                    // True si la cuenta es de Gmail
                    try
                    {
                        smtp.Send(mail);
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        saveErrors(ex.Message, false);
                    }
                }            
            }
            return false;
        }

        /// <summary>
        /// Convert the uploaded file to byte array
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] ReadFile(HttpPostedFile file)
        {
            byte[] data = new Byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);
            return data;
        }

        /// <summary>
        /// Validates a file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="extensions"></param>
        /// <param name="fileLimit"></param>
        /// <returns></returns>
        public static string validateFileUpload(HttpPostedFile file, string extensions, long fileLimit)
        {
            int nFileLen = file.ContentLength;
            if (nFileLen == 0)
            {
                return "The selected file has not have data";
            }
            if (nFileLen > fileLimit)
            {
                return "The selected file is bigger then 2MB, please verify.";
            }
            FileInfo imageInfo = new FileInfo(file.FileName);
            if (extensions.IndexOf(imageInfo.Extension) < 0)
                return "The file must have the next extensions: " + extensions;

            return "";
        }

        /// <summary>
        /// Regresa en un arreglo las url de los archivos que se encuentran en el folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="extensions">Puede ser null</param>
        /// <param name="subfolders"></param>
        /// <returns></returns>
        public static string[] getFolderFiles(string folder, string extensions = null, bool subfolders = false)
        {
            try
            {
                if(extensions == null && subfolders == false)
                    return Directory.GetFiles(folder);
                else if(extensions != null && subfolders == false)
                    return Directory.GetFiles(folder, extensions);
                else if (extensions != null && subfolders == true)
                    return Directory.GetFiles(folder, extensions, SearchOption.AllDirectories);
                else if (extensions == null && subfolders == true)
                    return Directory.GetFiles(folder, "", SearchOption.AllDirectories);
            }
            catch(Exception ex)
            {
                ex.ToString();
            }
            return null;
        }

        /// <summary>
        /// Upload a file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folder"></param>
        /// <param name="filename"></param>
        /// <param name="replaceFile"></param>
        /// <returns></returns>
        public static bool uploadFile(byte[] file, string folder, ref string filename, bool replaceFile = false)
        {
            try
            {
                string sFilename = filename;
                if (!replaceFile)
                {
                    int file_append = 0;
                    while (System.IO.File.Exists(HttpContext.Current.Server.MapPath(folder + sFilename)))
                    {
                        file_append++;
                        sFilename = System.IO.Path.GetFileNameWithoutExtension(filename)
                                         + "_" + file_append.ToString() + filename.Substring(filename.IndexOf('.'));
                    }
                }
                // Save the stream to disk
                System.IO.FileStream newFile = new System.IO.FileStream(HttpContext.Current.Server.MapPath(folder + sFilename),
                                                                        System.IO.FileMode.Create);
                newFile.Write(file, 0, file.Length);
                newFile.Close();
                filename = sFilename;
                return true;
            }
            catch (Exception ex)
            {
                saveErrors(ex.Message, false);
                return false;
            }
        }

        /// <summary>
        /// Removes the file from the server
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool deleteFiles(string urlFile,bool physicalPath = false)
        {
            string path = urlFile;
            if(!physicalPath)
                path = HttpContext.Current.Server.MapPath(urlFile);

            if(File.Exists(path))
            {
                File.Delete(path);
                if (!File.Exists(path))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the directories exists, else creates the directories
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="Server"></param>
        public static void directoryExists(string directory, HttpServerUtilityBase Server)
        {
            string[] directories = directory.Split('/');
            string currentDirectory = directories[0];
            foreach(string item in directories)
            {
                if (item != currentDirectory)
                    currentDirectory += "/" + item;
                string path = Server.MapPath(currentDirectory);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Generates a Random string
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string getRandomString(int size)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char ch;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                //ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * randoValue + 65)));
                ch = chars[random.Next(chars.Length)];
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Return a datetime from a string date.
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="isLast"></param>
        /// <returns></returns>
        public static DateTime? stringToDate(string strDate, bool isLast = false, char dateDivisor = '/')
        {
            try
            {
                char splitter = Convert.ToChar(dateDivisor);
                string[] dateParts = strDate.Split(splitter);
                int year = int.Parse(dateParts[2]);
                int month = int.Parse(dateParts[1]);
                int day = int.Parse(dateParts[0]);
                DateTime date;
                if (isLast)
                    date = new DateTime(year, month, day).AddDays(1);
                else
                    date = new DateTime(year, month, day);
                return date;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                return null;
            }
        }

        /// <summary>
        /// Convierte un formato en especifico a datetime
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="isLast"></param>
        /// <param name="dateDivisor"></param>
        /// <returns></returns>
        public static DateTime? stringFormatToDate(string strDate, string format = "ddMMyyyyHHmmssfff")
        {
            try
            {
                switch(format)
                {
                    case "ddMMyyyyHHmmssfff":
                        int dd = int.Parse(strDate.Substring(0, 2));
                        int MM = int.Parse(strDate.Substring(2, 2));
                        int yyyy = int.Parse(strDate.Substring(4, 4));
                        int hh = int.Parse(strDate.Substring(8, 2));
                        int mm = int.Parse(strDate.Substring(10, 2));
                        int ss = int.Parse(strDate.Substring(12, 2));
                        int fff = int.Parse(strDate.Substring(14, 3));
                        return new DateTime(yyyy, MM, dd, hh, mm, ss, fff);
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return null;
        }

        /// <summary>
        /// Check if the user is logged
        /// </summary>
        /// <returns></returns>
        public static bool userIsAuthenticated()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
                return true;

            return false;
        }

        /// <summary>
        /// Return the day of the week of a date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int returnDayOfWeek(DateTime date)
        {
            //Sunday = 1 (First day of the week.)
            //Saturday = 7 (Last day of the week.)
            int dayOfWeek = (int)date.DayOfWeek + 1;
            return dayOfWeek;
        }

        /// <summary>
        /// Redirect to airwing.no in https if the user is not in it.
        /// </summary>
        public static void redirectFromOtherDomains()
        {
            string urlAbsolute = HttpContext.Current.Request.Url.AbsoluteUri;
            if (urlAbsolute.IndexOf("airwing.no") < 0)
                HttpContext.Current.Response.Redirect("https://airwing.no");
        }

        /// <summary>
        /// Check that the url is in https
        /// </summary>
        public static void redirectToHTTPS()
        {
            //redirectFromOtherDomains();
            //if (!HttpContext.Current.Request.IsLocal && !HttpContext.Current.Request.IsSecureConnection)
            //{
            //    string redirectUrl = HttpContext.Current.Request.Url.ToString().Replace("http:", "https:");
            //    HttpContext.Current.Response.Redirect(redirectUrl);
            //}
        }

        /// <summary>
        /// Gets a list of string from 1 to 31
        /// </summary>
        /// <returns></returns>
        public static List<string> getEnumerableDaysNumber()
        {
            return Enumerable.Range(1, 31).Select(n => n.ToString("0#")).ToList();
        }

        /// <summary>
        /// Gets a list of string from 1 to 12
        /// </summary>
        /// <returns></returns>
        public static List<string> getEnumerableMonthsNumbers()
        {
            return Enumerable.Range(1, 12).Select(n => n.ToString("0#")).ToList();
        }

        /// <summary>
        /// Gets a list of string from 1970 until today
        /// </summary>
        /// <returns></returns>
        public static List<string> getEnumerableYearsNumbers()
        {
            int diff = DateTime.Now.Year + 1 - 1930;
            if (diff < 1)
                diff = 50;
            var years = Enumerable.Range(1930, diff).Select(n => n.ToString()).ToList();
            return years;
        }

        /// <summary>
        /// Divides a string in name and last name
        /// </summary>
        /// <param name="nombreCompleto"></param>
        /// <returns>String keys "name" & "lastname"</returns>
        public static Dictionary<string,string> getName_LastName(string nombreCompleto)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("name", "-");
            result.Add("lastname", "-");

            if (nombreCompleto != null && nombreCompleto.Trim() != string.Empty && nombreCompleto.Trim() != "")
            {
                var split = nombreCompleto.Split(new char[] { ' ' });
                int counter = 0;
                int half = split.Length / 2;

                result["name"] = "";
                result["lastname"] = "";
                foreach (string name in split)
                {
                    if (counter < half)
                        result["name"] += " " + name;
                    else
                        result["lastname"] += " " + name;
                    counter++;
                }
                result["name"] = result["name"].Trim();
                result["lastname"] = result["lastname"].Trim();
            }

            return result;
        }

        public static string replaceSpecialChars(string cadena){
            cadena = str_replace(new char[] {'á','à','â','ã','ª','ä'},'a',cadena);
            cadena = str_replace(new char[] {'Á','À','Â','Ã','Ä'},'A',cadena);
            cadena = str_replace(new char[] {'Í','Ì','Î','Ï'},'I',cadena);
            cadena = str_replace(new char[] {'í','ì','î','ï'},'i',cadena);
            cadena = str_replace(new char[] {'é','è','ê','ë'},'e',cadena);
            cadena = str_replace(new char[] {'É','È','Ê','Ë'},'E',cadena);
            cadena = str_replace(new char[] {'ó','ò','ô','õ','ö','º'},'o',cadena);
            cadena = str_replace(new char[] {'Ó','Ò','Ô','Õ','Ö'},'O',cadena);
            cadena = str_replace(new char[] {'ú','ù','û','ü'},'u',cadena);
            cadena = str_replace(new char[] {'Ú','Ù','Û','Ü'},'U',cadena);
            //cadena = str_replace(new char[] {'[','^','´','`','¨','~',']'},'',cadena);
            cadena = cadena.Replace("ç","c");
            cadena = cadena.Replace("Ç", "C");
            cadena = cadena.Replace("ñ", "n");
            cadena = cadena.Replace("Ñ", "N");
            cadena = cadena.Replace("Ý", "Y");
            cadena = cadena.Replace("ý", "y");
            cadena = cadena.Replace("&aacute;", "a");
            cadena = cadena.Replace("&Aacute;", "A");
            cadena = cadena.Replace("&eacute;", "e");
            cadena = cadena.Replace("&Eacute;", "E");
            cadena = cadena.Replace("&iacute;", "i");
            cadena = cadena.Replace("&Iacute;", "I");
            cadena = cadena.Replace("&oacute;", "o");
            cadena = cadena.Replace("&Oacute;", "O");
            cadena = cadena.Replace("&uacute;", "u");
            cadena = cadena.Replace("&Uacute;", "U");

            return cadena;
        }

        public static string str_replace(char[] array, char replaceTo, string cadena)
        {
            foreach(char c in array)
            {
                cadena = cadena.Replace(c, replaceTo);
            }
            return cadena;
        }

        #endregion
    }
}