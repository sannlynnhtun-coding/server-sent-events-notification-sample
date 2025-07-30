using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace ServerSentEventsNotification.ApiSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly Channel<string> _channel;

        public AnnouncementsController(Channel<string> channel)
        {
            _channel = channel;
        }

        // POST: /api/announcements
        [HttpPost]
        public async Task<IActionResult> PostAnnouncement([FromBody] string message)
        {
            // Write the message to the channel for all listeners
            await _channel.Writer.WriteAsync(message);
            return Accepted();
        }

        // GET: /api/announcements/stream
        [HttpGet("stream")]
        public async Task StreamAnnouncements(CancellationToken cancellationToken)
        {
            // Set the response headers for SSE
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            // Read from the channel and write to the response stream
            await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    // Format the message as an SSE event
                    await Response.WriteAsync($"event: announcement\n", cancellationToken);
                    await Response.WriteAsync($"data: {message}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Client closed the connection, which is expected.
                    break;
                }
            }
        }
    }
}
