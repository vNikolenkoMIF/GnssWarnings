
namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Impacts
{
    public class SwitchOffCorrectors : GnssIssueImpact
    {
        public SwitchOffCorrectors()
        {
        }

        protected override void MakeImpact()
        {
            //Stop all correctors
        }

        protected override void StopImpact()
        {
            //Restore all correctors
        }
    }
}