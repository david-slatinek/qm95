namespace qm95
{
    public interface ITransfer
    {
        public string MakeTransfer(int idAccount);
        public string RevertTransaction();
    }
}