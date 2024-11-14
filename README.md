# Jack Henry Sr. Software Engineer Coding Project

## Starting the Project

This app uses dotnet 7.0 to run a local ASP.NET service.

The project requires a `.env` file in the root directory with the appropriate variables set. An example may be found via `.env.example`. For those with the keyphrase, there is a Makefile command that will decrypt the `.env.gpg` file:

```sh
make reveal-env
```

Starting the project should require the following steps:

1. `dotnet restore ./src/jh.csproj`
1. `dotnet build ./src/jh.csproj`
1. `dotnet ./src/bin/Debug/net7.0/jh.dll`

## Using the project

This app uses a REST API interface to initiate subreddit monitoring as well as for querying statistics.

| Operation           | Method | Endpoint                                     | Body                      |
| ------------------- | ------ | -------------------------------------------- | ------------------------- |
| Watch new Subreddit | POST   | `localhost:5000/Reddit`                      | `{ "Subreddit": "cats" }` |
| Get top post        | GET    | `localhost:5000/Reddit/:subreddit/posts/top` | -                         |
| Get top user        | GET    | `localhost:5000/Reddit/:subreddit/users/top` | -                         |

## Considerations

There are some issues with the current implementation.

1. There is a race condition when the same user posts across subreddits. This appears to be related to the use of an in-memory database and should not present the same issue in a live database.
1. The time to throttle between calls is mathematically sound, but does not account for time to operate. As a result, the throttling time progressively shrinks between time segments, resetting once on a new segment. Given more time, an optimal solution might be to schedule out tasks as opposed to delaying them.

## Requirements Assumptions

There are two main requirements for the statistics to be returned.

1. Posts with the most upvotes.
1. Users with the most posts.

I will summarize my assumptions here, with elaboration in the following sections:

1. Posts with the most upvotes.
   - "Hot" posts. These are recent posts that have gained a notable amount of upvotes in a short amount of time.
1. Users with the most posts.
   - A live evaluation of incoming posts during the time the app is active.

### Assumptions - Posts with most upvotes

There are several ways to determine posts with the most upvotes:

1. Maximum upvotes across a period of time. This period of time may be across an hour,, day, week, month, year, or all time.
1. A manual observation and monitoring of posts that are created within a set period of time.
1. "Hot" posts. These are recent posts that have gained a notable amount of upvotes in a short amount of time.

In regards to the first point, the "top" section is generally static, and we do not expect much variance in that metric. This seems to be an unlikely interpretation, as it results in static data, when the goal is to render a set of changing data over time.

The second point could make for an interesting statistic; to observe posts as they come in, and to determine which posts excel over time. However, a subreddit with regular traffic can expect hundreds of posts an hour. Reddit's rate limiting is judicious, and monitoring new posts over time results in exponentially increasing API calls.

The final point seems the most likely interpretation of the requirements. The "hot" feed is considered to be the "default" view for any given subreddit. The "hot" feed serves as a single endpoint that may be used to determine the "hottest" recent posts. This could make for interesting and provacative metrics.

### Assumptions - Users with the most posts

There are several ways to determine suers with the most posts:

1. An evaluation of all posts to Reddit within a set period of time, collated, and sorted by the most active poster.
1. A live evaluation of incoming posts during the time the app is active.

The first interpretation, of one across a large period of a subreddit's history, quickly becomes unmanagable. Many subreddits have existed for upwards of a decade, receiving hundreds of posts an hour. This would dramatically exceed our rate limits.

The sceond interpretation seems reasonable. This metric would imply that we need to keep track of posts-by-user from the time that the app is active. This is an easily reportable metric, and should provide interesting metrics.
