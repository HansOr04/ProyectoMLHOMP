# ProyectoMLHOMP - Aplicación de Renta de Apartamentos en Ecuador

## Descripción del Proyecto

ProyectoMLHOMP es una aplicación web full-stack diseñada para facilitar la renta de apartamentos en Ecuador, con un enfoque principal en la ciudad de Quito. La aplicación conecta a propietarios de apartamentos con viajeros, fomentando el turismo local y proporcionando una experiencia de usuario intuitiva tanto para anfitriones como para huéspedes.

## Características Principales

- Registro y autenticación de usuarios (propietarios y huéspedes)
- Listado y búsqueda de apartamentos
- Sistema de reservas con verificación de disponibilidad
- Gestión de perfiles de usuario
- Sistema de reseñas y calificaciones
- Visualización de atracciones cercanas a los apartamentos
- Panel de control para propietarios para gestionar sus listados

## Tecnologías Utilizadas

- **Backend**: ASP.NET Core 8.0 (C#)
- **Frontend**: React con Vite
- **Base de Datos**: Entity Framework Core con SQL Server
- **Autenticación**: ASP.NET Core Identity
- **API**: RESTful API

## Estructura del Proyecto

```
ProyectoMLHOMP/
│
├── Backend/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Data/
│   ├── Program.cs
│   └── appsettings.json
│
├── Frontend/
│   ├── public/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── App.jsx
│   │   └── main.jsx
│   ├── index.html
│   ├── vite.config.js
│   └── package.json
│
└── ProyectoMLHOMP.sln
```

## Requisitos Previos

- .NET 8.0 SDK
- Node.js (versión 14 o superior)
- SQL Server

## Configuración del Proyecto

1. Clonar el repositorio:
   ```
   git clone https://github.com/tu-usuario/ProyectoMLHOMP.git
   cd ProyectoMLHOMP
   ```

2. Configurar el Backend:
   - Navegar a la carpeta `Backend`
   - Restaurar los paquetes NuGet:
     ```
     dotnet restore
     ```
   - Actualizar la cadena de conexión en `appsettings.json`
   - Aplicar las migraciones de la base de datos:
     ```
     dotnet ef database update
     ```

3. Configurar el Frontend:
   - Navegar a la carpeta `Frontend`
   - Instalar las dependencias:
     ```
     npm install
     ```

## Ejecución del Proyecto

1. Ejecutar el Backend:
   - En la carpeta `Backend`:
     ```
     dotnet run
     ```
   - La API estará disponible en `https://localhost:5001`

2. Ejecutar el Frontend:
   - En la carpeta `Frontend`:
     ```
     npm run dev
     ```
   - La aplicación React estará disponible en `http://localhost:5173`

## API Endpoints

- `GET /api/apartments`: Obtener todos los apartamentos
- `GET /api/apartments/{id}`: Obtener un apartamento específico
- `POST /api/apartments`: Crear un nuevo apartamento
- `PUT /api/apartments/{id}`: Actualizar un apartamento existente
- `DELETE /api/apartments/{id}`: Eliminar un apartamento
- `GET /api/apartments/search`: Buscar apartamentos con filtros

## Contribución

Las contribuciones son bienvenidas. Por favor, sigue estos pasos para contribuir:

1. Haz un fork del repositorio
2. Entra en tu rama o crea una rama nueva (`git checkout -b feature/AmazingFeature`)
3. Haz commit de tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Haz push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## Licencia

Este proyecto está bajo la licencia MIT. Ver el archivo `LICENSE` para más detalles.

## Contacto

Tu Nombre - [hansalazar04@gmail.com]

Link del Proyecto: [https://github.com/HansOr04/ProyectoMLHOMP]

