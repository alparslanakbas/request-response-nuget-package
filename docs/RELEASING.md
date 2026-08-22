# Releasing

Releases are published from an annotated or lightweight Git tag by `.github/workflows/release.yml`. The workflow uses NuGet trusted publishing, so no long-lived API key is stored in GitHub.

## One-time NuGet trusted publishing policy

Create a policy at `nuget.org/account/trustedpublishing` with these values:

- Policy name: `request-response-github`
- Package owner: `alparslanakbas`
- CI/CD provider: `GitHub Actions`
- Repository owner: `alparslanakbas`
- Repository: `request-response-nuget-package`
- Workflow file: `release.yml`
- Environment: leave empty
- Push scope: `Push only new package versions`
- Unlist or relist scope: disabled
- Package patterns, one per line:
  - `CD.RequestResponse.Middleware`
  - `CD.File-Logger.Middleware`

The repository, owner, and workflow names are case-sensitive claims in the short-lived credential.

## Publish

1. Confirm CI is green on `main`.
2. Confirm `CHANGELOG.md` contains the release notes.
3. Create and push a tag matching the package version:

```bash
git tag v2.0.0-preview.1
git push origin v2.0.0-preview.1
```

The workflow builds and tests the complete solution, packs both package IDs with the tag version, authenticates using OIDC, publishes the main package first, and then publishes the compatibility package.
