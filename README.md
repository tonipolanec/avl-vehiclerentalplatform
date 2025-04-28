# AVL Vehicle Rental Platform

A .NET Web API for a connected vehicle rental platform that processes telemetry data, manages rentals, and provides reporting capabilities.

## Overview

This backend application serves as the core of a connected vehicle rental platform. It:

- Processes vehicle telemetry data (odometer readings, battery levels)
- Manages vehicle inventory and customer information
- Handles rental bookings with price calculations
- Provides reporting endpoints for rental metrics

## Technologies Used

- **Backend**: .NET 8 Web API
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **API Documentation**: Swagger
- **Architecture**: Layered Architecture

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/tonipolanec/avl-vehiclerentalplatform.git
   cd avl-vehiclerentalplatform
   ```

2. Configure the database connection in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=_localhost_;Database=_VehicleRentalDB_;Username=_vr_admin_;Password=_password_;Include Error Detail=true"
   }
   ```

3. Apply database migrations:
   ```
   dotnet ef database update
   ```

### Running the Application

#### Using .NET CLI

```
dotnet run --project .\src\VehicleRental.API\
```

The API will be available at `https://localhost:5270/`


#### Using Docker (if implemented)

```
NOT IMPLEMENTED YET
```

## Architecture

[Describe your architecture here - e.g., layers, modules, patterns used]

### Domain Model

- **Vehicle**: Represents a rental vehicle with make, model, pricing information
- **Customer**: Represents a person who can rent vehicles
- **Rental**: Tracks a booking with start/end dates and rental metrics
- **Telemetry**: Stores vehicle data like odometer readings and battery levels

### Key Design Decisions

[Explain major design decisions - e.g., choice of database, architecture style, etc.]

## Features

- Vehicle, Customer, and Rental management (CRUD operations)
- Telemetry data processing and storage
- Rental price calculations based on distance, duration, and battery usage
- Validation rules to prevent invalid rentals
- Detailed reporting on vehicle usage and customer activity

## API Documentation

The API documentation is available via Swagger UI when the application is running: `https://localhost:5270/swagger`

Also API documentation is also available offline in generated HTML file: `api_documentation.html`

### Key Endpoints:

- `/api/vehicles` - Vehicle management
- `/api/customers` - Customer management
- `/api/rentals` - Rental management

## Database Seeding

The application automatically seeds the database with test data from:
- `vehicles.csv` - Vehicle inventory
- `telemetry.csv` - Telemetry readings

## Testing

Run the tests using:

```
dotnet test
```
or using IDE.

I wrote core unit tests for the API and the application logic. Those test can be found in the `tests` folder.

Also I made Postman collections for the API. Those can be found in the `postman-setup` folder.
In there you will find 2 collections:
- `Vehicle Rental API` - Contains all the endpoints for the API
    - As Swagger is not the best tool for testing, I made a Postman collection for the API.
- `Vehicle Rental Testing` - Contains a collection of requests that can be run together with one click of a button in preconfigured environment
    - This mimicks integration tests. Not all endpoints are included as this was only for demo purposes.


