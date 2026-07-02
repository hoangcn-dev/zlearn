using System;
using System.Collections.Generic;
using System.Linq;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.CatalogContext.Questions;
using Zlearn.V2.Domain.CatalogContext.Tags;
using Zlearn.V2.Domain.CatalogContext.Answers;
using Zlearn.V2.Domain.CatalogContext.Quizzes.Events;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Quizzes
{
    public class Quiz : AggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public int DownloadCount { get; set; }
        public bool IsPublic { get; set; }

        public Category? Category { get; set; }
        public List<Question> Questions { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();

        public Quiz() { }

        public Quiz(
            string quizId,
            string name,
            string slug,
            string categoryId,
            bool isPublic)
        {
            Id = quizId;
            Name = name;
            Slug = slug;
            CategoryId = categoryId;
            IsPublic = isPublic;
            DownloadCount = 0;
        }

        public void Update(
            string name,
            string slug,
            string categoryId,
            bool isPublic)
        {
            Name = name;
            Slug = slug;
            CategoryId = categoryId;
            IsPublic = isPublic;
        }

        public void PublishCreateEvent(string categoryName, string categorySlug)
        {
            var questionPayloads = Questions.Select(q => new QuestionPayload(
                q.Id,
                q.Order,
                q.Slug,
                q.StringContent,
                q.MediaFileUrls,
                q.Explanation,
                q.Answers.Select(a => new AnswerPayload(
                    a.Id,
                    a.Key,
                    a.StringContent,
                    a.MediaFileUrls,
                    a.IsCorrect
                )).ToList()
            )).ToList();

            var tagNames = Tags.Select(t => t.Name).ToList();

            RaiseEvent(new QuizCreatedEvent(Id, Name, Slug, CategoryId, categoryName, categorySlug, IsPublic, questionPayloads, tagNames));
        }

        public void PublishUpdateEvent(string categoryName, string categorySlug)
        {
            var questionPayloads = Questions.Select(q => new QuestionPayload(
                q.Id,
                q.Order,
                q.Slug,
                q.StringContent,
                q.MediaFileUrls,
                q.Explanation,
                q.Answers.Select(a => new AnswerPayload(
                    a.Id,
                    a.Key,
                    a.StringContent,
                    a.MediaFileUrls,
                    a.IsCorrect
                )).ToList()
            )).ToList();

            var tagNames = Tags.Select(t => t.Name).ToList();

            RaiseEvent(new QuizUpdatedEvent(Id, Name, Slug, CategoryId, categoryName, categorySlug, IsPublic, questionPayloads, tagNames));
        }

        public void Delete()
        {
            RaiseEvent(new QuizDeletedEvent(Id));
        }

        public void AddQuestion(Question question)
        {
            Questions.Add(question);
            RaiseEvent(new Questions.Events.QuestionCreatedEvent(question.Id, question.Slug, question.StringContent, question.Order, Id));
            
            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    RaiseEvent(new Answers.Events.AnswerCreatedEvent(answer.Id, answer.Key, answer.StringContent, answer.IsCorrect, question.Id));
                }
            }
        }

        public void RemoveQuestion(Question question)
        {
            Questions.Remove(question);
            RaiseEvent(new Questions.Events.QuestionDeletedEvent(question.Id));
            
            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    RaiseEvent(new Answers.Events.AnswerDeletedEvent(answer.Id));
                }
            }
        }

        public void UpdateQuestionsAndAnswers(List<Question> newQuestions, List<Question> deletedQuestions, List<Answer> deletedAnswers)
        {
            // 1. Process deleted answers
            foreach (var a in deletedAnswers)
            {
                RaiseEvent(new Answers.Events.AnswerDeletedEvent(a.Id));
            }

            // 2. Process deleted questions
            foreach (var q in deletedQuestions)
            {
                Questions.Remove(q);
                RaiseEvent(new Questions.Events.QuestionDeletedEvent(q.Id));
                if (q.Answers != null)
                {
                    foreach (var a in q.Answers)
                    {
                        RaiseEvent(new Answers.Events.AnswerDeletedEvent(a.Id));
                    }
                }
            }

            // 3. Process new questions
            foreach (var q in newQuestions)
            {
                Questions.Add(q);
                RaiseEvent(new Questions.Events.QuestionCreatedEvent(q.Id, q.Slug, q.StringContent, q.Order, Id));
                if (q.Answers != null)
                {
                    foreach (var a in q.Answers)
                    {
                        RaiseEvent(new Answers.Events.AnswerCreatedEvent(a.Id, a.Key, a.StringContent, a.IsCorrect, q.Id));
                    }
                }
            }
        }

        public void AddAnswerToQuestion(string questionId, Answer answer)
        {
            var q = Questions.FirstOrDefault(x => x.Id == questionId);
            if (q != null)
            {
                q.Answers.Add(answer);
                RaiseEvent(new Answers.Events.AnswerCreatedEvent(answer.Id, answer.Key, answer.StringContent, answer.IsCorrect, questionId));
            }
        }

        public void AddTag(Tag tag)
        {
            if (!Tags.Contains(tag))
            {
                Tags.Add(tag);
            }
        }

        public void RemoveTag(Tag tag)
        {
            Tags.Remove(tag);
        }
    }
}
