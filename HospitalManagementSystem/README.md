# Hospital Management System - Backend

---

## 📋 Descripción
Sistema de gestión hospitalaria desarrollado en ASP.NET Core 9 que proporciona una API RESTful completa para la administración de pacientes, doctores, citas médicas e historiales clínicos. Incluye autenticación JWT, generación de PDFs.

---

## 🛠️ Tecnologías y Requisitos
- .NET SDK (Versión 8.0 o superior).
- SQL Server (LocalDB, Express o superior).
- Postman o Swagger/OpenAPI para pruebas de endpoints.
- Visual Studio 2022 o Visual Studio Code.

---

## 📦 Dependencias
### Paquetes NuGet
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.0" />
<PackageReference Include="Dapper" Version="2.1.24" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="QuestPDF" Version="2024.7.1" />
```

---

## 🚀 Características
- ✅ Autenticación JWT con roles múltiples
- ✅ API RESTful completa con ASP.NET Core 9
- ✅ Base de Datos SQL Server con Docker
- ✅ Stored Procedures para operaciones críticas
- ✅ Generación de PDFs para historiales médicos
- ✅ Documentación Swagger interactiva
- ✅ Seguridad por roles (Patient, Doctor, Employee, Admin)
- ✅ Validaciones y manejo de errores
- ✅ Logging completo de operaciones
- ✅ CORS configurado para frontend Angular

---

## 📁 Estructura del Proyecto
```bash
HospitalManagementSystem/
├── 📁 Controllers/          # Controladores API
├── 📁 Models/               # Modelos de datos
├── 📁 Services/             # Lógica de negocio
├── 📁 DTOs/                 # Objetos de transferencia
├── 📁 Data/                 # Contexto de base de datos
├── 📁 Database/Scripts/     # Scripts SQL
├── 📄 appsettings.json      # Configuración
└── 📄 Program.cs            # Configuración principal
```

---

## 🛠️ Instalación y Configuración
 
1. Clonar el Proyecto
```bash
git clone <repository-url>
cd HospitalManagementSystem
```

2.  Configurar la Aplicación

appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HospitalDB;User=sa;Password=YourPassword123!;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong123!",
    "Issuer": "HospitalManagementSystem",
    "Audience": "HospitalClient",
    "ExpireHours": 24
  }
}
```

3. Ejecutar la Aplicación

Visual Studio:

- Presionar F5 o hacer clic en "Run"

Terminal:
```bash
dotnet run
```

### URLs:
```url
API: https://localhost:7000

Swagger: https://localhost:7000/swagger
```

---

## 📊 Estructura de Base de Datos

### Tablas Principales
- Users: Usuarios del sistema
- Roles: Roles (Admin, Doctor, Patient, Employee)
- Patients: Información de pacientes
- Doctors: Información de doctores
- Appointments: Citas médicas
- MedicalRecords: Historiales médicos
- DocumentTypes: Tipos de documento

### Diagrama de Base de Datos
```sql
Users (1) ← (1) Patients
Users (1) ← (1) Doctors
Patients (1) → (N) Appointments
Doctors (1) → (N) Appointments
Appointments (1) → (1) MedicalRecords
Roles (1) ← (N) Users
```
---

## 🧩 Stored Procedures Principales
- `sp_CreateUser`: Crear un nuevo usuario
- `sp_GetUserByUsername`: Obtener usuario por nombre de usuario
- `sp_ScheduleAppointment`: Programar una cita médica
- `sp_GetAppointmentsByDoctor`: Obtener citas por doctor
- `sp_GetMedicalRecordsByPatient`: Obtener historiales médicos por paciente
- `sp_UpdateMedicalRecord`: Actualizar historial médico
- `sp_DeleteAppointment`: Eliminar una cita médica
- `sp_GetAllDoctors`: Obtener todos los doctores disponibles
- `sp_GetAllPatients`: Obtener todos los pacientes registrados
- `sp_GetAppointmentsByPatient`: Obtener citas por paciente
- `sp_GetDocumentTypes`: Obtener tipos de documento disponibles
- `sp_GenerateMedicalReport`: Generar reporte médico en PDF
- `sp_AuthenticateUser`: Autenticar usuario y generar token JWT
- `sp_UpdateUserRole`: Actualizar el rol de un usuario
- `sp_GetUsersByRole`: Obtener usuarios por rol
- `sp_GetAppointmentDetails`: Obtener detalles de una cita médica
- `sp_GetPatientHistory`: Obtener el historial completo de un paciente


---

## 🔐 Autenticación y Autorización

### Roles del Sistema
- Admin: Acceso completo al sistema
- Doctor: Gestión de citas y historiales médicos
- Patient: Acceso a su información y citas
- Employee: Gestión administrativa de pacientes
}
### Flujo de Autenticación
- Registro: POST /api/auth/register
- Login: POST /api/auth/login
- Uso de Token: Incluir en header Authorization: Bearer {token}

---

## 📚 Endpoints de la API

### 🔐 Autenticación
| Método | Endpoint              | Descripción        | Roles   |
|---------|-----------------------|--------------------|---------|
| POST    | `/api/auth/register`  | Registrar usuario  | Público |
| POST    | `/api/auth/login`     | Iniciar sesión     | Público |
| GET     | `/api/auth/profile`   | Obtener perfil     | Todos   |

### 👥 Pacientes
| Método | Endpoint                     | Descripción          | Roles                          |
|---------|------------------------------|----------------------|--------------------------------|
| GET     | `/api/patients`              | Obtener todos        | Admin, Doctor, Employee         |
| GET     | `/api/patients/{id}`         | Obtener por ID       | Owner, Admin, Doctor, Employee  |
| POST    | `/api/patients`              | Crear paciente       | Admin, Employee                 |
| PUT     | `/api/patients/{id}`         | Actualizar paciente  | Owner, Admin, Employee          |
| DELETE  | `/api/patients/{id}`         | Eliminar paciente    | Admin                           |
| GET     | `/api/patients/search`       | Buscar pacientes     | Admin, Doctor, Employee         |


### 📅 Citas Médicas
| Método | Endpoint                                   | Descripción        | Roles                          |
|---------|--------------------------------------------|--------------------|--------------------------------|
| GET     | `/api/appointments`                        | Todas las citas     | Admin, Employee, Doctor         |
| GET     | `/api/appointments/patient/{id}`           | Citas por paciente  | Owner, Admin, Doctor, Employee  |
| GET     | `/api/appointments/doctor/{id}`            | Citas por doctor    | Admin, Doctor, Employee         |
| GET     | `/api/appointments/status/{status}`        | Citas por estado    | Admin, Employee, Doctor         |
| POST    | `/api/appointments`                        | Crear cita          | Admin, Employee, Patient        |
| PUT     | `/api/appointments/{id}/status`            | Actualizar estado   | Admin, Employee, Doctor         |
| DELETE  | `/api/appointments/{id}`                   | Cancelar cita       | Admin, Employee                 |


### 🏥 Historial Médico
| Método | Endpoint                                       | Descripción             | Roles                              |
|---------|------------------------------------------------|--------------------------|------------------------------------|
| GET     | `/api/medicalrecords/patient/{id}`             | Historial del paciente   | Owner, Admin, Doctor, Employee     |
| GET     | `/api/medicalrecords`                          | Todos los registros      | Admin, Employee, Doctor            |
| POST    | `/api/medicalrecords`                          | Crear registro           | Admin, Doctor                      |
| POST    | `/api/medicalrecords/patient/{id}/pdf`         | Generar PDF              | Owner, Admin, Doctor, Employee     |
| GET     | `/api/medicalrecords/patient/{id}/pdf/download`| Descargar PDF            | Owner, Admin, Doctor, Employee     |

---

## Ejemplos de Requests

## 🔐 FLUJO 1: AUTENTICACIÓN Y REGISTRO
### 1. Registrar un Paciente
POST /api/auth/register
```json
{
  "userName": "juanperez",
  "email": "juan.perez@hospital.com",
  "password": "Password123!",
  "role": "Patient",
  "documentTypeId": 1,
  "documentNumber": "123456789",
  "firstName": "Juan",
  "lastName": "Pérez",
  "birthDate": "1985-05-15",
  "gender": "Masculino",
  "address": "Calle 123, Bogotá",
  "phoneNumber": "3001234567"
}
```

### Respuesta Esperada:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-15T10:30:00Z",
  "user": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "userName": "juanperez",
    "email": "juan.perez@hospital.com",
    "role": "Patient"
  }
}
```

### 2. Registrar un Doctor
Endpoint: POST /api/auth/register
```json
{
  "userName": "dra_garcia",
  "email": "dra.garcia@hospital.com",
  "password": "Doctor123!",
  "role": "Doctor",
  "documentTypeId": 1,
  "documentNumber": "987654321",
  "firstName": "María",
  "lastName": "García",
  "specialty": "Cardiología",
  "licenseNumber": "MED-12345",
  "phoneNumber": "3007654321"
}
```

### 3. Registrar un Empleado
Endpoint: POST /api/auth/register
```json
{
  "userName": "empleado_admin",
  "email": "admin@hospital.com",
  "password": "Admin123!",
  "role": "Employee"
}
```

### 4. Login
Endpoint: POST /api/auth/login
```json
{
  "email": "juan.perez@hospital.com",
  "password": "Password123!"
}
```
Guarda el token para usarlo en los siguientes requests.

---

## 👥 FLUJO 2: GESTIÓN DE PACIENTES (Como Empleado/Admin)
### 5. Obtener Todos los Pacientes
Endpoint: GET /api/patients

Headers:
```text
Authorization: Bearer {token_empleado}
```

### Respuesta:
```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "documentType": "Cédula de Ciudadanía",
    "documentNumber": "123456789",
    "firstName": "Juan",
    "lastName": "Pérez",
    "birthDate": "1985-05-15T00:00:00",
    "email": "juan.perez@hospital.com",
    "gender": "Masculino",
    "address": "Calle 123, Bogotá",
    "phoneNumber": "3001234567",
    "isActive": true,
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```

### 6. Crear Paciente (Como Empleado)
Endpoint: POST /api/patients

Headers:
```text
Authorization: Bearer {token_empleado}
```

```json
{
  "documentTypeId": 1,
  "documentNumber": "1122334455",
  "firstName": "Ana",
  "lastName": "Rodríguez",
  "birthDate": "1990-08-20",
  "email": "ana.rodriguez@hospital.com",
  "gender": "Femenino",
  "address": "Avenida 456, Medellín",
  "phoneNumber": "3105556677"
}
```

### 7. Buscar Pacientes
Endpoint: GET /api/patients/search?term=Juan

Headers:
```text
Authorization: Bearer {token_empleado}
```
---

## 📅 FLUJO 3: GESTIÓN DE CITAS
### 8. Crear Cita (Como Paciente)
Endpoint: POST /api/appointments

Headers:
```text
Authorization: Bearer {token_paciente}
```

```json
{
  "patientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "doctorId": "b2c3d4e5-f6g7-8901-bcde-f23456789012",
  "appointmentDate": "2024-01-20T14:30:00",
  "reason": "Consulta por dolor en el pecho"
}
```

### Respuesta:
```json
{
  "id": "c3d4e5f6-g7h8-9012-cdef-345678901234",
  "patientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "patientName": "Juan Pérez",
  "patientDocument": "123456789",
  "doctorId": "b2c3d4e5-f6g7-8901-bcde-f23456789012",
  "doctorName": "María García",
  "doctorSpecialty": "Cardiología",
  "appointmentDate": "2024-01-20T14:30:00",
  "status": "Scheduled",
  "reason": "Consulta por dolor en el pecho",
  "createdAt": "2024-01-15T11:00:00Z"
}
```

### 9. Ver Citas del Paciente
Endpoint: GET /api/appointments/patient/a1b2c3d4-e5f6-7890-abcd-ef1234567890

Headers:
```text
Authorization: Bearer {token_paciente}
```

### 10. Ver Todas las Citas (Como Empleado)
Endpoint: GET /api/appointments

Headers:
```text
Authorization: Bearer {token_empleado}
```

### 11. Ver Citas del Doctor
Endpoint: GET /api/appointments/doctor/b2c3d4e5-f6g7-8901-bcde-f23456789012

Headers:
```text
Authorization: Bearer {token_doctor}
```

### 12. Actualizar Estado de Cita (Como Doctor)
Endpoint: PUT /api/appointments/c3d4e5f6-g7h8-9012-cdef-345678901234/status

Headers:
```text
Authorization: Bearer {token_doctor}
```
Body: "Completed"

---

### 🏥 FLUJO 4: HISTORIAL MÉDICO
### 13. Crear Registro Médico (Como Doctor)
Endpoint: POST /api/medicalrecords

Headers:
```text
Authorization: Bearer {token_doctor}
```json
{
  "appointmentId": "c3d4e5f6-g7h8-9012-cdef-345678901234",
  "diagnosis": "Dolor torácico atípico. Se descarta patología cardiaca grave.",
  "treatment": "Reposo relativo, antiinflamatorios no esteroideos por 5 días",
  "prescription": "Ibuprofeno 400mg cada 8 horas por 5 días",
  "notes": "Paciente debe regresar en 2 semanas para control. Evitar esfuerzo físico intenso."
}
```

### Respuesta:
```json
{
  "id": "d4e5f6g7-h8i9-0123-defg-456789012345",
  "appointmentId": "c3d4e5f6-g7h8-9012-cdef-345678901234",
  "appointmentDate": "2024-01-20T14:30:00",
  "patientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "patientName": "Juan Pérez",
  "doctorId": "b2c3d4e5-f6g7-8901-bcde-f23456789012",
  "doctorName": "María García",
  "doctorSpecialty": "Cardiología",
  "diagnosis": "Dolor torácico atípico. Se descarta patología cardiaca grave.",
  "treatment": "Reposo relativo, antiinflamatorios no esteroideos por 5 días",
  "prescription": "Ibuprofeno 400mg cada 8 horas por 5 días",
  "notes": "Paciente debe regresar en 2 semanas para control. Evitar esfuerzo físico intenso.",
  "recordDate": "2024-01-20T15:45:00Z"
}
```

### 14. Ver Historial Médico del Paciente
Endpoint: GET /api/medicalrecords/patient/a1b2c3d4-e5f6-7890-abcd-ef1234567890

Headers:
```text
Authorization: Bearer {token_paciente}
```

### Respuesta:
```json
[
  {
    "id": "d4e5f6g7-h8i9-0123-defg-456789012345",
    "recordDate": "2024-01-20T15:45:00Z",
    "doctorName": "María García",
    "specialty": "Cardiología",
    "appointmentDate": "2024-01-20T14:30:00",
    "diagnosis": "Dolor torácico atípico. Se descarta patología cardiaca grave.",
    "treatment": "Reposo relativo, antiinflamatorios no esteroideos por 5 días",
    "prescription": "Ibuprofeno 400mg cada 8 horas por 5 días",
    "notes": "Paciente debe regresar en 2 semanas para control. Evitar esfuerzo físico intenso."
  }
]
```

### 15. Ver Todos los Registros Médicos (Como Empleado/Doctor)
Endpoint: GET /api/medicalrecords

Headers:
```text
Authorization: Bearer {token_empleado}
```
---


## 📄 FLUJO 5: GENERACIÓN DE PDF
### 16. Generar PDF del Historial Médico
Endpoint: POST /api/medicalrecords/patient/a1b2c3d4-e5f6-7890-abcd-ef1234567890/pdf

Headers:
```text
Authorization: Bearer {token_paciente}
```

```json
{
  "patientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "startDate": "2024-01-01",
  "endDate": "2024-12-31"
}
```
Respuesta: Archivo PDF descargable

### 17. Descargar PDF Directamente
Endpoint: GET /api/medicalrecords/patient/a1b2c3d4-e5f6-7890-abcd-ef1234567890/pdf/download?startDate=2024-01-01&endDate=2024-12-31

Headers:
```text
Authorization: Bearer {token_paciente}
```
Respuesta: Archivo PDF descargable

---

## 🤝 Contribución
1. Fork el proyecto
2. Crear una rama feature (git checkout -b feature/AmazingFeature)
3. Commit cambios (git commit -m 'Add some AmazingFeature')
4. Push a la rama (git push origin feature/AmazingFeature)
5. Abrir un Pull Request

---
## 📄 Licencia
Este proyecto está bajo la Licencia MIT. Consulta el archivo LICENSE para más detalles.

---

## 📞 Contacto
- Nombre: Víctor Alfonso Vargas Díaz
- Email: victor19vargas2018@gmail.com
- Teléfono: +57 323 381 2937
- Ciudad: Medellín, Colombia
- Profesión: Ingeniero de Sistemas y Desarrollador Full Stack
- Portfolio: [victorvargasdev.tech](https://portafolio-web-victor-vargas-4a4714.netlify.app/)
- LinkedIn: [Victor Vargas](http://www.linkedin.com/in/victor-alfonso-𝚅𝚊𝚛𝚐𝚊𝚜-diaz-6b853a355)
- GitHub: [VictorVargasD](https://github.com/INGVictorVargas-Dev-1907)
- Whatsapp: +57 323 381 2937

---
¡Gracias por usar el Sistema de Gestión Hospitalaria!
