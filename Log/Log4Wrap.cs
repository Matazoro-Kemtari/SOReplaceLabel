using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Log
{
    /// <summary>
    /// 出力レベル
    /// </summary>
    public enum Level
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        None,
    };

    public sealed class Log4Wrap
    {
        /// <summary>
        /// Log4Net
        /// </summary>
        private ILog logger = null;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        private static Log4Wrap _singleInstance = new Log4Wrap();

        /// <summary>
        /// ログフォルダ
        /// </summary>
        public string LogFolderPath { get; private set; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <returns></returns>
        public static Log4Wrap GetInstance()
        {
            return _singleInstance;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private Log4Wrap()
        {
            log4net.Config.XmlConfigurator.Configure();
            logger = LogManager.GetLogger(@"Logger");
            DirectorySetting();
        }

        /// <summary>
        /// 表示レベルに応じた出力
        /// </summary>
        /// <param name="messaage"></param>
        /// <param name="level"></param>
        public void Log(object messaage, Level level)
        {
            switch(level)
            {
                case Level.Debug:
                {
                    Debug(messaage);
                    break;
                }
                case Level.Info:
                {
                    Info(messaage);
                    break;
                }
                case Level.Warn:
                {
                    Warn(messaage);
                    break;
                }
                case Level.Error:
                {
                    Error(messaage);
                    break;
                }
                case Level.Fatal:
                {
                    Fatal(messaage);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        /// <summary>
        /// DEBUG
        /// </summary>
        /// <param name="message"></param>
        public void Debug(object message)
        {
            logger.Debug(message);
        }

        /// <summary>
        /// INFO
        /// </summary>
        /// <param name="message"></param>
        public void Info(object message)
        {
            logger.Info(message);
        }

        /// <summary>
        /// WARN
        /// </summary>
        /// <param name="message"></param>
        public void Warn(object message)
        {
            logger.Warn(message);
        }

        /// <summary>
        /// ERROR
        /// </summary>
        /// <param name="message"></param>
        public void Error(object message)
        {
            logger.Error(message);
        }

        /// <summary>
        /// FATAL
        /// </summary>
        /// <param name="message"></param>
        public void Fatal(object message)
        {
            logger.Fatal(message);
        }

        /// <summary>
        /// フォルダを作成
        /// </summary>
        /// <param name="maxBuckupSize"></param>
        /// <returns></returns>
        public bool DirectorySetting()
        {
            bool result = false;

            try
            {
                // RootのLoggerを取得
                var rootLogger = ((Hierarchy)logger.Logger.Repository).Root;
                // RootのAppenderを取得
                var appender = rootLogger.GetAppender("RollingLogFileAppenderAll") as RollingFileAppender;

                if (appender != null)
                {
                    //Appenderの保存ファイル名から保存先ディレクトリを取得
                    var logDir = Directory.GetParent(appender.File);
                    //ディレクトリを作成し、アクセス権限を変更
                    if(!Directory.Exists(logDir.FullName))
                    {
                        Directory.CreateDirectory(logDir.FullName);
                    }
                    //ファイル書き込み権限を全てのユーザーにする（マルチログイン対応）
                    var security = Directory.GetAccessControl(logDir.FullName);
                    var rule = new FileSystemAccessRule(
                        new NTAccount("everyone"),
                        FileSystemRights.FullControl,
                        InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow);
                    security.SetAccessRule(rule);
                    Directory.SetAccessControl(logDir.FullName, security);
                    LogFolderPath = logDir.FullName;
                }
            }
            catch (Exception)
            {

            }

            return result;
        }

        /// <summary>
        /// 古い日付別ログデータを消去する
        /// </summary>
        /// <param name="maxBuckupSize"></param>
        /// <returns></returns>
        public bool RemoveOldRollingDateLog(int maxBuckupSize = 14)
        {
            bool result = false;

            try
            {
                // RootのLoggerを取得
                var rootLogger = ((Hierarchy)logger.Logger.Repository).Root;
                // RootのAppenderを取得
                var appender = rootLogger.GetAppender("RollingLogFileAppenderAll") as RollingFileAppender;

                if(appender != null)
                {
                    //Appenderの保存ファイル名から保存先ディレクトリを取得
                    var logDir = System.IO.Directory.GetParent(appender.File);
                    //ディレクトリ内のlogファイルを作成日降順で取得
                    var logFileInfos = Directory.GetFiles(logDir.FullName, "*.log")
                                             .Select(item => new System.IO.FileInfo(item))
                                             .OrderByDescending(item => item.CreationTime)
                                             .ToList().AsReadOnly();
                    
                    //最大保存数だけログを残し、残りのログを消去
                    foreach(var removeFile in logFileInfos.Skip(maxBuckupSize))
                    {
                        if(removeFile.Exists)
                        {
                            File.Delete(removeFile.FullName);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return result;
        }
    }
}
