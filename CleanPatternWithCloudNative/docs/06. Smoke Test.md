# Smoke Test - on Kubernetes

- [x] Verify CRON
- [x] Verify Pub / Sub
- [x] Aspire Dashboard
- [x] Read secret
- [x] Read Config
- [x] Verify Caching

To verify secrets, create a kubernetes secret

```PowerShell
kubectl create secret generic hello --from-literal=hello=world
```

To verify config, create a key value pair, on redis, key type string
