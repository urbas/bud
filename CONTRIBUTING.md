1.  Create a feature branch:

    ```bash
    git checkout -b feature/<issue number>-your-branch-name
    ```

    An example: `feature/42-interactive-mode`

    You can omit the issue number if there's no issue associted with your work. 

1.  Write a unit test or a system test for the feature. Unit tests are in `Bud.Core/src/test/cs`. System tests are in `Bud.SystemTests/src/test/cs`.

1.  Do teh codez.

1.  Write a release note in `RELEASE_NOTES.md`.

1.  Submit a pull request.