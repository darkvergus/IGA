using Domain.Core.Entities;
using Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController, Route("api/job-stream")]
public sealed class JobStreamController(JobService jobs) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task Stream(long id, CancellationToken cancellationToken)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        string? last = null;

        while (!cancellationToken.IsCancellationRequested && !HttpContext.RequestAborted.IsCancellationRequested)
        {
            Job? job = await jobs.GetAsync(id, cancellationToken);
            string now = job?.Status.ToString() ?? "Unknown";

            if (now != last)
            {
                await Response.WriteAsync($"data: {now}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
                last = now;

                if (now is "Completed" or "Failed")
                {
                    break;
                }
            }

            await Task.Delay(250, cancellationToken);
        }
    }
}