Actúa como un Arquitecto de Software y Desarrollador Full-Stack Senior. Vamos a construir un ERP / Sistema de Gestión Escolar desde cero. El colegio inicial es de tamaño medio: incluye etapas de Infantil (3, 4 y 5 años) y Primaria (1º a 6º curso), con 2 líneas por curso (ej. Clase A y Clase B).

Stack Tecnológico

Backend: .NET 8 (C#) usando Web API.

ORM: Entity Framework Core.

Base de datos inicial: SQLite (diseñado para ser migrado fácilmente a PostgreSQL/Supabase en el futuro).

Frontend: React 18+ (con Vite y TypeScript), usando Tailwind CSS para los estilos y React Router para la navegación.

Arquitectura Backend: Clean Architecture o estructurado en N-Capas (Controladores, Servicios, Repositorios/Data).

Modelo de Datos Principal (Entidades)
Por favor, utiliza nombres en inglés para el código y la base de datos como buena práctica, pero ten en cuenta la siguiente estructura de dominio:

School (Colegio): Id, Name, Address, ContactPhone, ContactEmail.

Teacher (Profesor): Id, FirstName, LastName, Specialty, HireDate.

Classroom (Clase): Id, GradeLevel (Infantil 3-5, Primaria 1-6), Line (A, B), SchoolId, TutorId (relación 1:1 con Teacher).

Student (Alumno): Id, FirstName, LastName, DateOfBirth, ClassroomId.

Parent (Padre/Tutor): Id, FirstName, LastName, Email, Phone, Address.

StudentParent (Tabla intermedia): Relación N:M entre alumnos y padres.

Schedule (Cuadrante/Horario): Id, ClassroomId, TeacherId, Subject (Asignatura), DayOfWeek, StartTime, EndTime.

Invoice (Facturación): Id, ParentId, StudentId, IssueDate, DueDate, TotalAmount, Status (Pending, Paid), Concept (Mensualidad, Comedor, Extraescolares).

Hoja de Ruta de Implementación
Vamos a trabajar paso a paso. No generes todo el código de golpe. Confirma que has entendido estas instrucciones y procede únicamente con el Paso 1, esperando mis comentarios antes de pasar al siguiente.

Paso 1: Setup del Backend. Creación de la solución en .NET 8, configuración de Entity Framework Core con SQLite y creación de las entidades del dominio con sus relaciones.

Paso 2: Base de Datos. Creación del DbContext, configuración de las claves foráneas (Fluent API o Data Annotations) y generación de la primera migración de SQLite.

Paso 3: Seed Data. Crear un script para poblar la base de datos con datos de prueba (El colegio de Infantil/Primaria con sus 2 líneas, algunos profesores y alumnos).

Paso 4: API Core. Creación de los repositorios, servicios y endpoints RESTful (Controladores) para gestionar los CRUD básicos.

Paso 5: Módulo de Facturación. Lógica específica en el backend para generar y listar facturas de los padres.

Paso 6: Setup del Frontend. Inicialización de React con Vite, configuración de Tailwind y enrutamiento básico.

Crea Paso 7: Paneles Frontend. Creación de los listados y formularios en React conectados a la API en .NET.