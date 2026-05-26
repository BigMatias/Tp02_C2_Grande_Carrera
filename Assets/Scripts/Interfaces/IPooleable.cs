// Warning: Baja - "Pooleable" no es palabra; el término correcto es "Poolable".
public interface IPooleable
{
   public bool IsActive { get; }
   public void Activate();
   public void Deactivate();
}