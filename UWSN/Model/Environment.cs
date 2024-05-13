namespace UWSN.Model
{
    public class Environment
    {
        /// <summary>
        /// Скорость ветра (м/с) для уравнения пассивного сонара модели вероятности
        /// </summary>
        public double PassiveSonarEqParameterW { get; set; } = 0.0;

        /// <summary>
        /// Фактор судоходства (безразмерная величина) для уравнения пассивного сонара модели вероятности
        /// </summary>
        public double PassiveSonarEqParameterS { get; set; } = 0.5;

        public List<Sensor> Sensors { get; set; } = new();
    }
}
