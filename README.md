# job-progress-signalr-demo
Messing around with sending Hangfire job progress to the front-end via SSE / WebSockets

## Initial Pass with SSE
The intial pass was to kick off the job with the `/merge/merge-customers` endpoint, then the front-end would listen for server-sent events (SSE) through the `/merge/merge-status` endpoint. The issue with this is that the SSE need to be sent via HTTP, separate from the job itself, so the front-end is just listening to this endpoint, and every second the endpoint is checking the job DB record for updates and sending them to the front-end. This is hitting the DB every second to check for updates, which isn't ideal, but it works.

## Second Pass with SignalR/WebSockets
To avoid hitting the DB repeatedly and just send events directly when individual customer merges happen, I tried an approach with SignalR and WebSockets. When a job is kicked off, the front-end sets up a SignalR connection through the `/mergeHub` endpoint that is registered in `Startup`. Then it joins a group using the `sessionId` returned from the endpoint that kicks off the job. This way we can start multiple jobs (and SignalR connections) and update the progress accordingly as events come in. In the job, we just send updates to the front-end by calling `await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveMergeUpdate", mergeJob);`. This will send the update to all connections in the group associated with the session ID of the job.

## Screenshot

In the image below, we can kick off multiple jobs at once, which opens a SignalR connection for each one and listens for updates. As updates come in, it updates the progress to display to the user. It's currently set up to just run one job at a time, so any others that are kicked off will just display as queued until the job starts.

![image](https://github.com/user-attachments/assets/896df5de-7b49-47c0-8e70-1df035b7a58f)


