# Helm

## Create package

1. Make your updates
2. Increase version in `squidex7/Chart.yml`
3. Run `helm package squidex7`
4. Run `helm repo index . --merge index.yaml` to update index. 