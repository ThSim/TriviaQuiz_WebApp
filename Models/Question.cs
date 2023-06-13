using System;
using System.Collections.Generic;

namespace TriviaQuiz_WebApp.Models;

public partial class Question
{
    public int Id { get; set; }

    public string Category { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Difficulty { get; set; } = null!;

    public string QuestionText { get; set; } = null!;

    public string CorrectAnswer { get; set; } = null!;

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
