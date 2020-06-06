using AutoMapper;
using BugTracker_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker_API
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Issue, GetIssueDto>()
                .ForMember(d => d.Comments, opt => opt.MapFrom(src => src.Comments.Count()));
            CreateMap<PostIssueDto, Issue>();
            CreateMap<PutIssueDto, Issue>();

            CreateMap<Comment, GetCommentDto>();
            CreateMap<PostCommentDto, Comment>();
            CreateMap<PutCommentDto, Comment>();
        }
    }
}
