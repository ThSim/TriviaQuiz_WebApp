namespace TriviaQuiz_WebApp.Models.HelperModel
{
    
        public class Overall
        {
            //assuming that API returns only verified questions
            public int total_num_of_verified_questions { get; set; }
        }

        public class CountGlobalRoot
        {
            public Overall overall { get; set; }
        }


        public class Token
        {
            public int response_code { get; set; }
            public string response_message { get; set; }
            public string token { get; set; }
        }

        public class Result
        {
            public string category { get; set; }
            public string type { get; set; }
            public string difficulty { get; set; }
            public string question { get; set; }
            public string correct_answer { get; set; }
            public List<string> incorrect_answers { get; set; }
        }

        public class Root
        {
            public int response_code { get; set; }
            public List<Result> results { get; set; }
        }
}
