using ProjectX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectX.Models
{
    public partial class Users
    {
        public List<SelectListItem> ListOfUsers { get; set; }
        public List<SelectListItem> courses { get; set; }
        public List<int> TestType { get; set; }
    }

    public partial class Scores
    {
        public string CourseName { get; set; }
        public string paperName { get; set; }
        public string LevelName { get; set; }
        public string userName { get; set; }

    }
    public class EvaluateTests
    {
        public Guid TestId { get; set; }
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string UserName { get; set; }
        public string CourseName { get; set; }
        public string status { get; set; }
        public Nullable<DateTime> lastReviewed { get; set; }
        public bool isRevalidate { get; set; }
    }

    public class ValidateTest
    {
        public Guid TestId { get; set; }
        public int UserId { get; set; }
        public int QId { get; set; }
        public string Question { get; set; }
        public string MultiLineAnswer { get; set; }
        public int Score { get; set; }
    }

    public partial class Questions
    {
        public ICollection<Options> options { get; set; }
    }
    public partial class Papers
    {
        public string CourseName { get; set; }
        public List<Papers> ListOfPapers { get; set; }
    }

    public partial class Levels
    {
        public List<SelectListItem> ListOfLevels { get; set; }
    }

    public partial class Courses
    {
        public List<SelectListItem> ListOfCourses { get; set; }
    }

    public class SelectedPaper
    {
        public int CourseId { get; set; }
        public int paperId { get; set; }
        public int levelId { get; set; }
    }

    public class SelectOption
    {
        public int QId { get; set; }
        public int SelectedOption { get; set; }
        public int duration { get; set; }
        public string Comprehensive { get; set; }
    }

    public partial class GetQuestions_Result
    {
        public List<choices> options { get; set; }
    }

    public class choices
    {
        public string Comprehensive { get; set; }
        public Nullable<int> choiceId { get; set; }
        public string choiceText { get; set; }
    }

    public class getscores
    {
        public int ReportId { get; set; }
        public int LogId { get; set; }
        public string userName { get; set; }
        public string paperName { get; set; }
        public string CourseName { get; set; }
        public string LevelName { get; set; }
        public int score { get; set; }
        public bool isRevalidate { get; set; }
    }

    public partial class StudentTestLog
    {
        public bool isCorrect { get; set; }
    }

    public class setQuestions
    {
        public int CourseId { get; set; }
        public int PaperId { get; set; }
        public int Level { get; set; }
        public string Question { get; set; }
        public Nullable<int> Duration { get; set; }
        public int OptionType { get; set; }

        public List<SetQuestionChoices> choices { get; set; }
    }

    public class SetQuestionChoices
    {
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string Option5 { get; set; }
    }

    public class courseSelectedTaken
    {
        public Guid TestId { get; set; }
        public string CourseName { get; set; }
        public string PaperName { get; set; }
        public string Level { get; set; }
        public bool isCorrected { get; set; }
    }

    public class courseChart
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int total { get; set; }
    }

    public class ShowAnswersList
    {
        public string Question { get; set; }
        public string CorrectOption { get; set; }
        public string SelectedOption { get; set; }
        public int Score { get; set; }
    }

    public class AssignCourseList
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int TestType { get; set; }
    }
}