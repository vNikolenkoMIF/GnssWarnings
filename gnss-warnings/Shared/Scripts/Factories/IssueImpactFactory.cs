using System;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Impacts;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Factories
{
    public class IssueImpactFactory
    {
        public IGnssIssueImpact GetImpactType(ImpactType impactType)
        {
            return impactType switch {
                ImpactType.NoImpact => new NoImpact(),
                ImpactType.DirectionCorrector => new DirectionCorrectionOff(),
                ImpactType.AllCorrectors => new SwitchOffCorrectors(),
                _ => throw new ArgumentOutOfRangeException(nameof(impactType), impactType, null)
            };
        }
    }
}
