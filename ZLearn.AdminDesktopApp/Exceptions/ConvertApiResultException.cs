namespace ZLearn.AdminDesktopApp.Exceptions
{
    public class ConvertApiResultException : Exception
    {
        public ConvertApiResultException(string mess = "Chuyển đổi dữ liệu thất bại"): base(mess) { }
    }
}
