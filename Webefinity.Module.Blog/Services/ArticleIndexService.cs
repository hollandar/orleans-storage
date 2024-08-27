using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Webefinity.ContentRoot;
using Webefinity.Module.Blog.Data;
using Webefinity.Module.Blog.Models;

namespace Webefinity.Module.Blog.Services
{
    public record ArticleRef(string Name);

    public class ArticleIndexService
    {
        private readonly BlogDbContext blogDbContext;

        public ArticleIndexService(BlogDbContext blogDbContext)
        {
            this.blogDbContext = blogDbContext;
        }

        public Task<IEnumerable<ArticleFrontmatter>> ListArticlesAsync(int page = 0, int pageSize = 10, string? search = null, string? tag = null)
        {
            HashSet<string> searchArticleIds = new HashSet<string>();
            var searching = !string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(tag);
            if (!String.IsNullOrWhiteSpace(search))
            {
                var words = search.ToLower().Split(' ').ToHashSet();
                searchArticleIds = blogDbContext.Words.Where(r => words.Contains(r.Word)).Select(r => r.ArticleId).Distinct().ToHashSet();
            }

            HashSet<string> tagArticleIds = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(tag))
            {
                tagArticleIds = blogDbContext.Tags.Where(r => r.Tag == tag).Select(r => r.ArticleId).Distinct().ToHashSet();
            }

            var enumerable = blogDbContext.Articles.Include(r => r.Tags).AsNoTracking();
            var fullArticles = enumerable.AsQueryable();
            HashSet<string> articleIds = new HashSet<string>();

            if (searching)
            {
                if (searchArticleIds.Count == 0) articleIds = tagArticleIds;
                else if (tagArticleIds.Count == 0) articleIds = searchArticleIds;
                else articleIds = searchArticleIds.Intersect(tagArticleIds).ToHashSet();

                fullArticles = fullArticles.Where(r => articleIds.Contains(r.Id));
            }

            var articles = fullArticles.Skip(page * pageSize).Take(pageSize);

            List<ArticleFrontmatter> result = new List<ArticleFrontmatter>();
            foreach (var articleDb in articles)
            {
                var frontmatter = new ArticleFrontmatter
                {
                    Id = articleDb.Id,
                    Title = articleDb.Title,
                    Author = articleDb.Author,
                    Date = articleDb.Date,
                    Image = articleDb.Image,
                    Summary = articleDb.Summary,
                    Tags = articleDb.Tags.Select(r => r.Tag).ToArray()
                };
                result.Add(frontmatter);
            }

            return Task.FromResult<IEnumerable<ArticleFrontmatter>>(result);
        }

        public Task<List<string>> ListTagsAsync() { 
            return blogDbContext.Tags.Select(r => r.Tag).Distinct().ToListAsync();
        }

    }
}
