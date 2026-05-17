public abstract class BaseStatus
{
    public StatusResolutionTiming StatusTiming { get; private set; }
    
    public BaseStatus(StatusResolutionTiming timing)
    {
        this.StatusTiming = timing;
    }

    public abstract void ProcessStatus();
}

public abstract class StackableBaseStatus : BaseStatus
{
    public uint StackSize { get; private set; }
    public StackableBaseStatus(StatusResolutionTiming timing) : base(timing)
    {
    }
}

public class PoisonStatus : BaseStatus
{
    public PoisonStatus(StatusResolutionTiming timing) : base(timing)
    {

    }

    public override void ProcessStatus()
    {
        throw new System.NotImplementedException();
    }
}