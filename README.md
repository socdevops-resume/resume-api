# CV Generator

A service for creating, previewing, and exporting professional CVs.  
This project consists of a **.NET Core Web API backend** with a MongoDB database, and a React frontend.

## 🧩 Project Structure



## ✨ Features

- **User Management**
  - User registration (sign-up)
  - User authentication (login)
  - Update and delete user profiles
  - Retrieve user details by username
- **CV Management**
  - Create CVs linked to the authenticated user
  - Retrieve all CVs belonging to the logged-in user
  - View CV details by ID (only if owned by current user)
  - Update and delete CVs
- **Data Storage**
  - MongoDB for persistent storage of users and CVs
- **Security**
  - Passwords stored as SHA256 hashes
  - JWT-based authentication and authorization
- **Developer Tools**
  - Integrated Swagger UI for API exploration

## 📦 API Endpoints

### **User**
| Method | Endpoint               | Description |
|--------|------------------------|-------------|
| POST   | `/api/user/signup`     | Create a new user account |
| PUT    | `/api/user/{id}`       | Update an existing user profile |
| GET    | `/api/user/{username}` | Retrieve a user by username |
| DELETE | `/api/user/{id}`       | Delete a user account |

### **Sessions**
| Method | Endpoint               | Description |
|--------|------------------------|-------------|
| POST   | `/api/user/login`     | Authenticate and log in a user |
| DELETE | `/api/user/logout`    | log out a user, discards token |

### **CV**
| Method | Endpoint          | Description |
|--------|-------------------|-------------|
| GET    | `/api/cvs`         | Retrieve all CVs for the logged-in user |
| GET    | `/api/cvs/{id}`    | Retrieve a specific CV (must belong to the user) |
| POST   | `/api/cvs`         | Create a new CV for the logged-in user |
| PUT    | `/api/cvs/{id}`    | Update a CV (must belong to the user) |
| DELETE | `/api/cvs/{id}`    | Delete a CV (must belong to the user) |

## 🚀 Getting Started

### 1. Backend Setup

#### Prerequisites
- [.NET 7+](https://dotnet.microsoft.com/en-us/download)
- [MongoDB](https://www.mongodb.com/try/download/community)

#### Installation
```bash
dotnet restore
dotnet run
```

## 📖 API Documentation

The backend includes interactive API docs powered by **Swagger UI**.  
After running the backend, open:

👉 [Swagger UI](http://localhost:5159/swagger/index.html)

From here you can:
- Explore all available endpoints
- Try requests directly in the browser
- Authenticate using JWT tokens via the **Authorize** button