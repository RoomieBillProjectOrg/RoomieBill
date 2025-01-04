namespace Roomiebill.Server.Common
{
    /// <summary>
    ///  This class should confirm that an email meets the requirements for a valid email.
    /// The email of the users in the app should be a BGU University email: means that the email should end with @bgu.ac.il or @post.bgu.ac.il.
    /// </summary>
    public class EmailValidator
    {
        public string Error { get; set; }

        public EmailValidator()
        {
            Error = "";
        }
        public bool ValidateEmail(string email)
        {
            if (!email.EndsWith("@bgu.ac.il") && !email.EndsWith("@post.bgu.ac.il"))
            {
                Error = "Email should be a BGU University email";
                return false;
            }
            return true;
        }
    }
}
