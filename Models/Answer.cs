using System;
using System.Collections.Generic;

namespace TriviaQuiz_WebApp.Models;

public partial class Answer
{
    public int Id { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int QuestionId { get; set; }

    public virtual Question Question { get; set; } = null!;
}
