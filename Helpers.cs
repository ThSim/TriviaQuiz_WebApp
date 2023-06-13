using System.Text;

namespace TriviaQuiz_WebApp
{
    public static class Helpers
    {

        public static string DecodeAndCapitalize(string input)
        {
            input = Encoding.UTF8.GetString(Convert.FromBase64String(input));
            string capInput = input ?? "";

            try
            {
                switch (input)
                {
                    case null: break;
                    case "": break;
                    default:
                        capInput = input[0].ToString().ToUpper() + input.Substring(1);
                        break;
                }
            }
            catch
            {
                return capInput;
            }
            return capInput;
        }
    }
}
