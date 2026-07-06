# TaskManagement

A small full-stack task manager: ASP.NET Core + SQLite on the back end, Angular on the front end, with a real-time task view over SignalR (WebSockets).

## Quick start

Prerequisites: [.NET 9 SDK](https://dotnet.microsoft.com/download) and [Node.js 22+](https://nodejs.org/).

```
Run.cmd
```

That builds the backend, launches it on **http://localhost:5000**, builds the frontend, and launches it on **http://localhost:4200** (opens in your browser). Each app runs in its own console window; close the windows to stop.

Sign in with the seeded demo account — `demo@taskmanagement.local` / `Demo123!` — or create your own account from the landing page.

The SQLite database (`Backend/TaskManagement.Api/taskmanagement.db`) is created and seeded automatically on first startup.

## Code structure

```
TaskManagement/
├── Backend/
│   ├── TaskManagement.Api/            ASP.NET Core Web API
│   │   ├── Domain/                    Entities: User, TaskItem, Priority
│   │   ├── Application/
│   │   │   ├── Auth/                  Register/Login commands + JWT service + DTOs
│   │   │   ├── Tasks/                 Task commands/queries + DTOs + ITaskNotifier
│   │   │   └── Common/                Password hasher, typed exceptions
│   │   ├── Infrastructure/            EF Core DbContext (SQLite), startup seeder
│   │   ├── Controllers/               Thin endpoints: DTO in → MediatR → DTO out
│   │   ├── Hubs/                      SignalR TasksHub + notifier
│   │   └── Middleware/                Exception → JSON error responses
│   └── TaskManagement.Api.Tests/      xUnit + Moq unit tests (49 tests)
├── Frontend/
│   └── task-manager/                  Angular app
│       └── src/app/
│           ├── pages/landing/         Sign in / create account
│           ├── pages/tasks/           Task list fed by the SignalR subscription
│           └── services/              AuthService, TaskService, RealtimeService
├── Run.cmd                            Build + launch both apps
└── README.md
```

## How it works

**CQRS with MediatR.** Controllers accept DTOs and dispatch commands/queries through MediatR. Handlers map DTOs to EF Core entities, persist to SQLite, and raise a broadcast. Reads and writes are separate request types (`GetTasksQuery` vs `CreateTaskCommand`, `CompleteTaskCommand`, `ReorderTaskCommand`, `DeleteTaskCommand`).

**Real-time view.** The Angular tasks page never polls: on connect, `TasksHub` pushes the user's full task list over the WebSocket, and every successful command re-broadcasts the updated list to that user's group. `RealtimeService` exposes it as an Angular signal that the page renders directly.

**Auth.** Register/login issue a JWT (passwords stored as PBKDF2 hashes). The token authenticates both HTTP calls (via an interceptor) and the SignalR connection (via query-string token). Each user only ever sees their own tasks.

**Task model.** `Order`, `Priority`, `Title`, `Description`, `CreatedDate` are required; `CompletedDate` and `CompletedDescription` are set when a task is completed. `Order` is kept contiguous per user across create/reorder/delete.

**Logging.** Standard `ILogger<T>` console logging throughout (auth events, task changes, SignalR connects, errors).

## API summary

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/register` | Create an account, returns JWT |
| POST | `/api/auth/login` | Sign in, returns JWT |
| GET | `/api/tasks` | Current user's tasks (ordered) |
| POST | `/api/tasks` | Create a task (order auto-assigned) |
| POST | `/api/tasks/{id}/complete` | Complete with a description |
| POST | `/api/tasks/{id}/reorder` | Move to a new position |
| DELETE | `/api/tasks/{id}` | Delete and renumber |
| WS | `/hubs/tasks` | SignalR hub, pushes `TasksUpdated` |

## Running the tests

```
cd Backend
dotnet test
```

Unit tests cover every command/query handler (including ownership and validation edge cases), the password hasher, the JWT service, and the hub's connect behavior. Handlers run against an in-memory SQLite database; SignalR and JWT dependencies are mocked behind interfaces (`ITaskNotifier`, `IJwtTokenService`).

## Manual run (without Run.cmd)

```
# Backend
cd Backend\TaskManagement.Api
dotnet run                          # http://localhost:5000

# Frontend (separate terminal)
cd Frontend\task-manager
npm install
npx ng serve                        # http://localhost:4200
```
