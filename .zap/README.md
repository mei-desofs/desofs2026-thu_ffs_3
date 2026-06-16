# DAST com OWASP ZAP — SafeVault

A SafeVault é uma **API REST JSON**. Um *baseline scan* (só spider + passive) quase
não encontra superfície de ataque numa API destas, por isso usamos o **ZAP API scan**
a partir da especificação OpenAPI e **autenticado** com um JWT.

## Ficheiros

| Ficheiro | Para que serve |
|----------|----------------|
| `swagger.json` | Especificação OpenAPI da API (alvo do scan). |
| `patch_swagger.py` | Preenche `example` em parâmetros de path (vaultId/documentId/userId) para o ZAP exercitar endpoints parametrizados. |
| `rules.tsv` | Política de alertas: o que é IGNORE / WARN / FAIL. SQLi (40018), XSS (40012/40014), Path Traversal (6) e OS Command Injection (90020) estão marcados como **FAIL**. |
| `zap.yaml` | Config do ZAP Automation Framework (scan passivo). |
| `zap-auth-scan.yaml` | Config do Automation Framework para scan **activo autenticado**. O token vem da env var `ZAP_AUTH_TOKEN` (não hardcoded). |
| `parse_report.py` | Utilitário para resumir os alertas/URLs de um relatório JSON. |
| `zap-api-report.{html,json}` | Último relatório do API scan (evidência). |

## Como correr localmente

```bash
# 1. Subir o stack
docker compose up -d --build

# 2. Obter um JWT de Admin fresco
export ZAP_AUTH_TOKEN="$(curl -s -X POST http://localhost:8080/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"email":"zap@safevault.io","password":"ZapScan!2026Aa","role":1}' | jq -r .accessToken)"

# 3. Correr o API scan autenticado, na mesma rede do compose
docker compose -f docker-compose.yml -f docker-compose.dast.yml up --build zap
# (ou via docker run --network safevault_backend ... ver dast.yml)
```

## No pipeline

`.github/workflows/dast.yml` faz isto automaticamente em cada push para `main`
(chamado como Stage 5 do `ci.yml`): sobe o stack, regista um Admin, obtém o JWT
e corre o API scan autenticado. O relatório fica como artefacto `zap-api-report`.

## Relatórios redundantes (podem ser removidos)

Estes ficaram de execuções antigas/experiências e criam ruído. O relatório
canónico é `zap-api-report.{html,json}`. Podem apagar com segurança:

- `zap_report.html` (baseline antigo)
- `zap-auth-report.{html,json}`
- `zap-full-auth-report.{html,json}`
- `login_response.json`
