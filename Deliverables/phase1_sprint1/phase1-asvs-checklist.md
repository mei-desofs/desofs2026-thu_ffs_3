# Phase 1 - ASVS Checklist (Enfoque em Arquitectura)

Este documento apresenta a avaliação de conformidade com o OWASP Application Security Verification Standard (ASVS) 5.0 para a Phase 1 do projecto SafeVault. A avaliação baseia-se no conteúdo documentado em [phase1_deliverable.md](phase1_deliverable.md) e no estado real observado no codigo-fonte e nos testes do projecto.

Escala de conformidade: **OK** - requisito cumprido; **PARCIAL** - requisito parcialmente cumprido ou com evidencia incompleta; **FALTA** - requisito nao cumprido ou sem evidencia.

---

## V1 - Architecture, Design and Threat Modeling

| ASVS | Estado | Nota |
|------|--------|------|
| 1.1.1 Ciclo de vida de desenvolvimento seguro (SSDLC) com requisitos de segurança formalizados | OK | Processo documentado e evidencia presente no deliverable de Phase 1 |
| 1.1.2 Threat modeling documentado e actualizado | OK | Analise STRIDE por elemento do DFD documentada |
| 1.1.3 Abuse cases documentados para funcionalidades criticas | OK | Seccao dedicada no deliverable principal |
| 1.1.4 Trust boundaries identificadas e documentadas nos diagramas de fluxo de dados | OK | DFDs com trust boundaries representadas |
| 1.4.1 Controlos de acesso aplicados no lado do servidor | PARCIAL | Definido na arquitectura; validacao de cobertura total a concluir em Sprint 1 |
| 1.6.3 Segredos e credenciais nao versionados no repositorio | FALTA | Identificado como gap critico; ficheiro appsettings.json contem valores por defeito que devem ser removidos |
| 1.7.1 Formato de registo de auditoria consistente e com metadados suficientes | PARCIAL | Mecanismo de auditoria implementado; metadados de contexto a completar |

---

## V2 e V3 - Authentication and Session Management

| ASVS | Estado | Nota |
|------|--------|------|
| 2.4.1 Palavras-passe armazenadas com funcao de hash adaptativa | OK | BCrypt implementado com factor de custo adequado |
| 2.5.2 Mensagens de erro de autenticacao genericas, sem revelacao de informacao | PARCIAL | Comportamento definido no design; validar ausencia de information leakage em todos os casos de erro |
| 3.4.1 Validacao de JWT incluindo assinatura, expiracao e claims | PARCIAL | Configuracao presente no Program.cs; hardening completo a validar com testes de integracao |

---

## V4 - Access Control

| ASVS | Estado | Nota |
|------|--------|------|
| 4.1.1 Controlos de acesso aplicados em todos os endpoints da API | PARCIAL | Desenho de autorizacao bem estruturado; cobertura total a comprovar com testes |
| 4.2.1 Prevencao de Insecure Direct Object Reference (IDOR) | PARCIAL | Mitigacao definida na arquitectura; evidencia de testes a completar |

---

## V5 - Validation, Sanitization and Encoding

| ASVS | Estado | Nota |
|------|--------|------|
| 5.1.3 Validacao de entradas aplicada no lado do servidor | OK | Definida no deliverable e presente no codigo-fonte |
| 5.3.4 Queries a base de dados parametrizadas, sem concatenacao dinamica | OK | Entity Framework Core utiliza parametrizacao automatica |
| Validacao do tipo real de ficheiro por analise de magic bytes | FALTA | Gap identificado; validacao actual e baseada em extensao e Content-Type declarado, nao em conteudo real |

---

## V7 - Error Handling and Logging

| ASVS | Estado | Nota |
|------|--------|------|
| 7.1.x Registos de log sem inclusao de dados sensiveis | PARCIAL | Comportamento definido; confirmacao necessaria no codigo e nos pipelines de CI |
| 7.4.1 Identificador de correlacao presente em respostas de erro | PARCIAL | Definido no design; alinhamento com output seguro a concluir |

---

## V9 - Communications

| ASVS | Estado | Nota |
|------|--------|------|
| 9.1.x Transporte de dados exclusivamente sobre TLS | OK | Definido no desenho de arquitectura; requireHttpsMetadata activo e HSTS configurado |

---

## V14 - Configuration

| ASVS | Estado | Nota |
|------|--------|------|
| 14.1.1 Seguranca integrada no pipeline de CI/CD | PARCIAL | Planeado para Phase 2 Sprint 1; automatizacao plena nao incluida no ambito de Phase 1 |
| 14.4.x Security headers configurados e validados | PARCIAL | Definido no design e implementado no SecurityHeadersMiddleware; validacao efectiva a confirmar |

---

## Sintese de conformidade para M1

A submissao de M1 pode ser realizada com este estado de conformidade. Para declarar Phase 1 como completa, os itens classificados como **FALTA** devem ser resolvidos prioritariamente, em particular a remocao de segredos versionados e a implementacao de validacao real de tipo de ficheiro. Os itens **PARCIAL** criticos, designadamente gestao de segredos, validacao de upload, tratamento de erros sem exposicao de detalhe interno e completude da auditoria, devem ser encerrados no inicio de Sprint 1.
