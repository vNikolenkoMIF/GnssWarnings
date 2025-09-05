namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Impacts
{
    public class DirectionCorrectionOff : GnssIssueImpact
    {
        public DirectionCorrectionOff()
        {
        }

        protected override void MakeImpact()
        {
            //Disble direction corrector only
        }

        protected override void StopImpact()
        {
            //Enable all location correctors
        }
    }
}