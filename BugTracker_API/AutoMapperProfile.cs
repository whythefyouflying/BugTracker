////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\AutoMapperProfile.cs </file>
///
/// <copyright file="AutoMapperProfile.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the automatic mapper profile class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using AutoMapper;
using BugTracker_API.Models;
using System.Linq;

namespace BugTracker_API
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An automatic mapper profile. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class AutoMapperProfile : Profile
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Default constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public AutoMapperProfile()
        {
            CreateMap<Project, GetProjectDto>()
                .ForMember(d => d.Issues, opt => opt.MapFrom(src => src.Issues.Count()));
            CreateMap<PostProjectDto, Project>();
            CreateMap<PutProjectDto, Project>();

            CreateMap<Issue, GetIssueDto>()
                .ForMember(d => d.Comments, opt => opt.MapFrom(src => src.Comments.Count()));
            CreateMap<PostIssueDto, Issue>();
            CreateMap<PutIssueDto, Issue>();

            CreateMap<Comment, GetCommentDto>();
            CreateMap<PostCommentDto, Comment>();
            CreateMap<PutCommentDto, Comment>();

            CreateMap<User, UserDto>();
        }
    }
}
