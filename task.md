# Backend Challenge: Connected Vehicle Rental Platform

## 📦 1. Overview

You’re stepping into the role of a backend engineer building the core of a connected vehicle rental platform. In this system, vehicles continuously send telemetry data (like odometer readings and battery levels), which needs to be processed and turned into actionable insights.

Rather than consuming real-time messages from a message queue, you’ll be working with **CSV files** that contain simulated telemetry and vehicle data for demonstration purposes.

### 🚀 Your Mission

Your task is to build a **.NET 8 or 9 Web API** that:

- ✅ Reads telemetry and vehicle data from CSV files.
- ✅ Persists this data into a relational database.
- ✅ Applies business logic and validations.
- ✅ Exposes structured CRUD operations and aggregated reporting endpoints.

---

## 📄 2. Input Data Format – CSV

### **vehicles.csv**
| vin               | make       | model     | year | pricePerKmInEuro | pricePerDayInEuro |
|-------------------|------------|-----------|------|------------------|-------------------|
| WAUZZZ4V4KA000002 | Audi       | e-tron    | 2019 | 0.30             | 45                |
| WVWZZZAUZLW000003 | Volkswagen | ID.4      | 2021 | 0.28             | 40                |
| WBA3A5C58DF123456 | BMW        | i3        | 2020 | 0.32             | 42                |

### **telemetry.csv**
| vin               | name         | value   | timestamp  |
|-------------------|--------------|---------|------------|
| WBA3A5C58DF123456 | odometer     | 120000  | 1713086400 |
| WBA3A5C58DF123456 | battery_soc  | 85      | 1713086500 |
| ...               | ...          | ...     | ...        |

#### 📘 Expected Telemetry/Channel Types:
- **odometer**: Total kilometers driven.
- **battery_soc**: Battery State of Charge (in %, range [0, 100]).

📌 **Note**:
- Your solution should support easy extensibility of telemetry types.
- Each row represents a measurement of a single telemetry type/signal.

---

## 🧱 3. Domain Modeling & Persistence

### Entities to Design and Persist:
1. **🚗 Vehicle**
   - Identified by `vin`.
   - Includes make, model, year, price per km, and price per day.

2. **👤 Customer**
   - Represents a vehicle renter.
   - Includes unique ID and name of the customer.

3. **📅 Rental**
   - Represents a specific vehicle rental/booking made by a specific customer.
   - Includes:
     - Start/end timestamps.
     - Lifecycle states: `ordered`, `cancelled`.
     - Metrics like odometer snapshots.

---

## 🌱 4. Database Seeding

Your solution must seed the database with the following:
- ✅ A predefined list of vehicles from `vehicles.csv`.
- ✅ A predefined telemetry dataset from `telemetry.csv`.

---

## ✅ 5. Business Rules & Validations

### Implement the following:
1. ❌ Prevent overlapping rentals for the same vehicle and customer.
2. ❌ Reject faulty telemetry data (e.g., invalid timestamps, negative values).
3. ✅ Add validations to API endpoints (requests/responses).
4. ✅ Log significant events (e.g., telemetry imported, rentals created, validation failures).

### Price Calculation Formula:
Let:
- `total_kilometers_per_rental`: Total kilometers driven during the rental.
- `number_of_rental_days`: Number of rental days.
- `price_per_km_in_euro`: Price per kilometer for the specific VIN.
- `price_per_day_in_euro`: Price per day for the specific VIN.
- `battery_delta_per_rental`: Change in battery percentage during the rental.

**Total Cost** =
`total_kilometers_per_rental × price_per_km_in_euro`
`+ number_of_rental_days × price_per_day_in_euro`
`+ max(0, -battery_delta_per_rental) × 0.2€`

---

## 📊 6. API Endpoints

### **Per Rental**
- **CREATE**: Rental for customer, vehicle, and time range.
- **CANCEL**: Rental.
- **UPDATE**: Rental (allow updating start/end date).
- **GET**: List of all rentals.
- **GET**: Single rental by ID with details:
  - Distance traveled (based on odometer).
  - Battery SOC at rental start and end.

### **Per Customer**
- **CREATE**: Customer.
- **UPDATE**: Customer (any metadata field).
- **DELETE**: Customer.
- **GET**: List of all customers.
- **GET**: Single customer by ID with details:
  - Total distance driven (for all rentals).
  - Total price (for all rentals).

### **Per Vehicle**
- **GET**: List of all vehicles.
- **GET**: Single vehicle by ID with details:
  - Total distance driven.
  - Total rental count.
  - Total rental income.

---

## 🧪 7. Testing

### Expected:
- **Unit Tests**:
  - Core logic and validation.
- **Integration Tests**:
  - Data processing and report generation.

📌 **Note**: Full test coverage is not required. Tests are for demonstration purposes only.

---

## ⚙️ 8. Tech Requirements

| Area          | Requirement                                   |
|---------------|-----------------------------------------------|
| **.NET**      | .NET 8, Web API                              |
| **Architecture** | Clean or layered architecture |
| **Persistence** | EF Core (required), Dapper (optional)        |
| **Database**  | Relational (PostgreSQL, MSSQL, etc.)          |
| **DevOps**    | Git (required), Docker (optional), Docker Compose (optional) |
| **API Docs**  | Swagger / OpenAPI (required)                      |
| **Auth**      | API Key (optional) or Bearer token (optional)            |

---

## 📄 9. Deliverables

You should provide:
- 🔗 A Git repository (or a ZIP archive).
- 📜 API documentation (Swagger UI/spec).
- 📘 A README that includes:
  - How to run your solution (e.g., Docker or CLI).
  - Architecture and design decisions.

---

## 🌟 10. Bonus Points (Optional)

Consider going the extra mile with:
- 📦 Docker Compose setup with:
  - Your API.
  - Relational database.
- 🔐 Authentication using API key, Bearer tokens, or other.
- 🧠 Use of CQRS or Mediator pattern (e.g., MediatR).
- ⚡ Use of Dapper for reporting/read endpoints.