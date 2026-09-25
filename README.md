# Huellitas · Clínica veterinaria

Aplicación académica ASP.NET Core MVC (.NET 10), Identity, Entity Framework Core, SQL Server y QuestPDF.

## Ejecutar

Requisitos: SDK .NET 10 y SQL Server Express LocalDB (instancia MSSQLLocalDB).

```powershell
dotnet restore
dotnet run
```

La conexión se configura en `appsettings.json`. Al arrancar se aplican las migraciones pendientes y se crean los roles y el administrador inicial. No se elimina ni se reinicia la base existente.

Cuenta académica inicial: `admin@veterinaria.com` / `Admin123*`. El registro público crea exclusivamente clientes y guarda su nombre completo. Al iniciar, las cuentas de la versión anterior que no tengan ningún rol reciben Cliente; deben cerrar y volver a iniciar sesión para renovar sus permisos.

1. Administrador: crear servicios, consultar todas las citas y actualizar sus estados, revisar dashboard y generar reportes.
2. Cliente: crear cuenta, consultar catálogo, registrar mascota, solicitar cita y consultar sus propias citas.
3. Visitante: landing y catálogo público. Para reservar se requiere una cuenta Cliente.

El catálogo y los indicadores usan datos reales: si no hay servicios, se muestra un estado vacío. No se insertan servicios, clientes ni citas ficticias en la base habitual. El gráfico y la ilustración funcionan localmente; los iconos secundarios Bootstrap Icons utilizan CDN.

## Verificación reproducible

Ejecutar **únicamente con una base de pruebas**. El script crea clientes, servicios, mascotas y citas con identificadores aleatorios y deja algunos registros para inspección. No debe apuntar a la aplicación con datos reales.

```powershell
dotnet run --no-launch-profile -- --urls http://localhost:5198 --ConnectionStrings:DefaultConnection 'Server=(localdb)\MSSQLLocalDB;Database=Veterinaria_Verificacion;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True'
```

En otra terminal, con Python y las dependencias de prueba:

```powershell
python -m pip install requests pypdf
python scripts/verificar.py http://localhost:5198
```

Resultado de la revisión: 76 comprobaciones HTTP correctas, compilación sin errores ni advertencias, y revisión visual de landing y registro en móvil/escritorio y de los tres PDF. Los resultados temporales se guardan en `tmp/verificacion` (ignorado por Git).

Consulta `CUMPLIMIENTO.md` para el detalle de la actividad y los puntos organizativos pendientes.

## Filtros y precios

- Servicios y catálogo: búsqueda por nombre/descripción, precio mínimo/máximo y orden por nombre o precio.
- Mascotas: nombre/raza, especie, sexo y orden por nombre o edad; siempre limitadas al cliente autenticado.
- Citas: mascota/servicio (también cliente para administradores), estado y fechas inclusivas; orden por fecha o precio.
- Filtros GET combinables con contador, validación y enlace para limpiar. El cambio de estado conserva la búsqueda.
- Reportes: búsqueda de clientes por nombre/correo en el selector.
- Precios con formato boliviano (`Bs 130,50`) en catálogo, servicios, reserva, citas y PDF de citas. Se muestra la **tarifa actual** del servicio: no representa un cobro ni un precio histórico congelado.
- Tipografía local con títulos Georgia y texto Trebuchet MS, menos mayúsculas y más espacio de lectura; no requiere descargar fuentes.
