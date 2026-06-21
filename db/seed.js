// seed.js
// Seeds a small but demo-friendly sample dataset plus the UI-driving metadata and
// a few example saved queries. Deterministic (seeded RNG) so results are stable.
// Runs after init-collections.js on first container start.

// Reassign the global `db` to the target database.
db = db.getSiblingDB('deepsearch');

// --- deterministic RNG (LCG) so the dataset is reproducible -------------------
function makeRng(seed) {
  let s = seed >>> 0;
  return function () {
    s = (Math.imul(s, 1664525) + 1013904223) >>> 0;
    return s / 4294967296;
  };
}
const rnd = makeRng(20260621);
const between = (min, max) => min + Math.floor(rnd() * (max - min + 1));

// --- Dimension value sets (Hebrew) -------------------------------------------
const years      = [2020, 2021, 2022, 2023, 2024];
const genders    = ['נשים', 'גברים'];
const ageGroups  = ['18-24', '25-35', '36-50', '51-67'];
const cities     = ['ירושלים', 'תל אביב', 'חיפה', 'באר שבע'];
const sectors    = ['כללי', 'חרדי', 'ערבי'];

// Average monthly income (ILS) for a cell, before per-person variation.
function baseIncome(city, gender, sector, ageGroup, year) {
  let v = 8000;
  v += { 'תל אביב': 4000, 'ירושלים': 2000, 'חיפה': 2500, 'באר שבע': 1500 }[city];
  v += gender === 'גברים' ? 1500 : 0;
  v += { 'כללי': 1500, 'חרדי': -1000, 'ערבי': -500 }[sector];
  v += { '18-24': -2000, '25-35': 0, '36-50': 2500, '51-67': 1500 }[ageGroup];
  v += (year - 2020) * 600; // mild yearly growth
  return v;
}

// How many sampled "people" to generate for a cell (bigger cities → more).
function peopleInCell(city, sector) {
  const cityBase = { 'תל אביב': 10, 'ירושלים': 9, 'חיפה': 6, 'באר שבע': 5 }[city];
  const sectorAdj = { 'כללי': 1, 'חרדי': 0, 'ערבי': 0 }[sector];
  return Math.max(2, cityBase + sectorAdj + between(-2, 2));
}

// Probability a person in this cell is employed.
function employmentRate(ageGroup, sector) {
  let p = 0.85;
  if (ageGroup === '18-24') p -= 0.25;
  if (ageGroup === '51-67') p -= 0.15;
  if (sector === 'חרדי') p -= 0.25;
  if (sector === 'ערבי') p -= 0.12;
  return p;
}

// --- statistics_fact: one document per sampled person ------------------------
const facts = [];
for (const year of years)
  for (const gender of genders)
    for (const ageGroup of ageGroups)
      for (const city of cities)
        for (const sector of sectors) {
          const n = peopleInCell(city, sector);
          const rate = employmentRate(ageGroup, sector);
          const base = baseIncome(city, gender, sector, ageGroup, year);
          for (let i = 0; i < n; i++) {
            const employed = rnd() < rate;
            // ±20% per-person variation around the cell base.
            const income = employed ? Math.round(base * (0.8 + rnd() * 0.4)) : 0;
            facts.push({
              year: NumberInt(year),
              gender,
              ageGroup,
              city,
              sector,
              employmentStatus: employed ? 'מועסק' : 'לא מועסק',
              income: NumberInt(income)
            });
          }
        }

db.statistics_fact.insertMany(facts);
print('Inserted ' + facts.length + ' statistics_fact documents');

// --- metadata: metrics -------------------------------------------------------
db.metadata.insertMany([
  { kind: 'metric', code: 'avg_income',   type: 'Average', field: 'income', label: 'שכר ממוצע' },
  { kind: 'metric', code: 'count_people', type: 'Count',                    label: 'כמות מועסקים' },
  { kind: 'metric', code: 'sum_income',   type: 'Sum',     field: 'income', label: 'סך השכר' }
]);

// --- metadata: dimensions ----------------------------------------------------
db.metadata.insertMany([
  { kind: 'dimension', code: 'year',     label: 'שנה',        values: years.map(String) },
  { kind: 'dimension', code: 'gender',   label: 'מגדר',       values: genders },
  { kind: 'dimension', code: 'city',     label: 'עיר',        values: cities },
  { kind: 'dimension', code: 'ageGroup', label: 'קבוצת גיל',  values: ageGroups },
  { kind: 'dimension', code: 'sector',   label: 'מגזר',       values: sectors }
]);
print('Seeded metadata (metrics + dimensions)');

// --- saved_queries: a few examples so the demo screen isn't empty ------------
// Definition shape matches the API's QueryDefinition (camelCase, string enums).
db.saved_queries.insertMany([
  {
    name: 'שכר ממוצע של נשים בירושלים לפי שנה',
    definition: {
      population: { gender: 'נשים', city: 'ירושלים' },
      metric: { type: 'Average', field: 'income' },
      period: { kind: 'Range', fromYear: 2021, toYear: 2024 },
      breakdowns: ['Year']
    },
    createdAt: new Date()
  },
  {
    name: 'כמות מועסקים לפי עיר (2024)',
    definition: {
      population: {},
      metric: { type: 'Count' },
      period: { kind: 'SingleYear', fromYear: 2024, toYear: 2024 },
      breakdowns: ['City']
    },
    createdAt: new Date()
  },
  {
    name: 'שכר ממוצע לפי מגדר ומגזר (2023)',
    definition: {
      population: {},
      metric: { type: 'Average', field: 'income' },
      period: { kind: 'SingleYear', fromYear: 2023, toYear: 2023 },
      breakdowns: ['Gender', 'Sector']
    },
    createdAt: new Date()
  }
]);
print('Seeded ' + db.saved_queries.countDocuments() + ' saved queries');
