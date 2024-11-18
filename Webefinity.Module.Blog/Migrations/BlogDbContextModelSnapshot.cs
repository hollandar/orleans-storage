﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Webefinity.Module.Blog.Data;

#nullable disable

namespace Webefinity.Module.Blog.Migrations
{
    [DbContext(typeof(BlogDbContext))]
    partial class BlogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("Webefinity.Module.Blog.Data.BlogArticle", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<string>("Summary")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("BlogArticles", (string)null);
                });

            modelBuilder.Entity("Webefinity.Module.Blog.Data.BlogTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ArticleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ArticleId");

                    b.HasIndex("Tag");

                    b.ToTable("BlogTags", (string)null);
                });

            modelBuilder.Entity("Webefinity.Module.Blog.Data.BlogWord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ArticleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Word")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ArticleId");

                    b.HasIndex("Word");

                    b.ToTable("BlogWords", (string)null);
                });

            modelBuilder.Entity("Webefinity.Module.Blog.Data.BlogTag", b =>
                {
                    b.HasOne("Webefinity.Module.Blog.Data.BlogArticle", "Article")
                        .WithMany("Tags")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Article");
                });

            modelBuilder.Entity("Webefinity.Module.Blog.Data.BlogWord", b =>
                {
                    b.HasOne("Webefinity.Module.Blog.Data.BlogArticle", "Article")
                        .WithMany("Words")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Article");
                });

            modelBuilder.Entity("Webefinity.Module.Blog.Data.BlogArticle", b =>
                {
                    b.Navigation("Tags");

                    b.Navigation("Words");
                });
#pragma warning restore 612, 618
        }
    }
}