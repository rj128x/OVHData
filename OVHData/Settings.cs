using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVHData
{
    /// <summary>
    /// Настройки точек
    /// </summary>
    public class PointsSettings
    {
        /// <summary>
        /// Список точек
        /// </summary>
        public List<PointInfo> Points { get; set; }
        /// <summary>
        /// Словарь точек по имени в овации
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute]        
        public Dictionary<string, string> PointsDict { get; set; }
        /// <summary>
        /// Словарь точек по имени в xml
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public Dictionary<string, string> PointsDictByDescr { get; set; }

        /// <summary>
        /// Инициализация массивов точек
        /// </summary>
        public void initData()
        {
            PointsDict = new Dictionary<string, string>();
            PointsDictByDescr = new Dictionary<string, string>();
            foreach (PointInfo pi in Points)
            {
                try {
                    PointsDict.Add(pi.Name, pi.Descr);
                }
                catch { 
                    Logger.Info(String.Format("Ошибка при добавлении точки в словарь {0} {1}",pi.Name,pi.Descr));
                }
                try {
                    PointsDictByDescr.Add(pi.Descr, pi.Name);
                }
                catch {
                    Logger.Info(String.Format("Ошибка при добавлении точки в словарь {0} {1}", pi.Name, pi.Descr));
                }
            }
        }
    }
    
    /// <summary>
    /// Описание точки
    /// </summary>
    public class PointInfo
    {
        /// <summary>
        /// Имя точки в овации
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public string Name{get;set;}
        /// <summary>
        /// имя точки в xml
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public string Descr { get; set; }
        /// <summary>
        /// Обрабатывать точку
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public bool Read { get; set; }
    }

    /// <summary>
    /// Класс настроек
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Ссылка на единственный объект настроек
        /// </summary>
        protected static Settings settings;
        /// <summary>
        /// Путь к папке с лог файлами
        /// </summary>
        public string LogPath { get; set; }
        /// <summary>
        /// Путь к файлу с настройками точек
        /// </summary>
        public string PointsFile { get; set; }
        /// <summary>
        /// Сдвиг по времени от UTC
        /// </summary>
        public int HoursUTC { get; set; }
                
        /// <summary>
        /// Настройки точек
        /// </summary>
        public PointsSettings Points;

        /// <summary>
        /// Ссылка на единственный объект точек
        /// </summary>
        public static Settings single
        {
            get
            {
                return settings;
            }
        }
         
        
        static Settings()
        {

        }

        /// <summary>
        /// Инициализация объекта настроек
        /// </summary>
        public static void init()
        {
            Settings settings = XMLSer<Settings>.fromXML("Data\\Settings.xml"); //Считываем настройки из файла
            settings.Points = XMLSer<PointsSettings>.fromXML("Data\\" + settings.PointsFile);//Считываем настройки точек
            settings.Points.initData();//Инициализация словарей точек
            Settings.settings = settings;  
        }

       
    }
}
