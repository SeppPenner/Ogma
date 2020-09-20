using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data.Models;
using Ogma3.Pages.Shared;

namespace Ogma3.Data.Repositories
{
    public class BlogpostsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BlogpostsRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<Blogpost>> GetPaginated(int page, int perPage)
        {
            return await _context.Blogposts
                .Paginate(page, perPage)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountBlogposts()
        {
            return await _context.Blogposts.CountAsync();
        }

        public async Task<ICollection<BlogpostCard>> GetPaginatedCardsForUser(string userName, int page, int perPage, bool publishedOnly = true)
        {
            return await _context.Blogposts
                .Where(b => b.Author.NormalizedUserName == userName.Normalize().ToUpper())
                .Where(b => b.IsPublished || !publishedOnly)
                .Paginate(page, perPage)
                .ProjectTo<BlogpostCard>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountForUser(string userName, bool publishedOnly = true)
        {
            return await _context.Blogposts
                .Where(b => b.Author.NormalizedUserName == userName.Normalize().ToUpper())
                .Where(b => b.IsPublished || !publishedOnly)
                .CountAsync();
        }
        
        public async Task<bool> TogglePublishedStatus(long id)
        {
            var story = await _context.Blogposts
                .FindAsync(id);
            story.IsPublished = !story.IsPublished;

            await _context.SaveChangesAsync();

            return story.IsPublished;
        }

        public async Task<BlogpostDetails> Get(long id)
        {
            return await _context.Blogposts
                .Where(b => b.Id == id)
                .ProjectTo<BlogpostDetails>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}