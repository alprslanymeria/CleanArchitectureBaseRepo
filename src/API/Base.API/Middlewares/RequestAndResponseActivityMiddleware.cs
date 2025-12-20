using Microsoft.IO;
using System.Diagnostics;

namespace Base.API.Middlewares;

// WHEN REQUEST CAME TO API, THE PART BEFORE THE NEXT() CALL IS EXECUTED
// WHEN NEXT() CALL IS EXECUTED, THE REQUEST IS PASSED TO THE NEXT MIDDLEWARE IN THE PIPELINE
// WHEN RESPONSE RETURNS FROM THE API, THE PART AFTER THE NEXT() CALL IS EXECUTED

public class RequestAndResponseActivityMiddleware(RequestDelegate next)
{
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager = new();

    public async Task InvokeAsync(HttpContext context)
    {
        // IF NO CURRENT ACTIVITY RETURN
        if (Activity.Current is null)
        {
            await next(context);
            return;
        }

        await AddRequestBodyContentToActivityTags(context);
        await AddResponseBodyContentToActivityTags(context);
    }

    private static async Task AddRequestBodyContentToActivityTags(HttpContext context)
    {
        // IN HERE WE MADE THE REQUEST IS READABLE MULTIPLE TIMES.
        // BECAUSE THE REQUEST IS A STREAM AND ONCE IT IS READ, IT CANNOT BE READ AGAIN UNLESS WE ENABLE BUFFERING.
        // IF WE NOT ENABLED, THE NEXT MIDDLEWARE OR THE CONTROLLER CANNOT READ THE REQUEST BODY.
        // AFTER READING THE BODY, THE STREAM'S POSITION IS RESET TO 0 SO THAT IT CAN BE READ AGAIN.
        context.Request.EnableBuffering();

        // IF REQUEST IS EMPTY, TAG ACCORDINGLY
        if (context.Request.ContentLength is long len && len == 0)
        {
            Activity.Current?.SetTag("http.request.body", string.Empty);
            return;
        }

        // RESET POSITION
        context.Request.Body.Position = 0;

        // READ REQUEST BODY
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);

        // TAG THE REQUEST BODY CONTENT TO THE CURRENT ACTIVITY
        Activity.Current?.SetTag("http.request.body", content);

        // RESET POSITION AGAIN FOR THE NEXT MIDDLEWARE TO READ
        context.Request.Body.Position = 0;
    }

    private async Task AddResponseBodyContentToActivityTags(HttpContext context)
    {
        // GET ORIGINAL STREAM
        var originalResponseBody = context.Response.Body;

        // CREATE MEMORY STREAM
        await using var buffer = _recyclableMemoryStreamManager.GetStream();

        // NORMALLY, THE RESPONSE STREAM IS WRITTEN TO CONTROLLER OR NEXT MIDDLEWARE.
        // IN HERE WE REPLACE IT WITH OUR MEMORY STREAM TO CAPTURE THE RESPONSE.
        context.Response.Body = buffer;

        try
        {
            await next(context).ConfigureAwait(false);

            // RESET POSITION
            buffer.Position = 0;

            // READ RESPONSE BODY
            using var reader = new StreamReader(buffer, leaveOpen: true);
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);

            // TAG THE RESPONSE BODY CONTENT TO THE CURRENT ACTIVITY
            Activity.Current?.SetTag("http.response.body", content);

            // REWIND AND COPY THE BUFFERED RESPONSE BACK TO THE ORIGINAL STREAM
            buffer.Position = 0;
            await buffer.CopyToAsync(originalResponseBody).ConfigureAwait(false);
        }
        finally
        {
            // RESTORE ORIGINAL RESPONSE BODY STREAM NO MATTER WHAT
            context.Response.Body = originalResponseBody;
        }
    }
}