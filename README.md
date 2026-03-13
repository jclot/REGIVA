# 🚀 REGIVA Costa Rica (SaaS)

**El Ecosistema Financiero y "CFO Autónomo" para PYMES en Costa Rica.**

![Status](https://img.shields.io/badge/Status-Development-yellow)
![.NET](https://img.shields.io/badge/.NET-Core_8.0-purple)
![Postgres](https://img.shields.io/badge/PostgreSQL-16-blue)
![Python](https://img.shields.io/badge/Python-3.11-green)
![License](https://img.shields.io/badge/License-Proprietary-red)

---

## 📖 Descripción General

**REGIVA** es una plataforma SaaS diseñada para resolver la "ceguera financiera" de las PYMES costarricenses. No es solo un sistema de facturación electrónica; es una herramienta de **inteligencia financiera** que combina el cumplimiento tributario obligatorio (Hacienda v4.4) con modelos de Inteligencia Artificial para proyectar flujos de caja y calcular scores crediticios alternativos.

El sistema utiliza una arquitectura **Multi-Tenant** segura, permitiendo gestionar múltiples empresas de forma aislada bajo una única base de datos mediante discriminación por `tenant_id`.

---

## 🏗 Stack Tecnológico

El proyecto sigue una arquitectura por capas (N-Layer) integrando servicios modernos:

| Capa | Tecnología |
|---|---|
| **Backend** | ASP.NET Core 8 (MVC + Web API) |
| **Base de Datos** | PostgreSQL 16 con Npgsql |
| **Acceso a Datos** | Repositorios con Dapper (alto rendimiento) |
| **Frontend** | Razor Views, Bootstrap, jQuery |
| **Inteligencia Artificial** | Python — Pandas, Scikit-learn, LSTM |
| **Infraestructura** | API de Hacienda (OAuth 2.0 / OIDC) |

---

## ✨ Módulos Principales

### 1. 🏢 Core Multi-Tenant
- Aislamiento lógico de datos por `tenant_id`
- Gestión de usuarios y roles (`tenant_users`)
- Seguridad y auditoría completa (`created_at`, `updated_by`, logs)

### 2. 🧾 Facturación Electrónica (Hacienda v4.4)
- **Documentos soportados:** Facturas, Tiquetes, Notas de Crédito/Débito, Mensajes de Aceptación
- **Validaciones:** Integración con Código CABYS y validación de esquemas XSD
- **REP (Recibo Electrónico de Pago):** Cálculo real de días de cobro (DSO) correlacionando facturas y pagos
- **Almacenamiento XML:** Gestión de XML firmados y respuestas de Hacienda

### 3. 🤖 Inteligencia Artificial (Python Engine)
- **Cash Flow Projections:** Modelos predictivos LSTM para estimar liquidez a 30 días
- **Credit Scoring:** Algoritmo *Random Forest* que califica el comportamiento de pago de clientes (0–100) sin depender de burós tradicionales
- **Detección de Anomalías:** "Escudo Fiscal" para detectar patrones de gasto inusuales

---

## 🗄️ Estructura de Base de Datos

La base de datos `regiva_cr` está diseñada en **3FN** e incluye los siguientes esquemas clave:

| Tabla | Propósito |
|---|---|
| `tenants` & `users` | Gestión de acceso y empresas |
| `electronic_documents` & `document_lines` | Tablas transaccionales principales |
| `payment_receipts` | Trazabilidad de pagos parciales y totales |
| `cash_flow_projections` & `credit_scores` | Resultados de los modelos de IA |

> **Nota:** Se utiliza un patrón de **Soft Delete** (`deleted_at`) en todas las tablas transaccionales.

---

## 🛠 Instalación y Configuración

### Prerrequisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Python 3.11+](https://www.python.org/)
- [DBeaver](https://dbeaver.io/) *(recomendado para gestión de BD)*

### 1. Clonar el Repositorio

```bash
git clone https://github.com/tu-usuario/regiva-cr.git
cd regiva-cr
```

### 2. Configuración de Base de Datos

Crea una base de datos vacía en PostgreSQL llamada `regiva_cr` y el usuario de aplicación:

```sql
CREATE USER regiva_app WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE regiva_cr TO regiva_app;
```

Luego actualiza la cadena de conexión en `appsettings.json`:

```xml
<connectionStrings>
  <add name="REGIVAConnection"
       connectionString="Host=localhost; Database=regiva_cr; Username=regiva_app; Password=secure_password; Pooling=true; MinPoolSize=5; MaxPoolSize=100"
       providerName="Npgsql" />
</connectionStrings>
```

### 3. Ejecutar Migraciones

Ejecuta el script SQL inicial ubicado en `/database/init.sql`, o usa EF Core si está configurado para la creación inicial.

### 4. Entorno Python (IA)

```bash
pip install pandas numpy scikit-learn lxml xmltodict
```

---

## 📂 Estructura del Proyecto

```
REGIVA.Web/
├── Controllers/
│   ├── Public/              # HomeController, AuthController (Landing & Login)
│   ├── Admin/               # Dashboard, Documents, Customers, Reports
│   └── API/                 # DocumentsApiController (AJAX)
├── Views/
│   ├── Shared/              # _Layout.cshtml, _Sidebar.cshtml
│   ├── Dashboard/           # Resumen financiero e IA
│   ├── Documents/           # Creación de facturas y anulación
│   └── Reports/             # Flujo de caja y reporte fiscal
├── wwwroot/
│   ├── css/                 # admin.css, landing.css
│   └── js/                  # Lógica de cliente (charts.js, documents.js)
└── Areas/                   # Soporte para estructura Multi-tenant
```

---

## 🗺 Hoja de Ruta (Roadmap)

| Fase | Período | Alcance |
|---|---|---|
| **Fase 1** | Q1 2025 | Cimientos legales, conexión API Hacienda, MVP de facturación básica |
| **Fase 2** | Q2 2025 | Motor de datos, recepción automática de facturas, lanzamiento Alpha |
| **Fase 3** | Q3 2025 | IA completa (LSTMs), migración v4.4, dashboards predictivos |
| **Fase 4** | Q4 2025+ | Escalamiento, servicios financieros (Factoring) y expansión |

---

## ⚖️ Licencia y Autoría

**Autor:** Julián Clot Córdoba — [julianclot123@gmail.com](mailto:julianclot123@gmail.com)

**Licencia:** Propietario — Todos los derechos reservados.

*Documentación basada en "Horizonte Tecnológico Costa Rica 2030", "Documentación de Base de Datos REGIVA" y "Arquitectura UI/UX REGIVA".*
