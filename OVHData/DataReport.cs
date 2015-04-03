using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace OVHData
{
    /// <summary>
    /// Делегат для обработки считанных данных
    /// </summary>
    /// <param name="name">Имя точки в xml</param>
    /// <param name="date">Дата</param>
    /// <param name="val">Считанное значение</param>
    public delegate void ProcessDataDelegate(string name, DateTime date, double val);

    /// <summary>
    /// Файл отчета с данными
    /// </summary>
    public class DataReport
    {
        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime DateStart { get; protected set; }
        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime DateEnd { get; protected set; }
        /// <summary>
        /// Слоаврь с данными по дате
        /// </summary>
        public Dictionary<DateTime, Dictionary<string, double>> Data;
        /// <summary>
        /// Интерва отчета в секундах
        /// </summary>
        public int Interval { get; set; }
        /// <summary>
        /// Событие на считывание данных
        /// </summary>
        public ProcessDataDelegate GetDataEvent{get;set;}
        /// <summary>
        /// Заполнять словарь Data
        /// </summary>
        public bool FillData { get; protected set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="dateStart">дата начала</param>
        /// <param name="dateEnd">дата окончания</param>
        /// <param name="interval">интервал в секундах</param>
        /// <param name="FillData">заполнять массив данных</param>
        public DataReport(DateTime dateStart, DateTime dateEnd, int interval, bool FillData)
        {
            Logger.Info(String.Format("Создание пустого отчета с {0} по {1}", dateStart, dateEnd));
            DateStart = dateStart;
            DateEnd = dateEnd;
            Interval = interval;

            //Если создавать массив Data создаем пустой отчет
            if (FillData){                
                DateTime date = dateStart.AddSeconds(0);
                Data = new Dictionary<DateTime, Dictionary<string, double>>();
                while (date < dateEnd)
                {
                    Data.Add(date, new Dictionary<string, double>());
                    foreach (PointInfo pi in Settings.single.Points.Points)
                    {
                        Data[date].Add(pi.Descr, 0);
                    }
                    date = date.AddSeconds(interval);
                }
            }
            Logger.Info("Пустой отчет создан");
        }

        /// <summary>
        /// Считываем данные
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public bool ReadData(List<PointInfo> points)
        {
            Logger.Info("Чтение данных");
            bool isOk = true;

            //Создаем подключение oledb
            OleDbConnection connection = new OleDbConnection("Provider=Ovation Process Historian OLE DB Provider; Data Source=Drop160_n6; RetrievalMode=MODE_LATEST;");
            OleDbDataReader reader = null;            
            try
            {
                Logger.Info("Подключение к базе");
                connection.Open();//Открываем подключение
                foreach (PointInfo pi in points)
                {
                    Logger.Info(String.Format("Чтение данных для точки {0} ({1})", pi.Name, pi.Descr));
                    OleDbCommand command = connection.CreateCommand();
                    //Создаем запрос
                    command.CommandText = String.Format("select timestamp, f_value  from processeddata (#{0}#, #{1}#, IntervalSize, {3}, PointNamesOnly, OPH_TIMEAVERAGE, '{2}')",
                                                        DateStart.AddHours(-Settings.single.HoursUTC).ToString("MM'/'dd'/'yyyy HH:mm:ss"),
                                                        DateEnd.AddHours(-Settings.single.HoursUTC).ToString("MM'/'dd'/'yyyy HH:mm:ss"),
                                                        pi.Name, Interval);
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader();//Вполнение запроса
                    while (reader.Read())//Чтение данных
                    {
                        DateTime dt = DateTime.Parse(reader[0].ToString());
                        dt=dt.AddHours(Settings.single.HoursUTC);
                        double val = (double)reader[1];//Получаем значение
                        if (FillData)//Если заполнять массив данных
                        {
                            try
                            {
                                Data[dt][pi.Descr] = val;
                            }
                            catch (Exception e)
                            {
                                Logger.Info("Ошибка при обработке данных ");
                                Logger.Info(e.ToString());
                            }
                        }
                        if (GetDataEvent != null)//Если присвоено событие на обработку данных выполняем
                        {
                            try
                            {
                                GetDataEvent(pi.Descr, dt, val);
                            }
                            catch (Exception e)
                            {
                                Logger.Info("Ошибка при обработке данных. Делегат");
                                Logger.Info(e.ToString());
                            }
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                isOk = false;
                Logger.Info("Ошибка при выборке из БД");
                Logger.Info(e.ToString());
            }
            finally //Закрываем подключения к базе
            {
                try {
                    Logger.Info("Закрытие reader ");
                    reader.Close(); }
                catch (Exception e) {
                    Logger.Info("Ошибка при закрытии reader");
                    Logger.Info(e.ToString());
                }
                try {
                    Logger.Info("Закрытие подключения ");
                    connection.Close();
                }
                catch (Exception e)
                {
                    Logger.Info("Ошибка при закрытии подключения");
                    Logger.Info(e.ToString());
                }
            }
            Logger.Info("Чтение данных завершено ok:" + isOk);
            return isOk;
        }


        /// <summary>
        /// Запись результатов в файл
        /// </summary>
        /// <param name="output">Имя файла</param>
        /// <param name="writeFirst">Выводить заколовок</param>
        public void WriteReportData(string output,bool writeFirst=false)
        {            
            Logger.Info("Сохранение данных" );
            
            String headerStr = String.Format("{0};{1}", "Дата", String.Join(";", Settings.single.Points.PointsDict.Values));//Создаем строку заголовка в формате csv
            if (writeFirst) {
                OutputData.writeToOutput(output, headerStr);
            }
            foreach (DateTime date in Data.Keys)//Создаем строки данных
            {
                string ValueStr = String.Format("{0};{1}", date.ToString("dd.MM.yyyy HH:mm:ss"), String.Join(";", Data[date].Values));
                OutputData.writeToOutput(output, ValueStr);
            }
            Logger.Info("Сохранение данных завершено");
        }
    }
}
