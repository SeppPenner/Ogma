using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data;
using Ogma3.Data.DTOs;
using Ogma3.Data.Models;

namespace Ogma3.Api.V1
{
    [Route("api/[controller]", Name = nameof(VotesController))]
    [ApiController]
    public class VotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OgmaUserManager _userManager;


        public VotesController(ApplicationDbContext context, OgmaUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET api/votes/5
        [HttpGet("{poolId}")]
        public async Task<CountReturn> GetVotes(long poolId)
        {
            var user = await _userManager.GetUserAsync(User);
            var count = await _context.Votes
                .AsNoTracking()
                .CountAsync(v => v.VotePoolId == poolId);
            var didUserVote = await _context.Votes
                .AsNoTracking()
                .AnyAsync(v => v.User == user && v.VotePoolId == poolId);

            return new CountReturn
            {
                Count = count,
                DidVote = didUserVote
            };
        }
        
        // POST api/votes
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> PostVote(VoteFromApiDTO data)
        {
            var user = await _userManager.GetUserAsync(User);
            var pool = await _context.VotePools
                .FindAsync(data.VotePool);
            var vote = await _context.Votes
                .FirstOrDefaultAsync(v => v.User == user && v.VotePoolId == data.VotePool);
            var didVote = false;

            // Check if the vote already exists
            if (vote == null)
            {
                pool.Votes.Add(new Vote {
                    User = user
                });
                didVote = true;
            }
            else
            {
                pool.Votes.Remove(vote);
            }

            // Save
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500);
            }

            var res = new CountReturn
            {
                Count = pool.Votes.Count,
                DidVote = didVote
            };
            return new OkObjectResult(res);
        }

        public class CountReturn
        {
            public int Count { get; set; }
            public bool DidVote { get; set; }
        }
    }
}