namespace HomeHomie.ElectricityModule
{
    public interface IElectricitySettings
    {
        public string? GraphicDomain { get; }
        public string? GraphicPage { get; }
        public int[]? DatesToCheck { get; }
    }
}
