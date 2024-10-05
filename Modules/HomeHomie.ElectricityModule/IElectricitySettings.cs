namespace HomeHomie.ElectricityModule
{
    public interface IElectricitySettings
    {
        public bool? CheckReports { get; }

        public string? GraphicDomain { get; }
        public string? GraphicPage { get; }
        public List<int>? DatesToCheck { get; }

        public string? ReportCheckDelay { get; }
        public string? GraphicCheckDelay { get; }
    }

    public class ElectricitySettings : IElectricitySettings
    {
        public bool? CheckReports { get; init; }

        public string? GraphicDomain { get; init; }
        public string? GraphicPage { get; init; }
        public List<int>? DatesToCheck { get; init; }
        public string? ReportCheckDelay { get; init; }
        public string? GraphicCheckDelay { get; init; }
    }
}
