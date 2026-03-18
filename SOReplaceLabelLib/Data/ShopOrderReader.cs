using CsvHelper;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SOReplaceLabelLib.Data
{
    public class ShopOrderReader
    {
        static string LastErr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static (bool, ShopOrderTexts) Read(string path)
        {
            bool result = false;
            ShopOrderTexts shopOrderTexts = null;
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(path, Encoding.GetEncoding("shift_jis"));
                using (var csv = new CsvReader(reader))
                {
                    reader = null;
                    csv.Configuration.RegisterClassMap<ShopOrderMap>();
                    csv.Configuration.HasHeaderRecord = false;
                    csv.Configuration.Delimiter = "\t";
                    csv.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
                    shopOrderTexts = csv.GetRecords<ShopOrderTexts>().FirstOrDefault();
                    result = shopOrderTexts != null;
                }
            }
            catch(Exception exp)
            {
                LastErr = exp.Message;
                return (result, shopOrderTexts);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return (result, shopOrderTexts); ;
        }
    }
}
