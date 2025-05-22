using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Index.Data;
using Webefinity.ContentRoot.Index.Services;
using Webefinity.ContentRoot.Index.Handlers;
using Webefinity.ContentRoot.IndexUI.Interfaces;

namespace Webefinity.ContentRoot.Index;

public static class StartupExtensions
{
    public static void AddFileIndexModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(options =>
        {
            options.HasKey(r => r.Id);
            options.HasIndex(r => new { r.Key, r.Collection, r.FileName });
            options.HasMany(r => r.Metadata).WithOne(r => r.FileMetadata).HasForeignKey(r => r.FileMetadataId);
        });

        modelBuilder.Entity<Metadata>(options =>
        {
            options.HasKey(r => r.Id);
            options.HasIndex(r => r.Key);
        });
    }

    public static void AddFileMetadataDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
    {
        services.AddDbContext<FileMetadataDbContext>(options);
        services.AddScoped<IFileMetadataDbContext>(sp => sp.GetRequiredService<FileMetadataDbContext>());
    }

    public static void AddFileMetadataDbContext<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddScoped<IFileMetadataDbContext>((sp) => new FileMetadataDbContextChild<TDbContext>(sp.GetRequiredService<TDbContext>()));
    }

    public static void AddIndexedContentRoot(this IServiceCollection services, string? key = null)
    {
        if (key is null)
        {
            services.AddScoped<IIndexedContentRootLibrary>((sp) => { 
                var contentRoot = sp.GetRequiredService<IContentRootLibrary>();
                var dbContext = sp.GetRequiredService<IFileMetadataDbContext>();
                return new IndexedContentRootService(contentRoot, dbContext, "Default");
            });
        } 
        else
        {
            services.AddKeyedScoped<IIndexedContentRootLibrary>(key, (sp, k) => {
                var contentRoot = sp.GetRequiredKeyedService<IContentRootLibrary>(key);
                var dbContext = sp.GetRequiredService<IFileMetadataDbContext>();
                return new IndexedContentRootService(contentRoot, dbContext, key);
            });
        }
        services.AddScoped<IFileBrowserService, FileBrowserService>();

    }

    public static void MapIndexedContentUI(this IEndpointRouteBuilder app, 
        string browserPolicy = "IC_Browser",
        string adminPolicy = "IC_Admin", 
        string? key = null)
    {
        var indexedContentRootKey = key;
        var getListUri = $"/api/icr/{key}/{{collection}}/list/{{search?}}";
        app.MapGet(getListUri, (IServiceProvider sp, string collection, string? search, [FromQuery] int skip = 0, [FromQuery] int take = 10) =>
            IndexedContentHandler.GetList(sp, collection, search, skip, take, indexedContentRootKey)
        ).RequireAuthorization(browserPolicy);

        var postFileUri = $"/api/icr/{key}/{{collection}}/upload";
        app.MapPost(postFileUri, (IServiceProvider sp, string collection, [FromBody] Stream content, [FromHeader(Name = "Content-Type")] string contentType, [FromHeader(Name = "X-FileName")] string fileName) =>
            IndexedContentHandler.PostFile(sp, collection, content, contentType, fileName, indexedContentRootKey)
        ).RequireAuthorization(browserPolicy);

        var getFileUri = $"/icr/{key}/{{collection}}/{{filename}}";
        app.MapGet(getFileUri, (IServiceProvider sp, HttpResponse response, string collection, string filename) =>
            IndexedContentHandler.GetFile(sp, response, collection, filename, indexedContentRootKey)
        ).RequireAuthorization(browserPolicy);

        var getFileMetaUri = $"/api/icr/{key}/{{collection}}/meta/{{filenameorid}}";
        app.MapGet(getFileMetaUri, (IServiceProvider sp, HttpResponse response, string collection, string filenameorid) =>
            IndexedContentHandler.GetFileMeta(sp, response, collection, filenameorid, indexedContentRootKey)
        ).RequireAuthorization(browserPolicy);

        var deleteFileUri = $"/api/icr/{key}/{{collection}}/{{filename}}";
        app.MapDelete(deleteFileUri, (IServiceProvider sp, string collection, string filename) =>
            IndexedContentHandler.DeleteFile(sp, collection, filename, indexedContentRootKey)
        ).RequireAuthorization(browserPolicy);
    }
}
