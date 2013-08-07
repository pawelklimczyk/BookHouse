namespace BookHouse.Domain
{
    public class OperationStatus<T>
    {
        public string OperationMessage { get; set; }
        public OperationResult Result { get; set; }
        public T Data { get; set; } 
    }


    public enum OperationResult
    {
        Passed,
        Failed
    }
}