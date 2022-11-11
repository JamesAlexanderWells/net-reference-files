using ProjectManagerApi.Configuration;
using ProjectManagerApi.Models;
using Microsoft.Extensions.Options;
using ProjectManagerApi.Extensions;
using Neo4j.Driver;
using Npgsql;

namespace ProjectManagerApi.Data
{
    public interface IIssueRepository
    {
        Task<Issue> GetIssueById(string id);

        Task<IEnumerable<Issue>> GetIssues();

        Task<Issue> CreateIssue(Issue issue);

        Task DeleteIssue(string id);

        Task<Issue> UpdateIssue(string id, Issue issue);
    }

    public class IssueRepository : IIssueRepository
    {
        private readonly DatabaseOptions _connectionStringOptions;

        private readonly IDriver _graphDriver;
        private readonly INoteRepository _noteRepository;

        public IssueRepository(IOptions<DatabaseOptions> connectionStringOptions, INoteRepository noteRepository)
        {
            _connectionStringOptions = connectionStringOptions.Value;
            _noteRepository = noteRepository;

            _graphDriver = GraphDatabase.Driver(_connectionStringOptions.Neo4jUri,
                AuthTokens.Basic(_connectionStringOptions.Neo4jUser, _connectionStringOptions.Neo4jPassword));
        }

        public async Task<Issue> GetIssueById(string id)
        {

            var query = @"
                MATCH (p:Issue)
                WHERE p.id = $id
                RETURN p";

            var session = _graphDriver.AsyncSession();

            try
            {
                var action = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new { id });
                    return await result.SingleAsync();
                });
                return action.ReadIssue(await _noteRepository.CheckIfNoteIsAttached(id));
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<Issue> CreateIssue(Issue issue)
        {
            var query = @"
                CREATE(p: Issue $issue)
                SET p.id = apoc.create.uuid()
                RETURN p";

            var createdDate = DateTimeOffset.Now;
            var session = _graphDriver.AsyncSession();

            try
            {
                var result = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new
                    {
                        issue = new
                        {
                            id = issue.Id,
                            name = issue.Name,
                            description = issue.Description,
                            priority = issue.Priority,
                            category = issue.Category,
                            status = issue.Status,
                            actionsToResolve = issue.ActionsToResolve,
                            owner = issue.Owner,
                            escalateTo = issue.EscalateTo,
                            escalationStatus = issue.EscalationStatus,
                            latestUpdate = issue.LatestUpdate,
                            dateRaised = issue.DateRaised,
                            targetResolutionDate = issue.TargetResolutionDate,
                            dateResolved = issue.DateResolved,
                            lastReviewDate = issue.LastReviewDate,
                            nextReviewDate = issue.NextReviewDate,
                        }
                    });
                    return await result.SingleAsync();
                });

                return result.ReadIssue();
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task DeleteIssue(string id)
        {
            var query = @"
                MATCH (p:Issue {id: $id})
                DETACH DELETE p";

            var session = _graphDriver.AsyncSession();

            try
            {
                await session.RunAsync(query, new { id });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<Issue>> GetIssues()
        {
            var query = @"
                MATCH (p:Issue)
                RETURN p";

            var session = _graphDriver.AsyncSession();

            try
            {
                var decisions = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query);
                    return await result.ToListAsync();
                });
                return await Task.WhenAll(decisions.Select(async p => p.ReadIssue(await _noteRepository.CheckIfNoteIsAttached(p[0].As<INode>()["id"].As<string>()))));
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<Issue> UpdateIssue(string id, Issue issue)
        {
            var query = @"
                MATCH (p:Issue {id: $id})
                SET p+= $issue
                RETURN p";

            var session = _graphDriver.AsyncSession();

            try
            {
                var result = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new
                    {
                        id,
                        issue = new
                        {
                            id = issue.Id,
                            name = issue.Name,
                            description = issue.Description,
                            priority = issue.Priority,
                            category = issue.Category,
                            status = issue.Status,
                            actionsToResolve = issue.ActionsToResolve,
                            owner = issue.Owner,
                            escalateTo = issue.EscalateTo,
                            escalationStatus = issue.EscalationStatus,
                            latestUpdate = issue.LatestUpdate,
                            dateRaised = issue.DateRaised,
                            targetResolutionDate = issue.TargetResolutionDate,
                            dateResolved = issue.DateResolved,
                            lastReviewDate = issue.LastReviewDate,
                            nextReviewDate = issue.NextReviewDate,
                        }
                    });
                    return await result.SingleAsync();
                });
                return result.ReadIssue(await _noteRepository.CheckIfNoteIsAttached(id));
            }
            finally
            {
                await session.CloseAsync();
            }
        }
    }
}