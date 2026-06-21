// init-collections.js
// Creates the Deep Search collections with JSON-schema validation and indexes.
// Runs automatically on first container start (mounted into
// /docker-entrypoint-initdb.d). Idempotent-ish: drops & recreates collections.

// Reassign the global `db` to the target database (works in mongosh and in the
// docker-entrypoint-initdb.d context).
db = db.getSiblingDB('deepsearch');

// --- statistics_fact: the queryable measure data ------------------------------
db.statistics_fact.drop();
db.createCollection('statistics_fact', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['year', 'gender', 'ageGroup', 'city', 'sector', 'employmentStatus', 'income'],
      properties: {
        year:             { bsonType: 'int' },
        gender:           { bsonType: 'string' },
        ageGroup:         { bsonType: 'string' },
        city:             { bsonType: 'string' },
        sector:           { bsonType: 'string' },
        employmentStatus: { bsonType: 'string' },
        income:           { bsonType: ['double', 'int'] }
      }
    }
  }
});
db.statistics_fact.createIndex({ year: 1 });
db.statistics_fact.createIndex({ city: 1 });
db.statistics_fact.createIndex({ gender: 1 });
db.statistics_fact.createIndex({ year: 1, city: 1, gender: 1 });

// --- metadata: drives the UI (metrics + dimensions) ---------------------------
db.metadata.drop();
db.createCollection('metadata', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['kind', 'code', 'label'],
      properties: {
        kind:   { enum: ['metric', 'dimension'] },
        code:   { bsonType: 'string' },
        label:  { bsonType: 'string' },
        type:   { bsonType: 'string' },   // metric only: Average | Count | Sum
        field:  { bsonType: 'string' },   // metric only: aggregated numeric field
        values: { bsonType: 'array' }     // dimension only: selectable values
      }
    }
  }
});
db.metadata.createIndex({ kind: 1, code: 1 }, { unique: true });

// --- saved_queries: persisted query definitions -------------------------------
db.saved_queries.drop();
db.createCollection('saved_queries', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['name', 'definition', 'createdAt'],
      properties: {
        name:       { bsonType: 'string' },
        definition: { bsonType: 'object' },
        createdAt:  { bsonType: 'date' }
      }
    }
  }
});
db.saved_queries.createIndex({ createdAt: -1 });

print('Deep Search collections created: statistics_fact, metadata, saved_queries');
