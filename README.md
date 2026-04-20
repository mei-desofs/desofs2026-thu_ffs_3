# SafeVault - Cofre Digital de Documentos

**DESOFS 2025/2026 - Turma thu_ffs - Grupo 3**

SafeVault e uma API REST em ASP.NET Core (.NET 9) com PostgreSQL para gestao segura de documentos sensiveis em contexto organizacional. O sistema suporta autenticacao JWT, autorizacao RBAC, versionamento de documentos, verificacao de integridade SHA-256 e auditoria completa de operacoes.

| Nome | Numero |
|---|------|
| Joao Loureiro | 1250526 |
| Goncalo Barbosa | 1240454 |
| Miguel Amorim | 1250540 |
| Diogo Maria | 1201832 |

---

## Estrutura do Repositorio

```
desofs2026-thu_ffs_3/
├── Deliverables/          # Artefactos de entrega por fase
│   ├── phase1_deliverable.md
│   ├── phase1-asvs-checklist.md
│   ├── phase1-traceability-matrix.md
│   ├── phase1-assessment-readiness.md
│   └── Diagrams/          # Diagramas de arquitectura e DFD
├── src/                   # Codigo-fonte da aplicacao
│   └── InterfaceAdapters/ # Controllers, Middleware, Program.cs
│   └── Application/       # Services, Use Cases
│   └── Domain/            # Entities, Value Objects, Aggregates
│   └── Infrastructure/    # EF Core, Storage, Logging
└── tests/                 # Testes unitarios e de integracao
```

---

## Entregaveis

Os documentos de Phase 1 encontram-se em [Deliverables/](Deliverables/). O indice completo com ligacoes esta em [Deliverables/README.md](Deliverables/README.md).

### Phase 1 - Documento principal

[Deliverables/phase1_deliverable.md](Deliverables/phase1_deliverable.md)

Cobre: visao geral do sistema, requisitos funcionais e de seguranca, arquitectura Clean Architecture + DDD, Data Flow Diagrams (nivel 0, 1 e 2), threat modeling STRIDE (22 ameacas), risk assessment OWASP, 12 mitigacoes e plano de testes de seguranca.

---

## Tecnologias

| Componente | Tecnologia |
|---|---|
| Framework | ASP.NET Core (.NET 9) |
| Base de Dados | PostgreSQL 16 |
| ORM | Entity Framework Core |
| Autenticacao | JWT Bearer + Refresh Tokens |
| Hashing de Passwords | BCrypt (cost factor >= 12) |
| Logging | Serilog |
| Testes | xUnit + Moq + FluentAssertions |
| Contentor | Docker + Docker Compose |
| CI/CD | GitHub Actions |