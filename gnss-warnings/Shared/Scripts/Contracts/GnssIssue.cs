namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts
{
    public enum GnssIssue
    {
        GnssDisconnected,
        RtkFixBecameFloat,
        RtkInvalid,
        FluctuatingAccuracy,
        InsufficientAccuracy,
        DirectionInsufficientAccuracy,
        DataCollectionInsufficientAccuracy
    }
}
