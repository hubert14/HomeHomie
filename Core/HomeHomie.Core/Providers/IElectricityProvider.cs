namespace HomeHomie.Core.Providers
{
    internal interface IElectricityProvider
    {
        public void StartRecieving();
        public void StopRecieving();
    }
}
