# Deep Search (חיפוש עמוק) — PoC

A Proof-of-Concept for the CBS (הלמ״ס) **Deep Search** system: government users build
**dynamic, SQL-free queries** over a central statistics dataset (employment, education,
population, geography), see a readable Hebrew phrasing of their question, run it
(table + chart), save/re-run queries, and ask questions in **free-text Hebrew** that
the system interprets into a structured query.

> Scope: a PoC focused on architecture, layering, clean code, correct DB access and a
> swappable AI integration — **not** a production system.

---

## AI-assisted development

This PoC was built with an AI-first workflow. The tools and artifacts used:

- **Kiro CLI** — the primary AI coding assistant, used end-to-end for planning,
  scaffolding, implementation, and tests directly from the terminal.
- **Custom agent** ([`.kiro/agents/lamas-dev.json`](.kiro/agents/lamas-dev.json)) —
  a project-scoped `lamas-dev` agent configured with curated tools/permissions and
  pinned resources (`client/package.json`, `client/angular.json`, the skills) so the
  assistant follows the project's Angular conventions when editing `client/` code.
- **Skills** ([`.kiro/skills/`](.kiro/skills)) — reusable instruction sets the agent loads:
  - `modern-angular` — Angular 21 conventions (standalone components, signals, PrimeNG/Aura, vitest, RTL Hebrew).
  - `accessibility` — a11y guidelines applied across the client UI.
- **Planning & traceability docs** ([`gen_ai/`](gen_ai)):
  - [`planning.md`](gen_ai/planning.md) — tech-stack decisions, solution structure, and design rationale.
  - [`tasks.md`](gen_ai/tasks.md) — a traceable implementation checklist with a requirements-traceability matrix (each task maps back to a functional requirement).
  - [`architecture.md`](gen_ai/architecture.md) — architecture diagram, key flows, and the AI diagram-generation prompt.
  - [`devops.md`](gen_ai/devops.md) — environments, CI/CD, branching, and secrets strategy.
  - [`DEPLOY.md`](gen_ai/DEPLOY.md) — deployment notes.
- **Swappable LLM in the product itself** — beyond build-time tooling, the app integrates
  an optional **Gemini** (free tier) engine behind `IFreeSearchService` for free-text Hebrew queries.

---

## Features

- **בניית שאילתה (Query Builder)** — pick metric / population / period / breakdowns, see
  the live Hebrew phrasing, run for a table + chart, and save.
- **חיפוש חופשי (Free-text)** — type a Hebrew question (e.g. *"הצג את השכר הממוצע של נשים
  בירושלים בשנים 2021-2024 לפי שנה"*), review the interpretation, then run it.
- **שאילתות שמורות (Saved queries)** — list and re-run saved queries.

---

## Tech stack

| Layer | Choice |
|-------|--------|
| Backend | .NET 9 Web API (C#) |
| Architecture | Clean Architecture + CQRS + MediatR |
| Validation | FluentValidation (MediatR pipeline behavior) |
| Database | MongoDB (local Docker → MongoDB Atlas on GCP) |
| Data access | MongoDB .NET Driver (typed, whitelisted, NoSQL-injection safe) |
| Frontend | Angular 21 + PrimeNG (SPA, RTL support) |
| Free-text / LLM | `IFreeSearchService`: rule-based default (offline) + Gemini (free tier), config-switched |

---

## Architecture

Clean Architecture with the dependency rule
`Api → Application → Domain` and `Infrastructure → Application/Domain`
(Domain depends on nothing; Application defines interfaces, Infrastructure implements them).

Both the **structured builder** and the **free-text** screen produce the **same**
`QueryDefinition`, which flows through one execution path
(`$match → $group → $project → $sort` aggregation) — **two entry points, one engine**.

Full diagram and flows: [`gen_ai/architecture.md`](gen_ai/architecture.md).
DevOps (environments, CI/CD, branching, secrets): [`gen_ai/devops.md`](gen_ai/devops.md).

---

## Project structure

```
/server                         # .NET 9 solution (Clean Architecture)
  DeepSearch.Domain/            # entities, value objects, enums — no dependencies
  DeepSearch.Application/       # CQRS commands/queries + handlers (MediatR), interfaces, validators
  DeepSearch.Infrastructure/    # Mongo repos, aggregation-pipeline builder, free-search services
  DeepSearch.Api/               # thin controllers, DI wiring, middleware, Swagger
  DeepSearch.UnitTests/         # 26 unit tests
/client                         # Angular 21 + PrimeNG SPA
/db
  init-collections.js           # creates collections + indexes + JSON-schema validation
  seed.js                       # ~3,800 Hebrew sample fact docs + metadata + example saved queries
/gen_ai                         # architecture, planning, devops, deploy docs
docker-compose.yml              # local MongoDB (+ mongo-express)
render.yaml                     # Render.com hosting config
```

---

## Running locally

### Prerequisites
- .NET 9 SDK
- Node.js 20+ and npm
- MongoDB (via Docker, or a local `mongod`)

### 1. Start MongoDB and seed data
```bash
# from repo root — starts MongoDB and auto-runs db/init-collections.js + db/seed.js
docker compose up -d
```
Without Docker, run a local `mongod` on `27017`, then:
```bash
mongosh "mongodb://127.0.0.1:27017" --file db/init-collections.js
mongosh "mongodb://127.0.0.1:27017" --file db/seed.js
```

### 2. Run the API
```bash
cd server
dotnet run --project DeepSearch.Api
# Swagger UI on the HTTPS port; HTTP: http://127.0.0.1:5080
```
Settings (`MongoDb`, `Cors`, `FreeSearch`) live in
`server/DeepSearch.Api/appsettings.json`. No secrets are committed.

### 3. Run the client
```bash
cd client
npm install
npm start            # ng serve → http://localhost:4200
```
The client calls the API at the URL in `client/src/environments/environment.ts`.

### 4. Run the tests
```bash
cd server
dotnet test          # 26 unit tests
```

---

## Switching the free-text engine to a real LLM

Default is the offline rule-based engine. To use Gemini (free tier), set in
`appsettings.json` (or environment variables):
```json
"FreeSearch": { "Provider": "Gemini", "Gemini": { "ApiKey": "<your key>" } }
```
No other code changes — `IFreeSearchService` is resolved from config.

---

## Assumptions & limitations (PoC)

- **No authentication/RBAC** — a real government system would require it; intentionally out of scope.
- **Small synthetic sample** (~3,800 documents across 480 population cells), not real CBS data.
- **Free-text engine** defaults to a rule-based Hebrew parser; the Gemini path is implemented but optional.
- **CI/CD is described, not implemented** (see `gen_ai/devops.md`).
- Single MongoDB store; no second database.

---

## Testing summary

- 26 unit tests: aggregation-pipeline builder (incl. NoSQL-injection safety), Hebrew
  phrasing, query/save validators, rule-based free-search, and the config-driven
  service selector.
- All API endpoints verified end-to-end against seeded MongoDB data
  (metadata, execute, phrase, save/list/run, free-search → execute).
