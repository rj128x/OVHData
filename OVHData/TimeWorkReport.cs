using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVHData
{
    /// <summary>
    /// Отчет для определения времени точки с ненулевыми данными (время работы)
    /// </summary>
    public class TimeWorkReport
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
        /// Время работы по месяцам
        /// </summary>
        public Dictionary<string, Dictionary<int, double>> DataWork { get; protected set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="dateStart">Дата начала</param>
        /// <param name="dateEnd">Дата окончания</param>
        public TimeWorkReport(DateTime dateStart, DateTime dateEnd)
        {
            DateStart = dateStart;
            DateEnd = dateEnd;
            DateTime date = DateStart.AddDays(0);
            //Инициализация результирующего массива
            DataWork = new Dictionary<string, Dictionary<int, double>>();
        }

        /// <summary>
        /// Чтение данных
        /// </summary>
        public void ReadData()
        {            
            DateTime date = DateStart.AddDays(0);
            DateTime de = date.AddDays(0);
            ProcessDataDelegate evnt = new ProcessDataDelegate(ProcessTimeData);//Инициализация события обработки
            while (date < DateEnd)
            {
                de = date.AddDays(1);//Считываем с шагом 1 сутки

                DataReport rep = new DataReport(date, de, 60, false);//Создаем отчет с шагом 60 секунд
                rep.GetDataEvent = evnt;//Присваиваем событие обработки
                rep.ReadData(Settings.single.Points.Points);//Чтение данных

                date = de.AddDays(0);
            }
        }

        /// <summary>
        /// Обработка считанных данных из овации
        /// </summary>
        /// <param name="name">Имя точки</param>
        /// <param name="date">Дата</param>
        /// <param name="val">значение</param>
        public void ProcessTimeData(string name, DateTime date, double val)
        {
            if (!DataWork.ContainsKey(name))//Если точка еще не определена в массиве, добавляем ее
            {
                DataWork.Add(name, new Dictionary<int, double>());
            }
            int month = date.Month;
            if (!DataWork[name].ContainsKey(month))//Если переход на новый месяц, добавляем месяц к данной точке
            {
                DataWork[name].Add(month, 0);
            }
            if (val != 0)//Если значение не 0, увеличиявем счетчик минут
            {
                DataWork[name][month] += 1;
            }

        }
    }
}
