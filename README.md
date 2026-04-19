# Colegio ERP 🎓

Colegio ERP is a comprehensive School Management System designed for medium-sized educational institutions. It provides a robust backend built with **.NET 8** and a sleek, modern, and highly interactive frontend developed using **React 18**, **TypeScript**, and **Tailwind CSS**.

![Project Status](https://img.shields.io/badge/Status-In%20Development-yellow)
![Backend](https://img.shields.io/badge/.NET-8.0-blue)
![Frontend](https://img.shields.io/badge/React-18.0-cyan)
![Database](https://img.shields.io/badge/SQLite-Latest-lightgrey)

---

## ✨ Features

- 🏫 **School Management**: Centralized management of school details and contact information.
- 👨‍🏫 **Teacher Directory**: Comprehensive profiles including specialties and hiring dates.
- 🏛️ **Classroom & Grade Levels**: Supports Early Childhood (3-5 years) and Primary Education (Grades 1-6) with multiple lines (A/B).
- 🎓 **Student Information System**: Manage student enrollments, birthdays, and classroom assignments.
- 👨‍👩‍👧 **Parental Portal**: Track parent/guardian contact details and student relationships.
- 📅 **Scheduling & Timetables**: Detailed classroom schedules including subjects, teachers, and timings.
- 💰 **Automated Invoicing**: Generation of receipts for tuition, dining, and extracurricular activities.

---

## 🛠️ Technology Stack

### Backend
- **Framework**: .NET 8 Minimal APIs
- **ORM**: Entity Framework Core 8
- **Database**: SQLite (Production-ready for easy migration to PostgreSQL/Supabase)
- **Architecture**: Clean Architecture / N-Layer
- **Testing**: xUnit

### Frontend
- **Framework**: React 18 (Vite)
- **Language**: TypeScript
- **Styling**: Tailwind CSS (Premium Modern UI)
- **Navigation**: React Router 6
- **Animations**: Framer Motion / CSS Transitions

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [Docker](https://www.docker.com/) (Optional, for containerized execution)

### 1. Repository Setup
```bash
git clone https://github.com/your-username/colegio-erp.git
cd colegio-erp
```

### 2. Backend Initialization
```bash
# Restore dependencies
dotnet restore

# Run migrations and update database
dotnet ef database update --project src/Colegio.Infrastructure --startup-project src/Colegio.Api

# Run the API ($http://localhost:8080)
dotnet run --project src/Colegio.Api
```

### 3. Frontend Initialization
```bash
cd web

# Install dependencies
npm install

# Start development server ($http://localhost:5173)
npm run dev
```

### 🐳 Running with Docker
The project includes a `docker-compose.yml` for the API:
```bash
docker-compose -f docker/docker-compose.yml up --build
```

---

## 📁 Project Structure

```text
.
├── src/
│   ├── Colegio.Domain/         # Core entities, interfaces, and domain logic
│   ├── Colegio.Infrastructure/ # EF Core, Migrations, Repositories implementation
│   └── Colegio.Api/            # Minimal APIs, Endpoints, and Configuration
├── web/                        # React Frontend Application
│   ├── src/pages/              # Main view components
│   └── src/components/         # Reusable UI components
├── tests/                      # Automated Test Suite
└── docker/                     # Docker configuration files
```

---

## 🧪 Testing
Run the backend test suite using:
```bash
dotnet test
```

---

## 📝 Design Principles
- **Modern Aesthetics**: Vibrant colors, dark modes, glassmorphism, and dynamic animations.
- **Premium UX**: High-end data visualizations and refined modal-based forms.
- **Responsive Design**: Mobile-first approach ensuring accessibility across all devices.

---

Designed with ❤️ for modern education.
