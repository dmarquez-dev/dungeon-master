# CLAUDE.md

## SIR sessions: one feature branch per session

Every SIR session runs on its own feature branch, cut from `main` and merged back by a pull request the user opens. Nothing commits to `main` directly. This overrides sir-sdlc's default Gate 3, which merges the session branch into the base branch, commits the session records there, and offers to push the base.

1. **Start from a current `main`.** Before `/sir:sir-up`, the main checkout is on `main`, its tree is clean, and `main` matches `origin/main` (`git fetch`, then `git rev-list --count main..origin/main` and `origin/main..main` both print 0). If any of these fails, say which one and stop. The previous session's PR must be merged before the next session starts.
2. **The session branch is the feature branch.** The sir plugin names it and records it in `.sir/sessions/<id>/state.json` as `branch`. Read the name there, and never build it yourself. The spec phase's `init-session` runs with `main` checked out, so `baseBranch` is `main`. This file cuts no branch of its own.
3. **Gate 3 does not run `merge-session`.** It commits the session records (`.sir/sessions/<id>/`, plus any `.sir/backlog.md` append) on the session branch, in the worktree, so they go through the PR with the code. Its push question pushes the session branch (`git push -u origin <branch>`), and its count is the commits the branch has ahead of `origin/main`. Answer it yes, since the PR needs the branch on `origin`. In this repository a session's `MERGED` status means "pushed for PR". Remove the worktree only after the push.
4. **Wrap, and any `/sir:sir-tend`, commit and push on the same session branch**, before the PR is opened, so the session's records and tends ride in the same PR.
5. **The user opens the PR into `main` and merges it with a merge commit, not a squash.** The `.sir/` records cite task and merge hashes, and a squash would leave those hashes off `main`.
6. **The session is done only once its PR has merged.** After wrap, report the branch and say the session is waiting on its PR. Do not poll. When the user says the PR is merged, check it once (`gh pr view <branch> --json state,mergeCommit`, `<branch>` from `state.json`), then check out `main` in the main checkout and fast-forward it to `origin/main`. Only then print the session-complete line. If `gh` is unavailable, take the user's word, check that `git merge-base --is-ancestor <branch> origin/main` succeeds after a fetch, and say which check was used.
7. **Branches are kept.** Deleting a merged session branch is the user's call.
