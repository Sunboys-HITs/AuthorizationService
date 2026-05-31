# AuthorizationService

## Deploy to k3s

GitHub Actions workflow `.github/workflows/deploy.yml` builds the service image, pushes it to GHCR, and applies `k8s/auth-service.yaml` to the `app` namespace.

Required repository secret:

```bash
KUBECONFIG_B64
```

Create it from the kubeconfig that points to the k3s API:

```bash
base64 -w0 ~/.kube/sunboys-k3s.yaml
```

The cluster must be able to pull:

```text
ghcr.io/sunboys-hits/authorization-service
```

If the GHCR package is private, create an image pull secret in the cluster or make the package public.
