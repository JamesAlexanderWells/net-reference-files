using ProjectManagerApi.Data;
using ProjectManagerApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Authorization;

namespace ProjectManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController : ControllerBase
    {
        private readonly IIssueRepository _issueRepository;

        public IssueController(IIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        [HttpGet]
        // [Authorize(Policy = "SuperAdmin")]
        public async Task<IEnumerable<Issue>> Get() => await _issueRepository.GetIssues();

        [HttpGet("{id}")]
        public async Task<Issue> Get(string id) => await _issueRepository.GetIssueById(id);

        [HttpPost]
        public async Task<Issue> Post([FromBody] Issue issue) => await _issueRepository.CreateIssue(issue);

        [HttpPut("{id}")]
        public async Task<Issue> Put(string id, [FromBody] Issue issue) => await _issueRepository.UpdateIssue(id, issue);

        [HttpDelete("{id}")]
        public async void Delete(string id) => await _issueRepository.DeleteIssue(id);
    }
}
