# Deep Search (חיפוש עמוק) — PoC

A Proof-of-Concept for the CBS (הלמ״ס) **Deep Search** system: government users
build **dynamic, SQL-free queries** over a central statistics dataset (employment,
education, population, geography), see a readable Hebrew phrasing of their question,
run it (table + chart), save/re-run queries, and ask questions in **free-text
Hebrew** that the system interprets into a structured query.

> Scope: this is a PoC focused on architecture, layering, clean code, correct DB
> access and a swappable AI integration — **not** a production system.

---

## Tech stack & why

| Layer | Choice | Rationale |
|-------|--------|-----------|
| Backend | **.NET 9 Web API (C#)** | First-class DI & layering — best showcases the engineering discipline being assessed |
| Architecture | **Clean Architecture + CQRS + MediatR** | Strict layer separation; commands/queries isolate intent; thin controllers |
| Validation | **FluentValidation** in a MediatR pipeline behavior | Cross-cutting validation without polluting handlers |
| Database | **MongoDB** (local → **MongoDB Atlas** on GCP) | Aggregation pipeline fits Average/Count/Sum + breakdowns; saved queries & metadata are natural documents; Atlas = managed SaaS on GCP |
| Data access | **MongoDB .NET Driver** (typed `BsonDocument` pipelines) | Parameterized values + whitelisted fields → NoSQL-injection safe; no ORM needed |
| Frontend | **Angular 21 + PrimeNG** | Angular 21 chosen because PrimeNG has no Angular-22 release yet; PrimeNG gives table/chart/inputs + RTL support |
| Free-text / LLM | **`IFreeSearchService`**: rule-based default (offline) + **Gemini** (free tier), config-switched | Examiner runs everything offline; a real, free, swappable LLM proves the architecture allows a future swap with no consumer changes |

**MongoDB vs relational (tradeoff):** a relational store + SQL is the classic fit
for official statistics. MongoDB was chosen for PoC speed, schema flexibility,
storing saved-query definitions as native documents, and the managed Atlas-on-GCP
option. The aggregation logic is fully encapsulated behind `IQueryExecutor`, so
swapping to a relational store later would not affect the rest of the system.

---

## Architecture

Clean Architecture with the dependency rule
`Api → Application → Domain` and `Infrastructure → Application/Domain`
(Domain depends on nothing; Application defines interfaces, Infrastructure
implements them).

Both the **structured builder** and the **free-text** screen produce the **same**
`QueryDefinition`, which flows through one execution path
(`$match → $group → $project → $sort` aggregation) — two entry points, one engine.

Full diagram and flows: **[`docs/architecture.md`](docs/architecture.md)**.
DevOps (environments, CI/CD, branching, secrets): **[`docs/devops.md`](docs/devops.md)**.

---

## Project structure

```
/server                         # .NET 9 solution (Clean Architecture)
  DeepSearch.Domain/            # entities, value objects, enums — no dependencies
  DeepSearch.Application/       # CQRS commands/queries + handlers (MediatR), interfaces, validators
  DeepSearch.Infrastructure/    # Mongo repos, aggregation-pipeline builder, free-search services
  DeepSearch.Api/               # thin controllers, DI wiring, middleware, Swagger
  DeepSearch.UnitTests/         # 26 unit tests (pipeline builder, phrasing, validators, free-search)
/client                         # Angular 21 + PrimeNG SPA
/db
  init-collections.js           # creates collections + indexes + JSON-schema validation
  seed.js                       # ~3,800 Hebrew sample fact docs + metadata + example saved queries
/docs
  architecture.md               # architecture diagram + AI prompt
  devops.md                     # DEV/TEST/PROD, CI/CD, branching, secrets
docker-compose.yml              # local MongoDB (+ mongo-express)
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
# Swagger: https://localhost:xxxx/swagger  (HTTP: http://127.0.0.1:5080)
```
Connection string & settings: `server/DeepSearch.Api/appsettings.json`
(`MongoDb`, `Cors`, `FreeSearch`). No secrets are committed.

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

## Using it
- **בניית שאילתה (Query Builder):** pick metric / population / period / breakdowns →
  see the live Hebrew phrasing → **הרץ** for a table + chart → **שמור** to save.
- **חיפוש חופשי (Free-text):** type a Hebrew question (e.g.
  *"הצג את השכר הממוצע של נשים בירושלים בשנים 2021-2024 לפי שנה"*), review the
  interpretation, then run it.
- **שאילתות שמורות (Saved):** list and re-run saved queries.

### Switching the free-text engine to a real LLM
Default is the offline rule-based engine. To use Gemini (free tier), set in
`appsettings.json` (or environment variables):
```json
"FreeSearch": { "Provider": "Gemini", "Gemini": { "ApiKey": "<your key>" } }
```
No other code changes — `IFreeSearchService` is resolved from config.

---

## Assumptions & limitations (PoC)
- **No authentication/RBAC.** A real government system would require it — noted as a
  production gap, intentionally out of scope here.
- **Small synthetic sample** (~3,800 documents across 480 population cells), not real
  CBS data; each document is one sampled survey record (employed/unemployed with an
  income), so Count/Average/Sum vary realistically across breakdowns.
- **Free-text engine** defaults to a rule-based Hebrew parser; the Gemini path is
  implemented but optional (needs an API key).
- **CI/CD is described, not implemented** (see `docs/devops.md`), per the task.
- Single MongoDB store; no second database (justified above).

---

## Testing summary
- 26 unit tests: aggregation-pipeline builder (incl. NoSQL-injection safety),
  Hebrew phrasing, query/save validators, rule-based free-search, and the
  config-driven service selector.
- All API endpoints were verified end-to-end against seeded MongoDB data
  (metadata, execute, phrase, save/list/run, free-search → execute).
