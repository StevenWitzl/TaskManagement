# TaskManagement

[![CI](https://github.com/StevenWitzl/TaskManagement/actions/workflows/ci.yml/badge.svg)](https://github.com/StevenWitzl/TaskManagement/actions/workflows/ci.yml)

A small full-stack task manager: ASP.NET Core + SQLite on the back end, React on the front end, with a real-time task view over SignalR (WebSockets).

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
│   │   │   ├── Auth/                  Register/Login commands + validators + JWT service
│   │   │   ├── Tasks/                 Task commands/queries + validators + ITaskNotifier
│   │   │   ├── Behaviors/             MediatR pipeline: validation + logging
│   │   │   └── Common/                Password hasher, typed exceptions
│   │   ├── Infrastructure/            EF Core DbContext (SQLite), startup seeder
│   │   ├── Controllers/               Thin endpoints: DTO in → MediatR → DTO out
│   │   ├── Hubs/                      SignalR TasksHub + notifier
│   │   └── Middleware/                Exception → JSON error responses
│   └── TaskManagement.Api.Tests/      xUnit + Moq unit tests (49 tests)
├── Frontend/
│   └── task-manager/                  React app (Vite + TypeScript)
│       └── src/
│           ├── pages/Landing.tsx      Sign in / create account (incl. first/last name)
│           ├── pages/Tasks.tsx        Task list fed by the SignalR subscription
│           ├── AuthContext.tsx        Session state + login/register/logout
│           ├── useRealtime.ts         SignalR connection → tasks state
│           └── api.ts                 Fetch wrapper with bearer token
├── Run.cmd                            Build + launch both apps
└── README.md
```

## How it works

**CQRS with MediatR.** Controllers accept DTOs and dispatch commands/queries through MediatR. Handlers map DTOs to EF Core entities, persist to SQLite, and raise a broadcast. Reads and writes are separate request types (`GetTasksQuery` vs `CreateTaskCommand`, `CompleteTaskCommand`, `ReorderTaskCommand`, `DeleteTaskCommand`).

**Cross-cutting concerns as pipeline behaviors.** Every request flows through `LoggingBehavior` (logs each command/query with its duration) and `ValidationBehavior` (runs the request's FluentValidation validators and rejects invalid input before it reaches a handler). Handlers only contain business rules — input-shape validation lives in per-command validators, and failures surface as a 400 with the aggregated messages.

**Real-time view.** The React tasks page never polls: on connect, `TasksHub` pushes the user's full task list over the WebSocket, and every successful command re-broadcasts the updated list to that user's group. The `useRealtime` hook exposes it as React state that the page renders directly.

**Auth.** Registration requires email, password, and first/last name (all validated server-side); login issues a JWT (passwords stored as PBKDF2 hashes). The token authenticates both HTTP calls and the SignalR connection (via query-string token). Each user only ever sees their own tasks, and the signed-in user's name is shown in the header.

**Task model.** `Order`, `Priority`, `Title`, `Description`, `CreatedDate` are required; `CompletedDate` (and an optional `CompletedDescription`) are set when a task is completed. `Order` only applies to open tasks and is always contiguous from 1 — completing, deleting, or reordering renumbers the open list. Completed tasks drop out of the numbering and are shown in completion order. Reordering is drag-and-drop in the UI.

**UI.** Tasks are created through a modal (+ Create task). The header shows the signed-in user's name plus a live count of open tasks by priority (High/Medium/Low).

**Logging.** Standard `ILogger<T>` console logging throughout (auth events, task changes, SignalR connects, errors).

## API summary

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/register` | Create an account, returns JWT |
| POST | `/api/auth/login` | Sign in, returns JWT |
| GET | `/api/tasks` | Current user's tasks (ordered) |
| POST | `/api/tasks` | Create a task (order auto-assigned) |
| POST | `/api/tasks/{id}/complete` | Complete with an optional description |
| POST | `/api/tasks/{id}/reorder` | Move to a new position (drag & drop in the UI) |
| DELETE | `/api/tasks/{id}` | Delete and renumber |
| WS | `/hubs/tasks` | SignalR hub, pushes `TasksUpdated` |

## Running the tests

```
cd Backend
dotnet test
```

Unit tests cover every command/query handler (including ownership edge cases), all FluentValidation validators, the validation pipeline behavior, the password hasher, the JWT service, and the hub's connect behavior. Handlers run against an in-memory SQLite database; SignalR and JWT dependencies are mocked behind interfaces (`ITaskNotifier`, `IJwtTokenService`).

CI (GitHub Actions) runs the backend test suite and the frontend type-check/build on every push and pull request — see `.github/workflows/ci.yml`.

## Manual run (without Run.cmd)

```
# Backend
cd Backend\TaskManagement.Api
dotnet run                          # http://localhost:5000

# Frontend (separate terminal)
cd Frontend\task-manager
npm install
npm run dev                         # http://localhost:4200
```
