using NightCycle;

public interface IFlashlightProvider
{
    PlayerFlashlight Current { get; set; }
    float? PendingEssence { get; set; }
}

public class FlashlightProvider : IFlashlightProvider
{
    public PlayerFlashlight Current { get; set; }
    public float? PendingEssence { get; set; }
}