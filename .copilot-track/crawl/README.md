# Copilot Track — Crawl Phase

This directory stores evidence, prompt snippets, and notes gathered during the **Crawl** phase of the AI-track course.

> **Branch strategy and PR automation are defined below** — read the [Branch Strategy](#branch-strategy) and [Automatic PRs](#automatic-prs) sections before starting exercises.

---

## What is the Crawl phase?

The track is structured as **Crawl → Walk → Run**:

| Phase | Goal |
|-------|------|
| **Crawl** | Understand the codebase; build mental models; confirm dev toolchain works |
| **Walk** | Make guided changes with Copilot assistance; write tests |
| **Run** | Independent features, refactors, and PR-ready contributions |

---

## Branch Strategy

Each exercise lives on its own branch, chained from the previous one so diffs stay focused:

```
main
 └─ exercise-0    (bootstrap scaffolding)
     └─ exercise-1
         └─ exercise-2
             └─ exercise-3
                 └─ …
```

**Rules:**
- `exercise-0` branches from `main`.
- Each subsequent exercise branches from the **previous** exercise branch — never from `main`.
- PR base is always the **previous exercise branch** (not `main`), so reviewers see only the delta for that exercise.
- Merge commits are fine; rebasing is fine — just keep the chain intact.

### Creating a branch for exercise N

```bash
# Replace N with the exercise number, e.g. 3
PREV=exercise-$((N-1))
git checkout "$PREV"
git pull origin "$PREV"           # sync if remote exists
git checkout -b exercise-$N
```

---

## Chain-PRs

Each lesson produces a **chain PR** — a small, focused pull request that:

1. Targets the **previous exercise branch** as base (not `main`).
2. Contains **evidence** that the stated goal was met (tests, screenshots, logs).
3. Uses the PR title format: `GHCP — Crawl: <ex#> <name>` (e.g. `GHCP — Crawl: Ex1 Repo orientation`).
4. Follows the PR description template below.

### Why chain PRs?

- Keeps history linear and bisectable.
- Each PR is independently reviewable without a giant context window.
- Evidence in the PR body makes AI assistance auditable.

---

## Automatic PRs

Yes — you can open a PR automatically at the end of each exercise using the [GitHub CLI (`gh`)](https://cli.github.com/):

```bash
# Run this at the end of every exercise (replace variables as needed)
EX=1
NAME="Repo orientation"
PREV_BRANCH="exercise-$((EX-1))"
CURR_BRANCH="exercise-$EX"

git push -u origin "$CURR_BRANCH"

gh pr create \
  --base "$PREV_BRANCH" \
  --head "$CURR_BRANCH" \
  --title "GHCP — Crawl: Ex${EX} ${NAME}" \
  --body-file ".copilot-track/crawl/pr-body-ex${EX}.md"
```

**Tip:** Write your PR body into `.copilot-track/crawl/pr-body-ex<N>.md` first, then run the command above. The body file is the single source of truth — commit it alongside your changes.

Install gh if needed: `brew install gh && gh auth login`

---

## Evidence in PRs

Every PR description must include an **Evidence** section with actual command output:

```markdown
## Evidence
- Tests/logs/metrics:
  ```
  dotnet test --filter "..." → X passed, 0 failed
  ```
- Screenshot or log (for UI/behaviour changes)
- Prompt log: `.copilot-track/crawl/lesson-<N>.md`
```

---

## Prompt usage

Save the prompts you used with Copilot here so teammates can reproduce results:

```
.copilot-track/crawl/
  README.md            ← this file
  lesson-01.md         ← prompts + notes for exercise 1
  lesson-02.md
  pr-body-ex1.md       ← PR body for exercise 1 (used by gh pr create)
  pr-body-ex2.md
  …
```

Each `lesson-NN.md` file should contain:
1. The **prompt** you sent to Copilot (verbatim or paraphrased).
2. A brief **summary** of what worked / didn't work.
3. Any **follow-up prompts** needed to reach the final answer.

---

## PR Description Template

**Title format:** `GHCP — Crawl: <ex#> <name>`  
*(e.g. `GHCP — Crawl: Ex1 Repo orientation`)*

```markdown
## Summary
- What changed and why
- Files/paths touched

## Evidence
- Tests/logs/metrics:
  ```
  <command and output summary>
  ```
- Prompt log: `.copilot-track/crawl/lesson-<N>.md`

## Risk & Rollback
- Risk: low / medium / high
- Rollback: revert <commit SHA>  OR  toggle <flag>

## Track
- Level: Crawl
- Exercise: Ex<N>
```
