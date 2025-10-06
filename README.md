# RestAPI
RestAPI Project
A robust ASP.NET Core REST API for user management with Swagger documentation, exception handling, and comprehensive testing capabilities.

Features
RESTful API - Clean, standards-compliant REST endpoints
Swagger Documentation - Interactive API documentation with XML comments
Exception Handling - Custom exception handling with proper HTTP status codes
Validation - Model validation with detailed error responses
Logging - Structured logging throughout the application
Database Integration - MSSQL database with stored procedures
Response Wrapping - Consistent API response format

Tech Stack
Framework: ASP.NET Core 6.0+
Database: Microsoft SQL Server
ORM: Dapper or Entity Framework Core
Documentation: Swagger/OpenAPI
Logging: Microsoft.Extensions.Logging

API Endpoints
Users Controller
Method	Endpoint	Description	Parameters
GET	/api/users	Get all users	-
GET	/api/users/{id}	Get user by ID	id (int)
POST	/api/users	Create new user	User data in body
PUT	/api/users/{id}	Update user	id (int), User data in body
DELETE	/api/users/{id}	Delete user	id (int)
