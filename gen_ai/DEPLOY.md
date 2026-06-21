# Deploying LAMAS / Deep Search (free tier)

This guide hosts the whole solution for free:

| Part | Host | Free tier |
|------|------|-----------|
| MongoDB 7 | **MongoDB Atlas** | M0 cluster (512 MB), no card |
| .NET 9 Web API | **Render** Web Service (Docker) | free (sleeps after ~15 min idle) |
| Angular SPA | **Render** Static Site | free |

The repo already contains everything needed:
`server/Dockerfile`, `render.yaml`, and the Angular production environment.

---

## 1. Database — MongoDB Atlas

1. Create a free account at <https://www.mongodb.com/atlas> and a **M0** (free) cluster.
2. **Database Access** → add a database user (username + password).
3. **Network Access** → add IP `0.0.0.0/0` (allow from anywhere) so Render can connect.
4. **Connect** → *Drivers* → copy the **SRV connection string**, e.g.
   ```
   mongodb+srv://<user>:<password>@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority
   ```
5. Seed the database (run from the repo root, requires `mongosh`):
   ```bash
   mongosh "mongodb+srv://<user>:<password>@cluster0.xxxxx.mongodb.net/deepsearch" db/init-collections.js
   mongosh "mongodb+srv://<user>:<password>@cluster0.xxxxx.mongodb.net/deepsearch" db/seed.js
   ```

> The app uses database name **`deepsearch`** (set via `MongoDb__Database`).

---

## 2. Push the repo to GitHub

Render deploys from a Git repo. Push this repository to GitHub (or GitLab).

---

## 3. Deploy both services — Render Blueprint

1. Render dashboard → **New +** → **Blueprint** → select your repo.
2. Render reads `render.yaml` and provisions two services: **`lamas-api`** and **`lamas-client`**.
3. On the **`lamas-api`** service, set the two secret env vars (marked `sync: false`):
   - `MongoDb__ConnectionString` = your Atlas SRV string (include the password and `/deepsearch` is optional — db is set separately)
   - `Cors__AllowedOrigins__0` = the client URL, e.g. `https://lamas-client.onrender.com`
4. Deploy. The API health check is `/api/health` (it also pings MongoDB).

---

## 4. Point the client at the API

The Angular app bakes the API URL at **build time** (`client/src/environments/environment.prod.ts`).

- The placeholder is `https://lamas-api.onrender.com/api`.
- If Render assigns the API a different host, edit that one line to match your API URL
  (keep the trailing `/api`), commit, and let the client redeploy.

---

## Notes & caveats

- **No authentication.** The API is public — anyone with the URL can query and save
  queries. Fine for a short test; add an API key / auth before real use.
- **Cold starts.** Render free services sleep after ~15 min idle; the first request
  afterward takes ~30–50s while it wakes.
- **Free Search provider** is `RuleBased` (no key required). To use Gemini instead,
  set `FreeSearch__Provider=Gemini` and `FreeSearch__Gemini__ApiKey=<key>` on the API.

---

## Local build commands (sanity checks)

```bash
# Client production build  → dist/client/browser
cd client && npm ci && npm run build

# API release publish (what the Dockerfile runs)
cd server && dotnet publish DeepSearch.Api/DeepSearch.Api.csproj -c Release -o ./out

# Unit tests
cd server && dotnet test DeepSearch.UnitTests/DeepSearch.UnitTests.csproj
```
