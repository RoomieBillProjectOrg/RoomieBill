namespace Roomiebill.Server.Common
{
    /// <summary>
    /// This class should confirm that a password meets the requirements for a valid password.
    /// The password should be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.
    /// </summary>
    public class PasswordValidator
    {
        public string Error { get; set; }

        public PasswordValidator()
        {
            Error = "";
        }

        public bool ValidatePassword(string password)
        {
            if (password.Length < 8)
            {
                Error = "Password must be at least 8 characters long";
                return false;
            }
            if (!password.Any(char.IsUpper))
            {
                Error = "Password does not contain an uppercase letter";
                return false;
            }
            if (!password.Any(char.IsLower))
            {
                Error = "Password does not contain a lowercase letter";
                return false;
            }
            if (!password.Any(char.IsDigit))
            {
                Error = "Password does not contain a number";
                return false;
            }
            if (password.All(char.IsLetterOrDigit))
            {
                Error = "Password does not contain a special character";
                return false;
            }
            return true;
        }
    }
}
