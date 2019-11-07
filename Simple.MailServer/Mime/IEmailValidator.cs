namespace SMTPd.Mime
{
    public interface IEmailValidator
    {
        bool Validate(string email);
    }
}
