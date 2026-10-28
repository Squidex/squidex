Follow CLAUDE.md.

Before committing, verify your changes:

- Backend: run `dotnet build` on the affected project and the related tests with `--filter "Category!=Dependencies & Category!=TestContainer"`.
- Frontend: run `npm run lint` and `npm test` in `frontend/`.
