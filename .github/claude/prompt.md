Follow CLAUDE.md.

Before committing, verify your changes:

- Backend: run `dotnet build` on the affected project and the related tests with `--filter "Category!=Dependencies & Category!=TestContainer"`.
- Frontend: run `npm run lint` and `npm test` in `frontend/`.

Commit messages and PR titles are a plain summary of the change:

- No prefix like `[backend]` or `[frontend]`.
- No issue or PR numbers like `#123`.
- No `Co-authored-by` trailers or other attribution.
