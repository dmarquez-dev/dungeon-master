# CLAUDE.md

## SIR sessions: one feature branch per session

Every SIR session runs on its own feature branch, cut from `main` and merged back by a pull request the user opens. Nothing commits to `main` directly.

1. **Start from a current `main`.** Before `/sir:sir-up`, the main checkout is on `main`, its tree is clean, and `main` matches `origin/main` (`git fetch`, then `git rev-list --count main..origin/main` and `origin/main..main` both print 0). If any of these fails, say which one and stop. The previous session's PR must be merged before the next session starts.
2. **Cut the feature branch after Gate 1.** Once Gate 1 is approved and the session id exists, create and check out `<type>/<topic>` from `main`. `<topic>` is the session's name slug, the part of the id after the timestamp. `<type>` is the conventional-commit type of the work, for example `feat`, `fix`, `refactor`, `test`, `docs` or `chore`, matching the existing `feat/` and `chore/` branches. Do this before the spec phase's `init-session`, because SIR records the checked-out branch as `baseBranch`. The session worktree branch stays `agent/<session-id>`, per `.sir/config.json`.
3. **Gate 3 merges into the feature branch, never into `main`.** Its push question publishes the feature branch. Answer it yes, since the PR needs the branch on `origin`.
4. **Wrap, and any `/sir:sir-tend`, commit and push on the same feature branch**, before the PR is opened, so the session's records and tends ride in the same PR.
5. **The user opens the PR into `main` and merges it with a merge commit, not a squash.** The `.sir/` records cite task and merge hashes, and a squash would leave those hashes off `main`.
6. **The session is done only once its PR has merged.** After wrap, report the branch and say the session is waiting on its PR. Do not poll. When the user says the PR is merged, check it once (`gh pr view <branch> --json state,mergeCommit`), then check out `main` in the main checkout and fast-forward it to `origin/main`. Only then print the session-complete line. If `gh` is unavailable, take the user's word, check that `git merge-base --is-ancestor <feature branch tip> origin/main` succeeds after a fetch, and say which check was used.
7. **Branches are kept.** Deleting a merged feature branch or an `agent/` branch is the user's call.
