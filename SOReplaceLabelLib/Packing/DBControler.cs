using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOReplaceLabelLib.Packing
{
    public class DBControler
    {
        private string FilePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        public DBControler()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbFile"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> OpenDB(string dbFile)
        {
            var result = false;

            try
            {
                using (var cn = new SQLiteConnection("DataSource=" + dbFile))
                {
                    await cn.OpenAsync();
                    cn.Close();
                }
                result = true;
            }
            catch(Exception exp)
            {
                return (result, exp.Message);
            }

            return (result, null);
        }

        /// <summary>
        /// データを検索する
        /// </summary>
        /// <param name="dbFile"></param>
        /// <param name="partsNo"></param>
        /// <returns></returns>
        public static (bool result, string message, List<Data.PartsPacking> packings) GetDatasByWorkNo(string dbFile, string partsNo)
        {
            var result = false;
            var foundDatas = new List<Data.PartsPacking>();

            try
            {
                using (var cn = new SQLiteConnection("DataSource=" + dbFile))
                {
                    var context = new DataContext(cn);
                    var table = context.GetTable<Data.PartsPacking>();
                    var data = table.Where(item => item.PartsNo == partsNo);
                    foundDatas.AddRange(data);
                    cn.Close();
                }
                result = true;
            }
            catch (Exception exp)
            {
                return (result, exp.Message, foundDatas);
            }

            return (result, string.Empty, foundDatas);
        }

        /// <summary>
        /// データを追加する
        /// </summary>
        /// <param name="dbFile"></param>
        /// <param name="partsNo"></param>
        /// <returns></returns>
        public static (bool result, string message) InsertData(string dbFile, Data.PartsPacking insertData)
        {
            var result = false;

            try
            {
                using (var cn = new SQLiteConnection("DataSource=" + dbFile))
                {
                    var context = new DataContext(cn);
                    context.GetTable<Data.PartsPacking>().InsertOnSubmit(insertData);
                    context.SubmitChanges();
                    cn.Close();
                }
                result = true;
            }
            catch (Exception exp)
            {
                return (result, exp.Message);
            }

            return (result, string.Empty);
        }

        /// <summary>
        /// データを更新する
        /// </summary>
        /// <param name="dbFile"></param>
        /// <param name="partsNo"></param>
        /// <returns></returns>
        public static (bool result, string message) UpdateData(string dbFile, Data.PartsPacking updateData)
        {
            var result = false;

            try
            {
                using (var cn = new SQLiteConnection("DataSource=" + dbFile))
                {
                    var context = new DataContext(cn);
                    foreach(var partsPacking in context.GetTable<Data.PartsPacking>().Where(item => item.Id == updateData.Id))
                    {
                        partsPacking.PartsNo = updateData.PartsNo;
                        partsPacking.PackMethod = updateData.PackMethod;
                    }
                    context.SubmitChanges();
                    cn.Close();
                }
                result = true;
            }
            catch (Exception exp)
            {
                return (result, exp.Message);
            }

            return (result, string.Empty);
        }
    }
}
