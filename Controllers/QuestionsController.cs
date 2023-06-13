using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TriviaQuiz_WebApp.Models;
using TriviaQuiz_WebApp.Models.HelperModel;

namespace TriviaQuiz_WebApp.Controllers
{
    public class QuestionsController : Controller
    {
        private TriviaDbContext _context;
        private int _totalQuestions;
        public List<Question> pagedQuestions;
        public static List<Question> shownList;


        public QuestionsController(TriviaDbContext context)
        {
            _context = context;
        }

        private static Token _sessToken;

        public async Task<IActionResult> Index(Token sessionToken,int pageNumber, string? category, string? difficulty, string? questionTerm)
        {
            if(sessionToken != null)
            {
                _sessToken = sessionToken;
            }
            

            try
            {
                    //fetch next set
                    if (String.IsNullOrEmpty(category) && String.IsNullOrEmpty(difficulty) && String.IsNullOrEmpty(questionTerm))
                    {
                        pagedQuestions = await FetchDataAsync(_sessToken.token,pageNumber);
                        shownList = pagedQuestions;
                    }
                    else
                    {
                        //filter currently listed questions
                        pagedQuestions = FilterData(shownList?? await FetchDataAsync(_sessToken.token,pageNumber), category, difficulty, questionTerm);
                    }
               
            }
            catch
            {
                throw new Exception("Failed to save data to local database.");
            }
            
            return View(pagedQuestions);
        }


        private List<Question> FilterData(List<Question> pagedQuestions, string? category = null, string? difficulty = null, string? questionTerm = null)
        {

            if (!string.IsNullOrEmpty(category))
            {
                pagedQuestions = pagedQuestions.Where(q => q.Category.Equals(category)).ToList();
            }

            if (!string.IsNullOrEmpty(difficulty))
            {
                pagedQuestions = pagedQuestions.Where(q => q.Difficulty == difficulty).ToList();
            }

            if (!string.IsNullOrEmpty(questionTerm))
            {
                pagedQuestions = pagedQuestions.Where(q => q.QuestionText.ToLower().Contains(questionTerm.Trim().ToLower())).ToList();
            }

            return pagedQuestions;
        }

        private async Task<List<Question>> FetchDataAsync(string token, int pageNumber, string? category = null, string? difficulty = null, string? questionTerm = null)
        {
            var pageSize = 10;

            int index = pageNumber == 0 ? 0 : (pageNumber == 1 ? pageSize : (pageNumber - 1) * pageSize);

            using (var client = new ApiClient())
            {
                _totalQuestions = await client.GetVerifiedQuestionCountAsync();
                if (index >= _totalQuestions)
                {
                    throw new Exception("Quiz Completed! All questions have been retrieved");
                }
                await Task.Run(() => client.GetQuestionsAsync(token));

                // pagedQuestions = await _context.Questions.Include(q => q.Answers)
                //.OrderBy(t => t.Id).Skip(index).Take(pageSize).ToListAsync();

                //maybe a "dirty" way but it was quicker than the above
                pagedQuestions = await _context.Questions.Include(q => q.Answers)
                                                         .OrderByDescending(t => t.Id)
                                                         .Take(pageSize).ToListAsync();

            }

            ViewBag.CurrentPage = pageNumber;

            return pagedQuestions;

        }


        //[HttpPost]
        //public IActionResult FilterQuestions(string? category, string? difficulty, string? questionTerm)
        //{

        //    if (!string.IsNullOrEmpty(category))
        //    {
        //        pagedQuestions = pagedQuestions.Where(q => q.Category.Equals(category)).ToList();
        //    }

        //    if (!string.IsNullOrEmpty(difficulty))
        //    {
        //        pagedQuestions = pagedQuestions.Where(q => q.Difficulty == difficulty).ToList();
        //    }

        //    if (!string.IsNullOrEmpty(questionTerm))
        //    {
        //        pagedQuestions = pagedQuestions.Where(q => q.QuestionText.ToLower().Contains(questionTerm.Trim().ToLower())).ToList();
        //    }

        //    return View(pagedQuestions);

        //}
    }
}
