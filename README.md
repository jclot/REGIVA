# 🚀 REGIVA Costa Rica (SaaS)

**El Ecosistema Financiero y "CFO Autónomo" para PYMES en Costa Rica.**

![Status](https://img.shields.io/badge/Status-Development-yellow) ![NET](https://img.shields.io/badge/.NET-Core_8.0-purple) ![Postgres](https://img.shields.io/badge/PostgreSQL-16-blue) ![Python](https://img.shields.io/badge/Python-3.11-green) ![License](https://img.shields.io/badge/License-Proprietary-red)

## 📖 Descripción General

[cite_start]**REGIVA** es una plataforma SaaS diseñada para resolver la "ceguera financiera" de las PYMES costarricenses[cite: 1062]. [cite_start]No es solo un sistema de facturación electrónica; es una herramienta de **inteligencia financiera** que combina el cumplimiento tributario obligatorio (Hacienda v4.4) con modelos de Inteligencia Artificial para proyectar flujos de caja y calcular scores crediticios alternativos[cite: 1091].

[cite_start]El sistema utiliza una arquitectura **Multi-Tenant** segura, permitiendo gestionar múltiples empresas de forma aislada bajo una única base de datos mediante discriminación por `tenant_id`[cite: 8, 28].

---

## 🏗 Stack Tecnológico

[cite_start]El proyecto sigue una arquitectura por capas (N-Layer) integrando servicios modernos[cite: 199, 1180]:

* [cite_start]**Backend:** ASP.NET Core 8 (MVC + Web API)[cite: 392].
* [cite_start]**Base de Datos:** PostgreSQL configurado con Npgsql[cite: 270].
* [cite_start]**Acceso a Datos:** Repositorios con Dapper para consultas de alto rendimiento[cite: 253].
* [cite_start]**Frontend:** Razor Views, Bootstrap, jQuery (Estructura MVC clásica)[cite: 200].
* [cite_start]**Inteligencia Artificial (IA):** Python (Pandas, Scikit-learn, LSTM para series temporales)[cite: 1211].
* [cite_start]**Infraestructura:** Integración con API de Hacienda (OAuth 2.0 / OIDC)[cite: 1141].

---

## ✨ Módulos Principales

### 1. Core Multi-Tenant
* [cite_start]Aislamiento lógico de datos por `tenant_id`[cite: 28].
* [cite_start]Gestión de usuarios y roles (`tenant_users`)[cite: 36].
* [cite_start]Seguridad y auditoría completa (`created_at`, `updated_by`, logs)[cite: 17, 144].

### 2. Facturación Electrónica (Hacienda v4.4)
* [cite_start]**Documentos:** Facturas, Tiquetes, Notas de Crédito/Débito, Mensajes de Aceptación[cite: 58].
* [cite_start]**Validaciones:** Integración obligatoria con Código CABYS y validación de esquemas XSD[cite: 51, 1163].
* [cite_start]**REP (Recibo Electrónico de Pago):** Cálculo real de días de cobro (DSO) correlacionando facturas y pagos[cite: 93, 1175].
* [cite_start]**Almacenamiento XML:** Gestión de XML firmados y respuestas de Hacienda[cite: 84].

### 3. Inteligencia Artificial (Python Engine)
* [cite_start]**Cash Flow Projections:** Modelos predictivos (LSTM) para estimar la liquidez a 30 días[cite: 100, 1211].
* [cite_start]**Credit Scoring:** Algoritmo *Random Forest* para calificar el comportamiento de pago de clientes (0-100) sin depender de burós tradicionales[cite: 110, 1224].
* [cite_start]**Detección de Anomalías:** "Escudo Fiscal" para detectar patrones de gasto inusuales[cite: 1235].

---

## 🗄️ Estructura de Base de Datos

[cite_start]La base de datos `regiva_cr` está diseñada en **3FN** e incluye los siguientes esquemas clave[cite: 15, 274]:

* [cite_start]`tenants` & `users`: Gestión de acceso y empresas[cite: 24, 29].
* [cite_start]`electronic_documents` & `document_lines`: Tablas transaccionales principales[cite: 54, 68].
* [cite_start]`payment_receipts`: Trazabilidad de pagos parciales y totales[cite: 92].
* [cite_start]`cash_flow_projections` & `credit_scores`: Tablas de resultados de los modelos de IA[cite: 100, 110].

> [cite_start]**Nota:** Se utiliza un patrón de **Soft Delete** (`deleted_at`) en todas las tablas transaccionales[cite: 16, 283].

---

## 🛠 Instalación y Configuración

### Prerrequisitos
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
* [PostgreSQL 14+](https://www.postgresql.org/download/)
* [Python 3.11+](https://www.python.org/)
* [cite_start][DBeaver](https://dbeaver.io/) (Recomendado para gestión de BD)[cite: 371].

### 1. Clonar el Repositorio
```bash
git clone [https://github.com/tu-usuario/regiva-cr.git](https://github.com/tu-usuario/regiva-cr.git)
cd regiva-cr
2. Configuración de Base de Datos
Crea una base de datos vacía en PostgreSQL llamada regiva_cr.

Crea el usuario de aplicación recomendado:

SQL

CREATE USER regiva_app WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE regiva_cr TO regiva_app;



Actualiza la cadena de conexión en appsettings.json o Web.config:

XML

<connectionStrings>
    <add name="REGIVAConnection"
         connectionString="Host=localhost; Database=regiva_cr; Username=regiva_user; Password=secret; Pooling=true; MinPoolSize=5; MaxPoolSize=100"
         providerName="Npgsql" />
</connectionStrings>




3. Ejecutar Migraciones
Ejecuta el script SQL inicial ubicado en /database/init.sql o usa EF Core si está configurado para la creación inicial.

4. Entorno Python (IA)
Instalar dependencias para los modelos predictivos y parsing XML:

Bash

pip install pandas numpy scikit-learn lxml xmltodict




📂 Estructura del Proyecto
Basado en la arquitectura UI/UX definida:

Plaintext

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
🗺 Hoja de Ruta (Roadmap)

Fase 1 (Q1 2025): Cimientos legales, conexión API Hacienda, MVP de facturación básica.


Fase 2 (Q2 2025): Motor de datos, recepción automática de facturas y lanzamiento Alpha.


Fase 3 (Q3 2025): Implementación total de IA (LSTMs), migración v4.4 completa y dashboards predictivos.


Fase 4 (Q4 2025+): Escalamiento, servicios financieros (Factoring) y expansión.

⚖️ Licencia y Autoría

Autor: Julián Clot Córdoba. Contacto: julianclot123@gmail.com. Licencia: Propietario.


Documentación basada en "Horizonte Tecnológico Costa Rica 2030", "Documentación de Base de Datos REGIVA" y "Arquitectura UI/UX REGIVA".
