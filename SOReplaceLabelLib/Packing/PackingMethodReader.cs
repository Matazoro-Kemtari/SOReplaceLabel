using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOReplaceLabelLib.Packing
{
    public static class PackingMethodReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static (bool, List<string>) ReadSettingFile(string path)
        {
            var methodList = new List<string>();
            if(!System.IO.File.Exists(path))
            {
                return (false, null);
            }

            try
            {
                using (var sw = new System.IO.StreamReader(path))
                {

                    string readline = null;
                    while((readline = sw.ReadLine()) != null)
                    {
                        methodList.Add(readline);
                    }
                }
            }
            catch(Exception)
            {
                return (false, null);
            }

            return (true, methodList);
        }
    }
}
