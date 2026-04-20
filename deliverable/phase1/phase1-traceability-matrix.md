# Phase 1 - Matriz de Rastreabilidade

Este documento garante a rastreabilidade explicita entre os requisitos de seguranca, as ameacas identificadas, as mitigacoes propostas e os testes planeados, em conformidade com a rubrica 6.1 do enunciado do projecto SafeVault.

A matriz deve ser actualizada no fecho de cada sprint para assegurar a continuidade do processo SSDLC.

---

## Matriz Principal

| Requisito de Seguranca | Ameaca (ID) | Mitigacao (ID) | Teste Planeado | Evidencia |
|------------------|-----------|---------|------------|--------|
| RS-01.1 Autenticacao com JWT seguro | T-02, T-01 | M-03 | Token adulterado rejeitado com 401; algoritmo "none" rejeitado com 401 | [phase1_deliverable.md](phase1_deliverable.md) |
| RS-01.3 Controlo de acesso baseado em papeis (RBAC) no servidor | T-07, T-11, T-14 | M-04 | Papel Viewer em endpoint Manager retorna 403; tentativa de IDOR retorna 403 | [phase1_deliverable.md](phase1_deliverable.md) |
| RS-01.4 Politica de bloqueio de conta apos falhas de autenticacao | T-05, T-06 | M-05 | Cinco falhas consecutivas resultam em bloqueio de conta | [phase1_deliverable.md](phase1_deliverable.md) |
| RS-02.2 Gestao de segredos fora do repositorio de codigo | T-16 | M-02 | Verificacao de configuracao sem segredos versionados | [appsettings.json](../../src/InterfaceAdapters/appsettings.json) |
| RS-02.3 Verificacao de integridade SHA-256 em documentos | T-10, T-21 | M-06 | Corrupcao de ficheiro resulta em erro de integridade | [phase1_deliverable.md](phase1_deliverable.md) |
| RS-02.4 Tratamento de erros sem exposicao de detalhe interno | T-12 | M-11 | Erro interno nao expoe stack trace nem mensagem de excepção | [ExceptionHandlingMiddleware.cs](../../src/InterfaceAdapters/Middleware/ExceptionHandlingMiddleware.cs) |
| RS-03.1 Transporte exclusivamente sobre TLS | T-20 | M-10 | Requisicao HTTP redirecionada ou bloqueada | [phase1_deliverable.md](phase1_deliverable.md) |
| RS-04.4 Validacao do tipo real de ficheiro por conteudo (magic bytes) | T-08 | M-01 | Upload com magic bytes invalidos rejeitado com 400 | [DocumentService.cs](../../src/Application/Services/DocumentService.cs) |
| RS-04.5 Prevencao de path traversal em operacoes de ficheiro | T-09 | M-01 | Nome de ficheiro com sequencias de travessia rejeitado | [phase1_deliverable.md](phase1_deliverable.md) |
| RS-05.1 Analise automatizada de dependencias (SCA) | Dependencias com vulnerabilidades conhecidas | Pipeline SCA | Verificacao de pacotes vulneraveis no pipeline de CI | [phase1_deliverable.md](phase1_deliverable.md) |
| RS-06.1 Registo de auditoria de eventos criticos | T-03, T-22 | M-07 | Login, upload, download e remocao registados com metadados | [AuditWriterService.cs](../../src/Infrastructure/Storage/AuditWriterService.cs) |
| RS-06.4 Deteccao de anomalias e exfiltracao em massa | Exfiltracao massiva de documentos | Regras de monitorizacao | Burst de downloads gera evento de auditoria ou alerta | [DocumentService.cs](../../src/Application/Services/DocumentService.cs) |

---

## Estado de Fecho

Os itens cujo estado e **PARCIAL** ou **FALTA** na ASVS checklist devem ser tratados como evidencia incompleta nesta matriz. A actualizacao desta matriz e obrigatoria no inicio de cada sprint, reflectindo o progresso real de implementacao e os resultados dos testes executados.