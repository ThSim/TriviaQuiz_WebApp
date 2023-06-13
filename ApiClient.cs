using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Linq.Expressions;
using TriviaQuiz_WebApp.Models;
using TriviaQuiz_WebApp.Models.HelperModel;


namespace TriviaQuiz_WebApp
{
    public class ApiClient : IDisposable
    {
        //Setting only the amount of questions, and not any other parameter, since all questions should be listed
        private string _baseQuestionsUrl = "https://opentdb.com/api.php?amount=10&encode=base64";

        private readonly string _qCountUrl = "https://opentdb.com/api_count_global.php";
        private readonly string _sessionTokenUrl = "https://opentdb.com/api_token.php?command=request";
        private readonly string _resetSessionTokenUrl = "https://opentdb.com/api_token.php?command=reset&token=";
        private HttpClient _HttpClient;
        private bool disposedValue;
        public static Token SessionToken;

        public ApiClient()
        {
            _HttpClient = new HttpClient();
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _HttpClient.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<int> GetVerifiedQuestionCountAsync()
        {
            var response = await _HttpClient.GetAsync(_qCountUrl);
            try
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var countResponse = JsonConvert.DeserializeObject<CountGlobalRoot>(content);
                return countResponse.overall.total_num_of_verified_questions;
            }
            catch
            {
                //If cannot retrieve count of (verified) questions, assume the max num is 10000.
                //procceed and if this exceeds - catch the response code 1
                return 10000;
            }
        }

        #region Questions

        public async Task GetQuestionsAsync(string token)
        {
//            var token = await GetRetrieveToken();
            var questionsUrl = $"{_baseQuestionsUrl}&token={token}";
            var triviaQuestions = await RetrieveQuestionsAsync(questionsUrl);
            await PopulateDataToDBAsync(triviaQuestions);
        }

        private async Task<List<Question>> RetrieveQuestionsAsync(string url)
        {
            var response = await _HttpClient.GetAsync(url);
            List<Question> retrievedQuestions = new List<Question>();

            try
            {
                response.EnsureSuccessStatusCode();

                Root root = await response.Content.ReadFromJsonAsync<Root>();

                if (root != null)
                {
                    switch (root.response_code)
                    {
                        case 1: throw new Exception("Insufficient number of available questions...");
                        case 2: throw new Exception("Application Error.");
                        case 3: throw new Exception("Your session expired!");
                        case 4: throw new Exception("Reached the finish!No more available questions found!");
                        case 0: break;
                        default: //something undocumented 
                            throw new Exception("Application Error.");
                    }
                }

                if (root?.results?.Count == 0)
                {
                    throw new Exception("No questions found");
                }

                foreach (var triviaQuestion in root.results)
                {
                    var question = new Question
                    {
                        Category = Helpers.DecodeAndCapitalize(triviaQuestion.category),
                        Type = Helpers.DecodeAndCapitalize(triviaQuestion.type),
                        Difficulty = Helpers.DecodeAndCapitalize(triviaQuestion.difficulty),
                        QuestionText = Helpers.DecodeAndCapitalize(triviaQuestion.question),
                        CorrectAnswer = Helpers.DecodeAndCapitalize(triviaQuestion.correct_answer)
                    };

                    //Add in question all available answers
                    question.Answers = triviaQuestion.incorrect_answers.Select(incorrectAnswer => new Answer
                    { AnswerText = Helpers.DecodeAndCapitalize(incorrectAnswer), IsCorrect = false }).ToList();

                    question.Answers.Add(new Answer { AnswerText = Helpers.DecodeAndCapitalize(triviaQuestion.correct_answer), IsCorrect = true });


                    //Randomize order of all answers
                    question.Answers = question.Answers.OrderBy(a => Guid.NewGuid()).ToList();

                    retrievedQuestions.Add(question);
                }
            }
            catch
            {
                throw new Exception($"Failed to retrieve data. Status code: {response.StatusCode}");
            }

            return retrievedQuestions;
        }


        private async Task PopulateDataToDBAsync(List<Question> triviaQuestions)
        {
            try
            {
                using (TriviaDbContext _dbContext = new TriviaDbContext())
                {
                    _dbContext.Questions.AddRange(triviaQuestions);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save data to local database.");
            }
        }
        #endregion

        #region Tokens

        public async Task<Token> GetRetrieveToken()
        {
            Token token = await GetTokenAsync(_sessionTokenUrl);
            return token;
        }


        public async Task<Token> GetResetToken(string curr_token)
        {
            string url = string.Concat(_resetSessionTokenUrl, curr_token);

            Token token = await GetTokenAsync(url);
            return token;
        }

        public async Task<Token> GetTokenAsync(string url)
        {
            var httpResponse = await _HttpClient.GetAsync(url);
            Token token = new Token();

            try
            {
                httpResponse.EnsureSuccessStatusCode();
                token = await httpResponse.Content.ReadFromJsonAsync<Token>();
            }
            catch (Exception)
            {
                throw new Exception("Oops... Something went wrong");
            }
            return token;
        }






        #endregion



        public async Task<Token> ResetTokenAndFlushDB(string token)
        {
            try
            {
                using (TriviaDbContext _dbContext = new TriviaDbContext())
                {

                    _dbContext.Database.ExecuteSqlRaw("DELETE FROM [Answers];");
                    _dbContext.Database.ExecuteSqlRaw("DELETE FROM [Questions];");
                    _dbContext.SaveChanges();
                }
                return await GetResetToken(token);
            }catch(Exception ex)
            {
                var ee = ex;
            }

            return await GetRetrieveToken();
        }


    }
}
