public interface IObjectPool<T>
{
    T Get();
    void Release(T item);
}
