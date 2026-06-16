import json

VAULT_ID    = "c0001259-1f1d-4a5d-b510-000000000001"
DOC_ID      = "d0001259-1f1d-4a5d-b510-000000000001"
USER_ID     = "f0001259-1f1d-4a5d-b510-58730b468603"

s = json.load(open(".zap/swagger.json"))

examples = {
    "vaultId":    VAULT_ID,
    "documentId": DOC_ID,
    "id":         USER_ID,
    "userId":     USER_ID,
}

for path, methods in s["paths"].items():
    for method, op in methods.items():
        for param in op.get("parameters", []):
            name = param.get("name")
            if name in examples:
                param["schema"]["example"] = examples[name]
                print(f"  [{method.upper()}] {path} -> {name} = {examples[name]}")

with open(".zap/swagger.json", "w") as f:
    json.dump(s, f, indent=2)

print("\nswagger.json actualizado com examples.")
