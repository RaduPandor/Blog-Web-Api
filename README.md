# Blog Web API

REST API for a blog platform, built with ASP.NET Core (.NET 8) and MySQL. It handles posts, user accounts and role-based access. The React frontend lives in [Blog-Web-App](https://github.com/RaduPandor/Blog-Web-App).

## Features

- CRUD endpoints for blog posts
- User registration, login and logout with ASP.NET Core Identity
- Cookie-based authentication, with CORS configured to allow credentials from the frontend
- Role-based authorization (`Admin`, `User`):
  - anyone can read posts
  - logged-in users can create posts and edit or delete their own
  - admins can edit or delete any post and manage users and roles
- Admin account and roles seeded on startup
- EF Core migrations applied automatically on startup, with retry while MySQL is starting
- Swagger UI in development

## Tech stack

C#, ASP.NET Core Web API (.NET 8), Entity Framework Core, Pomelo MySQL provider, ASP.NET Core Identity, MySQL 8, Docker, Docker Compose

## Project structure

```
BloggerWebApi/
├── Controllers/   AuthController, PostsController, BaseController (shared ownership/role checks)
├── Services/      UserService, PostService (business logic behind interfaces)
├── Interfaces/    IUserService, IPostService
├── Entities/      Post, ApplicationUser (extends IdentityUser), AdminUser (seed settings)
├── Dto/           request/response models, so entities aren't exposed directly
├── Migrations/    EF Core migrations
└── AppDbContext.cs
```

## Running it

Requires Docker.

```bash
git clone https://github.com/RaduPandor/Blog-Web-Api.git
cd Blog-Web-Api
docker compose up --build
```

- API: http://localhost:5174
- Swagger: http://localhost:5174/swagger
- MySQL: localhost:3306

On first start the database schema is created and an admin account is seeded from the `AdminUser` section in `appsettings.json`. The credentials there are for local development only.

The Compose file creates the `blogapp-network` Docker network, which the frontend joins. Start this repo before the frontend.

## API overview

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/posts` | Public |
| GET | `/api/posts/{id}` | Public |
| POST | `/api/posts` | Logged in |
| PUT | `/api/posts/{id}` | Owner or Admin |
| DELETE | `/api/posts/{id}` | Owner or Admin |
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/logout` | Logged in |
| GET | `/api/auth/me` | Logged in |
| PUT | `/api/auth/editprofile` | Logged in |
| GET | `/api/auth/{id}` | Self or Admin |
| PUT | `/api/auth/{id}` | Self or Admin |
| DELETE | `/api/auth/{id}` | Self or Admin |
| GET | `/api/auth/getall` | Admin |
| POST | `/api/auth/create` | Admin |
| PUT | `/api/auth/{id}/role` | Admin |

## What I'd improve next

- Token-based authentication (JWT or OAuth 2.0 / OpenID Connect) so the API isn't tied to browser cookies
- Move secrets out of `appsettings.json` into environment variables
- Integration tests for the authorization rules
