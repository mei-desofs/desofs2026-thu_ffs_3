# Phase 2 Sprint 1 - ASVS Checklist (Implementation Focus)

Escala: OK, PARCIAL, FALTA.

---

## V1 - Architecture, Design and Threat Modeling

| ASVS | Estado | Evidencia |
|---|---|---|
| 1.1.1 SSDLC com requisitos formalizados | OK | [phase1_deliverable.md](phase1_deliverable.md) |
| 1.1.2 Threat modeling actualizado | OK | [phase1_deliverable.md](phase1_deliverable.md) |
| 1.4.1 Controlos de acesso no servidor | OK | [src/InterfaceAdapters/Controllers](../../src/InterfaceAdapters/Controllers) |
| 1.6.3 Segredos fora do repositorio | OK | [src/InterfaceAdapters/appsettings.json](../../src/InterfaceAdapters/appsettings.json) |

---

## V2/V3 - Authentication and Session Management

| ASVS | Estado | Evidencia |
|---|---|---|
| 2.4.1 Hash adaptativo para passwords | OK | [src/Infrastructure/Security/PasswordHasherService.cs](../../src/Infrastructure/Security/PasswordHasherService.cs) |
| 2.5.2 Mensagens de erro genericas | OK | [src/InterfaceAdapters/Middleware/ExceptionHandlingMiddleware.cs](../../src/InterfaceAdapters/Middleware/ExceptionHandlingMiddleware.cs) |
| 3.4.1 Validacao de JWT | OK | [src/InterfaceAdapters/Program.cs](../../src/InterfaceAdapters/Program.cs) |

---

## V4 - Access Control

| ASVS | Estado | Evidencia |
|---|---|---|
| 4.1.1 RBAC em endpoints | OK | [src/InterfaceAdapters/Controllers](../../src/InterfaceAdapters/Controllers) |
| 4.2.1 Prevencao de IDOR | PARCIAL | [src/Application/Services/DocumentService.cs](../../src/Application/Services/DocumentService.cs) |

---

## V5 - Validation, Sanitization and Encoding

| ASVS | Estado | Evidencia |
|---|---|---|
| 5.1.3 Validacao server-side | OK | [src/Application/Services/DocumentService.cs](../../src/Application/Services/DocumentService.cs) |
| 5.3.4 Queries parametrizadas | OK | [src/Infrastructure/Repositories](../../src/Infrastructure/Repositories) |
| Validacao de magic bytes | OK | [src/Application/Services/DocumentService.cs](../../src/Application/Services/DocumentService.cs) |

---

## V7 - Error Handling and Logging

| ASVS | Estado | Evidencia |
|---|---|---|
| 7.1.x Logs sem dados sensiveis | PARCIAL | [src/Infrastructure/Storage/AuditWriterService.cs](../../src/Infrastructure/Storage/AuditWriterService.cs) |
| 7.4.1 Correlation id em erros | OK | [src/InterfaceAdapters/Middleware/ExceptionHandlingMiddleware.cs](../../src/InterfaceAdapters/Middleware/ExceptionHandlingMiddleware.cs) |

---

## V9 - Communications

| ASVS | Estado | Evidencia |
|---|---|---|
| 9.1.x TLS enforced | OK | [src/InterfaceAdapters/Program.cs](../../src/InterfaceAdapters/Program.cs) |

---

## V14 - Configuration

| ASVS | Estado | Evidencia |
|---|---|---|
| 14.1.1 Seguranca no pipeline CI/CD | OK | [\.github/workflows/ci.yml](../../.github/workflows/ci.yml), [\.github/workflows/codeql.yml](../../.github/workflows/codeql.yml) |
| 14.4.x Security headers | OK | [src/InterfaceAdapters/Middleware/SecurityHeadersMiddleware.cs](../../src/InterfaceAdapters/Middleware/SecurityHeadersMiddleware.cs) |
