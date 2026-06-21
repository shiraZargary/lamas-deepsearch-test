# Deep Search — Planning (Part B / חלק ב')

> PoC for the CBS (הלמ"ס) **Deep Search** system: dynamic, SQL-free querying of a
> central government dataset (employment, education, population, geography),
> plus a free-text / LLM query mode.
> Focus of grading: **architecture quality, layer separation, clean code, DI,
> correct DB access, Angular quality, UX, correct LLM integration.**

---

## 1. Tech Stack

| Layer | Choice | Why |
|-------|--------|-----|
| Server | **.NET 9 Web API (C#)** | Best-in-class DI & layering; ideal to demonstrate the engineering discipline being graded |
| Architecture | **Clean Architecture + CQRS + MediatR** | Clear separation of concerns; commands/queries isolate read vs write; MediatR keeps controllers thin |
| Validation | **FluentValidation** (MediatR pipeline behavior) | Cross-cutting validation without polluting handlers |
| DB | **MongoDB** (local Docker → **MongoDB Atlas on GCP**) | Document store; aggregation pipeline fits Average/Count/Sum + breakdowns; saved queries & metadata are natural JSON documents; Atlas = SaaS on GCP |
| Data access | **MongoDB .NET Driver** (typed `BsonDocument` aggregation pipelines) | No ORM needed; typed pipeline builder keeps user values as parameters (no string concat → NoSQL-injection safe) |
| Client | **Angular 21** (standalone components, signals) | PrimeNG-compatible latest (PrimeNG 21 has no Angular-22 support yet); signals for state |
| UI | **PrimeNG** + **PrimeNG Charts (Chart.js)** | Rich component set, built-in DataTable/dropdowns/charts, strong RTL/Hebrew support |
| Free-text / LLM | **`IFreeSearchService` abstraction**: rule-based default (offline, no key) + **`GeminiFreeSearchService`** real impl on the **Gemini API free tier** (Google AI Studio key) | Examiner runs everything offline with the default; real, free, swappable LLM proves the architecture allows a future swap with no changes to consumers |

---

## 2. Solution Structure

```
/server
  DeepSearch.sln
  DeepSearch.Domain/          # entities, value objects, enums — no dependencies
  DeepSearch.Application/     # CQRS commands/queries, handlers, interfaces, validators (MediatR)
  DeepSearch.Infrastructure/  # MongoDB driver, repositories, aggregation-pipeline builder, LLM clients
  DeepSearch.Api/             # Controllers (thin), DI wiring, middleware, error handling
  DeepSearch.UnitTests/       # pipeline builder + NL parser + handlers
/client                         # Angular 21 app
/db
  init-collections.js           # creates collections + indexes + validation schemas
  seed.js                       # sample documents (mongosh / mongoimport)
README.md
planning.md
```

### Dependency rule (Clean Architecture)
`Api → Application → Domain` and `Infrastructure → Application/Domain`.
Domain depends on nothing. Application defines interfaces; Infrastructure implements them.

---

## 3. Core Domain Contract

`QueryDefinition` — the single shape used by the builder UI, the LLM parser, and the executor:

```
QueryDefinition
  Population:  { Gender?, AgeGroup?, City?, Sector? }
  Metric:      { Type: Average | Count | Sum, Column }
  Period:      { Kind: SingleYear | Range, FromYear, ToYear }
  Breakdowns:  [ Year | Gender | City | ... ]
```

Both the structured builder and the NL parser produce this object; the executor
consumes only this object → one execution path, two entry points.

---

## 4. Database Design (MongoDB)

- **`statistics_fact`** (collection) — one document per record: `{ year, gender, ageGroup, city, sector, employmentStatus, income, ... }`. Indexes on common filter fields (`year`, `city`, `gender`). Optional JSON-schema validation for shape safety.
- **`metadata`** (collection) — drives the UI dynamically:
  - metric docs: `{ kind: "metric", code: "avg_income", type: "Average", field: "income", label }`
  - dimension docs: `{ kind: "dimension", code: "city", label, values: [...] }`
- **`saved_queries`** (collection) — `{ _id, name, definition: { ...QueryDefinition... }, createdAt }` (the definition is stored as a native sub-document, not a JSON string).

Single DB justification documented in README. Note in README the tradeoff vs a
relational store (SQL is the classic fit for official statistics; Mongo chosen for
PoC speed, schema flexibility, natural JSON saved-queries, and Atlas-on-GCP SaaS).

---

## 5. Task Breakdown

### Phase 0 — Setup
- [ ] 0.1 Create solution + Clean Architecture projects (Domain/Application/Infrastructure/Api) and reference graph.
- [ ] 0.2 Add NuGet: MediatR, FluentValidation, **MongoDB.Driver**, Swashbuckle.
- [ ] 0.3 Configure DI, CORS, Swagger, global exception-handling middleware.
- [ ] 0.4 Scaffold Angular 21 app (`ng new`, standalone, routing) + install & configure **PrimeNG** (theme, PrimeNG Charts).
- [ ] 0.5 `docker-compose` with **MongoDB** (+ optional mongo-express) for local dev.

### Phase 1 — Domain & Database
- [ ] 1.1 Domain entities + value objects (`QueryDefinition`, `Population`, `Metric`, `Period`, `Breakdown`) + enums.
- [ ] 1.2 `init-collections.js` (statistics_fact + metadata + saved_queries, indexes, validation schemas).
- [ ] 1.3 `seed.js` with representative Hebrew sample documents (genders, cities, age groups, years 2020–2024).
- [ ] 1.4 `MongoContext` / collection accessors + DI registration (connection string from config).

### Phase 2 — Query Building & Execution (Req #1, #2, #3)
- [ ] 2.1 `GetMetadataQuery` + handler → returns dimensions/metrics for UI dropdowns.
- [ ] 2.2 **Aggregation pipeline builder** (typed `$match` → `$group` → `$sort`; values passed as parameters via `BsonValue`, no string concat; fields whitelisted against metadata) in Infrastructure.
- [ ] 2.3 `ExecuteQueryQuery` + handler → returns `{ columns, rows }`.
- [ ] 2.4 `QuestionPhrasingService` → readable Hebrew sentence from `QueryDefinition` (Req #2).
- [ ] 2.5 FluentValidation validators + MediatR validation pipeline behavior.
- [ ] 2.6 Unit tests for the pipeline builder (injection safety, $group/breakdown correctness).

### Phase 3 — Saved Queries (Req #4)
- [ ] 3.1 `SaveQueryCommand`, `GetSavedQueriesQuery`, `RunSavedQueryQuery` + handlers.
- [ ] 3.2 Repository via MongoDB driver (store `definition` as a native sub-document in `saved_queries`).
- [ ] 3.3 Controller endpoints + unit tests.

### Phase 4 — Free-text / LLM (Req #5)
- [ ] 4.1 `IFreeSearchService` interface (text → `QueryDefinition` + interpretation notes).
- [ ] 4.2 `RuleBasedFreeSearchService` (**default**, offline, no key): map Hebrew keywords (נשים/גברים, ערים, ממוצע/כמות/סכום, טווחי שנים) → `QueryDefinition`.
- [ ] 4.3 `GeminiFreeSearchService` — **real** impl on the Gemini API free tier (Google AI Studio key); config-switched (`"FreeSearch:Provider": "RuleBased" | "Gemini"`); key from config/Secret Manager; prompt asks Gemini to return JSON matching `QueryDefinition`, validated server-side.
- [ ] 4.4 `FreeSearchCommand` + handler → returns interpreted `QueryDefinition` for user confirmation, then reuses the Phase-2 execution path.
- [ ] 4.5 Unit tests for the rule-based service.

### Phase 5 — Angular Client
- [ ] 5.1 `ApiService` (typed HTTP client) + models mirroring `QueryDefinition`.
- [ ] 5.2 Signal-based state service (current definition, results, saved list).
- [ ] 5.3 `QueryBuilderComponent` — population/metric/period/breakdown selectors (metadata-driven).
- [ ] 5.4 `QuestionPreviewComponent` — live readable sentence.
- [ ] 5.5 `ResultsComponent` — PrimeNG `p-table` + PrimeNG `p-chart`.
- [ ] 5.6 `SavedQueriesComponent` — list / re-run.
- [ ] 5.7 `NlQueryComponent` — free-text input → show interpretation → run.
- [ ] 5.8 Routing, layout, basic UX polish (loading/error states, RTL Hebrew support).

### Phase 6 — Cross-cutting & Docs
- [ ] 6.1 Global error handling + consistent API error contract.
- [ ] 6.2 Logging (Serilog) + request logging.
- [ ] 6.3 README: architecture, tech justifications (incl. **MongoDB vs relational tradeoff**), project structure, **run instructions**, assumptions/limits, DB setup (init + seed scripts).
- [ ] 6.4 DevOps doc (`docs/devops.md`): DEV/TEST/PROD, **Trunk-Based** branching, proposed CI/CD (GitHub Actions → Cloud Run + Firebase), config management, **secrets via GCP Secret Manager** (described, not implemented).
- [ ] 6.5 Final end-to-end smoke test (build server, build client, run a query, run an NL query, save & re-run).

---

## 6. Out of Scope (PoC limits)
- No real auth/RBAC (note as a production gap — government data would require it).
- Small sample dataset, not real CBS data.
- Real LLM call optional; **rule-based parser is the default** (Gemini free-tier impl available via config).
- CI/CD described, not executed.

---

## 7. Open Questions
1. Hebrew NL parsing assumed for the LLM mode (examples are Hebrew) — confirm.

## 8. Decided
- **Server: .NET 9** (C#) — confirmed, **not** Python.
- **Client: Angular 21** with **PrimeNG** (Angular 21 chosen because PrimeNG has no Angular-22 release yet).
- **Patterns: Clean Architecture + CQRS + MediatR.**
- **DB: MongoDB** (single store; local Docker → MongoDB Atlas on GCP).
- **Free-text / LLM: `IFreeSearchService`** — `RuleBasedFreeSearchService` default (offline) + `GeminiFreeSearchService` (Gemini API free tier), config-switched.
- **Branching strategy: Trunk-Based Development** — short-lived feature branches → PR + CI → always-releasable `main`; release tags; chosen over Git Flow because this is a continuously-deployed single web app (see `docs/devops.md`).
- **DevOps: documented only** (no pipeline files) — DEV/TEST/PROD, GitHub Actions CI/CD → Cloud Run + Firebase Hosting, GCP Secret Manager. See `docs/devops.md`.
