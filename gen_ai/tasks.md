# Deep Search — Implementation Tasks (Traceable)

Trackable implementation checklist for the Deep Search PoC.
Each task has an **ID**, a **status**, and a **Req** column tracing it back to the
test's functional requirements (חלק ב').

**Status legend:** `[ ]` todo · `[~]` in progress · `[x]` done · `[!]` blocked

---

## Requirements traceability matrix

| Req | Requirement (חלק ב') | Covered by tasks |
|-----|----------------------|------------------|
| R1 | בניית שאילתה — population / metric / period / breakdown | T2.1, T2.2, T2.3, T5.4 |
| R2 | יצירת ניסוח שאלה — readable sentence | T2.6, T5.5 |
| R3 | הרצת שאילתה — table + chart | T2.4, T2.5, T5.6 |
| R4 | שמירת שאילתה — save / list / re-run | T3.1, T3.2, T3.3, T5.7 |
| R5 | תשאול בשפה חופשית (LLM) | T4.1–T4.6, T5.8 |
| D1 | Architecture & layering | T0.1, T1.1, T6.1 |
| D2 | Server design (DI, layers, services, DB access, errors) | T0.2, T0.3, T2.*, T6.1, T6.2 |
| D3 | Database design (fact / metadata / saved) | T1.2, T1.3, T1.4 |
| D4 | Angular frontend (components, services, state, UX) | T5.* |
| D5 | DevOps (envs, CI/CD, config, secrets) | T7.1, T7.2, T7.3 |
| D6 | Submission (code + README) | T6.3, T6.4 |

---

## Phase 0 — Setup & skeleton

| ID | Task | Req | Status | Acceptance |
|----|------|-----|--------|------------|
| T0.1 | Create .NET 9 solution with Clean Architecture projects (Domain, Application, Infrastructure, Api) + reference graph | D1 | [x] | `dotnet build` passes; dependency rule enforced |
| T0.2 | Add NuGet: MediatR, FluentValidation, MongoDB.Driver, Swashbuckle, Serilog | D2 | [x] | Packages restore; versions pinned |
| T0.3 | Wire DI, CORS, Swagger, global exception middleware in Api | D2 | [x] | Swagger UI loads; CORS allows client origin |
| T0.4 | Scaffold Angular 21 app (standalone, routing) + install/configure PrimeNG (theme + charts) | D4 | [x] | `ng build` passes; PrimeNG toolbar/button render. **Angular 21 chosen** (PrimeNG 21 has no Angular-22 support yet) |
| T0.5 | `docker-compose` with MongoDB (+ mongo-express) | D3 | [x] | `docker compose up` exposes Mongo locally (file created; Docker not installed in this env) |

## Phase 1 — Domain & database

| ID | Task | Req | Status | Acceptance |
|----|------|-----|--------|------------|
| T1.1 | Domain model: `QueryDefinition`, `Population`, `Metric`, `Period`, `Breakdown` + enums | D1 | [x] | Domain project has no external deps; builds clean |
| T1.2 | `init-collections.js`: statistics_fact, metadata, saved_queries + indexes + validation schemas | D3 | [x] | Script creates collections & indexes (verified) |
| T1.3 | `seed.js`: Hebrew sample docs (genders, cities, age groups, years 2020–2024) | D3 | [x] | 480 fact docs + 8 metadata docs inserted (verified) |
| T1.4 | `MongoContext` + collection accessors + DI registration (conn string from config) | D2,D3 | [x] | `GET /api/health` → 200 `{mongo:"connected"}` (verified) |

## Phase 2 — Query building, phrasing & execution

| ID | Task | Req | Status | Acceptance |
|----|------|-----|--------|------------|
| T2.1 | `GetMetadataQuery` + handler → dimensions/metrics for UI | R1 | [x] | `GET /api/metadata` returns metrics & dimensions (verified) |
| T2.2 | Metadata endpoint controller + DTOs | R1 | [x] | `GET /api/metadata` → 200 + data (verified) |
| T2.3 | FluentValidation validators for `QueryDefinition` + MediatR validation behavior | R1,D2 | [x] | Validator unit tests pass (4); behavior wired |
| T2.4 | Aggregation pipeline builder (`$match`→`$group`→`$project`→`$sort`, parameterized, field whitelist) | R3,D2 | [x] | 7 unit tests: injection-safe + correct $group |
| T2.5 | `ExecuteQueryQuery` + handler + `POST /api/queries/execute` → `{columns, rows}` | R3 | [x] | Returns question + aggregated rows by year (verified) |
| T2.6 | `QuestionPhrasingService` → readable Hebrew sentence | R2 | [x] | 2 unit tests; live output verified end-to-end |

## Phase 3 — Saved queries

| ID | Task | Req | Status | Acceptance |
|----|------|-----|--------|------------|
| T3.1 | `SaveQueryCommand` + handler + repository (store definition sub-document) | R4 | [x] | `POST /api/queries/saved` persists (verified, returns id) |
| T3.2 | `GetSavedQueriesQuery` + handler + endpoint | R4 | [x] | `GET /api/queries/saved` lists (verified) |
| T3.3 | `RunSavedQueryQuery` + handler + endpoint (reuses ExecuteQuery path) | R4 | [x] | Re-run reproduces results; missing id → 404 (verified) |

## Phase 4 — LLM / free-text

| ID | Task | Req | Status | Acceptance |
|----|------|-----|--------|------------|
| T4.1 | `IFreeSearchService` interface (text → `QueryDefinition` + interpretation notes) | R5 | [x] | Interface + `FreeSearchResult` defined in Application |
| T4.2 | `RuleBasedFreeSearchService` (default, offline): Hebrew keyword mapping | R5 | [x] | 3 unit tests for sample Hebrew questions pass |
| T4.3 | `GeminiFreeSearchService` (Gemini free tier), config-switched, key from config/Secret Manager | R5 | [x] | Implemented (HttpClient + JSON-schema prompt); key from config |
| T4.4 | Service selection via config (`FreeSearch:Provider: RuleBased \| Gemini`) + DI factory | R5,D2 | [x] | `FreeSearchServiceSelector` + DI factory; 6 selector tests pass |
| T4.5 | `FreeSearchCommand` + handler → interpreted definition for confirmation | R5 | [x] | `POST /api/free-search` returns definition + question + notes (verified) |
| T4.6 | Unit tests for rule-based service + service-selection factory | R5 | [x] | 26/26 tests green; free-search→execute verified end-to-end |

## Phase 5 — Angular client

| ID | Task | Req | Status | Acceptance |
|----|------|-----|--------|------------|
| T5.1 | `ApiService` typed HTTP client + TS models mirroring `QueryDefinition` | D4 | [x] | Wraps all endpoints; typed; builds clean |
| T5.2 | Signal-based state service (current definition, results, saved list) | D4 | [x] | `QueryStateService` with signals; orchestrates API |
| T5.3 | Routing + app layout (RTL Hebrew support, nav) | D4 | [x] | 3 routes + toolbar nav; RTL set on document |
| T5.4 | `QueryBuilderComponent` — metadata-driven population/metric/period/breakdown | R1,D4 | [x] | Metadata-driven selects build a `QueryDefinition` |
| T5.5 | `QuestionPreviewComponent` — live readable sentence | R2,D4 | [x] | Bound to `question` signal (live phrase) |
| T5.6 | `ResultsComponent` — PrimeNG `p-table` + `p-chart` | R3,D4 | [x] | Table + bar chart from result signal |
| T5.7 | `SavedQueriesComponent` — list / re-run | R4,D4 | [x] | Lists saved + re-run via state |
| T5.8 | `NlQueryComponent` — free text → show interpretation → run | R5,D4 | [x] | Parse → notes/preview → run |
| T5.9 | UX polish: loading/error states, empty states | D4 | [x] | Button loading, error banners, empty messages |

> Verification: `ng build` (production) succeeds — Angular compiles all templates against imported PrimeNG components, confirming wiring. Dev server boots and serves `<app-root>` at :4200. Interactive in-browser click-through pending a browser environment.

## Phase 6 — Cross-cutting & docs

| ID | Task | Req | Status | Acceptance |
|----|------|-----|--------|------------|
| T6.1 | Consistent API error contract + global handling | D2 | [x] | `ExceptionHandlingMiddleware` → uniform JSON (400/404/500) |
| T6.2 | Serilog request/response logging | D2 | [x] | `UseSerilogRequestLogging` + Serilog wired in Program.cs |
| T6.3 | README: architecture, tech justifications (incl. Mongo-vs-relational, LLM), structure, **run instructions**, assumptions/limits, DB setup | D6 | [x] | `README.md` created; covers all sections |
| T6.4 | Link `docs/architecture.md` diagram from README | D6 | [x] | README links architecture.md + devops.md |

## Phase 7 — DevOps & deployment

| ID | Task | Req | Status | Acceptance |
|----|------|-----|--------|------------|
| T7.1 | Document DEV / TEST / PROD environments + config management | D5 | [x] | `docs/devops.md` — environments table + config section |
| T7.2 | Proposed CI/CD: GitHub Actions → Cloud Run (API) + Firebase Hosting (client) | D5 | [x] | Described in `docs/devops.md` (text only, no YAML) + CI/CD diagram prompt |
| T7.3 | Secrets strategy: env vars / GCP Secret Manager; no secrets committed | D5 | [x] | `docs/devops.md`; `appsettings` placeholders only (verified) |
| T7.5 | Branching strategy decision (Trunk-Based vs Git Flow) | D5 | [x] | **Trunk-Based** chosen + rationale in `docs/devops.md`, planning.md |
| T7.4 | (Optional) Deploy live demo: Atlas + Cloud Run + Firebase; note cold-start | D5 | [ ] | Public URL works — optional, not done |

---

## Verification gates (run before marking a phase done)

- [ ] `dotnet build` + `dotnet test` green (server)
- [ ] `ng build` green (client)
- [ ] End-to-end smoke: build a query → run → table+chart → save → re-run → NL query → run
- [ ] No secrets in the repo
