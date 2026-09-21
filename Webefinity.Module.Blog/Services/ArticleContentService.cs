using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Webefinity.Module.Blog.Data;
using Webefinity.Module.Blog.Models;
using Webefinity.Module.Blog.Options;
using Webefinity.Module.Scheduler.Interfaces;

namespace Webefinity.Module.Blog.Services
{
    public enum ResultType
    {
        NotFound,
        Found
    }

    public record ArticleContentResult(ResultType Result, ArticleFrontmatter Properties, string Content);
    public record ArticleEditResult(ResultType Result, ArticleEditModel ArticleModel);
    public record ArticleSaveResult(bool Success, string NewSlug, string ErrorMessage);
    public class ArticleContentService(IBlogDbContext blogDbContext, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, IOptions<BlogOptions> options, IJobManualTrigger jobManualTrigger)
    {
        public async Task<ArticleContentResult> GetArticleContentAsync(string articleId, CancellationToken stoppingToken)
        {
            var article = await blogDbContext.Articles.FindAsync(articleId, stoppingToken);
            if (article == null)
            {
                return new ArticleContentResult(ResultType.NotFound, new ArticleFrontmatter(), string.Empty);
            }

            var frontmatter = new ArticleFrontmatter
            {
                Id = article.Id,
                Title = article.Title,
                Author = article.Author,
                Date = article.Date,
                Tags = article.Tags?.Select(t => t.Tag).ToArray() ?? Array.Empty<string>(),
                Image = article.Image,
                Summary = article.Summary,
                State = article.State
            };
            return new ArticleContentResult(ResultType.Found, frontmatter, article.Content ?? string.Empty);
        }

        public async Task<ArticleEditResult> GetArticleForEditAsync(string slug, CancellationToken stoppingToken)
        {
            foreach (var policy in options.Value.AuthorSecurityPolicies)
            {
                var authResult = await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext?.User, policy);
                if (!authResult.Succeeded)
                {
                    return new ArticleEditResult(ResultType.NotFound, new ArticleEditModel());
                }
            }

            var article = await blogDbContext.Articles.FindAsync(slug, stoppingToken);
            if (article is null)
            {
                return new ArticleEditResult(ResultType.Found, new ArticleEditModel() { Id = null, NewId = slug, Date = DateTimeOffset.UtcNow, Author = httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Unknown", State = ArticleState.Draft });
            }

            var articleModel = new ArticleEditModel
            {
                Id = article.Id,
                NewId = article.Id,
                Title = article.Title,
                Author = article.Author,
                Date = article.Date,
                Tags = article.TagList ?? string.Empty,
                Image = article.Image,
                Summary = article.Summary,
                Content = article.Content ?? string.Empty,
                State = article.State
            };
            return new ArticleEditResult(ResultType.Found, articleModel);
        }

        public async Task<ArticleSaveResult> SaveArticleEditsAsync(ArticleEditModel editModel, CancellationToken stoppingToken)
        {
            foreach (var policy in options.Value.AuthorSecurityPolicies)
            {
                var authResult = await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext?.User, policy);
                if (!authResult.Succeeded)
                {
                    return new ArticleSaveResult(false, editModel.Id, "User is not authorized to save article edits.");
                }
            }
            var modelValidator = new ArticleEditModelValidator();
            var validationResult = await modelValidator.ValidateAsync(editModel, stoppingToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ArticleSaveResult(false, editModel.Id, $"Validation failed: {errorMessages}");
            }

            var article = await blogDbContext.Articles.FindAsync(editModel.Id, stoppingToken);
            if (article is null && !String.IsNullOrWhiteSpace(editModel.Id))
            {
                return new ArticleSaveResult(false, editModel.Id, "Article not found.");
            }

            if (editModel.Id != editModel.NewId)
            {
                var existingArticleWithNewId = await blogDbContext.Articles.FindAsync(editModel.NewId, stoppingToken);
                if (existingArticleWithNewId != null)
                {
                    return new ArticleSaveResult(false, editModel.NewId, $"Another article with the ID {editModel.NewId} already exists.");
                }
            }

            if (article is null && String.IsNullOrWhiteSpace(editModel.Id))
            {
                article = new BlogArticle { Id = editModel.NewId };
                blogDbContext.Articles.Add(article);
            }
            else if (article.Id != editModel.NewId)
            {
                blogDbContext.Articles.Remove(article);
                article = new BlogArticle { Id = editModel.NewId };
                blogDbContext.Articles.Add(article);
            }
            
            article.Title = editModel.Title;
            article.Author = editModel.Author;
            article.Date = editModel.Date;
            article.TagList = editModel.Tags;
            article.Image = editModel.Image;
            article.Summary = editModel.Summary;
            article.Content = editModel.Content;
            article.Image = editModel.Image;
            article.State = editModel.State;

            await blogDbContext.SaveChangesAsync(stoppingToken);

            jobManualTrigger.TriggerJob(Constants.BlobReindexTrigger);
            return new ArticleSaveResult(true, editModel.NewId, "Article edits saved successfully.");
        }
    }
}
