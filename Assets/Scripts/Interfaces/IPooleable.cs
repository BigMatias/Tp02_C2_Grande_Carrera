public interface IPooleable
{
   public bool IsActive { get; }
   public void Activate();
   public void Deactivate();
}