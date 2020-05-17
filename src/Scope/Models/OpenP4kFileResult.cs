namespace Scope.Models
{
    internal class OpenP4kFileResult
    {
        public OpenP4kFileResult()
        {
            Failed = false; Reason = "";
        }

        public OpenP4kFileResult(string failure)
        {
            Failed = true;
            Reason = failure;
        }

        internal bool Failed { get; }
        internal string Reason { get; }
    }
}
